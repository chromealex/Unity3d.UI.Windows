using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEditorInternal;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows {

	[CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Types.Layout))]
	public class WindowLayoutProperty : PropertyDrawer {

		private bool inited = false;
		private ReorderableList elements;

		public void Init(Rect position, SerializedProperty property, GUIContent label) {

			if (property == null) return;

			if (this.inited == false) {

				this.elements = new ReorderableList(property.serializedObject, property, true, true, false, false);
				this.elements.elementHeight = 70f;

				this.elements.drawHeaderCallback += (rect) => GUI.Label(rect, label);
				this.elements.drawElementCallback += (rect, index, active, focused) => {

					var startY = rect.y + 2f;
					var offset = 2f;
					var heightOffset = 20f;

					rect.y = startY;

					if (index >= property.arraySize) return;

					if (active == true && focused == true) {
						
						var window = (property.serializedObject.targetObject as LayoutWindowType);
						WindowLayoutElement.waitForComponentConnectionElementTemp = window.layout.layout.GetRootByTag(window.layout.components[index].tag);
						WindowLayoutElement.waitForComponentConnectionTemp = true;

					} else {

						if (active == true) {

							WindowLayoutElement.waitForComponentConnectionElementTemp = null;
							WindowLayoutElement.waitForComponentConnectionTemp = false;

						}

					}

					var item = property.GetArrayElementAtIndex(index);

					var descr = item.FindPropertyRelative("description");
					var tag = item.FindPropertyRelative("tag");
					var component = item.FindPropertyRelative("component");
					var sortingOrder = item.FindPropertyRelative("sortingOrder");

					var title = "Description";
					var height = this.GetPropertyHeight(descr, new GUIContent(title)) + offset;
					rect.height = height - offset;
					EditorGUI.LabelField(rect, descr.stringValue);

					title = "Tag";
					rect.y += height;
					height = this.GetPropertyHeight(tag, new GUIContent(title)) + offset;
					rect.height = height - offset;
					EditorGUI.PropertyField(rect, tag, new GUIContent(title), true);
					
					title = "Component";
					rect.y += height;
					height = this.GetPropertyHeight(component, new GUIContent(title)) + offset;
					rect.height = height - offset;
					EditorGUI.PropertyField(rect, component, new GUIContent(title), true);

					title = "Sorting Order";
					rect.y += height;
					height = this.GetPropertyHeight(sortingOrder, new GUIContent(title)) + offset;
					rect.height = height - offset;
					EditorGUI.PropertyField(rect, sortingOrder, new GUIContent(title), true);
					if (sortingOrder.intValue < 0) sortingOrder.intValue = 0;

					this.elements.elementHeight = (rect.y - startY) + heightOffset;
					
				};

			}

			this.inited = true;

		}

		private Editor canvasScalerEditor;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			
			var window = (property.serializedObject.targetObject as LayoutWindowType);
			this.Init(position, property.FindPropertyRelative("components"), new GUIContent("Components"));

			var scaleMode = property.FindPropertyRelative("scaleMode");
			var fixedScaleSize = property.FindPropertyRelative("fixedScaleResolution");
			var layout = property.FindPropertyRelative("layout");

			if (EditorGUI.PropertyField(position, property, label, false) == true) {
				
				++EditorGUI.indentLevel;

				var oldValue = layout.objectReferenceValue;
				EditorGUILayout.PropertyField(layout, new GUIContent("Layout:"));
				var newValue = layout.objectReferenceValue;
				if (oldValue != newValue) {

					if (oldValue == null ||
					    EditorUtility.DisplayDialog("Layout Changing Warning",
					                            "Do you really want to change this layout? Components list will be destroyed and the new one will be created. Are you sure?",
					                            "Yes",
					                            "No") == true) {

						this.canvasScalerEditor = null;
						this.inited = false;
						GUI.changed = true;
						window.layout.components = new Layout.Component[0];
						window.OnValidate();

					}

				}

				if (layout.objectReferenceValue != null) {
					
					CustomGUI.Splitter();

					EditorGUILayout.PropertyField(scaleMode, new GUIContent("Scale Mode:"));
					
					if (this.canvasScalerEditor == null) {
						
						var layoutSource = layout.objectReferenceValue as UnityEngine.UI.Windows.WindowLayout;
						if (layoutSource != null && layoutSource.canvasScaler != null) {
							
							this.canvasScalerEditor = Editor.CreateEditor(layoutSource.canvasScaler);
							
						} else {
							
							this.canvasScalerEditor = null;
							
						}
						
					}

					var mode = (UnityEngine.UI.Windows.WindowLayout.ScaleMode)System.Enum.GetValues(typeof(UnityEngine.UI.Windows.WindowLayout.ScaleMode)).GetValue(scaleMode.enumValueIndex);

					var enabled = true;
					if (mode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Normal || // Normal mode
						mode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Fixed) { // Fixed mode

						var layoutSource = layout.objectReferenceValue as UnityEngine.UI.Windows.WindowLayout;
						if (layoutSource != null) {

							if (mode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Fixed) {

								EditorGUILayout.PropertyField(fixedScaleSize, new GUIContent("Resolution:"));

							}

							layoutSource.SetScale(mode, fixedScaleSize.vector2Value);

						}
						enabled = false;

					}

					var oldState = GUI.enabled;
					GUI.enabled = enabled;
					if (this.canvasScalerEditor != null) this.canvasScalerEditor.OnInspectorGUI();
					GUI.enabled = oldState;
					CustomGUI.Splitter();

				}
				
				--EditorGUI.indentLevel;
				
				if (this.inited == true && window != null && window.layout.layout != null) {
					
					this.elements.DoLayoutList();
					
				}

			}

		}

	}

}
