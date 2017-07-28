using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using System.Linq;
using ME;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.Plugins.Resources;

namespace UnityEngine.UI.Windows {
	
	public class WindowSystemResources : ServiceManager<WindowSystemResources> {

		public const string BUNDLES_ROOT_PATH = "Bundles";

		public override UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions GetAuthPermission() {

			return UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions.None;

		}

		public override string GetServiceName() {

			return WindowSystemResources.GetName();

		}

		public static string GetName() {

			return "Resources";

		}

		[System.Serializable]
		public class Item {

			#if UNITY_EDITOR
			public string name;
			#endif

			public int id;
			public System.Type type;
			public System.Action<Item> onObjectLoaded;
            public System.Action<Item> onObjectFailed;

            #if UNITY_EDITOR
            private int _referencesCount;
			public int referencesCount {

				set {

					this._referencesCount = value;
					this.UpdateName();

				}

				get {

					return this._referencesCount;

				}

			}
			#endif

			public List<IResourceReference> references;

			public object _loadedObject;
			public object loadedObject {

				set {

					this._loadedObject = value;
					#if UNITY_EDITOR
					this.UpdateName();
					#endif

				}

				get {

					return this._loadedObject;

				}

			}

			public bool loaded;
		    public bool loadingResult;

			#if UNITY_EDITOR
			private void UpdateName() {

				if (this._loadedObject != null) this.name = string.Format("[{0}] {1} ({2})", this._referencesCount, (this._loadedObject != null) ? (this._loadedObject is Object ? (this._loadedObject as Object).name : this._loadedObject.ToString()) : "Null", (this.type == null ? "Null" : this.type.ToString()));

			}
			#endif

			public void Dispose(ResourceBase resource) {

				resource.Unload();

				//if (this.loaded == true && this.loadedObjectId < 0 && this.loadedObject != null) Object.Destroy(this.loadedObject);
				this.loadedObject = null;
				this.onObjectLoaded = null;
				this.references = null;

				this.id = 0;
				this.type = null;
				this.loaded = false;

				#if UNITY_EDITOR
				this.referencesCount = 0;
				#endif

			}

		};

		[System.Serializable]
		public class ObjectEntity {

			public string name;
			public int instanceId;
			public WindowObject reference;
			public WindowBase window;

		};

		public List<ObjectEntity> registered = new List<ObjectEntity>();

		public bool forceAsyncOff = true;
		public float resourceAsyncLoadFadeTime = 0.5f;
		public WindowSystemResourcesMap resourcesMap;
		public List<Item> loaded = new List<Item>();

		public static void RegisterObject(WindowObject obj) {

			if (WindowSystem.IsDebugWeakReferences() == false) return;
			if (Application.isPlaying == false) return;

			//if ((obj.GetWindow() is MW2.UI.EditorTools.Console.ConsoleScreen) == false/*obj.GetComponentsInParent<MW2.UI.EditorTools.Console.ConsoleScreen>(true).Length == 0*/) {

				//Debug.Log("ADD: " + obj.name + " :: " + obj.GetInstanceID());

				var item = WindowSystemResources.instance.registered.FirstOrDefault(x => x.instanceId == obj.GetInstanceID());
				if (item != null) {

					Debug.LogError("Object is already in list: " + item.name + " :: " + item.instanceId);

				}
				WindowSystemResources.instance.registered.Add(new ObjectEntity() { name = string.Format("{0} ({1})", obj.name, obj.GetInstanceID()), instanceId = obj.GetInstanceID(), reference = obj, window = obj.GetWindow() });
				WeakReferenceInfo.Register(obj);

			//}

		}

		public static void UnregisterObject(WindowObject obj) {

			if (WindowSystem.IsDebugWeakReferences() == false) return;
			if (Application.isPlaying == false) return;

			//if ((obj.GetWindow() is MW2.UI.EditorTools.Console.ConsoleScreen) == false) {

				//Debug.Log("DEL: " + obj.name + " :: " + obj.GetInstanceID());

				var item = WindowSystemResources.instance.registered.FirstOrDefault(x => x.instanceId == obj.GetInstanceID());
				if (item != null) {
					
					WindowSystemResources.instance.registered.RemoveAll(x => x.instanceId == obj.GetInstanceID());

				} else {

					Debug.LogWarning(string.Format("Trying to unregister reference that doesn't exists: {0} ({1})", obj.name, obj.GetInstanceID()), obj);

				}

			//}

		}

