using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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
					if (prop != null) {

						var value = PropertyExtensions.GetRawValue(prop, attribute);
						if (bitMask == true) {

							var result = 0;
							if (needState is byte) {

								result = ((int)value & (byte)needState);

							} else if (needState is int) {

								result = ((int)value & (int)needState);

							}

							state = true;
							if (inverseCondition == true) {
								
								if (result != 0)
									state = false;
								
							} else {
								
								if (result == 0)
									state = false;
								
							}
							
						} else {
							
							state = true;
							if (object.Equals(needState, value) == !inverseCondition)
								state = false;
							
						}

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

				if (attr is T)
					return attr as T;

			}

			return default(T);

		}

		public static SerializedProperty GetRelativeProperty(SerializedProperty property, string path, string fieldName) {
			
			var splitted = path.Split('.');
			if (splitted.Length > 1) {

				if (splitted.Length > 3 && splitted[splitted.Length - 2] == "Array" && splitted[splitted.Length - 1].Contains("[") == true) {
					
					path = string.Join(".", splitted, 0, splitted.Length - 3) + "." + fieldName;
					
				} else {

					var fieldSplitted = fieldName.Split('<');
					var fromEnd = fieldSplitted.Length;

					fieldName = fieldSplitted[fieldSplitted.Length - 1];

					path = string.Join(".", splitted, 0, splitted.Length - fromEnd) + "." + fieldName;

				}
				
			} else {
				
				path = fieldName;
				
			}

			return property.serializedObject.FindProperty(path);
			
		}

		public static object GetRawValue(SerializedProperty thisSP, BitmaskBaseAttribute attribute) {
			
			if (thisSP == null)
				return null;
			
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
					throw new NotImplementedException("Unimplemented propertyType " + thisSP.propertyType + ".");
					
			}
			
		}

		public static object GetTargetObjectOfProperty(SerializedProperty prop) {
			//object rootObject = null;
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements) {
				//rootObject = obj;
				if (element.Contains("[")) {
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				} else {
					obj = GetValue_Imp(obj, element);
				}

			}
			return obj;
		}

		public static object GetTargetObjectOfProperty(SerializedProperty prop, out object rootObject) {
			rootObject = null;
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements) {
				rootObject = obj;
				if (element.Contains("[")) {
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				} else {
					obj = GetValue_Imp(obj, element);
				}

			}
			return obj;
		}

		private static object GetValue_Imp(object source, string name) {
			if (source == null)
				return null;
			var type = source.GetType();

			while (type != null) {
				var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (f != null)
					return f.GetValue(source);

				var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p != null)
					return p.GetValue(source, null);

				type = type.BaseType;
			}
			return null;
		}

		private static object GetValue_Imp(object source, string name, int index) {
			var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
			if (enumerable == null)
				return null;
			var enm = enumerable.GetEnumerator();
			//while (index-- >= 0)
			//    enm.MoveNext();
			//return enm.Current;

			for (int i = 0; i <= index; i++) {
				if (!enm.MoveNext())
					return null;
			}
			return enm.Current;
		}

	}
	#endif

}