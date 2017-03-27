using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class GoogleServiceV4 : AnalyticsService {

		#if GOOGLE_ANALYTICS_V4
		public GoogleAnalyticsV4 googleAnalytics;
		#endif

		public override string GetServiceName() {

			return "GoogleAnalyticsV4";

		}

		public override bool IsSupported() {

			#if GOOGLE_ANALYTICS_V4
			if (Application.systemLanguage == SystemLanguage.Chinese ||
				Application.systemLanguage == SystemLanguage.ChineseSimplified ||
				Application.systemLanguage == SystemLanguage.ChineseTraditional) {

				return false;

			}

			return true;
			#else
			return false;
			#endif

		}

		public override bool IsConnected() {

			return true;

		}

		public void OnApplicationQuit() {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) return;

			this.googleAnalytics.StopSession();
			#endif

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.StartSession();
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(long id) {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.SetUserIDOverride(id.ToString());
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(string id) {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.SetUserIDOverride(id.ToString());
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.LogEvent(string.Format("Screen: {0} (ID: {1}) - {2}", FlowSystem.GetWindow(screenId).title, screenId, group1), group2, group3, weight);
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(string eventName, string group1, string group2, string group3, int weight) {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.LogEvent(string.Format("{0}: {1}", eventName, group1), group2, group3, weight);
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenTransition(int index, int screenId, int toScreenId, bool popup) {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.LogScreen(string.Format("{0} (ID: {1}) to {2} (ID: {3})", FlowSystem.GetWindow(screenId).title, screenId, FlowSystem.GetWindow(toScreenId).title, toScreenId));
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.LogTransaction(string.Format("Screen: {0} (ID: {1})", FlowSystem.GetWindow(screenId).title, screenId), productId, (double)price, 0d, 0d, currency);
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature) {

			#if GOOGLE_ANALYTICS_V4
			if (this.googleAnalytics == null) yield break;

			this.googleAnalytics.LogTransaction(eventName, productId, (double)price, 0d, 0d, currency);
			#endif
			yield return 0;

		}

		#if UNITY_EDITOR
		private bool foundType = false;
		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {

			var found = false;
			if (this.foundType == false) {

				var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				foreach (var ass in assemblies) {

					var type = ass.GetTypes().FirstOrDefault(x => x.Name == "GoogleAnalyticsV4");
					if (type != null) {
						
						found = true;
						this.foundType = true;
						break;

					}

				}

			} else {

				found = this.foundType;

			}

			if (found == false) {

				UnityEditor.EditorGUILayout.HelpBox("Google Analytics is disabled. To enable Google Analytics download version from official github repo.", UnityEditor.MessageType.Warning);
				if (GUILayout.Button("Open Link", skin.button) == true) {

					Application.OpenURL("https://github.com/googleanalytics/google-analytics-plugin-for-unity");

				}

			} else {

				#if GOOGLE_ANALYTICS_V4
				UnityEditor.EditorGUILayout.HelpBox("Google Analytics is enabled.", UnityEditor.MessageType.Info);
				#else
				UnityEditor.EditorGUILayout.HelpBox("Google Analytics is disabled. To enable, please add `GOOGLE_ANALYTICS_V4` to your project build settings `Scripting Define Symbols`.", UnityEditor.MessageType.Warning);
				#endif

			}

		}
		#endif

	}

}