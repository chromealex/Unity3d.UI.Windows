using UnityEngine;
using System.Collections;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Social;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEditor.UI.Windows.Components.Social {

	public class Social : FlowAddon {

		private static SocialSettings settings;

		private Editor editor;

		public override void OnFlowSettingsGUI() {

			if (Social.settings == null) Social.settings = ME.EditorUtilities.GetAssetsOfType<SocialSettings>(useCache: false).FirstOrDefault();

			var settings = Social.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (SocialSettings)."), MessageType.Error);

			} else {

				GUILayout.Label(FlowAddon.MODULE_INSTALLED);

				if (this.editor == null) this.editor = Editor.CreateEditor(settings);
				if (this.editor != null) {

					this.editor.OnInspectorGUI();

				}

			}

		}

	}

}