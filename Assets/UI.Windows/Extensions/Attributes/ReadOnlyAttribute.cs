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

		var h = EditorGUI.GetPropertyHeight(property, label, true) + 2f;

		var attribute = this.attribute as ReadOnlyAttribute;
		if (string.IsNullOrEmpty(attribute.header) == false) {

			if (attribute.drawHeaderOnly == true) {

				return h;

			} else {

				return h * 2f;

			}

		}

		return h;

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var attribute = this.attribute as ReadOnlyAttribute;
		if (string.IsNullOrEmpty(attribute.header) == true) {
			
			var oldState = GUI.enabled;
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = oldState;

		} else {

			var posHeader = position;
			var posContent = new Rect(posHeader.x, posHeader.y + posHeader.height * 0.5f, posHeader.width, posHeader.height * 0.5f);

			var oldValue = new GUIContent(label);
			EditorGUI.LabelField(posHeader, attribute.header, EditorStyles.boldLabel);

			if (attribute.drawHeaderOnly == false) {

				var oldState = GUI.enabled;
				GUI.enabled = false;
				EditorGUI.PropertyField(posContent, property, oldValue, true);
				GUI.enabled = oldState;

			}

		}

	}

}
#endif