using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI.Windows.Extensions.Net;
using UnityEngine.UI.Windows.Extensions.Tiny;
using UnityEngine.UI.Windows.Plugins.Analytics.Net.Api;
using UnityEngine.UI.Windows.Plugins.Flow;
using System;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class UIWService : AnalyticsService {

		public override AuthKeyPermissions GetAuthPermissions() {

			return AuthKeyPermissions.Analytics;

		}

		#region Transport
		private NetClient net = new NetClient();
		public bool logTcp = true;
		[System.NonSerialized]
		#if PRODUCTION
		public string host = "services.unity3dwindows.com";
		#else
		public string host = "test.unity3dwindows.com";
		#endif
//		public string host = "localhost";
		[System.NonSerialized]
		public int port = 9997;
		public int timeout = 3000;
		public Dictionary<StatType, StatTO> userMap = new Dictionary<StatType, StatTO>();

		[System.NonSerialized]
		private AuthTO authTO = null;
		[System.NonSerialized]
		private System.DateTime connectDT;
		[System.NonSerialized]
		private bool connecting = false;
		[System.NonSerialized]
		private bool needReconnect = false;
		[System.NonSerialized]
		private int minDelay = 5;
		[System.NonSerialized]
		private int maxDelay = 5 * 128;
		[System.NonSerialized]
		private int reconnectDelay = 5;


		#if UNITY_EDITOR
		public void Update() {

			if (Application.isPlaying == true) return;

			UnityEditor.EditorApplication.delayCall += () => this.Update();
			this.net.ReceiveMsg();

		}
		#endif

		private void Connect(string key, System.Action<bool> onResult = null) {

			if (this.net.Connected() == true) {

				if (onResult != null) onResult.Invoke(true);

			} else {

				this.connectDT = System.DateTime.UtcNow;
				this.connecting = true;
				this.net.Connect(host, port, (b) => {
					this.connecting = false;

					if (b == false) {
						if (onResult != null) onResult.Invoke(false);

					} else {
						this.authTO = new AuthTO() {key=key};
						this.net.SendMsgSilently(AuthTO.version, this.SerializeB(authTO));
						if (onResult != null) onResult.Invoke(true);
					}
				});
				this.net.onRecMsg = this.OnRecMsg;
			}

		}

		private void Disconnect(System.Action<bool> onResult = null) {
			Debug.Log("Closing stat");

			this.authTO = null;
			this.net.Close();
			this.connecting = false;
			if (onResult != null) onResult.Invoke(true);

		}

		public void OnApplicationQuit() {

			this.Disconnect();

		}

		private void SendMsg<T>(T to) where T : StatTO {
			if (!net.Connected() && this.needReconnect == true) {
				Reconnect();
			}

			if (this.logTcp == true) {

				#if UNITY_EDITOR
				if (to is StatReqTO) {
					Debug.Log("Stat> " + to.GetTypeTO() + ":" + (to as StatReqTO).idx);
				} else {
					Debug.Log("Stat> " + to.GetTypeTO());
				}
				#else
                Debug.Log("Stat> " + to.GetTypeTO());
				#endif

			}

			this.net.SendMsgSilently((byte)to.GetTypeTO(), this.SerializeB(to));

/*
			#if UNITY_EDITOR
			if (this.logTcp == true) {
				if (to is StatReqTO) {
					Debug.Log("Stat_ " + to.GetTypeTO() + ":" + (to as StatReqTO).idx);
				}
			}
			#endif
*/

		}
		
		#if UNITY_EDITOR
		private void OnResponse(StatResTOB resTO) {

			if (this.responseMap.ContainsKey(resTO.idx) == true) {

				var onResult = this.responseMap[resTO.idx];
				this.responseMap.Remove(resTO.idx);

				onResult(resTO.GetResult());

			} else {

				Debug.LogWarning("Callback " + resTO.idx + " not found");

			}
			
		}
		#endif

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

		private void OnRecMsg(short typeShort, byte[] data) {

			#if UNITY_EDITOR
			var typeByte = (byte)typeShort;
			if (System.Enum.IsDefined(typeof(StatType), typeByte) == false) {

				Debug.LogWarning("Unknown request: " + typeShort);
				return;

			}
			
			StatType type = (StatType)typeByte;
			StatResTOB to = null;

			if (StatType.OnStatResScreen == type) {

				to = this.DeserializeB<StatResScreen>(data);

			} else if (StatType.OnStatResScreenTransition == type) {

				to = this.DeserializeB<StatResScreenTransition>(data);

			} else if (StatType.OnStatResHeatmapData == type) {

				to = this.DeserializeB<StatResHeatmapData>(data);

			} else {

				Debug.LogWarning("Unknown request: " + type);

			}

			if (this.logTcp == true && to != null) {

				Debug.Log("Stat." + to.GetTypeTO() + ":" + to.idx + " " + to.GetResult().ToString());

			}

			this.OnResponse(to);
			#else
			Debug.LogWarning("Unknown request: " + typeShort);
			#endif

		}
		#endregion

		#region Client API Events
		public override string GetServiceName() {
			
			return "UIWAnalytics";
			
		}

		public override IEnumerator Auth(string key) {
			net.SetName("stat");
			this.needReconnect = true;

			var hasError = false;
			this.Connect(key, (resultOk) => hasError = !resultOk);

			while (this.net.Connected() == false && hasError == false) {

				yield return false;

			}

			var seconds = (System.DateTime.UtcNow - connectDT).TotalSeconds;

			if (hasError == false) {
				WindowSystemLogger.Warning(this, string.Format("Stat connected: {0} in {1} s", host, seconds));

				foreach (var identityMsg in this.userMap.Values) {
					this.SendMsg(identityMsg);
				}

			} else {
				WindowSystemLogger.Warning(this, string.Format("Stat connecting error to host: {0} in {1} s", host, seconds));
			}

			yield return false;

		}

		private void Reconnect() {
			// if previous successful connection disconnected in 20s
			if (this.connecting == false &&  this.reconnectDelay == this.minDelay && (System.DateTime.UtcNow - this.connectDT).TotalSeconds < 20) {
				this.reconnectDelay = this.maxDelay;
			}

			// this.authTO != null -> had a successful connection
			if (this.connecting == false && this.authTO != null && (System.DateTime.UtcNow - this.connectDT).TotalSeconds > this.reconnectDelay) {
				Debug.Log(string.Format("Stat reconnecting to host: {0} ...", host));

				Connect(authTO.key, (success) => {
					var seconds = (System.DateTime.UtcNow - connectDT).TotalSeconds;

					Debug.Log(string.Format("Stat reconnect to host: {0} r: {1} s: {2}", host, success, seconds));
					if (success) {
						reconnectDelay = minDelay;

						foreach (var identityMsg in this.userMap.Values) {
							this.SendMsg(identityMsg);
						}

					} else {
						reconnectDelay = Math.Min(reconnectDelay * 2, maxDelay);
					}
				});
			}
		}

		public override IEnumerator OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			this.SendMsg(new StatEvent(screenId, group1, group2, group3, weight));
			yield return false;

		}

		public override IEnumerator OnScreenTransition(int index, int screenId, int toScreenId, bool popup) {

			this.SendMsg(new StatScreenTransition(index, screenId, toScreenId, popup));
			yield return false;

		}

		public override IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			this.SendMsg(new StatTransaction(screenId, productId, price, currency, receipt, signature));
			yield return false;

		}

		public override IEnumerator OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {

			this.SendMsg(new StatScreenPoint(screenId, screenWidth, screenHeight, tag, x, y));
			yield return false;

		}

		public override IEnumerator SetUserId(long id) {
			return SetUserData(new StatSetUserId(id.ToString()));
		}

		public override IEnumerator SetUserId(string id) {
			return SetUserData(new StatSetUserId(id.ToString()));
		}

		public override IEnumerator SetUserGender(User.Gender gender) {
			return SetUserData(new StatSetUserGender(gender.ToString()));
		}

		public override IEnumerator SetUserBirthYear(int birthYear) {
			return SetUserData(new StatUserBirthYear(birthYear));
		}

		private IEnumerator SetUserData(StatTO to) {
			if (this.net.Connected()) {
				this.SendMsg(to);
			}
			this.userMap[to.GetTypeTO()] = to;
			yield return false;
		}
		#endregion

		#region Editor API Events
		#if UNITY_EDITOR
		private int idx = 0;
		private Dictionary<int, System.Action<Result>> responseMap = new Dictionary<int, System.Action<Result>>();

		public override void OnEditorAuth(string key, System.Action<bool> onResult) {
			net.SetName("statEditor");
			this.needReconnect = true;
			this.Connect(key, (b) => {
				Debug.Log("Stat Editor connected: " + host);
				onResult(b);
			});

		}

		public override void OnEditorDisconnect(System.Action<bool> onResult) {

			this.Disconnect(onResult);

		}

		public override void GetScreen(int screenId, UserFilter filter, System.Action<ScreenResult> onResult) {

			this.SendEditorMsg(new StatGetScreen(screenId), (r) => onResult(r as ScreenResult));

		}

		public override void GetScreenTransition(int index, int screenId, int toScreenId, UserFilter filter, System.Action<ScreenResult> onResult) {

			this.SendEditorMsg(new StatGetScreenTransition(index, screenId, toScreenId), (r) => onResult(r as ScreenResult));

		}

		public override void GetHeatmapData(int screenId, int screenWidth, int screenHeight, UserFilter filter, System.Action<HeatmapResult> onResult) {

			this.SendEditorMsg(new StatGetHeatmapData(screenId, screenWidth, screenHeight, filter), (r) => onResult(r as HeatmapResult));

		}

		private void SendEditorMsg<T>(T to, System.Action<Result> onResult) where T : StatReqTO {

			to.idx = this.idx++;
			this.responseMap.Add(to.idx, onResult);
			this.SendEditorMsg(to);

		}

		private void SendEditorMsg<T>(T to) where T : StatReqTO {

			this.SendMsg(to);

		}

		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {

			var wasChanged = GUI.changed;
			GUI.changed = false;

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

				var newState = UnityEditor.EditorGUILayout.ToggleLeft(string.Format("Show {0}", (item.processing == true ? "(Processing...)" : string.Empty)), item.show);
				if (newState != item.show && item.processing == false) {

					item.processing = true;

					if (newState == true) {

						if (onReset != null) onReset.Invoke();

						// Connecting
						this.OnEditorAuth(this.GetAuthKey(item), (result) => {

							UnityEditor.EditorApplication.delayCall += () => {
								
								Debug.Log("Stat Editor Connected!");
								item.show = result;
								item.processing = false;

							};

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

			GUI.changed = wasChanged;

		}
		#endif
		#endregion

	}

}