using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows.Extensions {
	
	[ExecuteInEditMode()]
	public class ScrollRect : UnityEngine.UI.ScrollRect, IScrollHandler {

		public ComponentEvent onBeginDragEvent = new ComponentEvent();
		public ComponentEvent onEndDragEvent = new ComponentEvent();
		public ComponentEvent onMovingBeginEvent = new ComponentEvent();
		public ComponentEvent onMovingEndEvent = new ComponentEvent();
		public LayoutElement layoutElement;

		private bool isMoving = false;

		protected override void Start() {

			base.Start();

			this.scrollSensitivity = WindowSystemInput.GetScrollSensitivity();

		}

		public override void OnScroll(PointerEventData data) {

			base.OnScroll(data);

			this.velocity = data.delta;

		}

		public override void OnBeginDrag(PointerEventData eventData) {

			base.OnBeginDrag(eventData);

			this.onBeginDragEvent.Invoke();

		}

		public override void OnEndDrag(PointerEventData eventData) {

			base.OnEndDrag(eventData);

			this.onEndDragEvent.Invoke();

		}

		public Vector2 GetSize() {

			if (this.content != null) {

				return this.content.rect.size;

			}

			return Vector2.zero;

		}

		public void Move(Vector2 delta) {
			
			var size = this.GetSize();
			var pos = this.normalizedPosition;
			pos.Scale(size);

			pos += delta;

			this.normalizedPosition = new Vector2(pos.x / size.x, pos.y / size.y);

		}

		protected override void LateUpdate() {

			base.LateUpdate();

			if (this.velocity.sqrMagnitude > 0.001f) {

				if (this.isMoving == false) {

					this.onMovingBeginEvent.Invoke();
					this.isMoving = true;

				}

			} else {

				if (this.isMoving == true) {

					this.onMovingEndEvent.Invoke();
					this.isMoving = false;

				}

			}

		}

		/*private bool horizontalLast;
		private bool verticalLast;

		private bool asDropdown = false;
		private float maxHeight;

		protected override void Start() {

			base.Start();
			
			this.horizontalLast = this.horizontal;
			this.verticalLast = this.vertical;

			this.UpdateScrollBars();

		}

		protected override void LateUpdate() {

			base.LateUpdate();
			
			if (this.horizontalLast != this.horizontal ||
			    this.verticalLast != this.vertical) {
				
				this.UpdateScrollBars();
				
			}

		}

		protected void UpdateScrollBars() {
			
			if (this.horizontalScrollbar != null) this.horizontalScrollbar.interactable = this.horizontal;
			if (this.verticalScrollbar != null) this.verticalScrollbar.interactable = this.vertical;

			this.horizontalLast = this.horizontal;
			this.verticalLast = this.vertical;

		}
		
		protected override void OnRectTransformDimensionsChange() {

			base.OnRectTransformDimensionsChange();
			
			this.UpdateDropdown();
			
		}
		
		protected override void OnTransformParentChanged() {

			base.OnTransformParentChanged();

			this.UpdateDropdown();
			
		}

		public void UpdateDropdown() {

			if (this.asDropdown == false) return;
			if (this.IsActive() == false) return;

			if (this.layoutElement != null) {
				
				var rect = this.transform as RectTransform;
				
				if (rect.sizeDelta.y >= this.maxHeight) {
					
					this.vertical = true;
					
					this.layoutElement.minHeight = maxHeight;
					this.layoutElement.preferredHeight = -1f;
					
				} else {
					
					this.vertical = false;
					
					this.layoutElement.minHeight = -1f;
					this.layoutElement.preferredHeight = maxHeight;
					
				}

				this.UpdateScrollBars();
				
			}

		}

		public void SetupAsDropdown(float maxHeight) {

			this.asDropdown = true;

			this.maxHeight = maxHeight;
			this.UpdateDropdown();

		}*/

		#if UNITY_EDITOR
		protected override void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			base.OnValidate();

			this.layoutElement = this.GetComponent<LayoutElement>();

		}
		#endif

	}

}