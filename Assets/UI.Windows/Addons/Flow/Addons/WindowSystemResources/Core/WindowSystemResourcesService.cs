using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Resources {

	public interface IWindowSystemResourcesService : IService {

		System.Collections.Generic.IEnumerator<byte> GetData(string key, System.Action<WindowSystemResourcesResult> onResult);

	};
	
	public class Result {
		
		public bool hasError = true;
		
	};
	
	public class WindowSystemResourcesResult : Result {
		
		public string data;
		
	};

	public abstract class WindowSystemResourcesService : ServiceBase, IWindowSystemResourcesService {
		
		public virtual System.Collections.Generic.IEnumerator<byte> GetData(string key, System.Action<WindowSystemResourcesResult> onResult) {

			yield return 0;

		}

		#if UNITY_EDITOR
		public virtual void EditorLoad(WindowSystemResourcesSettings settings, WindowSystemResourcesServiceItem item) {
		}

		protected override void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			this.OnInspectorGUI(settings as WindowSystemResourcesSettings, item as WindowSystemResourcesServiceItem, onReset, skin);
			
		}
		
		protected virtual void OnInspectorGUI(WindowSystemResourcesSettings settings, WindowSystemResourcesServiceItem item, System.Action onReset, GUISkin skin) {
			
			
			
		}
		#endif

	}

}