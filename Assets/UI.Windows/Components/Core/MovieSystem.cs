#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY || UNITY_WEBGL
#define MOVIE_TEXTURE_NO_SUPPORT
#endif
using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows {

	public class MovieSystem : MonoBehaviour {

		[System.Serializable]
		public class QualityItem {

			public int qualityIndex;
			public bool canPlay;
			[Tooltip("Any of width or height must be greater than minSize to play/pause them")]
			public int minSize;

		};

		public float delayToPause = 0.1f;
		public bool defaultPlayingState = true;
		public QualityItem[] quality;

		private int lastQualityIndex = -1;

		private Dictionary<long, int> playing = new Dictionary<long, int>();
		private List<IImageComponent> playingByComponent = new List<IImageComponent>();
		#if !MOVIE_TEXTURE_NO_SUPPORT
		private List<MovieTexture> playingByTextures = new List<MovieTexture>();
		#endif

		private static MovieSystem instance;

		public void Awake() {

			MovieSystem.instance = this;

		}

		public void LateUpdate() {

			if (this.lastQualityIndex != QualitySettings.GetQualityLevel()) {

				this.lastQualityIndex = QualitySettings.GetQualityLevel();

				MovieSystem.PlayPauseAll();

			}

		}

		public static QualityItem GetQualityItem() {

			return MovieSystem.instance.quality.FirstOrDefault(x => x.qualityIndex == QualitySettings.GetQualityLevel());

		}

		private bool CanPlayByQuality_INTERNAL(Texture texture) {

			var item = MovieSystem.GetQualityItem();
			if (item == null) return this.defaultPlayingState;

			if (texture.width > item.minSize || texture.height > item.minSize) {

				return item.canPlay;

			}

			return this.defaultPlayingState;

		}

		#if !MOVIE_TEXTURE_NO_SUPPORT
		private long GetKey(MovieTexture texture, IImageComponent component) {

			return texture.GetInstanceID();//(texture.GetInstanceID() << 16 | (component as MonoBehaviour).GetInstanceID()& 0xffff);

		}
		#endif

		public static bool IsMovie(Texture texture) {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
			return texture is MovieTexture;
			#else
			return false;
			#endif

		}
		
		public static void PlayPauseAll() {
			
			MovieSystem.instance.PlayPauseAll_INTERNAL();
			
		}
		
		private void PlayPauseAll_INTERNAL() {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
			this.StopAllCoroutines();
			foreach (var movie in this.playingByTextures) {
				
				if (this.CanPlayByQuality_INTERNAL(movie) == true) {

					movie.Play();

				} else {

					this.StartCoroutine(this.PauseWithDelay_YIELD(movie, this.delayToPause));

				}
				
			}
			#endif
			
		}

		public static void PlayAll() {
			
			MovieSystem.instance.PlayAll_INTERNAL();
			
		}
		
		private void PlayAll_INTERNAL() {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
			foreach (var movie in this.playingByTextures) {
				
				movie.Play();
				
			}
			#endif
			
		}

		public static void PauseAll() {
			
			MovieSystem.instance.PauseAll_INTERNAL();
			
		}
		
		private void PauseAll_INTERNAL() {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
			this.playingByComponent.Clear();
			this.playing.Clear();

			this.StopAllCoroutines();
			foreach (var movie in this.playingByTextures) {
				
				this.StartCoroutine(this.PauseWithDelay_YIELD(movie, this.delayToPause));
				
			}
			#endif
			
		}

		public static void StopAll() {

			MovieSystem.instance.StopAll_INTERNAL();

		}

		private void StopAll_INTERNAL() {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
			this.playingByComponent.Clear();
			this.playing.Clear();

			foreach (var item in this.playingByTextures) {

				item.Stop();

			}

			this.playingByTextures.Clear();
			#endif

		}

		public static void PlayAndPause(IImageComponent component, bool loop) {

			MovieSystem.instance.Play_INTERNAL(component, loop, pause: true);

		}

		private void Play_INTERNAL(IImageComponent component, bool loop, bool pause) {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
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

						if (pause == true || this.CanPlayByQuality_INTERNAL(movie) == false) {

							this.StartCoroutine(this.PauseWithDelay_YIELD(component, movie, this.delayToPause));

						}

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
			WindowSystemLogger.Log(component, "`Play` method not supported on current platform");
			#endif
			
		}
		
		#if !MOVIE_TEXTURE_NO_SUPPORT
		private IEnumerator PauseWithDelay_YIELD(MovieTexture movie, float delay) {
			
			var timer = 0f;
			while (timer < delay) {
				
				timer += Time.unscaledDeltaTime;
				yield return false;
				
			}
			
			movie.Pause();
			
		}
		
		private IEnumerator PauseWithDelay_YIELD(IImageComponent component, MovieTexture movie, float delay) {
			
			var timer = 0f;
			while (timer < delay) {
				
				timer += Time.unscaledDeltaTime;
				yield return false;
				
			}

			this.Pause_INTERNAL(component);
			
		}
		#endif
		
		private void Stop_INTERNAL(IImageComponent component, bool pause) {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
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

						if (pause == true) {

							movie.Pause();

						} else {

							movie.Stop();

						}

						this.playingByTextures.Remove(movie);

					}

				}

			}
			#else
			WindowSystemLogger.Log(component, "`Stop` method not supported on current platform");
			#endif
			
		}
		
		private void Pause_INTERNAL(IImageComponent component) {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
			this.Stop_INTERNAL(component, pause: true);
			#else
			WindowSystemLogger.Log(component, "`Pause` method not supported on current platform");
			#endif
			
		}
		
		private bool IsPlaying_INTERNAL(IImageComponent component) {
			
			#if !MOVIE_TEXTURE_NO_SUPPORT
			var image = component.GetRawImageSource();
			if (image == null) return false;
			
			var movie = image.mainTexture as MovieTexture;
			if (movie != null) {
				
				return movie.isPlaying;
				
			}
			#else
			WindowSystemLogger.Log(component, "`IsPlaying` method not supported on current platform");
			#endif
			
			return false;

		}

		public static void Play(IImageComponent component, bool loop) {

			if (MovieSystem.instance != null) MovieSystem.instance.Play_INTERNAL(component, loop, pause: false);

		}
		
		public static void Stop(IImageComponent component) {
			
			if (MovieSystem.instance != null) MovieSystem.instance.Stop_INTERNAL(component, pause: false);

		}

		public static void Pause(IImageComponent component) {
			
			if (MovieSystem.instance != null) MovieSystem.instance.Pause_INTERNAL(component);

		}

		public static bool IsPlaying(IImageComponent component) {

			if (MovieSystem.instance == null) return false;

			return MovieSystem.instance.IsPlaying_INTERNAL(component);

		}

	}

}