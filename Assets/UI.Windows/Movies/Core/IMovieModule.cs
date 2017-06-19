using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using ME;

#if UNITY_PS4
using UnityEngine.PS4;
#endif

namespace UnityEngine.UI.Windows.Movies {

	public interface IMovieModule {

		void Init(MovieSystem system);
		bool IsMovie(Texture texture);
		void PlayPauseAll();
		void PauseAll();
		void StopAll();
		void PlayAll();
		void Play(IImageComponent component, bool loop, bool pause, System.Action onComplete);
		void Stop(IImageComponent component, int instanceId = 0);
		void Pause(IImageComponent component);
		bool IsPlaying(IImageComponent component);
		ResourceAsyncOperation LoadTexture(IImageComponent component);

		bool IsMaterialLoadingType();
		bool IsVerticalFlipped();

	}

	[System.Serializable]
	public class MovieModuleBase : IMovieModule {
		
		[System.Serializable]
		public class MovieItem {

			public enum State : byte {
				Playing,
				Stopped,
				Paused,
			};

			//public long id;
			public State state = State.Stopped;

			/*public List<WindowComponent> components;
			public Object loadedObject;
			public int loadedObjectId;
			
			public Texture texture {
				
				get {
					
					return this.loadedObject as Texture;
					
				}
				
			}*/

			public WindowSystemResources.Item resource;

			public int id {

				get {

					return this.resource.id;

				}

			}

			public Texture texture {

				get {

					return this.resource.loadedObject as Texture;

				}

			}

			public List<IResourceReference> components {

				get {

					return this.resource.references;

				}

			}

		};

		public int counter;

		public MovieSystem system;
		private System.Action<Material> onUpdateMaterial;
		private System.Action<IImageComponent, Texture> onUpdateTexture;

		public List<MovieItem> current = new List<MovieItem>();

		protected float delayToPause {

			get {

				return this.system.delayToPause;

			}

		}

		~MovieModuleBase() {

			this.OnDeinit();

		}

		public void Init(MovieSystem system) {

			this.system = system;

			this.OnInit();

		}

		public void RegisterOnUpdateMaterial(System.Action<Material> onUpdate) {

			this.onUpdateMaterial += onUpdate;

		}

		public void UnregisterOnUpdateMaterial(System.Action<Material> onUpdate) {

			this.onUpdateMaterial -= onUpdate;

		}

		public void SetUpdateMaterial(Material material) {

			if (this.onUpdateMaterial != null) this.onUpdateMaterial.Invoke(material);

		}

		public void RegisterOnUpdateTexture(System.Action<IImageComponent, Texture> onUpdate) {

			this.onUpdateTexture += onUpdate;

		}

		public void UnregisterOnUpdateTexture(System.Action<IImageComponent, Texture> onUpdate) {

			this.onUpdateTexture -= onUpdate;

		}

		public void SetUpdateTexture(IImageComponent component, Texture material) {

			if (this.onUpdateTexture != null) this.onUpdateTexture.Invoke(component, material);

		}

		protected virtual void OnDeinit() {}

		protected virtual void OnInit() {}

		public virtual void Update() {}

		public virtual bool IsMaterialLoadingType() {

			return false;

		}

		public virtual bool IsVerticalFlipped() {

			return false;

		}

		public ResourceAsyncOperation LoadTexture(IImageComponent component) {

			/*if (this.IsMaterialLoadingType() == true) {

			} else {

				MovieSystem.UnregisterOnUpdateTexture(this.ValidateTexture);
				MovieSystem.RegisterOnUpdateTexture(this.ValidateTexture);

			}*/

			var request = new ResourceAsyncOperation();
			var resource = component.GetResource();
			var movieItem = this.GetMovieItem(component, resource);
			this.StartCoroutine(this.LoadTexture_YIELD(request, component, movieItem, resource));

			return request;

		}

		/*private void ValidateTexture(IImageComponent component, Texture texture) {

			var comp = component as WindowComponent;
			foreach (var item in this.current) {

				if (item.components.Contains(comp) == true) {

					item.loadedObject = texture;
					item.loadedObjectId = texture.GetInstanceID();

				}

			}

		}*/

