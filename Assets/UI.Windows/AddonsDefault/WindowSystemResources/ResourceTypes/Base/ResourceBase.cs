using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Plugins.Resources;
using System.Linq;

namespace UnityEngine.UI.Windows {

	public interface IUnloadableItem {

		void Unload();

	};

	public class ResourceBase {
		
		public enum Platform : byte {

			Common,
			Standalone,
			StandaloneWindows,
			StandaloneLinux,
			StandaloneOSX,
			iOS,
			Android,
			PS4,
			XBOXONE,
			Switch,

		};

		public ResourceBase() {}

		public ResourceBase(bool async) {

			this.async = async;

		}

		public ResourceBase(ResourceBase other) {

			this.async = other.async;

			this.assetPath = other.assetPath;
			this.assetBundleName = other.assetBundleName;
			this.resourcesPath = other.resourcesPath;
			this.webPath = other.webPath;

			this.loadableWeb = other.loadableWeb;
			this.loadableResource = other.loadableResource;
			this.loadableStream = other.loadableStream;
			this.loadableAssetBundle = other.loadableAssetBundle;

			this.multiObjects = other.multiObjects;
			this.objectIndex = other.objectIndex;

			this.multiObjectsAssetBundle = other.multiObjectsAssetBundle;
			this.objectIndexAssetBundle = other.objectIndexAssetBundle;

			this.customResourcePath = other.customResourcePath;
			this.readableTexture = other.readableTexture;
			this.id = other.id;
			this.cacheVersion = other.cacheVersion;

			this.directRef = other.directRef;

			this.streamingAssetsPathStandalone = other.streamingAssetsPathStandalone;
			this.streamingAssetsPathStandaloneWindows = other.streamingAssetsPathStandaloneWindows;
			this.streamingAssetsPathStandaloneLinux = other.streamingAssetsPathStandaloneLinux;
			this.streamingAssetsPathStandaloneOSX = other.streamingAssetsPathStandaloneOSX;
			this.streamingAssetsPathIOS = other.streamingAssetsPathIOS;
			this.streamingAssetsPathAndroid = other.streamingAssetsPathAndroid;
			this.streamingAssetsPathPS4 = other.streamingAssetsPathPS4;
			this.streamingAssetsPathXBOXONE = other.streamingAssetsPathXBOXONE;
			this.streamingAssetsPathSwitch = other.streamingAssetsPathSwitch;
			this.streamingAssetsPathCommon = other.streamingAssetsPathCommon;

			this.streamingAssetsPathStandaloneMovieAudio = other.streamingAssetsPathStandaloneMovieAudio;
			this.streamingAssetsPathStandaloneWindowsMovieAudio = other.streamingAssetsPathStandaloneWindowsMovieAudio;
			this.streamingAssetsPathStandaloneLinuxMovieAudio = other.streamingAssetsPathStandaloneLinuxMovieAudio;
			this.streamingAssetsPathStandaloneOSXMovieAudio = other.streamingAssetsPathStandaloneOSXMovieAudio;
			this.streamingAssetsPathIOSMovieAudio = other.streamingAssetsPathIOSMovieAudio;
			this.streamingAssetsPathAndroidMovieAudio = other.streamingAssetsPathAndroidMovieAudio;
			this.streamingAssetsPathPS4MovieAudio = other.streamingAssetsPathPS4MovieAudio;
			this.streamingAssetsPathXBOXONEMovieAudio = other.streamingAssetsPathXBOXONEMovieAudio;
			this.streamingAssetsPathSwitchMovieAudio = other.streamingAssetsPathSwitchMovieAudio;
			this.streamingAssetsPathCommonMovieAudio = other.streamingAssetsPathCommonMovieAudio;

			this.streamingAssetsPathStandaloneIsMovie = other.streamingAssetsPathStandaloneIsMovie;
			this.streamingAssetsPathStandaloneWindowsIsMovie = other.streamingAssetsPathStandaloneWindowsIsMovie;
			this.streamingAssetsPathStandaloneLinuxIsMovie = other.streamingAssetsPathStandaloneLinuxIsMovie;
			this.streamingAssetsPathStandaloneOSXIsMovie = other.streamingAssetsPathStandaloneOSXIsMovie;
			this.streamingAssetsPathIOSIsMovie = other.streamingAssetsPathIOSIsMovie;
			this.streamingAssetsPathAndroidIsMovie = other.streamingAssetsPathAndroidIsMovie;
			this.streamingAssetsPathPS4IsMovie = other.streamingAssetsPathPS4IsMovie;
			this.streamingAssetsPathXBOXONEIsMovie = other.streamingAssetsPathXBOXONEIsMovie;
			this.streamingAssetsPathSwitchIsMovie = other.streamingAssetsPathSwitchIsMovie;
			this.streamingAssetsPathCommonIsMovie = other.streamingAssetsPathCommonIsMovie;

			this.canBeUnloaded = other.canBeUnloaded;

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

		[ReadOnly] public bool readableTexture;

		[ReadOnly] public int id;

		[ReadOnly] public int cacheVersion;

		[ReadOnly] public string streamingAssetsPathStandalone;
		[ReadOnly] public string streamingAssetsPathStandaloneWindows;
		[ReadOnly] public string streamingAssetsPathStandaloneLinux;
		[ReadOnly] public string streamingAssetsPathStandaloneOSX;
		[ReadOnly] public string streamingAssetsPathIOS;
		[ReadOnly] public string streamingAssetsPathAndroid;
		[ReadOnly] public string streamingAssetsPathPS4;
		[ReadOnly] public string streamingAssetsPathXBOXONE;
		[ReadOnly] public string streamingAssetsPathSwitch;
		[ReadOnly] public string streamingAssetsPathCommon;

		[ReadOnly] public string streamingAssetsPathStandaloneMovieAudio;
		[ReadOnly] public string streamingAssetsPathStandaloneWindowsMovieAudio;
		[ReadOnly] public string streamingAssetsPathStandaloneLinuxMovieAudio;
		[ReadOnly] public string streamingAssetsPathStandaloneOSXMovieAudio;
		[ReadOnly] public string streamingAssetsPathIOSMovieAudio;
		[ReadOnly] public string streamingAssetsPathAndroidMovieAudio;
		[ReadOnly] public string streamingAssetsPathPS4MovieAudio;
		[ReadOnly] public string streamingAssetsPathXBOXONEMovieAudio;
		[ReadOnly] public string streamingAssetsPathSwitchMovieAudio;
		[ReadOnly] public string streamingAssetsPathCommonMovieAudio;

		[ReadOnly] public bool streamingAssetsPathStandaloneIsMovie;
		[ReadOnly] public bool streamingAssetsPathStandaloneWindowsIsMovie;
		[ReadOnly] public bool streamingAssetsPathStandaloneLinuxIsMovie;
		[ReadOnly] public bool streamingAssetsPathStandaloneOSXIsMovie;
		[ReadOnly] public bool streamingAssetsPathIOSIsMovie;
		[ReadOnly] public bool streamingAssetsPathAndroidIsMovie;
		[ReadOnly] public bool streamingAssetsPathPS4IsMovie;
		[ReadOnly] public bool streamingAssetsPathXBOXONEIsMovie;
		[ReadOnly] public bool streamingAssetsPathSwitchIsMovie;
		[ReadOnly] public bool streamingAssetsPathCommonIsMovie;

		[ReadOnly] public bool canBeUnloaded;

		//private static Dictionary<Graphic, int> iterations = null;
		//private static Dictionary<Graphic, Color> colorCache = null;

		//private ResourceAsyncOperation task;

		public Object directRef;
		public Texture2D directRefTexture;
		public Sprite directRefSprite;

		public object loadedObject {

			get {

				return WindowSystemResources.GetResourceObjectById(this);

			}

		}

		public bool loaded {

			get {

				return WindowSystemResources.IsResourceObjectLoaded(this);

			}

		}

		public bool IsLoadable(bool checkDirectRef = true) {

			return
				this.loadableAssetBundle == true ||
				this.loadableResource == true ||
				this.loadableStream == true ||
				this.loadableWeb == true ||
				(checkDirectRef == true && this.directRef != null);

		}

		public bool HasDirectRef() {

			return this.directRef != null;

		}

		public T GetDirectRef<T>() {

			if (this.directRef != null) {

				if (typeof(T) == typeof(Sprite) && this.directRef is Texture2D) {

					return (T)(object)this.directRefSprite;

				}

				if (typeof(T) == typeof(Texture2D) && this.directRef is Sprite) {

					return (T)(object)this.directRefTexture;

				}

				if (this.directRef is GameObject) {

					return (T)(object)((this.directRef as GameObject).GetComponent(typeof(T)));

				}

				return (T)(object)this.directRef;

			}

			return default(T);

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

				this.UnloadObject_INTERNAL(this.loadedObject as Object);

			}

		}

