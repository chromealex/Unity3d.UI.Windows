using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Localization {

	public interface ILocalizationService : IService {

		System.Collections.Generic.IEnumerator<byte> GetData(LocalizationSettings settings, System.Action<LocalizationResult> onResult);

	};
	
	public class Result {
		
		public bool hasError = true;
		public string errorText;
		
	};
	
	public class LocalizationResult : Result {
		
		public string data;
		
	};

	public abstract class LocalizationService : ServiceBase, ILocalizationService {
		
		public virtual System.Collections.Generic.IEnumerator<byte> GetData(LocalizationSettings settings, System.Action<LocalizationResult> onResult) {

			yield return 0;

		}

		#if UNITY_EDITOR
		public virtual void EditorLoad(LocalizationSettings settings, LocalizationServiceItem item) {
		}

		protected override void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			this.OnInspectorGUI(settings as LocalizationSettings, item as LocalizationServiceItem, onReset, skin);
			
		}
		
		protected virtual void OnInspectorGUI(LocalizationSettings settings, LocalizationServiceItem item, System.Action onReset, GUISkin skin) {
			
			
			
		}
		#endif

	}

}