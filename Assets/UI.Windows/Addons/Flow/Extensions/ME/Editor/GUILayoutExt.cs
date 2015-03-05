using UnityEngine;
using System.Collections;

namespace ME {

	public class GUILayoutExt : MonoBehaviour {
		
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

			var shadowColor = Color.black;
			shadowColor.a = 0.6f;

			var shadowOffset = -1f;

			var oldColor = GUI.color;

			GUI.color = shadowColor;
			GUILayout.Label(content, style, options);
			var rect = GUILayoutUtility.GetLastRect();
			rect.x += shadowOffset;
			rect.y += shadowOffset;

			GUI.color = oldColor;
			GUI.Label(rect, content, style);

		}

	}

}