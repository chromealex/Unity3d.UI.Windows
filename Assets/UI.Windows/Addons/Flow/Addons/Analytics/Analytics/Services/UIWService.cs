using ME;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Extensions.Tiny;
using UnityEngine.UI.Windows.Plugins.Analytics.Net;
using UnityEngine.UI.Windows.Plugins.Analytics.Net.Api;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class UIWService : AnalyticsService {

		public override AuthKeyPermissions GetAuthPermissions() {

			return AuthKeyPermissions.Analytics;

		}

		#region Transport
		private NetClient net = new NetClient();
		public bool logTcp = true;
		[System.NonSerialized]
		public string host = "test.unity3dwindows.com";
//		public string host = "localhost";
		[System.NonSerialized]
		public int port = 9997;
		public int timeout = 3000;
		public Dictionary<StatType, StatTO> userMap = new Dictionary<StatType, StatTO>();

		#if UNITY_EDITOR
		public void Update() {

			if (Application.isPlaying == true) return;

			UnityEditor.EditorApplication.delayCall += () => this.Update();
			ResendLost();
			this.net.ReceiveMsg();

		}
		#endif

		private void Connect(string key, System.Action<bool> onResult = null) {

			if (this.net.Connected() == true) {

				if (onResult != null) onResult.Invoke(true);

			} else {

				this.net.Connect(host, port, (b) => {
					if (b == false) {
						onResult(false);
					} else {
						AuthTO authTO = new AuthTO() {key=key};
						this.net.SendMsg(AuthTO.version, this.SerializeB(authTO));
						onResult(true);
					}
				});
				this.net.onRecMsg = this.OnRecMsg;

			}

		}

		private void Disconnect(System.Action<bool> onResult = null) {

			this.net.Close();
			if (onResult != null) onResult.Invoke(true);

		}
		
		private void SendMsg<T>(T to) where T : StatTO {

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

			this.Connect(key);

			while (this.net.Connected() == false) {

				yield return false;

			}

			Debug.Log("Stat connected: " + host);

			foreach(StatTO to in userMap.Values) {
				SendMsg(to);
			}
			userMap.Clear();

			yield return false;

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
			} else {
				this.userMap[to.GetTypeTO()] = to;
			}
			yield return false;
		}
		#endregion

		#region Editor API Events
		#if UNITY_EDITOR
		private int idx = 0;
		private Dictionary<int, System.Action<Result>> responseMap = new Dictionary<int, System.Action<Result>>();
		private List<KeyValuePair<long, StatReqTO>> requestList = new List<KeyValuePair<long, StatReqTO>>();

		public override void OnEditorAuth(string key, System.Action<bool> onResult) {

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
			this.requestList.Add(new KeyValuePair<long, StatReqTO>(System.DateTime.UtcNow.Ticks, to));
			this.SendEditorMsg(to);

		}

		private void SendEditorMsg<T>(T to) where T : StatReqTO {

			this.SendMsg(to);

		}

		private void ResendLost() {

			if (requestList.Count == 0) return;

			if (responseMap.Count == 0) {
				this.requestList.Clear();
				return;
			}

			if (this.net == null || this.net.Connected() == false) {
				this.requestList.Clear();
				return;
			}

			List<StatReqTO> toResend = new List<StatReqTO>();
			long threshold = System.DateTime.UtcNow.Ticks - timeout * 100000L;

			int i = 0;
			for (; i < this.requestList.Count && this.requestList[i].Key < threshold; i++) {

				var to = this.requestList[i].Value;

				if (this.responseMap.ContainsKey(to.idx) == true) {

					toResend.Add(to);

				}

			}

			this.requestList.RemoveRange(0, i);

			if (toResend.Count == 0) return;

			Debug.LogWarning("ResendLost packets " + toResend.Count);
			foreach(var to in toResend) {

				this.requestList.Add(new KeyValuePair<long, StatReqTO>(System.DateTime.UtcNow.Ticks, to));
				SendEditorMsg(to);

			}

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