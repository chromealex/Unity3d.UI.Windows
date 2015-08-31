using UnityEngine;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows {

	public class WindowGUIUtilities {

		public static bool ButtonAddon(string addonName, string buttonCaption, params GUILayoutOption[] options) {
			
			return WindowGUIUtilities.ButtonAddon(addonName, buttonCaption, string.Empty, GUI.skin.button, options);
			
		}

		public static bool ButtonAddon(string addonName, string buttonCaption, GUIStyle buttonStyle, params GUILayoutOption[] options) {
			
			return WindowGUIUtilities.ButtonAddon(addonName, buttonCaption, string.Empty, buttonStyle, options);

		}

		public static bool ButtonAddon(string addonName, string buttonCaption, string description, params GUILayoutOption[] options) {

			return WindowGUIUtilities.ButtonAddon(addonName, buttonCaption, description, GUI.skin.button, options);

		}

		public static bool ButtonAddon(string addonName, string buttonCaption, string description, GUIStyle buttonStyle, params GUILayoutOption[] options) {

			var result = false;

			if (CoreUtilities.IsAddonAvailable(addonName) == true) {
				
				if (GUILayout.Button(buttonCaption, buttonStyle, options) == true) {

					result = true;

				}
				
			} else {

				if (string.IsNullOrEmpty(description) == false) {
					
					EditorGUILayout.HelpBox("Addon `" + addonName + "` not found. Install it from the site: " + VersionInfo.DOWNLOAD_LINK, MessageType.None);

					EditorGUILayout.HelpBox(description, MessageType.Info);

				}

			}

			return result;

		}
		
	}

}