using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Linq;
using System;
using UnityEngine.UI.Windows.UserInfo;

#if FACEBOOK_ANALYTICS_API
using Facebook.Unity;
#endif

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class FacebookService : AnalyticsService {
		
		public override string GetAuthKey(ServiceItem item) {

			return item.authKey;
			
		}

		public override string GetServiceName() {
			
			return "FacebookAnalytics";
			
		}

		public override bool IsSupported() {

			#if FACEBOOK_ANALYTICS_API && !UNITY_EDITOR
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

			#if FACEBOOK_ANALYTICS_API
			return FB.IsInitialized;
			#else
			return true;
			#endif

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			#if FACEBOOK_ANALYTICS_API
			var connected = false;

			if (FB.IsInitialized == true) {
				
				FB.ActivateApp();
				connected = true;

			} else {

				FB.Init(() => {

					FB.ActivateApp();
					connected = true;

				});

			}

			while (connected == false) {

				yield return 0;

			}

			var rootScreenId = FlowSystem.GetRootWindow();
			var eventName = "Application Start";
			FB.LogAppEvent(eventName, 1f, new Dictionary<string, object>() {

				{ "Root Screen", rootScreenId },
				{ "Platform", Application.platform.ToString() }

			});
			#endif

			yield return 0;

		}

		public override void OnApplicationPause(bool paused) {

			if (this.isActive == true) {

				#if FACEBOOK_ANALYTICS_API
				if (paused == false) {
					
					if (FB.IsInitialized == true) {
						
						FB.ActivateApp();

					} else {

						FB.Init(() => {

							FB.ActivateApp();

						});

					}

				}
				#endif

			}

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			#if FACEBOOK_ANALYTICS_API
			if (screenId >= 0) {

				var eventName = string.Format("{0}({1})", FlowSystem.GetWindow(screenId).title, screenId);

				FB.LogAppEvent(eventName, 1f, new Dictionary<string, object>() {

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

			#if FACEBOOK_ANALYTICS_API
			FB.LogAppEvent(eventName, 1f, new Dictionary<string, object>() {

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

			#if FACEBOOK_ANALYTICS_API
			if (screenId >= 0 && toScreenId >= 0) {
				
				var eventName = "Screen Transition";
				FB.LogAppEvent(eventName, 1f, new Dictionary<string, object>() {

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

			#if FACEBOOK_ANALYTICS_API
			FB.LogPurchase((float)price, currency, new Dictionary<string, object>() {

				{ "Window", string.Format("{0} (ID: {1}), product: {2}", FlowSystem.GetWindow(screenId).title, screenId, productId) },
				{ "Receipt", receipt },
				{ "CustomParameter", User.instance.customParameter },

			});
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature) {

			#if FACEBOOK_ANALYTICS_API
			FB.LogPurchase((float)price, currency, new Dictionary<string, object>() {

				{ "Event", string.Format("{0}, product: {2}", eventName, productId) },
				{ "Receipt", receipt },
				{ "Success", 1 },
				{ "CustomParameter", User.instance.customParameter },

			});
			#endif

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {

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

					var type = ass.GetTypes().FirstOrDefault(x => x.Name == "FB" && x.Namespace == "Facebook.Unity");
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

				UnityEditor.EditorGUILayout.HelpBox("To enable Facebook Analytics go to Facebook Developer Portal and import FacebookSDK.", UnityEditor.MessageType.Warning);
				if (GUILayout.Button("Open Link", skin.button) == true) {

					Application.OpenURL("https://developers.facebook.com/docs/unity");

				}

			} else {

				#if FACEBOOK_ANALYTICS_API
				UnityEditor.EditorGUILayout.HelpBox("Facebook Analytics is enabled.", UnityEditor.MessageType.Info);
				#else
				UnityEditor.EditorGUILayout.HelpBox("Facebook Analytics is disabled. To enable, please add `FACEBOOK_ANALYTICS_API` to your project build settings `Scripting Define Symbols`.", UnityEditor.MessageType.Warning);
				#endif

			}

		}
		#endif

	}

}