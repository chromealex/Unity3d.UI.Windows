#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public static class EditorGUIUtilityExt {

	public static void LookLikeControls() {

		EditorGUIUtility.labelWidth = 0f;
		EditorGUIUtility.fieldWidth = 0f;

	}

	public static void LookLikeControls(float labelWidth, float fieldWidth) {

		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;

	}

}
#endif