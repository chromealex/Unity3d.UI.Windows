using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class ResourceParametersAttribute : PropertyAttribute {

		public ResourceBase.ControlType drawOnly;

		public ResourceParametersAttribute(ResourceBase.ControlType drawOnly) {

			this.drawOnly = drawOnly;

		}

	}

	public class ResourceBase {
		
		public enum ControlType : byte {
			
			None = 0x0,
			Show = 0x1,
			Hide = 0x2,
			Init = 0x4,
			Deinit = 0x8,
			
		};

		public enum Platform : byte {

			Standalone,
			iOS,
			Android,
			PS4,
			XBOXONE,

		};

		public ResourceBase() {}
		
		public ResourceBase(ResourceBase source) {

			this.async = source.async;
			this.controlType = source.controlType;
			this.assetPath = source.assetPath;
			this.resourcesPath = source.resourcesPath;
			this.loadableResource = source.loadableResource;
			this.loadableStream = source.loadableStream;
			this.multiObjects = source.multiObjects;
			this.objectIndex = source.objectIndex;
			this.customResourcePath = source.customResourcePath;
			this.id = source.id;
			
			this.loadedObject = source.loadedObject;
			this.loadedObjectId = source.loadedObjectId;
			this.loaded = source.loaded;
			
			this.streamingAssetsPathStandalone = source.streamingAssetsPathStandalone;
			this.streamingAssetsPathIOS = source.streamingAssetsPathIOS;
			this.streamingAssetsPathAndroid = source.streamingAssetsPathAndroid;
			this.streamingAssetsPathPS4 = source.streamingAssetsPathPS4;
			this.streamingAssetsPathXBOXONE = source.streamingAssetsPathXBOXONE;

		}

		[ReadOnly("loadableStream", state: true)]
		public bool @async = true;

		[BitMask(typeof(ControlType))]
		public ControlType controlType = ControlType.None;

		[ReadOnly] public string assetPath;
		[ReadOnly] public string resourcesPath;
		[ReadOnly] public bool loadableResource = false;
		[ReadOnly] public bool loadableStream = false;
		
		[ReadOnly] public bool multiObjects;
		[ReadOnly] public int objectIndex;
		
		[ReadOnly] public string customResourcePath;

		[ReadOnly] public long id;

		[System.NonSerialized]
		public Object loadedObject;
		[System.NonSerialized]
		public int loadedObjectId;
		[System.NonSerialized]
		public bool loaded;

		[ReadOnly] public string streamingAssetsPathStandalone;
		[ReadOnly] public string streamingAssetsPathIOS;
		[ReadOnly] public string streamingAssetsPathAndroid;
		[ReadOnly] public string streamingAssetsPathPS4;
		[ReadOnly] public string streamingAssetsPathXBOXONE;

		private static Dictionary<Graphic, int> iterations = null;
		private static Dictionary<Graphic, Color> colorCache = null;

		public long GetId() {

			return this.id;

		}

		public void Unload(Object item) {

			Resources.UnloadAsset(item);

		}

		public void Unload() {

			var obj = this.loadedObject;
			this.loadedObject = null;
			this.loadedObjectId = 0;
			this.loaded = false;
			if (obj != null) Resources.UnloadAsset(obj);

		}

		public T Load<T>() where T : Object {

			if (this.loadedObject != null) {

				return this.loadedObject as T;

			}

			#region Load Resource
			if (this.loadableResource == true) {

				this.loadedObject = Resources.Load<T>(this.resourcesPath);
				if (this.loadedObject != null) {
					
					this.loadedObjectId = this.loadedObject.GetInstanceID();
					this.loaded = true;

					return this.loadedObject as T;

				}

			}
			#endregion

			return null;

		}

		public IEnumerator Load<T>(IImageComponent component, Graphic graphic, string customResourcePath, System.Action<T> callback) where T : Object {

			if (ResourceBase.iterations == null) ResourceBase.iterations = new Dictionary<Graphic, int>();
			if (ResourceBase.colorCache == null) ResourceBase.colorCache = new Dictionary<Graphic, Color>();

			customResourcePath = customResourcePath ?? (string.IsNullOrEmpty(this.customResourcePath) == true ? null : this.customResourcePath);
			this.customResourcePath = customResourcePath;

			var isFade = (WindowSystemResources.GetAsyncLoadFadeTime() > 0f);

			var iterationFailed = false;
			var iteration = 0;
			var oldColor = Color.white;
			if (graphic != null) {

				var isEmpty = true;
				if (graphic is Image) {

					isEmpty = ((graphic as Image).sprite == null);

				}

				if (isEmpty == true && graphic is RawImage) {

					isEmpty = ((graphic as RawImage).texture == null);

				}

				if (isFade == true) {

					TweenerGlobal.instance.removeTweens(graphic);

				}

				if (ResourceBase.iterations.TryGetValue(graphic, out iteration) == false) {

					ResourceBase.iterations.Add(graphic, iteration);

				}

				if (ResourceBase.colorCache.TryGetValue(graphic, out oldColor) == true) {

					// restoring color
					graphic.color = oldColor;

				} else {

					ResourceBase.colorCache.Add(graphic, graphic.color);

				}

				oldColor = graphic.color;
				if (isEmpty == true) graphic.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0f);

				++ResourceBase.iterations[graphic];
				iteration = ResourceBase.iterations[graphic];

				//Debug.Log("Loading: " + customResourcePath + ", iter: " + iteration, graphic);

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

				var task = MovieSystem.LoadTexture(component);
				while (task.isDone == false) {
						
					yield return false;

				}

				iterationFailed = !(graphic == null || iteration == ResourceBase.iterations[graphic]);
				if (iterationFailed == false) {

					//Debug.Log("Loaded: " + customResourcePath + ", iter: " + iteration, graphic);

					callback.Invoke(task.asset as T);

				}

				task.Dispose();
				task = null;

			}
			#endregion

			if (iterationFailed == false && graphic != null) {

				if (isFade == true) {

					TweenerGlobal.instance.addTweenAlpha(graphic, WindowSystemResources.GetAsyncLoadFadeTime(), oldColor.a).tag(graphic).onCancel((g) => { g.color = oldColor; });

				} else {

					graphic.color = oldColor;

				}

			}

		}

		public string GetStreamPath() {

			var combine = true;
			var path = string.Empty;
			#if UNITY_STANDALONE || UNITY_EDITOR
			path = this.streamingAssetsPathStandalone;
			#elif UNITY_IPHONE
			path = "Data/Raw/" + this.streamingAssetsPathIOS;
			combine = false;
			#elif UNITY_ANDROID
			path = this.streamingAssetsPathAndroid;
			#elif UNITY_PS4
			path = this.streamingAssetsPathPS4;
			#elif UNITY_XBOXONE
			path = this.streamingAssetsPathXBOXONE;
			#else
			path = this.streamingAssetsPathStandalone;
			#endif

			if (combine == true) {
				
				return System.IO.Path.Combine(Application.streamingAssetsPath, path);
				
			} else {
				
				return path;
				
			}

		}

		#if UNITY_EDITOR
		public virtual void Reset() {
			
			this.customResourcePath = null;
			this.resourcesPath = null;
			this.loadableResource = false;
			this.loadableStream = false;
			
		}
		
		public virtual void Validate() {
			
		}
		
		public virtual void Validate(Object item) {
			
			if (item == null) {
				
				return;
				
			}
			
			this.assetPath = UnityEditor.AssetDatabase.GetAssetPath(item);

			#region Resources
			{

				var resourcePath = (this.assetPath.Contains("/Resources/") == true ? this.assetPath.Split(new string[] { "/Resources/" }, System.StringSplitOptions.None)[1] : string.Empty);
				var ext = System.IO.Path.GetExtension(resourcePath);
				this.resourcesPath = resourcePath.Substring(0, resourcePath.Length - ext.Length);
				
				this.loadableResource = (string.IsNullOrEmpty(this.resourcesPath) == false);

			}
			#endregion

			#region StreamingAssets
			{

				var streamingAssetsPath = (this.assetPath.Contains("/StreamingAssets/") == true ? this.assetPath.Split(new string[] { "/StreamingAssets/" }, System.StringSplitOptions.None)[1] : string.Empty);
				var platformSplit = streamingAssetsPath.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);

				if (platformSplit.Length > 0) {

					var localPath = string.Join("/", platformSplit, 1, platformSplit.Length - 1);
					var localDir = System.IO.Path.GetDirectoryName(localPath);
					var filenameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(streamingAssetsPath);
					
					this.streamingAssetsPathIOS			= this.GetPlatformDirectory(Platform.iOS, localDir, filenameWithoutExt);
					this.streamingAssetsPathAndroid		= this.GetPlatformDirectory(Platform.Android, localDir, filenameWithoutExt);
					this.streamingAssetsPathStandalone	= this.GetPlatformDirectory(Platform.Standalone, localDir, filenameWithoutExt);
					this.streamingAssetsPathPS4			= this.GetPlatformDirectory(Platform.PS4, localDir, filenameWithoutExt);
					this.streamingAssetsPathXBOXONE		= this.GetPlatformDirectory(Platform.XBOXONE, localDir, filenameWithoutExt);

				}
				
				this.loadableStream = (string.IsNullOrEmpty(streamingAssetsPath) == false);

			}
			#endregion

			this.id = item.GetInstanceID();

		}

		private string GetPlatformDirectory(Platform platform, string localDir, string filenameWithoutExt) {

			var platformDir = platform.ToString() + "/";

			var sPath = System.IO.Path.Combine(Application.streamingAssetsPath, platformDir + localDir);
			if (System.IO.Directory.Exists(sPath) == true) {

				var files = System.IO.Directory.GetFiles(sPath);
				foreach (var file in files) {

					var ext = System.IO.Path.GetExtension(file).ToLower();
					if (ext == ".meta") continue;

					var curFilename = System.IO.Path.GetFileNameWithoutExtension(file);
					if (filenameWithoutExt == curFilename) {

						ext = System.IO.Path.GetExtension(file);
						return System.IO.Path.Combine(platformDir + localDir, filenameWithoutExt + ext);

					}

				}

			}

			return string.Empty;

		}
		#endif
		
	};

}