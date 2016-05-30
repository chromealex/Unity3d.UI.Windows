using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;
using System.Linq;

namespace UnityEngine.UI.Windows.Movies {

	public interface IMovieModule {

		void Init(MovieSystem system);
		bool IsMovie(Texture texture);
		void PlayPauseAll();
		void PauseAll();
		void StopAll();
		void PlayAll();
		void Play(IImageComponent component, bool loop, bool pause);
		void Stop(IImageComponent component);
		void Pause(IImageComponent component);
		bool IsPlaying(IImageComponent component);
		ResourceAsyncOperation LoadTexture(IImageComponent component);

	}

	[System.Serializable]
	public class MovieModuleBase : IMovieModule {
		
		[System.Serializable]
		public class Item {

			public enum State : byte {
				Playing,
				Stopped,
				Paused,
			};

			public long id;
			public State state = State.Stopped;

			public List<WindowComponent> components;

			public Object loadedObject;
			public int loadedObjectId;
			
			public Texture texture {
				
				get {
					
					return this.loadedObject as Texture;
					
				}
				
			}

		};

		protected MovieSystem system;

		public List<Item> current = new List<Item>();

		protected float delayToPause {

			get {

				return this.system.delayToPause;

			}

		}
		
		public void Init(MovieSystem system) {

			this.system = system;

			this.OnInit();

		}

		protected virtual void OnInit() {
		}

		public ResourceAsyncOperation LoadTexture(IImageComponent component) {

			var request = new ResourceAsyncOperation();
			this.StartCoroutine(this.LoadTexture_YIELD(request, component, component.GetResource()));

			return request;

		}

		protected virtual IEnumerator LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, ResourceBase resource) {

			yield return false;

		}

		#region ACTIONS
		public void PlayPauseAll() {

			this.StopAllCoroutines();
			foreach (var task in this.current) {

				var texture = task.texture;
				if (MovieSystem.CanPlayByQuality(texture) == true) {
					
					foreach (var component in task.components) {

						this.OnPlay((component as ILoadableResource).GetResource(), texture);
						
					}

				} else {

					foreach (var component in task.components) {

						this.StartCoroutine(this.PauseWithDelay_YIELD(component as IImageComponent, texture, this.delayToPause, this.OnPause));

					}

				}
				
			}

		}

		public void PauseAll() {

			this.StopAllCoroutines();
			foreach (var task in this.current) {
				
				var texture = task.texture;
				foreach (var component in task.components) {
					
					this.StartCoroutine(this.PauseWithDelay_YIELD(component as IImageComponent, texture, this.delayToPause, this.OnPause));
					
				}

			}

		}

		public void StopAll() {

			foreach (var task in this.current) {
				
				var texture = task.texture;
				foreach (var component in task.components) {
					
					this.OnStop((component as ILoadableResource).GetResource(), texture);
					
				}
				
			}

		}

		public void PlayAll() {
			
			foreach (var task in this.current) {
				
				var texture = task.texture;
				foreach (var component in task.components) {
					
					this.OnPlay((component as ILoadableResource).GetResource(), texture);
					
				}

			}

		}

		public void Play(IImageComponent component, bool loop, bool pause) {
			
			var image = component.GetRawImageSource();
			if (image == null) return;

			var resource = component.GetResource();
			if (resource.loaded == false) {

				Debug.LogWarning("Resource was not loaded yet. Play interrupted.", component as MonoBehaviour);
				return;

			}

			var movie = resource.loadedObject as Texture;

			var item = this.current.FirstOrDefault(x => x.id == resource.GetId());
			if (item != null) {
				
				var c = component as WindowComponent;
				if (item.components.Contains(c) == false) {

					item.components.Add(c);

				}

			} else {

				item = new Item() {

					id = resource.GetId(),
					components = new List<WindowComponent>() { component as WindowComponent },
					loadedObject = resource.loadedObject,
					loadedObjectId = resource.loadedObjectId,

				};

				this.current.Add(item);
				
			}

			if (item.state != Item.State.Playing) {

				item.state = Item.State.Playing;
				this.OnPlay(resource, movie, loop);

			}
			
			if (pause == true || MovieSystem.CanPlayByQuality(movie) == false) {
				
				this.StartCoroutine(this.PauseWithDelay_YIELD(component, this.delayToPause));
				
			}

		}

		public void Stop(IImageComponent component) {
			
			var image = component.GetRawImageSource();
			if (image == null) return;
			
			var resource = component.GetResource();
			if (resource.loaded == false) {
				
				//Debug.LogWarning("Resource was not loaded yet. Stop interrupted.", component as MonoBehaviour);
				return;
				
			}

			var movie = resource.loadedObject as Texture;

			var item = this.current.FirstOrDefault(x => x.id == resource.GetId());
			if (item != null) {
				
				item.components.Remove(component as WindowComponent);

				if (item.components.Count == 0) {
					
					if (item.state != Item.State.Stopped) {
						
						item.state = Item.State.Stopped;
						this.OnStop(resource, movie);

						this.current.RemoveAll(x => x.id == item.id);

					}

				}

			}

		}

