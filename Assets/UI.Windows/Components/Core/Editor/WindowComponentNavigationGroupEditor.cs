using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEditor.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Components {

	[CustomPropertyDrawer(typeof(NavigationGroup))]
	public class WindowComponentNavigationGroupEditor : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			var navGroup = PropertyExtensions.GetTargetObjectOfProperty(property) as NavigationGroup;
			var newValue = EditorGUI.Toggle(position, label, navGroup.IsActive());
			if (newValue != navGroup.IsActive()) {

				navGroup.SetActive(newValue);
				navGroup.OnValidate();
				property.serializedObject.ApplyModifiedProperties();

			}

			if (newValue == true) EditorGUILayout.HelpBox("This component in NavigationGroup, this means navigation will be between childs only.", MessageType.Info);

		}

	}

}