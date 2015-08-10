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

		public void OnUpdate() {

			CanvasUpdater.ForceUpdate(this.canvas, this.canvasScaler);
			
		}
		
		public static void ForceUpdate() {
			
			if (CanvasUpdater.onUpdate != null) CanvasUpdater.onUpdate.Invoke();
			
		}
		
		public static void ForceUpdate(Canvas canvas, CanvasScaler canvasScaler) {

			if (canvas == null) return;

			var _factor = canvas.scaleFactor;
			canvas.scaleFactor = _factor - 0.01f;
			canvas.scaleFactor = _factor;

			if (canvasScaler != null) {

				_factor = canvasScaler.scaleFactor;
				canvasScaler.scaleFactor = _factor - 0.01f;
				canvasScaler.scaleFactor = _factor;

			}

		}
		
		#if UNITY_EDITOR
		public void OnValidate() {
			
			this.canvas = this.GetComponent<Canvas>();
			this.canvasScaler = this.GetComponent<CanvasScaler>();
			
		}
		#endif

	}

}