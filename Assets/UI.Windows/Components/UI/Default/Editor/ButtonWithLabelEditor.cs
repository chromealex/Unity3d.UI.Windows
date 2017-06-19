using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityEditor.UI {

	[CanEditMultipleObjects()]
	[CustomEditor(typeof(ButtonWithLabel))]
	public class ButtonWithLabelEditor : ButtonWithReferenceEditor {
		
		private SerializedProperty label;
		private SerializedProperty labelColor;

		protected override void OnEnable() {
			
			base.OnEnable();
			
			this.label = base.serializedObject.FindProperty("label");
			this.labelColor = base.serializedObject.FindProperty("labelColor");
			
		}
		
		public override void OnInspectorGUI() {
			
			base.OnInspectorGUI();
			
			base.serializedObject.Update();
			
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.label);
			EditorGUILayout.PropertyField(this.labelColor);
			
			base.serializedObject.ApplyModifiedProperties();
			
		}

	}
	
	[CanEditMultipleObjects()]
	[CustomEditor(typeof(ButtonWithReference))]
	public class ButtonWithReferenceEditor : ButtonExtendedEditor {
		
		private SerializedProperty rootButton;
		private SerializedProperty referenceButton;

		protected override void OnEnable() {

			base.OnEnable();

			this.rootButton = base.serializedObject.FindProperty("rootButton");
			this.referenceButton = base.serializedObject.FindProperty("referenceButton");

		}

		public override void OnInspectorGUI() {
			
			base.OnInspectorGUI();
			
			base.serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.rootButton);
			EditorGUILayout.PropertyField(this.referenceButton);

			base.serializedObject.ApplyModifiedProperties();
			
		}
		
	}

}