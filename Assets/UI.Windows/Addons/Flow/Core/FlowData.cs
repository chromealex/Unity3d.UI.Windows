using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Extensions;

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

		public string lastModified = "-";
		public Version version;

		public string namespaceName;
		public bool forceRecompile;

		public float zoom = 1f;
		public Vector2 scrollPos = new Vector2(-1f, -1f);

		public int rootWindow;
		public List<int> defaultWindows = new List<int>();

		public List<FlowWindow> windows = new List<FlowWindow>();
		public bool isDirty = false;

		public List<FlowTag> tags = new List<FlowTag>();

		//private Rect selectionRect;
		private List<int> selected = new List<int>();

		public bool flowWindowWithLayout;
		public float flowWindowWithLayoutScaleFactor = 0f;

		#if UNITY_EDITOR
		#region UPGRADES
		#pragma warning disable 612,618
		public void UpgradeTo095() {
			
			this.UpgradeTo094();
			
		}

		public void UpgradeTo094() {
			
			foreach (var window in this.windows) {
				
				window.attachItems = new List<FlowWindow.AttachItem>();
				foreach (var attachId in window.attaches) {
					
					window.attachItems.Add(new FlowWindow.AttachItem(attachId));
					
				}
				
			}
			
		}
		#pragma warning restore 612,618
		#endregion
		#endif

		/*private void OnEnable() {

			this.namespaceName = string.IsNullOrEmpty(this.namespaceName) == true ? string.Format("{0}.UI", this.name) : this.namespaceName;

		}*/

		public WindowBase GetRootScreen() {

			var flowWindow = this.windows.FirstOrDefault((w) => w.id == this.rootWindow);
			if (flowWindow != null) {

				return flowWindow.GetScreen();

			}

			return null;

		}

		public List<WindowBase> GetAllScreens(System.Func<FlowWindow, bool> predicate = null) {
			
			var list = new List<WindowBase>();
			foreach (var window in this.windows) {

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

		public void Save() {

			#if UNITY_EDITOR
			if (this.isDirty == true) {

				var dateTime = System.DateTime.Now;
				this.lastModified = dateTime.ToString("dd.MM.yyyy hh:mm");

				UnityEditor.EditorUtility.SetDirty(this);

			}
			#endif

			this.isDirty = false;

		}
		
		public int GetNextTagId() {
			
			var maxId = 0;
			foreach (var tag in this.tags) {
				
				if (tag.id > maxId) maxId = tag.id;
				
			}
			
			return maxId + 1;
			
		}
		
		public FlowWindow.AttachItem GetAttachItem(int from, int to) {
			
			var fromWindow = this.GetWindow(from);
			var item = fromWindow.attachItems.FirstOrDefault((element) => element.targetId == to);

			if (item == null) {

				return FlowWindow.AttachItem.Empty;

			}

			return item;

		}

		public FlowTag GetTag(int id) {

			return this.tags.FirstOrDefault((t) => t.id == id);

		}

		public void AddTag(FlowWindow window, FlowTag tag) {

			var contains = this.tags.FirstOrDefault((t) => t.title.ToLower() == tag.title.ToLower());
			if (contains == null) {

				this.tags.Add(tag);

			} else {

				tag = contains;

			}

			window.AddTag(tag);

			this.isDirty = true;

		}
		
		public void RemoveTag(FlowWindow window, FlowTag tag) {
			
			window.RemoveTag(tag);

			this.isDirty = true;

		}

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
			foreach (var window in this.windows) {
				
				if (window.IsContainer() == false && ids.Contains(window.id) == true) {
					
					this.selected.Add(window.id);
					
				}
				
			}
			
		}

		public void SelectWindowsInRect(Rect rect, System.Func<FlowWindow, bool> predicate = null) {

			//this.selectionRect = rect;

			this.selected.Clear();
			foreach (var window in this.windows) {

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
				
				return this.windows.Any((w) => w.id == source && w.AlreadyAttached(other, component));
				
			}
			
			return this.windows.Any((w) => w.id == source && w.AlreadyAttached(other));
			
		}
		
		public void DestroyWindow(int id) {
			
			// Remove window
			this.windows.Remove(this.GetWindow(id));
			
			this.selected.Remove(id);
			this.defaultWindows.Remove(id);
			
			foreach (var window in this.windows) {
				
				window.Detach(id, oneWay: true);
				
			}

			this.ResetCache();

			this.isDirty = true;
			
		}
		#endif
		
		public FlowWindow CreateWindow(FlowWindow.Flags flags) {
			
			var newId = this.AllocateId();
			var window = new FlowWindow(newId, flags);
			
			this.windows.Add(window);
			this.windowsCache.Clear();
			
			this.isDirty = true;
			
			return window;

		}

		public FlowWindow CreateDefaultLink() {
			
			var newId = this.AllocateId();
			var window = new FlowWindow(newId, isDefaultLink: true);
			window.title = "Default Link";
			
			window.rect.width = 150f;
			window.rect.height = 30f;

			this.windows.Add(window);
			this.windowsCache.Clear();
			
			this.isDirty = true;
			
			return window;
			
		}
		
		public FlowWindow CreateContainer() {
			
			var newId = this.AllocateId();
			var window = new FlowWindow(newId, isContainer: true);
			
			this.windows.Add(window);
			this.windowsCache.Clear();
			
			this.isDirty = true;
			
			return window;
			
		}

		public FlowWindow CreateWindow() {
			
			var newId = this.AllocateId();
			var window = new FlowWindow(newId, isContainer: false);
			
			this.windows.Add(window);
			this.windowsCache.Clear();
			
			this.isDirty = true;
			
			return window;
			
		}

		public IEnumerable<FlowWindow> GetWindows() {
			
			return this.windows.Where((w) => w.IsContainer() == false && w.IsEnabled());
			
		}
		
		public IEnumerable<FlowWindow> GetContainers() {
			
			return this.windows.Where((w) => w.IsContainer() == true && w.IsEnabled());
			
		}
		
		public FlowWindow GetWindow(WindowBase window) {

			if (window == null) return null;

			return this.windows.FirstOrDefault((w) => {

				if (w.GetScreen() != null) {

					return w.GetScreen().SourceEquals(window);

				}

				return window.SourceEquals(w.GetScreen());

			});

		}

		private Cache windowsCache = new Cache();
		public FlowWindow GetWindow(int id) {

			if (this.windowsCache.IsEmpty() == true) {

				this.windowsCache.Fill(this.windows, (w, i) => w.id, (w, i) => i);

			}

			var index = this.windowsCache.GetValue(id);
			if (index == -1) return null;
			if (index < 0 || index >= this.windows.Count) return null;

			return this.windows[index];

		}

		public void ResetCache() {

			this.windowsCache.Clear();

		}

		public int AllocateId() {

			var maxId = 0;
			foreach (var window in this.windows) {

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
