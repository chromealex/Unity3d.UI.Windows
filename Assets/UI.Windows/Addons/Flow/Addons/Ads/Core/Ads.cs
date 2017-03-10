using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Ads {

	public class Ads : ServiceManager<Ads> {
		
		public override string GetServiceName() {

			return "Ads";

		}

		public static bool IsConnected() {

			var services = Ads.GetServices<IAdsService>();
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

			return AuthKeyPermissions.Ads;

		}

		public static bool CanShow() {

			var result = false;
			Ads.ForEachService<IAdsService>(s => {

				var resultShow = s.CanShow();
				if (resultShow == true) result = resultShow;

			});

			return result;

		}

		public static void Show(string name, Dictionary<object, object> options = null, System.Action onFinish = null, System.Action onFailed = null, System.Action onSkipped = null) {
			
			Ads.ForEachService<IAdsService>(s => ME.Coroutines.Run(s.Show(name, options, onFinish, onFailed, onSkipped)));

		}

	}

}