using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Types;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Console {

	public class ConsoleSettings : ServiceSettings {

		public ConsoleScreen screen;

		public override void AddService(ServiceItem item) {}
		public override List<ServiceItem> GetItems() { return null; }

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Console/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<ConsoleSettings>();
			
		}
		#endif

	}

}