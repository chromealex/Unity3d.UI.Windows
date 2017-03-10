using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Types;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Resources {

	public class WindowSystemResourcesSettings : ServiceSettings {

		public bool loadFromStreamingAssets;
		public string url;
		public List<WindowSystemResourcesServiceItem> items = new List<WindowSystemResourcesServiceItem>();
		
		public override void AddService(ServiceItem item) {
			
			this.items.Add(item as WindowSystemResourcesServiceItem);
			
		}
		
		public override List<ServiceItem> GetItems() {
			
			return this.items.Cast<ServiceItem>().ToList();
			
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Resources/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowSystemResourcesSettings>();
			
		}
		#endif

	}

}