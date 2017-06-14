
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

			[Tooltip("Turn off camera render instead of GameObject.SetActive(false)")]
			public bool turnOffCameraRenderOnly = true;

			[Tooltip("Restores UI.EventSystem last selected element on window activation")]
			public bool restoreSelectedElement = true;

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

			public bool IsRestoreSelectedElement() {

				return this.restoreSelectedElement;

			}

		}

		[System.Serializable]
		public class Canvas {

			public bool overridePixelPerfect;
			[ReadOnly("overridePixelPerfect", state: false)]
			public bool pixelPerfect;

			public string sortingLayerName = "Windows";

			public void Apply(UnityEngine.Canvas canvas) {

			}

			public void ApplyEveryInstance(UnityEngine.Canvas canvas, WindowBase window) {

				if (this.overridePixelPerfect == true) canvas.pixelPerfect = this.pixelPerfect;
				canvas.sortingLayerName = this.sortingLayerName;

				if (window != null) {
					
					if (window.preferences.overrideCanvasPixelPerfect == true) {

						canvas.pixelPerfect = window.preferences.canvasPixelPerfect;

					}

				}

			}

		}

		[System.Serializable]
		public class Camera {

			public enum Mode : byte {

				Auto = 0,
				Orthographic,
				Perspective,

			}

			public bool orthographic = true;

			[Header("Orthographic")]
			[ReadOnly("orthographic", false)]
			public float orthographicSize = 5f;
			[ReadOnly("orthographic", false)]
			public float orthographicNearClipPlane = -100f;
			[ReadOnly("orthographic", false)]
			public float orthographicFarClipPlane = 100f;

			[Header("Perspective")]
			[ReadOnly("orthographic", true)]
			public float fieldOfView = 60f;
			[ReadOnly("orthographic", true)]
			public float perspectiveNearClipPlane = 0.001f;
			[ReadOnly("orthographic", true)]
			public float perspectiveFarClipPlane = 1000f;

			[Header("Common")]
			public bool useOcclusionCulling = false;
			public bool hdr = false;
			public bool msaa = false;

			public Camera() {}

			public Camera(Camera other) {

				this.orthographic = other.orthographic;
				this.orthographicSize = other.orthographicSize;
				this.fieldOfView = other.fieldOfView;
				this.orthographicNearClipPlane = other.orthographicNearClipPlane;
				this.orthographicFarClipPlane = other.orthographicFarClipPlane;
				this.perspectiveNearClipPlane = other.perspectiveNearClipPlane;
				this.perspectiveFarClipPlane = other.perspectiveFarClipPlane;
				this.useOcclusionCulling = other.useOcclusionCulling;
				#if UNITY_5_6_OR_NEWER
				this.hdr = other.hdr;
				this.msaa = other.msaa;
				#else
				this.hdr = other.hdr;
				#endif

			}

			public void Apply(UnityEngine.Camera camera, Mode mode = Mode.Auto) {

				var nearClipPlane = 0f;
				var farClipPlane = 0f;
				var orthographic = false;

				if (mode == Mode.Auto) {
					
					nearClipPlane = (this.orthographic == true ? this.orthographicNearClipPlane : this.perspectiveNearClipPlane);
					farClipPlane = (this.orthographic == true ? this.orthographicFarClipPlane : this.perspectiveFarClipPlane);
					orthographic = this.orthographic;

				} else if (mode == Mode.Orthographic) {

					nearClipPlane = this.orthographicNearClipPlane;
					farClipPlane = this.orthographicFarClipPlane;
					orthographic = true;

				} else if (mode == Mode.Perspective) {

					nearClipPlane = this.perspectiveNearClipPlane;
					farClipPlane = this.perspectiveFarClipPlane;
					orthographic = false;

				}

				camera.orthographic = orthographic;
				camera.fieldOfView = this.fieldOfView;
				camera.orthographicSize = this.orthographicSize;
				camera.nearClipPlane = nearClipPlane;
				camera.farClipPlane = farClipPlane;
				camera.useOcclusionCulling = this.useOcclusionCulling;
				#if UNITY_5_6_OR_NEWER
				camera.allowHDR = this.hdr;
				camera.allowMSAA = this.msaa;
				#else
				camera.hdr = this.hdr;
				#endif

			}
			
			public void ApplyEveryInstance(UnityEngine.Camera camera) {
			}

		}

		public Base baseInfo;
		public Canvas canvas;
		public Camera camera;
		
		public void Apply(UnityEngine.Camera camera = null, UnityEngine.Canvas canvas = null, Camera.Mode mode = Camera.Mode.Auto) {
			
			if (camera != null) this.camera.Apply(camera, mode);
			if (canvas != null) this.canvas.Apply(canvas);
			
		}
		
		public void ApplyEveryInstance(UnityEngine.Camera camera = null, UnityEngine.Canvas canvas = null, WindowBase window = null) {
			
			if (camera != null) this.camera.ApplyEveryInstance(camera);
			if (canvas != null) this.canvas.ApplyEveryInstance(canvas, window);
			
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowSystemSettings>();
			
		}
		#endif

	}

}