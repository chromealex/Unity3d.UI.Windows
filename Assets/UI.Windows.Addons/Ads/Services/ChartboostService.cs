using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Linq;
#if CHARTBOOST_API
using ChartboostSDK;
#endif

namespace UnityEngine.UI.Windows.Plugins.Ads.Services {

	public class ChartboostService : AdsService {

		public string iosId;
		public string iosSignature;
		public string androidId;
		public string androidSignature;

		//private bool isPlaying = false;

		public override string GetAuthKey(ServiceItem item) {

			#pragma warning disable
			var itemAds = (item as AdsServiceItem);
			#pragma warning restore

			#if UNITY_IOS
			return itemAds.iosKey;
			#elif UNITY_ANDROID
			return itemAds.androidKey;
			#else
			return item.authKey;
			#endif

		}

		public override string GetServiceName() {
			
			return "Chartboost";
			
		}

		public override bool IsSupported() {

			#if CHARTBOOST_API
			return true;
			#else
			return false;
			#endif

		}

		public override bool IsConnected() {

			#if CHARTBOOST_API
			return Chartboost.isInitialized();
			#else
			return false;
			#endif

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			#pragma warning disable
			var itemAds = (serviceItem as AdsServiceItem);
			#pragma warning restore

			#if CHARTBOOST_API
			var ageGate = false;
			var autocache = true;
			#if UNITY_IPHONE
			Chartboost.setShouldPauseClickForConfirmation(ageGate);
			#endif
			Chartboost.setAutoCacheAds(autocache);
			Chartboost.setMediation(CBMediation.AdMob, "1.0");

			#if UNITY_IPHONE
			Chartboost.CreateWithAppId(itemAds.iosKey, this.iosSignature);
			#elif UNITY_ANDROID
			Chartboost.CreateWithAppId(itemAds.androidKey, this.androidSignature);
			#endif

			#endif

			yield return 0;

		}

		public override bool CanShow() {

			return false;

		}

		#if UNITY_EDITOR
		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Ads.AdsSettings settings, AdsServiceItem item, System.Action onReset, GUISkin skin) {

			var found = false;
			#if CHARTBOOST_API
			found = true;
			#endif

			if (found == false) {

				UnityEditor.EditorGUILayout.HelpBox("To enable Chartboost download sdk from `http://answers.chartboost.com/en-us/articles/download` and add `CHARTBOOST_API` define into ProjectSettings.", UnityEditor.MessageType.Warning);

			} else {

				UnityEditor.EditorGUILayout.HelpBox("Chartboost is enabled.", UnityEditor.MessageType.Info);

				item.iosKey = UnityEditor.EditorGUILayout.TextField("Id (iOS): ", this.iosId);
				item.iosKey = UnityEditor.EditorGUILayout.TextField("Signature (iOS): ", this.iosSignature);
				item.androidKey = UnityEditor.EditorGUILayout.TextField("Id (Android): ", this.androidId);
				item.androidKey = UnityEditor.EditorGUILayout.TextField("Signature (Android): ", this.androidSignature);
				item.testMode = UnityEditor.EditorGUILayout.Toggle("Test mode: ", item.testMode);

			}

		}
		#endif

	}

}