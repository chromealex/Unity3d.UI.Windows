//#define TURN_ON
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;
using System.Linq;
using UnityEngine.UI.Windows.Components;

namespace UnityEditor.UI.Windows.Hierarchy {

	[InitializeOnLoad]
	public class HierarchyEditor {

		public class Colors {

			public class ColorItem {

				public Color back;
				public Color text;

				public ColorItem(Color back, Color text) {

					this.back = back;
					this.text = text;

				}

			}
			
			public ColorItem screens = new ColorItem(new Color32(102, 204, 255, 255), Color.white);
			public ColorItem layouts = new ColorItem(new Color32(255, 204, 102, 255), new Color32(100, 100, 100, 255));
			public ColorItem transitions = new ColorItem(new Color32(204, 255, 102, 255), new Color32(100, 100, 100, 255));
			public ColorItem modules = new ColorItem(new Color32(204, 204, 204, 255), new Color32(100, 100, 100, 255));
			public ColorItem components = new ColorItem(new Color32(255, 255, 102, 255), new Color32(100, 100, 100, 255));
			public ColorItem layoutElements = new ColorItem(new Color32(204, 102, 255, 255), Color.white);
			public ColorItem linkers = new ColorItem(new Color32(255, 111, 207, 255), Color.white);

			public ColorItem error = new ColorItem(new Color32(255, 0, 0, 255), Color.white);

		}

		public class Item {

			public bool hasScreen;
			public bool hasLayout;
			public bool hasComponent;

			public bool hasTransition;
			
			public bool hasModule;
			
			public bool hasLayoutElement;
			public bool hasLinker;

		}
		
		public static Colors colors = new Colors();
		public static Dictionary<int, Item> cache = new Dictionary<int, Item>();
		
		public static void DrawWithCache(int instanceID, Rect selectionRect) {

			Item item;
			if (HierarchyEditor.cache.TryGetValue(instanceID, out item) == false) {

				item = new Item();
				
				item.hasScreen = HierarchyEditor.markedScreens.Contains(instanceID);
				item.hasLayout = HierarchyEditor.markedLayouts.Contains(instanceID);
				item.hasComponent = HierarchyEditor.markedComponents.Contains(instanceID);
				item.hasLayoutElement = HierarchyEditor.markedLayoutElements.Contains(instanceID);
				item.hasLinker = HierarchyEditor.markedLinkers.Contains(instanceID);

				var obj = Object.FindObjectsOfType<GameObject>().FirstOrDefault((element) => element.GetInstanceID() == instanceID);
				if (obj != null) {
					
					var window = obj.GetComponent<LayoutWindowType>();
					if (window != null) {
						
						if (window.transition.transition != null) {
							
							item.hasTransition = true;
							
						}
						
						if (window.modules.GetModulesInfo().Count() > 0) {
							
							item.hasModule = true;

						}

					}
					
				}

				HierarchyEditor.cache.Add(instanceID, item);

			}

			HierarchyEditor.OnItemGUI(instanceID, selectionRect, item);

		}

		public static List<int> markedScreens = new List<int>();
		public static List<int> markedLayouts = new List<int>();
		public static List<int> markedComponents = new List<int>();
		public static List<int> markedLayoutElements = new List<int>();
		public static List<int> markedLinkers = new List<int>();

		private static float left = 0f;
		private static float top = 0f;
		private static int levels = 0;
		private static GUIStyle backgroundStyle;

		private static GameObject first;

		static HierarchyEditor() {

			// Init
			#if TURN_ON
			EditorApplication.update += HierarchyEditor.Update;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyEditor.HierarchyItemGUI;
			EditorApplication.hierarchyWindowChanged += HierarchyEditor.OnChanged;
			#endif

		}

