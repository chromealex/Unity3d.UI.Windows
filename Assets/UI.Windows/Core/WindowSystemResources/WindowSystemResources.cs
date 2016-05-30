using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using System.Linq;

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

		internal void SetValues(bool isDone, float progress, Object asset) {

			this.isDone = isDone;
			this.progress = progress;
			this.asset = asset;

		}

		public void Dispose() {

			this.asset = null;
			System.GC.SuppressFinalize(this);

		}

	}

	public class WindowSystemResources : MonoBehaviour {

		[System.Serializable]
		public class Item {

			public long id;
			public Object @object;
			public System.Action onObjectLoaded;
			public List<WindowComponent> references;

			public Object loadedObject;
			public int loadedObjectId;

			public bool loaded;

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

		public static float GetAsyncLoadFadeTime() {

			return WindowSystemResources.instance.resourceAsyncLoadFadeTime;

		}
		
		public static void LoadCustom<T>(ResourceBase resource, System.Action<T> callback) where T : MonoBehaviour {
			
			WindowSystemResources.instance.LoadCustom_INTERNAL(resource, callback);
			
		}
		
		private void LoadCustom_INTERNAL<T>(ResourceBase resource, System.Action<T> callback) where T : MonoBehaviour {
			
			if (resource is MonoResource) {

				this.StartCoroutine(resource.Load<T>(null, null, null, callback));
				
			}
			
		}

		public static void Load(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, string customResourcePath = null) {
			
			WindowSystemResources.instance.Load_INTERNAL(resourceController, onDataLoaded, onComplete, customResourcePath);
			
		}
		
		private void Load_INTERNAL(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, string customResourcePath = null) {

			this.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete, customResourcePath);

		}

		public static void LoadAuto(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, bool onShowHide) {

			var type = resourceController.GetResource().controlType;
			if (onShowHide == true && (type & AutoResourceItem.ControlType.Show) != 0) {

				WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete);

			} else if (onShowHide == false && (type & AutoResourceItem.ControlType.Init) != 0) {
				
				WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete);

			}

		}

		private void LoadAuto_INTERNAL(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, string customResourcePath = null) {

			var image = resourceController as IImageComponent;

			System.Action<Object> setup = (data) => {

				if (data == null) {

					WindowSystemLogger.Error(image, string.Format("Error in ResourcesManager: Required resource can't be loaded. Resource: {0}", image.GetResource().GetId()));
					return;

				}

				var res = resourceController.GetResource();
				res.loadedObject = data;
				res.loadedObjectId = data.GetInstanceID();
				res.loaded = true;

			};

			Graphic source = image.GetImageSource();
			if (source != null) {
				
				this.LoadAndSetup_INTERNAL<Sprite>(image, source, (data) => {

					setup.Invoke(data);
					image.SetImage(data, () => {

						if (onComplete != null) onComplete.Invoke();

					});

					if (onDataLoaded != null) onDataLoaded.Invoke();

				}, customResourcePath);

			} else {

				source = image.GetRawImageSource();
				if (source != null) {
					
					this.LoadAndSetup_INTERNAL<Texture>(image, source, (data) => {

						setup.Invoke(data);
						image.SetImage(data, () => {

							if (onComplete != null) onComplete.Invoke();

						});

						if (onDataLoaded != null) onDataLoaded.Invoke();

					}, customResourcePath);

				}

			}

		}

		private void LoadAndSetup_INTERNAL<T>(IImageComponent image, Graphic graphic, System.Action<T> callbackOnLoad, string customResourcePath = null) where T : Object {

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

			Item item;
			if (this.IsLoaded<T>(image as WindowComponent, image.GetResource(), out item, callbackOnLoad) == false) {
				
				/*coroutine = */this.StartCoroutine(image.GetResource().Load<T>(image, graphic, customResourcePath, (data) => {

					if (data == null) {

						WindowSystemLogger.Error(image, string.Format("Error in ResourcesManager: Required resource can't loaded. Resource: {0}", image.GetResource().GetId()));
						return;

					}

					item.@object = data;
					
					item.loadedObject = data;
					item.loadedObjectId = data.GetInstanceID();
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
						callback.Invoke(itemInner.@object as T);

					};

					itemInner.onObjectLoaded += callbackInner;

				} else {

					callback(itemInner.@object as T);

				}

				return true;

			}

			item = new Item() { id = resource.GetId(), @object = null, loaded = false, references = new List<WindowComponent>() { reference } };
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

			if (resource.loaded == false) return;

			//Debug.LogWarning("Unload: " + resource.GetId(), resourceController as MonoBehaviour);
			var item = this.loaded.FirstOrDefault(x => x.id == resource.GetId());
			if (item != null) {

				if (item.references.Remove(resourceController as WindowComponent) == true) {
					
					this.loaded.RemoveAll(x => {

						if (x.id == resource.GetId() && x.references.Count == 0) {

							if (x.loadedObjectId < 0) Object.Destroy(x.loadedObject);
							return true;

						}

						return false;

					});

				}

			}

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