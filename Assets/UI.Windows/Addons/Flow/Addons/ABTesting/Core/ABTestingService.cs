using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.ABTesting.Net.Api;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.ABTesting {

	public interface IABTestingService : IService {

		System.Collections.Generic.IEnumerator<byte> GetData(int testId, System.Action<ABTestResult> onResult);
		System.Collections.Generic.IEnumerator<byte> GetDataAll(System.Action<ABTestsResult> onResult);

		#if UNITY_EDITOR
		void Save(int testId, ABTestingItemsTO data, System.Action<bool> onResult);
		void SaveAll(Dictionary<int, ABTestingItemsTO> testMap, System.Action<bool> onResult);
		#endif

	};
	
	public class Result {
		
		public bool hasError = true;
		
	};
	
	public class ABTestResult : Result {
		
		public ABTestingItemsTO data;
		
	};

	public class ABTestResultJson : Result {

		public string data;

	};

	public class ABTestsResult : Result {

		public Dictionary<int, ABTestingItemsTO> data;

	};

	public class ABTestsResultJson : Result {

		public List<ABTestingJsonItemTO> data;

	};

	public abstract class ABTestingService : ServiceBase, IABTestingService {

		public abstract System.Collections.Generic.IEnumerator<byte> GetData(int testId, System.Action<ABTestResult> onResult);
		public abstract System.Collections.Generic.IEnumerator<byte> GetDataAll(System.Action<ABTestsResult> onResult);

		#if UNITY_EDITOR
		public abstract void Save(int testId, ABTestingItemsTO data, System.Action<bool> onResult);
		public abstract void SaveAll(Dictionary<int, ABTestingItemsTO> testMap, System.Action<bool> onResult);

		protected override void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			this.OnInspectorGUI(settings as ABTestingSettings, item as ABTestingServiceItem, onReset, skin);
			
		}
		
		protected virtual void OnInspectorGUI(ABTestingSettings settings, ABTestingServiceItem item, System.Action onReset, GUISkin skin) {
			
			
			
		}
		#endif

	}

}