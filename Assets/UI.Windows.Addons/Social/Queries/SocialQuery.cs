using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Social.Queries {

	public class SocialQuery : ScriptableObject {

		[TextArea]
		public string url;
		public HTTPType method;
		public HTTPParams parameters;

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Social/Query")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<SocialQuery>();
			
		}
		#endif

	}

}