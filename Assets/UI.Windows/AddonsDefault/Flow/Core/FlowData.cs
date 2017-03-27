using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Extensions;
using ME;
using UnityEngine.UI.Windows.Audio;
using UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEngine.UI.Windows.Plugins.Flow {
	
	public enum ModeLayer : byte {
		
		Flow,
		Audio,
		
	};

	[System.Serializable]
	public class FlowTag {
		
		public int id;
		public string title;
		public int color;
		public bool enabled = true;
		
		public FlowTag(int id, string title) {
			
			this.id = id;
			this.title = title;
			this.color = 0;
			this.enabled = true;
			
		}
		
	};

	public enum FlowView : byte {

		None = 0x0,
		Layout = 0x1,
		VideoTransitions = 0x2,
		AudioTransitions = 0x4,

		Transitions = VideoTransitions | AudioTransitions,

	};

	public enum AuthKeyPermissions : byte {

		None = 0x0,
		ABTesting = 0x1,
		Analytics = 0x2,
		Ads = 0x4,

	};

	public class FlowData : ScriptableObject {

		[Header("Version Data")]
		public string lastModified = "-";
		public long lastModifiedUnix = 0;
		public Version version;
		
		#if UNITY_EDITOR
		[Header("Services Auth")]
		public string authKey;
		[HideInInspector]
		public string editorAuthKey;
		#endif
		[HideInInspector]
		public string buildAuthKey;
		[HideInInspector]
		public AuthKeyPermissions authKeyPermissions = AuthKeyPermissions.None;

		[Header("Compiler Plugin")]
		public string namespaceName;
		public bool forceRecompile;
		public bool minimalScriptsSize;

		[Header("Flow Data")]
		[ReadOnly]
		public float zoom = 1f;
		[ReadOnly]
		public Vector2 scrollPos = new Vector2(1f, 1f);

		[ReadOnly]
		public int rootWindow;
		[ReadOnly]
		public List<int> defaultWindows = new List<int>();

		[System.Obsolete("Windows List was deprecated. Use windowAssets List instead.")]
		[HideInInspector]
		public List<FlowWindow> windows = new List<FlowWindow>();
		[ReadOnly]
		public List<Data.FlowWindow> windowAssets = new List<Data.FlowWindow>();

		[ReadOnly]
		public List<FlowTag> tags = new List<FlowTag>();
		
		public Audio.Data audio = new Audio.Data();
		
		public ModeLayer modeLayer = ModeLayer.Flow;

		//private Rect selectionRect;
		[HideInInspector]
		private List<int> selected = new List<int>();

		[HideInInspector]
		public FlowView flowView = FlowView.Layout | FlowView.Transitions;
		[HideInInspector]
		public float flowWindowWithLayoutScaleFactor = 0f;

		[ReadOnly]
		public bool isDirty = false;

		[ReadOnly]
		public bool recompileNeeded = false;

		#if UNITY_EDITOR
		#region UPGRADES
		#pragma warning disable 612,618
		public bool UpgradeTo105() {

			this.flowView = FlowView.Layout | FlowView.Transitions;
			
			UnityEditor.EditorUtility.SetDirty(this);

			return true;
			
		}

		public bool UpgradeTo103() {

			// Need to recompile
			return true;
			
		}

		public bool UpgradeTo102() {

			this.modeLayer = ModeLayer.Flow;

			UnityEditor.EditorUtility.SetDirty(this);

			return true;
			
		}

		public bool UpgradeTo099() {

			return true;

		}
		#pragma warning restore 612,618
		#endregion
		#endif

		#if UNITY_EDITOR
		[ContextMenu("Setup")]
		public void Setup() {
			
			this.audio.Setup();

			var screens = this.GetAllScreens();
			foreach (var screen in screens) {

				if (screen != null && screen.window != null) screen.window.Setup(this);

			}
			
		}

		[ContextMenu("Refresh All Screens")]
		public void RefreshAllScreens() {

			foreach (var window in this.windowAssets) {

				window.RefreshScreen();

			}

		}

		public void ValidateAuthKey(System.Action<bool> onResult) {

			var splitted = this.authKey.Split('-');
			if (splitted.Length == 2) {

				this.buildAuthKey = splitted[0];
				this.editorAuthKey = splitted[1];
				
				var rnd = Random.Range(0, 3);
				if (rnd == 0) {
					
					this.authKeyPermissions = AuthKeyPermissions.ABTesting;

				} else if (rnd == 1) {
					
					this.authKeyPermissions = AuthKeyPermissions.Analytics;

				} else {

					this.authKeyPermissions = AuthKeyPermissions.ABTesting | AuthKeyPermissions.Analytics;

				}

				this.authKeyPermissions |= AuthKeyPermissions.Ads;

				onResult(true);

			} else {

				this.buildAuthKey = string.Empty;
				this.editorAuthKey = string.Empty;
				this.authKeyPermissions = AuthKeyPermissions.None;
				
				onResult(false);

			}

		}

		public string GetAuthKeyEditor() {
			
			return this.editorAuthKey;
			
		}
		
		public bool IsValidAuthKey() {
			
			return string.IsNullOrEmpty(this.authKey) == false;
			
		}
		#endif

		public bool IsCompileDirty() {

			return this.recompileNeeded;

		}

		public void SetCompileDirty(bool state) {

			this.recompileNeeded = state;

		}
		
		public string GetAuthKeyBuild() {
			
			return this.buildAuthKey;
			
		}

		public bool IsValidAuthKey(AuthKeyPermissions permission) {

			if (permission == AuthKeyPermissions.None) return true;

			return (this.authKeyPermissions & permission) != 0;
			
		}

		public bool HasView(FlowView view) {

			return (this.flowView & view) != 0;

		}

		/*private void OnEnable() {

			this.namespaceName = string.IsNullOrEmpty(this.namespaceName) == true ? string.Format("{0}.UI", this.name) : this.namespaceName;

		}*/

		public WindowBase GetRootScreen(bool runtime = false) {

			var flowWindow = this.windowAssets.FirstOrDefault((w) => w.id == this.rootWindow);
			if (flowWindow != null) {

				return flowWindow.GetScreen(runtime).Load<WindowBase>();

			}

			return null;

		}

		public List<WindowItem> GetAllScreens(System.Func<Data.FlowWindow, bool> predicate = null, bool runtime = false) {
			
			var list = new List<WindowItem>();
			foreach (var window in this.windowAssets) {

			    if (runtime == true) {

                    window.FilterRuntimeScreens();

			    }

				if (window.IsSmall() == true) continue;
                if (predicate != null && predicate(window) == false) continue;

				var screen = window.GetScreen(runtime);
				if (screen == null) continue;

				list.Add(screen);
				
			}
			
			return list;
			
		}
		
		public List<WindowItem> GetDefaultScreens(bool runtime = false) {

			return this.GetAllScreens((w) => this.defaultWindows.Contains(w.id), runtime);
			
		}

		public string GetModulesPath() {

			var modulesPath = string.Empty;
			var data = this;

			#if UNITY_EDITOR
			var dataPath = UnityEditor.AssetDatabase.GetAssetPath(data);
			var directory = System.IO.Path.GetDirectoryName(dataPath);
			var projectName = data.name;
			modulesPath = System.IO.Path.Combine(directory, string.Format("{0}.Modules", projectName));
			#endif

			return modulesPath;

		}

		public void Save() {

			#if UNITY_EDITOR
			if (this.isDirty == true) {

				var dateTime = System.DateTime.Now;
				this.lastModified = dateTime.ToString("dd.MM.yyyy hh:mm");
				this.lastModifiedUnix = dateTime.ToUnixTime();

				UnityEditor.EditorUtility.SetDirty(this);

				foreach (var window in this.windowAssets) {

					this.VerifyRename(window.id);
					if (window.GetScreen() != null && window.GetScreen().window != null) window.GetScreen().window.Setup(window.id, this);
					window.isDirty = true;
					window.Save();

				}

			}
			#endif

			this.isDirty = false;

		}

		public AttachItem GetAttachItem(int from, int to) {
			
			var fromWindow = this.GetWindow(from);
			var item = fromWindow.attachItems.FirstOrDefault((element) => element.targetId == to);

			if (item == null) {

				return AttachItem.Empty;

			}

			return item;

		}

		#region TAGS
		public int GetNextTagId() {
			
			var maxId = 0;
			foreach (var tag in this.tags) {
				
				if (tag.id > maxId) maxId = tag.id;
				
			}
			
			return maxId + 1;
			
		}

		public FlowTag GetTag(int id) {

			return this.tags.FirstOrDefault((t) => t.id == id);

		}

		public void AddTag(Data.FlowWindow window, FlowTag tag) {

			var contains = this.tags.FirstOrDefault((t) => t.title.ToLower() == tag.title.ToLower());
			if (contains == null) {

				this.tags.Add(tag);

			} else {

				tag = contains;

			}

			window.AddTag(tag);

			this.isDirty = true;

		}
		
		public void RemoveTag(Data.FlowWindow window, FlowTag tag) {
			
			window.RemoveTag(tag);

			this.isDirty = true;

		}
		#endregion

		#region AUDIO
		public int GetNextAudioItemId(ClipType clipType) {
			
			var maxId = 0;
			var states = this.audio.GetStates(clipType);
			foreach (var state in states) {
				
				if (state.key > maxId) maxId = state.key;
				
			}
			
			return maxId + 1;
			
		}

		public void AddAudioItem(ClipType clipType, Audio.Data.State state) {

			state.key = this.GetNextAudioItemId(clipType);
			this.audio.Add(clipType, state);

		}

		public void RemoveAudioItem(ClipType clipType, int key) {

			this.audio.Remove(clipType, key);

		}
		#endregion

		public void SetRootWindow(int id) {
			
			this.rootWindow = id;

			this.isDirty = true;

		}
		
		public int GetRootWindow() {
			
			return this.rootWindow;
			
		}

		public List<int> GetDefaultWindows() {

			return this.defaultWindows;

		}

		public void SetDefaultWindows(List<int> defaultWindows) {

			this.defaultWindows = defaultWindows;

		}

		public void ResetSelection() {

			this.selected.Clear();

		}

		public List<int> GetSelected() {

			return this.selected;

		}
		
		public void SelectWindows(int[] ids) {

			this.selected.Clear();
			foreach (var window in this.windowAssets) {
				
				if (window.IsContainer() == false && ids.Contains(window.id) == true) {
					
					this.selected.Add(window.id);
					
				}
				
			}
			
		}

		public void SelectWindowsInRect(Rect rect, System.Func<Data.FlowWindow, bool> predicate = null) {

			//this.selectionRect = rect;

			this.selected.Clear();
			foreach (var window in this.windowAssets) {

				if (window.IsContainer() == false && rect.Overlaps(window.rect, true) == true && (predicate == null || predicate(window) == true)) {

					this.selected.Add(window.id);

				}

			}

		}

		public Vector2 GetScrollPosition() {
			
			return this.scrollPos;
			
		}
		
		public void SetScrollPosition(Vector2 scrollPos) {
			
			this.scrollPos = scrollPos;
			
		}

		#if UNITY_EDITOR
		public void Attach(int source, int index, int other, bool oneWay, WindowLayoutElement component = null) {
			
			var window = this.GetWindow(source);
			window.Attach(other, index, oneWay, component);
			
			this.isDirty = true;
			
		}
		
		public void Detach(int source, int index, int other, bool oneWay, WindowLayoutElement component = null) {
			
			var window = this.GetWindow(source);
			window.Detach(other, index, oneWay, component);
			
			this.isDirty = true;
			
		}
		
		public bool AlreadyAttached(int source, int index, int other, WindowLayoutElement component = null) {

			return this.windowAssets.Any((w) => w.id == source && w.AlreadyAttached(other, index, component));
			
		}

		public void Attach(int source, int other, bool oneWay, WindowLayoutElement component = null) {

			var window = this.GetWindow(source);
			window.Attach(other, 0, oneWay, component);

			this.isDirty = true;

		}
		
		public void Detach(int source, int other, bool oneWay, WindowLayoutElement component = null) {
			
			var window = this.GetWindow(source);
			window.Detach(other, 0, oneWay, component);

			this.isDirty = true;

		}

		public bool AlreadyAttached(int source, int other, WindowLayoutElement component = null) {

			return this.windowAssets.Any((w) => w.id == source && w.AlreadyAttached(other, 0, component));
			
		}
		
		public void DestroyWindow(int id) {
			
			// Remove window
			//this.windows.Remove(this.GetWindow(id));
			var data = this.GetWindow(id);
			this.windowAssets.Remove(data);
			
			#if UNITY_EDITOR
			// Delete any old sub assets inside the prefab.
			/*var assetPath = UnityEditor.AssetDatabase.GetAssetPath(data);
			var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
			for (int i = 0; i < assets.Length; ++i) {

				var asset = assets[i];
				if (UnityEditor.AssetDatabase.IsMainAsset(asset) ||
				    asset is GameObject ||
				    asset is Component) {

					continue;

				} else {

					Object.DestroyImmediate(asset, allowDestroyingAssets: true);

				}

			}*/
			Object.DestroyImmediate(data, allowDestroyingAssets: true);
			#endif

			this.selected.Remove(id);
			this.defaultWindows.Remove(id);
			
			foreach (var window in this.windowAssets) {
				
				window.Detach(id, oneWay: true);
				
			}

			this.ResetCache();

			this.isDirty = true;
			
		}
		#endif
		
		public Data.FlowWindow CreateWindow(Data.FlowWindow.Flags flags) {

			return this.CreateWindow_INTERNAL(flags);

		}

		public Data.FlowWindow CreateDefaultLink() {

			var window = this.CreateWindow_INTERNAL(
				UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.IsSmall |
				UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.CantCompiled |
				UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.ShowDefault
				);
			
			window.title = "Default Link";
			window.rect.width = 150f;
			window.rect.height = 30f;

			return window;

		}

		public Data.FlowWindow CreateContainer() {

			return this.CreateWindow_INTERNAL(UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.IsContainer);
			
		}

		public Data.FlowWindow CreateWindow() {

			return this.CreateWindow_INTERNAL(UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.Default);

		}

		private Data.FlowWindow CreateWindow_INTERNAL(Data.FlowWindow.Flags flags) {
			
			var newId = this.AllocateId();

			//var window = new Data.FlowWindow(newId, flags);
			//this.windows.Add(window);

			var window = Data.FlowWindow.CreateInstance<Data.FlowWindow>();
			#if UNITY_EDITOR
			window.name = window.ToString();
			UnityEditor.AssetDatabase.AddObjectToAsset(window, this);
			#endif
			window.Setup(newId, flags);

			this.windowAssets.Add(window);
			this.windowsCache.Clear();
			this.isDirty = true;

			this.Save();

			#if UNITY_EDITOR
			UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(window), UnityEditor.ImportAssetOptions.ForceUpdate);
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.EditorUtility.SetDirty(window);
			#endif

			return window;

		}

		public void VerifyRename(int id) {

			var data = this.GetWindow(id);
			if (data == null) return;

			data.name = data.ToString();

		}

		public IEnumerable<Data.FlowWindow> GetWindows() {
			
			return this.windowAssets.Where((w) => w.IsContainer() == false && w.IsEnabled());
			
		}
		
		public IEnumerable<Data.FlowWindow> GetContainers() {
			
			return this.windowAssets.Where((w) => w.IsContainer() == true && w.IsEnabled());
			
		}
		
		public Data.FlowWindow GetWindow(WindowBase window, bool runtime) {

			if (window == null) return null;

			return this.windowAssets.FirstOrDefault((w) => {

				if (w.GetScreen(runtime) != null) {

					return w.GetScreen(runtime).Load<WindowBase>().SourceEquals(window);

				}

				return window.SourceEquals(null);

			});

		}

		private Cache windowsCache = new Cache();
		public Data.FlowWindow GetWindow(int id) {

			if (this.windowsCache.IsEmpty() == true) {

				this.windowsCache.Fill(this.windowAssets, (w, i) => w.id, (w, i) => i);

			}

			var index = this.windowsCache.GetValue(id);
			if (index == -1) return null;
			if (index < 0 || index >= this.windowAssets.Count) return null;

			return this.windowAssets[index];

		}

		public void ResetCache() {

			this.windowsCache.Clear();

		}

		public int AllocateId() {

			var maxId = 0;
			foreach (var window in this.windowAssets) {

				if (maxId < window.id) maxId = window.id;

			}

			return ++maxId;

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Flow/Graph")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<FlowData>();
			
		}
		#endif

	}

}
