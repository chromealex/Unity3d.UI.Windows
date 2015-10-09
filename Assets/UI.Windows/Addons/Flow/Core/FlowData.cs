using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Extensions;
using ME;
using UnityEngine.UI.Windows.Audio;

namespace UnityEngine.UI.Windows.Plugins.Flow {
	
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

	public class FlowData : ScriptableObject {

		[Header("Version Data")]
		public string lastModified = "-";
		public long lastModifiedUnix = 0;
		public Version version;
		
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

		//private Rect selectionRect;
		[HideInInspector]
		private List<int> selected = new List<int>();

		[HideInInspector]
		public bool flowWindowWithLayout;
		[HideInInspector]
		public float flowWindowWithLayoutScaleFactor = 0f;

		[ReadOnly]
		public bool isDirty = false;

		#if UNITY_EDITOR
		#region UPGRADES
		#pragma warning disable 612,618
		public bool UpgradeTo099() {

			return true;

		}

		public bool UpgradeTo098() {
			
			foreach (var oldWindow in this.windows) {

				var window = Data.FlowWindow.CreateInstance<Data.FlowWindow>();
				//var path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/ + ".asset");
				//UnityEditor.AssetDatabase.CreateAsset(window, path);

				//window = UnityEditor.AssetDatabase.LoadAssetAtPath<Data.FlowWindow>(path);

				window.name = window.directory;

				// copy data from old FlowWindow to new
				window.compiled = oldWindow.compiled;
				window.compiledBaseClassName = oldWindow.compiledBaseClassName;
				window.compiledDerivedClassName = oldWindow.compiledDerivedClassName;
				window.compiledDirectory = oldWindow.compiledDirectory;
				window.compiledNamespace = oldWindow.compiledNamespace;
				window.compiledScreenName = oldWindow.compiledScreenName;
				
				window.id = oldWindow.id;
				window.flags = (Flow.Data.FlowWindow.Flags)(int)oldWindow.flags;
				window.title = oldWindow.title;
				window.directory = oldWindow.directory;
				window.rect = oldWindow.rect;

				window.functionExitId = oldWindow.functionExitId;
				window.functionId = oldWindow.functionId;
				window.functionRootId = oldWindow.functionRootId;

				window.isVisibleState = oldWindow.isVisibleState;
				window.randomColor = oldWindow.randomColor;

				window.screenWindowId = oldWindow.screenWindowId;
				window.SetScreen(oldWindow.screen);

				window.smallStyleDefault = oldWindow.smallStyleDefault;
				window.smallStyleSelected = oldWindow.smallStyleSelected;

				window.storeType = (Flow.Data.FlowWindow.StoreType)(byte)oldWindow.storeType;
				window.tags = oldWindow.tags;

				var states = oldWindow.states;
				window.states = new Flow.Data.CompletedState[states.Length];
				for (int i = 0; i < states.Length; ++i) {

					window.states[i] = (Flow.Data.CompletedState)(byte)states[i];
					
				}

				var attachedComponents = oldWindow.attachedComponents;
				window.attachedComponents = new List<Flow.Data.FlowWindow.ComponentLink>();
				for (int i = 0; i < attachedComponents.Count; ++i) {
					
					var old = attachedComponents[i];
					window.attachedComponents.Add(new Flow.Data.FlowWindow.ComponentLink(old.targetWindowId, old.sourceComponentTag, old.comment));
					
				}
				
				var attachItems = oldWindow.attachItems;
				window.attachItems = new List<Flow.Data.FlowWindow.AttachItem>();
				for (int i = 0; i < attachItems.Count; ++i) {
					
					var old = attachItems[i];
					window.attachItems.Add(new Flow.Data.FlowWindow.AttachItem(old.targetId) { transition = old.transition, transitionParameters = old.transitionParameters, editor = old.editor });
					
				}
				
				var comments = oldWindow.comments;
				window.comments = new List<Flow.Data.DefaultElement>();
				for (int i = 0; i < comments.Count; ++i) {
					
					var old = comments[i];
					window.comments.Add(new Flow.Data.DefaultElement() { tag = old.tag, comment = old.comment, library = old.library, elementPath = old.elementPath, elementComponent = old.elementComponent });
					
				}

				window.SetDirty();
				window.Save();

				window.RefreshScreen();
				UnityEditor.Selection.activeObject = null;

				window.name = window.ToString();
				UnityEditor.AssetDatabase.AddObjectToAsset(window, this);
				UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(window), UnityEditor.ImportAssetOptions.ForceUpdate);
				UnityEditor.AssetDatabase.SaveAssets();

				this.windowAssets.Add(window);

			}

