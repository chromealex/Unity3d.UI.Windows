using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI.Windows.UserInfo;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class AmplitudeService : AnalyticsService {

		public bool isDebug;

		public override string GetServiceName() {

			return "AmplitudeAnalytics";

		}

		public override bool IsSupported() {

			return true;

		}

		public override bool IsConnected() {

			return true;

		}

		public void OnApplicationQuit() {

			#if AMPLITUDE_ANALYTICS_API
			#endif

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			#if AMPLITUDE_ANALYTICS_API
			var amplitude = Amplitude.Instance;
			amplitude.logging = this.isDebug;
			amplitude.trackSessionEvents(true);
			amplitude.init(serviceItem.authKey);
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(long id) {

			#if AMPLITUDE_ANALYTICS_API
			var amplitude = Amplitude.Instance;
			amplitude.setUserId(id.ToString());
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(string id) {

			#if AMPLITUDE_ANALYTICS_API
			var amplitude = Amplitude.Instance;
			amplitude.setUserId(id.ToString());
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			#if AMPLITUDE_ANALYTICS_API
			if (screenId >= 0) {

				var amplitude = Amplitude.Instance;
				amplitude.logEvent(string.Format("Screen: {0} (ID: {1}) - {2}", FlowSystem.GetWindow(screenId).title, screenId, group1), new Dictionary<string, object>() {

					{ "Group1", group1 },
					{ "Group2", group2 },
					{ "Group3", group3 },
					{ "Weight", weight.ToString() },
					{ "CustomParameter", User.instance.customParameter },

				});

			}
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(string eventName, string group1, string group2, string group3, int weight) {

			#if AMPLITUDE_ANALYTICS_API
			var amplitude = Amplitude.Instance;
			amplitude.logEvent(string.Format("{0}: {1}", eventName, group1), new Dictionary<string, object>() {

				{ "Group1", group1 },
				{ "Group2", group2 },
				{ "Group3", group3 },
				{ "Weight", weight.ToString() },
				{ "CustomParameter", User.instance.customParameter },

			});
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenTransition(int index, int screenId, int toScreenId, bool popup) {

			#if AMPLITUDE_ANALYTICS_API
			if (screenId >= 0 && toScreenId >= 0) {

				var amplitude = Amplitude.Instance;
				amplitude.logEvent(
					"ScreenTransition",
					new Dictionary<string, object>() {

						{ "From", FlowSystem.GetWindow(screenId).title },
						{ "FromID", screenId },
						{ "To", FlowSystem.GetWindow(toScreenId).title },
						{ "ToID", toScreenId },
						{ "CustomParameter", User.instance.customParameter },

					}
				);

			}
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			#if AMPLITUDE_ANALYTICS_API
			if (screenId >= 0) {
				
				var amplitude = Amplitude.Instance;
				amplitude.logRevenue(productId, 1, (double)price, receipt, signature, "purchase", new Dictionary<string, object>() {

					{ "Currency", currency },
					{ "Screen", screenId.ToString() },
					{ "CustomParameter", User.instance.customParameter },

				});

			}
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature) {

			#if AMPLITUDE_ANALYTICS_API
			var amplitude = Amplitude.Instance;
			amplitude.logRevenue(productId, 1, (double)price, receipt, signature, "purchase", new Dictionary<string, object>() {

				{ "Currency", currency },
				{ "Event", eventName },
				{ "CustomParameter", User.instance.customParameter },

			});
			#endif
			yield return 0;

		}

		#if UNITY_EDITOR
		private static bool foundType = false;
		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {

			var found = false;
			if (foundType == false) {

				var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				foreach (var ass in assemblies) {

					var type = ass.GetTypes().FirstOrDefault(x => x.Name == "Amplitude");
					if (type != null) {
						
						found = true;
						foundType = true;
						break;

					}

				}

			} else {

				found = foundType;

			}

			if (found == false) {

				UnityEditor.EditorGUILayout.HelpBox("Amplitude Analytics is disabled. To enable Amplitude Analytics download version from official github repo.", UnityEditor.MessageType.Warning);
				if (GUILayout.Button("Open Link", skin.button) == true) {

					Application.OpenURL("https://github.com/amplitude/unity-plugin");

				}

			} else {

				#if AMPLITUDE_ANALYTICS_API
				UnityEditor.EditorGUILayout.HelpBox("Amplitude Analytics is enabled.", UnityEditor.MessageType.Info);
				#else
				UnityEditor.EditorGUILayout.HelpBox("Amplitude Analytics is disabled. To enable, please add `AMPLITUDE_ANALYTICS_API` to your project build settings `Scripting Define Symbols`.", UnityEditor.MessageType.Warning);
				#endif

			}

		}
		#endif

	}

}