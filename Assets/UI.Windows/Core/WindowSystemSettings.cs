
using System.Linq;namespace UnityEngine.UI.Windows {

	public class WindowSystemSettings : UnityEngine.ScriptableObject {

		[System.Serializable]
		public class Base {

			[System.Serializable]
			public struct DepthLayer {

				public int layer;
				public float minDepth;
				public float maxDepth;
				public float minZDepth;

			}
			
			public int poolSize = 100;
			public int preallocatedWindowsPoolSize = 0;

			public float zDepthStep = 200f;

			public DepthLayer[] depthLayers = new DepthLayer[] {
				new DepthLayer() { layer = -4, minDepth = -89f, maxDepth = -70f, minZDepth = -40000f },
				new DepthLayer() { layer = -3, minDepth = -69f, maxDepth = -50f, minZDepth = -30000f },
				new DepthLayer() { layer = -2, minDepth = -49f, maxDepth = -30f, minZDepth = -20000f },
				new DepthLayer() { layer = -1, minDepth = -29f, maxDepth = -10f, minZDepth = -10000f },
				new DepthLayer() { layer = 0, minDepth = 1f, maxDepth = 20f, minZDepth = 0f },
				new DepthLayer() { layer = 1, minDepth = 21f, maxDepth = 40f, minZDepth = 10000f },
				new DepthLayer() { layer = 2, minDepth = 41f, maxDepth = 60f, minZDepth = 20000f },
				new DepthLayer() { layer = 3, minDepth = 61f, maxDepth = 80f, minZDepth = 30000f },
				new DepthLayer() { layer = 4, minDepth = 81f, maxDepth = 100f, minZDepth = 40000f }
			};

			public float GetMinDepth(int layer) {
				
				return this.depthLayers.First(x => x.layer == layer).minDepth;
				
			}
			
			public float GetMaxDepth(int layer) {
				
				return this.depthLayers.First(x => x.layer == layer).maxDepth;

			}
			
			public float GetMinZDepth(int layer) {
				
				return this.depthLayers.First(x => x.layer == layer).minZDepth;

			}

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