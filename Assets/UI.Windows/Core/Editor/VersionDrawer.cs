using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows {

	[CustomPropertyDrawer(typeof(VersionCheck))]
	public class VersionCheckDrawer : PropertyDrawer {
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return 16f;

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			VersionCheckDrawer.Draw(position, property, label);

		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label) {
			
			var enabled = property.FindPropertyRelative("enabled");
			var version = property.FindPropertyRelative("version");

			var major = version.FindPropertyRelative("major");
			var minor = version.FindPropertyRelative("minor");
			var release = version.FindPropertyRelative("release");
			var type = version.FindPropertyRelative("type");

			//GUILayout.BeginHorizontal();
			{

				var labelWidth = EditorGUIUtility.labelWidth;
				//var offset = new Vector2(-23f - 100f - labelWidth, -7f - 16f);

				var indent = EditorGUI.indentLevel * 16f;

				GUI.Label(new Rect(position.x + indent, position.y, labelWidth - indent, position.height), label);

				var fullRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

				var count = 4;
				var firstOffset = 25f;

				var pos0 = new Rect(fullRect);
				pos0.width = firstOffset;
				enabled.boolValue = EditorGUI.Toggle(pos0, enabled.boolValue);

				fullRect.x += firstOffset;
				fullRect.width -= firstOffset;

				EditorGUI.BeginDisabledGroup(!enabled.boolValue);
				{

					var pos1 = new Rect(fullRect);
					pos1.x = pos0.x + pos0.width;
					pos1.width = pos1.width / count;
					major.intValue = EditorGUI.IntField(pos1, major.intValue);
					
					var pos2 = new Rect(fullRect);
					pos2.x = pos1.x + pos1.width;
					pos2.width = pos2.width / count;
					minor.intValue = EditorGUI.IntField(pos2, minor.intValue);
					
					var pos3 = new Rect(fullRect);
					pos3.x = pos2.x + pos2.width;
					pos3.width = pos3.width / count;
					release.intValue = EditorGUI.IntField(pos3, release.intValue);
					
					var pos4 = new Rect(fullRect);
					pos4.x = pos3.x + pos3.width;
					pos4.width = pos4.width / count;
					var index = Version.Type.GetIndex(type.stringValue);
					index = EditorGUI.Popup(pos4, index, Version.Type.GetValues());
					if (index >= 0) type.stringValue = Version.Type.GetValues()[index];
					//type.stringValue = EditorGUI.TextField(pos4, type.stringValue);

				}
				EditorGUI.EndDisabledGroup();

			}
			//GUILayout.EndHorizontal();

		}

	}

}