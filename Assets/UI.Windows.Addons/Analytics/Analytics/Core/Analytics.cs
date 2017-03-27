using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.UserInfo;

namespace UnityEngine.UI.Windows.Plugins.Analytics {

	public class Analytics : ServiceManager<Analytics> {
		
		public override string GetServiceName() {

			return "Analytics";

		}

		public static bool IsConnected() {

			var services = Analytics.GetServices<IAnalyticsService>();
			var result = true;
			for (int i = 0; i < services.Count; ++i) {

				if (services[i].IsConnected() == false) {
					
					result = false;
					break;

				}

			}

			return result;

		}

		public override AuthKeyPermissions GetAuthPermission() {

			return AuthKeyPermissions.Analytics;

		}

		public override void OnInitialized() {
			
			WindowSystem.onTransition.AddListener(Analytics.OnScreenTransition);

			User.instance.onSetUserId += Analytics.SetUserId;
			User.instance.onSetUserId2 += Analytics.SetUserId;
			User.instance.onSetBirthYear += Analytics.SetUserBirthYear;
			User.instance.onSetGender += Analytics.SetUserGender;

		}

		public static void SetUserId(string id) {

			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.SetUserId(id)));

		}

		public static void SetUserId(long id) {
			
			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.SetUserId(id)));

		}
		
		public static void SetUserGender(Gender gender) {
			
			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.SetUserGender(gender)));

		}
		
		public static void SetUserBirthYear(int birthYear) {
			
			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.SetUserBirthYear(birthYear)));

		}

		public static void OnScreenTransition(int index, int screenId, int toScreenId, bool hide) {

			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.OnScreenTransition(index, screenId, toScreenId, !hide)));

		}

		public static void SendEvent(int screenId, string group1, string group2 = null, string group3 = null, int weight = 1) {

			if (group1 == null) group1 = string.Empty;
			if (group2 == null) group2 = string.Empty;
			if (group3 == null) group3 = string.Empty;

			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.OnEvent(screenId, group1, group2, group3, weight)));

		}

		public static void SendEvent(string eventName, string group1, string group2 = null, string group3 = null, int weight = 1) {

			if (eventName == null) eventName = string.Empty;
			if (group1 == null) group1 = string.Empty;
			if (group2 == null) group2 = string.Empty;
			if (group3 == null) group3 = string.Empty;

			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.OnEvent(eventName, group1, group2, group3, weight)));

		}

		public static void SendTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {

			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.OnTransaction(screenId, productId, price, currency, receipt, signature)));

		}

		public static void SendTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature) {

			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.OnTransaction(eventName, productId, price, currency, receipt, signature)));

		}

		public static void SendScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {
			
			Analytics.ForEachService<IAnalyticsService>(s => ME.Coroutines.Run(s.OnScreenPoint(screenId, screenWidth, screenHeight, tag, x, y)));

		}

	}

}