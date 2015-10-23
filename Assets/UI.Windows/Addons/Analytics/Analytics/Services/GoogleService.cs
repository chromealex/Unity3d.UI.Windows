using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class GoogleService : AnalyticsService {

		public override string GetPlatformName() {

			return "GoogleAnalytics";

		}
		
		public override IEnumerator Auth(string key) {

			yield return false;

		}

		public override IEnumerator OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			yield return false;

		}

		public override IEnumerator OnScreenTransition(int index, int screenId, int toScreenId) {
			
			yield return false;

		}
		
		public override IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {
			
			yield return false;
			
		}
		
		#if UNITY_EDITOR
		public override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsItem item, System.Action onReset, GUISkin skin) {
			
			UnityEditor.EditorGUILayout.LabelField("Key:");
			item.authKey = UnityEditor.EditorGUILayout.TextArea(item.authKey, skin.textArea);

		}
		#endif

	}

}