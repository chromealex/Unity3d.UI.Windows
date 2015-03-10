using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Flow {
	
	[System.Serializable]
	public class FlowTag {
		
		public int id;
		public string title;
		public int color;
		
		public FlowTag(int id, string title) {
			
			this.id = id;
			this.title = title;
			this.color = 0;
			
		}
		
	};

	public class FlowData : ScriptableObject {
		
		public string namespaceName;
		public bool forceRecompile;

		public Vector2 scrollPos = new Vector2(-1f, -1f);

		public int rootWindow;
		public List<int> defaultWindows = new List<int>();

		public List<FlowWindow> windows = new List<FlowWindow>();
		public bool isDirty = false;

		public List<FlowTag> tags = new List<FlowTag>();

		//private Rect selectionRect;
		private List<int> selected = new List<int>();

		void OnEnable() {

			namespaceName = string.IsNullOrEmpty( namespaceName ) ? this.name + ".UI" : namespaceName;
		}

		public void Save() {

#if UNITY_EDITOR
			if (this.isDirty == true) UnityEditor.EditorUtility.SetDirty(this);
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

		public FlowTag GetTag(int id) {

			return this.tags.FirstOrDefault((t) => t.id == id);

		}

		public void AddTag(FlowWindow window, FlowTag tag) {

			var contains = this.tags.Any((t) => t.title == tag.title);
			if (contains == false) this.tags.Add(tag);

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
				
				if (window.isContainer == false && ids.Contains(window.id) == true) {
					
					this.selected.Add(window.id);
					
				}
				
			}
			
		}

		public void SelectWindowsInRect(Rect rect, System.Func<FlowWindow, bool> predicate = null) {

			//this.selectionRect = rect;

			this.selected.Clear();
			foreach (var window in this.windows) {

				if (window.isContainer == false && rect.Overlaps(window.rect, true) == true && (predicate == null || predicate(window) == true)) {

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

		public void Attach(int source, int other, bool oneWay) {

			var window = this.GetWindow(source);
			window.Attach(other, oneWay);

			this.isDirty = true;

		}
		
		public void Detach(int source, int other, bool oneWay) {
			
			var window = this.GetWindow(source);
			window.Detach(other, oneWay);

			this.isDirty = true;

		}
		
		public bool AlreadyAttached(int source, int other) {

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

			this.isDirty = true;

		}
		
		public FlowWindow CreateDefaultLink() {
			
			var newId = this.AllocateId();
			var window = new FlowWindow(newId, isDefaultLink: true);
			
			this.windows.Add(window);
			
			this.isDirty = true;
			
			return window;
			
		}

		public FlowWindow CreateWindow() {
			
			var newId = this.AllocateId();
			var window = new FlowWindow(newId, isContainer: false);
			
			this.windows.Add(window);
			
			this.isDirty = true;
			
			return window;
			
		}
		
		public FlowWindow CreateContainer() {
			
			var newId = this.AllocateId();
			var window = new FlowWindow(newId, isContainer: true);
			
			this.windows.Add(window);
			
			this.isDirty = true;
			
			return window;
			
		}

		public IEnumerable<FlowWindow> GetWindows() {
			
			return this.windows.Where((w) => w.isContainer == false);
			
		}
		
		public IEnumerable<FlowWindow> GetContainers() {
			
			return this.windows.Where((w) => w.isContainer == true);
			
		}

		public FlowWindow GetWindow(int id) {

			return this.windows.FirstOrDefault((w) => w.id == id);

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
