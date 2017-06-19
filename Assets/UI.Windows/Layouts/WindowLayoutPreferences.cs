using UnityEngine;

namespace UnityEngine.UI.Windows {

	public class WindowLayoutPreferences : ScriptableObject {
		
		public bool fixedScale;
		public Vector2 fixedScaleResolution = new Vector2(1024f, 768f);
		[Range(0f, 1f)]
		public float matchWidthOrHeight = 0f;

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Layout Preferences")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowLayoutPreferences>();
			
		}
		#endif


	}

}