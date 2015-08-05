using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneEditOnlyAttribute : PropertyAttribute {

	public SceneEditOnlyAttribute() {

	}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneEditOnlyAttribute))]
public class SceneEditOnlyDrawer : PropertyDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		return EditorGUI.GetPropertyHeight(property, label, true);

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		if (ME.EditorUtilities.IsPrefab((property.serializedObject.targetObject as Component).gameObject) == true) {

			var oldState = GUI.enabled;
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = oldState;

		} else {

			EditorGUI.PropertyField(position, property, label, true);

		}

	}

}
#endif