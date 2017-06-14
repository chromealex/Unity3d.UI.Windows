using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using uiExtensions = UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Components {

	public class CarouselInnerDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {

		public WindowComponent windowObject;
		public CarouselComponent carouselComponent;

		public uiExtensions::ScrollRect scollRect;
		private bool isCrouselMovement;
		private bool isScrollRectMovement;

		public void Start() {

			this.scollRect.manualMoving = true;

			if (this.windowObject != null) {

				if (this.windowObject is CarouselComponent) {

					this.carouselComponent = this.windowObject as CarouselComponent;
					return;

				}

				this.carouselComponent = this.windowObject.GetSubComponentInParent<CarouselComponent>();

			}

		}

		#region Drag Handlers
		public void OnBeginDrag(PointerEventData eventData) {
			
			var horizontal = (Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y));
			this.isCrouselMovement = (this.carouselComponent.axis == CarouselComponent.Axis.Vertical && horizontal == false) || (this.carouselComponent.axis == CarouselComponent.Axis.Horizontal && horizontal == true);
			this.isScrollRectMovement = (this.scollRect.vertical == true && horizontal == false) || (this.scollRect.horizontal == true && horizontal == true);

			if (this.isCrouselMovement == true) {

				if (this.carouselComponent != null) this.carouselComponent.OnBeginDrag(eventData);

			} else if (this.isScrollRectMovement == true) {

				this.scollRect.DoBeginDrag(eventData);

			}

		}

		public void OnDrag(PointerEventData eventData) {
			
			if (this.isCrouselMovement == true) {

				if (this.carouselComponent != null) this.carouselComponent.OnDrag(eventData);

			} else if (this.isScrollRectMovement == true) {

				this.scollRect.DoDrag(eventData);

			}

		}

		public void OnEndDrag(PointerEventData eventData) {

			if (this.isCrouselMovement == true) {

				if (this.carouselComponent != null) this.carouselComponent.OnEndDrag(eventData);

			} else if (this.isScrollRectMovement == true) {

				this.scollRect.DoEndDrag(eventData);

			}

		}
		#endregion

	}

}