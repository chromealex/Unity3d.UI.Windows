using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Linq;
using System;
using UnityEngine.UI.Windows.UserInfo;

#if DELTADNA_ANALYTICS_API
using DeltaDNA;
#endif

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class DeltaDNAService : AnalyticsService {

		public string environmentKey;
		public string collectUrl;
		public string engageUrl;

		public override string GetAuthKey(ServiceItem item) {

			return item.authKey;
			
		}

		public override string GetServiceName() {
			
			return "DeltaDNAAnalytics";
			
		}

		public override bool IsSupported() {

			return true;

		}

		public override bool IsConnected() {
			
			#if DELTADNA_ANALYTICS_API
			return DDNA.Instance.HasStarted;
			#else
			return true;
			#endif

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {
			
			#if DELTADNA_ANALYTICS_API
			DDNA.Instance.ClientVersion = User.instance.version;
			DDNA.Instance.StartSDK(
				this.environmentKey,
				this.collectUrl,
				this.engageUrl,
				User.instance.id2
			);
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			#if DELTADNA_ANALYTICS_API
			if (screenId >= 0) {

				var eventName = string.Format("{0}({1})", FlowSystem.GetWindow(screenId).title, screenId);
				var gameEvent = new GameEvent(eventName)
					.AddParam("Group1", group1)
					.AddParam("Group2", group2)
					.AddParam("Group3", group3)
					.AddParam("Weight", weight)
					.AddParam("CustomParameter", User.instance.customParameter);

				DDNA.Instance.RecordEvent(gameEvent);
			    
			}
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(string eventName, string group1, string group2, string group3, int weight) {

			#if DELTADNA_ANALYTICS_API
			var gameEvent = new GameEvent(eventName)
				.AddParam("Group1", group1)
				.AddParam("Group2", group2)
				.AddParam("Group3", group3)
				.AddParam("Weight", weight)
				.AddParam("CustomParameter", User.instance.customParameter);

			DDNA.Instance.RecordEvent(gameEvent);
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenTransition(int index, int screenId, int toScreenId, bool popup) {

			#if DELTADNA_ANALYTICS_API
			if (screenId >= 0 && toScreenId >= 0) {

				var eventName = "Screen Transition";
				var gameEvent = new GameEvent(eventName)
					.AddParam("From", string.Format("{0} (ID: {1})", FlowSystem.GetWindow(screenId).title, screenId))
					.AddParam("To", string.Format("{0} (ID: {1})", FlowSystem.GetWindow(toScreenId).title, toScreenId))
					.AddParam("Path", string.Format("Path Index: {0}", index))
					.AddParam("Popup", string.Format("Popup: {0}", popup))
					.AddParam("CustomParameter", User.instance.customParameter);

				DDNA.Instance.RecordEvent(gameEvent);

			}
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			#if DELTADNA_ANALYTICS_API
			if (screenId >= 0) {

				var gameEvent = new Transaction(
					string.Format("{0} (ID: {1}), product: {2}", FlowSystem.GetWindow(screenId).title, screenId, productId),
					"PURCHASE",
					new Product(),
					new Product().SetRealCurrency(currency, (int)price)
				)
					.SetProductId(productId)
					.SetReceipt(receipt)
					.SetReceiptSignature(signature)
					.SetServer(WindowSystem.GetCurrentRuntimePlatform().ToString())
					.AddParam("CustomParameter", User.instance.customParameter);

				DDNA.Instance.RecordEvent(gameEvent);

			}
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature) {

			#if DELTADNA_ANALYTICS_API
			var gameEvent = new Transaction(
				string.Format("{0}, product: {2}", eventName, productId),
				"PURCHASE",
				new Product(),
				new Product().SetRealCurrency(currency, (int)price)
			)
				.SetProductId(productId)
				.SetReceipt(receipt)
				.SetReceiptSignature(signature)
				.SetServer(WindowSystem.GetCurrentRuntimePlatform().ToString())
				.AddParam("CustomParameter", User.instance.customParameter);

			DDNA.Instance.RecordEvent(gameEvent);
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {

			yield return 0;

		}

		#if UNITY_EDITOR
		public override void GetHeatmapData(int screenId, int screenWidth, int screenHeight, UserFilter filter, System.Action<HeatmapResult> onResult) {

		}

		private static bool foundType = false;
		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {

			var found = false;
			if (foundType == false) {

				var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				foreach (var ass in assemblies) {

					var type = ass.GetTypes().FirstOrDefault(x => x.Name == "DDNA" && x.Namespace == "DeltaDNA");
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

				UnityEditor.EditorGUILayout.HelpBox("To enable DeltaDNA Analytics go to DeltaDNA web site and download Unity SDK.", UnityEditor.MessageType.Warning);
				if (GUILayout.Button("Open Link", skin.button) == true) {

					Application.OpenURL("http://docs.deltadna.com/advanced-integration/unity-sdk/");

				}

			} else {

				#if DELTADNA_ANALYTICS_API
				UnityEditor.EditorGUILayout.HelpBox("DeltaDNA is enabled.", UnityEditor.MessageType.Info);
				#else
				UnityEditor.EditorGUILayout.HelpBox("DeltaDNA is disabled. To enable, please add `DELTADNA_ANALYTICS_API` to your project build settings `Scripting Define Symbols`.", UnityEditor.MessageType.Warning);
				#endif

			}

		}
		#endif

	}

}