using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEditor.UI.Windows.Plugins.Social {
	
	[CustomEditor(typeof(ModuleSettings), true)]
	public class SocialModuleSettingsEditor : Editor {

		public override void OnInspectorGUI() {

			this.DrawDefaultInspector();

			var target = this.target as ModuleSettings;
			target.OnInspectorGUI();

		}

	}

}
