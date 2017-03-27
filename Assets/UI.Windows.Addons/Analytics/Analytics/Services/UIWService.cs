using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI.Windows.Extensions.Net;
using UnityEngine.UI.Windows.Extensions.Tiny;
using UnityEngine.UI.Windows.Plugins.Analytics.Net.Api;
using UnityEngine.UI.Windows.Plugins.Flow;
using System;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.UserInfo;

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
		private System.DateTime connectDT = System.DateTime.Today;
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

		public override bool IsSupported() {

			return true;

		}

		public override bool IsConnected() {

			return this.net.Connected();

		}

#if UNITY_EDITOR
        public void UpdateEditor() {

			if (Application.isPlaying == true) return;

			UnityEditor.EditorApplication.delayCall += () => this.UpdateEditor();
			this.net.ReceiveMsg();

		}
#endif


		public void Update() {

            this.net.ReceiveMsg();

        }

		private void Connect(string key, System.Action<bool> onResult = null) {

			if (this.net.Connected() == true) {
                Debug.Log("Stat, Is already connected");
                if (onResult != null) onResult.Invoke(true);

            } else if (connecting == true) {
                Debug.LogError("Stat, Is already connecting");

            } else {
				this.connectDT = System.DateTime.UtcNow;
				this.connecting = true;
				this.net.Connect(host, port, (b) => {
                    var seconds = (System.DateTime.UtcNow - connectDT).TotalSeconds;
                    Debug.Log(string.Format("Stat connect to host: {0} r: {1} s: {2}", host, b, seconds));
                    reconnectDelay = b ? minDelay : System.Math.Min(reconnectDelay * 2, maxDelay);

					if (b == true) {
                        this.authTO = new AuthTO() {key=key};
                        this.net.SendMsg(AuthTO.version, this.SerializeB(authTO));
					}

                    this.connecting = false;
                    if (onResult != null) onResult.Invoke(b);
                });
				this.net.onRecMsg = this.OnRecMsg;
			}

		}

		private void Disconnect(System.Action<bool> onResult = null) {
			//Debug.Log("Closing stat");

			this.authTO = null;
			this.net.Close();
			this.connecting = false;
			if (onResult != null) onResult.Invoke(true);

		}

		public void OnApplicationQuit() {

			this.Disconnect();

		}

        private void _SendMsg<T>(T to) where T : StatTO {
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

            this.net.SendMsg((byte)to.GetTypeTO(), this.SerializeB(to));
        }

        public void SendMsg<T>(T to) where T : StatTO {
            if (this.authTO == null) {
				
				if (this.logTcp == true) Debug.LogError("Sending Msg in Non-Auth Mode: " + to.GetTypeTO());

            } else if (!net.Connected()) {
                if (needReconnect == true) {
                    Reconnect();
                }
                if (connecting)  {
//                    outQueue.Add(to);
                } else {
                    Debug.LogError("SendMsg On Disconnected: " + to.GetTypeTO());
                }

            } else {
                _SendMsg(to);
            }
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

		public override System.Collections.Generic.IEnumerator<byte> Auth(string key, ServiceItem serviceItem) {
			net.SetName("stat");
			this.needReconnect = true;

			var hasError = false;
			this.Connect(key, (resultOk) => hasError = !resultOk);

			while (this.net.Connected() == false && hasError == false) {

				yield return 0;

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

			yield return 0;

		}

		private void Reconnect() {
            var sinceLastConnect = (System.DateTime.UtcNow - this.connectDT).TotalSeconds;
			// if previous successful connection disconnected in 20s
			if (this.connecting == false &&  this.reconnectDelay == this.minDelay && sinceLastConnect < 20) {
				this.reconnectDelay = this.maxDelay;
			}

			// this.authTO != null -> had a successful connection
			if (this.connecting == false && this.authTO != null && sinceLastConnect > this.reconnectDelay) {
				Debug.Log(string.Format("Stat reconnecting to host: {0} ...", host));

				Connect(authTO.key, (success) => {
					if (success) {
						reconnectDelay = minDelay;

						foreach (var identityMsg in this.userMap.Values) {
							this.SendMsg(identityMsg);
						}

					} else {
						reconnectDelay = Math.Min(reconnectDelay * 2, maxDelay);
					}
				});
			} else {
                Debug.Log(string.Format("Stat not-reconnecting to host: {0} connecting: {1} auth: {2} since: {3} delay: {4}", host, this.connecting, this.authTO, sinceLastConnect, this.reconnectDelay));
            }
		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			this.SendMsg(new StatEvent(screenId, group1, group2, group3, weight));
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnEvent(string eventName, string group1, string group2, string group3, int weight) {

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenTransition(int index, int screenId, int toScreenId, bool popup) {

			this.SendMsg(new StatScreenTransition(index, screenId, toScreenId, popup));
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			this.SendMsg(new StatTransaction(screenId, productId, price, currency, receipt, signature));
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature) {

			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {

			this.SendMsg(new StatScreenPoint(screenId, screenWidth, screenHeight, tag, x, y));
			yield return 0;

		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(long id) {
			return SetUserData(new StatSetUserId(id.ToString()));
		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserId(string id) {
			return SetUserData(new StatSetUserDuid(id.ToString()));
		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserGender(Gender gender) {
			return SetUserData(new StatSetUserGender(gender.ToString()));
		}

		public override System.Collections.Generic.IEnumerator<byte> SetUserBirthYear(int birthYear) {
			return SetUserData(new StatUserBirthYear(birthYear));
		}

		private System.Collections.Generic.IEnumerator<byte> SetUserData(StatTO to) {
			if (this.net.Connected()) {
				this.SendMsg(to);
			}
			this.userMap[to.GetTypeTO()] = to;
			yield return 0;
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
						UnityEditor.EditorApplication.delayCall += () => this.UpdateEditor();

					} else {

						// Disconnecting
						this.OnEditorDisconnect((result) => {
							
							item.show = false;
							item.processing = false;
							if (onReset != null) onReset.Invoke();

						});
						UnityEditor.EditorApplication.delayCall += () => this.UpdateEditor();

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