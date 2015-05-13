using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Social.Core {
	
	[System.Serializable]
	public class Platform {
		
		public bool active;
		public ModuleSettings settings;
		
		public string GetPlatformName() {
			
			return this.settings.GetPlatformName();
			
		}
		
		public string GetPlatformClassName() {
			
			return this.settings.GetPlatformClassName();
			
		}
		
	}

	public class SocialSettings : ScriptableObject {

		public Platform[] activePlatforms;

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Social/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<SocialSettings>();
			
		}
		#endif

	}

}