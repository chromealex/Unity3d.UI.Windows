using UnityEngine;
using System;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReadOnlyAttribute : PropertyAttribute {

	public readonly string fieldName;
	public readonly object state;
	public readonly bool bitMask;

	public ReadOnlyAttribute() {

		this.fieldName = null;
		this.state = false;
		this.bitMask = false;

	}

	public ReadOnlyAttribute(string fieldName, object state = null, bool bitMask = false) {
		
		this.fieldName = fieldName;
		this.state = state;
		this.bitMask = bitMask;
		
	}

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

		return EditorGUI.GetPropertyHeight(property, label, true) + 2f;

	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		var state = false;

		var attribute = this.attribute as ReadOnlyAttribute;
		if (string.IsNullOrEmpty(attribute.fieldName) == false) {
			
			var bitMask = (this.attribute as ReadOnlyAttribute).bitMask;
			
			var needState = attribute.state;
			var prop = this.GetRelativeProperty(property, property.propertyPath, attribute.fieldName);

			var value = this.GetRawValue(prop);
			if (bitMask == true) {
				
				state = true;
				if (((int)value & (int)needState) == 0) state = false;

			} else {

				state = true;
				if (object.Equals(needState, value) == false) state = false;

			}

		}

		var oldState = GUI.enabled;
		GUI.enabled = state;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = oldState;

	}

	private SerializedProperty GetRelativeProperty(SerializedProperty property, string path, string fieldName) {

		var splitted = path.Split('.');
		if (splitted.Length > 1) {

			path = string.Join(".", splitted, 0, splitted.Length - 1) + "." + fieldName;

		} else {

			path = fieldName;

		}

		return property.serializedObject.FindProperty(path);

	}

	public object GetRawValue(SerializedProperty thisSP) {

		switch (thisSP.propertyType) {
			case SerializedPropertyType.Integer:
				return thisSP.intValue;
			case SerializedPropertyType.Boolean:
				return thisSP.boolValue;
			case SerializedPropertyType.Float:
				return thisSP.floatValue;
			case SerializedPropertyType.String:
				return thisSP.stringValue;
			case SerializedPropertyType.Color:
				return thisSP.colorValue;
			case SerializedPropertyType.ObjectReference:
				return thisSP.objectReferenceValue;
			case SerializedPropertyType.LayerMask:
				return thisSP.intValue;
			case SerializedPropertyType.Enum:

				if ((this.attribute as ReadOnlyAttribute).bitMask == true) {

					return thisSP.intValue;

				} else {

					int enumI = thisSP.enumValueIndex;
					return new KeyValuePair<int, string>(enumI, thisSP.enumNames[enumI]);

				}

			case SerializedPropertyType.Vector2:
				return thisSP.vector2Value;
			case SerializedPropertyType.Vector3:
				return thisSP.vector3Value;
			case SerializedPropertyType.Rect:
				return thisSP.rectValue;
			case SerializedPropertyType.ArraySize:
				return thisSP.intValue;
			case SerializedPropertyType.Character:
				return (char)thisSP.intValue;
			case SerializedPropertyType.AnimationCurve:
				return thisSP.animationCurveValue;
			case SerializedPropertyType.Bounds:
				return thisSP.boundsValue;
			case SerializedPropertyType.Quaternion:
				return thisSP.quaternionValue;
				
			default:
				throw new NotImplementedException("Unimplemented propertyType "+thisSP.propertyType+".");

		}

	}

}
#endif