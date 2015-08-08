using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

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

		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			
			//this.OnPreviewGUI(Color.white, r, background);
			
		}
		
		public virtual void OnPreviewGUI(Color color, Rect r, GUIStyle background) {
			
			//this.OnPreviewGUI(color, r, background, false, false);
			
		}

		public virtual void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable) {

			this.OnPreviewGUI(color, r, background, drawInfo, selectable, hovered: false);

		}

		public void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable, bool hovered) {

			var _target = this.target as WindowComponent;

			this.DrawComponent(_target, _target.name, r, background);
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