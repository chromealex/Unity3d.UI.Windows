using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class GoogleService : AnalyticsService {

		public GoogleAnalyticsV3 googleAnalytics;

		public override string GetServiceName() {

			return "GoogleAnalytics";

		}

		public void OnApplicationQuit() {

			if (this.googleAnalytics == null) return;

			this.googleAnalytics.StopSession();

		}

		public override IEnumerator Auth(string key) {
			
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.StartSession();
			yield return false;

		}

		public override IEnumerator SetUserId(long id) {
			
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.SetUserIDOverride(id.ToString());
			yield return false;

		}

		public override IEnumerator SetUserId(string id) {
			
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.SetUserIDOverride(id.ToString());
			yield return false;

		}

		public override IEnumerator OnEvent(int screenId, string group1, string group2, string group3, int weight) {
			
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.LogEvent(string.Format("Screen: {0} (ID: {1}) - {2}", FlowSystem.GetWindow(screenId).title, screenId, group1), group2, group3, weight);
			yield return false;

		}

		public override IEnumerator OnScreenTransition(int index, int screenId, int toScreenId, bool popup) {
			
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.LogScreen(string.Format("{0} (ID: {1}) to {2} (ID: {3})", FlowSystem.GetWindow(screenId).title, screenId, FlowSystem.GetWindow(toScreenId).title, toScreenId));
			yield return false;

		}
		
		public override IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {
			
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.LogTransaction(string.Format("Screen: {0} (ID: {1})", FlowSystem.GetWindow(screenId).title, screenId), productId, (double)price, 0d, 0d, currency);
			yield return false;
			
		}
		
		#if UNITY_EDITOR
		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {
			
			UnityEditor.EditorGUILayout.HelpBox("To enable Google Analytics download version V3 from official site.", UnityEditor.MessageType.Info);
			if (GUILayout.Button("Open Link", skin.button) == true) {

				Application.OpenURL("https://github.com/googleanalytics/google-analytics-plugin-for-unity/raw/master/googleanalyticsv3.unitypackage");

			}

		}
		#endif

	}

}