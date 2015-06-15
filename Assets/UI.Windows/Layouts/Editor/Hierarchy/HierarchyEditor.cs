using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows.Hierarchy {

	[InitializeOnLoad]
	public class HierarchyEditor {
		
		public static Texture2D layout;
		public static Texture2D screen;

		public static List<int> markedScreens = new List<int>();
		public static List<int> markedLayouts = new List<int>();
		
		static HierarchyEditor() {

			// Init
			HierarchyEditor.layout = Resources.Load<Texture2D>("UI.Windows/Icons/LayoutsIcon");
			HierarchyEditor.screen = Resources.Load<Texture2D>("UI.Windows/Icons/ScreensIcon");

			EditorApplication.update += HierarchyEditor.Update;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyEditor.HierarchyItemGUI;

		}

		public static void Update() {

			var go = Object.FindObjectsOfType<GameObject>();

			HierarchyEditor.markedScreens.Clear();
			HierarchyEditor.markedLayouts.Clear();
			foreach (var g in go) {
				
				if (g.GetComponent<LayoutWindowType>() != null) HierarchyEditor.markedScreens.Add(g.GetInstanceID());
				if (g.GetComponent<WindowLayout>() != null) HierarchyEditor.markedLayouts.Add(g.GetInstanceID());

			}

		}
		
		public static void HierarchyItemGUI(int instanceID, Rect selectionRect) {

			const float size = 16f;

			var r = new Rect(selectionRect); 
			r.x += r.width - size;
			r.width = size;
			r.height = size;

			if (HierarchyEditor.markedScreens.Contains(instanceID) == true) GUI.Label(r, HierarchyEditor.screen); 
			if (HierarchyEditor.markedLayouts.Contains(instanceID) == true) GUI.Label(r, HierarchyEditor.layout); 

		}
		
	}

}