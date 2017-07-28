using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Audio;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using System;
using UnityEditor.UI.Windows.Extensions;
using UnityEditorInternal;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Flow;
using ME;
using UnityEngine.UI.Windows.Types;
using UnityEditor.UI.Windows.Internal.ReorderableList;
using UnityEngine.UI.Windows.Components;

namespace UnityEditor.UI.Windows.Plugins.Flow.Layout {

	public class LayoutSettingsEditor {

		public static int selectedTagIndex = -1;
		public static Rect selectedTagRect;
		
		private bool inited = false;
		//private ReorderableList elements;
		private ReorderableListControl elements;
		private IReorderableListAdaptor adaptor;
		private ReorderableListControl.ItemDrawer<SerializedProperty> onItemDraw;
		private List<UnityEngine.UI.Windows.Types.Layout.Component> components;
		private List<SerializedProperty> items;
		
		private LayoutWindowType window;

		public void Reset() {

			this.inited = false;

		}

		public void Init(Rect position, SerializedProperty property, GUIContent label) {
			
			if (property == null) return;
			if (this.inited == true) return;
			
			this.window = (property.serializedObject.targetObject as LayoutWindowType);

			this.items = new List<SerializedProperty>();
			this.components = new List<UnityEngine.UI.Windows.Types.Layout.Component>();
			for (int i = 0; i < property.arraySize; ++i) {
				
				var element = property.GetArrayElementAtIndex(i);
				if (this.window.GetCurrentLayout().layout == null) continue;
				
				var rootElement = this.window.GetCurrentLayout().layout.GetRootByTag(this.window.GetCurrentLayout().components[i].tag);
				if (rootElement != null && rootElement.showInComponentsList == true) {
					
					this.components.Add(this.window.GetCurrentLayout().components[i]);
					this.items.Add(element);
					
				}
				
			}
			
			this.onItemDraw = (rect, item) => {
				
				var h = 0f;
				return LayoutSettingsEditor.OnItemDraw(this.window, this.items, this.components, true, true, rect, item, out h);
				
			};
			
			System.Func<int, float> getHeight = (index) => {
				
				var h = 56f;
				LayoutSettingsEditor.OnItemDraw(this.window, this.items, this.components, false, true, new Rect(), this.items[index], out h);
				
				return h;
				
			};
			
			this.elements = new ReorderableListControl(ReorderableListFlags.HideAddButton |
			                                           ReorderableListFlags.HideRemoveButtons |
			                                           ReorderableListFlags.DisableAutoScroll |
			                                           ReorderableListFlags.DisableReordering);
			
			this.adaptor = new ComponentsListAdaptor<SerializedProperty>(this.items, this.onItemDraw, getHeight);
			
			this.inited = true;
			
		}

