using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.UI.Windows.Extensions;
using UnityEditor;
#endif

public class HiddenAttribute : BitmaskBaseAttribute {

	public readonly string fieldName;
	public readonly object state;
	public readonly bool inverseCondition;

	public HiddenAttribute() : base(false) {

		this.fieldName = null;
		this.state = false;
		this.inverseCondition = false;

	}

	public HiddenAttribute(string fieldName, object state = null, bool bitMask = false, bool inverseCondition = false) : base(bitMask) {
		
		this.fieldName = fieldName;
		this.state = state;
		this.inverseCondition = inverseCondition;
		
	}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HiddenAttribute))]
public class HiddenAttributeDrawer : PropertyDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		return EditorGUI.GetPropertyHeight(property, label, true) + 2f;

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var state = false;
		
		var attribute = this.GetAttribute<HiddenAttribute>();
		if (string.IsNullOrEmpty(attribute.fieldName) == false) {
			
			var bitMask = attribute.bitMask;

			var inverseCondition = attribute.inverseCondition;
			var needState = attribute.state;
			var prop = property.GetRelativeProperty(property.propertyPath, attribute.fieldName);

			var value = prop.GetRawValue(attribute);
			if (bitMask == true) {
				
				state = true;
				if (inverseCondition == true) {
					
					if (((int)value & (int)needState) != 0) state = false;

				} else {

					if (((int)value & (int)needState) == 0) state = false;

				}

			} else {

				state = true;
				if (object.Equals(needState, value) == !inverseCondition) state = false;

			}

		}

		if (state == true) {

			EditorGUI.PropertyField(position, property, label, true);

		}

	}

}
#endif