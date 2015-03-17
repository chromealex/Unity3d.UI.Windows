using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ME;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using ADB = UnityEditor.AssetDatabase;
#endif

namespace UnityEngine.UI.Windows.Plugins.Flow {

	public class FlowSystem {

		public FlowData data;

		private static FlowSystem _instance;
		private static FlowSystem instance {

			get {

				if (FlowSystem._instance == null) FlowSystem._instance = new FlowSystem();
				return FlowSystem._instance;

			}

		}
		
		public static Vector2 grid;

		public static Rect Grid(Rect rect) {
			
			rect.x = Mathf.Round(rect.x / FlowSystem.grid.x) * FlowSystem.grid.x;
			rect.y = Mathf.Round(rect.y / FlowSystem.grid.y) * FlowSystem.grid.y;
			
			return rect;
			
		}

		public static void Save() {

			FlowSystem.instance.data.Save();

		}

		public static void SetDirty() {

			FlowSystem.instance.data.isDirty = true;

		}

		public static FlowData GetData() {
			
			return FlowSystem.instance.data;
			
		}

		public static void SetData(FlowData data) {

			FlowSystem.instance.data = data;

		}

		public static bool HasData() {

			return FlowSystem.instance.data != null;

		}

		public static List<FlowTag> GetTags() {
			
			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.tags;

		}

		public static void AddTag(FlowWindow window, FlowTag tag) {

			FlowSystem.instance.data.AddTag(window, tag);

		}

		public static void RemoveTag(FlowWindow window, FlowTag tag) {
			
			FlowSystem.instance.data.RemoveTag(window, tag);

		}

		public static void SetRootWindow(int id) {
			
			if (FlowSystem.HasData() == false) return;

			FlowSystem.instance.data.SetRootWindow(id);
			
		}
		
		public static int GetRootWindow() {
			
			if (FlowSystem.HasData() == false) return -1;

			return FlowSystem.instance.data.GetRootWindow();
			
		}

		public static List<int> GetDefaultWindows() {
			
			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetDefaultWindows();

		}

		public static void SetDefaultWindows(List<int> list) {

			FlowSystem.instance.data.SetDefaultWindows(list);

		}

		public static IEnumerable<FlowWindow> GetWindows() {
			
			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetWindows();
			
		}
		
		public static IEnumerable<FlowWindow> GetContainers() {

			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetContainers();
			
		}

		public static FlowWindow GetWindow(int id) {
			
			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetWindow(id);

		}
		
		public static FlowWindow CreateContainer() {
			
			return FlowSystem.instance.data.CreateContainer();
			
		}

		public static FlowWindow CreateWindow() {

			return FlowSystem.instance.data.CreateWindow();

		}
		
		public static FlowWindow CreateDefaultLink() {
			
			return FlowSystem.instance.data.CreateDefaultLink();
			
		}

		#if UNITY_EDITOR
		public static void DestroyWindow(int id) {

			FlowSystem.instance.data.DestroyWindow(id);

		}
		
		public static void Attach(int source, int other, bool oneWay, WindowLayoutElement component = null) {

			FlowSystem.instance.data.Attach(source, other, oneWay, component);

		}
		
		public static void Detach(int source, int other, bool oneWay, WindowLayoutElement component = null) {
			
			FlowSystem.instance.data.Detach(source, other, oneWay, component);

		}
		
		public static bool AlreadyAttached(int source, int other, WindowLayoutElement component = null) {
			
			return FlowSystem.instance.data.AlreadyAttached(source, other, component);

		}
		#endif
		
		public static void SetScrollPosition(Vector2 pos) {
			
			if (FlowSystem.HasData() == false) return;

			FlowSystem.instance.data.SetScrollPosition(pos);
			
		}
		
		public static Vector2 GetScrollPosition() {

			if (FlowSystem.HasData() == false) return Vector2.zero;

			return FlowSystem.instance.data.GetScrollPosition();
			
		}
		
		public static void MoveContainerOrWindow(int id, Vector2 delta) {
			
			var window = FlowSystem.GetWindow(id);
			if (window.isContainer == true) {
				
				var childs = window.attaches;
				foreach (var child in childs) {
					
					FlowSystem.MoveContainerOrWindow(child, delta);
					
				}
				
			} else {
				
				window.Move(delta);
				
			}
			
		}
		
		public static void ForEachContainer(int startId, System.Func<FlowWindow, string, string> each, string accumulate = "") {
			
			var window = FlowSystem.GetWindow(startId);
			if (window.isContainer == true) {

				accumulate += each(window, accumulate);

				var childs = window.attaches;
				foreach (var child in childs) {

					FlowSystem.ForEachContainer(child, each, accumulate);
					
				}
				
			}
			
		}

		public static void SelectWindows(params int[] ids) {
			
			if (FlowSystem.HasData() == false) return;
			
			FlowSystem.instance.data.SelectWindows(ids);

		}

		public static void SelectWindowsInRect(Rect rect, System.Func<FlowWindow, bool> predicate = null) {

			if (FlowSystem.HasData() == false) return;

			FlowSystem.instance.data.SelectWindowsInRect(rect, predicate);

		}

		public static List<int> GetSelected() {

			return FlowSystem.instance.data.GetSelected();

		}

		public static void ResetSelection() {

			FlowSystem.instance.data.ResetSelection();

		}

	}

}