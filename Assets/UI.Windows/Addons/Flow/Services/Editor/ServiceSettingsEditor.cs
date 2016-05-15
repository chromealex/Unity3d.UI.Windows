using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Collections.Generic;
using System;
using ME;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;

namespace UnityEditor.UI.Windows.Plugins.Services {
	
	public static class Services {
		
		public static List<IService> GetList(string @namespace) {
			
			var output = new List<IService>();

			//Debug.Log("Get: " + @namespace);
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				
				var query = from t in assembly.GetTypes()
					where t.IsClass && t.IsNested == false && t.Namespace == @namespace
						select t.Name.ToLower();
				
				foreach (var element in query) {
					
					var go = new GameObject(string.Format("[Temp] {0}", element.ToLower()));
					go.hideFlags = HideFlags.HideAndDontSave;

					var instance = go.AddComponent(System.Type.GetType(string.Format("{0}.{1}, {2}", @namespace, element, assembly.FullName), throwOnError: false, ignoreCase: true)) as IService;
					if (instance != null) output.Add(instance);
					//Debug.Log(string.Format("{0}.{1}, {2}", @namespace, element, assembly.FullName));
				}
				
			}
			
			return output;
			
		}
		
	}

	public abstract class ServiceSettingsEditor : Editor {
		
		private System.Action onReset;

		public List<IService> services = new List<IService>();

		public abstract string GetNamespace();
		public abstract string GetServiceName();

		public void Init() {

			//EditorApplication.delayCall += () => {

				this.services = Services.GetList(this.GetNamespace());
				
				//this.Repaint();
				
			//};
			
		}
		
		public void SetResetCallback(System.Action onReset) {
			
			this.onReset = onReset;
			
		}

		public void DrawServices<TService>() where TService : ServiceItem, new() {
			
			var target = this.target as ServiceSettings;
			
			var count = this.services.Count;
			if (count == 0) {

				GUILayout.Label("No Services Found");
				this.Init();
				return;
				
			}

			var items = target.GetItems();
			if (items == null || count != items.Count) {
				
				var newServices = new List<IService>();
				for (int i = 0; i < this.services.Count; ++i) {
					
					if (target.HasService(this.services[i].GetServiceName()) == false) {
						
						newServices.Add(this.services[i]);
						
					}
					
				}
				
				for (int i = 0; i < newServices.Count; ++i) {
					
					target.AddService<TService>(newServices[i]);
					
				}

				return;
				
			}
			
			GUILayout.Label("Services", EditorStyles.boldLabel);
			if (items.Count == 0) {
				
				EditorGUILayout.HelpBox(string.Format("No {0} Services were found.", this.GetServiceName()), MessageType.Warning);
				
			} else {
				
				for (int i = 0; i < items.Count; ++i) {
					
					var item = items[i];
					GUILayout.BeginVertical(EditorStyles.helpBox);
					{
						
						var newEnabled = EditorGUILayout.ToggleLeft(this.services[i].GetServiceName().ToSentenceCase().UppercaseWords(), item.enabled, EditorStyles.boldLabel);
						if (newEnabled != item.enabled) {
							
							item.enabled = newEnabled;
							EditorUtility.SetDirty(target);
							
						}
						
						if (item.enabled == true) {
							
							this.services[i].DrawInspectorGUI(target, item, this.onReset, FlowSystemEditorWindow.defaultSkin);
							
						}
						
					}
					GUILayout.EndVertical();
					
				}
				
			}
			
			if (GUI.changed == true) {

				target.SetChanged();
				EditorUtility.SetDirty(target);
				
			}
			
		}

	}

}