		/*public T Load<T>() where T : Object {

			if (this.loadedObject != null) {

				return this.loadedObject as T;

			}

			#region Load Resource
			if (this.loadableResource == true) {

				this.loadedObject = UnityEngine.UI.Windows.WindowSystemResources.Load<T>(this.resourcesPath);
				if (this.loadedObject != null) {

					return this.loadedObject as T;

				}

			}
			#endregion

			return null;

		}*/

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

					callback.Invoke(UnityEngine.UI.Windows.WindowSystemResources.Load<AudioClip>(this.resourcesPath));

				}

				yield break;

			}
			#endregion

			#region Load Stream
			if (this.loadableStream == true) {

				var path = this.GetStreamPath(withFile: true);
				//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("Loading: " + path);
				var www = new WWW(path);
				while (www.isDone == false) {

					yield return 0;

				}

				if (string.IsNullOrEmpty(www.error) == true) {
#if UNITY_5_6_OR_NEWER
					var clip = www.GetAudioClip();
#else
					var clip = www.audioClip;
#endif
					//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("Callback!");
					callback.Invoke(clip);

				} else {

					//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("Callback!");
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

		public System.Collections.Generic.IEnumerator<byte> Load<T>(IResourceReference component, string customResourcePath, System.Action<T> callback, bool async) /*where T : Object*/ {
			
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
			platformPath = string.Format("{0}/", UnityEngine.UI.Windows.WindowSystemResources.ALL_TARGETS[UnityEditor.EditorUserBuildSettings.activeBuildTarget]);
			#elif UNITY_ANDROID
			platformPath = "Android/";
			#elif UNITY_IOS || UNITY_TVOS
			platformPath = "iOS/";
			#elif UNITY_PS4
			platformPath = "PS4/";
			#elif UNITY_SWTICH
			platformPath = "Switch/";
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
			{
				#if UNITY_STANDALONE_OSX
				path = this.streamingAssetsPathStandaloneOSXMovieAudio;
				#endif
			}
			{
				#if UNITY_STANDALONE_LINUX
				path = this.streamingAssetsPathStandaloneLinuxMovieAudio;
				#endif
			}
			{
				#if UNITY_STANDALONE_WIN
				path = this.streamingAssetsPathStandaloneWindowsMovieAudio;
				#endif
			}
			if (string.IsNullOrEmpty(path) == true) path = this.streamingAssetsPathStandaloneMovieAudio;
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
			#elif UNITY_SWITCH
			path = this.streamingAssetsPathSwitchMovieAudio;
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
			{
				#if UNITY_STANDALONE_OSX
				path = this.streamingAssetsPathStandaloneOSX;
				result = this.streamingAssetsPathStandaloneOSXIsMovie;
				#endif
			}
			{
				#if UNITY_STANDALONE_LINUX
				path = this.streamingAssetsPathStandaloneLinux;
				result = this.streamingAssetsPathStandaloneLinuxIsMovie;
				#endif
			}
			{
				#if UNITY_STANDALONE_WIN
				path = this.streamingAssetsPathStandaloneWindows;
				result = this.streamingAssetsPathStandaloneWindowsIsMovie;
				#endif
			}
			if (string.IsNullOrEmpty(path) == true) {
				
				path = this.streamingAssetsPathStandalone;
				result = this.streamingAssetsPathStandaloneIsMovie;
				
			}
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
			#elif UNITY_SWITCH
			result = this.streamingAssetsPathSwitchIsMovie;
			path = this.streamingAssetsPathSwitch;
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
			return (fileExt == "avi" || fileExt == "mp4" || fileExt == "ogv" || fileExt == "mov" || fileExt == "webm");

		}

		public string GetStreamPath(bool withFile = false) {

			var combine = true;
			var path = string.Empty;
			var prefix = string.Empty;
			var prefixPath = string.Empty;
			var result = string.Empty;
			#if UNITY_STANDALONE || UNITY_EDITOR
			{
				#if UNITY_STANDALONE_OSX
				path = this.streamingAssetsPathStandaloneOSX;
				#endif
			}
			{
				#if UNITY_STANDALONE_LINUX
				path = this.streamingAssetsPathStandaloneLinux;
				#endif
			}
			{
				#if UNITY_STANDALONE_WIN
				path = this.streamingAssetsPathStandaloneWindows;
				#endif
			}
			if (string.IsNullOrEmpty(path) == true) path = this.streamingAssetsPathStandalone;
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
			#elif UNITY_SWITCH
			path = this.streamingAssetsPathSwitch;
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

		public static int GetJavaHash(string s) {

			if (string.IsNullOrEmpty(s) == true) return 0;

			int hash = 0;
			foreach (char c in s) {

				hash = 31 * hash + c;

			}

			return hash;

		}
		
		public static long GetKey(int val1, int val2) {

			return (long)(((long)val1 << 32) | ((long)val2 & 0xffffffff));

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
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneWindows, this.GetPlatformDirectory(Platform.StandaloneWindows, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneLinux, this.GetPlatformDirectory(Platform.StandaloneLinux, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneOSX, this.GetPlatformDirectory(Platform.StandaloneOSX, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathPS4, this.GetPlatformDirectory(Platform.PS4, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathXBOXONE, this.GetPlatformDirectory(Platform.XBOXONE, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathSwitch, this.GetPlatformDirectory(Platform.Switch, localDir, filenameWithoutExt));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathCommon, this.GetPlatformDirectory(Platform.Common, localDir, filenameWithoutExt));
					
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathIOSMovieAudio, this.GetMovieAudio(this.streamingAssetsPathIOS));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathAndroidMovieAudio, this.GetMovieAudio(this.streamingAssetsPathAndroid));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneMovieAudio, this.GetMovieAudio(this.streamingAssetsPathStandalone));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneWindowsMovieAudio, this.GetMovieAudio(this.streamingAssetsPathStandaloneWindows));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneLinuxMovieAudio, this.GetMovieAudio(this.streamingAssetsPathStandaloneLinux));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneOSXMovieAudio, this.GetMovieAudio(this.streamingAssetsPathStandaloneOSX));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathPS4MovieAudio, this.GetMovieAudio(this.streamingAssetsPathPS4));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathXBOXONEMovieAudio, this.GetMovieAudio(this.streamingAssetsPathXBOXONE));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathSwitchMovieAudio, this.GetMovieAudio(this.streamingAssetsPathSwitch));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathCommonMovieAudio, this.GetMovieAudio(this.streamingAssetsPathCommon));
					
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathIOSIsMovie, this.IsMovie(this.streamingAssetsPathIOS));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathAndroidIsMovie, this.IsMovie(this.streamingAssetsPathAndroid));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneIsMovie, this.IsMovie(this.streamingAssetsPathStandalone));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneWindowsIsMovie, this.IsMovie(this.streamingAssetsPathStandaloneWindows));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneLinuxIsMovie, this.IsMovie(this.streamingAssetsPathStandaloneLinux));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathStandaloneOSXIsMovie, this.IsMovie(this.streamingAssetsPathStandaloneOSX));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathPS4IsMovie, this.IsMovie(this.streamingAssetsPathPS4));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathXBOXONEIsMovie, this.IsMovie(this.streamingAssetsPathXBOXONE));
					ME.EditorUtilities.SetValueIfDirty(ref this.streamingAssetsPathSwitchIsMovie, this.IsMovie(this.streamingAssetsPathSwitch));
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
					
					if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogError("Missing WindowSystemAssetBundlesMap!");
					
				} else {
					
					ME.EditorUtilities.SetValueIfDirty(ref this.objectIndexAssetBundle, map.GetIndex(this.assetBundleName, this.assetPath));
					
				}
				
			}
			#endregion

			var uniquePath = (this.multiObjects == true) ? (string.Format("{0}#{1}", this.GetAssetPath(), this.objectIndex)) : this.GetAssetPath();
			ME.EditorUtilities.SetValueIfDirty(ref this.id, ResourceBase.GetJavaHash(uniquePath));

			ME.EditorUtilities.SetValueIfDirty(ref this.canBeUnloaded, ((item is GameObject) == false && (item is Component) == false));

			#region directRef
			if (this.IsLoadable(checkDirectRef: false) == false) {

				ME.EditorUtilities.SetObjectIfDirty(ref this.directRef, item);
				this.directRefTexture = item as Texture2D;
				this.directRefSprite = item as Sprite;
				
				if (item is Texture) {
					
					this.directRefSprite = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(this.assetPath).OfType<Sprite>().FirstOrDefault<Sprite>();

				}
				
				if (item is Sprite) {
				
					this.directRefTexture = (item as Sprite).texture;

				}

			} else {

				ME.EditorUtilities.SetObjectIfDirty(ref this.directRef, null);
				var obj = (Object)this.directRefSprite;
				ME.EditorUtilities.SetObjectIfDirty(ref obj, null);
				obj = (Object)this.directRefTexture;
				ME.EditorUtilities.SetObjectIfDirty(ref obj, null);

			}
			#endregion

		}
		
		private string GetAssetPath() {

			var path = this.streamingAssetsPathIOS;
			if (string.IsNullOrEmpty(path) == true) path = this.streamingAssetsPathAndroid;
			if (string.IsNullOrEmpty(path) == true) path = this.streamingAssetsPathStandalone;
			if (string.IsNullOrEmpty(path) == true) path = this.streamingAssetsPathPS4;
			if (string.IsNullOrEmpty(path) == true) path = this.streamingAssetsPathXBOXONE;
			if (string.IsNullOrEmpty(path) == true) path = this.streamingAssetsPathSwitch;
			if (string.IsNullOrEmpty(path) == true) path = this.streamingAssetsPathCommon;
			if (string.IsNullOrEmpty(path) == true) path = this.assetPath;

			return path;

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