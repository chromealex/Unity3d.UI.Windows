using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Localization;
using UnityEditor.UI.Windows.Extensions;
using UnityEditor.UI.Windows.Hierarchy;

namespace UnityEditor.UI.Windows.Plugins.Localization {

	[CustomPropertyDrawer(typeof(LocalizationKey))]
	public class LocalizationKeyDrawer : PropertyDrawer {
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return 16f;

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			LocalizationKeyDrawer.Draw(position, property, label);

		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label) {
			
			LocalizationSystem.TryToLoadCache();

			property.serializedObject.Update();

			var keyProperty = property.FindPropertyRelative("key");
			var parametersCountProperty = property.FindPropertyRelative("parameters");
			if (keyProperty != null && parametersCountProperty != null) {

				//GUILayout.BeginHorizontal();
				{

					var labelWidth = EditorGUIUtility.labelWidth;
					var offset = new Vector2(-23f - 100f - labelWidth, -7f - 16f);

					var indent = EditorGUI.indentLevel * 16f;

					GUI.Label(new Rect(position.x + indent, position.y, labelWidth - indent, position.height), label);

					var buttonRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

					var keyValue = keyProperty.stringValue;
					var keyFull = string.Format("{0} ({1})", keyValue, LocalizationSystem.Get(keyValue, LocalizationSystem.DEFAULT_EDITOR_LANGUAGE));
					if (GUI.Button(buttonRect, keyFull, EditorStyles.textField) == true) {

						var rect = position;
						Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(position.x, position.y));
						rect.x = vector.x;
						rect.y = vector.y;

						var popup = new Popup() { title = "Localization Keys", screenRect = new Rect(rect.x + labelWidth + offset.x, rect.y + rect.height + offset.y, rect.width - labelWidth - offset.x, 200f), searchText = keyProperty.stringValue, separator = '|' };
						popup.Item("None", () => {

							parametersCountProperty.serializedObject.Update();
							keyProperty.serializedObject.Update();

							keyProperty.stringValue = string.Empty;
							parametersCountProperty.intValue = 0;

							keyProperty.serializedObject.ApplyModifiedProperties();
							parametersCountProperty.serializedObject.ApplyModifiedProperties();

						});
						foreach (var key in LocalizationSystem.GetKeys()) {

							var finalKey = key;
							var finalKeyFull = string.Format("{0} <color=grey>({1})</color>", finalKey, LocalizationSystem.Get(finalKey, LocalizationSystem.DEFAULT_EDITOR_LANGUAGE));
							popup.Item(finalKeyFull, () => {

								parametersCountProperty.serializedObject.Update();
								keyProperty.serializedObject.Update();

								keyProperty.stringValue = finalKey;
								parametersCountProperty.intValue = LocalizationSystem.GetParametersCount(finalKey);

								keyProperty.serializedObject.ApplyModifiedProperties();
								parametersCountProperty.serializedObject.ApplyModifiedProperties();

							});

						}

						popup.Show();

					}

					var textFieldRect = buttonRect;//GUILayoutUtility.GetLastRect();
					EditorGUIUtility.AddCursorRect(textFieldRect, MouseCursor.Text);

					const float sizeX = 32f;
					const float sizeY = 16f;
					HierarchyEditor.DrawLabel(new Rect(textFieldRect.x + textFieldRect.width - sizeX, textFieldRect.y, sizeX, sizeY), HierarchyEditor.colors.screens, "LOC");

					if (LocalizationSystem.ContainsKey(keyProperty.stringValue) == false) {

						HierarchyEditor.DrawLabel(new Rect(textFieldRect.x + textFieldRect.width - sizeX - 54f, textFieldRect.y, 54f, sizeY), HierarchyEditor.colors.error, "NO KEY");

					}

				}
				//GUILayout.EndHorizontal();

			}

			property.serializedObject.ApplyModifiedProperties();

		}

	}

}