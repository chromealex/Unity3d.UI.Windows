using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Services {

	public class UnityService : AnalyticsService {
		
		public override string GetAuthKey(ServiceItem item) {

			return item.authKey;
			
		}

		public override string GetServiceName() {
			
			return "UnityAnalytics";
			
		}

		public override IEnumerator Auth(string key) {

			var rootScreenId = FlowSystem.GetRootWindow();
			var eventName = "Application Start";
            UnityEngine.Analytics.Analytics.CustomEvent(eventName, new Dictionary<string, object>() {
				
				{ "Root Screen", rootScreenId }
				
			});

			yield return false;

		}
		
		public override IEnumerator SetUserId(string id) {
			
			UnityEngine.Analytics.Analytics.SetUserId(id.ToString());
			
			yield return false;
			
		}

		public override IEnumerator SetUserId(long id) {

			UnityEngine.Analytics.Analytics.SetUserId(id.ToString());

			yield return false;

		}

		public override IEnumerator SetUserGender(User.Gender gender) {

			UnityEngine.Analytics.Gender output = UnityEngine.Analytics.Gender.Unknown;

			switch (gender) {
				
				case User.Gender.Female:
					output = UnityEngine.Analytics.Gender.Female;
					break;
					
				case User.Gender.Male:
					output = UnityEngine.Analytics.Gender.Male;
					break;
					
				case User.Gender.Any:
					output = UnityEngine.Analytics.Gender.Unknown;
					break;

			}

			UnityEngine.Analytics.Analytics.SetUserGender(output);

			yield return false;

		}

		public override IEnumerator SetUserBirthYear(int birthYear) {
			
			UnityEngine.Analytics.Analytics.SetUserBirthYear(birthYear);

			yield return false;

		}

		public override IEnumerator OnEvent(int screenId, string group1, string group2, string group3, int weight) {

			var eventName = string.Format("{0}({1})", FlowSystem.GetWindow(screenId).title, screenId);

			UnityEngine.Analytics.Analytics.CustomEvent(eventName, new Dictionary<string, object>() {

				{ "Group1", group1 },
				{ "Group2", group2 },
				{ "Group3", group3 },
				{ "Weight", weight }

			});

			yield return false;

		}

		public override IEnumerator OnScreenTransition(int index, int screenId, int toScreenId, bool popup) {

			UnityEngine.Analytics.Analytics.CustomEvent("Screen Transition", new Dictionary<string, object>() {
				
				{ "From", string.Format("{0} (ID: {1})", FlowSystem.GetWindow(screenId).title, screenId) },
				{ "To", string.Format("{0} (ID: {1})", FlowSystem.GetWindow(toScreenId).title, toScreenId) },
				{ "Path", string.Format("Path Index: {0}", index) },
				{ "Popup", string.Format("Popup: {0}", popup) }
				
			});

			yield return false;

		}
		
		public override IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {
			
			UnityEngine.Analytics.Analytics.Transaction(productId, price, currency, receipt, signature);
			yield return false;
			
		}

		public override IEnumerator OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {

			var eventName = string.Format("P{0}", screenId);
			var point = new Vector3(x, y, tag);
			UnityAnalyticsHeatmap.HeatmapEvent.Send(eventName, point);
			yield return false;

		}

		#if UNITY_EDITOR
		public override void GetHeatmapData(int screenId, int screenWidth, int screenHeight, UserFilter filter, System.Action<HeatmapResult> onResult) {

		}

		protected override void OnInspectorGUI(UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {
			
			UnityEditor.EditorGUILayout.HelpBox("To enable Unity Analytics open `Window/Unity Services` window and login there.", UnityEditor.MessageType.Info);

		}
		#endif

	}

}