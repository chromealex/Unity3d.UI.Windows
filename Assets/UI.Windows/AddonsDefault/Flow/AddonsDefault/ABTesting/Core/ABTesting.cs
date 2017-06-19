using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.ABTesting {

	public class ABTesting : ServiceManager<ABTesting> {
		
		public override string GetServiceName() {
			
			return "ABTesting";
			
		}
		
		public override AuthKeyPermissions GetAuthPermission() {
			
			return AuthKeyPermissions.ABTesting;
			
		}

		public override System.Collections.IEnumerator OnAfterAuth(IService serviceBase) {

			var service = serviceBase as ABTestingService;

			yield return this.StartCoroutine(service.GetDataAll((result) => {

				if (result.hasError == false) {

					foreach (var abTest in result.data) {

						var window = FlowSystem.GetData().GetWindow(abTest.Key);
						if (window != null) {

							window.abTests = new ABTestingItems(abTest.Value);

						}

					}

				}

			}));

			yield return 0;

		}

	}

}