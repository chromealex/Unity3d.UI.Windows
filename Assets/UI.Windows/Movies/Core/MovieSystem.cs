using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using UnityEngine.UI.Windows.Movies;
using UnityEngine.Video;

namespace UnityEngine.UI.Windows {

	public class MovieSystem : MonoBehaviour, IServiceBase {

		public string GetServiceName() {

			return "Movie System";

		}

		[System.Serializable]
		public class QualityItem {

			public int qualityIndex;
			public bool canPlay;
			public RuntimePlatform platform;
			[Tooltip("Any of width or height must be greater than minSize to play/pause them")]
			public int minSize;

		};

		public VideoPlayer videoPlayer;
		public float delayToPause = 0.1f;
		public bool defaultPlayingState = true;
		public QualityItem[] quality;
		//private int lastQualityIndex = -1;

		public MovieModuleBase movieModule;
		
		private static MovieSystem instance;

		public void Awake() {

			MovieSystem.instance = this;

		}

        public void Start() {

            this.InitializeModule_INTERNAL();

        }

        protected void OnDestory() {

            MovieSystem.instance = null;

	    }

		public void Update() {

			if (this.movieModule != null) this.movieModule.Update();

		}

		/*public void LateUpdate() {

			if (this.lastQualityIndex != QualitySettings.GetQualityLevel()) {

				this.lastQualityIndex = QualitySettings.GetQualityLevel();

				MovieSystem.PlayPauseAll();

			}

		}*/

		public static void RegisterOnUpdateTexture(System.Action<IImageComponent, Texture> onUpdate) {

			if (MovieSystem.instance != null) MovieSystem.instance.movieModule.RegisterOnUpdateTexture(onUpdate);

		}

		public static void UnregisterOnUpdateTexture(System.Action<IImageComponent, Texture> onUpdate) {

			if (MovieSystem.instance != null) MovieSystem.instance.movieModule.UnregisterOnUpdateTexture(onUpdate);

		}

		public static void RegisterOnUpdateMaterial(System.Action<Material> onUpdate) {

			if (MovieSystem.instance != null) MovieSystem.instance.movieModule.RegisterOnUpdateMaterial(onUpdate);

		}

		public static void UnregisterOnUpdateMaterial(System.Action<Material> onUpdate) {

			if (MovieSystem.instance != null) MovieSystem.instance.movieModule.UnregisterOnUpdateMaterial(onUpdate);

		}

		public static bool IsMaterialLoadingType() {

			return MovieSystem.instance.movieModule.IsMaterialLoadingType();

		}

		public static bool IsVerticalFlipped() {

			return MovieSystem.instance.movieModule.IsVerticalFlipped();

		}

		public static void Unload(IImageComponent resourceController, ResourceBase resource) {

			MovieSystem.instance.movieModule.Unload(resourceController, resource);

		}

		public static ResourceAsyncOperation LoadTexture(IImageComponent component) {

			return MovieSystem.instance.movieModule.LoadTexture(component);

		}

		public static QualityItem GetQualityItem() {

			return MovieSystem.instance.quality.FirstOrDefault(x => x.qualityIndex == QualitySettings.GetQualityLevel() && x.platform == Application.platform);

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

		public static void Rewind(IImageComponent component, bool pause) {

			MovieSystem.instance.Rewind_INTERNAL(component, pause);

		}

		public static void PlayAndPause(IImageComponent component, bool loop) {

			MovieSystem.instance.Play_INTERNAL(component, loop, pause: true, onComplete: null);

		}

		private void Rewind_INTERNAL(IImageComponent component, bool pause) {

			this.movieModule.Rewind(component, pause);

		}

		private void Play_INTERNAL(IImageComponent component, bool loop, bool pause, System.Action onComplete) {

			this.movieModule.Play(component, loop, pause, onComplete);

		}

		private void Stop_INTERNAL(IImageComponent component, int instanceId) {

			if (component.IsMovie() == true && component.IsPlaying() == true) this.movieModule.Stop(component, instanceId);

		}

		private void Pause_INTERNAL(IImageComponent component) {

			if (component.IsMovie() == true && component.IsPlaying() == true) this.movieModule.Pause(component);

		}
		
		private bool IsPlaying_INTERNAL(IImageComponent component) {
			
			return this.movieModule.IsPlaying(component);

		}

		public static void Play(IImageComponent component, bool loop, System.Action onComplete = null) {

			if (MovieSystem.instance != null) MovieSystem.instance.Play_INTERNAL(component, loop, pause: false, onComplete: onComplete);

		}

		public static void Stop(IImageComponent component, int instanceId = 0) {

			if (MovieSystem.instance != null) MovieSystem.instance.Stop_INTERNAL(component, instanceId);

		}

		public static void Pause(IImageComponent component) {
			
			if (MovieSystem.instance != null) MovieSystem.instance.Pause_INTERNAL(component);

		}

		public static bool IsPlaying(IImageComponent component) {

			if (MovieSystem.instance == null) return false;

			return MovieSystem.instance.IsPlaying_INTERNAL(component);

		}

		protected virtual void InitializeModule_INTERNAL() {

			#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID
			this.movieModule = new MovieStandaloneModule(this.videoPlayer);
			#elif UNITY_IOS || UNITY_TVOS
			this.movieModule = new MovieIOSModule();
			#elif UNITY_PS4
			this.movieModule = new MoviePS4Module();
			#else
			this.movieModule = new MovieNoSupportModule();
			#endif

			this.movieModule.Init(this);

		}

	}

}