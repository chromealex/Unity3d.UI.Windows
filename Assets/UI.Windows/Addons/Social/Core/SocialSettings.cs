using UnityEngine;
using System.Collections;
using System;

namespace UnityEngine.UI.Windows.Plugins.Social {

	public class SubModuleSettings {
		
		[TextArea]
		public string url;
		public HTTPType method;
		public HTTPParams parameters;

	}
	
	[System.Serializable]
	public class FriendsSettings : SubModuleSettings {
		
	}

	[System.Serializable]
	public class ProfileSettings : SubModuleSettings {

	}
	
	[System.Serializable]
	public class AuthSettings : SubModuleSettings {

		public enum ProfileLoadType : byte {
			LoadAutoProfileAfterLogin = 0,
			DontLoadProfile = 1,
			LoadStandardProfile = 2,
		};

		[Header("Profile Loading")]
		public ProfileLoadType profileLoadType = ProfileLoadType.LoadAutoProfileAfterLogin;
		public ProfileSettings autoProfileSettings;

	}

	public class ModuleSettings : ScriptableObject {
		
		[Header("Base Settings")]
		public FriendsSettings standardFriendsSettings;
		public ProfileSettings standardProfileSettings;
		public AuthSettings authSettings;

		public virtual HTTPParams Prepare(string token, HTTPParams parameters) {

			return parameters;

		}

		public virtual int GetPermissionsMask() {
			
			return 0;
			
		}
		
		public virtual string GetPermissions() {

			return string.Empty;

		}

	}

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