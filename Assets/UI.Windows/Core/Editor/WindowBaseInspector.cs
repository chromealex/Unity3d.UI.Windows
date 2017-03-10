using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI.Windows;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI.Windows {

	[CustomEditor(typeof(WindowBase), editorForChildClasses: true)]
	public class WindowBaseInspector : Editor {

		private AnimBool fold = new AnimBool();

		public override void OnInspectorGUI() {

			this.serializedObject.Update();
			
			var so = this.serializedObject;
			var win = so.targetObject as WindowBase;
			var isPrefab = ME.EditorUtilities.IsPrefab(win.gameObject);

			GUILayout.Label(string.Empty, GUILayout.Height(0f));
			var lastRect = GUILayoutUtility.GetLastRect();

			var iconSize = 18f;
			var rectIcon = new Rect(0f, lastRect.y, iconSize, iconSize);
			var infoStyle = ME.Utilities.CacheStyle("WindowBaseInspector", "InfoButton", (name) => {
				
				var style = new GUIStyle("U2D.pivotDot");
				style.alignment = TextAnchor.MiddleCenter;
				style.fontStyle = FontStyle.Bold;
				style.normal.textColor = Color.blue;
				return style;
				
			});
			
			var miniLabelStyle = ME.Utilities.CacheStyle("WindowBaseInspector", "MiniLabel", (name) => {
				
				var style = new GUIStyle(EditorStyles.miniLabel);
				//style.normal.textColor = Color.grey;
				style.fixedHeight = 16f;
				style.stretchHeight = false;
				
				return style;
				
			});
			
			var backStyle = ME.Utilities.CacheStyle("WindowBaseInspector", "BoxStyle", (name) => {
				
				var style = new GUIStyle("ChannelStripAttenuationBar");
				style.border = new RectOffset(0, 0, 0, 4);
				style.fixedHeight = 0f;

				return style;
				
			});

			if (GUI.Button(rectIcon, "i", infoStyle) == true) {

				win.editorInfoFold = !win.editorInfoFold;
				
			}

			this.fold.target = win.editorInfoFold;

			if (EditorGUILayout.BeginFadeGroup(this.fold.faded) == true) {

				// Call variations
				var methods = this.serializedObject.targetObject.GetType().GetMethods().Where((info) => info.IsVirtual == false && info.Name == "OnParametersPass").ToList();
				var callVariationsCount = methods.Count();
				var hasEmptyCall = this.serializedObject.targetObject.GetType().GetMethods().Where((info) => info.IsVirtual == true && info.GetBaseDefinition() != info && info.Name == "OnEmptyPass").Count() > 0;

				if (callVariationsCount > 0 || hasEmptyCall == true) {
					
					GUILayout.Label(string.Format("Call Variations Count: {0}", callVariationsCount));
					GUILayout.Label(string.Format("Empty Calls: {0}", (hasEmptyCall == true ? "True" : "False")));
					
					var boxStyle = new GUIStyle(EditorStyles.helpBox);
					boxStyle.stretchHeight = true;
					boxStyle.fixedHeight = 0f;
					boxStyle.wordWrap = true;

					EditorGUILayout.BeginVertical();
					{

						for (int i = 0; i < callVariationsCount; ++i) {
							
							var method = methods[i];
							var parameters = method.GetParameters();
							var list = new List<string>();
							foreach (var p in parameters) {

								list.Add(string.Format("<color=#000080fff>{0}</color> {1}", ME.Utilities.FormatParameter(p.ParameterType), p.Name));

							}

							EditorGUILayout.BeginVertical(boxStyle);
							{

								var labelStyle = ME.Utilities.CacheStyle("WindowBaseInspector", "label", (name) => {

									var style = new GUIStyle(EditorStyles.largeLabel);
									style.richText = true;
									style.stretchHeight = false;
									style.fixedHeight = 100f;
									style.wordWrap = true;
									return style;

								});
								EditorGUILayout.SelectableLabel(string.Join("\n", list.ToArray()), labelStyle, GUILayout.Height(100f));
								
							}
							EditorGUILayout.EndVertical();

						}

					}
					EditorGUILayout.EndVertical();

				} else {
					
					EditorGUILayout.HelpBox("Selected window has no `OnParametersPass` and/or `OnEmptyPass` methods.", MessageType.Info);

				}

			}
			EditorGUILayout.EndFadeGroup();

			var activeState = so.FindProperty("activeState");
			var currentState = so.targetObject.GetType().GetMethod("GetState");//.FindProperty("currentState");
			var dragState = so.FindProperty("dragState");
			var pausedState = so.FindProperty("paused");

			if (activeState != null && dragState != null && currentState != null && pausedState != null) {

				GUILayout.BeginHorizontal(backStyle);
				{
					
					GUILayout.BeginVertical();
					{
						GUILayout.Label("Active State", miniLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
						GUILayout.Label(activeState.enumNames[activeState.enumValueIndex], isPrefab == true ? EditorStyles.label : EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					{
						GUILayout.Label("Window State", miniLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
						GUILayout.Label(currentState.Invoke(so.targetObject, null).ToString()/*currentState.enumNames[currentState.enumValueIndex]*/, isPrefab == true ? EditorStyles.label : EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
					}
					GUILayout.EndVertical();
					
					if (win.preferences.draggable == true) {

						GUILayout.BeginVertical();
						{
							GUILayout.Label("Drag State", miniLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
							GUILayout.Label(dragState.enumNames[dragState.enumValueIndex], isPrefab == true ? EditorStyles.label : EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
						}
						GUILayout.EndVertical();

					}

					GUILayout.BeginVertical();
					{
						GUILayout.Label("Paused State", miniLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
						GUILayout.Label(pausedState.boolValue.ToString(), isPrefab == true ? EditorStyles.label : EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
					}
					GUILayout.EndVertical();

				}
				GUILayout.EndHorizontal();

			}

			ME.EditorUtilitiesEx.DrawInspector(this, typeof(WindowBase));

			// Draw default
			//this.DrawDefaultInspector();

			this.serializedObject.ApplyModifiedProperties();

		}

	}

}