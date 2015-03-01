using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Flow {

	public class FlowData : ScriptableObject {

		public Vector2 scrollPos = new Vector2(-1f, -1f);

		public List<FlowWindow> windows = new List<FlowWindow>();
		public bool isDirty = false;

		//private Rect selectionRect;
		private List<int> selected = new List<int>();

		public void Save() {

#if UNITY_EDITOR
			if (this.isDirty == true) UnityEditor.EditorUtility.SetDirty(this);
#endif

			this.isDirty = false;

		}

		public void ResetSelection() {

			this.selected.Clear();

		}

		public List<int> GetSelected() {

			return this.selected;

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

			foreach (var window in this.windows) {

				window.Detach(id, oneWay: true);

			}

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