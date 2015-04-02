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

namespace UnityEditor.UI.Windows.Types {
	
	[CustomEditor(typeof(LayoutWindowType), true)]
	public class LayoutWindowTypeEditor : Editor, IPreviewEditor {

		public override void OnInspectorGUI() {

			this.DrawDefaultInspector();

		}

		public override bool HasPreviewGUI() {
			
			return true;
			
		}
		
		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			
			this.OnPreviewGUI(Color.white, r, background);
			
		}
		
		public virtual void OnPreviewGUI(Color color, Rect r, GUIStyle background) {
			
			this.OnPreviewGUI(color, r, background, false, false);
			
		}

		private Layout.Component selectedComponent;
		private WindowLayoutEditor layoutEditor;
		public virtual void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable) {

			var _target = this.target as LayoutWindowType;
			var layout = _target.layout.layout;
			var layoutElements = _target.layout.components;

			if (layout != null) {

				if (this.layoutEditor == null) this.layoutEditor = Editor.CreateEditor(layout) as WindowLayoutEditor;
				if (this.layoutEditor != null) {

					//var emptyStyle = GUIStyle.none;

					if (Event.current.type != EventType.Layout) {

						this.layoutEditor.OnPreviewGUI(color, r, background, drawInfo, true, (element, elementRect, isClicked) => {

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

						});

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

				EditorGUIUtility.LookLikeControls();

			}

		}

	}
	
}