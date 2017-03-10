using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof (MinMaxSliderAttribute))]
class MinMaxSliderDrawer : ReadOnlyAttributeDrawer {

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

		var state = ReadOnlyAttributeDrawer.IsEnabled(this, property);
		var oldState = GUI.enabled;
		GUI.enabled = state && oldState;

		if (property.propertyType == SerializedPropertyType.Vector2) {
			Vector2 range = property.vector2Value;
			float min = range.x;
			float max = range.y;
			MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
			EditorGUI.BeginChangeCheck ();
			EditorGUI.MinMaxSlider(label, position, ref min, ref max, attr.min, attr.max);
			if (EditorGUI.EndChangeCheck ()) {
				range.x = min;
				range.y = max;
				property.vector2Value = range;
			}
		} else {
			EditorGUI.LabelField (position, label, "Use only with Vector2");
		}

		GUI.enabled = oldState;

	}
}