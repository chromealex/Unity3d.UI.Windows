using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;
#if UNITY_PS4
using UnityEngine.PS4;
#endif

namespace UnityEngine.UI.Windows.Movies {

	#if UNITY_PS4
	[System.Serializable]
	public class MoviePS4Module : MovieModuleBase {
		
		public class Item {

			public Material material;
			public PS4VideoPlayer player;

			public void Update() {

				this.player.Update();

				/*int cropleft, cropright, croptop, cropbottom, width, height;
				this.player.GetVideoCropValues(out cropleft, out cropright, out croptop, out cropbottom, out width, out height);

				float scalex = 1.0f;
				float scaley = 1.0f;
				float offx = 0.0f;
				float offy = 0.0f;
				if ((width > 0) && (height > 0)) {

					int fullwidth = width + cropleft + cropright;
					scalex = (float)width / (float)fullwidth;
					offx = (float)cropleft / (float)fullwidth;
					int fullheight = height + croptop + cropbottom;
					scaley = (float)height / (float)fullheight;
					offy = (float)croptop /(float)fullheight;

				}*/

			}

		};

		private Dictionary<string, Item> playingInstances = new Dictionary<string, Item>();

		public override bool IsMaterialLoadingType() {

			return true;

		}

		protected override void OnInit() {

			base.OnInit();



		}

		protected override System.Collections.Generic.IEnumerator<byte> LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, ResourceBase resource) {

			var filePath = resource.GetStreamPath();

			//Debug.Log("LOADING: " + filePath);

			Item item = null;
			var instance = this.FindInstance(resource);
			if (instance != null) {

				item = instance;

			} else {

				item = this.CreatePlayerInstance();
				this.playingInstances.Add(filePath, item);

			}

			//Debug.Log ("DONE: " + item.material);
			asyncOperation.SetValues(isDone: true, progress: 1f, asset: item.material);

			yield return 0;

		}

		public override void Update() {

			foreach (var item in this.playingInstances) {

				item.Value.Update();

			}

		}

		private Item CreatePlayerInstance() {

			var item = new Item();

			var player = new PS4VideoPlayer();
			player.PerformanceLevel = PS4VideoPlayer.Performance.Default;
			//player.demuxVideoBufferSize = 2 * 1024 * 1024;	// change the demux buffer from it's 1mb default	
			player.demuxVideoBufferSize = 1024;
			player.numOutputVideoFrameBuffers = 20;		// increasing this can stop frame stuttering
			player.videoMemoryType = PS4VideoPlayer.MemoryType.WB_ONION;

			var lumaTex = new UnityEngine.PS4.PS4ImageStream();
			lumaTex.Create(1920, 1080, PS4ImageStream.Type.R8, 0);
			var chromaTex = new UnityEngine.PS4.PS4ImageStream();
			chromaTex.Create(1920 / 2, 1080 / 2, PS4ImageStream.Type.R8G8, 0);

			player.Init(lumaTex, chromaTex);

			item.player = player;
			item.material = new Material(Shader.Find("FMVTexture"));
			item.material.mainTexture = lumaTex.GetTexture();
			item.material.SetTexture("_CromaTex", chromaTex.GetTexture());

			return item;

		}

		public override bool IsMovie(Texture texture) {

			return true;//string.IsNullOrEmpty(resource.streamingAssetsPath);

		}

		protected override void OnPlay(ResourceBase resource, Texture movie) {

			var path = resource.GetStreamPath();
			var instance = this.FindInstance(resource);
			if (instance != null) {

				instance.player.Play(path, PS4VideoPlayer.Looping.None);

			}

		}

		protected override void OnPlay(ResourceBase resource, Texture movie, bool loop) {

			var path = resource.GetStreamPath();
			var instance = this.FindInstance(resource);
			//Debug.Log ("-----------------------OnPlay: " + path + " :: " + (instance != null));
			if (instance != null) {

				instance.player.Play(path, loop == true ? PS4VideoPlayer.Looping.Continuous : PS4VideoPlayer.Looping.None);

			}

		}

		protected override void OnPause(ResourceBase resource, Texture movie) {

			var instance = this.FindInstance(resource);
			if (instance != null) {

				instance.player.Pause();

			}

		}

		protected override void OnStop(ResourceBase resource, Texture movie) {

			var instance = this.FindInstance(resource);
			if (instance != null) {

				instance.player.Stop();
				this.playingInstances.Remove(resource.GetStreamPath());

			}


		}

		protected override bool IsPlaying(ResourceBase resource, Texture movie) {

			var instance = this.FindInstance(resource);
			if (instance != null) {

				return (instance.player.playerState != PS4VideoPlayer.VidState.STOP &&
					instance.player.playerState != PS4VideoPlayer.VidState.PAUSE);

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
	#endif

}