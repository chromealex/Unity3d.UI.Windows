using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Plugins.Resources;

namespace UnityEngine.UI.Windows {

	public interface IUnloadableItem {

		void Unload();

	};

	public class ResourceBase {
		
		public enum Platform : byte {

			Common,
			Standalone,
			iOS,
			Android,
			PS4,
			XBOXONE,

		};

		public ResourceBase() {}

		public ResourceBase(bool async) {

			this.async = async;

		}

		[ReadOnly("loadableStream", state: true)]
		public bool async = true;

		//#if UNITY_EDITOR
		//[BundleIgnore]
		[ReadOnly] public string assetPath;
		//#endif

		[ReadOnly] public string assetBundleName;
		[ReadOnly] public string resourcesPath;
		[ReadOnly] public string webPath;

		[ReadOnly] public bool loadableWeb;
		[ReadOnly] public bool loadableResource;
		[ReadOnly] public bool loadableStream;
		[ReadOnly] public bool loadableAssetBundle;
		
		[ReadOnly] public bool multiObjects;
		[ReadOnly] public int objectIndex;

		[ReadOnly] public bool multiObjectsAssetBundle;
		[ReadOnly] public int objectIndexAssetBundle;
		
		[ReadOnly] public string customResourcePath;

		[ReadOnly] public int id;

		[ReadOnly] public int cacheVersion;

		public Object loadedObject {

			get {

				return WindowSystemResources.GetResourceObjectById(this);

			}

		}

		public bool loaded {

			get {

				return WindowSystemResources.IsResourceObjectLoaded(this);

			}

		}

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

		[ReadOnly] public bool streamingAssetsPathStandaloneIsMovie;
		[ReadOnly] public bool streamingAssetsPathIOSIsMovie;
		[ReadOnly] public bool streamingAssetsPathAndroidIsMovie;
		[ReadOnly] public bool streamingAssetsPathPS4IsMovie;
		[ReadOnly] public bool streamingAssetsPathXBOXONEIsMovie;
		[ReadOnly] public bool streamingAssetsPathCommonIsMovie;

		[ReadOnly] public bool canBeUnloaded;

		//private static Dictionary<Graphic, int> iterations = null;
		//private static Dictionary<Graphic, Color> colorCache = null;

		//private ResourceAsyncOperation task;

		public bool IsValid() {

			return 
				this.loadableResource == true ||
				this.loadableStream == true ||
				this.loadableWeb == true ||
				this.loadableAssetBundle == true;

		}

		public int GetId() {

			return this.id;

		}

		private void UnloadObject_INTERNAL(Object item) {

			if (item == null) return;

			if (this.loadableResource == true) {

				if (this.canBeUnloaded == true) {

					Resources.UnloadAsset(item);

				}

			} else if (this.loadableStream == true) {

			} else if (this.loadableAssetBundle == true) {

				Object.DestroyImmediate(item, true);

			}

		}

		public void Unload() {
			
			if (this.IsLoadable() == true) {

				this.UnloadObject_INTERNAL(this.loadedObject);

			}

		}

		/*public T Load<T>() where T : Object {

			if (this.loadedObject != null) {

				return this.loadedObject as T;

			}

			#region Load Resource
			if (this.loadableResource == true) {

				this.loadedObject = Resources.Load<T>(this.resourcesPath);
				if (this.loadedObject != null) {

					return this.loadedObject as T;

				}

			}
			#endregion

			return null;

		}*/

		public bool IsLoadable() {

			return
				this.loadableAssetBundle == true ||
				this.loadableResource == true ||
				this.loadableStream == true ||
				this.loadableWeb == true;

		}

