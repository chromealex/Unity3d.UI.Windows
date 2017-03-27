using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Types;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.ABTesting {

	public class ABTestingSettings : ServiceSettings {
		
		public List<ABTestingServiceItem> items = new List<ABTestingServiceItem>();
		
		public override void AddService(ServiceItem item) {
			
			this.items.Add(item as ABTestingServiceItem);
			
		}
		
		public override List<ServiceItem> GetItems() {
			
			return this.items.Cast<ServiceItem>().ToList();
			
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/ABTesting/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<ABTestingSettings>();
			
		}
		#endif

	}

}