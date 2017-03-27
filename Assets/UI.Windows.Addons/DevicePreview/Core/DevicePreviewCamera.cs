#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Plugins.DevicePreview {

	[ExecuteInEditMode]
	public class DevicePreviewCamera : MonoBehaviour {

		public float timeout = 0.2f;
		private RenderTexture targetTexture;
		private System.Action callback;

		public void Initialize(RenderTexture texture, System.Action callback) {

			this.targetTexture = texture;
			this.callback = callback;

			this.StartCoroutine(this.Render());

		}

		public void CleanUp() {

			GameObject.DestroyImmediate(this.gameObject);

		}

		private Vector2 mainGameViewSize;
		public void RestoreMainGameViewSize() {

			this.SetMainGameViewSize(Mathf.RoundToInt(this.mainGameViewSize.x), Mathf.RoundToInt(this.mainGameViewSize.y));

		}

		private EditorWindow GetMainGameView() {
			
			var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			var getMainGameView = gameViewType.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			return (UnityEditor.EditorWindow)getMainGameView.Invoke(null,null);
			
		}

		private void SetMainGameViewSize(int width, int height) {

			var gameView = this.GetMainGameView();
			var prop = gameView.GetType().GetProperty("currentGameViewSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var gvsize = prop.GetValue(gameView, new object[0]{});
			var gvSizeType = gvsize.GetType();

			var w = gvSizeType.GetProperty("width", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			var h = gvSizeType.GetProperty("height", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			//I have 2 instance variable which this function sets:
			this.mainGameViewSize.x = (int)w.GetValue(gvsize, new object[0]{});
			this.mainGameViewSize.y = (int)h.GetValue(gvsize, new object[0]{});

			w.SetValue(gvsize, width, null);
			h.SetValue(gvsize, height, null);

		}

		private int currentIteration = 0;
		private bool working = false;
		public System.Collections.IEnumerator Render() {
			
			++this.currentIteration;

			if (this.working == true) {

				this.ResetWorking();
				//yield break;

			}

			this.canvasesInWork.Clear();

			this.working = true;

			this.SetMainGameViewSize(this.targetTexture.width, this.targetTexture.height);

			var allCanvases = Canvas.FindObjectsOfType<Canvas>();
			var allCameras = Camera.allCameras;
			foreach (var camera in allCameras) {

				var found = false;
				foreach (var canvas in allCanvases) {

					if (canvas.renderMode == RenderMode.ScreenSpaceCamera &&
					    canvas.worldCamera == camera) {
						
						yield return this.StartCoroutine(this.Render(this.currentIteration, camera, canvas));
						found = true;

					}

				}

				if (found == false) yield return this.StartCoroutine(this.Render(this.currentIteration, camera, null));

			}

			this.working = false;
			
			this.RestoreMainGameViewSize();

			if (this.callback != null) this.callback();

		}

		public void ResetWorking() {

			foreach (var canvas in this.canvasesInWork) {

				canvas.renderMode = RenderMode.ScreenSpaceCamera;

			}

		}

		private List<Canvas> canvasesInWork = new List<Canvas>();
		public System.Collections.IEnumerator Render(int currentIteration, Camera camera, Canvas canvas) {

			if (canvas != null) {

				// Workout of unity bug: Canvas render mode

				canvas.renderMode = RenderMode.WorldSpace;
				this.canvasesInWork.Add(canvas);

				// Delegate render
				yield return this.StartCoroutine(this.ResetRender(currentIteration, camera, canvas));

			} else {

				if (currentIteration == this.currentIteration) {

					//camera.RenderToCubemap(this.targetTexture);
					Graphics.Blit(this.TakeScreenshot(camera), this.targetTexture);

				} else {

					Debug.LogWarning("Out of sync: " + currentIteration + "/" + this.currentIteration);

				}

			}

		}

		public System.Collections.Generic.IEnumerator<byte> ResetRender(int currentIteration, Camera camera, Canvas canvas) {
			
			var canvasScaler = canvas.GetComponent<CanvasScaler>();
			if (canvasScaler != null) {
				
				canvasScaler.SendMessage("Update");
				
			}

			var current = Selection.instanceIDs;

			Selection.activeGameObject = canvas.gameObject;

			Canvas.ForceUpdateCanvases();

			var timeout = this.timeout;
			var time = UnityEditor.EditorApplication.timeSinceStartup;
			while (UnityEditor.EditorApplication.timeSinceStartup < time + timeout) {

				yield return 0;

			}

			Canvas.ForceUpdateCanvases();

			this.StartCoroutine(this.Render(currentIteration, camera, null));

			if (currentIteration == this.currentIteration) {

				canvas.renderMode = RenderMode.ScreenSpaceCamera;

			} else {
				
				Debug.LogWarning("Out of sync: " + currentIteration + "/" + this.currentIteration);

			}

			Selection.instanceIDs = current;

		}

		public void OnRenderImage(RenderTexture src, RenderTexture dest) {

			this.targetTexture = src;
			RenderTexture.active = src;
			Graphics.Blit(src, dest);

		}

		public Texture2D TakeScreenshot(Camera camera) {

			var width = this.targetTexture.width;
			var height = this.targetTexture.height;

			Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

			camera.targetTexture = this.targetTexture;
			camera.Render();

			RenderTexture.active = this.targetTexture;

			screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			screenshot.Apply(false);

			camera.targetTexture = null;

			RenderTexture.active = null;

			return screenshot;

		}

	}

}
#endif