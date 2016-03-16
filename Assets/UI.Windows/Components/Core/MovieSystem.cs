#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
#define UNITY_MOBILE
#endif
using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class MovieSystem : MonoBehaviour {
		
		private Dictionary<long, int> playing = new Dictionary<long, int>();
		private List<IImageComponent> playingByComponent = new List<IImageComponent>();
		private List<MovieTexture> playingByTextures = new List<MovieTexture>();

		private static MovieSystem instance;

		public void Awake() {

			MovieSystem.instance = this;

		}

		private long GetKey(MovieTexture texture, IImageComponent component) {

			return texture.GetInstanceID();//(texture.GetInstanceID() << 16 | (component as MonoBehaviour).GetInstanceID()& 0xffff);

		}

		public static void StopAll() {

			MovieSystem.instance.StopAll_INTERNAL();

		}

		private void StopAll_INTERNAL() {

			this.playingByComponent.Clear();
			this.playing.Clear();

			foreach (var item in this.playingByTextures) {

				item.Stop();

			}

			this.playingByTextures.Clear();

		}

		private void Play_INTERNAL(IImageComponent component, bool loop) {
			
			#if !UNITY_MOBILE
			var image = component.GetRawImageSource();
			if (image == null) return;
			
			var movie = image.mainTexture as MovieTexture;
			if (movie != null) {

				var componentIsPlaying = this.playingByComponent.Contains(component);
				if (componentIsPlaying == false) {

					this.playingByComponent.Add(component);

					var key = this.GetKey(movie, component);

					var count = 0;
					var contains = this.playing.TryGetValue(key, out count);

					if (count == 0) {

						movie.loop = loop;
						movie.Play();

						this.playingByTextures.Add(movie);

					}

					++count;

					if (contains == true) {

						this.playing[key] = count;

					} else {

						this.playing.Add(key, count);

					}

				}

			}
			#else
			WindowSystemLogger.Log(component, "`Play` method not supported on mobile platforms");
			#endif
			
		}
		
		private void Stop_INTERNAL(IImageComponent component) {
			
			#if !UNITY_MOBILE
			var image = component.GetRawImageSource();
			if (image == null) return;
			
			var movie = image.mainTexture as MovieTexture;
			if (movie != null) {

				var componentIsPlaying = this.playingByComponent.Contains(component);
				if (componentIsPlaying == true) {

					this.playingByComponent.Remove(component);

					var key = this.GetKey(movie, component);

					var count = 0;
					if (this.playing.TryGetValue(key, out count) == true) {

						--this.playing[key];
						--count;

					}

					if (count <= 0) {

						movie.Stop();

						this.playingByTextures.Remove(movie);

					}

				}

			}
			#else
			WindowSystemLogger.Log(component, "`Stop` method not supported on mobile platforms");
			#endif
			
		}
		
		private void Pause_INTERNAL(IImageComponent component) {
			
			#if !UNITY_MOBILE
			var image = component.GetRawImageSource();
			if (image == null) return;
			
			var movie = image.mainTexture as MovieTexture;
			if (movie != null) {

				movie.Pause();

			}
			#else
			WindowSystemLogger.Log(component, "`Pause` method not supported on mobile platforms");
			#endif
			
		}
		
		private bool IsPlaying_INTERNAL(IImageComponent component) {
			
			#if !UNITY_MOBILE
			var image = component.GetRawImageSource();
			if (image == null) return false;
			
			var movie = image.mainTexture as MovieTexture;
			if (movie != null) {
				
				return movie.isPlaying;
				
			}
			#else
			WindowSystemLogger.Log(component, "`IsPlaying` method not supported on mobile platforms");
			#endif
			
			return false;

		}

		public static void Play(IImageComponent component, bool loop) {

			MovieSystem.instance.Play_INTERNAL(component, loop);

		}
		
		public static void Stop(IImageComponent component) {
			
			MovieSystem.instance.Stop_INTERNAL(component);

		}

		public static void Pause(IImageComponent component) {
			
			MovieSystem.instance.Pause_INTERNAL(component);

		}

		public static bool IsPlaying(IImageComponent component) {
			
			return MovieSystem.instance.IsPlaying_INTERNAL(component);

		}

	}

}