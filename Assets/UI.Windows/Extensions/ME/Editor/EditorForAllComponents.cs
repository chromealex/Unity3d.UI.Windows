using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ME {

	[CustomEditor(typeof(Component), editorForChildClasses: true)]
	public class EditorForAllComponents : Editor {

		public override void OnInspectorGUI() {

			EditorUtilitiesEx.DrawInspector(this, typeof(Component));

		}

	}

}