		public void Pause(IImageComponent component) {
			
			var image = component.GetRawImageSource();
			if (image == null) return;
			
			var resource = component.GetResource();
			if (resource.loaded == false) {
				
				Debug.LogWarning("Resource was not loaded yet. Pause interrupted.", component as MonoBehaviour);
				return;
				
			}
			
			var movie = resource.loadedObject as Texture;

			var item = this.current.FirstOrDefault(x => x.id == resource.GetId());
			if (item != null) {
				
				//item.components.Remove(component as WindowComponent);

				//if (item.components.Count == 0) {

					if (item.state != Item.State.Paused) {
						
						item.state = Item.State.Paused;
						this.OnPause(resource, movie);
						
					}
					
				//}

			}

		}
		
		public bool IsPlaying(IImageComponent component) {
			
			var image = component.GetRawImageSource();
			if (image == null) return false;
			
			return this.IsPlaying(component.GetResource(), image.mainTexture);
			
		}
		
		public virtual bool IsMovie(Texture texture) {
			
			return false;
			
		}

		protected virtual void OnPlay(ResourceBase resource, Texture movie) {
			
			WindowSystemLogger.Log(this.system, "`Play` method not supported on current platform");
			
		}

		protected virtual void OnPlay(ResourceBase resource, Texture movie, bool loop) {
			
			WindowSystemLogger.Log(this.system, "`Play` method not supported on current platform");
			
		}

		protected virtual void OnPause(ResourceBase resource, Texture movie) {
			
			WindowSystemLogger.Log(this.system, "`Pause` method not supported on current platform");
			
		}

		protected virtual void OnStop(ResourceBase resource, Texture movie) {
			
			WindowSystemLogger.Log(this.system, "`Stop` method not supported on current platform");
			
		}

		protected virtual bool IsPlaying(ResourceBase resource, Texture texture) {
			
			WindowSystemLogger.Log(this.system, "`IsPlaying` method not supported on current platform");
			return false;

		}
		#endregion

		protected void StopAllCoroutines() {

			this.system.StopAllCoroutines();

		}

		protected Coroutine StartCoroutine(IEnumerator routine) {

			return this.system.StartCoroutine(routine);

		}

		protected IEnumerator PauseWithDelay_YIELD(IImageComponent component, Texture movie, float delay, System.Action<ResourceBase, Texture> callback) {
			
			var timer = 0f;
			while (timer < delay) {
				
				timer += Time.unscaledDeltaTime;
				yield return false;
				
			}
			
			yield return false;

			callback.Invoke(component.GetResource(), movie);
			
		}
		
		protected IEnumerator PauseWithDelay_YIELD(IImageComponent component, float delay) {
			
			var timer = 0f;
			while (timer < delay) {
				
				timer += Time.unscaledDeltaTime;
				yield return false;
				
			}
			
			yield return false;

			this.Pause(component);
			
		}

	}

	#if UNITY_IPHONE
	[System.Serializable]
	public class MovieIOSModule : MovieModuleBase {

		private Dictionary<string, VideoPlayerInterface> playingInstances = new Dictionary<string, VideoPlayerInterface>();
		
		protected override IEnumerator LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, ResourceBase resource) {
			
			var filePath = resource.GetStreamPath();
			
			var instance = this.FindInstance(resource);
			if (instance != null) {

				instance.Load(asyncOperation, filePath);

			} else {

				var item = new VideoPlayerInterface(this.system);
				this.playingInstances.Add(filePath, item);
				item.Load(asyncOperation, filePath);

			}

			yield return false;

		}

		public override bool IsMovie(Texture texture) {

			return true;//string.IsNullOrEmpty(resource.streamingAssetsPath);
			
		}
		
		protected override void OnPlay(ResourceBase resource, Texture movie) {

			var path = resource.GetStreamPath();
			var instance = this.FindInstance(resource);
			if (instance == null) {

				Debug.Log("CREATE NEW VideoPlayerInterface");
				var video = new VideoPlayerInterface(this.system);
				if (video.Play(path) == true) {

					this.playingInstances.Add(path, video);
					
				}

			}

		}
		
		protected override void OnPlay(ResourceBase resource, Texture movie, bool loop) {
			
			var path = resource.GetStreamPath();
			var instance = this.FindInstance(resource);
			if (instance == null) {

				Debug.Log("CREATE NEW VideoPlayerInterface");
				var video = new VideoPlayerInterface(this.system);
				if (video.Play(path, loop) == true) {
					
					this.playingInstances.Add(path, video);
					
				}
				
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
				this.playingInstances.Remove(resource.GetStreamPath());
				
			}

			
		}
		
		protected override bool IsPlaying(ResourceBase resource, Texture movie) {

			var instance = this.FindInstance(resource);
			if (instance != null) {
				
				return instance.IsPlaying();
				
			}

			return false;
			
		}

		private VideoPlayerInterface FindInstance(ResourceBase resource) {

			VideoPlayerInterface value;
			if (this.playingInstances.TryGetValue(resource.GetStreamPath(), out value) == true) {

				return value;

			}

			return null;

		}

	}
	#endif
	
	#if UNITY_ANDROID
	[System.Serializable]
	public class MovieAndroidModule : MovieModuleBase {

	}
	#endif

	[System.Serializable]
	public class MovieNoSupportModule : MovieModuleBase {

	}

}