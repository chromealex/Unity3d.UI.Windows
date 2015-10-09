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

namespace UnityEditor.UI.Windows.Extensions {

	#if UNITY_EDITOR
	public static class PropertyExtensions {

		public static T GetAttribute<T>(this PropertyDrawer drawer) where T : PropertyAttribute {

			var attrs = drawer.fieldInfo.GetCustomAttributes(inherit: true);
			foreach (var attr in attrs) {

				if (attr is T) return attr as T;

			}

			return default(T);

		}

		public static SerializedProperty GetRelativeProperty(this SerializedProperty property, string path, string fieldName) {
			
			var splitted = path.Split('.');
			if (splitted.Length > 1) {
				
				path = string.Join(".", splitted, 0, splitted.Length - 1) + "." + fieldName;
				
			} else {
				
				path = fieldName;
				
			}
			
			return property.serializedObject.FindProperty(path);
			
		}
		
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