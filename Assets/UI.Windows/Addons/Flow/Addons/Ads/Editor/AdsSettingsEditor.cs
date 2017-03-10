using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI.Windows.Plugins.Flow;
using ME;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEditor.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.Plugins.Ads;

namespace UnityEditor.UI.Windows.Plugins.Ads {

	[CustomEditor(typeof(AdsSettings))]
	public class AdsSettingsEditor : ServiceSettingsEditor {

		public override string GetServiceName() {
			
			return "Ads";
			
		}
		
		public override string GetNamespace() {
			
			return "UnityEngine.UI.Windows.Plugins.Ads.Services";
			
		}
		
		public override void OnInspectorGUI() {
			
			this.DrawServices<AdsServiceItem>();
			
		}

	}

}