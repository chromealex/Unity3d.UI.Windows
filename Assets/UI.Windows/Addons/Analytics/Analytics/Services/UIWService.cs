//#define TEMPJSON
#if TEMPJSON
using FullSerializer;
#endif
using ME;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI.Windows.Plugins.Analytics.Net;
using UnityEngine.UI.Windows.Plugins.Analytics.Net.Api;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class UIWService : AnalyticsService {

		#region Transport
		private NetClient net = new NetClient();
		public bool logTcp = true;
		public string host = "test.unity3dflow.com";
//		public string host = "localhost";
		public int port = 9997;

		public override void Update() {
			
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall += () => this.Update();
			#endif

			this.net.ReceiveMsg();

		}

		private void Connect(string key, System.Action<bool> onResult = null) {

			if (this.net.Connected() == true) {

				if (onResult != null) onResult.Invoke(true);

			} else {
				
				this.net.Connect(host, port, onResult);
				this.net.onRecMsg = this.OnRecMsg;

			}

		}

		private void Disconnect(System.Action<bool> onResult = null) {

			//this.net.Close();
			if (onResult != null) onResult.Invoke(true);

		}
		
		private void SendMsg<T>(T to) where T : StatTO {
			
			#if UNITY_EDITOR
			if (this.logTcp == true) {

				if (to is StatReqTO) {
					Debug.Log("Stat> " + to.GetTypeTO() + ":" + (to as StatReqTO).idx);
				} else {
					Debug.Log("Stat> " + to.GetTypeTO());
				}

			}
			#endif

			this.net.SendMsg((byte)to.GetTypeTO(), this.Serialize(to));
			
		}
		
		#if UNITY_EDITOR
		private void OnResponse(StatResTO resTO) {

			if (this.responseMap.ContainsKey(resTO.idx) == true) {

				var onResult = this.responseMap[resTO.idx];
				this.responseMap.Remove(resTO.idx);

				onResult(resTO.result);

			} else {

				Debug.LogWarning("Callback " + resTO.idx + " not found");

			}
			
		}
		#endif

		#if TEMPJSON
		public static class JSON {
			
			public static string Serialize<T>(T source) {
				
				fsData data;
				new fsSerializer().TrySerialize(source, out data);
				
				return fsJsonPrinter.CompressedJson(data);
				
			}
			
			public static T Deserialize<T>(string data) {
				
				T result = default(T);
				fsData dataResponse = fsJsonParser.Parse(data);
				new fsSerializer().TryDeserialize<T>(dataResponse, ref result);
				
				return result;
				
			}
			
		}
		#endif
		
		private T Deserialize<T>(byte[] data) {

			#if TEMPJSON
			return JSON.Deserialize<T>(Encoding.UTF8.GetString(data, 0, data.Length));
			#endif
			return default(T);

		}
		
		private byte[] Serialize<T>(T data) {

			#if TEMPJSON
			return Encoding.UTF8.GetBytes(JSON.Serialize<T>(data));
			#endif
			return null;

		}
		
		private void OnRecMsg(short typeShort, byte[] data) {

			#if UNITY_EDITOR
			var typeByte = (byte)typeShort;
			if (System.Enum.IsDefined(typeof(StatType), typeByte) == false) {

				Debug.LogWarning("Unknown request: " + typeShort);
				return;

			}
			
			StatType type = (StatType)typeByte;
			StatResTO to = null;

			if (StatType.OnStatResScreen == type) {

				to = this.Deserialize<StatResScreen>(data);

			} else if (StatType.OnStatResScreenTransition == type) {

				to = this.Deserialize<StatResScreenTransition>(data);

			} else {

				Debug.LogWarning("Unknown request: " + type);

			}

			if (this.logTcp == true && to != null) {

				Debug.Log("Stat." + to.GetTypeTO() + ":" + to.idx + " " + to.result.uniqueCount + ", " + to.result.count);

			}

			this.OnResponse(to);
			#else
			Debug.LogWarning("Unknown request: " + typeShort);
			#endif

		}
		#endregion

		#region Client API Events
		public override string GetPlatformName() {
			
			return "UIWAnalytics";
			
		}

		public override IEnumerator Auth(string key) {

			this.Connect(key);

			while (this.net.Connected() == false) {

				yield return false;

			}

			Debug.Log("Stat connected");

			yield return false;

		}

		public override IEnumerator OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			this.SendMsg(new StatEvent(screenId, group1, group2, group3, weight));
			yield return false;

		}

		public override IEnumerator OnScreenTransition(int index, int screenId, int toScreenId) {

			this.SendMsg(new StatScreenTransition(index, screenId, toScreenId));
			yield return false;

		}

		public override IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			this.SendMsg(new StatTransition(screenId, productId, price, currency, receipt, signature));
			yield return false;

		}

		public override IEnumerator OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {

			this.SendMsg(new StatScreenPoint(screenId, screenWidth, screenHeight, tag, x, y));
			yield return false;

		}
		#endregion

		#region Editor API Events
		#if UNITY_EDITOR
		private long idx = 0;
		private Dictionary<long, System.Action<Result>> responseMap = new Dictionary<long, System.Action<Result>>();

		public override void OnEditorAuth(string key, System.Action<bool> onResult) {

			this.Connect(key, onResult);

		}

		public override void OnEditorDisconnect(System.Action<bool> onResult) {

			this.Disconnect(onResult);

		}

        public override void GetScreen(int screenId, System.Action<Result> onResult) {

			this.SendEditorMsg(new StatGetScreen(screenId), onResult);

		}

		public override void GetScreenTransition(int index, int screenId, int toScreenId, System.Action<Result> onResult) {

			this.SendEditorMsg(new StatGetScreenTransition(index, screenId, toScreenId), onResult);

		}

		private void SendEditorMsg(StatReqTO to, System.Action<Result> onResult) {

			to.idx = this.idx++;
			this.responseMap.Add(to.idx, onResult);
			this.SendEditorMsg(to);

		}

		private void SendEditorMsg<T>(T to) where T : StatTO {

			if (this.net.Connected() == true) {

				this.SendMsg(to);

			}

		}

		public override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsItem item, System.Action onReset, GUISkin skin) {

			UnityEditor.EditorGUILayout.LabelField("Key:");
			item.authKey = UnityEditor.EditorGUILayout.TextArea(item.authKey, skin.textArea);

			CustomGUI.Splitter();

			GUI.changed = item.userFilter.OnGUI(skin) || GUI.changed;

			CustomGUI.Splitter();

			if (GUI.changed == true) {

				item.isChanged = true;

			}

			UnityEditor.EditorGUI.BeginDisabledGroup(!item.isChanged);
			if (GUILayout.Button("Apply", skin.button) == true) {

				item.isChanged = false;
				GUI.changed = true;

				if (onReset != null) onReset.Invoke();

			}
			UnityEditor.EditorGUI.EndDisabledGroup();

			UnityEditor.EditorGUI.BeginDisabledGroup(item.processing);
			{

				var newState = UnityEditor.EditorGUILayout.ToggleLeft("Show " + (item.processing == true ? "(Processing...)" : string.Empty), item.show);
				if (newState != item.show && item.processing == false) {

					item.processing = true;

					if (newState == true) {

						// Connecting
						this.OnEditorAuth(item.authKey, (result) => {

							item.show = result;
							item.processing = false;

						});
						UnityEditor.EditorApplication.delayCall += () => this.Update();

					} else {

						// Disconnecting
						this.OnEditorDisconnect((result) => {
							
							item.show = false;
							item.processing = false;
							if (onReset != null) onReset.Invoke();

						});
						UnityEditor.EditorApplication.delayCall += () => this.Update();

					}

				}

			}
			UnityEditor.EditorGUI.EndDisabledGroup();

			if (GUI.changed == true) {

				UnityEditor.EditorUtility.SetDirty(settings);

			}

		}
		#endif
		#endregion

	}

}