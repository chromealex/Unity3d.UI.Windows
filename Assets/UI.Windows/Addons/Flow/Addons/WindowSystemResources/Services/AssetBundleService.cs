using ME;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Extensions.Tiny;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Resources.Services {

	public class AssetBundleService : WindowSystemResourcesService {
		
		public override AuthKeyPermissions GetAuthPermissions() {

			return AuthKeyPermissions.None;

		}

		public override bool IsSupported() {

			return true;

		}

		public override string GetServiceName() {

			return "AssetBundle Resources";

		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			var settings = this.serviceManager.settings as WindowSystemResourcesSettings;
			return this.GetData(settings.url, (result) => {

				if (result.hasError == false) {



				} else {



				}

			});

		}

		#region Client API Events
		public override System.Collections.Generic.IEnumerator<byte> GetData(string url, System.Action<WindowSystemResourcesResult> onResult) {

			#if !UNITY_EDITOR
			if (this.serviceManager.logEnabled == true) {
			#endif
				
				WindowSystemLogger.Log(this, string.Format("Loading: {0}", url));
				
			#if !UNITY_EDITOR
			}
			#endif

			var www = new WWW(url);
			#if UNITY_EDITOR
			if (Application.isPlaying == false) {

				while (www.isDone == false) {

					if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Wait a while", "...", www.progress) == true) {

						break;

					}

				}

				UnityEditor.EditorUtility.ClearProgressBar();

			} else {
			#endif
				
				while (www.isDone == false) {

					yield return 0;

				}

			#if UNITY_EDITOR
			}
			#endif

			onResult.Invoke(new WindowSystemResourcesResult() { hasError = !string.IsNullOrEmpty(www.error), data = www.text });

			www.Dispose();
			www = null;

		}
		#endregion

		#region Editor API Events
		#if UNITY_EDITOR
		public override void EditorLoad(WindowSystemResourcesSettings settings, WindowSystemResourcesServiceItem item) {
			
		}

		protected override void OnInspectorGUI(WindowSystemResourcesSettings settings, WindowSystemResourcesServiceItem item, System.Action onReset, GUISkin skin) {

			//var data = FlowSystem.GetData();

			if (settings == null) return;

			var newValue = GUILayout.Toggle(settings.loadFromStreamingAssets, "Use Streaming Assets");
			if (settings.loadFromStreamingAssets == true) {

				UnityEditor.EditorGUILayout.HelpBox("StreamingAssets directory will be used to load AssetBundles", UnityEditor.MessageType.Info);

			}

			if (newValue != settings.loadFromStreamingAssets) {

				settings.loadFromStreamingAssets = newValue;
				UnityEditor.EditorUtility.SetDirty(settings);

			}

			UnityEditor.EditorGUI.BeginDisabledGroup(settings.loadFromStreamingAssets);
			{

				if (settings.url == null) settings.url = string.Empty;

				GUILayout.Label("URL:");	
				var newKey = GUILayout.TextArea(settings.url);
				if (newKey != settings.url) {

					settings.url = newKey;
					UnityEditor.EditorUtility.SetDirty(settings);

				}

			}
			UnityEditor.EditorGUI.EndDisabledGroup();

		}
		#endif
		#endregion

	}

}