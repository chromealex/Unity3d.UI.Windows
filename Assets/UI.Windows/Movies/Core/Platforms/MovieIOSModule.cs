using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;
using System.Runtime.InteropServices;

namespace UnityEngine.UI.Windows.Movies {

	[System.Serializable]
	public class MovieIOSModule : MovieModuleBase {

		[DllImport("__Internal")]
		private static extern bool VideoPlayer_Initialize(uint capacity);
		[DllImport("__Internal")]
		private static extern void VideoPlayer_Finalize();
		[DllImport("__Internal")]
		private static extern uint VideoPlayer_AddPlayer();
		[DllImport("__Internal")]
		private static extern void VideoPlayer_RemovePlayer(uint idx);
		[DllImport("__Internal")]
		private static extern bool VideoPlayer_CanOutputToTexture(string filename);
		[DllImport("__Internal")]
		private static extern bool VideoPlayer_PlayerReady(uint idx);
		[DllImport("__Internal")]
		private static extern bool VideoPlayer_PlayerIsPlaying(uint idx);
		[DllImport("__Internal")]
		private static extern void VideoPlayer_SetLoop(uint idx, bool loop);
		[DllImport("__Internal")]
		private static extern void VideoPlayer_Play(uint idx);
		[DllImport("__Internal")]
		private static extern void VideoPlayer_Stop(uint idx);
		[DllImport("__Internal")]
		private static extern void VideoPlayer_Pause(uint idx);
		[DllImport("__Internal")]
		private static extern void VideoPlayer_Rewind(uint idx, bool pause);
		[DllImport("__Internal")]
		private static extern float VideoPlayer_DurationSeconds(uint idx);
		[DllImport("__Internal")]
		private static extern void VideoPlayer_VideoExtents(uint idx, ref int w, ref int h);
		[DllImport("__Internal")]
		private static extern int VideoPlayer_CurFrameTexture(uint idx);
		[DllImport("__Internal")]
		private static extern bool VideoPlayer_LoadVideo(uint idx, string filename);

		public class Item {

			private uint idx = uint.MaxValue;
			private Texture2D videoTexture = null;
			private System.Action onComplete;

			public void Init(MovieModuleBase module, MovieItem movieItem) {

				if (this.idx == uint.MaxValue) this.idx = VideoPlayer_AddPlayer();

			}
				
			public bool Load(string path) {

				if (this.idx != uint.MaxValue && VideoPlayer_CanOutputToTexture(path) == true) {
					
					return VideoPlayer_LoadVideo(this.idx, path);

				} else {

					return false;

				}

			}

			public bool IsReady() {
				
				if (this.idx == uint.MaxValue) {

					return false;

				} else {

					return VideoPlayer_PlayerReady(this.idx);

				}
				
			}

			public void Dispose() {
				
				if (this.idx != uint.MaxValue) {
					
					VideoPlayer_RemovePlayer(this.idx);
					this.idx = uint.MaxValue;

				}

				this.videoTexture = null;

			}

			public void Update() {

				if (this.IsReady() == true && this.IsPlaying() == true) {

					var texture = VideoPlayer_CurFrameTexture(this.idx);
					if (texture != 0 && this.videoTexture != null) {

						this.videoTexture.UpdateExternalTexture((System.IntPtr)texture);

					} else if (texture == 0) {

						if (this.onComplete != null) this.onComplete.Invoke();

					}

				}

			}

			public void Rewind(bool pause) {

				if (this.idx == uint.MaxValue) return;
				
				VideoPlayer_Rewind(this.idx, pause);

			}

			public void Stop() {

				if (this.idx == uint.MaxValue) return;

				VideoPlayer_Stop(this.idx);

			}

			public void Play(bool loop, System.Action onComplete) {

				if (this.idx == uint.MaxValue) return;

				this.onComplete = onComplete;
				VideoPlayer_SetLoop(this.idx, loop);
				VideoPlayer_Play(this.idx);

			}

			public void Pause() {

				if (this.idx == uint.MaxValue) return;
				
				VideoPlayer_Pause(this.idx);

			}

			public bool IsPlaying() {

				if (this.idx == uint.MaxValue) return false;

				return VideoPlayer_PlayerIsPlaying(this.idx);

			}

			public Texture2D GetTexture() {
				
				if (this.IsReady() == true) {
					
					int nativeTex = VideoPlayer_CurFrameTexture(this.idx);

					int w = 0, h = 0;
					VideoPlayer_VideoExtents(this.idx, ref w, ref h);

					if (videoTexture == null) {
						
						videoTexture = Texture2D.CreateExternalTexture(w, h, TextureFormat.BGRA32, false, false, (System.IntPtr)nativeTex);
						videoTexture.filterMode = FilterMode.Bilinear;
						videoTexture.wrapMode = TextureWrapMode.Repeat;

					}

					videoTexture.UpdateExternalTexture((System.IntPtr)nativeTex);

				} else {

					videoTexture = null;

				}

				return videoTexture;

			}

		}

		private ME.SimpleDictionary<string, Item> playingInstances = new ME.SimpleDictionary<string, Item>();

		protected override void OnDeinit() {

			VideoPlayer_Finalize();

		}

		protected override void OnInit() {

			VideoPlayer_Initialize(2);

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

		public override void Update() {

			base.Update();

			for (int i = 0; i < this.playingInstances.Count; ++i) {

				this.playingInstances.GetValueAt(i).Update();

			}

		}

		protected override IEnumerator LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, MovieItem movieItem, ResourceBase resource) {

			var streamPath = resource.GetStreamPath(withFile: false);

			var item = this.FindInstance(resource);
			if (item != null) {

				Debug.LogError("[MediaMovieModule] Already playing " + streamPath);

			} else {

				item = this.CreatePlayerInstance(movieItem);
				this.playingInstances.Add(streamPath, item);
				this.counter = this.playingInstances.Count;

			}

			yield return false;

			if (item.Load(streamPath) == false) {

				asyncOperation.SetValues(isDone: true, progress: 1f, asset: null);

			} else {
				
				while (item.IsReady() == false) yield return false;

				asyncOperation.SetValues(isDone: true, progress: 1f, asset: item.GetTexture());

			}
				
		}

		public Item CreatePlayerInstance(MovieItem movieItem) {

			var item = new Item();

			item.Init(this, movieItem);
		
			return item;

		}

		public override bool IsMovie(Texture texture) {

			return true;

		}

		protected override void OnRewind(ResourceBase resource, Texture movie, bool pause) {

			var instance = this.FindInstance(resource);
			if (instance != null) {

				instance.Rewind(pause);

			}

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

		protected override void OnPause(ResourceBase resource, Texture movie) {

			var instance = this.FindInstance(resource);
			if (instance != null) {

				instance.Pause();

			}

		}

		protected override void OnStop(ResourceBase resource, Texture movie) {

			var instance = this.FindInstance(resource);
			if (instance != null) {

				instance.Stop();

			}

		}

		protected override bool IsPlaying(ResourceBase resource, Texture movie) {

			var instance = this.FindInstance(resource);
			if (instance != null) {

				return instance.IsPlaying();

			}

			return false;

		}

		private Item FindInstance(ResourceBase resource) {

			Item value;
			if (this.playingInstances.TryGetValue(resource.GetStreamPath(), out value) == true) {

				return value;

			}

			return null;

		}

	}

}