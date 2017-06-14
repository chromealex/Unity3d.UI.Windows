using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows;
using UnityEditor.UI.Windows.Hierarchy;

namespace UnityEditor.UI.Windows {

	using UnityEditor.UI.Windows.Extensions;

	[CustomPropertyDrawer(typeof(LinkerComponentProxy))]
	public class BeginGroupProperty : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return 16f;//EditorGUI.GetPropertyHeight(property, label, includeChildren: true);

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			const float sizeX = 18f;
			const float sizeY = 16f;

			var prop = property.FindPropertyRelative("component");
			var component = prop.objectReferenceValue;
			var newComponent = EditorGUI.ObjectField(position, label, component, typeof(WindowComponent), allowSceneObjects: true);
			if (newComponent != component) {

				prop.objectReferenceValue = newComponent;
				property.serializedObject.ApplyModifiedProperties();

			}

			HierarchyEditor.DrawLabel(new Rect(position.x + position.width - sizeX - 54f, position.y, 54f, sizeY), HierarchyEditor.colors.linkers, "PROXY");

		}

	}

}