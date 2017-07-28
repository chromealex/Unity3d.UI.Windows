using ME;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Extensions.Tiny;
using UnityEngine.UI.Windows.Extensions.Net;
using UnityEngine.UI.Windows.Plugins.ABTesting.Net.Api;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.ABTesting.Services {

	public class UIWService : ABTestingService {

		private readonly string H_TOKEN = "Token";

		public bool logNetwork = true;
		private HttpClient net = new HttpClient() {
			logPrefix = "AbTest "
		};
		[System.NonSerialized]
		#if PRODUCTION
		public string host = "services.unity3dwindows.com";
		#else
		public string host = "test.unity3dwindows.com";
		#endif
//		public string host = "localhost:9980";

		private string key;


		public override void Awake() {

			base.Awake();

			net.log = logNetwork;
			net.keepAlive = true;

		}

		public override bool IsSupported() {

			return true;

		}

		public override AuthKeyPermissions GetAuthPermissions() {

			return AuthKeyPermissions.ABTesting;

		}

		#region Client API Events
		public override string GetServiceName() {
			
			return "UIW ABTesting";
			
		}

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {

			var request = "{\"key\": \"" + key + "\"}";
			Debug.Log(this.GetServiceName() + " :: " + host);
			return net.JsonPost(host, "/v/auth", request, response => {
				var to = DeserializeT<ResponseData<string>>(response);

				if (!to.WithError()) {
					net.extraHeaders[H_TOKEN] = to.GetData();
					Debug.Log("AbTest connected: " + host);

				} else {
					Debug.LogError(GetServiceName() + " Auth Error: " + to.error.text);
					net.extraHeaders.Remove(H_TOKEN);
				}

			}, errorText => { // Transport errors

				net.extraHeaders.Remove(H_TOKEN);
			});

		}

		#if UNITY_EDITOR
		public override void OnEditorAuth(string key, System.Action<bool> onResult) {

			Debug.Log("AbTest Editor connecting...");

			var request = "{\"key\": \"" + key + "\"}";

			this.StartCoroutine(
			net.JsonPost(host, "/v/authEditor", request, response => {
				var to = DeserializeT<ResponseData<string>>(response);

				if (!to.WithError()) {
					net.extraHeaders[H_TOKEN] = to.GetData();
					Debug.Log("AbTest Editor connected: " + host);
					onResult(true);

				} else {

					Debug.LogError(GetServiceName() + " Auth Error: " + to.error.text);
					net.extraHeaders.Remove(H_TOKEN);
					onResult(false);
				}

			}, errorText => { // Transport errors

				net.extraHeaders.Remove(H_TOKEN);
				onResult(false);
			}));

		}
		#endif

		public override System.Collections.Generic.IEnumerator<byte> GetData(int testId, System.Action<ABTestResult> onResult) {

			var request = "{\"id\": " + testId + "}";

			return net.JsonPost(host, "/v/abtest/get", request, response => {
				ABTestResultJson resultJson = DeserializeT<ABTestResultJson>(response);
				ABTestResult result = new ABTestResult();
				result.hasError = resultJson.hasError;
				if (resultJson.data != null) {
					result.data = DeserializeT<ABTestingItemsTO>(resultJson.data);
				}
				onResult(result);

			}, errorText => { // Transport errors
				onResult(new ABTestResult());
			});

		}

		public override System.Collections.Generic.IEnumerator<byte> GetDataAll(System.Action<ABTestsResult> onResult) {

			var request = "{}";

			return net.JsonPost(host, "/v/abtest/getAll", request, response => {
				ABTestsResultJson resultJson = DeserializeT<ABTestsResultJson>(response);
				ABTestsResult result = new ABTestsResult();
				result.hasError = resultJson.hasError;
				if (resultJson.data != null) {
					result.data = new Dictionary<int, ABTestingItemsTO>();
					foreach (var e in resultJson.data) {
						result.data.Add(e.testId, DeserializeT<ABTestingItemsTO>(e.jsonData));
					}
				}

				onResult(result);

			}, errorText => { // Transport errors
				onResult(new ABTestsResult());
			});

		}
		#endregion

		protected T DeserializeB<T>(byte[] data) {
			return Json.Decode<T>(Encoding.UTF8.GetString(data, 0, data.Length));
		}

		protected byte[] SerializeB<T>(T data) {
			return Encoding.UTF8.GetBytes(Json.Encode(data));
		}

		protected T DeserializeT<T>(string data) {
			return Json.Decode<T>(data);
		}

		protected string SerializeT<T>(T data) {
			return Json.Encode(data);
		}

		#region Editor API Events
		#if UNITY_EDITOR
        public override void Save(int testId, ABTestingItemsTO to, System.Action<bool> onResult) {

			var map = new Dictionary<int, ABTestingItemsTO>();
			map.Add(testId, to);
			this.SaveAll(map, onResult);

		}

		public override void SaveAll(Dictionary<int, ABTestingItemsTO> testMap, System.Action<bool> onResult) {
			List<ABTestingJsonItemTO> testList = new List<ABTestingJsonItemTO>();

			foreach(KeyValuePair<int, ABTestingItemsTO> e in testMap) {
				testList.Add(new ABTestingJsonItemTO(e.Key, SerializeT(e.Value)));
			}

			var data = SerializeT(testList);

			StartCoroutine(net.JsonPost(host, "/v/abtest/saveAll", data, response => {

				onResult(true);

			}, errorText => { // Transport errors

				onResult(false);

			}));
		}

		protected override void OnInspectorGUI(ABTestingSettings settings, ABTestingServiceItem item, System.Action onReset, GUISkin skin) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			UnityEditor.EditorGUI.BeginDisabledGroup(item.processing);
			if (GUILayout.Button(item.processing == true ? "Saving..." : "Save All", skin.button) == true) {

				if (item.processing == false) {
					
					item.processing = true;
					
					// Connecting
					this.OnEditorAuth(item.authKey, (result) => {

						UnityEditor.EditorApplication.delayCall += () => {

							Dictionary<int, ABTestingItemsTO> testMap = new Dictionary<int, ABTestingItemsTO>();

							foreach (var testWindow in data.GetWindows()) {

								if (testWindow.IsABTest() == true) {

									testMap.Add(testWindow.id, testWindow.abTests.GetTO());
//									this.Save(testWindow.id, testWindow.abTests.GetTO(), (saveResult) => {});

								}

							}

							if (testMap.Count != 0) {

								this.SaveAll(testMap, (saveResult) => {});

							}

							item.processing = false;

						};
						
					});

				}

			}
			UnityEditor.EditorGUI.EndDisabledGroup();

		}
		#endif
		#endregion

	}

}