using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Ads;

namespace UnityEngine.UI.Windows.Plugins.Ads {

	public class AdsSettings : ServiceSettings {
		
		public List<AdsServiceItem> items = new List<AdsServiceItem>();

		public override void AddService(ServiceItem item) {

			this.items.Add(item as AdsServiceItem);

		}

		public override List<ServiceItem> GetItems() {

			return this.items.Cast<ServiceItem>().ToList();

		}

		public override void SetChanged() {
			
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Ads/Settings")]
		public static void CreateInstance() {

			ME.EditorUtilities.CreateAsset<AdsSettings>();

		}
		#endif

	}

}