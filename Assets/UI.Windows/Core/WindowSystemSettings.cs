
namespace UnityEngine.UI.Windows {

	public class WindowSystemSettings : UnityEngine.ScriptableObject {

		[System.Serializable]
		public class Base {
			
			public float minDepth = 90f;
			public float maxDepth = 98f;
			public int poolSize = 100;

			public float minZDepth = 0f;

		}

		[System.Serializable]
		public class Camera {

			public bool orthographic = true;
			public float orthographicSize = 5f;
			public float nearClipPlane = -100f;
			public float farClipPlane = 100f;
			public bool useOcclusionCulling = false;
			public bool hdr = false;
			
			public void Apply(UnityEngine.Camera camera) {
				
				camera.orthographic = this.orthographic;
				camera.orthographicSize = this.orthographicSize;
				camera.nearClipPlane = this.nearClipPlane;
				camera.farClipPlane = this.farClipPlane;
				camera.useOcclusionCulling = this.useOcclusionCulling;
				camera.hdr = this.hdr;

			}

		}

		[System.Serializable]
		public class SortingLayer {

			public string name = "Windows";

		}

		public Base baseInfo;
		public Camera camera;
		public SortingLayer sortingLayer;

		public void Apply(UnityEngine.Camera camera = null, UnityEngine.Canvas canvas = null) {

			if (camera != null) this.camera.Apply(camera);
			if (canvas != null) canvas.sortingLayerName = this.sortingLayer.name;

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowSystemSettings>();
			
		}
		#endif

	}

}