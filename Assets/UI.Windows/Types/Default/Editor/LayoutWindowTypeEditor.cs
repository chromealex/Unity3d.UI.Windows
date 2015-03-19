using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Linq;

namespace UnityEditor.UI.Windows {
	
	[CustomEditor(typeof(LayoutWindowType), true)]
	public class LayoutWindowTypeEditor : Editor, IPreviewEditor {
		
		public override bool HasPreviewGUI() {
			
			return true;
			
		}
		
		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			
			this.OnPreviewGUI(Color.white, r, background);
			
		}
		
		public virtual void OnPreviewGUI(Color color, Rect r, GUIStyle background) {
			
			this.OnPreviewGUI(color, r, background, false, false);
			
		}

		private WindowLayoutEditor layoutEditor;
		public virtual void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable) {

			var _target = this.target as LayoutWindowType;
			var layout = _target.layout.layout;
			//var layoutElements = _target.layout.components;

			if (layout != null) {

				if (this.layoutEditor == null) this.layoutEditor = Editor.CreateEditor(layout) as WindowLayoutEditor;
				if (this.layoutEditor != null) {

					//var emptyStyle = GUIStyle.none;

					this.layoutEditor.OnPreviewGUI(color, r, background, drawInfo, selectable, (element, elementRect) => {

						/*var tag = element.tag;
						var comp = layoutElements.FirstOrDefault((e) => e.tag == tag);
						if (comp != null) {

							comp.OnPreviewGUI(elementRect, emptyStyle);

						}*/

					});

				}

			}

		}

	}
	
}