using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI.Windows.UserInfo;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class AppsflyerService : AnalyticsService {

		public bool isDebug;
		public string key;
		[Tooltip("You should enter the number only and not the \"ID\" prefix")]
		public string iosAppId;
		[Tooltip("Set your Android package name")]
		public string androidAppId;
		public string androidDevKey;
		//public string androidGCMProjectNumber;

		private bool tokenSent;

		public override string GetServiceName() {

			return "AppsflyerAnalytics";

		}

		public override bool IsSupported() {

			#if UNITY_ANDROID || UNITY_IOS
			return true;
			#else
			return false;
			#endif

		}

		public override bool IsConnected() {

			return true;

		}

		public void OnApplicationQuit() {

			#if APPSFLYER_ANALYTICS_API
			#if UNITY_ANDROID
			AndroidJavaObject activity = 
			new AndroidJavaClass("com.unity3d.player.UnityPlayer")
			.GetStatic<AndroidJavaObject>("currentActivity");
			activity.Call<bool>("moveTaskToBack", true);
			#endif
			#endif

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {
			
			#if APPSFLYER_ANALYTICS_API
			AppsFlyer.setIsDebug(this.isDebug);
			AppsFlyer.setAppsFlyerKey(this.key);
			#if UNITY_IOS
			AppsFlyer.setAppID(this.iosAppId);
			AppsFlyer.trackAppLaunch();
			//AppsFlyer.getConversionData();
			//UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
			#elif UNITY_ANDROID
			AppsFlyer.setAppID(this.androidAppId);
			AppsFlyer.init(this.androidDevKey);
			//AppsFlyer.setGCMProjectNumber(this.androidGCMProjectNumber);
			#endif
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(long id) {

			#if APPSFLYER_ANALYTICS_API
			AppsFlyer.setCustomerUserID(id.ToString());
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(string id) {

			#if APPSFLYER_ANALYTICS_API
			AppsFlyer.setCustomerUserID(id.ToString());
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			#if APPSFLYER_ANALYTICS_API
			if (screenId >= 0) {
				
				AppsFlyer.trackRichEvent(string.Format("Screen: {0} (ID: {1}) - {2}", FlowSystem.GetWindow(screenId).title, screenId, group1), new Dictionary<string, string>() {

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

			#if APPSFLYER_ANALYTICS_API
			AppsFlyer.trackRichEvent(string.Format("{0}: {1}", eventName, group1), new Dictionary<string, string>() {

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

			#if APPSFLYER_ANALYTICS_API
			if (screenId >= 0 && toScreenId >= 0) {
					
				AppsFlyer.trackEvent("ScreenTransition", string.Format("{0} (ID: {1}) to {2} (ID: {3})", FlowSystem.GetWindow(screenId).title, screenId, FlowSystem.GetWindow(toScreenId).title, toScreenId));

			}
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			#if APPSFLYER_ANALYTICS_API
			if (screenId >= 0) {
				
				AppsFlyer.setCurrencyCode(currency);
				AppsFlyer.trackRichEvent(AFInAppEvents.PURCHASE, new Dictionary<string, string>() {

					{ AFInAppEvents.CURRENCY, currency },
					{ AFInAppEvents.REVENUE, price.ToString() },
					{ AFInAppEvents.QUANTITY, "1" },
					{ AFInAppEvents.CONTENT_ID, productId },
					{ "Screen", screenId.ToString() },
					{ "CustomParameter", User.instance.customParameter },

				});

			}
			#endif
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature) {

			#if APPSFLYER_ANALYTICS_API
			AppsFlyer.setCurrencyCode(currency);
			AppsFlyer.trackRichEvent(AFInAppEvents.PURCHASE, new Dictionary<string, string>() {

				{ AFInAppEvents.CURRENCY, currency },
				{ AFInAppEvents.REVENUE, price.ToString() },
				{ AFInAppEvents.QUANTITY, "1" },
				{ AFInAppEvents.CONTENT_ID, productId },
				{ "Event", eventName },
				{ "CustomParameter", User.instance.customParameter },

			});
			#endif
			yield return 0;

		}

		private void Update () {
			
			#if UNITY_IOS 
			if (this.tokenSent == false) { 
				
				byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;           
				if (token != null) {
					
					//For iOS uninstall
					AppsFlyer.registerUninstall(token);
					this.tokenSent = true;

				}

			}    
			#endif
		}

		#if UNITY_EDITOR
		private static bool foundType = false;
		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {

			var found = false;
			if (foundType == false) {

				var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				foreach (var ass in assemblies) {

					var type = ass.GetTypes().FirstOrDefault(x => x.Name == "AppsFlyer");
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

				UnityEditor.EditorGUILayout.HelpBox("Appsflyer Analytics is disabled. To enable Appsflyer Analytics download version from official github repo.", UnityEditor.MessageType.Warning);
				if (GUILayout.Button("Open Link", skin.button) == true) {

					Application.OpenURL("https://github.com/AppsFlyerSDK/Unity");

				}

			} else {

				#if APPSFLYER_ANALYTICS_API
				UnityEditor.EditorGUILayout.HelpBox("Appsflyer Analytics is enabled.", UnityEditor.MessageType.Info);
				#else
				UnityEditor.EditorGUILayout.HelpBox("Appsflyer Analytics is disabled. To enable, please add `APPSFLYER_ANALYTICS_API` to your project build settings `Scripting Define Symbols`.", UnityEditor.MessageType.Warning);
				#endif

			}

		}
		#endif

	}

}