using UnityEngine;

namespace UnityEditor.UI.Windows {

	public class WindowGUIUtilities {
		
		public static bool ButtonAddon(string addonName, string buttonCaption, string description = "") {

			var result = false;

			if (WindowUtilities.IsAddonAvailable(addonName) == true) {
				
				if (GUILayout.Button(buttonCaption) == true) {

					result = true;

				}
				
			} else {
				
				EditorGUILayout.HelpBox("Addon `" + addonName + "` not found. Install it from the site: " + VersionInfo.downloadLink, MessageType.None);

				if (string.IsNullOrEmpty(description) == false) {

					EditorGUILayout.HelpBox(description, MessageType.Info);

				}

			}

			return result;

		}
		
	}

}