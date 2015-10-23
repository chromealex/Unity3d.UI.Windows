using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Analytics;
using ME;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace UnityEditor.UI.Windows.Plugins.Heatmap {

	[CustomEditor(typeof(HeatmapSettings))]
	public class HeatmapSettingsEditor : Editor {

		private System.Action onReset;
		public List<IAnalyticsService> services = new List<IAnalyticsService>();

		public void Init() {

			EditorApplication.delayCall += () => {

				this.services.Clear();

				string @namespace = "UnityEngine.UI.Windows.Plugins.Analytics.Services";

				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in assemblies) {
					
					var query = from t in assembly.GetTypes()
						where t.IsClass && t.IsNested == false && t.Namespace == @namespace
						select t.Name.ToLower();
					
					foreach (var element in query) {

						var go = new GameObject("[Temp] " + element.ToLower());
						go.hideFlags = HideFlags.HideAndDontSave;
						var instance = go.AddComponent(System.Type.GetType(@namespace + "." + element + ", " + assembly.FullName, throwOnError: false, ignoreCase: true)) as IAnalyticsService;
						this.services.Add(instance);
						/*
						var instance = System.Activator.CreateInstance(assembly.FullName,
					                                               @namespace + "." + element.ToLower(),
					                                               true,
					                                               BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
					                                               null, null, null, null, null);
						
						if (instance != null) {
							
							var service = (IAnalyticsService)instance.Unwrap();
							if (service != null) {

								this.services.Add(service);

							}

						}*/
						
					}

				}

				this.Repaint();

			};

		}

		public void SetResetCallback(System.Action onReset) {

			this.onReset = onReset;

		}

		public override void OnInspectorGUI() {
			
			var target = this.target as HeatmapSettings;

			var count = this.services.Count;
			if (count == 0) {

				this.Init();
				return;

			}

			if (target.keys == null || count != target.keys.Count) {

				var newServices = new List<IAnalyticsService>();
				for (int i = 0; i < this.services.Count; ++i) {

					if (target.HasService(this.services[i].GetPlatformName()) == false) {

						newServices.Add(this.services[i]);

					}

				}

				for (int i = 0; i < newServices.Count; ++i) {
					
					target.AddService(newServices[i]);
					
				}
				
			}

			GUILayout.Label("Services", EditorStyles.boldLabel);
			if (target.keys.Count == 0) {

				EditorGUILayout.HelpBox("No Analytic Services were found.", MessageType.Warning);

			} else {

				for (int i = 0; i < target.keys.Count; ++i) {

					var item = target.keys[i];
					EditorGUILayout.BeginVertical(EditorStyles.helpBox);
					{

						var newEnabled = EditorGUILayout.ToggleLeft(this.services[i].GetPlatformName().ToSentenceCase().UppercaseWords(), item.enabled, EditorStyles.boldLabel);
						if (newEnabled != item.enabled) {

							item.enabled = newEnabled;
							EditorUtility.SetDirty(target);

						}

						if (item.enabled == true) {

							++EditorGUI.indentLevel;
							try {

								this.services[i].OnInspectorGUI(target, item, this.onReset, FlowSystemEditorWindow.defaultSkin);

							} catch (UnityException) {
							}
							--EditorGUI.indentLevel;

						}

					}
					EditorGUILayout.EndVertical();

				}

			}

			if (GUI.changed == true) {

				target.SetChanged();
				EditorUtility.SetDirty(target);

			}

		}

	}

}