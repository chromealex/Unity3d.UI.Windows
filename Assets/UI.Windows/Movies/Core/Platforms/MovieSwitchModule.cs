#if UNITY_SWITCH && !UNITY_EDITOR

using UnityEngine.Switch;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Movies {

    [System.Serializable]
    public class MovieSwitchModule : MovieModuleBase {

        public class Item {

            private const int textureWidth = 2048;
            private const int textureHeight = 2048;

            private bool isReady;
            private bool isError;

            private System.Action onComplete;

            private readonly Material material;
            private readonly SwitchVideoPlayer videoPlayer;

            public Item(string path) {

                SwitchFMVTexture lumaTexture = new SwitchFMVTexture();
                lumaTexture.Create(textureWidth, textureHeight, SwitchFMVTexture.Type.R8);
                SwitchFMVTexture chromaTexture = new SwitchFMVTexture();
                chromaTexture.Create(textureWidth / 2, textureHeight / 2, SwitchFMVTexture.Type.R8G8);

                this.material = new Material(Shader.Find("FMVTextureSwitch"));
                this.material.mainTexture = lumaTexture.GetTexture();
                this.material.SetTexture("_ChromaTex", chromaTexture.GetTexture());

                this.videoPlayer = new SwitchVideoPlayer(this.OnMovieEvent, true);
                this.videoPlayer.Init(lumaTexture, chromaTexture);

                // Switch doesn't work with paths like 'file:///rom/Data/StreamingAssets/...', so we must cut everything before 'rom:/'.
                if (path.StartsWith("file:///")) {

                    path = path.Substring(8);

                }

                bool played = this.videoPlayer.Play(path);
                if (played == true) {

                    this.videoPlayer.Pause(true);

                } else {

                    this.isError = true;

                }

            }

            public void Destroy() {

                this.videoPlayer.Stop();
                this.videoPlayer.Terminate();

            }

            private void OnMovieEvent(int movieEventId) {

                SwitchVideoPlayer.Event movieEvent = (SwitchVideoPlayer.Event)movieEventId;
                switch (movieEvent) {

                    case SwitchVideoPlayer.Event.Created: {

                        this.isReady = true;
                        break;

                    }

                    case SwitchVideoPlayer.Event.EndOfStream: {

                        if (this.onComplete != null) {

                            this.onComplete.Invoke();

                        }
                        break;

                    }

                    case SwitchVideoPlayer.Event.LoopPointReached: {

                        break;

                    }

                }

            }

            public void Update() {

                if (this.videoPlayer.IsPlaying()) {

                    this.videoPlayer.Update();

                    Vector4 uv = this.videoPlayer.GetCurrentUV(textureWidth, textureHeight);
                    this.material.SetVector("_MainTex_ST", uv);
                    this.material.SetVector("_ChromaTex_ST", uv);

                }

            }

            public bool IsReady() {

                return this.isReady;

            }

            public bool IsError() {

                return this.isError;

            }

            public bool IsPlaying() {

                return this.videoPlayer.IsPlaying();

            }

            public Material GetMaterial() {

                return this.material;

            }

            public void SetLoop(bool loop) {

                this.videoPlayer.isLooping = loop;

            }

            public void Play(System.Action onComplete) {

                this.onComplete = onComplete;
                this.videoPlayer.Pause(false);

            }

            public void Pause() {

                this.videoPlayer.Pause(true);

            }

            public void Stop() {

                this.onComplete = null;
                this.videoPlayer.Stop();

            }

        }

        private readonly Dictionary<int, Item> itemsByResourceId = new Dictionary<int, Item>(20);

        public MovieSwitchModule() {

        }

        public override bool IsMaterialLoadingType() {

            return true;

        }

        public override void Update() {

            foreach (Item item in itemsByResourceId.Values) {

                item.Update();

            }

        }

        protected override System.Collections.IEnumerator LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, MovieItem movieItem, ResourceBase resource) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == false) {

                asyncOperation.SetValues(isDone: false, progress: 0.0f, asset: null);

                var resourcePath = resource.GetStreamPath(withFile: true);
                item = new Item(resourcePath);
                this.itemsByResourceId[resourceId] = item;

            }

            while (item.IsReady() == false) {

                if (item.IsError() == true) {

                    asyncOperation.SetValues(isDone: true, progress: 1f, asset: null);
                    yield break;

                }

                yield return 0;

            }

            asyncOperation.SetValues(isDone: true, progress: 1f, asset: item.GetMaterial());

        }

        public override bool IsMovie(Texture texture) {

            return true;

        }

        protected override void OnUnload(ResourceBase resource) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                this.itemsByResourceId.Remove(resourceId);
                item.Destroy();

            }

        }

        protected override void OnPlay(ResourceBase resource, Texture movie, System.Action onComplete) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                item.Play(onComplete);

            }

        }

        protected override void OnPlay(ResourceBase resource, Texture movie, bool loop, System.Action onComplete) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                item.SetLoop(loop);
                item.Play(onComplete);

            }

        }

        protected override void OnPause(ResourceBase resource, Texture movie) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                item.Pause();

            }

        }

        protected override void OnStop(ResourceBase resource, Texture movie) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                item.Stop();

            }

        }

        protected override bool IsPlaying(ResourceBase resource, Texture movie) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                return item.IsPlaying();

            } else {

                return false;

            }

        }

    }

}

#endif
