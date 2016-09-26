using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using ME;

namespace UnityEngine.UI.Windows {

	public interface IUnloadableItem {

		void Unload();

	};

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

			Common,
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
			this.streamingAssetsPathCommon = source.streamingAssetsPathCommon;

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

		[ReadOnly] public int id;

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
		[ReadOnly] public string streamingAssetsPathCommon;

		[ReadOnly] public string streamingAssetsPathStandaloneMovieAudio;
		[ReadOnly] public string streamingAssetsPathIOSMovieAudio;
		[ReadOnly] public string streamingAssetsPathAndroidMovieAudio;
		[ReadOnly] public string streamingAssetsPathPS4MovieAudio;
		[ReadOnly] public string streamingAssetsPathXBOXONEMovieAudio;
		[ReadOnly] public string streamingAssetsPathCommonMovieAudio;

		protected virtual bool canBeUnloaded {

			get {

				return true;

			}

		}

		//private static Dictionary<Graphic, int> iterations = null;
		//private static Dictionary<Graphic, Color> colorCache = null;

		//private ResourceAsyncOperation task;

		public bool IsValid() {

			return 
				this.loadableResource == true ||
				this.loadableStream == true;

		}

		public int GetId() {

			return this.id;

		}

		public void Unload(Object item) {

			if (item != null) {

				if (this.canBeUnloaded == true) {

					Resources.UnloadAsset(item);

				} else {

					//Resources.UnloadUnusedAssets();

				}

			}

		}

		public void Unload() {

			var obj = this.loadedObject;
			this.loadedObject = null;
			this.loadedObjectId = 0;
			this.loaded = false;

			if (this.loadableResource == true) {

				this.Unload(obj);

			}

		}

		public T Load<T>() where T : Object {

			if (this.loadedObject != null) {

				return this.loadedObject as T;

			}

			#region Load Resource
			if (this.loadableResource == true) {

				this.loadedObject = Resources.Load<T>(this.resourcesPath);
				if (this.loadedObject != null) {

					this.loadedObjectId = this.loadedObject.GetID();
					this.loaded = true;

					return this.loadedObject as T;

				}

			}
			#endregion

			return null;

		}

		public IEnumerator LoadAudioClip(System.Action<AudioClip> callback) {

			#region Load Resource
			if (this.loadableResource == true) {

				if (this.async == true) {

					var request = Resources.LoadAsync<AudioClip>(this.resourcesPath);
					while (request.isDone == false) {

						yield return false;

					}

					callback.Invoke(request.asset as AudioClip);

				} else {

					callback.Invoke(Resources.Load<AudioClip>(this.resourcesPath));

				}

				yield break;

			}
			#endregion

			#region Load Stream
			if (this.loadableStream == true) {

				var path = this.GetStreamPath(withFile: true);
				//Debug.Log("Loading: " + path);
				var www = new WWW(path);
				while (www.isDone == false) {

					yield return false;

				}

				if (string.IsNullOrEmpty(www.error) == true) {
					
					var clip = www.audioClip;
					//Debug.Log("Callback!");
					callback.Invoke(clip);

				} else {

					//Debug.Log("Callback!");
					callback.Invoke(null);

				}

				www.Dispose();
				www = null;

				yield break;

			}
			#endregion

			callback.Invoke(null);

		}

		public bool IsMaterialLoadingType() {

			if (this.loadableStream == true) {

				return MovieSystem.IsMaterialLoadingType();

			}

			return false;

		}

		public IEnumerator Load<T>(IImageComponent component, Graphic graphic, string customResourcePath, System.Action<T> callback) where T : Object {

			yield return WindowSystemResources.LoadResource<T>(this, component, graphic, customResourcePath, callback);

		}

		public bool HasMovieAudio() {
			
			return string.IsNullOrEmpty(this.GetMovieAudioStreamPath());

		}
		
		public string GetMovieAudioStreamPath(bool withFile = false) {

			var combine = true;
			var result = string.Empty;
			var prefix = string.Empty;
			var prefixPath = string.Empty;
			var path = string.Empty;
			#if UNITY_STANDALONE || UNITY_EDITOR
			path = this.streamingAssetsPathStandaloneMovieAudio;
			#elif UNITY_IPHONE || UNITY_TVOS
			path = this.streamingAssetsPathIOSMovieAudio;
			combine = true;
			if (withFile == true) {

				prefixPath = "Data/Raw/";

			}
			#elif UNITY_ANDROID
			path = this.streamingAssetsPathAndroidMovieAudio;
			#elif UNITY_PS4
			path = this.streamingAssetsPathPS4MovieAudio;
			prefix = "streamingAssets/";
			#elif UNITY_XBOXONE
			path = this.streamingAssetsPathXBOXONEMovieAudio;
			#else
			path = this.streamingAssetsPathStandalone;
			if (path.Contains("://") == false) {
				
				prefix = "file:///";
				
			}
			#endif

			if (string.IsNullOrEmpty(path) == true) {

				path = this.streamingAssetsPathCommonMovieAudio;

			}

			if (string.IsNullOrEmpty(path) == false) {

				path = path.Replace("\\", "/");

				if (combine == true) {

					result = prefix + System.IO.Path.Combine(Application.streamingAssetsPath, prefixPath + path);

				} else {

					result = prefix + prefixPath + path;

				}

			}

			if (withFile == true && string.IsNullOrEmpty(result) == false) {

				if (result.Contains("://") == false) {

					result = "file:///" + result;

				}

			}

			return result;

		}

