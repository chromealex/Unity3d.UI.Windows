using ME;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Extensions.Tiny;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.GameData.Services {

	public class GoogleService : GameDataService {
		
		public override AuthKeyPermissions GetAuthPermissions() {

			return AuthKeyPermissions.None;

		}

		public override string GetServiceName() {

			return "Google Docs Data";

		}

		public override IEnumerator Auth(string key) {

			var settings = this.serviceManager.settings as GameDataSettings;
			return this.GetData(settings.url, (result) => {

				if (result.hasError == false) {

					GameDataSystem.TryToSaveCSV(result.data);

				} else {

					GameDataSystem.TryToLoadCache();

				}

			});

		}

		#region Client API Events
		public override IEnumerator GetData(string url, System.Action<GameDataResult> onResult) {
			
			if (Application.isPlaying == false || this.serviceManager.logEnabled == true) {
				
				Debug.LogFormat("[ GameData ] Loading: {0}", url);
				
			}

			var www = new WWW(url + "&www_cache=" + Random.Range(0, 100000).ToString());
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

					yield return false;

				}

			#if UNITY_EDITOR
			}
			#endif

			onResult.Invoke(new GameDataResult() { hasError = !string.IsNullOrEmpty(www.error), data = www.text });

		}
		#endregion

		#region Editor API Events
		#if UNITY_EDITOR
		protected override void OnInspectorGUI(GameDataSettings settings, GameDataServiceItem item, System.Action onReset, GUISkin skin) {

			if (settings == null) return;

			var data = FlowSystem.GetData();
			if (data == null) return;

			GUILayout.Label("URL:");
			var newKey = GUILayout.TextArea(settings.url);
			if (newKey != settings.url) {

				settings.url = newKey;
				UnityEditor.EditorUtility.SetDirty(settings);

			}

			UnityEditor.EditorGUI.BeginDisabledGroup(item.processing);
			if (GUILayout.Button(item.processing == true ? "Loading..." : "Load", skin.button) == true) {

				if (item.processing == false) {
					
					item.processing = true;
					
					// Connecting
					this.OnEditorAuth(item.authKey, (result) => {

						//UnityEditor.EditorApplication.delayCall += () => {

							this.StartCoroutine(this.GetData(settings.url, (res) => {
								
								if (res.hasError == false) {

									GameDataSystem.TryToSaveCSV(res.data);

								}

								item.processing = false;

							}));

						//};
						
					});

				}

			}
			UnityEditor.EditorGUI.EndDisabledGroup();

		}
		#endif
		#endregion

	}

}