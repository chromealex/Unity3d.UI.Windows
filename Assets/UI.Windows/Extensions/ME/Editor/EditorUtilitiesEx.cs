using UnityEngine;
using System.Collections;
using UnityEditor.UI.Windows;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI.Windows;

namespace ME {

	public partial class EditorUtilitiesEx {

		private static GUIStyle GetLabelStyle() {
			
			return ME.Utilities.CacheStyle("UI.Windows.EditorUtilitiesEx.labelStyle", "labelStyle", (name) => {
				
				var _style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
				return _style;
				
			});

		}

		public static void DrawSplitter(string label) {

			var splitted = label.Split('`');
			if (splitted.Length > 1) {

				label = splitted[0];

			}

			var labelStyle = EditorUtilitiesEx.GetLabelStyle();
			var size = labelStyle.CalcSize(new GUIContent(label.ToSentenceCase()));
			GUILayout.Label(label.ToSentenceCase().UppercaseWords(), labelStyle);
			var lastRect = GUILayoutUtility.GetLastRect();
			CustomGUI.Splitter(new Rect(lastRect.x, lastRect.y + lastRect.height * 0.5f, lastRect.width * 0.5f - size.x * 0.5f, 1f));
			CustomGUI.Splitter(new Rect(lastRect.x + lastRect.width * 0.5f + size.x * 0.5f, lastRect.y + lastRect.height * 0.5f, lastRect.width * 0.5f - size.x * 0.5f, 1f));

		}

		public static void DrawInspector(Editor wcEditor, System.Type baseType, List<string> ignoreClasses = null) {

			var so = new SerializedObject(wcEditor.targets);
			var target = wcEditor.target;
			
			so.Update();

			var baseTypes = new List<System.Type>();
			var baseTargetType = target.GetType();
			baseTypes.Add(baseTargetType);
			while (baseType != baseTargetType) {

				baseTargetType = baseTargetType.BaseType;
				baseTypes.Add(baseTargetType);

			}
			baseTypes.Reverse();

			SerializedProperty prop = so.GetIterator();
			var result = prop.NextVisible(true);

			if (result == true) {

				EditorGUILayout.PropertyField(prop, false);

				var currentType = EditorUtilitiesEx.FindTypeByProperty(baseTypes, prop);
				EditorGUILayout.BeginVertical();
				{

					while (prop.NextVisible(false) == true) {
						
						var cType = EditorUtilitiesEx.FindTypeByProperty(baseTypes, prop);
						if (cType != currentType) {
							
							currentType = cType;

							var name = cType.Name;
							if (ignoreClasses != null && ignoreClasses.Contains(name) == true) continue;

							EditorUtilitiesEx.DrawSplitter(name);

						}

						EditorGUILayout.PropertyField(prop, true);

					}

					prop.Reset();

				}
				EditorGUILayout.EndVertical();

			}

			so.ApplyModifiedProperties();

		}

		public static System.Type FindTypeByProperty(List<System.Type> types, SerializedProperty prop) {

			foreach (var type in types) {

				var field = type.GetField(prop.propertyPath, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
				if (field != null) {

					return type;

				}

			}

			return null;

		}

	}

}