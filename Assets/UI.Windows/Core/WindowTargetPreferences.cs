using UnityEngine;

namespace UnityEngine.UI.Windows {

	public class WindowTargetPreferences : ScriptableObject {

		public TargetPreferences preferences = new TargetPreferences();

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Target Preferences")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowTargetPreferences>();
			
		}
		#endif


	}

}