
namespace UnityEngine.UI.Extensions {

	public class CanvasUpdater : MonoBehaviour {
		
		public Canvas canvas;
		public CanvasScaler canvasScaler;

		/*public delegate void OnUpdateHandler();
		public static event OnUpdateHandler onUpdate;
		
		public void Awake() {
			
			CanvasUpdater.onUpdate += this.OnUpdate;
			
		}
		
		public void OnDestroy() {
			
			CanvasUpdater.onUpdate -= this.OnUpdate;
			
		}

		public void OnUpdate() {

			CanvasUpdater.ForceUpdate(this.canvas, this.canvasScaler);
			
		}*/
		
		/*public static void ForceUpdate() {
			
			if (CanvasUpdater.onUpdate != null) CanvasUpdater.onUpdate.Invoke();
			
		}*/
		
		public static void ForceUpdate(Canvas canvas, CanvasScaler canvasScaler = null) {

			/*if (canvas == null) return;
			if (Application.isPlaying == false) return;

			const float delta = 0.05f;

			var _factor = canvas.scaleFactor;
			canvas.scaleFactor = _factor - delta;
			canvas.scaleFactor = _factor;

			if (canvasScaler != null) {

				_factor = canvasScaler.scaleFactor;
				canvasScaler.scaleFactor = _factor - delta;
				canvasScaler.scaleFactor = _factor;

			}*/

		}
		
		#if UNITY_EDITOR
		public void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			this.canvas = this.GetComponent<Canvas>();
			this.canvasScaler = this.GetComponent<CanvasScaler>();
			
		}
		#endif

	}

}