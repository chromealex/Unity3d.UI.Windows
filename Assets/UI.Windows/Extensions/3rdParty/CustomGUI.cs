#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CustomGUI {
	
	public static readonly GUIStyle splitter;
	
	static CustomGUI() {
		//GUISkin skin = GUI.skin;
		
		splitter = new GUIStyle();
		splitter.normal.background = EditorGUIUtility.whiteTexture;
		splitter.stretchWidth = true;
		splitter.margin = new RectOffset(0, 0, 7, 7);
	}
	
	private static readonly Color splitterColor = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
	
	// GUILayout Style
	public static void Splitter(Color rgb, float thickness = 1) {
		Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitter, GUILayout.Height(thickness));
		
		if (Event.current.type == EventType.Repaint) {
			Color restoreColor = GUI.color;
			GUI.color = rgb;
			splitter.Draw(position, false, false, false, false);
			GUI.color = restoreColor;
		}
	}
	
	public static void Splitter(float thickness, GUIStyle splitterStyle) {
		Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitterStyle, GUILayout.Height(thickness));
		
		if (Event.current.type == EventType.Repaint) {
			Color restoreColor = GUI.color;
			GUI.color = splitterColor;
			splitterStyle.Draw(position, false, false, false, false);
			GUI.color = restoreColor;
		}
	}
	
	public static void Splitter(float thickness = 1) {
		Splitter(thickness, splitter);
	}
	
	// GUI Style
	public static void Splitter(Rect position) {
		if (Event.current.type == EventType.Repaint) {
			Color restoreColor = GUI.color;
			GUI.color = splitterColor;
			splitter.Draw(position, false, false, false, false);
			GUI.color = restoreColor;
		}
	}
	
}
#endif