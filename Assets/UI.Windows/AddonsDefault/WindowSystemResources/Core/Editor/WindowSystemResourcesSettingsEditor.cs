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
using UnityEngine.UI.Windows.Plugins.Resources;

namespace UnityEditor.UI.Windows.Plugins.Resources {

	[CustomEditor(typeof(WindowSystemResourcesSettings))]
	public class WindowSystemResourcesSettingsEditor : ServiceSettingsEditor {

		public override string GetServiceName() {

			return "Resources";

		}

		public override string GetNamespace() {

			return "UnityEngine.UI.Windows.Plugins.Resources.Services";

		}
		
		public override void OnInspectorGUI() {

			this.DrawServices<WindowSystemResourcesServiceItem>();

		}

	}

}