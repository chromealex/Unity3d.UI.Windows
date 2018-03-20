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

			//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("Get: " + @namespace);
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
					//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log(string.Format("{0}.{1}, {2}", @namespace, element, assembly.FullName));

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

		public void Init<TService>() where TService : ServiceItem, new() {

			//EditorApplication.delayCall += () => {

			var target = this.target as ServiceSettings;
			var items = target.GetItems();
			this.services = Services.GetList(this.GetNamespace());
			//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log(string.Format("Services found: {0}, Current count: {1}", this.services.Count, items == null ? 0 : items.Count));
			var count = this.services.Count;
			if (items == null || count != items.Count) {

				var newServices = new List<IService>();
				for (int i = 0; i < count; ++i) {

					if (target.HasService(this.services[i].GetServiceName()) == false) {

						newServices.Add(this.services[i]);

					}

				}

				for (int i = 0; i < newServices.Count; ++i) {

					//target.AddService(newServices[i] as ServiceItem);
					target.AddService<TService>(newServices[i]);
					//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log(string.Format("ServiceSettings: New service added with the name `{0}`", newServices[i].GetServiceName()));

				}

			}
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
				this.Init<TService>();
				return;
				
			}

			if (GUILayout.Button("Refresh") == true) this.Init<TService>();

			var items = target.GetItems();
			
			GUILayout.Label("Services", EditorStyles.boldLabel);
			if (items.Count == 0) {
				
				EditorGUILayout.HelpBox(string.Format("No {0} Services were found.", this.GetServiceName()), MessageType.Warning);
				
			} else {
				
				for (int i = 0; i < items.Count; ++i) {
					
					var item = items[i];
					GUILayout.BeginVertical(EditorStyles.helpBox);
					{
						
						var newEnabled = EditorGUILayout.ToggleLeft(item.serviceName.ToSentenceCase().UppercaseWords(), item.enabled, EditorStyles.boldLabel);
						if (newEnabled != item.enabled) {
							
							item.enabled = newEnabled;
							EditorUtility.SetDirty(target);
							
						}
						
						if (item.enabled == true) {
							
                            var service = this.services.FirstOrDefault(x => x.GetServiceName() == item.serviceName);
							if (service != null) service.DrawInspectorGUI(target, item, this.onReset, FlowSystemEditorWindow.defaultSkin);
							
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