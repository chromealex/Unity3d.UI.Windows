using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI.Windows.Plugins.Flow;
using ME;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.UI.Windows.Plugins.ABTesting;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEditor.UI.Windows.Plugins.Services;

namespace UnityEditor.UI.Windows.Plugins.ABTesting {

	[CustomEditor(typeof(ABTestingSettings))]
	public class ABTestingSettingsEditor : ServiceSettingsEditor {

		public override string GetServiceName() {

			return "ABTesting";

		}

		public override string GetNamespace() {

			return "UnityEngine.UI.Windows.Plugins.ABTesting.Services";

		}
		
		public override void OnInspectorGUI() {

			this.DrawServices<ABTestingServiceItem>();

		}

	}

}