		protected virtual System.Collections.IEnumerator LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, MovieItem movieItem, ResourceBase resource) {

			yield return 0;

		}

#region ACTIONS
		public void PlayPauseAll() {

			this.StopAllCoroutines();
			foreach (var task in this.current) {

				var texture = task.texture;
				if (MovieSystem.CanPlayByQuality(texture) == true) {
					
					foreach (var component in task.components) {

						this.OnPlay((component as ILoadableResource).GetResource(), texture, null);
						
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
					
					this.OnPlay((component as ILoadableResource).GetResource(), texture, null);
					
				}

			}

		}

		public void Unload(IImageComponent component, ResourceBase resource) {
			
			//if (resource.loaded == false) return;

			/*if (resource.loaded == false) {

				Debug.LogWarning("Resource was not loaded yet. Unload interrupted.", component as MonoBehaviour);
				return;

			}*/

			this.current.RemoveAll(x => (x.id == 0 || x.id == resource.GetId()) && (x.components == null || x.components.Count == 0));

			this.OnUnload(resource);

		}

		private MovieItem GetMovieItem(IImageComponent component, ResourceBase resource) {

			var item = this.current.FirstOrDefault(x => x.id == resource.GetId());
			//Debug.Log("GetMovieItem: " + (item != null) + " :: " + resource.GetId() + " :: " + component, component as MonoBehaviour);
			if (item != null) {

				var c = component as IResourceReference;
				if (item.components.Contains(c) == false) {

					item.components.Add(c);

				}

			} else {

				//Debug.Log("new MOVIE ITEM: " + resource.GetId());
				item = new MovieItem() {

					resource = WindowSystemResources.GetItem(resource),
					state = MovieItem.State.Stopped,

				};

				this.current.Add(item);

			}

			return item;

		}

		public void Rewind(IImageComponent component, bool pause) {

			var resource = component.GetResource();
			if (resource.loaded == false) {

				Debug.LogWarning("Resource was not loaded yet. Rewind interrupted.", component as MonoBehaviour);
				return;

			}

			var item = this.GetMovieItem(component, resource);
			item.state = (pause == true) ? MovieItem.State.Paused : MovieItem.State.Playing;

			var movie = resource.loadedObject as Texture;
			this.OnRewind(resource, movie, pause);

		}

		public void Play(IImageComponent component, bool loop, bool pause, System.Action onComplete) {
			
			var resource = component.GetResource();
			if (resource.loaded == false) {

				Debug.LogWarning("Resource was not loaded yet. Play interrupted.", component as MonoBehaviour);
				return;

			}

			var item = this.GetMovieItem(component, resource);
			var movie = resource.loadedObject as Texture;

			if (item.state != MovieItem.State.Playing) {

				item.state = MovieItem.State.Playing;
				this.OnPlay(resource, movie, loop, onComplete);

			}
			
			if (pause == true || MovieSystem.CanPlayByQuality(movie) == false) {
				
				this.StartCoroutine(this.PauseWithDelay_YIELD(component, this.delayToPause));
				
			}

		}

		public void Stop(IImageComponent component, int instanceId = 0) {
			
			var resource = component.GetResource();
			if (resource.loaded == false) {
				
				Debug.LogWarning("Resource was not loaded yet. Stop interrupted.", component as MonoBehaviour);
				return;
				
			}

			var item = this.current.FirstOrDefault(x => {

				if (instanceId != 0) {

					return (x.resource.loadedObject as Object).GetID() == instanceId;

				} else {

					return x.id == resource.GetId();

				}

			});

			if (item != null) {

				//Debug.Log("Stop: " + item.id + " :: " + instanceId + " :: " + item.components.Count + " :: " + (component as MonoBehaviour), component as MonoBehaviour);
				//if (WindowSystemResources.Remove(item.resource, component as WindowComponent) == true) {

				if (item.components != null && item.components.Count == 0) {

					if (item.state != MovieItem.State.Stopped) {

						item.state = MovieItem.State.Stopped;

						var movie = resource.loadedObject as Texture;
						this.OnStop(resource, movie);

						//this.current.RemoveAll(x => x.id == item.id);

					}

				}

				//}

			}

		}

		public void Pause(IImageComponent component) {
			
			//var image = component.GetRawImageSource();
			//if (image == null) return;
			
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

				if (item.state != MovieItem.State.Paused) {
					
					item.state = MovieItem.State.Paused;
					this.OnPause(resource, movie);
					
				}
					
				//}

			}

		}
		
		public bool IsPlaying(IImageComponent component) {
			
			//var image = component.GetRawImageSource();
			//if (image == null) return false;

			var resource = component.GetResource();
			if (resource.loaded == false) {

				//Debug.LogWarning("Resource was not loaded yet. IsPlaying returns false.", component as MonoBehaviour);
				return false;

			}

			var movie = resource.loadedObject as Texture;

			return this.IsPlaying(component.GetResource(), movie);
			
		}
		
		public virtual bool IsMovie(Texture texture) {
			
			return false;
			
		}

		protected virtual void OnUnload(ResourceBase resource) {
		}

		protected virtual void OnRewind(ResourceBase resource, Texture movie, bool pause) {

			WindowSystemLogger.Log(this.system, "`Rewind` method not supported on current platform");

		}

		protected virtual void OnPlay(ResourceBase resource, Texture movie, System.Action onComplete) {
			
			WindowSystemLogger.Log(this.system, "`Play` method not supported on current platform");
			
		}

		protected virtual void OnPlay(ResourceBase resource, Texture movie, bool loop, System.Action onComplete) {
			
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

		public void StopAllCoroutines() {

			this.system.StopAllCoroutines();

		}

		public Coroutine StartCoroutine(System.Collections.IEnumerator routine) {

			return this.system.StartCoroutine(routine);

		}

		protected System.Collections.Generic.IEnumerator<byte> PauseWithDelay_YIELD(IImageComponent component, Texture movie, float delay, System.Action<ResourceBase, Texture> callback) {
			
			var timer = 0f;
			while (timer < delay) {
				
				timer += Time.unscaledDeltaTime;
				yield return 0;
				
			}
			
			yield return 0;

			callback.Invoke(component.GetResource(), movie);
			
		}
		
		protected System.Collections.Generic.IEnumerator<byte> PauseWithDelay_YIELD(IImageComponent component, float delay) {
			
			var timer = 0f;
			while (timer < delay) {
				
				timer += Time.unscaledDeltaTime;
				yield return 0;
				
			}
			
			yield return 0;

			this.Pause(component);
			
		}

	}

}