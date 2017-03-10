using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Services {
	
	public interface IServiceBase {
		
		string GetServiceName();
		
	};

	public interface IService : IServiceBase {
		
		bool isActive { set; get; }

		bool IsSupported();

		string GetAuthKey(ServiceItem item);
		AuthKeyPermissions GetAuthPermissions();
		
		System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem);

		#if UNITY_EDITOR
		void OnEditorAuth(string key, System.Action<bool> onResult);
		void OnEditorDisconnect(System.Action<bool> onResult);
		void DrawInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin);
		#endif
		
	};

	public abstract class ServiceBase : MonoBehaviour, IService {
		
		public bool isActive { set; get; }
		
		public ServiceManagerBase serviceManager;
		
		public virtual void Awake() {

			if (this.serviceManager == null) return;
			this.serviceManager.Register(this);
			
		}
		
		public virtual string GetAuthKey(ServiceItem item) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) {
				
				return FlowSystem.GetData().GetAuthKeyEditor();
				
			} else {
				
				return FlowSystem.GetData().GetAuthKeyBuild();
				
			}
			#else
			return FlowSystem.GetData().GetAuthKeyBuild();
			#endif
			
		}

		public virtual AuthKeyPermissions GetAuthPermissions() {
			
			return AuthKeyPermissions.None;
			
		}

		public abstract bool IsSupported();

		public virtual System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {
			
			yield return 0;
			
		}

		public virtual void OnApplicationPause(bool paused) {
		}

		public abstract string GetServiceName();
		
		#if UNITY_EDITOR
		public virtual void OnEditorAuth(string key, System.Action<bool> onResult) {
			
			if (onResult != null) onResult.Invoke(false);
			
		}
		
		public virtual void OnEditorDisconnect(System.Action<bool> onResult) {
			
			if (onResult != null) onResult.Invoke(false);
			
		}

		public void DrawInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			if (FlowSystem.GetData() == null) {

				GUILayout.Label("No Data");
				return;

			}

			++UnityEditor.EditorGUI.indentLevel;
			try {

				if (this.GetAuthPermissions() != AuthKeyPermissions.None && FlowSystem.GetData().IsValidAuthKey() == false) {
					
					UnityEditor.EditorGUILayout.HelpBox("Authorization Key is not valid.", UnityEditor.MessageType.Error);
					FlowSystem.DrawEditorGetKeyButton(skin);
					
				} else if (FlowSystem.GetData().IsValidAuthKey(this.GetAuthPermissions()) == false) {
					
					UnityEditor.EditorGUILayout.HelpBox("You have no permission to use this service.", UnityEditor.MessageType.Warning);
					FlowSystem.DrawEditorGetKeyButton(skin);
					
				} else {

					this.DoInspectorGUI(settings, item, onReset, skin);

				}
				
			} catch (UnityException e) {

				Debug.LogException(e);

			}
			--UnityEditor.EditorGUI.indentLevel;

		}
		
		protected abstract void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin);
		
		public virtual void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			if (this.serviceManager == null) this.serviceManager = this.GetComponent<ServiceManagerBase>();
			
		}
		#endif

	}

}