		private static List<bool> componentParametersSearched;
		public static SerializedProperty OnItemDraw(LayoutWindowType window, List<SerializedProperty> items, List<UnityEngine.UI.Windows.Types.Layout.Component> components, bool draw, bool withParameters, Rect rect, SerializedProperty item, out float elementHeight) {

			if (LayoutSettingsEditor.componentParametersSearched == null ||
				LayoutSettingsEditor.componentParametersSearched.Count != components.Count) {

				LayoutSettingsEditor.componentParametersSearched = new List<bool>();
				for (int i = 0; i < components.Count; ++i) {

					LayoutSettingsEditor.componentParametersSearched.Add(false);

				}

			}

			var startY = rect.y + 2f;
			var offset = 2f;
			var heightOffset = 20f;
			
			elementHeight = 0f;
			rect.y = startY;
			
			var index = items.IndexOf(item);
			if (index < 0 || index >= items.Count) return item;
			if (item == null) return null;

			var descr = item.FindPropertyRelative("description");
			var tag = item.FindPropertyRelative("tag");
			var component = item.FindPropertyRelative("component");
			var sortingOrder = item.FindPropertyRelative("sortingOrder");
			
			if (descr == null ||
			    tag == null ||
			    component == null ||
			    sortingOrder == null) return item;

			var parameters = item.FindPropertyRelative("componentParameters");
			var componentParametersEditor = components[index].componentParametersEditor;

			if (rect.Contains(Event.current.mousePosition) == true) {

				//WindowLayoutElement.waitForComponentConnectionElementTemp = window.GetCurrentLayout().layout.GetRootByTag((LayoutTag)tag.enumValueIndex);
				LayoutSettingsEditor.selectedTagIndex = tag.enumValueIndex;

			}

			if (LayoutSettingsEditor.selectedTagIndex == tag.enumValueIndex) {

				var _offset = 4f;
				var newRect = new Rect(rect.x - _offset, rect.y - _offset, rect.width + _offset * 2f, rect.height + _offset);

				LayoutSettingsEditor.selectedTagRect = newRect;

				var oldColor = GUI.color;
				GUI.color = new Color(0f, 1f, 0f, 0.2f);
				GUI.Box(newRect, string.Empty, EditorStyles.textArea);
				GUI.Box(newRect, string.Empty, EditorStyles.popup);
				GUI.color = oldColor;

			}

			/*if (active == true && focused == true) {
						
				//var window = (property.serializedObject.targetObject as LayoutWindowType);
				WindowLayoutElement.waitForComponentConnectionElementTemp = window.layout.layout.GetRootByTag((LayoutTag)tag.enumValueIndex);
				WindowLayoutElement.waitForComponentConnectionTemp = true;
				
			} else {
				
				if (active == true) {
					
					WindowLayoutElement.waitForComponentConnectionElementTemp = null;
					WindowLayoutElement.waitForComponentConnectionTemp = false;
					
				}
				
			}*/
			
			var title = "Description";
			var height = EditorGUI.GetPropertyHeight(descr, new GUIContent(title)) + offset;
			rect.height = height - offset;
			if (draw == true) {
				
				EditorGUI.LabelField(rect, descr.stringValue, EditorStyles.boldLabel);
				EditorGUI.LabelField(rect, tag.enumNames[tag.enumValueIndex], ME.Utilities.CacheStyle("WindowLayoutProperty.Styles.tag", "tag", (name) => {
					
					var style = new GUIStyle(EditorStyles.boldLabel);
					style.alignment = TextAnchor.MiddleRight;
					return style;
					
				}));
				
			}
			
			title = "Component";
			rect.y += height;
			height = EditorGUI.GetPropertyHeight(component, new GUIContent(title)) + offset;
			rect.height = height - offset;
			
			{
				
				var oldComponent = component.objectReferenceValue;
				
				var nRect = rect;
				var buttonWidth = 30f;
				nRect.width -= buttonWidth;
				if (draw == true) EditorGUI.PropertyField(nRect, component, new GUIContent(title), true);

				//Debug.Log(oldComponent + " == " + component.objectReferenceValue + " :: " + (oldComponent == component.objectReferenceValue));
				var newComponent = component.objectReferenceValue;
				if (oldComponent != newComponent) {

					//Debug.Log("NEW COMP: " + newComponent);
					parameters.objectReferenceValue = components[index].OnComponentChanged(window, newComponent as WindowComponent);
					//Debug.Log("NEW COMP: " + parameters.objectReferenceValue);
					components[index].componentParametersEditor = null;
					item.serializedObject.ApplyModifiedPropertiesWithoutUndo();
					
					UnityEditor.EditorUtility.SetDirty(window);

				}

				if (newComponent == null && components[index].componentResource.id != 0) {

					components[index].OnComponentChanged(window, null);
					components[index].componentParametersEditor = null;
					item.serializedObject.ApplyModifiedPropertiesWithoutUndo();

					UnityEditor.EditorUtility.SetDirty(window);

				}
				
				nRect.x += nRect.width;
				nRect.width = buttonWidth;
				if (draw == true) {

					var i = index;
					GUILayoutExt.DrawComponentChooser(nRect, window.gameObject, components[i].component, (elem) => {
						
						if (component.objectReferenceValue != elem) {
							
							component.objectReferenceValue = elem;
							var c = component.objectReferenceValue;
							parameters.objectReferenceValue = components[i].OnComponentChanged(window, c as WindowComponent);
							components[i].componentParametersEditor = null;
							item.serializedObject.ApplyModifiedPropertiesWithoutUndo();
							
							UnityEditor.EditorUtility.SetDirty(window);

						}
						
					});

				}
				
			}
			
			title = "Sorting Order";
			rect.y += height;
			height = EditorGUI.GetPropertyHeight(sortingOrder, new GUIContent(title)) + offset;
			rect.height = height - offset;
			if (draw == true) {

				var oldValue = sortingOrder.intValue;
				EditorGUI.PropertyField(rect, sortingOrder, new GUIContent(title), true);
				if (sortingOrder.intValue < 0) {

					sortingOrder.intValue = 0;

				}
				if (oldValue != sortingOrder.intValue) {

					item.serializedObject.ApplyModifiedPropertiesWithoutUndo();
					UnityEditor.EditorUtility.SetDirty(window);

				}

			}
			
			if (parameters.objectReferenceValue == null && LayoutSettingsEditor.componentParametersSearched[index] == false) {
				
				components[index].OnComponentChanged(window, components[index].component);
				LayoutSettingsEditor.componentParametersSearched[index] = true;
				
				UnityEditor.EditorUtility.SetDirty(window);

			}

			if (withParameters == true) {

				var editor = componentParametersEditor;
				if (editor == null && parameters.objectReferenceValue != null) {
					
					var e = Editor.CreateEditor(parameters.objectReferenceValue) as IParametersEditor;
					components[index].componentParametersEditor = componentParametersEditor = e;
					
				}
				
				if (editor != null) {
					
					title = "Parameters";
					rect.y += height;
					var _height = height = 16f;
					rect.height = height - offset;
					var editorFoldout = item.FindPropertyRelative("editorParametersFoldout");
					if (draw == true) {
						
						++EditorGUI.indentLevel;
						var newValue = EditorGUI.Foldout(rect, editorFoldout.boolValue, new GUIContent(title));
						if (newValue != editorFoldout.boolValue) {

							editorFoldout.boolValue = newValue;
							editorFoldout.serializedObject.ApplyModifiedProperties();

						}
						--EditorGUI.indentLevel;
						
					}
					
					if (editorFoldout.boolValue == true) {
						
						rect.y += height;
						height = 0f;
						
						if (draw == true) {
							
							++EditorGUI.indentLevel;
							editor.OnParametersGUI(rect);
							--EditorGUI.indentLevel;
							
							//parameters.objectReferenceValue = editor.target;
							//editor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
							
						}
						
						height = editor.GetHeight() + offset;
						rect.y += height - _height;
						rect.height = height - offset;
						
					}
					
				}

			}

			elementHeight = (rect.y - startY) + heightOffset;

			return item;
			
		}

