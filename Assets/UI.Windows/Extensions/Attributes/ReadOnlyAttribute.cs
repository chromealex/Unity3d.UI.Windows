using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.UI.Windows.Extensions;
using UnityEditor;
#endif

public class ReadOnlyAttribute : ConditionAttribute {
	
	public ReadOnlyAttribute() : base() {}
	public ReadOnlyAttribute(string fieldName, object state = null, bool bitMask = false, bool inverseCondition = false) : base(fieldName, state, bitMask, inverseCondition) {}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyAttributeDrawer : PropertyDrawer {

	public static bool IsEnabled(PropertyDrawer drawer, SerializedProperty property) {

		return PropertyExtensions.IsEnabled<ReadOnlyAttribute>(drawer, property);

	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		return EditorGUI.GetPropertyHeight(property, label, true) + 2f;

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var state = ReadOnlyAttributeDrawer.IsEnabled(this, property);

		var oldState = GUI.enabled;
		GUI.enabled = state && oldState;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = oldState;

	}

}
#endif

public class ReadOnlyBeginGroupAttribute : ConditionAttribute {

	public ReadOnlyBeginGroupAttribute() : base() {}
	public ReadOnlyBeginGroupAttribute(string fieldName, object state = null, bool bitMask = false, bool inverseCondition = false) : base(fieldName, state, bitMask, inverseCondition) {}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyBeginGroupAttribute))]
public class ReadOnlyBeginGroupAttributeDrawer : PropertyDrawer {

	public static bool IsEnabled(PropertyDrawer drawer, SerializedProperty property) {

		return PropertyExtensions.IsEnabled<ReadOnlyBeginGroupAttribute>(drawer, property);

	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		return EditorGUI.GetPropertyHeight(property, label, true) + 2f;

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		EditorGUI.PropertyField(position, property, label, true);

		var state = ReadOnlyBeginGroupAttributeDrawer.IsEnabled(this, property);

		EditorGUI.BeginDisabledGroup(!state);

	}

}
#endif

public class ReadOnlyEndGroupAttribute : ConditionAttribute {

	public ReadOnlyEndGroupAttribute() : base() {}
	public ReadOnlyEndGroupAttribute(string fieldName, object state = null, bool bitMask = false, bool inverseCondition = false) : base(fieldName, state, bitMask, inverseCondition) {}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyEndGroupAttribute))]
public class ReadOnlyEndGroupAttributeDrawer : PropertyDrawer {
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		return EditorGUI.GetPropertyHeight(property, label, true) + 2f;

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		EditorGUI.PropertyField(position, property, label, true);

		EditorGUI.EndDisabledGroup();

	}

}
#endif