using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReadOnlyAttribute : PropertyAttribute {

	public readonly string header;
	public readonly bool drawHeaderOnly;

	public ReadOnlyAttribute() {

		this.header = null;
		this.drawHeaderOnly = false;

	}
	
	public ReadOnlyAttribute(string header, bool drawHeaderOnly = false) {

		this.header = header;
		this.drawHeaderOnly = drawHeaderOnly;

	}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		
		var attribute = this.attribute as ReadOnlyAttribute;
		if (string.IsNullOrEmpty(attribute.header) == false) {

			if (attribute.drawHeaderOnly == true) {

				return 1f;

			} else {

				return 5f;

			}

		}

		return EditorGUI.GetPropertyHeight(property, label, true);

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var attribute = this.attribute as ReadOnlyAttribute;
		if (string.IsNullOrEmpty(attribute.header) == true) {
			
			var oldState = GUI.enabled;
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = oldState;

		} else {

			var oldValue = new GUIContent(label);
			EditorGUILayout.LabelField(attribute.header, EditorStyles.boldLabel);

			if (attribute.drawHeaderOnly == false) {

				var oldState = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.PropertyField(property, oldValue, true);
				GUI.enabled = oldState;

			}

		}

	}

}
#endif