using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using System.Linq;
using ME;

namespace UnityEngine.UI.Windows {

	public class ResourceAsyncOperation : YieldInstruction, System.IDisposable {

		public ResourceAsyncOperation() {}

		~ResourceAsyncOperation() {

			this.Dispose();

		}

		public int priority { get; set; }
		public bool isDone { get; private set; }
		public float progress { get; private set; }
		public Object asset { get; private set; }
		public bool broke { get; private set; }

		internal void SetValues(bool isDone, float progress, Object asset) {

			this.isDone = isDone;
			this.progress = progress;
			this.asset = asset;

		}

		public void Break() {

			this.broke = true;

		}

		public void Dispose() {

			this.asset = null;
			System.GC.SuppressFinalize(this);

		}

	}

	public class WindowSystemResources : MonoBehaviour {

		[System.Serializable]
		public class Item {

			public int id;
			public System.Action onObjectLoaded;
			public List<WindowComponent> references;

			public Object loadedObject;
			public int loadedObjectId;

			public bool loaded;

			public void Dispose() {
				
				//if (this.loaded == true && this.loadedObjectId < 0 && this.loadedObject != null) Object.Destroy(this.loadedObject);
				this.loadedObject = null;
				this.onObjectLoaded = null;
				this.references = null;

				this.id = 0;
				this.loadedObjectId = 0;
				this.loaded = false;

			}

		};

		public float resourceAsyncLoadFadeTime = 0.5f;

		public WindowSystemResourcesMap resourcesMap;

		public List<Item> loaded = new List<Item>();
		//private Dictionary<int, Coroutine> loading = new Dictionary<int, Coroutine>();

		private static WindowSystemResources instance;

		public void Awake() {

			WindowSystemResources.instance = this;
			
			this.resourcesMap.Reset();

		}

		private void ValidateTexture(IImageComponent component, Texture texture) {

			var comp = component as WindowComponent;
			foreach (var item in this.loaded) {

				if (item.references.Contains(comp) == true) {
					
					item.loadedObject = texture;
					item.loadedObjectId = texture.GetID();

				}

			}

		}

		public static bool Remove(Item item, WindowComponent component, bool forced = false) {

			if (item == null) {

				//Debug.LogWarning("[WindowSystemResources] Trying to remove failed with null in `item`.");
				return false;

			}

			if (item.references == null) {

				//Debug.LogWarning("[WindowSystemResources] Trying to remove failed with null in `references`.");
				return false;

			}

			if (item.references.Remove(component) == true) {

				if (item.references.Count == 0) {

					WindowSystemResources.instance.loaded.RemoveAll(x => {

						if ((x.loaded == true || forced == true) && x.id == item.id) {

							x.Dispose();
							return true;

						}

						return false;

					});

					return true;//WindowSystemResources.instance.loaded.Any(x => x.references.Contains(component) == false);

				}

			}

			return false;

		}

		public static Item GetItem(ResourceBase resource) {

			return WindowSystemResources.instance.loaded.FirstOrDefault(x => x.id == resource.GetId());

		}

		public static float GetAsyncLoadFadeTime() {

			return WindowSystemResources.instance.resourceAsyncLoadFadeTime;

		}

		public static void LoadCustom(IEnumerator routine) {

			WindowSystemResources.instance.StartCoroutine(routine);

		}
		
		public static void LoadCustom<T>(ResourceBase resource, System.Action<T> callback) where T : MonoBehaviour {
			
			WindowSystemResources.instance.LoadCustom_INTERNAL(resource, callback);
			
		}
		
		private void LoadCustom_INTERNAL<T>(ResourceBase resource, System.Action<T> callback) where T : MonoBehaviour {
			
			if (resource is MonoResource) {

				this.StartCoroutine(resource.Load<T>(null, null, null, callback));
				
			}
			
		}

		public static void Load(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, System.Action onFailed = null, string customResourcePath = null) {
			
			WindowSystemResources.instance.Load_INTERNAL(resourceController, onDataLoaded, onComplete, onFailed, customResourcePath);
			
		}
		
