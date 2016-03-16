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
	/*public class WindowComponentEditor : Editor, IPreviewEditor {
		
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

		}

		private void DrawComponent(WindowComponent comp, string name, Rect r, GUIStyle background) {

			if (comp == null) return;
			
			if (Event.current.type != EventType.Layout) {

				comp.OnPreviewGUI(r, background);

			}

		}

	}*/

	[CustomEditor(typeof(WindowComponentBase), true)]
	public class WindowComponentBaseEditor : Editor {
		
		public override void OnInspectorGUI() {
			
			this.serializedObject.Update();
			
			var so = this.serializedObject;
			var comp = so.targetObject as WindowComponentBase;
			var isPrefab = ME.EditorUtilities.IsPrefab(comp.gameObject);

			var miniLabelStyle = ME.Utilities.CacheStyle("WindowBaseInspector", "MiniLabel", (name) => {
				
				var style = new GUIStyle(EditorStyles.miniLabel);
				//style.normal.textColor = Color.grey;
				style.fixedHeight = 16f;
				style.stretchHeight = false;
				
				return style;
				
			});

			var backStyle = ME.Utilities.CacheStyle("WindowBaseInspector", "BoxStyle", (name) => {
				
				var style = new GUIStyle("ChannelStripAttenuationBar");
				style.border = new RectOffset(0, 0, 0, 4);
				style.fixedHeight = 0f;
				
				return style;
				
			});

			var state = so.FindProperty("currentState");
			var manualShowHideControl = so.FindProperty("manualShowHideControl");

			if (state != null && manualShowHideControl != null) {
				
				GUILayout.BeginHorizontal(backStyle);
				{

					GUILayout.BeginVertical();
					{
						GUILayout.Label("State", miniLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
						GUILayout.Label(state.enumNames[state.enumValueIndex], isPrefab == true ? EditorStyles.label : EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
					}
					GUILayout.EndVertical();
					
					GUILayout.BeginVertical();
					{
						GUILayout.Label("Manual Show/Hide", miniLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
						GUILayout.Label(manualShowHideControl.boolValue.ToString(), isPrefab == true ? EditorStyles.label : EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
					}
					GUILayout.EndVertical();

				}
				GUILayout.EndHorizontal();
				
			}

			ME.EditorUtilitiesEx.DrawInspector(this, typeof(WindowComponentBase), new List<string>() { "WindowComponentBase" });

			// Draw default
			//this.DrawDefaultInspector();
			
			this.serializedObject.ApplyModifiedProperties();
			
		}

	}
}