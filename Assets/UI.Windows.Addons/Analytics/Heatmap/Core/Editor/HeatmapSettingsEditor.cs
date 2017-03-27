using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Analytics;
using ME;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEditor.UI.Windows.Plugins.Services;

namespace UnityEditor.UI.Windows.Plugins.Heatmap {

	[CustomEditor(typeof(HeatmapSettings))]
	public class HeatmapSettingsEditor : ServiceSettingsEditor {

		public override string GetServiceName() {
			
			return "Analytics";
			
		}
		
		public override string GetNamespace() {
			
			return "UnityEngine.UI.Windows.Plugins.Analytics.Services";
			
		}
		
		public override void OnInspectorGUI() {
			
			this.DrawServices<AnalyticsServiceItem>();
			
		}

	}

}