		private void Load_INTERNAL(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, System.Action onFailed = null, string customResourcePath = null) {

			this.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete, onFailed, customResourcePath);

		}

		public static void LoadAuto(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, bool onShowHide, System.Action onFailed = null) {

			var type = resourceController.GetResource().controlType;
			if (onShowHide == true && (type & AutoResourceItem.ControlType.Show) != 0) {

				WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete, onFailed);

			} else if (onShowHide == false && (type & AutoResourceItem.ControlType.Init) != 0) {
				
				WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete, onFailed);

			}

		}

		private void LoadAuto_INTERNAL(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, System.Action onFailed = null, string customResourcePath = null) {
			
			var image = resourceController as IImageComponent;

			System.Action<Object> setup = (data) => {

				if (data == null) {

					if (onFailed != null) onFailed.Invoke();
					WindowSystemLogger.Error(image, string.Format("Error in ResourcesManager: Required resource can't be loaded. Resource: {0}", image.GetResource().GetId()));
					return;

				}

				var res = resourceController.GetResource();
				res.loadedObject = data;
				res.loadedObjectId = data.GetID();
				res.loaded = true;

			};

			Graphic source = image.GetImageSource();
			if (source == null) source = image.GetRawImageSource();

			var isMaterial = image.GetResource().IsMaterialLoadingType();

			if (isMaterial == true) {

			} else {

				MovieSystem.UnregisterOnUpdateTexture(this.ValidateTexture);

			}

			if (isMaterial == true) {

				this.LoadAndSetup_INTERNAL<Material>(image, source, (data) => {

					setup.Invoke(data);
					image.SetMaterial(data, callback: () => {

						if (onComplete != null) onComplete.Invoke();

					});

					if (onDataLoaded != null) onDataLoaded.Invoke();

				}, onFailed, customResourcePath);

			} else {

				if (source is Image) {
					
					this.LoadAndSetup_INTERNAL<Sprite>(image, source, (data) => {

						setup.Invoke(data);
						image.SetImage(data, () => {

							if (onComplete != null) onComplete.Invoke();

						});

						if (onDataLoaded != null) onDataLoaded.Invoke();

					}, onFailed, customResourcePath);

				} else if (source is RawImage) {

					this.LoadAndSetup_INTERNAL<Texture>(image, source, (data) => {

						setup.Invoke(data);
						image.SetImage(data, () => {

							if (onComplete != null) onComplete.Invoke();

						});

						MovieSystem.RegisterOnUpdateTexture(this.ValidateTexture);

						if (onDataLoaded != null) onDataLoaded.Invoke();

					}, onFailed, customResourcePath);

				}

			}

		}

		private void LoadAndSetup_INTERNAL<T>(IImageComponent image, Graphic graphic, System.Action<T> callbackOnLoad, System.Action callbackOnFailed, string customResourcePath = null) where T : Object {

			//var key = (image as WindowComponent).GetInstanceID();

			//Coroutine coroutine;
			/*if (this.loading.TryGetValue(key, out coroutine) == true) {

				this.StopCoroutine(coroutine);
				this.loading.Remove(key);

			}*/

			/*this.loaded.ForEach(z => { z.references.Remove(image as WindowComponent); });
			this.loaded.RemoveAll(x => {

				if (x.references.Count == 0) {

					//if (x.loadedObjectId < 0) Object.Destroy(x.loadedObject);
					return true;

				}

				return false;

			});*/

			var resource = image.GetResource();

			Item item;
			if (this.IsLoaded<T>(image as WindowComponent, resource, out item, callbackOnLoad) == false) {
				
				/*coroutine = */this.StartCoroutine(image.GetResource().Load<T>(image, graphic, customResourcePath, (data) => {

					if (data == null) {

						if (callbackOnFailed != null) callbackOnFailed.Invoke();
						//WindowSystemLogger.Error(image, string.Format("Error in ResourcesManager: Required resource can't loaded. Resource: {0}", image.GetResource().GetId()));
						return;

					}

					//Debug.Log("OBJECT LOADED: " + image.GetResource().GetStreamPath());

					item.loadedObject = data;
					item.loadedObjectId = data.GetID();
					//item.streamPath = resource.GetStreamPath();
					item.loaded = true;

					if (item.onObjectLoaded != null) item.onObjectLoaded.Invoke();
					callbackOnLoad(data);

					//this.loading.Remove(key);

				}));

				//this.loading.Add(key, coroutine);
				
			}

		}

		private bool IsLoaded<T>(WindowComponent reference, ResourceBase resource, out Item item, System.Action<T> callback) where T : Object {

			var itemInner = this.loaded.FirstOrDefault(x => x.id == resource.GetId()/*(x.@object == null || x.@object is T)*/);
			if (itemInner != null) {
				
				item = itemInner;

				if (itemInner.references.Contains(reference) == false) {

					itemInner.references.Add(reference);

				} else {

					//Debug.LogError("IsLoaded returns `true` but reference already in list.", reference);

				}

				if (itemInner.loaded == false) {

					System.Action callbackInner = null;
					callbackInner = () => {

						itemInner.onObjectLoaded -= callbackInner;
						callback.Invoke(itemInner.loadedObject as T);

					};

					itemInner.onObjectLoaded += callbackInner;

				} else {

					callback(itemInner.loadedObject as T);

				}

				return true;

			}

			item = new Item() { id = resource.GetId(), loadedObject = null, loaded = false, references = new List<WindowComponent>() { reference } };
			this.loaded.Add(item);

			return false;

		}

		public static void Unload(ResourceBase resource, Object source) {

			resource.Unload(source);

		}

		public static void Unload(ILoadableResource resourceController, ResourceBase resource, bool resetController = true) {

			WindowSystemResources.instance.Unload_INTERNAL(resourceController, resource, resetController);

		}

		public static void UnloadAuto(ILoadableResource resourceController, bool onShowHide) {
			
			var type = resourceController.GetResource().controlType;
			if (onShowHide == true && (type & AutoResourceItem.ControlType.Hide) != 0) {
				
				WindowSystemResources.instance.Unload_INTERNAL(resourceController, resourceController.GetResource());
				
			} else if (onShowHide == false && (type & AutoResourceItem.ControlType.Deinit) != 0) {
				
				WindowSystemResources.instance.Unload_INTERNAL(resourceController, resourceController.GetResource());
				
			}

		}
		
		private void Unload_INTERNAL(ILoadableResource resourceController, ResourceBase resource, bool resetController = true) {
			/*
			if (resource.loaded == false) {


				return;

			}*/

			//Debug.LogWarning("Unload: " + resource.GetId() + " :: " + resource.GetStreamPath(), resourceController as MonoBehaviour);

			var item = this.loaded.FirstOrDefault(x => x.id == resource.GetId());
			if (WindowSystemResources.Remove(item, resourceController as WindowComponent, forced: true) == true) {

				//Debug.LogWarning("Unload movie: " + resource.GetId(), resourceController as MonoBehaviour);

				MovieSystem.Unload(resourceController as IImageComponent, resource);

			}

			/*if (item != null) {

				if (item.references.Remove(resourceController as WindowComponent) == true) {
					
					this.loaded.RemoveAll(x => {

						if (x.id == resource.GetId() && x.references.Count == 0) {

							if (x.loadedObjectId < 0) Object.Destroy(x.loadedObject);
							return true;

						}

						return false;

					});

				}

			}*/

			if (resetController == true) {

				var image = resourceController as IImageComponent;
				var source = image.GetImageSource();
				if (source != null) {
					
					image.ResetImage();
					resource.Unload(source.sprite);

				} else {
					
					var sourceRaw = image.GetRawImageSource();
					if (sourceRaw != null) {
						
						image.ResetImage();
						resource.Unload(sourceRaw.texture);

					}
					
				}

			}
			
		}

		[System.Serializable]
		public class TaskItem {

			public int id;
			public ResourceBase resource;
			public IImageComponent component;
			public Graphic graphic;
			public string customResourcePath;
			public System.Action<Object> onSuccess;
			public System.Action onFailed;

			public ResourceAsyncOperation task;

			public void RaiseSuccess(Object obj) {

				this.onSuccess.Invoke(obj);

			}

			public void RaiseFailed() {

				this.onFailed.Invoke();

			}

			public void Dispose() {

				this.resource = null;
				this.component = null;
				this.graphic = null;
				this.customResourcePath = null;
				this.onSuccess = null;
				this.onFailed = null;

				if (this.task != null) {
					
					this.task.Dispose();
					this.task = null;

				}

			}

		};

		public List<TaskItem> tasks;
		private Dictionary<Graphic, int> taskInterations;

		public static IEnumerator LoadResource<T>(ResourceBase resource, IImageComponent component, Graphic graphic, string customResourcePath, System.Action<T> callback) where T : Object {

			yield return WindowSystemResources.instance.LoadResource_INTERNAL<T>(resource, component, graphic, customResourcePath, callback);

		}

		private IEnumerator LoadResource_INTERNAL<T>(ResourceBase resource, IImageComponent component, Graphic graphic, string customResourcePath, System.Action<T> callback) where T : Object {

			if (this.taskInterations == null) this.taskInterations = new Dictionary<Graphic, int>();

			var iteration = 0;
			if (graphic != null) {
				
				if (this.taskInterations.TryGetValue(graphic, out iteration) == false) {

					this.taskInterations.Add(graphic, ++iteration);

				}

			}

			var task = new TaskItem();
			task.id = resource.GetId();
			task.resource = resource;
			task.component = component;
			task.graphic = graphic;
			task.customResourcePath = customResourcePath;
			task.onSuccess = (obj) => {

				if (graphic != null) {

					var iterationFailed = !(graphic == null || iteration == this.taskInterations[graphic]);
					if (iterationFailed == false) {

						callback.Invoke(obj as T);

					} else {

						task.onFailed.Invoke();

					}

				} else {

					callback.Invoke(obj as T);

				}

			};
			task.onFailed = () => {

				callback.Invoke(null);

			};

			this.tasks.Add(task);

			//Debug.Log("Resource Task: " + task.resource.assetPath);

			yield return this.StartCoroutine(this.StartTask<T>(task));

			task.Dispose();
			this.tasks.Remove(task);

		}

		private IEnumerator StartTask<T>(TaskItem task) where T : Object {

			#region Load Resource
			if (task.resource.loadableResource == true || string.IsNullOrEmpty(task.customResourcePath) == false) {

				var resourcePath = task.customResourcePath ?? task.resource.resourcesPath;
				if (task.resource.async == true) {

					var asyncTask = Resources.LoadAsync<T>(resourcePath);
					while (asyncTask.isDone == false) {

						yield return false;

					}

					if (task.resource.multiObjects == true && task.resource.objectIndex >= 0) {

						task.RaiseSuccess(Resources.LoadAll(resourcePath)[task.resource.objectIndex]);

					} else {

						task.RaiseSuccess(asyncTask.asset);

					}

					asyncTask = null;

				} else {

					if (task.resource.multiObjects == true && task.resource.objectIndex >= 0) {

						task.RaiseSuccess(Resources.LoadAll(resourcePath)[task.resource.objectIndex]);
						
					} else {

						var asset = Resources.Load<T>(resourcePath);
						task.RaiseSuccess(asset);

					}

				}

			} else if (task.resource.loadableStream == true) {
				
				task.task = MovieSystem.LoadTexture(task.component);
				var timer = 0f;
				while (task.task.isDone == false) {

					timer += Time.unscaledDeltaTime;

					if (timer >= 3f) {

						break;

					}

					yield return false;

				}

				if (task.task != null && task.task.isDone == true) {

					//Debug.Log("Loaded: " + component.GetResource().GetStreamPath() + ", iter: " + iteration + ", type: " + typeof(T).ToString() + ", asset: " + task.asset, graphic);
					task.RaiseSuccess(task.task.asset);

				} else {

					task.RaiseFailed();

				}

			}
			#endregion

		}

		/*private void ___() {

			if (ResourceBase.iterations == null) ResourceBase.iterations = new Dictionary<Graphic, int>();
			//if (ResourceBase.colorCache == null) ResourceBase.colorCache = new Dictionary<Graphic, Color>();

			customResourcePath = customResourcePath ?? (string.IsNullOrEmpty(this.customResourcePath) == true ? null : this.customResourcePath);
			this.customResourcePath = customResourcePath;

			var isFade = (WindowSystemResources.GetAsyncLoadFadeTime() > 0f);

			var iterationFailed = false;
			var iteration = 0;
			var oldColor = Color.white;
			if (graphic != null) {
				
				if (ResourceBase.iterations.TryGetValue(graphic, out iteration) == false) {

					ResourceBase.iterations.Add(graphic, iteration);

				}

				++ResourceBase.iterations[graphic];
				iteration = ResourceBase.iterations[graphic];

			}

			#region Load Resource
			if (this.loadableResource == true || string.IsNullOrEmpty(customResourcePath) == false) {

				var resourcePath = customResourcePath ?? this.resourcesPath;

				if (this.async == true) {

					var task = Resources.LoadAsync<T>(resourcePath);
					while (task.isDone == false) {

						yield return false;

					}

					iterationFailed = !(graphic == null || iteration == ResourceBase.iterations[graphic]);
					if (iterationFailed == false) {

						if (this.multiObjects == true && this.objectIndex >= 0) {

							callback.Invoke(Resources.LoadAll(resourcePath)[this.objectIndex] as T);

						} else {

							callback.Invoke(task.asset as T);

						}

					}

					task = null;

				} else {

					if (this.multiObjects == true && this.objectIndex >= 0) {

						callback.Invoke(Resources.LoadAll(resourcePath)[this.objectIndex] as T);

					} else {

						var asset = Resources.Load<T>(resourcePath);
						callback.Invoke(asset);

					}

				}

			} else if (this.loadableStream == true) {

				if (this.task != null) {

					//Debug.LogWarning("Resource start loading while old was not complete");

				}

				this.task = MovieSystem.LoadTexture(component);
				while (this.task != null && this.task.isDone == false) {

					iterationFailed = !(graphic == null || iteration == ResourceBase.iterations[graphic]);
					if (iterationFailed == true && this.task != null) {

						//Debug.Log("Break: " + this.GetStreamPath());

						this.task.Break();
						this.task.Dispose();
						this.task = null;
						callback.Invoke(null);
						yield break;

					}

					yield return false;

				}

				if (this.task != null) {

					iterationFailed = !(graphic == null || iteration == ResourceBase.iterations[graphic]);
					if (iterationFailed == false) {

						//Debug.Log("Loaded: " + component.GetResource().GetStreamPath() + ", iter: " + iteration + ", type: " + typeof(T).ToString() + ", asset: " + task.asset, graphic);

						callback.Invoke(this.task.asset as T);

					} else {

						callback.Invoke(null);

					}

					this.task.Dispose();
					this.task = null;

				}

			}
			#endregion

		}*/
		
		#if UNITY_EDITOR
		public static void Validate(ILoadableResource resourceController) {

			if (Application.isPlaying == true) return;

			var mapInstance = WindowSystemResourcesMap.FindFirst();
			if (mapInstance == null) return;

			if (resourceController.GetResource().controlType != AutoResourceItem.ControlType.None) {

				var image = resourceController as IImageComponent;
				mapInstance.Register(image);

				var source = image.GetImageSource();
				if (source != null) {
					
					resourceController.GetResource().Validate(source.sprite);
					image.ResetImage();
					//image.SetImage(resourceController.GetResource().tempSprite);

				} else {
					
					var sourceRaw = image.GetRawImageSource();
					if (sourceRaw != null) {
						
						resourceController.GetResource().Validate(sourceRaw.texture);
						image.ResetImage();

					}
					
				}

				resourceController.GetResource().Validate(resourceController.GetResource().tempObject);

			} else {

				if (resourceController.GetResource().loadableResource == true) {
					
					var image = resourceController as IImageComponent;
					mapInstance.Unregister(image);

					image.SetImage(resourceController.GetResource().tempObject as Sprite);
					image.SetImage(resourceController.GetResource().tempObject as Texture);
					
					resourceController.GetResource().Reset();

				}

			}

		}
		#endif

	}

}