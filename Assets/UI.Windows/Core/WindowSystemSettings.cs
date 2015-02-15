
public class WindowSystemSettings : UnityEngine.ScriptableObject {

	[System.Serializable]
	public class Base {
		
		public float minDepth = 90f;
		public float maxDepth = 98f;
		public int poolSize = 100;

	}

	[System.Serializable]
	public class Camera {

		public bool orthographic = true;
		public float orthographicSize = 5f;
		public float nearClipPlane = -100f;
		public float farClipPlane = 100f;
		public bool useOcclusionCulling = false;
		public bool hdr = false;

	}

	[System.Serializable]
	public class SortingLayer {

		public string name = "Windows";

	}

	public Base baseInfo;
	public Camera camera;
	public SortingLayer sortingLayer;

	public void Apply(UnityEngine.Camera camera = null, UnityEngine.Canvas canvas = null) {



	}

	#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Create/UI Windows/Settings")]
	public static void CreateInstance() {
		
		ME.EditorUtilities.CreateAsset<WindowSystemSettings>();
		
	}
	#endif

}
