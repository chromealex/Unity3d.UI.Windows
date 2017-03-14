using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEditorInternal;
using UnityEngine.UI.Windows.Types;
using System.Collections.Generic;
using UnityEditor.UI.Windows.Internal.ReorderableList;
using UnityEngine.UI.Windows.Components;
using UnityEditor.UI.Windows.Plugins.Flow.Layout;

namespace UnityEditor.UI.Windows {

	[CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Types.Layout))]
	public class WindowLayoutProperty : PropertyDrawer {

		private LayoutSettingsEditor editor;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			if (this.editor == null) this.editor = new LayoutSettingsEditor();

			//property.serializedObject.Update();

			this.editor.OnGUI(position, property, label);

			property.serializedObject.ApplyModifiedProperties();

		}

	}

	[CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Types.Layouts))]
	public class WindowLayoutsProperty : PropertyDrawer {

		private LayoutsSettingsEditor editor;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			if (this.editor == null) this.editor = new LayoutsSettingsEditor();

			this.editor.OnGUI(position, property, label);

			property.serializedObject.ApplyModifiedProperties();

		}

	}

}
