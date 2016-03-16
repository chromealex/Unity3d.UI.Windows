using UnityEngine;
using System.Collections;
using UnityEditor.UI.Windows;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI.Windows;

namespace ME {

	public partial class EditorUtilitiesEx {

		public static void DrawInspector(WindowComponentBaseEditor wcEditor) {

			var labelStyle = ME.Utilities.CacheStyle("UI.Windows.EditorUtilitiesEx.labelStyle", "labelStyle", (name) => {

				var _style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
				return _style;

			});

			var so = new SerializedObject(wcEditor.targets);
			var target = wcEditor.target;
			
			so.Update();

			var baseTypes = new List<System.Type>();
			var baseType = target.GetType();
			baseTypes.Add(baseType);
			while (typeof(WindowComponentBase) != baseType) {

				baseType = baseType.BaseType;
				baseTypes.Add(baseType);

			}
			baseTypes.Reverse();

			SerializedProperty prop = so.GetIterator();
			prop.NextVisible(true);

			EditorGUILayout.PropertyField(prop, false);

			var currentType = EditorUtilitiesEx.FindTypeByProperty(baseTypes, prop);
			EditorGUILayout.BeginVertical();
			{
				while (prop.NextVisible(false) == true) {
					
					var cType = EditorUtilitiesEx.FindTypeByProperty(baseTypes, prop);
					if (cType != currentType) {
						
						currentType = cType;

						var name = cType.Name;
						if (name == "WindowComponentBase") continue;

						var size = labelStyle.CalcSize(new GUIContent(name.ToSentenceCase()));
						GUILayout.Label(name.ToSentenceCase().UppercaseWords(), labelStyle);
						var lastRect = GUILayoutUtility.GetLastRect();
						CustomGUI.Splitter(new Rect(lastRect.x, lastRect.y + lastRect.height * 0.5f, lastRect.width * 0.5f - size.x * 0.5f, 1f));
						CustomGUI.Splitter(new Rect(lastRect.x + lastRect.width * 0.5f + size.x * 0.5f, lastRect.y + lastRect.height * 0.5f, lastRect.width * 0.5f - size.x * 0.5f, 1f));

					}
					EditorGUILayout.PropertyField(prop, true);

				}
				prop.Reset();
			}
			EditorGUILayout.EndVertical();

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