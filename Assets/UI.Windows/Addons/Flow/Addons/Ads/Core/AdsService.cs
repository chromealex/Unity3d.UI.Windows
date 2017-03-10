using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Ads {

	public interface IAdsService : IService {

		bool CanShow();
		System.Collections.Generic.IEnumerator<byte> Show(string name, Dictionary<object, object> options = null, System.Action onFinish = null, System.Action onFailed = null, System.Action onSkipped = null);

		bool IsConnected();

	};

	public abstract class AdsService : ServiceBase, IAdsService {
		
		public virtual bool CanShow() {

			return false;

		}

		public virtual System.Collections.Generic.IEnumerator<byte> Show(string name, Dictionary<object, object> options = null, System.Action onFinish = null, System.Action onFailed = null, System.Action onSkipped = null) {

			yield return 0;

		}

		public abstract bool IsConnected();

		#if UNITY_EDITOR
		protected override void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			this.OnInspectorGUI(settings as AdsSettings, item as AdsServiceItem, onReset, skin);

		}

		protected virtual void OnInspectorGUI(AdsSettings settings, AdsServiceItem item, System.Action onReset, GUISkin skin) {



		}
		#endif

	}

}