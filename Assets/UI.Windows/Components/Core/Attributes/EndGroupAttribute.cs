using UnityEngine;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows {

	public class EndGroupAttribute : PropertyAttribute {

	}

}

#if UNITY_EDITOR
namespace UnityEditor.UI.Windows {

	[CustomPropertyDrawer(typeof(EndGroupAttribute))]
	public class EndGroupProperty : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			
			return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			EditorGUI.PropertyField(position, property, label, includeChildren: true);

			CustomGUI.Splitter();

			EditorGUI.EndDisabledGroup();
			--EditorGUI.indentLevel;

		}

	}

}
#endif