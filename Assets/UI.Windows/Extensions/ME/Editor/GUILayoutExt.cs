using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows;

namespace ME {

	public partial class GUILayoutExt {

		public static int Popup(int value, string[] values, GUIStyle labelStyle, params GUILayoutOption[] layout) {

			var style = new GUIStyle("IN Popup");//"SimplePopup");//"ExposablePopupMenu");//"IN Popup");
			style.font = labelStyle.font;
			style.fontSize = labelStyle.fontSize;
			style.padding = labelStyle.padding;
			style.margin = labelStyle.margin;
			style.contentOffset = labelStyle.contentOffset;
			style.fixedHeight = 0f;
			style.wordWrap = false;

			style.padding.right += 10;

			var shadowColor = EditorGUIUtility.isProSkin == true ? Color.black : Color.white;
			shadowColor.a = 0.6f;
			
			var shadowOffset = -1f;

			/*GUILayout.BeginHorizontal();
			{
				GUILayoutExt.LabelWithShadow(values[value], labelStyle, layout);
*/
				var oldColor = GUI.color;
				
				GUI.color = shadowColor;
				var shadowStyle = new GUIStyle(style);
				shadowStyle.normal.textColor = shadowColor;
				
				//GUILayout.Label(content, shadowStyle, options);
				value = EditorGUILayout.Popup(value, values, shadowStyle, layout);

				var rect = GUILayoutUtility.GetLastRect();
				rect.x += shadowOffset;
				rect.y += shadowOffset;
				
				GUI.color = oldColor;
				//GUI.Label(rect, content, style);

				value = EditorGUI.Popup(rect, value, values, style);

			/*}
			GUILayout.EndHorizontal();
*/
			return value;

		}
		
		public static T ObjectField<T>(T data, bool allowSceneObjects, GUIStyle style) where T : ScriptableObject {
			
			var id = GUIUtility.GetControlID(FocusType.Passive) + 1;
			var title = data == null ? "None" : data.name;
			
			GUILayout.Label(title, style);
			
			var rect = GUILayoutUtility.GetLastRect();
			var leftRect = rect;
			leftRect.width -= style.border.right;
			var rightRect = rect;
			rightRect.x += leftRect.width;
			rightRect.width = style.border.right;
			
			if (Event.current.clickCount == 1 && leftRect.Contains(Event.current.mousePosition) == true) {
				
				Selection.activeObject = data;
				Event.current.Use();
				
			}
			
			if (Event.current.clickCount == 1 && rightRect.Contains(Event.current.mousePosition) == true) {
				
				EditorGUIUtility.ShowObjectPicker<T>(data, allowSceneObjects, "t:" + (typeof(T).Name), id);
				Event.current.Use();
				
			}
			
			if (EditorGUIUtility.GetObjectPickerControlID() == id && id > 0) {
				
				data = EditorGUIUtility.GetObjectPickerObject() as T;
				
			}
			
			return data;
			
		}
		
		public static T ScreenField<T>(T data, bool allowSceneObjects, GUIStyle style) where T : WindowBase {
			
			var id = GUIUtility.GetControlID(FocusType.Passive) + 1;
			var title = data == null ? "None" : data.name;
			
			GUILayout.Label(title, style);
			
			var rect = GUILayoutUtility.GetLastRect();
			var leftRect = rect;
			leftRect.width -= style.border.right;
			var rightRect = rect;
			rightRect.x += leftRect.width;
			rightRect.width = style.border.right;
			
			if (Event.current.clickCount == 1 && leftRect.Contains(Event.current.mousePosition) == true) {
				
				Selection.activeObject = data;
				Event.current.Use();
				
			}
			
			if (Event.current.clickCount == 1 && rightRect.Contains(Event.current.mousePosition) == true) {
				
				EditorGUIUtility.ShowObjectPicker<T>(data, allowSceneObjects, "t:" + (typeof(T).Name), id);
				Event.current.Use();
				
			}
			
			if (EditorGUIUtility.GetObjectPickerControlID() == id && id > 0) {
				
				data = EditorGUIUtility.GetObjectPickerObject() as T;
				
			}
			
			return data;
			
		}

		public static void LabelWithShadow(string text, params GUILayoutOption[] options) {

			GUILayoutExt.LabelWithShadow(new GUIContent(text), options);

		}
		
		public static void LabelWithShadow(string text, GUIStyle style, params GUILayoutOption[] options) {
			
			GUILayoutExt.LabelWithShadow(new GUIContent(text), style, options);

		}
		
		public static void LabelWithShadow(GUIContent content, params GUILayoutOption[] options) {
			
			GUILayoutExt.LabelWithShadow(content, GUI.skin.label, options);

		}

		public static void LabelWithShadow(GUIContent content, GUIStyle style, params GUILayoutOption[] options) {

			var shadowColor = EditorGUIUtility.isProSkin == true ? Color.black : Color.white;
			shadowColor.a = 0.6f;

			var shadowOffset = -1f;

			var oldColor = GUI.color;

			GUI.color = shadowColor;
			var shadowStyle = new GUIStyle(style);
			shadowStyle.normal.textColor = shadowColor;

			GUILayout.Label(content, shadowStyle, options);
			var rect = GUILayoutUtility.GetLastRect();
			rect.x += shadowOffset;
			rect.y += shadowOffset;

			GUI.color = oldColor;
			GUI.Label(rect, content, style);

		}

	}

	public class GUIExt {
		
		public static void DrawTextureRotated(Vector3 pos, Texture texture, Quaternion rotation, float angle = 90f) {
			
			var matrixBackup = GUI.matrix;
			
			var size = new Vector2(texture.width, texture.height) * 2f;
			var rect = new Rect(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f, size.x, size.y);
			var pivot = rect.center;

			angle -= rotation.eulerAngles.x;

			GUIUtility.RotateAroundPivot(angle, pivot);
			GUI.DrawTexture(rect, texture);
			
			GUI.matrix = matrixBackup;
			
		}

		public static void DrawTextureRotated(Vector3 pos, Texture texture, float angle) {
			
			var matrixBackup = GUI.matrix;

			var size = new Vector2(texture.width, texture.height) * 2f;
			var rect = new Rect(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f, size.x, size.y);

			var pivot = rect.center;
			GUIUtility.RotateAroundPivot(angle, pivot);
			GUI.DrawTexture(rect, texture);
			
			GUI.matrix = matrixBackup;
			
		}

		public static void DrawTextureRotated(Rect rect, Texture texture, float angle) {

			Matrix4x4 matrixBackup = GUI.matrix;
			
			var pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
			GUIUtility.RotateAroundPivot(angle, pivot);
			GUI.DrawTexture(rect, texture);

			GUI.matrix = matrixBackup;

		}

	}

}