		public string GetStreamPath(bool withFile = false) {

			var combine = true;
			var path = string.Empty;
			var prefix = string.Empty;
			var prefixPath = string.Empty;
			var result = string.Empty;
			#if UNITY_STANDALONE || UNITY_EDITOR
			path = this.streamingAssetsPathStandalone;
			/*if (path.Contains("://") == false) {

				prefix = "file:///";

			}*/
			#elif UNITY_IPHONE || UNITY_TVOS
			path = this.streamingAssetsPathIOS;
			combine = true;
			if (withFile == true) {
				
				prefixPath = "Data/Raw/";

			}
			#elif UNITY_ANDROID
			path = this.streamingAssetsPathAndroid;
			#elif UNITY_PS4
			path = this.streamingAssetsPathPS4;
			combine = false;
			prefix = "streamingAssets/";
			#elif UNITY_XBOXONE
			path = this.streamingAssetsPathXBOXONE;
			#else
			path = this.streamingAssetsPathStandalone;
			if (path.Contains("://") == false) {

				prefix = "file:///";

			}
			#endif

			if (string.IsNullOrEmpty(path) == true) {
				
				path = this.streamingAssetsPathCommon;

			}

			if (string.IsNullOrEmpty(path) == false) {

				path = path.Replace("\\", "/");
				
				if (combine == true) {
					
					result = prefix + System.IO.Path.Combine(Application.streamingAssetsPath, prefixPath + path);
					
				} else {
					
					result = prefix + prefixPath + path;
					
				}
				
			}

			if (withFile == true && string.IsNullOrEmpty(result) == false) {
				
				if (result.Contains("://") == false) {
					
					result = "file:///" + result;
					
				}

			}

			return result;

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
					this.streamingAssetsPathCommon		= this.GetPlatformDirectory(Platform.Common, localDir, filenameWithoutExt);

					this.streamingAssetsPathIOSMovieAudio			= this.GetMovieAudio(this.streamingAssetsPathIOS);
					this.streamingAssetsPathAndroidMovieAudio		= this.GetMovieAudio(this.streamingAssetsPathAndroid);
					this.streamingAssetsPathStandaloneMovieAudio	= this.GetMovieAudio(this.streamingAssetsPathStandalone);
					this.streamingAssetsPathPS4MovieAudio			= this.GetMovieAudio(this.streamingAssetsPathPS4);
					this.streamingAssetsPathXBOXONEMovieAudio		= this.GetMovieAudio(this.streamingAssetsPathXBOXONE);
					this.streamingAssetsPathCommonMovieAudio		= this.GetMovieAudio(this.streamingAssetsPathCommon);
					/*
					if (string.IsNullOrEmpty(this.streamingAssetsPathIOSMovieAudio) == true) this.streamingAssetsPathIOSMovieAudio = this.streamingAssetsPathCommonMovieAudio;
					if (string.IsNullOrEmpty(this.streamingAssetsPathAndroidMovieAudio) == true) this.streamingAssetsPathAndroidMovieAudio = this.streamingAssetsPathCommonMovieAudio;
					if (string.IsNullOrEmpty(this.streamingAssetsPathStandaloneMovieAudio) == true) this.streamingAssetsPathStandaloneMovieAudio = this.streamingAssetsPathCommonMovieAudio;
					if (string.IsNullOrEmpty(this.streamingAssetsPathPS4MovieAudio) == true) this.streamingAssetsPathPS4MovieAudio = this.streamingAssetsPathCommonMovieAudio;
					if (string.IsNullOrEmpty(this.streamingAssetsPathXBOXONEMovieAudio) == true) this.streamingAssetsPathXBOXONEMovieAudio = this.streamingAssetsPathCommonMovieAudio;
			*/
				}
				
				this.loadableStream = (string.IsNullOrEmpty(streamingAssetsPath) == false);

			}
			#endregion

			string uniquePath = (this.multiObjects == true) ? (string.Format("{0}#{1}", this.assetPath, this.objectIndex)) : this.assetPath;
			this.id = ResourceBase.GetJavaHash(uniquePath);

		}

        private static int GetJavaHash(string s) {

            int hash = 0;
            foreach (char c in s) {

                hash = 31 * hash + c;

            }
            return hash;

        }

		private string GetMovieAudio(string streamingPath) {

			if (string.IsNullOrEmpty(streamingPath) == true) {
				
				return string.Empty;
				
			}

			var path = System.IO.Path.Combine(Application.streamingAssetsPath, System.IO.Path.GetDirectoryName(streamingPath));
			if (System.IO.Directory.Exists(path) == true) {

				var filename = System.IO.Path.GetFileNameWithoutExtension(streamingPath);
				var filenameWithExt = System.IO.Path.GetFileName(streamingPath);
				var files = System.IO.Directory.GetFiles(path);
				var found = string.Empty;
				foreach (var file in files) {
					
					if (filenameWithExt == System.IO.Path.GetFileName(file)) continue;

					var curFilename = System.IO.Path.GetFileNameWithoutExtension(file);
					if (curFilename == filename) {

						found = System.IO.Path.GetFileName(file);
						break;

					}

				}

				if (string.IsNullOrEmpty(found) == false) {
					
					return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(streamingPath), found);

				}

			}

			return string.Empty;

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
						var combinedPath = System.IO.Path.Combine(platformDir + localDir, filenameWithoutExt + ext);
						combinedPath = combinedPath.Replace('\\', '/');
						return combinedPath;

					}

				}

			}

			return string.Empty;

		}
		#endif
		
	};

}