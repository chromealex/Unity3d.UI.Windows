using UnityEngine;
using UnityEditor.UI.Windows.Extensions;
using System.Linq;

namespace UnityEditor.UI.Windows.Plugins.Flow.Layout {

	public class LayoutsSettingsEditor {

		public class Styles {

			public GUIStyle leftButton;
			public GUIStyle midButton;
			public GUIStyle rightButton;
			public GUIStyle removeButton;
			public GUIStyle addButton;

			public GUIStyle box;

			public Styles() {

				this.leftButton = new GUIStyle(EditorStyles.miniButtonLeft);
				this.leftButton.fixedWidth = 10f;
				this.midButton = new GUIStyle(EditorStyles.miniButtonMid);
				this.rightButton = new GUIStyle(EditorStyles.miniButtonRight);
				this.rightButton.fixedWidth = 10f;

				this.removeButton = new GUIStyle(this.midButton);
				this.removeButton.fixedWidth = 30f;
				this.addButton = new GUIStyle(this.rightButton);
				this.addButton.fixedWidth = 30f;

				this.box = new GUIStyle(EditorStyles.helpBox);

			}

		}

		public Styles styles;
		private int selected = 0;
		private LayoutSettingsEditor layoutSettingsEditor;

		public void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			if (this.styles == null) {
				
				this.styles = new Styles();

			}

			if (this.layoutSettingsEditor == null) {
				
				this.layoutSettingsEditor = new LayoutSettingsEditor();

			}

			//property.serializedObject.Update();

			if (EditorGUI.PropertyField(position, property, label, false) == true) {

				++EditorGUI.indentLevel;

				var typesArray = property.FindPropertyRelative("types");
				var layoutsArray = property.FindPropertyRelative("layouts");

				if (typesArray.arraySize == 0) {

					typesArray.ClearArray();
					typesArray.InsertArrayElementAtIndex(0);
					var val = typesArray.GetArrayElementAtIndex(0);
					val.stringValue = "Horizontal";

					typesArray.InsertArrayElementAtIndex(1);
					val = typesArray.GetArrayElementAtIndex(1);
					val.stringValue = "Vertical";

					if (layoutsArray.arraySize == 0) {

						layoutsArray.InsertArrayElementAtIndex(0);
						val = layoutsArray.GetArrayElementAtIndex(0);
						object root;
						PropertyExtensions.GetTargetObjectOfProperty(val, out root);
						var layoutRoot = root as UnityEngine.UI.Windows.Types.Layouts;
						layoutRoot.layouts[0] = (property.serializedObject.targetObject as UnityEngine.UI.Windows.Types.LayoutWindowType).GetCurrentLayout();
						layoutRoot.layouts[0].enabled = true;

					}

				}

				//var valLay = layoutsArray.GetArrayElementAtIndex(0);
				//valLay.FindPropertyRelative("enabled").boolValue = true;

				if (layoutsArray.arraySize < typesArray.arraySize) {

					//layoutsArray.InsertArrayElementAtIndex(layoutsArray.arraySize - 1);
					var val = layoutsArray.GetArrayElementAtIndex(layoutsArray.arraySize - 1);
					object root;
					PropertyExtensions.GetTargetObjectOfProperty(val, out root);
					var layoutRoot = root as UnityEngine.UI.Windows.Types.Layouts;
					var list = layoutRoot.layouts.ToList();
					list.Add(new UnityEngine.UI.Windows.Types.Layout());
					layoutRoot.layouts = list.ToArray();

					EditorUtility.SetDirty(property.serializedObject.targetObject);

				}

				GUILayout.BeginHorizontal();
				{
					
					var oldState = GUI.enabled;
					GUI.enabled = false;
					GUILayout.Button(string.Empty, this.styles.leftButton);
					GUI.enabled = oldState;

					for (int i = 0; i < typesArray.arraySize; ++i) {

						var element = typesArray.GetArrayElementAtIndex(i);
						var check = (i == this.selected) ? true : false;
						var newCheck = GUILayout.Toggle(check, element.stringValue, this.styles.midButton);
						if (newCheck != check) {

							if (newCheck == true) {

								this.selected = i;

								var currentLayout = layoutsArray.GetArrayElementAtIndex(this.selected);
								object root;
								PropertyExtensions.GetTargetObjectOfProperty(currentLayout, out root);
								var layoutRoot = root as UnityEngine.UI.Windows.Types.Layouts;
								layoutRoot.currentLayoutIndex = this.selected;

								this.layoutSettingsEditor.Reset();

							}

						}

					}

					/*GUILayout.Button("-", this.styles.removeButton);
					if (GUILayout.Button("+", this.styles.addButton) == true) {

						typesArray.InsertArrayElementAtIndex(typesArray.arraySize - 1);
						var val = typesArray.GetArrayElementAtIndex(typesArray.arraySize - 1);
						val.stringValue = "Vertical";

						layoutsArray.InsertArrayElementAtIndex(layoutsArray.arraySize - 1);

					}*/

					oldState = GUI.enabled;
					GUI.enabled = false;
					GUILayout.Button(string.Empty, this.styles.rightButton);
					GUI.enabled = oldState;

				}
				GUILayout.EndHorizontal();

				--EditorGUI.indentLevel;

				GUILayout.BeginVertical(this.styles.box);
				{
					
					var currentLayout = layoutsArray.GetArrayElementAtIndex(this.selected);
					this.layoutSettingsEditor.OnGUI(position, currentLayout, label);

				}
				GUILayout.EndVertical();

			}

			property.serializedObject.ApplyModifiedProperties();

		}

	}

}