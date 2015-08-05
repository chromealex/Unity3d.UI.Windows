using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Extensions {

	public class CanvasUpdater : MonoBehaviour {

		public Canvas canvas;

		public delegate void OnUpdateHandler();
		public static event OnUpdateHandler onUpdate;
		
		public void Awake() {
			
			CanvasUpdater.onUpdate += this.OnUpdate;
			
		}
		
		public void OnDestroy() {
			
			CanvasUpdater.onUpdate -= this.OnUpdate;
			
		}

		public void OnUpdate() {

			this.canvas.scaleFactor = 0f;
			this.canvas.scaleFactor = 1f;
			
		}

		public static void ForceUpdate() {

			if (CanvasUpdater.onUpdate != null) CanvasUpdater.onUpdate.Invoke();

		}

	}

}