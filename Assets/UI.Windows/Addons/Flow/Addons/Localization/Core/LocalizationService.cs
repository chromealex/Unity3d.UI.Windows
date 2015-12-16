using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Localization {

	public interface ILocalizationService : IService {

		IEnumerator GetData(string key, System.Action<LocalizationResult> onResult);

	};
	
	public class Result {
		
		public bool hasError = true;
		
	};
	
	public class LocalizationResult : Result {
		
		public string data;
		
	};

	public abstract class LocalizationService : ServiceBase, ILocalizationService {

		public virtual IEnumerator GetData(string key, System.Action<LocalizationResult> onResult) {

			yield return false;

		}

		#if UNITY_EDITOR
		protected override void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			this.OnInspectorGUI(settings as LocalizationSettings, item as LocalizationServiceItem, onReset, skin);
			
		}
		
		protected virtual void OnInspectorGUI(LocalizationSettings settings, LocalizationServiceItem item, System.Action onReset, GUISkin skin) {
			
			
			
		}
		#endif

	}

}