		public static object GetResourceObjectById(ResourceBase resource) {

			var item = WindowSystemResources.GetItem(resource);
			if (item == null) return null;
			
			return item.loadedObject;

		}

		public static bool IsResourceObjectLoaded(ResourceBase resource) {

			var item = WindowSystemResources.GetItem(resource);
			if (item == null) return false;

			return item.loaded;

		}

		public static WindowSystemResourcesSettings GetSettings() {

#if UNITY_EDITOR
			if (Application.isPlaying == false) {

				WindowSystemResourcesSettings settings = null;

				var obj = GameObject.FindObjectOfType<WindowSystemResources>();
				if (obj == null) {
					
					var objs = ME.EditorUtilities.GetObjectsOfType<WindowSystemResources>("Assets/", string.Empty, (w) => true, useCache: false);
					if (objs.Length > 0) {

						settings = objs[0].settings as WindowSystemResourcesSettings;

					} else {

						Debug.LogError("No settings `WindowSystemResourcesSettings` found");

					}

				} else {

					settings = obj.settings as WindowSystemResourcesSettings;

				}
				
				return settings;

			}
#endif

			return WindowSystemResources.instance.settings as WindowSystemResourcesSettings;

		}

		private void ValidateTexture(IImageComponent component, Texture texture) {

			var comp = component as IResourceReference;
			foreach (var item in this.loaded) {

				if (item.references.Contains(comp) == true) {
					
					item.loadedObject = texture;

				}

			}

		}

		public static bool Remove(Item item, IResourceReference component, ResourceBase resource) {

			if (item == null) {

				//Debug.LogWarning("[WindowSystemResources] Trying to remove failed with null in `item`.");
				return false;

			}

			if (item.references == null) {

				//Debug.LogWarning("[WindowSystemResources] Trying to remove failed with null in `references`.");
				return false;

			}

			if (item.references.Remove(component) == true) {

				#if UNITY_EDITOR
				item.referencesCount = item.references.Count;
				#endif

			    return WindowSystemResources.RemoveIfRefsZero_INTERNAL(item, resource);

			}

			return false;

		}

	    private static bool RemoveIfRefsZero_INTERNAL(Item item, ResourceBase resource) {

			if (item == null || item.references == null || item.references.Count != 0) return false;

            item.Dispose(resource);
            WindowSystemResources.instance.loaded.RemoveAll(x => {

				if (x.id == item.id && x.type == item.type) {

                    return true;

                }

                return false;

            });

            return true;

	    }

		public static Item GetItem(ResourceBase resource) {

			return WindowSystemResources.instance.loaded.FirstOrDefault(x => x.id == resource.GetId());

		}

		public static float GetAsyncLoadFadeTime() {

			return WindowSystemResources.instance.resourceAsyncLoadFadeTime;

		}

		public static void LoadCustom(System.Collections.Generic.IEnumerator<byte> routine) {

			Coroutines.Run(routine);

		}

		public static void Load(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, System.Action onFailed = null, string customResourcePath = null) {
			
			WindowSystemResources.instance.Load_INTERNAL(resourceController, onDataLoaded, onComplete, onFailed, customResourcePath);
			
		}
		
		private void Load_INTERNAL(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, System.Action onFailed = null, string customResourcePath = null) {

			this.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete, onFailed, customResourcePath);

		}