			return false;
			
		}

		public bool UpgradeTo094() {
			
			foreach (var window in this.windows) {
				
				window.attachItems = new List<FlowWindow.AttachItem>();
				foreach (var attachId in window.attaches) {
					
					window.attachItems.Add(new FlowWindow.AttachItem(attachId));
					
				}
				
			}

			return false;
			
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

				if (screen != null) screen.Setup(this);

			}
			
		}
		#endif

		/*private void OnEnable() {

			this.namespaceName = string.IsNullOrEmpty(this.namespaceName) == true ? string.Format("{0}.UI", this.name) : this.namespaceName;

		}*/

		public WindowBase GetRootScreen() {

			var flowWindow = this.windowAssets.FirstOrDefault((w) => w.id == this.rootWindow);
			if (flowWindow != null) {

				return flowWindow.GetScreen();

			}

			return null;

		}

		public List<WindowBase> GetAllScreens(System.Func<Data.FlowWindow, bool> predicate = null) {
			
			var list = new List<WindowBase>();
			foreach (var window in this.windowAssets) {

				if (window.IsSmall() == true) continue;

				if (predicate != null && predicate(window) == false) continue;

				var screen = window.GetScreen();
				if (screen == null) continue;

				list.Add(screen);
				
			}
			
			return list;
			
		}
		
		public List<WindowBase> GetDefaultScreens() {

			return this.GetAllScreens((w) => this.defaultWindows.Contains(w.id));
			
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
					if (window.GetScreen() != null) window.GetScreen().Setup(this);
					window.isDirty = true;
					window.Save();

				}

			}
			#endif

			this.isDirty = false;

		}

		public Data.FlowWindow.AttachItem GetAttachItem(int from, int to) {
			
			var fromWindow = this.GetWindow(from);
			var item = fromWindow.attachItems.FirstOrDefault((element) => element.targetId == to);

			if (item == null) {

				return Data.FlowWindow.AttachItem.Empty;

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
		public void Attach(int source, int other, bool oneWay, WindowLayoutElement component = null) {

			var window = this.GetWindow(source);
			window.Attach(other, oneWay, component);

			this.isDirty = true;

		}
		
		public void Detach(int source, int other, bool oneWay, WindowLayoutElement component = null) {
			
			var window = this.GetWindow(source);
			window.Detach(other, oneWay, component);

			this.isDirty = true;

		}
		
		public bool AlreadyAttached(int source, int other, WindowLayoutElement component = null) {
			
			if (component != null) {
				
				return this.windowAssets.Any((w) => w.id == source && w.AlreadyAttached(other, component));
				
			}
			
			return this.windowAssets.Any((w) => w.id == source && w.AlreadyAttached(other));
			
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
			UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(window), UnityEditor.ImportAssetOptions.ForceUpdate);
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.EditorUtility.SetDirty(window);
			#endif
			window.Setup(newId, flags);

			this.windowAssets.Add(window);
			this.windowsCache.Clear();
			this.isDirty = true;

			this.Save();

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
		
		public Data.FlowWindow GetWindow(WindowBase window) {

			if (window == null) return null;

			return this.windowAssets.FirstOrDefault((w) => {

				if (w.GetScreen() != null) {

					return w.GetScreen().SourceEquals(window);

				}

				return window.SourceEquals(w.GetScreen());

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
