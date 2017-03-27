using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Localization.UI;
using UnityEditor.UI.Windows.Hierarchy;

namespace UnityEditor.UI.Windows.Plugins.Localization.UI {
/*
	[CustomPropertyDrawer(typeof(Sprite))]
	public class ImageDrawer : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			var valid = false;
			var text = property.serializedObject.targetObject as UnityEngine.UI.Image;
			var locImage = property.serializedObject.targetObject as LocalizationImage;
			if (text != null && property.serializedObject.targetObject.GetType() == typeof(UnityEngine.UI.Image)) {

				valid = true;

			} else if (locImage != null && property.serializedObject.targetObject.GetType() == typeof(LocalizationImage)) {

				valid = true;

			}

			return valid ? 16f : 0f;

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			//property.serializedObject.Update();

			var text = property.serializedObject.targetObject as UnityEngine.UI.Image;
			var locImage = property.serializedObject.targetObject as LocalizationImage;
			if (text != null && property.serializedObject.targetObject.GetType() == typeof(UnityEngine.UI.Image)) {

				EditorGUILayout.PropertyField(property);

				var width = 120f;
				if (GUI.Button(new Rect(position.x + position.width - width, position.y, width, 16f), new GUIContent("Switch to Localized", "Replace Image component with LocalizationImage"), EditorStyles.miniButtonMid) == true) {
					
					FlowDatabase.ReplaceComponents<UnityEngine.UI.Image, LocalizationImage>(text, typeof(LocalizationImage));
					return;

				}

			} else if (locImage != null && property.serializedObject.targetObject.GetType() == typeof(LocalizationImage)) {

				EditorGUILayout.PropertyField(property);

				var width = 120f;
				if (GUI.Button(new Rect(position.x + position.width - width, position.y, width, 16f), new GUIContent("Switch to normal Image", "Replace Image component with LocalizationImage"), EditorStyles.miniButtonMid) == true) {

					FlowDatabase.ReplaceComponents<LocalizationImage, UnityEngine.UI.Image>(locImage, typeof(UnityEngine.UI.Image));
					return;

				}

			} else {

				EditorGUILayout.PropertyField(property, label);

			}

			//property.serializedObject.ApplyModifiedProperties();

		}

	}*/

}