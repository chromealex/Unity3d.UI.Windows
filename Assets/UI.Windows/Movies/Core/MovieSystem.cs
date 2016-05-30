using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Movies;

namespace UnityEngine.UI.Windows {

	public class MovieSystem : MonoBehaviour, IServiceBase {

		public string GetServiceName() {

			return "Movie System";

		}

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

		public MovieModuleBase movieModule;
		
		private static MovieSystem instance;

		public void Awake() {

			MovieSystem.instance = this;

			this.InitializeModule_INTERNAL();

		}

		public void LateUpdate() {

			if (this.lastQualityIndex != QualitySettings.GetQualityLevel()) {

				this.lastQualityIndex = QualitySettings.GetQualityLevel();

				MovieSystem.PlayPauseAll();

			}

		}

		public static ResourceAsyncOperation LoadTexture(IImageComponent component) {

			return MovieSystem.instance.movieModule.LoadTexture(component);

		}

		public static QualityItem GetQualityItem() {

			return MovieSystem.instance.quality.FirstOrDefault(x => x.qualityIndex == QualitySettings.GetQualityLevel());

		}

		public static bool CanPlayByQuality(Texture texture) {

			return MovieSystem.instance.CanPlayByQuality_INTERNAL(texture);

		}

		private bool CanPlayByQuality_INTERNAL(Texture texture) {

			var item = MovieSystem.GetQualityItem();
			if (item == null) return this.defaultPlayingState;

			if (texture.width > item.minSize || texture.height > item.minSize) {

				return item.canPlay;

			}

			return this.defaultPlayingState;

		}

		public static long GetKey(ResourceBase resource, IImageComponent component) {

			return resource.GetId();

		}
		
		public static bool IsMovie(Texture texture) {
			
			return MovieSystem.instance.IsMovie_INTERNAL(texture);
			
		}
		
		private bool IsMovie_INTERNAL(Texture texture) {
			
			return this.movieModule.IsMovie(texture);
			
		}

		public static void PlayPauseAll() {
			
			MovieSystem.instance.PlayPauseAll_INTERNAL();
			
		}
		
		private void PlayPauseAll_INTERNAL() {

			this.movieModule.PlayPauseAll();

		}

		public static void PlayAll() {
			
			MovieSystem.instance.PlayAll_INTERNAL();
			
		}
		
		private void PlayAll_INTERNAL() {

			this.movieModule.PlayAll();

		}

		public static void PauseAll() {
			
			MovieSystem.instance.PauseAll_INTERNAL();
			
		}
		
		private void PauseAll_INTERNAL() {

			this.movieModule.PauseAll();

		}

		public static void StopAll() {

			MovieSystem.instance.StopAll_INTERNAL();

		}

		private void StopAll_INTERNAL() {

			this.movieModule.StopAll();

		}

		public static void PlayAndPause(IImageComponent component, bool loop) {

			MovieSystem.instance.Play_INTERNAL(component, loop, pause: true);

		}

		private void Play_INTERNAL(IImageComponent component, bool loop, bool pause) {

			this.movieModule.Play(component, loop, pause);

		}

		private void Stop_INTERNAL(IImageComponent component) {

			this.movieModule.Stop(component);

		}
		
		private void Pause_INTERNAL(IImageComponent component) {

			this.movieModule.Pause(component);

		}
		
		private bool IsPlaying_INTERNAL(IImageComponent component) {
			
			return this.movieModule.IsPlaying(component);

		}

		public static void Play(IImageComponent component, bool loop) {

			if (MovieSystem.instance != null) MovieSystem.instance.Play_INTERNAL(component, loop, pause: false);

		}
		
		public static void Stop(IImageComponent component) {
			
			if (MovieSystem.instance != null) MovieSystem.instance.Stop_INTERNAL(component);

		}

		public static void Pause(IImageComponent component) {
			
			if (MovieSystem.instance != null) MovieSystem.instance.Pause_INTERNAL(component);

		}

		public static bool IsPlaying(IImageComponent component) {

			if (MovieSystem.instance == null) return false;

			return MovieSystem.instance.IsPlaying_INTERNAL(component);

		}

		protected virtual void InitializeModule_INTERNAL() {

			#if UNITY_STANDALONE || UNITY_EDITOR
			this.movieModule = new MovieStandaloneModule();
			#elif UNITY_IPHONE
			this.movieModule = new MovieIOSModule();
			#else
			this.movieModule = new MovieNoSupportModule();
			#endif

			this.movieModule.Init(this);

		}

	}

}