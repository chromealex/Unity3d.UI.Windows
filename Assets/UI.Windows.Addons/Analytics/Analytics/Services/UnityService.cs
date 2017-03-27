using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Linq;
using UnityEngine.UI.Windows.UserInfo;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class UnityService : AnalyticsService {
		
		public override string GetAuthKey(ServiceItem item) {

			return item.authKey;
			
		}

		public override string GetServiceName() {
			
			return "UnityAnalytics";
			
		}

		public override bool IsSupported() {

			return true;

		}

		public override bool IsConnected() {

			// ignore UnityAnalytics
			return true;

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			#if UNITY_ANALYTICS_API
			var rootScreenId = FlowSystem.GetRootWindow();
			var eventName = "Application Start";
            UnityEngine.Analytics.Analytics.CustomEvent(eventName, new Dictionary<string, object>() {
				
				{ "Root Screen", rootScreenId },
				{ "Platform", Application.platform.ToString() }
				
			});
			#endif

			yield return 0;

		}
		
		public override System.Collections.Generic.IEnumerator<byte> SetUserId(string id) {

			#if UNITY_ANALYTICS_API
			UnityEngine.Analytics.Analytics.SetUserId(id.ToString());
			#endif
			
			yield return 0;
			
		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(long id) {

			#if UNITY_ANALYTICS_API
			UnityEngine.Analytics.Analytics.SetUserId(id.ToString());
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserGender(Gender gender) {

			#if UNITY_ANALYTICS_API
			UnityEngine.Analytics.Gender output = UnityEngine.Analytics.Gender.Unknown;

			switch (gender) {
				
				case Gender.Female:
					output = UnityEngine.Analytics.Gender.Female;
					break;
					
				case Gender.Male:
					output = UnityEngine.Analytics.Gender.Male;
					break;
					
				case Gender.Any:
					output = UnityEngine.Analytics.Gender.Unknown;
					break;

			}

			UnityEngine.Analytics.Analytics.SetUserGender(output);
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserBirthYear(int birthYear) {

			#if UNITY_ANALYTICS_API
			UnityEngine.Analytics.Analytics.SetUserBirthYear(birthYear);
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			#if UNITY_ANALYTICS_API
			if (screenId >= 0) {

				var eventName = string.Format("{0}({1})", FlowSystem.GetWindow(screenId).title, screenId);

				UnityEngine.Analytics.Analytics.CustomEvent(eventName, new Dictionary<string, object>() {

					{ "Group1", group1 },
					{ "Group2", group2 },
					{ "Group3", group3 },
					{ "Weight", weight },
					{ "CustomParameter", User.instance.customParameter },

				});

			}
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(string eventName, string group1, string group2, string group3, int weight) {

			#if UNITY_ANALYTICS_API
			UnityEngine.Analytics.Analytics.CustomEvent(eventName, new Dictionary<string, object>() {

				{ "Group1", group1 },
				{ "Group2", group2 },
				{ "Group3", group3 },
				{ "Weight", weight },
				{ "CustomParameter", User.instance.customParameter },

			});
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenTransition(int index, int screenId, int toScreenId, bool popup) {

			#if UNITY_ANALYTICS_API
			if (screenId >= 0 && toScreenId >= 0) {
					
				UnityEngine.Analytics.Analytics.CustomEvent("Screen Transition", new Dictionary<string, object>() {
					
					{ "From", string.Format("{0} (ID: {1})", FlowSystem.GetWindow(screenId).title, screenId) },
					{ "To", string.Format("{0} (ID: {1})", FlowSystem.GetWindow(toScreenId).title, toScreenId) },
					{ "Path", string.Format("Path Index: {0}", index) },
					{ "Popup", string.Format("Popup: {0}", popup) },
					{ "CustomParameter", User.instance.customParameter },
					
				});

			}
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			#if UNITY_ANALYTICS_API
			UnityEngine.Analytics.Analytics.Transaction(productId, price, currency, receipt, signature);
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature) {

			#if UNITY_ANALYTICS_API
			UnityEngine.Analytics.Analytics.Transaction(productId, price, currency, receipt, signature);
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {

			#if UNITY_ANALYTICS_API
			var eventName = string.Format("P{0}", screenId);
			var point = new Vector3(x, y, tag);
			UnityAnalyticsHeatmap.HeatmapEvent.Send(eventName, point);
			#endif
			yield return 0;

		}

		#if UNITY_EDITOR
		public override void GetHeatmapData(int screenId, int screenWidth, int screenHeight, UserFilter filter, System.Action<HeatmapResult> onResult) {

		}

		private bool foundType = false;
		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {

			var found = false;
			if (this.foundType == false) {

				var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				foreach (var ass in assemblies) {

					var type = ass.GetTypes().FirstOrDefault(x => x.Name == "Analytics" && x.Namespace == "UnityEngine.Analytics");
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

				UnityEditor.EditorGUILayout.HelpBox("To enable Unity Analytics open `Window/Unity Services` window, login there and turn on `Analytics`.", UnityEditor.MessageType.Warning);

			} else {

				#if UNITY_ANALYTICS_API
				UnityEditor.EditorGUILayout.HelpBox("Unity Analytics is enabled.", UnityEditor.MessageType.Info);
				#else
				UnityEditor.EditorGUILayout.HelpBox("Unity Analytics is disabled. To enable, please add `UNITY_ANALYTICS_API` to your project build settings `Scripting Define Symbols`.", UnityEditor.MessageType.Warning);
				#endif

			}

		}
		#endif

	}

}