		public static void LoadAuto(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, bool onShowHide, System.Action onFailed = null) {

			var type = (resourceController.GetResource() as ResourceAuto).controlType;
			if (onShowHide == true && (type & ResourceAuto.ControlType.Show) != 0) {

				WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete, onFailed);

			} else if (onShowHide == false && (type & ResourceAuto.ControlType.Init) != 0) {
				
				WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController, onDataLoaded, onComplete, onFailed);

			}

		}

		private void LoadAuto_INTERNAL(ILoadableResource resourceController, System.Action onDataLoaded, System.Action onComplete, System.Action onFailed = null, string customResourcePath = null) {
			
			var image = resourceController as IImageComponent;
			var async = image.GetResource().async;

			System.Action<Object> setup = (data) => {

				if (data == null) {

					if (onFailed != null) onFailed.Invoke();
					WindowSystemLogger.Error(image, string.Format("Error in ResourcesManager: Required resource can't be loaded. Resource: {0}", image.GetResource().GetId()));
					return;

				}

			};

			var source = image.GetGraphicSource();

			var isMaterial = image.GetResource().IsMaterialLoadingType();
			if (isMaterial == false) {
				
				MovieSystem.UnregisterOnUpdateTexture(this.ValidateTexture);

			}

			if (isMaterial == true) {

				this.LoadRefCounter_INTERNAL<Material>(resourceController, (data) => {

					setup.Invoke(data);
					if (onDataLoaded != null) onDataLoaded.Invoke();

					image.SetMaterial(data, callback: () => {

						if (onComplete != null) onComplete.Invoke();

					});

				}, onFailed, async, customResourcePath);

			} else {

				if (source is Image) {
					
					this.LoadRefCounter_INTERNAL<Sprite>(resourceController, (data) => {

						setup.Invoke(data);
						if (onDataLoaded != null) onDataLoaded.Invoke();

						image.SetImage(data, () => {

							if (onComplete != null) onComplete.Invoke();

						});

					}, onFailed, async, customResourcePath);

				} else if (source is RawImage) {

					this.LoadRefCounter_INTERNAL<Texture>(resourceController, (data) => {

						setup.Invoke(data);
						if (onDataLoaded != null) onDataLoaded.Invoke();

						image.SetImage(data, () => {

							if (onComplete != null) onComplete.Invoke();

						});

						if (isMaterial == true) MovieSystem.RegisterOnUpdateTexture(this.ValidateTexture);

					}, onFailed, async, customResourcePath);

				}

			}

		}

		public static void LoadRefCounter<T>(IResourceReference reference, ResourceBase resource, System.Action<T> callbackOnLoad, System.Action callbackOnFailed, bool async) /*where T : Object*/ {

			if (WindowSystemResources.instance != null) {

				WindowSystemResources.instance.LoadRefCounter_INTERNAL<T>(reference, resource, callbackOnLoad, callbackOnFailed, async, customResourcePath: null);

			}

		}

		public static void LoadRefCounter<T>(ILoadableResource image, System.Action<T> callbackOnLoad, System.Action callbackOnFailed, bool async) where T : Object {
			
			if (WindowSystemResources.instance != null) {

				WindowSystemResources.instance.LoadRefCounter_INTERNAL<T>(image, callbackOnLoad, callbackOnFailed, async, customResourcePath: null);

			}

		}

		public static void LoadRefCounter<T>(ILoadableResource image, System.Action<T> callbackOnLoad, System.Action callbackOnFailed, bool async, string customResourcePath) /*where T : Object*/ {

			if (WindowSystemResources.instance != null) {

				WindowSystemResources.instance.LoadRefCounter_INTERNAL<T>(image, callbackOnLoad, callbackOnFailed, async, customResourcePath);

			}

		}

		private void LoadRefCounter_INTERNAL<T>(ILoadableResource image, System.Action<T> callbackOnLoad, System.Action callbackOnFailed, bool async, string customResourcePath) /*where T : Object*/ {
			
			IResourceReference reference = null;
			if (image is ILoadableReference) {

				reference = (image as ILoadableReference).GetReference();

			} else {

				reference = image as IResourceReference;

			}

			var resource = image.GetResource();
			this.LoadRefCounter_INTERNAL(reference, resource, callbackOnLoad, callbackOnFailed, async, customResourcePath);

		}

		private void LoadRefCounter_INTERNAL<T>(IResourceReference reference, ResourceBase resource, System.Action<T> callbackOnLoad, System.Action callbackOnFailed, bool async, string customResourcePath) /*where T : Object*/ {

			if (resource.IsLoadable() == true) {

				//Debug.Log("Check: " + resource.assetPath + ", typeof: " + typeof(T).ToString());
				Item item;
				if (this.IsLoaded<T>(reference, resource, out item, callbackOnLoad, callbackOnFailed) == false) {

					//Debug.Log("Loading: " + resource.assetPath);
					Coroutines.Run(resource.Load<T>(reference, customResourcePath, (data) => {

						//Debug.Log("Loaded: " + resource.assetPath + " >> " + data);
						if (data == null) {

							//Debug.LogWarning(string.Format("Failed to load resource in {0}", resource.assetPath));

                            item.loadingResult = false;
						    if (item.onObjectFailed != null) {

								item.onObjectFailed.Invoke(item);

						    }

						    item.onObjectLoaded = null;
                            item.onObjectFailed = null;
                            //if (callbackOnFailed != null) callbackOnFailed.Invoke();
                            //WindowSystemLogger.Error(image, string.Format("Error in ResourcesManager: Required resource can't loaded. Resource: {0}", image.GetResource().GetId()));
                            return;

						}

					    item.loadingResult = true;
						item.loadedObject = data;
						item.loaded = true;

						if (item.onObjectLoaded != null) item.onObjectLoaded.Invoke(item);
						item.onObjectLoaded = null;
                        item.onObjectFailed = null;

                        //callbackOnLoad(data);

                    }, async));

				}

			} else {

				if (callbackOnFailed != null) callbackOnFailed.Invoke();

			}

		}

        private void OnItemCallback_INTERNAL<T>(Item item, ResourceBase resource, System.Action<T> callbackLoaded, System.Action callbackFailed) /*where T : Object*/ {

            item.onObjectLoaded += (itemInner) => {

                if (WindowSystemResources.RemoveIfRefsZero_INTERNAL(itemInner, resource) == true) return;

				if (callbackLoaded != null) callbackLoaded.Invoke((T)itemInner.loadedObject);

            };

			item.onObjectFailed += (itemInner) => {

				if (callbackFailed != null) callbackFailed.Invoke();

			};

        }
        
        private bool IsLoaded<T>(IResourceReference reference, ResourceBase resource, out Item item, System.Action<T> callbackLoaded, System.Action callbackFailed) /*where T : Object*/ {

			var typeOf = typeof(T);
			var itemInner = this.loaded.FirstOrDefault(x => x.id == resource.GetId() && x.type == typeOf/*(x.@object == null || x.@object is T)*/);
			if (itemInner != null) {
				
				item = itemInner;

				if (itemInner.references.Contains(reference) == false) {

					itemInner.references.Add(reference);
					#if UNITY_EDITOR
					itemInner.referencesCount = itemInner.references.Count;
					#endif

				} else {
					
					//Debug.LogError("IsLoaded returns `true` but reference already in list.", reference);

				}

				if (itemInner.loaded == false) {
                    
				    this.OnItemCallback_INTERNAL<T>(itemInner, resource, callbackLoaded, callbackFailed);
                    
				} else {
                    
				    if (itemInner.loadingResult == true) {

						callbackLoaded.Invoke((T)itemInner.loadedObject);

				    } else {
				        
                        callbackFailed.Invoke();

				    }

				}

				return true;

			}

			item = new Item() { id = resource.GetId(), type = typeOf, loadedObject = null, loaded = false, references = new List<IResourceReference>() { reference } };
			#if UNITY_EDITOR
			item.referencesCount = 1;
			#endif
			this.loaded.Add(item);

            this.OnItemCallback_INTERNAL(item, resource, callbackLoaded, callbackFailed);

            return false;

		}

		public static void UnloadResource_INTERNAL(IResourceReference resourceController, ResourceBase resource) {

			var item = WindowSystemResources.instance.loaded.FirstOrDefault(x => x.id == resource.GetId());
			if (WindowSystemResources.Remove(item, resourceController, resource) == true) {
				
				MovieSystem.Unload(resourceController as IImageComponent, resource);

			}

		}

		public static void Unload(IResourceReference resourceController, ResourceBase resource, bool resetController = true) {

			WindowSystemResources.instance.Unload_INTERNAL(resourceController, resource, resetController);

		}

		public static void UnloadAuto(ILoadableResource resourceController, bool onShowHide) {
			
			var type = (resourceController.GetResource() as ResourceAuto).controlType;
			if (onShowHide == true && (type & ResourceAuto.ControlType.Hide) != 0) {
				
				WindowSystemResources.instance.Unload_INTERNAL(resourceController as IResourceReference, resourceController.GetResource());
				
			} else if (onShowHide == false && (type & ResourceAuto.ControlType.Deinit) != 0) {
				
				WindowSystemResources.instance.Unload_INTERNAL(resourceController as IResourceReference, resourceController.GetResource());
				
			}

		}
		
		private void Unload_INTERNAL(IResourceReference resourceController, ResourceBase resource, bool resetController = true) {

			if (resetController == true) {

				var image = resourceController as IImageComponent;
				var source = image.GetImageSource();
				if (source != null) {

					image.ResetImage();
					//resource.Unload();

				} else {

					var sourceRaw = image.GetRawImageSource();
					if (sourceRaw != null) {

						image.ResetImage();
						//resource.Unload();

					}

				}

			}

			WindowSystemResources.UnloadResource_INTERNAL(resourceController, resource);

		}

		[System.Serializable]
		public class TaskItem {

			public int id;
			public ResourceBase resource;
			public bool async;
			public IResourceReference component;
			//public Graphic graphic;
			public string customResourcePath;
			public System.Action<object> onSuccess;
			public System.Action onFailed;
			public IEnumerator coroutine;

			public ResourceAsyncOperation task;

			public void RaiseSuccess(object obj) {

				this.onSuccess.Invoke(obj);

			}

			public void RaiseFailed() {

				this.onFailed.Invoke();

			}

			public void Dispose() {

				this.resource = null;
				this.component = null;
				//this.graphic = null;
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
		
		public static void LoadResource<T>(ResourceBase resource, System.Action<T> callback, bool async) /*where T : Object*/ {

			Coroutines.Run(WindowSystemResources.instance.LoadResource_INTERNAL<T>(resource, component: null, customResourcePath: null, callback: callback, async: async));

		}

		public static System.Collections.Generic.IEnumerator<byte> LoadResource<T>(ResourceBase resource, IResourceReference component, string customResourcePath, System.Action<T> callback, bool async) /*where T : Object*/ {
			
			return WindowSystemResources.instance.LoadResource_INTERNAL<T>(resource, component, customResourcePath, callback, async);

		}

		private System.Collections.Generic.IEnumerator<byte> LoadResource_INTERNAL<T>(ResourceBase resource, IResourceReference component, string customResourcePath, System.Action<T> callback, bool async) /*where T : Object*/ {

			var task = new TaskItem();
			task.id = resource.GetId();

			if (this.forceAsyncOff == true) {

				// Disabling async resource loading on Android, because for some reason Resources.LoadAsync() is terribly slow
				// as of Unity 5.6.2p2 (takes ~2 minutes to load some resources).
				async = false;

			}

			task.async = async;
			task.resource = resource;
			task.component = component;
			task.customResourcePath = customResourcePath;
			task.onSuccess = (obj) => {

				T resultObj = default(T);

				if (obj is GameObject && typeof(T).HasBaseType(typeof(Component)) == true && typeof(T).HasBaseType(typeof(GameObject)) == false) {

					resultObj = (obj as GameObject).GetComponent<T>();

				} else {

					//Debug.Log(typeof(T) + " << " + obj);
					resultObj = (T)obj;

				}

				callback.Invoke(resultObj);

			};
			task.onFailed = () => {

				callback.Invoke(default(T));

			};

			this.tasks.Add(task);

			//Debug.Log("Resource Task: " + task.resource.assetPath);

			task.coroutine = this.StartTask<T>(task);
			while (task.coroutine.MoveNext() == true) {
				
				yield return 0;

			}

			task.Dispose();
			this.tasks.Remove(task);

		}

		private System.Collections.Generic.IEnumerator<byte> StartTask<T>(TaskItem task) /*where T : Object*/ {

			var isBytesOutput = (typeof(T) == typeof(byte[]));

			#region Load Resource
			if (task.resource.loadableWeb == true) {

				if (task.resource.webPath.Contains("://") == false) {

					task.resource.webPath = string.Format("file://{0}", task.resource.webPath);

				}

				WWW www = null;
				if (task.resource.cacheVersion > 0) {

					www = WWW.LoadFromCacheOrDownload(task.resource.webPath, task.resource.cacheVersion);

				} else {

					www = new WWW(task.resource.webPath);

				}

				while (www.isDone == false) yield return 0;

				if (string.IsNullOrEmpty(www.error) == true) {

					var type = typeof(T);
					if (type == typeof(Texture) ||
						type == typeof(Texture2D)) {

						task.RaiseSuccess(task.resource.readableTexture == true ? www.texture : www.textureNonReadable);

					} else {

						task.RaiseSuccess(www.bytes);

					}

				} else {

					Debug.LogError(string.Format("Task WebRequest [{0}] error: {1}", www.url, www.error));
					task.RaiseFailed();

				}

				www.Dispose();
				www = null;

			} else if (task.resource.loadableResource == true || (string.IsNullOrEmpty(task.customResourcePath) == false && task.resource.loadableAssetBundle == false)) {

				Object data = null;
				var resourcePath = task.customResourcePath ?? task.resource.resourcesPath;
				if (task.async == true) {

					var asyncTask = Resources.LoadAsync(resourcePath, isBytesOutput == true ? typeof(TextAsset) : typeof(T));
					while (asyncTask.isDone == false) {

						yield return 0;

					}

					data = asyncTask.asset;
					asyncTask = null;

				}

				if (task.resource.multiObjects == true && task.resource.objectIndex >= 0) {
					
					task.RaiseSuccess(Resources.LoadAll(resourcePath)[task.resource.objectIndex]);

				} else {

					if (isBytesOutput == true) {

						if (data == null) data = Resources.Load<TextAsset>(resourcePath);
						task.RaiseSuccess(((data as TextAsset).bytes));

					} else {

						if (data == null) data = Resources.Load(resourcePath, typeof(T));
						task.RaiseSuccess(data);

					}

				}

			} else if (task.resource.loadableStream == true) {

				if (task.resource.IsMovie() == true) {

					//Debug.Log("LoadMovie: " + task.component);
					task.task = MovieSystem.LoadTexture(task.component as IImageComponent);
					var startTime = Time.realtimeSinceStartup;
					var timer = 0f;
					while (task.task.isDone == false) {

						timer = Time.realtimeSinceStartup - startTime;

						if (timer >= 3f) {

							break;

						}

						yield return 0;

					}

					if (task.task != null && task.task.isDone == true) {

						//Debug.Log("Loaded: " + component.GetResource().GetStreamPath() + ", iter: " + iteration + ", type: " + typeof(T).ToString() + ", asset: " + task.asset, graphic);
						task.RaiseSuccess(task.task.asset);

					} else {

						task.RaiseFailed();

					}

				} else {

					WWW www = null;
					if (task.resource.cacheVersion > 0) {

						www = WWW.LoadFromCacheOrDownload(task.resource.GetStreamPath(withFile: true), task.resource.cacheVersion);

					} else {

						www = new WWW(task.resource.GetStreamPath(withFile: true));

					}

					while (www.isDone == false) yield return 0;

					if (string.IsNullOrEmpty(www.error) == true) {

						var type = typeof(T);
						//Debug.Log("LOADED: " + type.ToString() + " :: " + task.resource.GetStreamPath(withFile: true));
						if (type == typeof(Texture) ||
							type == typeof(Texture2D)) {

							task.RaiseSuccess(task.resource.readableTexture == true ? www.texture : www.textureNonReadable);

						} else {
							
							var data = www.bytes;
							if (isBytesOutput == true) {

								task.RaiseSuccess(data);

							} else {

								task.RaiseSuccess(null);

							}

						}

					} else {

						//Debug.Log("NOT LOADED: " + task.resource.GetStreamPath(withFile: true) + " :: " + www.error);
						task.RaiseFailed();

					}

					www.Dispose();
					www = null;

				}

			} else if (task.resource.loadableAssetBundle == true) {

				#if UNITY_IOS
				if (UnityEngine.iOS.OnDemandResources.enabled == true) {
					/*var request = UnityEngine.iOS.OnDemandResources.PreloadAsync(new string[] { odrTag } );
					// Wait until request is completed
					yield return request;
					// Check for errors
					if (request.error != null) {
						task.RaiseFailed();
						return;
					}
					 var bundle = AssetBundle.CreateFromFile("res://" + resourceName);*/
				}
				#endif

				var path = task.resource.GetAssetBundlePath(true);

				WWW www = null;
				if (task.resource.cacheVersion > 0) {

					www = WWW.LoadFromCacheOrDownload(path, task.resource.cacheVersion);

				} else {

					www = new WWW(path);

				}

				while (www.isDone == false) yield return 0;

				if (string.IsNullOrEmpty(www.error) == true && www.assetBundle != null) {

					var assets = www.assetBundle.LoadAllAssets();
					var asset = assets[task.resource.objectIndexAssetBundle];
					task.RaiseSuccess(asset);
					www.assetBundle.Unload(false);

				} else {

					task.RaiseFailed();

				}

				www.Dispose();
				www = null;

			}
			#endregion

		}

		#if UNITY_EDITOR
		public static void Validate(ILoadableResource resourceController) {

			if (Application.isPlaying == true) return;

			var image = resourceController as IImageComponent;
			image.GetResource().Validate();

			//var mapInstance = WindowSystemResourcesMap.FindFirst();
			//if (mapInstance == null) return;

			/*if (resourceController.GetResource().controlType != ResourceAuto.ControlType.None) {

				var image = resourceController as IImageComponent;
				//mapInstance.Register(image);

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

				resourceController.GetResource().Validate((resourceController.GetResource() as IResourceValidationObject).GetValidationObject());

			} else {

				if (resourceController.GetResource().loadableResource == true) {
					
					var image = resourceController as IImageComponent;
					//mapInstance.Unregister(image);

					image.SetImage((resourceController.GetResource() as IResourceValidationObject).GetValidationObject() as Sprite);
					image.SetImage((resourceController.GetResource() as IResourceValidationObject).GetValidationObject() as Texture);
					
					resourceController.GetResource().ResetToDefault();

				}

			}*/

		}
		
		public static readonly Dictionary<UnityEditor.BuildTarget, string> ALL_TARGETS = new Dictionary<UnityEditor.BuildTarget, string>() {
			{ UnityEditor.BuildTarget.Android, "Android" },
			{ UnityEditor.BuildTarget.iOS, "iOS" },

			{ UnityEditor.BuildTarget.PS4, "PS4" },
			{ UnityEditor.BuildTarget.XboxOne, "XboxOne" },
			{ UnityEditor.BuildTarget.tvOS, "tvOS" },

			{ UnityEditor.BuildTarget.StandaloneLinuxUniversal, "Linux" },
			{ UnityEditor.BuildTarget.StandaloneOSXUniversal, "Mac" },
			{ UnityEditor.BuildTarget.StandaloneWindows, "Windows" },
		};

		public static string GetAssetBundlePath(UnityEditor.BuildTarget target) {
			var directory = WindowSystemResources.ALL_TARGETS[target];
			var settings = WindowSystemResources.GetSettings();
			var outputPath = settings != null && settings.loadFromStreamingAssets == true ? 
					Application.streamingAssetsPath :
				System.IO.Path.Combine(System.Environment.CurrentDirectory, WindowSystemResources.BUNDLES_ROOT_PATH);
			outputPath = System.IO.Path.Combine(outputPath, directory);

			return outputPath;

		}
		#endif

	}

}