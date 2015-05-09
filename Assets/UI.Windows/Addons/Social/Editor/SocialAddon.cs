using UnityEngine;
using System.Collections;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Social;

namespace UnityEditor.UI.Windows.Components.Social {

	public class Social : FlowAddon {

		private SocialSettings settings;

		public override void OnFlowSettingsGUI() {
			
			GUILayout.Label("Module Installed");
			
		}

	}

}