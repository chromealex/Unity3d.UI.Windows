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
		
		private bool inited = false;
		//private ReorderableList elements;
		private ReorderableListControl elements;
		private IReorderableListAdaptor adapter;
		private ReorderableListControl.ItemDrawer<SerializedProperty> onItemDraw;
		private List<UnityEngine.UI.Windows.Types.Layout.Component> components;
		private List<SerializedProperty> items;
		
		private LayoutWindowType window;
		
		public void Init(Rect position, SerializedProperty property, GUIContent label) {
			
			if (property == null) return;
			if (this.inited == true) return;
			
			this.window = (property.serializedObject.targetObject as LayoutWindowType);

			this.items = new List<SerializedProperty>();
			this.components = new List<UnityEngine.UI.Windows.Types.Layout.Component>();
			for (int i = 0; i < property.arraySize; ++i) {
				
				var element = property.GetArrayElementAtIndex(i);
				if (this.window.layout.layout == null) continue;
				
				var rootElement = this.window.layout.layout.GetRootByTag(this.window.layout.components[i].tag);
				if (rootElement != null && rootElement.showInComponentsList == true) {
					
					this.components.Add(this.window.layout.components[i]);
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
			
			this.adapter = new ComponentsListAdaptor<SerializedProperty>(this.items, this.onItemDraw, getHeight);
			
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
				
				var newComponent = component.objectReferenceValue;
				if (oldComponent != newComponent) {
					
					parameters.objectReferenceValue = components[index].OnComponentChanged(window, newComponent as WindowComponent);
					components[index].componentParametersEditor = null;
					
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
							
						}
						
					});

				}
				
			}
			
			title = "Sorting Order";
			rect.y += height;
			height = EditorGUI.GetPropertyHeight(sortingOrder, new GUIContent(title)) + offset;
			rect.height = height - offset;
			if (draw == true) EditorGUI.PropertyField(rect, sortingOrder, new GUIContent(title), true);
			if (sortingOrder.intValue < 0) sortingOrder.intValue = 0;
			
			if (parameters.objectReferenceValue == null && LayoutSettingsEditor.componentParametersSearched[index] == false) {
				
				components[index].OnComponentChanged(window, components[index].component);
				LayoutSettingsEditor.componentParametersSearched[index] = true;
				
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
					var foldout = false;
					var editorFoldout = item.FindPropertyRelative("editorParametersFoldout");
					foldout = editorFoldout.boolValue;
					if (draw == true) {
						
						++EditorGUI.indentLevel;
						editorFoldout.boolValue = foldout = EditorGUI.Foldout(rect, editorFoldout.boolValue, new GUIContent(title));
						--EditorGUI.indentLevel;
						
					}
					
					if (foldout == true) {
						
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
			
			//property.serializedObject.Update();
			
			//var window = (property.serializedObject.targetObject as LayoutWindowType);
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
						this.window.layout.components = new UnityEngine.UI.Windows.Types.Layout.Component[0];
						this.window.OnValidate();
						
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
				
				if (this.inited == true && this.window != null && this.window.layout.layout != null) {
					
					if (GUILayout.Button("Reset") == true) {
						
						this.inited = false;
						
					}
					
					ReorderableListGUI.Title("Components");
					this.elements.Draw(this.adapter);
					
				}
				
			}
			
			property.serializedObject.ApplyModifiedProperties();
			
		}

	}

}