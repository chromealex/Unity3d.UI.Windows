using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ME {

	[CustomEditor(typeof(ScriptableObject), editorForChildClasses: true)]
	public class EditorForAllScriptableObjects : Editor {

		public override void OnInspectorGUI() {

			EditorUtilitiesEx.DrawInspector(this, typeof(ScriptableObject));

		}

	}

}