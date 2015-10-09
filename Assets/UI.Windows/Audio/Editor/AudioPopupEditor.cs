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

							options[k] = new GUIContent(i >= 0 ? string.Format(category + "[{0}] {1}", keys[k], name) : name);

							++k;

						}
						
						EditorGUI.IntPopup(position, property, options, keys, label);

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