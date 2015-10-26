using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Analytics {

	public class Analytics : MonoBehaviour {

		private static Analytics instance;

		public HeatmapSettings settings;
		private List<AnalyticsService> services = new List<AnalyticsService>();

		public void Awake() {
			
			Analytics.instance = this;

		}

		public IEnumerator Start() {

			Debug.Log("[ Analytics ] Initializing...");
			
			var settings = this.settings;
			if (settings == null) yield break;
			
			WindowSystem.onTransition.AddListener(Analytics.OnScreenTransition);

			var items = settings.keys;

			foreach (var service in this.services) {

				for (int i = 0; i < items.Count; ++i) {
					
					var item = items[i];
					if (item.platformName == service.GetPlatformName()) {

						service.isActive = item.enabled;
						if (service.isActive == true) yield return this.StartCoroutine(service.Auth(item.authKey));
						break;

					}

				}

			}
			
			Debug.Log("[ Analytics ] Initialized");

		}

		public void Register(AnalyticsService service) {

			this.services.Add(service);

		}

		public static void SetUserId(string id) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif
			
			foreach (var service in Analytics.instance.services) {
				
				if (service.isActive == true) {
					
					Analytics.instance.StartCoroutine(service.SetUserId(id));
					
				}
				
			}
			
		}

		public static void SetUserId(long id) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			foreach (var service in Analytics.instance.services) {
				
				if (service.isActive == true) {
					
					Analytics.instance.StartCoroutine(service.SetUserId(id));
					
				}
				
			}

		}
		
		public static void SetUserGender(User.Gender gender) {

			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			foreach (var service in Analytics.instance.services) {
				
				if (service.isActive == true) {
					
					Analytics.instance.StartCoroutine(service.SetUserGender(gender));
					
				}
				
			}

		}
		
		public static void SetUserBirthYear(int birthYear) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			foreach (var service in Analytics.instance.services) {
				
				if (service.isActive == true) {
					
					Analytics.instance.StartCoroutine(service.SetUserBirthYear(birthYear));
					
				}
				
			}

		}

		public static void OnScreenTransition(int index, int screenId, int toScreenId, bool hide) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			foreach (var service in Analytics.instance.services) {
				
				if (service.isActive == true) {

					Analytics.instance.StartCoroutine(service.OnScreenTransition(index, screenId, toScreenId));

				}

			}

		}
		
		public static void SendEvent(int screenId, string group1, string group2 = null, string group3 = null, int weight = 1) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			foreach (var service in Analytics.instance.services) {
				
				if (service.isActive == true) {
					
					Analytics.instance.StartCoroutine(service.OnEvent(screenId, group1, group2, group3, weight));
					
				}
				
			}
		}
		
		public static void SendTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			foreach (var service in Analytics.instance.services) {
				
				if (service.isActive == true) {
					
					Analytics.instance.StartCoroutine(service.OnTransaction(screenId, productId, price, currency, receipt, signature));
					
				}
				
			}
		}
		
		public static void SendScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			foreach (var service in Analytics.instance.services) {
				
				if (service.isActive == true) {
					
					Analytics.instance.StartCoroutine(service.OnScreenPoint(screenId, screenWidth, screenHeight, tag, x, y));
					
				}
				
			}
		}

	}

}