		public System.Collections.Generic.IEnumerator<byte> LoadAudioClip(System.Action<AudioClip> callback) {

			#region Load Resource
			if (this.loadableResource == true) {

				if (this.async == true) {

					var request = Resources.LoadAsync<AudioClip>(this.resourcesPath);
					while (request.isDone == false) {

						yield return 0;

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

					yield return 0;

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

		public System.Collections.Generic.IEnumerator<byte> Load<T>(IResourceReference component, string customResourcePath, System.Action<T> callback, bool async) where T : Object {
			
			return WindowSystemResources.LoadResource<T>(this, component, customResourcePath, callback, async);

		}

		public bool HasMovieAudio() {
			
			return string.IsNullOrEmpty(this.GetMovieAudioStreamPath());

		}
		
		public string GetAssetBundleRelativePath() {

			return this.assetBundleName;

		}

		public string GetAssetBundlePath(bool withFile) {

			var result = this.GetAssetBundleRelativePath();
			var settings = WindowSystemResources.GetSettings();
			var isRemoteBundle = (settings != null && settings.loadFromStreamingAssets == false && string.IsNullOrEmpty(settings.url) == false);

			var platformPath = string.Empty;
#if UNITY_EDITOR
			platformPath = UnityEngine.UI.Windows.WindowSystemResources.ALL_TARGETS[UnityEditor.EditorUserBuildSettings.activeBuildTarget] + "/";
#elif UNITY_ANDROID
			platformPath = "Android/";
			#elif UNITY_IOS || UNITY_TVOS
			platformPath = "iOS/";
#elif UNITY_PS4
			platformPath = "PS4/";
#elif UNITY_XBOXONE
			platformPath = "XboxOne/";
#elif UNITY_STANDALONE_LINUX
			platformPath = "Linux/";
#elif UNITY_STANDALONE_OSX
			platformPath = "Mac/";
#elif UNITY_STANDALONE_WIN
			platformPath = "Windows/";
#endif

			platformPath = string.Format("0.6.6b/{0}", platformPath);

			var path = (isRemoteBundle == true ? settings.url + platformPath : Application.streamingAssetsPath + "/" + platformPath);
			return ((withFile == true && isRemoteBundle == false) ? "file:///" : string.Empty) + path + result;

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
			/*if (withFile == true) {

				prefixPath = "Data/Raw/";

			}*/
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
		
		public bool IsMovie() {

			var result = false;
			var path = string.Empty;
			#if UNITY_STANDALONE || UNITY_EDITOR
			result = this.streamingAssetsPathStandaloneIsMovie;
			path = this.streamingAssetsPathStandalone;
			#elif UNITY_IPHONE || UNITY_TVOS
			result = this.streamingAssetsPathIOSIsMovie;
			path = this.streamingAssetsPathIOS;
			#elif UNITY_ANDROID
			result = this.streamingAssetsPathAndroidIsMovie;
			path = this.streamingAssetsPathAndroid;
			#elif UNITY_PS4
			result = this.streamingAssetsPathPS4IsMovie;
			path = this.streamingAssetsPathPS4;
			#elif UNITY_XBOXONE
			result = this.streamingAssetsPathXBOXONEIsMovie;
			path = this.streamingAssetsPathXBOXONE;
			#else
			result = this.streamingAssetsPathCommonIsMovie;
			path = this.streamingAssetsPathCommon;
			#endif

			if (string.IsNullOrEmpty(path) == true) {
				
				result = this.streamingAssetsPathCommonIsMovie;
				
			}

			return result;

		}

		public bool IsMovie(string streamingPath) {

			var fileExt = System.IO.Path.GetExtension(streamingPath).Trim('.').ToLower();
			return (fileExt == "avi" || fileExt == "mp4" || fileExt == "ogv" || fileExt == "mov");

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
			/*if (withFile == true) {
				
				prefixPath = "Data/Raw/";

			}*/
			#elif UNITY_ANDROID
			path = this.streamingAssetsPathAndroid;
			#elif UNITY_PS4
			path = this.streamingAssetsPathPS4;
			combine = false;
			prefix = "streamingAssets/";
			#elif UNITY_XBOXONE
			path = this.streamingAssetsPathXBOXONE;
			#else
			path = this.streamingAssetsPathCommon;
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

		protected static int GetJavaHash(string s) {

			int hash = 0;
			foreach (char c in s) {

				hash = 31 * hash + c;

			}

			return hash;

		}

		#if UNITY_EDITOR
		public virtual void ResetToDefault() {

			this.assetPath = null;
			this.customResourcePath = null;
			this.resourcesPath = null;
			this.assetBundleName = null;

			this.objectIndex = 0;
			this.objectIndexAssetBundle = 0;
			this.id = 0;

			this.multiObjectsAssetBundle = false;
			this.loadableResource = false;
			this.loadableStream = false;
			this.loadableAssetBundle = false;
			this.loadableWeb = false;

		}
		
		public static string GetResourcePathFromAssetPath(string assetPath) {

			if (assetPath.Contains("/Resources/") == true) {
				
				var splitted = assetPath.Split(new string[] { "/Resources/" }, System.StringSplitOptions.None);
				var joined = new List<string>();
				for (int i = 1; i < splitted.Length; ++i) {

					joined.Add(splitted[i]);
					if (i < splitted.Length - 1) joined.Add("Resources");

				}
				
				var resourcePath = string.Join("/", joined.ToArray());
				var ext = System.IO.Path.GetExtension(resourcePath);
				return resourcePath.Substring(0, resourcePath.Length - ext.Length);
				
			}

			return string.Empty;

		}

		public virtual void Validate() {
			
		}
		
		public virtual void Validate(Object item) {
			
			if (item == null) {
				
				return;
				
			}

			ME.EditorUtilities.SetValueIfDirty(ref this.assetPath, UnityEditor.AssetDatabase.GetAssetPath(item));

			#region Resources
			{

				var resourcePath = (this.assetPath.Contains("/Resources/") == true ? this.assetPath.Split(new string[] { "/Resources/" }, System.StringSplitOptions.None)[1] : string.Empty);
				var ext = System.IO.Path.GetExtension(resourcePath);
				ME.EditorUtilities.SetValueIfDirty(ref this.resourcesPath, resourcePath.Substring(0, resourcePath.Length - ext.Length));
				
				ME.EditorUtilities.SetValueIfDirty(ref this.loadableResource, (string.IsNullOrEmpty(this.resourcesPath) == false));

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
					
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathIOS, this.GetPlatformDirectory(Platform.iOS, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathAndroid, this.GetPlatformDirectory(Platform.Android, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandalone, this.GetPlatformDirectory(Platform.Standalone, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathPS4, this.GetPlatformDirectory(Platform.PS4, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathXBOXONE, this.GetPlatformDirectory(Platform.XBOXONE, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathCommon, this.GetPlatformDirectory(Platform.Common, localDir, filenameWithoutExt));
					
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathIOSMovieAudio, this.GetMovieAudio(this.streamingAssetsPathIOS));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathAndroidMovieAudio, this.GetMovieAudio(this.streamingAssetsPathAndroid));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneMovieAudio, this.GetMovieAudio(this.streamingAssetsPathStandalone));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathPS4MovieAudio, this.GetMovieAudio(this.streamingAssetsPathPS4));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathXBOXONEMovieAudio, this.GetMovieAudio(this.streamingAssetsPathXBOXONE));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathCommonMovieAudio, this.GetMovieAudio(this.streamingAssetsPathCommon));
					
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathIOSIsMovie, this.IsMovie(this.streamingAssetsPathIOS));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathAndroidIsMovie, this.IsMovie(this.streamingAssetsPathAndroid));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneIsMovie, this.IsMovie(this.streamingAssetsPathStandalone));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathPS4IsMovie, this.IsMovie(this.streamingAssetsPathPS4));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathXBOXONEIsMovie, this.IsMovie(this.streamingAssetsPathXBOXONE));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathCommonIsMovie, this.IsMovie(this.streamingAssetsPathCommon));
					
				}
				
