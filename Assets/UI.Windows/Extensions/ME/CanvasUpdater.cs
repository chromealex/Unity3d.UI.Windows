using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Extensions {

	public class CanvasUpdater : MonoBehaviour {
		
		public Canvas canvas;
		public CanvasScaler canvasScaler;

		public delegate void OnUpdateHandler();
		public static event OnUpdateHandler onUpdate;
		
		public void Awake() {
			
			CanvasUpdater.onUpdate += this.OnUpdate;
			
		}
		
		public void OnDestroy() {
			
			CanvasUpdater.onUpdate -= this.OnUpdate;
			
		}

		public void OnValidate() {
			
			this.canvas = this.GetComponent<Canvas>();
			this.canvasScaler = this.GetComponent<CanvasScaler>();

		}

		public void OnUpdate() {

			CanvasUpdater.ForceUpdate(this.canvas, this.canvasScaler);
			
		}
		
		public static void ForceUpdate() {
			
			if (CanvasUpdater.onUpdate != null) CanvasUpdater.onUpdate.Invoke();
			
		}
		
		public static void ForceUpdate(Canvas canvas, CanvasScaler canvasScaler) {
			
			canvas.scaleFactor = 0.99f;
			canvas.scaleFactor = 1f;

			if (canvasScaler != null) {

				canvasScaler.scaleFactor = 1f;

			}

		}

	}

}