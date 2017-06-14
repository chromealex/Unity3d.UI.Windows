using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEditor.UI.Windows.Hierarchy;
using UnityEngine.UI.Windows.Components;
using UnityEditor.UI.Windows.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.UI.Windows {

	[CustomPropertyDrawer(typeof(ResourceMono))]
	public class ResourceMonoEditor : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return base.GetPropertyHeight(property, label);

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			const float iconSizeX = 30f;
			const float asyncSizeX = 58f;
			const float asyncSizeY = 16f;
			const float asyncToggleSizeX = 18f;

			var propertyPosition = new Rect(position.x, position.y, position.width - iconSizeX - asyncSizeX + 30f, position.height);

			var asyncRectLabel = new Rect(position.x + propertyPosition.width, position.y, asyncSizeX - asyncToggleSizeX, asyncSizeY);
			var asyncRect = new Rect(asyncRectLabel.x + asyncRectLabel.width, position.y, asyncToggleSizeX, asyncSizeY);

			var tempObject = property.FindPropertyRelative("tempResource");
			var oldObj = tempObject.objectReferenceValue;
			var newObj = EditorGUI.ObjectField(propertyPosition, label, oldObj, typeof(MonoBehaviour), allowSceneObjects: false);
			if (oldObj != newObj) {

				var obj = PropertyExtensions.GetTargetObjectOfProperty(property);
				var res = obj as ResourceMono;
				if (res != null) {
						
					if (newObj == null) {

						res.ResetToDefault();

					} else {

						res.Validate(newObj);

					}

					property.serializedObject.ApplyModifiedProperties();

				} else {

					Debug.LogWarning("ResourceBase couldn't be found");

				}

			}

			EditorGUI.BeginDisabledGroup(newObj == null);

			var asyncToggle = property.FindPropertyRelative("async");
			if (GUI.Button(asyncRectLabel, new GUIContent("Async", "Loads Object Asynchronously"), EditorStyles.miniButtonLeft) == true) {

				asyncToggle.boolValue = !asyncToggle.boolValue;
				property.serializedObject.ApplyModifiedProperties();

			}

			var oldColor = GUI.color;
			GUI.color = (asyncToggle.boolValue == false ? oldColor : Color.green);
			asyncToggle.boolValue = GUI.Toggle(asyncRect, asyncToggle.boolValue, string.Empty, EditorStyles.miniButtonRight);
			GUI.color = oldColor;

			EditorGUI.EndDisabledGroup();

		}

	}

}