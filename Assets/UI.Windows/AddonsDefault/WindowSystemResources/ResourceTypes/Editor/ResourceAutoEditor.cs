using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEditor.UI.Windows.Hierarchy;
using UnityEngine.UI.Windows.Components;
using UnityEditor.UI.Windows.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.UI.Windows {

	[CustomPropertyDrawer(typeof(ResourceAuto))]
	public class ResourceAutoEditor : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return base.GetPropertyHeight(property, label);

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			const float iconSizeX = 30f;
			const float iconSizeY = 16f;

			const float asyncSizeX = 58f;
			const float asyncSizeY = 16f;
			const float asyncToggleSizeX = 18f;

			var controlTypeSizeXMax = 140f;
			var controlTypeSizeX = 0f;

			var attr = PropertyExtensions.GetAttribute<ResourceParametersAttribute>(this);

			var controlType = property.FindPropertyRelative("controlType");
			var values = System.Enum.GetValues(typeof(UnityEngine.UI.Windows.ResourceAuto.ControlType));
			var names = System.Enum.GetNames(typeof(UnityEngine.UI.Windows.ResourceAuto.ControlType));

			var items = new List<byte>();
			var itemNames = new List<string>();

			if (attr != null) {

				for (int i = 0; i < values.Length; ++i) {

					var value = (byte)values.GetValue(i);
					if (value == 0) continue;

					if (((byte)attr.drawOnly & value) != 0) {

						controlTypeSizeX += controlTypeSizeXMax / values.Length;

						items.Add(value);
						itemNames.Add(names[i]);

					}

				}

			} else {

				for (int i = 0; i < values.Length; ++i) {

					var value = (byte)values.GetValue(i);
					if (value == 0) continue;

					controlTypeSizeX += controlTypeSizeXMax / values.Length;

					items.Add(value);
					itemNames.Add(names[i]);

				}

			}

			var propertyPosition = new Rect(position.x, position.y, position.width - iconSizeX - asyncSizeX - controlTypeSizeX + 30f, position.height);
			var labelPosition = new Rect(position.x + propertyPosition.width - iconSizeX - 18f, propertyPosition.y, iconSizeX, iconSizeY);

			var asyncRectLabel = new Rect(position.x + propertyPosition.width, position.y, asyncSizeX - asyncToggleSizeX, asyncSizeY);
			var asyncRect = new Rect(asyncRectLabel.x + asyncRectLabel.width, position.y, asyncToggleSizeX, asyncSizeY);

			var controlTypeRect = new Rect(asyncRect.x + asyncRect.width, position.y, controlTypeSizeX, asyncSizeY);

			var tempObject = property.FindPropertyRelative("tempObject");

			/*var _obj = PropertyExtensions.GetTargetObjectOfProperty(property);
			var _res = _obj as ResourceAuto;
			if (_res != null) {

				if (tempObject.objectReferenceValue == null) {

					_res.ResetToDefault();

				} else {

					_res.Validate(tempObject.objectReferenceValue);

				}

			}*/

			/*PropertyExtensions.GetTargetObjectsOfProperty<ResourceAuto>(property, (res) => {

				if (res != null) {

					if (tempObject.objectReferenceValue == null) {

						res.ResetToDefault();

					} else {

						res.Validate(tempObject.objectReferenceValue);

					}

				}

			});*/

			var oldObj = tempObject.objectReferenceValue;
			var newObj = EditorGUI.ObjectField(propertyPosition, label, oldObj, typeof(Object), allowSceneObjects: false);
			if (oldObj != newObj) {

				tempObject.objectReferenceValue = newObj;
				property.serializedObject.ApplyModifiedProperties();

				PropertyExtensions.GetTargetObjectsOfProperty<ResourceAuto>(property, (res) => {
					
					if (res != null) {
							
						if (newObj == null) {

							res.ResetToDefault();

						} else {

							res.Validate(newObj);

						}

					} else {

						Debug.LogWarning("ResourceBase cannot be found");

					}

				});

			}

			EditorGUI.BeginDisabledGroup(tempObject.objectReferenceValue == null);

			var asyncToggle = property.FindPropertyRelative("async");
			if (GUI.Button(asyncRectLabel, new GUIContent("Async", "Loads Object Asynchronously"), EditorStyles.miniButtonLeft) == true) {

				asyncToggle.boolValue = !asyncToggle.boolValue;

			}

			var oldColor = GUI.color;
			GUI.color = (asyncToggle.boolValue == false ? oldColor : Color.green);
			asyncToggle.boolValue = GUI.Toggle(asyncRect, asyncToggle.boolValue, string.Empty, EditorStyles.miniButtonRight);
			GUI.color = oldColor;

			EditorGUI.EndDisabledGroup();

			for (int i = 0; i < items.Count; ++i) {

				var k = i;

				var width = controlTypeRect.width / items.Count;
				var value = items[i];
				var enumValue = controlType.intValue;

				var isOn = (enumValue & value) != 0;
				isOn = GUI.Toggle(new Rect(controlTypeRect.x + (k * width), controlTypeRect.y, width, controlTypeRect.height), isOn, new GUIContent(itemNames[i], "Event when you want to initialize/deinitialize this resource"), (k == 0 ? EditorStyles.miniButtonLeft : (i == items.Count - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid)));

				if (isOn == true) {

					enumValue |= value;

				} else {

					enumValue &= ~value;

				}

				controlType.intValue = enumValue;

			}

			//controlType.enumValueIndex = (int)(UnityEngine.UI.Windows.ResourceAuto.ControlType)EditorGUI.EnumMaskField(controlTypeRect, (UnityEngine.UI.Windows.ResourceAuto.ControlType)(controlType.enumValueIndex >= 0 ? controlType.enumValueIndex : 0));

			HierarchyEditor.DrawLabel(labelPosition, HierarchyEditor.colors.transitions, "RES");

		}

	}

}