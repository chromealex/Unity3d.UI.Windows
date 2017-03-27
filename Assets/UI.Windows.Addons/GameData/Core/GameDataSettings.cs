using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Types;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.GameData {

	public class GameDataSettings : ServiceSettings {

		public string url;
		public string eTag;
		public List<GameDataServiceItem> items = new List<GameDataServiceItem>();
		
		public override void AddService(ServiceItem item) {
			
			this.items.Add(item as GameDataServiceItem);
			
		}
		
		public override List<ServiceItem> GetItems() {
			
			return this.items.Cast<ServiceItem>().ToList();
			
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/GameData/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<GameDataSettings>();
			
		}
		#endif

	}

}