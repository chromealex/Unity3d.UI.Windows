using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows.Plugins.Social;
using UnityEngine.UI.Windows.Plugins.Social.Core;

namespace UnityEditor.UI.Windows.Plugins.Social {

	[CustomEditor(typeof(SocialSettings))]
	public class SocialSettingsEditor : Editor {

		private static ModuleSettings[] socials;

		public void OnEnable() {
			
			// Scan for the installed socials
			SocialSettingsEditor.socials = ME.EditorUtilities.GetAssetsOfType<ModuleSettings>(useCache: false);

		}

		public override void OnInspectorGUI() {

			var target = this.target as SocialSettings;
			var socials = SocialSettingsEditor.socials;

			if (target.activePlatforms == null) target.activePlatforms = new Platform[0];

			if (target.activePlatforms.Length != socials.Length) {

				System.Array.Resize(ref target.activePlatforms, socials.Length);

			}

			for (int i = 0; i < target.activePlatforms.Length; ++i) {
				
				var sourcePlatform = socials[i];

				if (target.activePlatforms[i] == null) {

					target.activePlatforms[i] = new Platform();
					target.activePlatforms[i].active = true;
					target.activePlatforms[i].settings = sourcePlatform;

				}

				var platform = target.activePlatforms[i];
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				{
					platform.active = EditorGUILayout.ToggleLeft(sourcePlatform.GetPlatformName(), platform.active, EditorStyles.boldLabel);
					var oldState = GUI.enabled;
					GUI.enabled = platform.active;
					platform.settings = EditorGUILayout.ObjectField(platform.settings, typeof(ModuleSettings), false) as ModuleSettings;
					GUI.enabled = oldState;
				}
				EditorGUILayout.EndVertical();

			}

		}

	}

}