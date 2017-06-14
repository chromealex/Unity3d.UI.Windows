using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI.Windows.Hierarchy;

[CustomPropertyDrawer(typeof(SceneEditOnlyAttribute))]
public class SceneEditOnlyDrawer : PropertyDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		return EditorGUI.GetPropertyHeight(property, label, true);

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		
		const float sizeX = 76f;
		const float sizeY = 16f;

		var indent = EditorGUI.indentLevel * 15f;
		var propertyPosition = new Rect(position.x, position.y, position.width - sizeX, position.height);
		var labelPosition = new Rect(propertyPosition.x + propertyPosition.width - indent, propertyPosition.y, sizeX + indent, propertyPosition.height);

		if (ME.EditorUtilities.IsPrefab((property.serializedObject.targetObject as Component).gameObject) == true) {

			var oldState = GUI.enabled;
			GUI.enabled = false;
			EditorGUI.PropertyField(propertyPosition, property, label, true);
			GUI.enabled = oldState;

		} else {

			EditorGUI.PropertyField(propertyPosition, property, label, true);

		}

		HierarchyEditor.DrawLabel(new Rect(labelPosition.x + labelPosition.width - sizeX, labelPosition.y, sizeX, sizeY), HierarchyEditor.colors.layouts, "SCENE EDIT");

	}

}