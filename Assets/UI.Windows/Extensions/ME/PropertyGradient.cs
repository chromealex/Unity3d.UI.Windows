using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

#if UNITY_EDITOR
using UnityEditor;

public static class PropertyGradient {

	public static Gradient GetGradient(this SerializedProperty sp) {

		BindingFlags instanceAnyPrivacyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty(
			"gradientValue",
			instanceAnyPrivacyBindingFlags,
			null,
			typeof(Gradient),
			new Type[0],
			null
		);
		if (propertyInfo == null)
			return null;

		Gradient gradientValue = propertyInfo.GetValue(sp, null) as Gradient;
		return gradientValue;
	}

}
#endif