using UnityEngine;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows {

	public class EndGroupAttribute : PropertyAttribute {

	}

	public class EndGroupReadOnlyAttribute : ReadOnlyAttribute {
		
		public EndGroupReadOnlyAttribute() : base() {}
		public EndGroupReadOnlyAttribute(string fieldName, object state = null, bool bitMask = false, bool inverseCondition = false) : base(fieldName, state, bitMask, inverseCondition) {}

	}

}

#if UNITY_EDITOR
namespace UnityEditor.UI.Windows {

	[CustomPropertyDrawer(typeof(EndGroupAttribute))]
	public class EndGroupProperty : PropertyDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			EditorGUI.PropertyField(position, property, label, includeChildren: true);

			CustomGUI.Splitter();

			EditorGUI.EndDisabledGroup();
			--EditorGUI.indentLevel;

		}

	}

	[CustomPropertyDrawer(typeof(EndGroupReadOnlyAttribute))]
	public class EndGroupReadOnlyProperty : ReadOnlyAttributeDrawer {

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return base.GetPropertyHeight(property, label);

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			base.OnGUI(position, property, label);

			CustomGUI.Splitter();

			EditorGUI.EndDisabledGroup();
			--EditorGUI.indentLevel;

		}

	}

}
#endif