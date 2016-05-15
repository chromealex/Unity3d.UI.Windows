using UnityEngine;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows {

	public class BeginGroupAttribute : PropertyAttribute {

		public string otherFieldName;

		public BeginGroupAttribute() {}

		public BeginGroupAttribute(string otherFieldName) {

			this.otherFieldName = otherFieldName;

		}

	}

}

#if UNITY_EDITOR
namespace UnityEditor.UI.Windows {

	using UnityEditor.UI.Windows.Extensions;

	[CustomPropertyDrawer(typeof(BeginGroupAttribute))]
	public class BeginGroupProperty : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			
			EditorGUI.PropertyField(position, property, label, includeChildren: true);

			var attribute = PropertyExtensions.GetAttribute<BeginGroupAttribute>(this);
			var otherProp = PropertyExtensions.GetRelativeProperty(property, property.propertyPath, attribute.otherFieldName);

			var isNull = false;
			if (property.propertyType == SerializedPropertyType.ObjectReference) {

				isNull = (property.objectReferenceValue == null);

			}

			if (isNull == true && otherProp != null) {

				if (otherProp.propertyType == SerializedPropertyType.ObjectReference) {

					isNull = (otherProp.objectReferenceValue == null);

				}

			}

			++EditorGUI.indentLevel;
			EditorGUI.BeginDisabledGroup(isNull);

			CustomGUI.Splitter();

		}

	}

}
#endif