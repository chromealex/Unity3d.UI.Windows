using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Linq;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace UnityEngine.UI.Windows.Plugins.Ads.Services {

	public class UnityAdsService : AdsService {

		private bool isPlaying = false;

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
			
			return "UnityAds";
			
		}

		public override bool IsSupported() {

			#if UNITY_ADS
			return Advertisement.isSupported;
			#else
			return false;
			#endif

		}

		public override bool IsConnected() {

			#if UNITY_ADS
			return Advertisement.isInitialized;
			#else
			return false;
			#endif

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			#if UNITY_ADS
			if (Advertisement.isSupported == true) {

				Advertisement.Initialize(key, testMode: (serviceItem as AdsServiceItem).testMode);

				while (Advertisement.isInitialized == false) {

					yield return 0;

				}

			}
			#endif

			yield return 0;

		}

		public override bool CanShow() {

			#if UNITY_ADS
			return Advertisement.IsReady();
			#else
			return false;
			#endif

		}

		public override System.Collections.Generic.IEnumerator<byte> Show(string name, Dictionary<object, object> options = null, System.Action onFinish = null, System.Action onFailed = null, System.Action onSkipped = null) {

			#if UNITY_ADS
			if (Advertisement.IsReady() == true && this.isPlaying == false) {

				this.isPlaying = true;
				Advertisement.Show(name, new ShowOptions() {
					resultCallback = (result) => {
						
						switch (result) {

							case ShowResult.Failed:
								if (onFailed != null) onFailed.Invoke();
								break;

							case ShowResult.Finished:
								if (onFinish != null) onFinish.Invoke();
								break;

							case ShowResult.Skipped:
								if (onSkipped != null) onSkipped.Invoke();
								break;

						}

						this.isPlaying = false;

					}
				});

			} else {

				if (onFailed != null) onFailed.Invoke();

			}
			#endif
				
			yield return 0;

		}

		#if UNITY_EDITOR
		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Ads.AdsSettings settings, AdsServiceItem item, System.Action onReset, GUISkin skin) {

			var found = false;
			#if UNITY_ADS
			found = true;
			#endif

			if (found == false) {

				UnityEditor.EditorGUILayout.HelpBox("To enable Unity Ads open `Window/Unity Services` window, login there and turn on `Ads`.", UnityEditor.MessageType.Warning);

			} else {

				UnityEditor.EditorGUILayout.HelpBox("Unity Ads is enabled.", UnityEditor.MessageType.Info);

				item.iosKey = UnityEditor.EditorGUILayout.TextField("GameID (iOS): ", item.iosKey);
				item.androidKey = UnityEditor.EditorGUILayout.TextField("GameID (Android): ", item.androidKey);
				item.testMode = UnityEditor.EditorGUILayout.Toggle("Test mode: ", item.testMode);

			}

		}
		#endif

	}

}