using UnityEngine;
using System.Collections;
using UnityEditor;
using ME;

namespace ME {

	[CustomEditor(typeof(GridLayoutGroup))]
	public class GridLayoutGroupEditor : UnityEditor.UI.GridLayoutGroupEditor {

		public override void OnInspectorGUI() {

			base.OnInspectorGUI();

			//this.DrawDefaultInspector();
			var stretchWidth = this.serializedObject.FindProperty("stretchWidth");
			var stretchHeight = this.serializedObject.FindProperty("stretchHeight");
			EditorGUILayout.PropertyField(stretchWidth);
			EditorGUILayout.PropertyField(stretchHeight);

		}

	}

}