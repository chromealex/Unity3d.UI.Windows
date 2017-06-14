using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Localization.UI;
using UnityEditor.UI.Windows.Hierarchy;

namespace UnityEditor.UI.Windows.Plugins.Localization.UI {

	/*[CustomPropertyDrawer(typeof(TextAreaAttribute))]
	public class TextDrawer : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return 0f;

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			//property.serializedObject.Update();

			var text = property.serializedObject.targetObject as UnityEngine.UI.Text;
			var locText = property.serializedObject.targetObject as LocalizationText;
			if (text != null && property.serializedObject.targetObject.GetType() == typeof(UnityEngine.UI.Text)) {

				EditorGUILayout.LabelField(label);
				property.stringValue = EditorGUILayout.TextArea(property.stringValue, GUILayout.MinHeight(40f));

				var width = 120f;
				if (GUI.Button(new Rect(position.x + position.width - width, position.y, width, 16f), new GUIContent("Switch to Localized", "Replace Text component with LocalizationText"), EditorStyles.miniButtonMid) == true) {
					
					FlowDatabase.ReplaceComponents<UnityEngine.UI.Text, LocalizationText>(text, typeof(LocalizationText));
					return;

				}

			} else if (locText != null && property.serializedObject.targetObject.GetType() == typeof(LocalizationText)) {

				EditorGUILayout.LabelField(label);
				property.stringValue = EditorGUILayout.TextArea(property.stringValue, GUILayout.MinHeight(40f));

				var width = 120f;
				if (GUI.Button(new Rect(position.x + position.width - width, position.y, width, 16f), new GUIContent("Switch to normal Text", "Replace Text component with LocalizationText"), EditorStyles.miniButtonMid) == true) {

					FlowDatabase.ReplaceComponents<LocalizationText, UnityEngine.UI.Text>(locText, typeof(UnityEngine.UI.Text));
					return;

				}

			} else {

				EditorGUILayout.LabelField(label);
				property.stringValue = EditorGUILayout.TextArea(property.stringValue, GUILayout.MinHeight(40f));

				//EditorGUILayout.PropertyField(property, label);

			}

			//property.serializedObject.ApplyModifiedProperties();

		}

	}*/

}