		public static void OnItemGUI(int instanceID, Rect selectionRect, Item item) {
			
			const float sizeX = 32f;
			const float sizeY = 16f;
			
			const float offset = 1f;
			const float firstElementDeltaHeight = 5f;
			const float leftOffset = 14f;
			
			if (HierarchyEditor.backgroundStyle == null) {
				
				HierarchyEditor.backgroundStyle = new GUIStyle(EditorStyles.label);
				HierarchyEditor.backgroundStyle.normal.background = Texture2D.whiteTexture;
				
			}
			
			selectionRect.width += offset;
			
			var levels = 0;
			
			var rect = new Rect(selectionRect); 
			rect.x += rect.width - offset;
			rect.width = sizeX;
			rect.height = sizeY;
			
			if (HierarchyEditor.first != null && HierarchyEditor.first.GetInstanceID() == instanceID) {
				
				rect.y -= firstElementDeltaHeight;
				rect.height += firstElementDeltaHeight;
				
				CustomGUI.Splitter(new Rect(selectionRect.xMin - leftOffset, HierarchyEditor.top, selectionRect.width + leftOffset, 1f));
				
			}
			
			// Draw background
			var back = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
			GUI.Label(new Rect(HierarchyEditor.left - offset, rect.yMin, sizeX * HierarchyEditor.levels + offset, rect.height), string.Empty, HierarchyEditor.backgroundStyle);
			GUI.backgroundColor = back;
			
			if (item.hasScreen == true) {
				
				rect.x -= sizeX;
				HierarchyEditor.DrawLabel(rect, HierarchyEditor.colors.screens, "SCR");
				++levels;
				
				if (item.hasTransition == true) {
							
					rect.x -= sizeX;
					HierarchyEditor.DrawLabel(rect, HierarchyEditor.colors.transitions, "TR");
					++levels;
					
				}
				
				if (item.hasModule == true) {
					
					rect.x -= sizeX;
					HierarchyEditor.DrawLabel(rect, HierarchyEditor.colors.modules, "MOD");
					++levels;
					
				}

			}
			
			if (item.hasLayoutElement == true) {
				
				rect.x -= sizeX;
				HierarchyEditor.DrawLabel(rect, HierarchyEditor.colors.layoutElements, "LE");
				++levels;
				
			}

			if (item.hasLayout == true) {
				
				rect.x -= sizeX;
				HierarchyEditor.DrawLabel(rect, HierarchyEditor.colors.layouts, "LAY");
				++levels;
				
			}

			if (item.hasLinker == true) {
				
				rect.x -= sizeX;
				HierarchyEditor.DrawLabel(rect, HierarchyEditor.colors.linkers, "LNK");
				++levels;
				
			} else if (item.hasComponent == true) {
				
				rect.x -= sizeX;
				HierarchyEditor.DrawLabel(rect, HierarchyEditor.colors.components, "COM");
				++levels;
				
			}

			CustomGUI.Splitter(new Rect(HierarchyEditor.left - offset, rect.yMin, 1f, rect.height));
			
			if (levels > HierarchyEditor.levels) {
				
				HierarchyEditor.levels = levels;
				
			}
			
			if (rect.xMin < HierarchyEditor.left || HierarchyEditor.left <= 0f) {
				
				HierarchyEditor.left = rect.xMin;
				
			}
			
			if (rect.yMax > HierarchyEditor.top || HierarchyEditor.top == 0f) {
				
				HierarchyEditor.top = rect.yMax;
				
			}

		}

		public static void DrawLabel(Rect rect, Colors.ColorItem colors, string label) {

			const float offset = 1f;
			
			rect.x += offset;
			rect.y += offset;
			rect.width -= offset * 2f;
			rect.height -= offset * 2f;

			var oldColor = GUI.backgroundColor;
			GUI.backgroundColor = colors.back;
			var style = new GUIStyle(EditorStyles.boldLabel);
			style.padding = new RectOffset(-1, 0, 0, 1);
			style.margin = new RectOffset();
			style.border = new RectOffset();
			style.alignment = TextAnchor.MiddleCenter;
			style.normal.background = Texture2D.whiteTexture;
			style.normal.textColor = colors.text;
			GUI.Label(rect, label, style);
			GUI.backgroundColor = oldColor;

		}

		private static float updateTimer = 0f;
		public static void Update() {

			if (HierarchyEditor.updateTimer < 1f && Application.isPlaying == true) {

				HierarchyEditor.updateTimer += Time.deltaTime;
				return;

			}

			HierarchyEditor.updateTimer = 0f;

			//HierarchyEditor.OnChanged();

		}

		public static void OnChanged() {

			System.Action<GameObject> onItem = (go) => {
				
				if (go.GetComponent<LayoutWindowType>() != null) HierarchyEditor.markedScreens.Add(go.GetInstanceID());
				if (go.GetComponent<WindowLayout>() != null) HierarchyEditor.markedLayouts.Add(go.GetInstanceID());
				if (go.GetComponent<WindowComponent>() != null) HierarchyEditor.markedComponents.Add(go.GetInstanceID());
				if (go.GetComponent<WindowLayoutElement>() != null) HierarchyEditor.markedLayoutElements.Add(go.GetInstanceID());
				if (go.GetComponent<LinkerComponent>() != null) HierarchyEditor.markedLinkers.Add(go.GetInstanceID());
				
				if (go.transform.parent == null && go.transform.GetSiblingIndex() == 0) {
					
					HierarchyEditor.first = go;
					
				}

			};

			var gos = Object.FindObjectsOfType<GameObject>();
			
			HierarchyEditor.first = null;
			
			HierarchyEditor.markedScreens.Clear();
			HierarchyEditor.markedLayouts.Clear();
			HierarchyEditor.markedComponents.Clear();
			HierarchyEditor.markedLayoutElements.Clear();
			HierarchyEditor.markedLinkers.Clear();

			foreach (var go in gos) {

				onItem(go);

			}

			gos = Selection.gameObjects;
			foreach (var go in gos) {

				if (ME.EditorUtilities.IsPrefab(go) == false) continue;
				
				onItem(go);

			}

			HierarchyEditor.left = 0f;
			HierarchyEditor.top = 0f;
			HierarchyEditor.levels = 0;
			EditorApplication.RepaintHierarchyWindow();

		}

		public static void HierarchyItemGUI(int instanceID, Rect selectionRect) {

			HierarchyEditor.DrawWithCache(instanceID, selectionRect);

		}
		
	}

}