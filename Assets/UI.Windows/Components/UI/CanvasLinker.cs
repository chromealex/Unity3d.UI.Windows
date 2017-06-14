using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows.Extensions {
	
	public class CanvasLinker : MonoBehaviour {
		
		[Header("Any Window Object (Required)")]
		public WindowObject windowObject;

		[Header("Targets")]
		public UnityEngine.Canvas canvas;
		public BaseRaycaster raycaster;
		
		[Header("Parameters")]
		public int orderDelta;
		public bool overridePixelPerfect;
		[Hidden("overridePixelPerfect", state: false)]
		public bool pixelPrefect = true;

		[Header("Raycaster Source (Optional)")]
		public BaseRaycaster raycasterSource;

		public void UpdateOrderDelta() {

			var window = this.windowObject.GetWindow();
			if (window != null) {
				
				this.canvas.sortingOrder = window.GetSortingOrder() + this.orderDelta;

			}

		}

		public void OnEnable() {

			this.Init();

		}

		public void OnDisable() {

			WindowSystem.GetEvents().Unregister(this.windowObject, WindowEventType.OnShowBegin, this.Init);
			WindowSystem.GetEvents().Unregister(this.windowObject, WindowEventType.OnWindowActive, this.Init);

		}

		public void Init() {

			if (this.canvas != null && this.windowObject != null && this.windowObject.GetWindow() == null) {

				WindowSystem.GetEvents().Register(this.windowObject, WindowEventType.OnShowBegin, this.Init);
				WindowSystem.GetEvents().Register(this.windowObject, WindowEventType.OnWindowActive, this.Init);
				return;

			}

			WindowSystem.GetEvents().Unregister(this.windowObject, WindowEventType.OnShowBegin, this.Init);
			WindowSystem.GetEvents().Unregister(this.windowObject, WindowEventType.OnWindowActive, this.Init);

			var raycasterTemp = false;
			if (this.canvas != null && this.windowObject != null) {
				
				var window = this.windowObject.GetWindow();
				if (window == null) {

					#if UNITY_EDITOR
					Debug.LogWarning("[ CanvasLinker ] WindowObject::GetWindow() is null", this);
					#endif
					return;

				}

				//Debug.Log("[ CanvasLinker ] Initialized", this);

				this.canvas.overrideSorting = true;
				this.canvas.sortingLayerName = window.GetSortingLayerName();
				this.canvas.sortingOrder = window.GetSortingOrder() + this.orderDelta;

				if (this.overridePixelPerfect == true) {

					this.canvas.overridePixelPerfect = true;
					this.canvas.pixelPerfect = this.pixelPrefect;

				}

				if (this.raycasterSource == null) {

					this.raycasterSource = (window as LayoutWindowType).GetCurrentLayout().GetLayoutInstance().GetComponent<BaseRaycaster>();
					raycasterTemp = true;

				}

			}
			
			if (this.raycaster != null && this.raycasterSource != null) {
				
				var graphicRaycaster = this.raycaster as GraphicRaycaster;
				var graphicRaycasterSource = this.raycasterSource as GraphicRaycaster;
				if (graphicRaycaster != null && graphicRaycasterSource != null) {
					
					graphicRaycaster.blockingObjects = graphicRaycasterSource.blockingObjects;
					graphicRaycaster.ignoreReversedGraphics = graphicRaycasterSource.ignoreReversedGraphics;
					
				}

				if (raycasterTemp == true) {

					this.raycasterSource = null;

				}
				
			}
			
		}
		
	}

}