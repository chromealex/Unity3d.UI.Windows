using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;

namespace UnityEngine.UI.Windows.Movies {

    [System.Serializable]
    public class MovieIOSModule : MovieModuleBase {

		#if UNITY_IOS || UNITY_TVOS
        [DllImport("__Internal")]
        private static extern bool VideoPlayer_Initialize(uint capacity);
        [DllImport("__Internal")]
        private static extern void VideoPlayer_Finalize();
        [DllImport("__Internal")]
        private static extern uint VideoPlayer_AddPlayer(string filename);
        [DllImport("__Internal")]
        private static extern void VideoPlayer_RemovePlayer(uint index);
        [DllImport("__Internal")]
        private static extern bool VideoPlayer_IsPlayerFailed(uint index);
        [DllImport("__Internal")]
        private static extern bool VideoPlayer_IsPlayerReady(uint index);
        [DllImport("__Internal")]
        private static extern uint VideoPlayer_GetTextureId(uint index);
        [DllImport("__Internal")]
        private static extern uint VideoPlayer_GetVideoSize(uint index, out uint width, out uint height);
        [DllImport("__Internal")]
        private static extern void VideoPlayer_Update(uint index, out bool complete);
        [DllImport("__Internal")]
        private static extern void VideoPlayer_Play(uint index, bool loop);
		#else
		private static bool VideoPlayer_Initialize(uint capacity) { return false; }
		private static void VideoPlayer_Finalize() {}
		private static uint VideoPlayer_AddPlayer(string filename) { return 0u; }
		private static void VideoPlayer_RemovePlayer(uint index) {}
		private static bool VideoPlayer_IsPlayerFailed(uint index) { return false; }
		private static bool VideoPlayer_IsPlayerReady(uint index) { return false; }
		private static uint VideoPlayer_GetTextureId(uint index) { return 0u; }
		private static uint VideoPlayer_GetVideoSize(uint index, out uint width, out uint height) { width = 0u; height = 0u; return 0u; }
		private static void VideoPlayer_Update(uint index, out bool complete) { complete = true; }
		private static void VideoPlayer_Play(uint index, bool loop) {}
		#endif

        public class Item {

            private readonly uint index;
            private bool failed;
            private bool ready;
            private Texture2D texture = null;
            private System.Action onComplete;

            public Item(string path) {

                this.index = MovieIOSModule.VideoPlayer_AddPlayer(path);
                this.failed = false;
                this.ready = false;

            }

            public void Dispose() {

                MovieIOSModule.VideoPlayer_RemovePlayer(this.index);

            }

            private void OnReady() {

                uint textureId = MovieIOSModule.VideoPlayer_GetTextureId(this.index);
                uint width, height;
                MovieIOSModule.VideoPlayer_GetVideoSize(this.index, out width, out height);
                this.texture = Texture2D.CreateExternalTexture((int)width, (int)height, TextureFormat.BGRA32, false, false, (System.IntPtr) textureId);

            }

            public void Update() {

                if (this.failed == true) {

                    return;

                }

                if (this.ready == false) {

                    this.failed = MovieIOSModule.VideoPlayer_IsPlayerFailed(this.index);
                    if (this.failed == true) {

                        return;

                    }

                    this.ready = MovieIOSModule.VideoPlayer_IsPlayerReady(this.index);
                    if (this.ready == true) {

                        this.OnReady();

                    }

                } else {

                    bool complete;
                    MovieIOSModule.VideoPlayer_Update(this.index, out complete);

                    if (complete == true) {

                        if (this.onComplete != null) {

							this.onComplete.Invoke();
							this.onComplete = null;

                        }

                    }

                }

            }

            public bool IsFailed() {

                return this.failed;

            }

            public bool IsReady() {

                return this.ready;

            }

            public Texture2D GetTexture() {

                Assert.IsTrue(this.ready);
                return this.texture;

            }

			public void Stop() {
				
				this.onComplete = null;

			}

            public void Play(bool loop, System.Action onComplete) {

                this.onComplete = onComplete;
                MovieIOSModule.VideoPlayer_Play(this.index, loop);

            }

        }

        private readonly ME.SimpleDictionary<string, Item> playingInstances = new ME.SimpleDictionary<string, Item>();

        protected override void OnInit() {

			base.OnInit();

            MovieIOSModule.VideoPlayer_Initialize(2);

        }

        protected override void OnDeinit() {

			base.OnDeinit();

            MovieIOSModule.VideoPlayer_Finalize();

        }

        private Item FindInstance(ResourceBase resource) {

            Item value;
            if (this.playingInstances.TryGetValue(resource.GetStreamPath(), out value) == true) {

                return value;

            }

            return null;

        }

        public override void Update() {

            base.Update();

            for (int i = 0; i < this.playingInstances.Count; ++i) {

                this.playingInstances.GetValueAt(i).Update();

            }

        }

        protected override System.Collections.IEnumerator LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, MovieItem movieItem, ResourceBase resource) {
			
            var streamPath = resource.GetStreamPath(withFile: false);

            var instance = this.FindInstance(resource);
            if (instance != null) {

                Debug.LogError("[MediaMovieModule] Already playing " + streamPath);

            } else {

                instance = new Item(streamPath);
                this.playingInstances.Add(streamPath, instance);
                this.counter = this.playingInstances.Count;

            }

            yield return 0;

            while (true) {

                if (instance.IsFailed() == true) {

                    asyncOperation.SetValues(isDone: true, progress: 1f, asset: null);
                    break;

                } else if (instance.IsReady() == true) {

                    asyncOperation.SetValues(isDone: true, progress: 1f, asset: instance.GetTexture());
                    break;

                } else {

                    yield return 0;

                }

            }

        }

        protected override void OnUnload(ResourceBase resource) {

            var key = resource.GetStreamPath();

            Item value;
            if (this.playingInstances.TryGetValue(key, out value) == true) {

                value.Dispose();
                this.playingInstances.Remove(key);
                this.counter = this.playingInstances.Count;

            }

        }

        public override bool IsVerticalFlipped() {

            return false;

        }

        public override bool IsMovie(Texture texture) {

            return true;

        }

        protected override void OnPlay(ResourceBase resource, Texture movie, System.Action onComplete) {

            var instance = this.FindInstance(resource);
            if (instance != null) {

                instance.Play(loop: false, onComplete: onComplete);

            }

        }

        protected override void OnPlay(ResourceBase resource, Texture movie, bool loop, System.Action onComplete) {

            var instance = this.FindInstance(resource);
            if (instance != null) {

                instance.Play(loop, onComplete);

            }

        }

		protected override void OnStop(ResourceBase resource, Texture movie) {

			var instance = this.FindInstance(resource);
			if (instance != null) {

				instance.Stop();

			}

		}

    }

}