				ME.EditorUtilities.SetValueIfDirty(ref this.loadableStream, (string.IsNullOrEmpty(streamingAssetsPath) == false));

			}
			#endregion

			#region AssetBundles
			var importer = UnityEditor.AssetImporter.GetAtPath(this.assetPath);
			ME.EditorUtilities.SetValueIfDirty(ref this.loadableAssetBundle, importer != null && string.IsNullOrEmpty(importer.assetBundleName) == false);
			if (this.loadableAssetBundle == true) {
				
				ME.EditorUtilities.SetValueIfDirty(ref this.assetBundleName, importer.assetBundleName);
				ME.EditorUtilities.SetValueIfDirty(ref this.loadableResource, false);
				ME.EditorUtilities.SetValueIfDirty(ref this.loadableStream, false);
				ME.EditorUtilities.SetValueIfDirty(ref this.multiObjectsAssetBundle, true);
				
				WindowSystemAssetBundlesMap map = null;
				var objects = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject");
				for (int i = 0, size = objects.Length; i < size; ++i) {
					
					var path = UnityEditor.AssetDatabase.GUIDToAssetPath(objects[i]);
					if (path.Contains("WindowSystemAssetBundlesMap")) {
						
						map = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(WindowSystemAssetBundlesMap)) as WindowSystemAssetBundlesMap;
						break;
						
					}
					
				}
				
				if (map == null) {
					
					Debug.LogError("Missing WindowSystemAssetBundlesMap!");
					
				} else {
					
					ME.EditorUtilities.SetValueIfDirty(ref this.objectIndexAssetBundle, map.GetIndex(this.assetBundleName, this.assetPath));
					
				}
				
			}
			#endregion

			string uniquePath = (this.multiObjects == true) ? (string.Format("{0}#{1}", this.assetPath, this.objectIndex)) : this.assetPath;
			ME.EditorUtilities.SetValueIfDirty(ref this.id, ResourceBase.GetJavaHash(uniquePath));

			ME.EditorUtilities.SetValueIfDirty(ref this.canBeUnloaded, ((item is GameObject) == false && (item is Component) == false));

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