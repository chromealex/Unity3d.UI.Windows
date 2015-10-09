using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.UI.Windows.Extensions;
using UnityEditor;
#endif

public class ReadOnlyAttribute : BitmaskBaseAttribute {
	
	public readonly object state;
	public readonly string fieldName;
	public readonly bool inverseCondition;
	
	public ReadOnlyAttribute() : base(false) {
		
		this.fieldName = null;
		this.state = false;
		this.inverseCondition = false;
		
	}
	
	public ReadOnlyAttribute(string fieldName, object state = null, bool bitMask = false, bool inverseCondition = false) : base(bitMask) {
		
		this.fieldName = fieldName;
		this.state = state;
		this.inverseCondition = inverseCondition;
		
	}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyAttributeDrawer : PropertyDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		return EditorGUI.GetPropertyHeight(property, label, true) + 2f;

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var state = false;

		var attribute = this.GetAttribute<ReadOnlyAttribute>();
		if (string.IsNullOrEmpty(attribute.fieldName) == false) {
			
			var bitMask = (this.attribute as ReadOnlyAttribute).bitMask;
			
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

		var oldState = GUI.enabled;
		GUI.enabled = state;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = oldState;

	}

}
#endif