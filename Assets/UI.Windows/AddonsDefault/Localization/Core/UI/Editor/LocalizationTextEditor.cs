using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.Localization.UI;

namespace UnityEditor.UI.Windows.Plugins.Localization.UI {

	[CustomEditor(typeof(LocalizationText), true)]
	public class LocalizationTextEditor : UnityEditor.UI.TextEditor {

		public override void OnInspectorGUI() {

			base.OnInspectorGUI();

			var property = this.serializedObject.FindProperty("localizationKey");
			var label = new GUIContent("Localization Key");

			var rect = GUILayoutUtility.GetRect(label, EditorStyles.textField);
			LocalizationKeyDrawer.Draw(rect, property, label);

		}

	}

}