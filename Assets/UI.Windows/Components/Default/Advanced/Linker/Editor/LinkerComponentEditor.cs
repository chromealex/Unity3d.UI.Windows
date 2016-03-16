using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows.Components {

	[CustomEditor(typeof(LinkerComponent))]
	public class LinkerComponentEditor : WindowComponentBaseEditor {
		
		private SerializedProperty prefab;
		private SerializedProperty prefabParameters;
		private IParametersEditor editor;
		private bool foldout;

		private WindowComponent oldComponent;

		public void OnEnable() {
			
			this.prefab = this.serializedObject.FindProperty("prefab");
			this.prefabParameters = this.serializedObject.FindProperty("prefabParameters");

			this.oldComponent = this.prefab.objectReferenceValue as WindowComponent;

		}

		public override void OnInspectorGUI() {

			var offset = 2f;

			//this.DrawDefaultInspector();
			base.OnInspectorGUI();

			var newComponent = this.prefab.objectReferenceValue as WindowComponent;
			if (this.oldComponent != newComponent) {
				
				this.prefabParameters.objectReferenceValue = Layout.AddParametersFor((this.target as Component).gameObject, newComponent);
				this.prefabParameters.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				this.editor = null;
				
			}

			this.serializedObject.Update();

			if (this.prefab.objectReferenceValue != null) {

				if (this.editor == null) {

					this.editor = Editor.CreateEditor(this.prefabParameters.objectReferenceValue) as IParametersEditor;

				}
				
				if (this.editor != null) {
					
					CustomGUI.Splitter();

					var title = "Parameters";
					++EditorGUI.indentLevel;
					this.foldout = EditorGUILayout.Foldout(this.foldout, new GUIContent(title));
					var rect = GUILayoutUtility.GetLastRect();
					--EditorGUI.indentLevel;

					if (this.foldout == true) {

						rect.y += rect.height;
						rect.height = this.editor.GetHeight();
						GUILayout.Space(rect.height);

						var height = 16f;
						rect.height = height - offset;
						++EditorGUI.indentLevel;
						this.editor.OnParametersGUI(rect);
						--EditorGUI.indentLevel;

					}
					
				}

			}

			this.serializedObject.ApplyModifiedProperties();

			this.oldComponent = newComponent;

		}

	}

}