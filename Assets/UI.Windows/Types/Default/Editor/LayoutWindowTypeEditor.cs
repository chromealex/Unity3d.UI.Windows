using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using ME;
using System.Collections.Generic;

namespace UnityEditor.UI.Windows.Types {
	
	[CustomEditor(typeof(LayoutWindowType), true)]
	public class LayoutWindowTypeEditor : WindowBaseInspector, IPreviewEditor {
		
		public void OnEnable() {
		}

		public void OnDisable() {}

		public override bool HasPreviewGUI() {
			
			return true;
			
		}

		public override void OnPreviewGUI(Rect rect, GUIStyle style) {
			
			this.OnPreviewGUI(Color.white, rect, style, drawInfo: true, selectable: false, hovered: false, selectedElement: null, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo: true, selectable: false, hovered: false, selectedElement: null, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo, selectable, hovered: false, selectedElement: null, onSelection: null, onElementGUI: null, highlighted: null);
			
		}

		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, bool hovered) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo, selectable, hovered, selectedElement: null, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable, WindowLayoutElement selectedElement) {
			
			this.OnPreviewGUI(color, r, background, drawInfo, selectable, hovered: false, selectedElement: selectedElement, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, WindowLayoutElement selectedElement, System.Action<WindowLayoutElement> onSelection, List<WindowLayoutElement> highlighted) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo: true, selectable: true, hovered: false, selectedElement: selectedElement, onSelection: onSelection, onElementGUI: null, highlighted: highlighted);
			
		}

		private Layout.Component selectedComponent;
		private WindowLayoutEditor layoutEditor;
		private WindowLayout layoutCache;
		public void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable, bool hovered, WindowLayoutElement selectedElement, System.Action<WindowLayoutElement> onSelection, System.Action<WindowLayoutElement, Rect, bool> onElementGUI, List<WindowLayoutElement> highlighted) {

			var _target = this.target as LayoutWindowType;
			var layout = _target.GetCurrentLayout().layout;
			var layoutElements = _target.GetCurrentLayout().components;

			if (layout != null) {

				if (this.layoutEditor == null || this.layoutCache != layout) this.layoutEditor = Editor.CreateEditor(layout) as WindowLayoutEditor;
				this.layoutCache = layout;
				if (this.layoutEditor != null) {

					//var emptyStyle = GUIStyle.none;

					if (Event.current.type != EventType.Layout) {

						this.layoutEditor.OnPreviewGUI(color, r, background, drawInfo, selectable: selectable, hovered: hovered, selectedElement: selectedElement, onSelection: onSelection, onElementGUI: (element, elementRect, isClicked) => {

							if (isClicked == true) {

								var tag = element.tag;
								var comp = layoutElements.FirstOrDefault((e) => e.tag == tag);
								if (comp != null) {

									this.selectedComponent = comp;

								}

							}

							/*var tag = element.tag;
							var comp = layoutElements.FirstOrDefault((e) => e.tag == tag);
							if (comp != null) {

								comp.OnPreviewGUI(elementRect, emptyStyle);

							}*/

						}, highlighted: highlighted);

					}

				}

			}

		}

		public override void OnPreviewSettings() {

			base.OnPreviewSettings();

			var comp = this.selectedComponent;
			if (comp != null && comp.tag != LayoutTag.None) {

				EditorGUIUtility.labelWidth = 100f;

				comp.component = EditorGUILayout.ObjectField(comp.description.ToLower().UppercaseWords(), comp.component, typeof(WindowComponent), false) as WindowComponent;

				if (GUILayout.Button("...") == true) {
					
					WindowComponentLibraryChooser.Show((element) => {
						
						comp.component = element.mainComponent;
						EditorUtility.SetDirty(comp.component);
						
					});
					
				}

				EditorGUIUtilityExt.LookLikeControls();

			}

		}

	}
	
}