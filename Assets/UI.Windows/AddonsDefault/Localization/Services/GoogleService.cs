using ME;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Extensions.Tiny;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif

namespace UnityEngine.UI.Windows.Plugins.Localization.Services {

	public class GoogleService : LocalizationService {
		
		public override AuthKeyPermissions GetAuthPermissions() {

			return AuthKeyPermissions.None;

		}

		public override bool IsSupported() {

			return true;

		}

		public override string GetServiceName() {

			return "Google Docs Localization";

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			var settings = this.serviceManager.settings as LocalizationSettings;
			return this.GetData(settings, (result) => {

				if (result.hasError == false) {

					LocalizationSystem.TryToSaveCSV(result.data);

				} else {

					Debug.LogError(string.Format("[ Localization.GoogleService ] CSV GetData error: {0}", result.errorText));
					LocalizationSystem.TryToLoadCache();

				}

			});

		}

		#region Client API Events
		public override System.Collections.Generic.IEnumerator<byte> GetData(LocalizationSettings settings, System.Action<LocalizationResult> onResult) {

			#if !UNITY_EDITOR
			if (this.serviceManager.logEnabled == true) {
			#endif
				
				WindowSystemLogger.Log(this, string.Format("Loading: {0} ({1})", settings.url, settings.eTag));
				
			#if !UNITY_EDITOR
			}
			#endif

			var eTag = settings.eTag;
			var eTagPrefsKey = "LocalizationSystem.GoogleService.ETag";
			if (PlayerPrefs.HasKey(eTagPrefsKey) == true) {

				eTag = PlayerPrefs.GetString(eTagPrefsKey);

			}

			var www = UnityWebRequest.Get(settings.url);
			www.SetRequestHeader("ETag", eTag);
			www.Send();
			#if UNITY_EDITOR
			if (Application.isPlaying == false) {
				
				while (www.isDone == false) {

					if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Wait a while", "...", www.downloadProgress) == true) {

						break;

					}

				}

				UnityEditor.EditorUtility.ClearProgressBar();

				eTag = www.GetResponseHeader("ETag");
				if (eTag != null) {

					settings.eTag = eTag;
					PlayerPrefs.SetString(eTagPrefsKey, eTag);

				}

			} else {
			#endif
				
				while (www.isDone == false) {

					yield return 0;

				}

				eTag = www.GetResponseHeader("ETag");
				if (eTag != null) {

					PlayerPrefs.SetString(eTagPrefsKey, eTag);

				}

			#if UNITY_EDITOR
			}
			#endif

			onResult.Invoke(new LocalizationResult() { hasError = !string.IsNullOrEmpty(www.error), data = www.downloadHandler.text, errorText = www.error });

			www.Dispose();
			www = null;

		}
		#endregion

		#region Editor API Events
		#if UNITY_EDITOR
		public override void EditorLoad(LocalizationSettings settings, LocalizationServiceItem item) {

			if (item.processing == false) {

				item.processing = true;

				// Connecting
				this.OnEditorAuth(item.authKey, (result) => {

					//UnityEditor.EditorApplication.delayCall += () => {

					this.StartCoroutine(this.GetData(settings, (res) => {

						if (res.hasError == false) {

							LocalizationSystem.TryToSaveCSV(res.data);

						}

						item.processing = false;

					}));

					//};

				});

			}

		}

		protected override void OnInspectorGUI(LocalizationSettings settings, LocalizationServiceItem item, System.Action onReset, GUISkin skin) {

			//var data = FlowSystem.GetData();

			if (settings == null) return;

			GUILayout.Label("URL:");
			var newKey = GUILayout.TextArea(settings.url);
			if (newKey != settings.url) {

				settings.url = newKey;
				UnityEditor.EditorUtility.SetDirty(settings);

			}

			GUILayout.Label(string.Format("ETag: {0}", settings.eTag));

			UnityEditor.EditorGUI.BeginDisabledGroup(item.processing);
			if (GUILayout.Button(item.processing == true ? "Loading..." : "Load", skin.button) == true) {
				
				this.EditorLoad(settings, item);

			}
			UnityEditor.EditorGUI.EndDisabledGroup();

		}
		#endif
		#endregion

	}

}