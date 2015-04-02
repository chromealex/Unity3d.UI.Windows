using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEditorInternal;

namespace UnityEditor.UI.Windows {

	[CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Modules.Modules))]
	public class WindowBaseModulesProperty : PropertyDrawer {

		private bool inited = false;
		private ReorderableList elements;

		public void Init(Rect position, SerializedProperty property, GUIContent label) {

			if (this.inited == false) {

				this.elements = new ReorderableList(property.serializedObject, property, true, true, true, true);
				this.elements.elementHeight = 70f;

				this.elements.drawHeaderCallback += rect => GUI.Label(rect, label);
				this.elements.drawElementCallback += (rect, index, active, focused) => {

					var startY = rect.y + 2f;
					var offset = 2f;
					var heightOffset = 10f;

					rect.y = startY;

					if (index >= property.arraySize) return;

					var item = property.GetArrayElementAtIndex(index);

					var moduleSource = item.FindPropertyRelative("moduleSource");
					var sortingOrder = item.FindPropertyRelative("sortingOrder");
					var backgroundLayer = item.FindPropertyRelative("backgroundLayer");

					var title = "Module";
					var height = this.GetPropertyHeight(moduleSource, new GUIContent(title));
					EditorGUI.PropertyField(rect, moduleSource, new GUIContent(title), true);

					title = "Sorting Order";
					rect.y = startY + height + offset;
					height += this.GetPropertyHeight(sortingOrder, new GUIContent(title)) + offset;
					EditorGUI.PropertyField(rect, sortingOrder, new GUIContent(title), true);
					if (sortingOrder.intValue < 0) sortingOrder.intValue = 0;

					title = "Is Background Layer";
					rect.y = startY + height + offset;
					height += this.GetPropertyHeight(backgroundLayer, new GUIContent(title)) + offset;
					EditorGUI.PropertyField(rect, backgroundLayer, new GUIContent(title), true);

					this.elements.elementHeight = height + heightOffset;
					
				};

			}

			this.inited = true;

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			this.Init(position, property.FindPropertyRelative("elements"), label);

			if (EditorGUI.PropertyField(position, property, label) == true) {

				this.elements.DoLayoutList();

			}

		}

	}

}
