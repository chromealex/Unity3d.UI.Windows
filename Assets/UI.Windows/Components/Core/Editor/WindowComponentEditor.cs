using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEditor.UI.Windows {

	// Uncomment below line to enable preview functionality
	//[CustomEditor(typeof(WindowComponent), true)]
	public class WindowComponentEditor : Editor, IPreviewEditor {
		
		public void OnEnable() {
		}

		public void OnDisable() {
		}

		public override bool HasPreviewGUI() {

			return true;

		}

		public override void OnPreviewGUI(Rect rect, GUIStyle style) {

		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style) {

		}

		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo, selectable, hovered: false, selectedElement: null, onSelection: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, bool hovered) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo, selectable, hovered, selectedElement: null, onSelection: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable, WindowLayoutElement selectedElement) {
			
			this.OnPreviewGUI(color, r, background, drawInfo, selectable, hovered: false, selectedElement: selectedElement, onSelection: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, WindowLayoutElement selectedElement, System.Action<WindowLayoutElement> onSelection, List<WindowLayoutElement> highlighted) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo: true, selectable: true, hovered: false, selectedElement: selectedElement, onSelection: onSelection, highlighted: highlighted);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, bool hovered, WindowLayoutElement selectedElement, System.Action<WindowLayoutElement> onSelection, List<WindowLayoutElement> highlighted) {

			var _target = this.target as WindowComponent;

			this.DrawComponent(_target, _target.name, rect, style);
			/*
			var components = _target.GetComponentsInChildren<WindowComponent>(true);
			var xCount = Mathf.CeilToInt(Mathf.Sqrt(components.Length));
			if (xCount == 0) xCount = 1;
			var yCount = components.Length / xCount;

			var x = 0;
			var y = 0;
			foreach (var comp in components) {

				//if (comp == this) continue;

				var c = comp;

				if (comp is LinkerComponent) {

					c = (comp as LinkerComponent).prefab;

				}

				var xOffset = (r.width / xCount) * x;
				var yOffset = (r.height / yCount) * y;

				this.DrawComponent(c, c.name, new Rect(r.x + xOffset, r.y + yOffset, r.width / xCount, r.height / yCount), background);

				++x;
				
				if (xCount == x) {

					x = 0;
					++y;

				}

			}*/

		}

		private void DrawComponent(WindowComponent comp, string name, Rect r, GUIStyle background) {

			if (comp == null) return;
			
			if (Event.current.type != EventType.Layout) {

				comp.OnPreviewGUI(r, background);

			}

		}

	}

}