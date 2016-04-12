using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class ResourceBase {
		
		public bool @async = true;
		
		[ReadOnly] public string assetPath;
		[ReadOnly] public string resourcesPath;
		[ReadOnly] public bool loadable = false;
		
		[ReadOnly] public bool multiObjects;
		[ReadOnly] public int objectIndex;

		[ReadOnly] public string customResourcePath;

		private Dictionary<Graphic, int> iterations = new Dictionary<Graphic, int>();

		public IEnumerator Load<T>(Graphic graphic, string customResourcePath, System.Action<T> callback) where T : Object {

			customResourcePath = customResourcePath ?? (string.IsNullOrEmpty(this.customResourcePath) == true ? null : this.customResourcePath);
			this.customResourcePath = customResourcePath;
			
			if (this.loadable == true || string.IsNullOrEmpty(customResourcePath) == false) {
				
				var resourcePath = customResourcePath ?? this.resourcesPath;
				
				if (this.async == true) {

					var iteration = 0;
					var oldColor = Color.white;
					if (graphic != null) {

						TweenerGlobal.instance.removeTweens(graphic, immediately: true);
						oldColor = graphic.color;

						if (this.iterations.TryGetValue(graphic, out iteration) == false) {

							this.iterations.Add(graphic, iteration);

						}

						++this.iterations[graphic];
						iteration = this.iterations[graphic];

					}

					var task = Resources.LoadAsync<T>(resourcePath);
					while (task.isDone == false) {
						
						yield return false;
						
					}

					if (graphic == null || iteration == this.iterations[graphic]) {

						if (this.multiObjects == true && this.objectIndex >= 0) {
							
							callback.Invoke(Resources.LoadAll(resourcePath)[this.objectIndex] as T);
							
						} else {
							
							callback.Invoke(task.asset as T);
							
						}

						if (graphic != null) {

							TweenerGlobal.instance.addTweenAlpha(graphic, WindowSystemResources.GetAsyncLoadFadeTime(), 0f, oldColor.a).tag(graphic).onCancel((g) => { g.color = oldColor; });

						}

					}

				} else {
					
					if (this.multiObjects == true && this.objectIndex >= 0) {
						
						callback.Invoke(Resources.LoadAll(resourcePath)[this.objectIndex] as T);
						
					} else {
						
						var asset = Resources.Load<T>(this.resourcesPath);
						callback.Invoke(asset);
						
					}
					
				}
				
			}
			
		}
		
		public void Unload(Object item) {
			
			Resources.UnloadAsset(item);
			
		}

		#if UNITY_EDITOR
		public virtual void Reset() {
			
			this.customResourcePath = null;
			this.resourcesPath = null;
			this.loadable = false;

		}
		
		public virtual void Validate(Object item) {
			
			if (item == null) {

				return;

			}

			this.assetPath = UnityEditor.AssetDatabase.GetAssetPath(item);

			var resourcePath = (this.assetPath.Contains("/Resources/") == true ? this.assetPath.Split(new string[] { "/Resources/" }, System.StringSplitOptions.None)[1] : string.Empty);
			var ext = System.IO.Path.GetExtension(resourcePath);
			this.resourcesPath = resourcePath.Substring(0, resourcePath.Length - ext.Length);
			
			this.loadable = (string.IsNullOrEmpty(this.resourcesPath) == false);
			
		}
		#endif

	};

	[System.Serializable]
	public class MonoResource : ResourceBase {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		public MonoBehaviour tempResource;
		
		public override void Reset() {
			
			base.Reset();
			
			this.tempResource = null;
			
		}
		
		public override void Validate(Object item) {
			
			if (item == null) {
				
				if (this.tempResource != null) {
					
					item = this.tempResource;
					
				}
				
				if (item == null) return;
				
			}
			
			this.tempResource = item as MonoBehaviour;
			
			base.Validate(item);
			
		}
		#endif

	};

	[System.Serializable]
	public class TextureResource : ResourceBase {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		public Texture tempTexture;
		
		public override void Reset() {
			
			base.Reset();
			
			this.tempTexture = null;
			
		}
		
		public override void Validate(Object item) {

			if (item == null) {
				
				if (this.tempTexture != null) {
					
					item = this.tempTexture;
					
				}
				
				if (item == null) return;
				
			}

			this.tempTexture = item as Texture;
			
			base.Validate(item);
			
		}
		#endif
		
	};
	
	[System.Serializable]
	public class SpriteResource : ResourceBase {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		public Sprite tempSprite;
		
		public override void Reset() {
			
			base.Reset();
			
			this.tempSprite = null;
			
		}
		
		public override void Validate(Object item) {

			if (item == null) {
				
				if (this.tempSprite != null) {
					
					item = this.tempSprite;
					
				}
				
				if (item == null) return;
				
			}

			this.tempSprite = item as Sprite;

			if (this.tempSprite != null) {
				
				var imp = UnityEditor.TextureImporter.GetAtPath(this.assetPath) as UnityEditor.TextureImporter;
				var allObjects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(this.assetPath);
				this.multiObjects = false;
				if (imp.spriteImportMode == UnityEditor.SpriteImportMode.Multiple) {
					
					this.multiObjects = true;
					this.objectIndex = System.Array.IndexOf(allObjects, item);
					
				}
				
			}

			base.Validate(item);
			
		}
		#endif

	};
	
	[System.Serializable]
	public class AutoResourceItem : ResourceBase {
		
		public enum ControlType : byte {
			
			None = 0x0,
			OnShowHide,
			OnInitDeinit,
			
		};

		public ControlType controlType = ControlType.None;

		#if UNITY_EDITOR
		//[HideInInspector]
		public Sprite tempSprite;
		//[HideInInspector]
		public Texture tempTexture;
		#endif

		#if UNITY_EDITOR
		public override void Reset() {

			base.Reset();

			this.tempSprite = null;
			this.tempTexture = null;
			
		}
		
		public override void Validate(Object item) {
			
			if (item == null) {
				
				if (this.tempSprite != null) {
					
					item = this.tempSprite;
					
				}
				
				if (this.tempTexture != null) {
					
					item = this.tempTexture;
					
				}

				if (item == null) return;
				
			}
			
			this.tempSprite = item as Sprite;

			if (this.tempSprite != null) {
				
				var imp = UnityEditor.TextureImporter.GetAtPath(this.assetPath) as UnityEditor.TextureImporter;
				var allObjects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(this.assetPath);
				this.multiObjects = false;
				if (imp.spriteImportMode == UnityEditor.SpriteImportMode.Multiple) {
					
					this.multiObjects = true;
					this.objectIndex = System.Array.IndexOf(allObjects, item);
					
				}
				
			}
			
			this.tempTexture = item as Texture;

			base.Validate(item);

		}
		#endif
		
	}

	public class WindowSystemResources : MonoBehaviour {

		public float resourceAsyncLoadFadeTime = 0.5f;

		public WindowSystemResourcesMap resourcesMap;

		private static WindowSystemResources instance;

		public void Awake() {

			WindowSystemResources.instance = this;

		}

		public void Start() {

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

				this.StartCoroutine(resource.Load<T>(null, null, callback));
				
			}
			
		}

		public static void Load(ILoadableResource resourceController, ResourceBase resource, System.Action callback) {
			
			WindowSystemResources.instance.Load_INTERNAL(resourceController, resource, callback);
			
		}
		
		private void Load_INTERNAL(ILoadableResource resourceController, ResourceBase resource, System.Action callback) {

			if (resource is SpriteResource) {
				
				var image = resourceController as IImageComponent;
				var source = image.GetImageSource();
				this.StartCoroutine(resource.Load<Sprite>(source, null, (data) => {

					image.SetImage(data);
					callback.Invoke();

				}));

			} else if (resource is TextureResource) {
				
				var image = resourceController as IImageComponent;
				var source = image.GetRawImageSource();
				this.StartCoroutine(resource.Load<Texture>(source, null, (data) => {

					image.SetImage(data);
					callback.Invoke();

				}));
				
			}

		}

		public static void LoadAuto(ILoadableResource resourceController, string customResourcePath) {

			WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController, customResourcePath);
			
		}

		public static void LoadAuto(ILoadableResource resourceController, bool onShowHide) {

			var type = resourceController.GetResource().controlType;
			if (onShowHide == true && type == AutoResourceItem.ControlType.OnShowHide) {

				WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController);

			} else if (onShowHide == false && type == AutoResourceItem.ControlType.OnInitDeinit) {
				
				WindowSystemResources.instance.LoadAuto_INTERNAL(resourceController);

			}

		}

		private void LoadAuto_INTERNAL(ILoadableResource resourceController, string customResourcePath = null) {

			var image = resourceController as IImageComponent;
			var source = image.GetImageSource();
			if (source != null) {

				this.StartCoroutine(resourceController.GetResource().Load<Sprite>(source, customResourcePath, (data) => image.SetImage(data)));

			} else {

				var sourceRaw = image.GetRawImageSource();
				if (sourceRaw != null) {
					
					this.StartCoroutine(resourceController.GetResource().Load<Texture>(sourceRaw, customResourcePath, (data) => image.SetImage(data)));

				}

			}

		}
		
		public static void Unload(ResourceBase resource, Object source) {

			resource.Unload(source);

		}

		public static void Unload(ILoadableResource resourceController, ResourceBase resource) {

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

		public static void UnloadAuto(ILoadableResource resourceController, bool onShowHide) {
			
			var type = resourceController.GetResource().controlType;
			if (onShowHide == true && type == AutoResourceItem.ControlType.OnShowHide) {
				
				WindowSystemResources.instance.Unload_INTERNAL(resourceController);
				
			} else if (onShowHide == false && type == AutoResourceItem.ControlType.OnInitDeinit) {
				
				WindowSystemResources.instance.Unload_INTERNAL(resourceController);
				
			}

		}
		
		private void Unload_INTERNAL(ILoadableResource resourceController) {
			
			var image = resourceController as IImageComponent;
			var source = image.GetImageSource();
			if (source != null) {
				
				image.ResetImage();
				resourceController.GetResource().Unload(source.sprite);

			} else {
				
				var sourceRaw = image.GetRawImageSource();
				if (sourceRaw != null) {
					
					image.ResetImage();
					resourceController.GetResource().Unload(sourceRaw.texture);

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
				mapInstance.Register(image as ImageComponent);

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
						//image.SetImage(resourceController.GetResource().tempTexture);

					}
					
				}

			} else {

				if (resourceController.GetResource().loadable == true) {
					
					var image = resourceController as IImageComponent;
					mapInstance.Unregister(image as ImageComponent);

					image.SetImage(resourceController.GetResource().tempSprite);
					image.SetImage(resourceController.GetResource().tempTexture);
					
					resourceController.GetResource().Reset();

				}

			}

		}
		#endif

	}

}