		public float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			return EditorGUI.GetPropertyHeight(property, label);

		}

		private Editor canvasScalerEditor;
		public void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			LayoutSettingsEditor.selectedTagIndex = -1;

			//property.serializedObject.Update();
			
			//var window = (property.serializedObject.targetObject as LayoutWindowType);
			this.Init(position, property.FindPropertyRelative("components"), new GUIContent("Components"));

			var enabledField = property.FindPropertyRelative("enabled");

			var scaleMode = property.FindPropertyRelative("scaleMode");
			var fixedScaleSize = property.FindPropertyRelative("fixedScaleResolution");
			var matchWidthOrHeight = property.FindPropertyRelative("matchWidthOrHeight");
			var layoutPreferences = property.FindPropertyRelative("layoutPreferences");
			var allowCustomLayoutPreferences = property.FindPropertyRelative("allowCustomLayoutPreferences");
			var layout = property.FindPropertyRelative("layout");

			//if (EditorGUI.PropertyField(position, property, label, false) == true) {
				
				//++EditorGUI.indentLevel;

			EditorGUILayout.PropertyField(enabledField);
			EditorGUI.BeginDisabledGroup(!enabledField.boolValue);
			{

				CustomGUI.Splitter();

				{
					
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
							this.window.GetCurrentLayout().components = new UnityEngine.UI.Windows.Types.Layout.Component[0];
							this.window.OnValidate();

							property.serializedObject.ApplyModifiedProperties();

						}
						
					}

				}

				if (layout.objectReferenceValue != null || property.serializedObject.targetObject is FlowLayoutWindowTypeTemplate) {

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

					if (layout.objectReferenceValue == null && property.serializedObject.targetObject is FlowLayoutWindowTypeTemplate) {

						EditorGUILayout.HelpBox("To use ScaleMode you need to setup `Layout` to store values. You can only use `Preferences` mode without `Layout`.", MessageType.Warning);

						if (mode != UnityEngine.UI.Windows.WindowLayout.ScaleMode.Preferences) {

							mode = UnityEngine.UI.Windows.WindowLayout.ScaleMode.Preferences;
							scaleMode.enumValueIndex = (int)mode;

						}

					}

					EditorGUILayout.PropertyField(allowCustomLayoutPreferences, new GUIContent("Allow Custom Layout Preferences", "This option allows set up custom preferences by the code. If false code set up will do nothing."));

					++EditorGUI.indentLevel;

					var enabled = true;
					if (mode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Normal || // Normal mode
						mode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Fixed || // Fixed mode
						mode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Preferences) { // Custom mode

						if (mode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Fixed) {

							EditorGUILayout.PropertyField(fixedScaleSize, new GUIContent("Resolution:"));

						} else if (mode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Preferences) {

							EditorGUILayout.PropertyField(layoutPreferences, new GUIContent("File Link:"));
							if (layoutPreferences.objectReferenceValue != null) {

								var obj = new SerializedObject(layoutPreferences.objectReferenceValue);
								var fixedScale = obj.FindProperty("fixedScale").boolValue;
								fixedScaleSize.vector2Value = obj.FindProperty("fixedScaleResolution").vector2Value;
								matchWidthOrHeight.floatValue = obj.FindProperty("matchWidthOrHeight").floatValue;

								if (fixedScale == true) {

									mode = UnityEngine.UI.Windows.WindowLayout.ScaleMode.Fixed;

								} else {

									mode = UnityEngine.UI.Windows.WindowLayout.ScaleMode.Normal;

								}

							}

						}

						var layoutSource = layout.objectReferenceValue as UnityEngine.UI.Windows.WindowLayout;
						if (layoutSource != null) {

							layoutSource.SetScale(mode, fixedScaleSize.vector2Value, matchWidthOrHeight.floatValue);

						}

						enabled = false;

					}

					EditorGUI.BeginDisabledGroup(!enabled);
					if (this.canvasScalerEditor != null) this.canvasScalerEditor.OnInspectorGUI();
					EditorGUI.EndDisabledGroup();

					--EditorGUI.indentLevel;

				}

				CustomGUI.Splitter();

				if (this.inited == true && this.window != null && this.window.GetCurrentLayout().layout != null) {
					
					ReorderableListGUI.Title("Components");
					var lastRect = GUILayoutUtility.GetLastRect();
					var rect = new Rect(lastRect.x + lastRect.width - 100f - 4f, lastRect.y, 100f, lastRect.height);
					if (GUI.Button(rect, "Refresh", EditorStyles.toolbarButton) == true) {

						var target = property.serializedObject.targetObject as LayoutWindowType;
						target.OnValidateEditor();

						this.Reset();

					}
					this.elements.Draw(this.adaptor);
					
				}
					
			}
			EditorGUI.EndDisabledGroup();

			//}
			
			//property.serializedObject.ApplyModifiedProperties();
			
		}

	}

}