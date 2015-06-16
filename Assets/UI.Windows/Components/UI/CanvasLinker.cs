using System.Collections;

namespace UnityEngine.UI.Windows.Extensions {

	public class CanvasLinker : MonoBehaviour {

		public WindowObject windowObject;
		public UnityEngine.Canvas canvas;
		public int orderDelta;

		public void Start() {

			this.Init();

		}

		public void Init() {
			
			if (this.canvas != null && this.windowObject != null) {

				var window = this.windowObject.GetWindow();
				if (window == null) return;

				this.canvas.overrideSorting = true;
				this.canvas.sortingLayerName = window.GetSortingLayerName();
				this.canvas.sortingOrder = window.GetSortingOrder() + this.orderDelta;

			}

		}

	}

}