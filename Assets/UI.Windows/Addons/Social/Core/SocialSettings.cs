using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Social {

	public class SocialSettings : ScriptableObject {

		[BitMask(typeof(Platform))]
		public Platform activePlatforms;

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Social/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<SocialSettings>();
			
		}
		#endif

	}

}