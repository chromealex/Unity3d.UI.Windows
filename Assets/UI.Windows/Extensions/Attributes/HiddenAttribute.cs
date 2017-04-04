using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.UI.Windows.Extensions;
using UnityEditor;
#endif

public class HiddenAttribute : ConditionAttribute {

	public HiddenAttribute() : base() {}
	public HiddenAttribute(string fieldName, object state = null, bool bitMask = false, bool inverseCondition = false) : base(fieldName, state, bitMask, inverseCondition) {}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HiddenAttribute))]
public class HiddenAttributeDrawer : PropertyDrawer {

	public static bool IsEnabled(PropertyDrawer drawer, SerializedProperty property) {
		
		return PropertyExtensions.IsEnabled<HiddenAttribute>(drawer, property);

	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		
		var state = HiddenAttributeDrawer.IsEnabled(this, property);
		if (state == true) {

			return EditorGUI.GetPropertyHeight(property, label, true) + 2f;

		}

		return 0f;

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var state = HiddenAttributeDrawer.IsEnabled(this, property);
		if (state == true) {

			EditorGUI.PropertyField(position, property, label, true);

		}

	}

}
#endif