using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class BitmaskBaseAttribute : PropertyAttribute {
	
	public readonly bool bitMask;
	
	public BitmaskBaseAttribute(bool bitMask) {
		
		this.bitMask = bitMask;
		
	}
	
}

public class ConditionAttribute : BitmaskBaseAttribute {
	
	public readonly string fieldName;
	public readonly object state;
	public readonly bool inverseCondition;
	
	public ConditionAttribute() : base(false) {
		
		this.fieldName = null;
		this.state = false;
		this.inverseCondition = false;
		
	}
	
	public ConditionAttribute(string fieldName, object state = null, bool bitMask = false, bool inverseCondition = false) : base(bitMask) {
		
		this.fieldName = fieldName;
		this.state = state;
		this.inverseCondition = inverseCondition;
		
	}

}

namespace UnityEditor.UI.Windows.Extensions {

	#if UNITY_EDITOR
	public static class PropertyExtensions {
		
		public static bool IsEnabled<T>(PropertyDrawer drawer, SerializedProperty property) where T : ConditionAttribute {
			
			var state = false;

			var attribute = drawer.GetAttribute<T>();
			if (attribute != null) {

				if (string.IsNullOrEmpty(attribute.fieldName) == false) {
					
					var bitMask = attribute.bitMask;
					
					var inverseCondition = attribute.inverseCondition;
					var needState = attribute.state;
					var prop = PropertyExtensions.GetRelativeProperty(property, property.propertyPath, attribute.fieldName);
					
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

			} else {
				
				state = true;
				
			}
			
			return state;
			
		}

		public static string GetPathWithoutArray(this string path, string name) {

			var search = ".Array." + name;

			if (path.Contains(search) == true) {

				var splitted = path.Split(new string[] { search }, StringSplitOptions.RemoveEmptyEntries);
				path = splitted[0];

			}

			return path;
			
		}

		public static T GetAttribute<T>(this PropertyDrawer drawer) where T : PropertyAttribute {

			var attrs = drawer.fieldInfo.GetCustomAttributes(inherit: true);
			foreach (var attr in attrs) {

				if (attr is T) return attr as T;

			}

			return default(T);

		}
		
		public static SerializedProperty GetRelativeProperty(SerializedProperty property, string path, string fieldName) {
			
			var splitted = path.Split('.');
			if (splitted.Length > 1) {

				if (splitted.Length > 3 && splitted[splitted.Length - 2] == "Array" && splitted[splitted.Length - 1].Contains("[") == true) {
					
					path = string.Join(".", splitted, 0, splitted.Length - 3) + "." + fieldName;
					
				} else {
					
					path = string.Join(".", splitted, 0, splitted.Length - 1) + "." + fieldName;
					
				}
				
			} else {
				
				path = fieldName;
				
			}

			//path = path.GetPathWithoutArray(property.name);
			
			return property.serializedObject.FindProperty(path);
			
		}
		/*
		public static SerializedProperty GetRelativeProperty(this SerializedProperty property, string path, string fieldName) {
			
			var splitted = path.Split('.');
			if (splitted.Length > 1) {

				if (splitted.Length > 2 && splitted[splitted.Length - 1] == "Array") {

					path = string.Join(".", splitted, 0, splitted.Length - 2) + "." + fieldName;

				} else {

					path = string.Join(".", splitted, 0, splitted.Length - 1) + "." + fieldName;
					
				}

			} else {
				
				path = fieldName;
				
			}

			Debug.Log(path + " :: " + fieldName);
			//path = path.GetPathWithoutArray(property.name);

			return property.serializedObject.FindProperty(path);
			
		}*/
		
		public static object GetRawValue(this SerializedProperty thisSP, BitmaskBaseAttribute attribute) {
			
			if (thisSP == null) return null;
			
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
					
					if (attribute.bitMask == true) {
						
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

}