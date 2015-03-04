using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReadOnlyAttribute : PropertyAttribute {

	public readonly string header;

	public ReadOnlyAttribute() {

		this.header = null;

	}
	
	public ReadOnlyAttribute(string header) {

		this.header = header;

	}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		
		var attribute = this.attribute as ReadOnlyAttribute;
		if (string.IsNullOrEmpty(attribute.header) == false) {

			return 5f;

		}

		return EditorGUI.GetPropertyHeight(property, label, true);

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var attribute = this.attribute as ReadOnlyAttribute;
		if (string.IsNullOrEmpty(attribute.header) == true) {

			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;

		} else {

			var oldValue = new GUIContent(label);
			EditorGUILayout.LabelField(attribute.header, EditorStyles.boldLabel);

			GUI.enabled = false;
			EditorGUILayout.PropertyField(property, oldValue, true);
			GUI.enabled = true;

		}

	}

}
#endif