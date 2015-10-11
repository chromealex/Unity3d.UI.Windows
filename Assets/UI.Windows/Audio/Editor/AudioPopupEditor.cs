using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Audio;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using System;
using UnityEditor.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Audio {

	[CustomPropertyDrawer(typeof(AudioPopupAttribute))]
	public class AudioPopupEditor : PropertyDrawer {
		
		public static int Draw(Rect rect, int id, ClipType clipType, UnityEngine.UI.Windows.Audio.Data data, GUIContent label) {
			
			return AudioPopupEditor.Draw_INTERNAL(id, clipType, data, label, rect, layout: false);
			
		}

		public static int DrawLayout(int id, ClipType clipType, UnityEngine.UI.Windows.Audio.Data data, GUIContent label) {

			return AudioPopupEditor.Draw_INTERNAL(id, clipType, data, label, new Rect(), layout: true);

		}

		private static int Draw_INTERNAL(int id, ClipType clipType, UnityEngine.UI.Windows.Audio.Data data, GUIContent label, Rect rect, bool layout) {

			if (data != null) {
				
				var states = data.GetStates(clipType);
				var keys = new int[states.Count + 1];
				var options = new GUIContent[states.Count + 1];
				
				var k = 0;
				for (int i = -1; i < states.Count; ++i) {
					
					var category = string.Empty;
					var name = string.Empty;
					if (i == -1) {
						
						name = "None";
						
					} else {
						
						category = states[i].category;
						keys[k] = states[i].key;
						var clip = states[i].clip;
						name = clip == null ? "Null" : clip.ToString();
						
					}
					
					if (string.IsNullOrEmpty(category) == false) {
						
						category = category.Trim('/').Trim() + "/";
						
					}
					
					options[k] = new GUIContent((i >= 0) ? string.Format(category + "[{0}] {1}", keys[k], name) : name);
					
					++k;
					
				}

				if (layout == true) {

					if (label == null) {
						
						id = EditorGUILayout.IntPopup(id, options, keys);

					} else {

						id = EditorGUILayout.IntPopup(label, id, options, keys);

					}

				} else {

					if (label == null) {
						
						id = EditorGUI.IntPopup(rect, id, options, keys);

					} else {

						id = EditorGUI.IntPopup(rect, label, id, options, keys);

					}

				}

			}

			return id;

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			
			var attribute = this.GetAttribute<AudioPopupAttribute>();
			if (attribute != null) {

				var serializedObject = property.serializedObject;
				if (serializedObject.isEditingMultipleObjects == false) {

					ClipType clipType = attribute.clipType;
					if (string.IsNullOrEmpty(attribute.fieldName) == false) {

						var prop = property.GetRelativeProperty(property.propertyPath, attribute.fieldName);
						clipType = (ClipType)((KeyValuePair<int, string>)prop.GetRawValue(attribute)).Key;

					}

					Data data = null;
					var flowData = property.GetRelativeProperty(property.propertyPath, "flowData");
					if (flowData != null) {
						
						var _data = flowData.GetRawValue(attribute) as UnityEngine.UI.Windows.Plugins.Flow.FlowData;
						if (_data != null) data = _data.audio;

					}

					if (data != null) {

						property.intValue = AudioPopupEditor.Draw(position, property.intValue, clipType, data, label);
						property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

					} else {

						EditorGUI.PropertyField(position, property, label);

					}

				}

			} else {
				
				EditorGUI.PropertyField(position, property, label);

			}

		}

	}

}