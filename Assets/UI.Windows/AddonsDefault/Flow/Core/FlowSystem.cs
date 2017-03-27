
using System.Collections.Generic;
using UnityEngine.UI.Windows.Audio;

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
			
			rect.x = Mathf.Floor(rect.x / FlowSystem.grid.x) * FlowSystem.grid.x;
			rect.y = Mathf.Floor(rect.y / FlowSystem.grid.y) * FlowSystem.grid.y;
			
			return rect;
			
		}

		public static void Save() {

			FlowSystem.instance.data.Save();

		}

		public static bool IsCompileDirty() {

			return FlowSystem.instance.data.IsCompileDirty();

		}

		public static void SetCompileDirty(bool state = true) {
			
			if (FlowSystem.instance == null ||
			    FlowSystem.instance.data == null) {
				
				return;
				
			}
			
			FlowSystem.instance.data.SetCompileDirty(state);

		}

		public static void SetDirty() {

			if (FlowSystem.instance == null ||
				FlowSystem.instance.data == null) {

				return;

			}

			FlowSystem.instance.data.isDirty = true;

		}

		public static FlowData GetData() {
			
			return FlowSystem.instance.data;
			
		}

		public static void SetData(FlowData data) {

			var _data = FlowSystem.instance.data;
			if (_data != data && data != null) data.ResetCache();

			FlowSystem.instance.data = data;

		}

		public static bool HasData() {

			return FlowSystem.instance.data != null;

		}

		public static void SetZoom(float value) {

			if (FlowSystem.instance == null ||
				FlowSystem.instance.data == null) return;

			var changed = (FlowSystem.instance.data.zoom != value);

			if (value > 0.98f) value = 1f;

			FlowSystem.instance.data.zoom = value;
			if (changed == true) FlowSystem.SetDirty();

		}

		public static float GetZoom() {

			if (FlowSystem.instance.data == null) return 1f;

			return FlowSystem.instance.data.zoom;

		}

		public static AttachItem GetAttachItem(int from, int to) {

			return FlowSystem.instance.data.GetAttachItem(from, to);

		}
		
		#region TAGS
		public static List<FlowTag> GetTags() {
			
			if (FlowSystem.HasData() == false) return null;
			
			return FlowSystem.instance.data.tags;
			
		}
		
		public static void AddTag(Data.FlowWindow window, FlowTag tag) {
			
			FlowSystem.instance.data.AddTag(window, tag);
			
		}
		
		public static void RemoveTag(Data.FlowWindow window, FlowTag tag) {
			
			FlowSystem.instance.data.RemoveTag(window, tag);
			
		}
		#endregion
		
		#region AUDIO
		public static List<Audio.Data.State> GetAudioItems(ClipType clipType) {
			
			if (FlowSystem.HasData() == false) return null;
			
			return FlowSystem.instance.data.audio.GetStates(clipType);
			
		}
		
		public static void AddAudioItem(ClipType clipType, Audio.Data.State state) {
			
			FlowSystem.instance.data.AddAudioItem(clipType, state);
			
		}
		
		public static void RemoveAudioItem(ClipType clipType, int key) {
			
			FlowSystem.instance.data.RemoveAudioItem(clipType, key);
			
		}
		#endregion

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

		public static IEnumerable<Data.FlowWindow> GetWindows() {
			
			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetWindows();
			
		}
		
		public static IEnumerable<Data.FlowWindow> GetContainers() {

			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetContainers();
			
		}

		public static Data.FlowWindow GetWindow(WindowBase window, bool runtime) {
			
			if (FlowSystem.HasData() == false) return null;
			
			return FlowSystem.instance.data.GetWindow(window, runtime);

		}

		public static Data.FlowWindow GetWindow(int id) {
			
			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetWindow(id);

		}
		
		public static Data.FlowWindow CreateContainer() {
			
			return FlowSystem.instance.data.CreateContainer();
			
		}

		public static Data.FlowWindow CreateWindow() {
			
			return FlowSystem.instance.data.CreateWindow();
			
		}
		
		public static Data.FlowWindow CreateWindow(Data.FlowWindow.Flags flags) {
			
			return FlowSystem.instance.data.CreateWindow(flags);
			
		}

		public static Data.FlowWindow CreateDefaultLink() {

			return FlowSystem.instance.data.CreateDefaultLink();

		}

		#if UNITY_EDITOR
		public static void DestroyWindow(int id) {

			FlowSystem.instance.data.DestroyWindow(id);
			FlowSystem.SetCompileDirty();

		}
		
		public static void Attach(int source, int other, bool oneWay, WindowLayoutElement component = null) {

			FlowSystem.instance.data.Attach(source, other, oneWay, component);
			FlowSystem.SetCompileDirty();

		}
		
		public static void Detach(int source, int other, bool oneWay, WindowLayoutElement component = null) {
			
			FlowSystem.instance.data.Detach(source, other, oneWay, component);
			FlowSystem.SetCompileDirty();

		}
		
		public static bool AlreadyAttached(int source, int other, WindowLayoutElement component = null) {
			
			return FlowSystem.instance.data.AlreadyAttached(source, other, component);

		}
		
		public static void Attach(int source, int index, int other, bool oneWay, WindowLayoutElement component = null) {

			FlowSystem.instance.data.Attach(source, index, other, oneWay, component);
			FlowSystem.SetCompileDirty();
			
		}
		
		public static void Detach(int source, int index, int other, bool oneWay, WindowLayoutElement component = null) {
			
			FlowSystem.instance.data.Detach(source, index, other, oneWay, component);
			FlowSystem.SetCompileDirty();
			
		}
		
		public static bool AlreadyAttached(int source, int index, int other, WindowLayoutElement component = null) {
			
			return FlowSystem.instance.data.AlreadyAttached(source, index, other, component);
			
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
			window.isMovingState = true;

			if (window.IsContainer() == true) {

				var childs = window.attachItems;
				foreach (var child in childs) {
					
					FlowSystem.MoveContainerOrWindow(child.targetId, delta);
					
				}
				
			} else {
				
				window.Move(delta);
				
			}
			
		}
		
		public static void ForEachContainer(int startId, System.Func<Data.FlowWindow, string, string> each, string accumulate = "") {
			
			var window = FlowSystem.GetWindow(startId);
			if (window.IsContainer() == true) {

				accumulate += each(window, accumulate);

				var childs = window.attachItems;
				foreach (var child in childs) {

					FlowSystem.ForEachContainer(child.targetId, each, accumulate);
					
				}
				
			}
			
		}

		public static void SelectWindows(params int[] ids) {
			
			if (FlowSystem.HasData() == false) return;
			
			FlowSystem.instance.data.SelectWindows(ids);

		}

		public static void SelectWindowsInRect(Rect rect, System.Func<Data.FlowWindow, bool> predicate = null) {

			if (FlowSystem.HasData() == false) return;

			FlowSystem.instance.data.SelectWindowsInRect(rect, predicate);

		}

		public static List<int> GetSelected() {

			return FlowSystem.instance.data.GetSelected();

		}

		public static void ResetSelection() {

			FlowSystem.instance.data.ResetSelection();

		}

#if UNITY_EDITOR
		public static void DrawEditorGetKeyButton(GUISkin skin, string title = "Get Key") {

			GUILayout.BeginHorizontal();
			{

				GUILayout.FlexibleSpace();

				if (GUILayout.Button(title, skin.button, GUILayout.Height(40f), GUILayout.Width(150f)) == true) {

					Application.OpenURL(VersionInfo.GETKEY_LINK);

				}
				
				GUILayout.FlexibleSpace();

			}
			GUILayout.EndHorizontal();

		}
#endif

	}

}