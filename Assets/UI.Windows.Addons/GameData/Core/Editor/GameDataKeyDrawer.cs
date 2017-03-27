using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.GameData;
using UnityEditor.UI.Windows.Extensions;
using UnityEditor.UI.Windows.Hierarchy;

namespace UnityEditor.UI.Windows.Plugins.GameData {
	
	[CustomPropertyDrawer(typeof(GDFloat))]
	public class GDFloatKeyDrawer : PropertyDrawer {
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return 16f;

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			GDKeyDrawer.Draw(position, property, label);

		}

	}
	
	[CustomPropertyDrawer(typeof(GDInt))]
	public class GDIntKeyDrawer : PropertyDrawer {
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			
			return 16f;
			
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			
			GDKeyDrawer.Draw(position, property, label);
			
		}
		
	}
	
    [CustomPropertyDrawer(typeof(GDBool))]
    public class GDBoolKeyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            return 16f;

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            GDKeyDrawer.Draw(position, property, label);

        }

    }
    
    public class GDEnumKeyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            return 16f;

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            GDKeyDrawer.Draw(position, property, label);

        }

    }

    public static class GDKeyDrawer {

		public static void Draw(Rect position, SerializedProperty property, GUIContent label) {
			
			if (Application.isPlaying == false) {
				
				GameDataSystem.TryToLoadCache();
				
			}
			
			//property.serializedObject.Update();
			
			var keyProperty = property.FindPropertyRelative("key");
			if (keyProperty != null) {
				
				//GUILayout.BeginHorizontal();
				{
					
					var labelWidth = EditorGUIUtility.labelWidth;
					var offset = new Vector2(-23f - 100f - labelWidth, -7f - 16f);
					
					var indent = EditorGUI.indentLevel * 16f;
					
					GUI.Label(new Rect(position.x + indent, position.y, labelWidth - indent, position.height), label);
					
					var buttonRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);
					
					var style = ME.Utilities.CacheStyle("GameDataKeyDrawer.TextStyle", "inputField", (name) => {
						
						var _style = new GUIStyle(EditorStyles.textField);
						_style.richText = true;
						return _style;
						
					});
					
					var keyValue = keyProperty.stringValue;
					var keyFull = string.Format("[<color=green><b>{1}</b></color>] {0}", keyValue, GameDataSystem.Get(keyValue, GameDataSystem.DEFAULT_EDITOR_VERSION));
					if (GUI.Button(buttonRect, keyFull, style) == true) {
						
						var rect = position;
						Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(position.x, position.y));
						rect.x = vector.x;
						rect.y = vector.y;
						
						var popup = new Popup() { title = "GameData Keys", screenRect = new Rect(rect.x + labelWidth + offset.x, rect.y + rect.height + offset.y, rect.width - labelWidth - offset.x, 200f), searchText = keyProperty.stringValue, separator = '|' };
						popup.ItemByPath("None", () => {
							
							keyProperty.serializedObject.Update();
							keyProperty.stringValue = string.Empty;
							keyProperty.serializedObject.ApplyModifiedProperties();
							
							EditorUtility.SetDirty(keyProperty.serializedObject.targetObject);
							
						});
						
						foreach (var key in GameDataSystem.GetKeys()) {
							
							var finalKey = key;
							var finalKeyFull = string.Format("{0} <color=grey>({1})</color>", finalKey, GameDataSystem.Get(finalKey, GameDataSystem.DEFAULT_EDITOR_VERSION));
							popup.ItemByPath(finalKeyFull, () => {
								
								keyProperty.serializedObject.Update();
								keyProperty.stringValue = finalKey;
								keyProperty.serializedObject.ApplyModifiedProperties();
								
								EditorUtility.SetDirty(keyProperty.serializedObject.targetObject);
								
							});
							
						}
						
						popup.Show();
						
					}
					
					var textFieldRect = buttonRect;//GUILayoutUtility.GetLastRect();
					EditorGUIUtility.AddCursorRect(textFieldRect, MouseCursor.Text);
					
					const float sizeX = 32f;
					const float sizeY = 16f;
					HierarchyEditor.DrawLabel(new Rect(textFieldRect.x + textFieldRect.width - sizeX, textFieldRect.y, sizeX, sizeY), HierarchyEditor.colors.layoutElements, "GD");
					
					if (GameDataSystem.ContainsKey(keyProperty.stringValue) == false) {
						
						HierarchyEditor.DrawLabel(new Rect(textFieldRect.x + textFieldRect.width - sizeX - 54f, textFieldRect.y, 54f, sizeY), HierarchyEditor.colors.error, "NO KEY");
						
					} else {
						
						HierarchyEditor.DrawLabel(new Rect(textFieldRect.x + textFieldRect.width - sizeX - 54f, textFieldRect.y, 54f, sizeY), HierarchyEditor.colors.components, GameDataSystem.DEFAULT_EDITOR_VERSION.ToString());
						
					}
					
				}
				//GUILayout.EndHorizontal();
				
			}
			
			//property.serializedObject.ApplyModifiedProperties();
			
		}

	}

}