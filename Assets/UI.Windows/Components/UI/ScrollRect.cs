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
		public bool manualMoving = false;

		private bool isMoving = false;

		private Bounds viewBounds;
		private Bounds contentBounds;
		private Vector3[] corners;

		protected ScrollRect() : base() {

			this.corners = new Vector3[4];

		}

		public override void SetLayoutVertical() {

			base.SetLayoutVertical();

			this.viewBounds = new Bounds(this.viewRect.rect.center, this.viewRect.rect.size);
			this.contentBounds = this.GetBounds();

		}

		public override void SetLayoutHorizontal() {

			base.SetLayoutHorizontal();

			this.viewBounds = new Bounds(this.viewRect.rect.center, this.viewRect.rect.size);
			this.contentBounds = this.GetBounds();

		}

		private Bounds GetBounds() {
			
			if (this.content == null) {
				
				return default(Bounds);

			}

			Vector3 vector = new Vector3(3.402823E+38f, 3.402823E+38f, 3.402823E+38f);
			Vector3 vector2 = new Vector3(-3.402823E+38f, -3.402823E+38f, -3.402823E+38f);
			Matrix4x4 worldToLocalMatrix = this.viewRect.worldToLocalMatrix;
			this.content.GetWorldCorners(this.corners);

			for (int i = 0; i < 4; ++i) {
				
				Vector3 vector3 = worldToLocalMatrix.MultiplyPoint3x4(this.corners[i]);
				vector = Vector3.Min(vector3, vector);
				vector2 = Vector3.Max(vector3, vector2);

			}

			Bounds result = new Bounds(vector, Vector3.zero);
			result.Encapsulate(vector2);

			return result;

		}

		public bool vScrollingNeeded {
			get {
				return !Application.isPlaying || this.contentBounds.size.y > this.viewBounds.size.y + 0.01f;
			}
		}

		public bool hScrollingNeeded {
			get {
				return !Application.isPlaying || this.contentBounds.size.x > this.viewBounds.size.x + 0.01f;
			}
		}

		protected override void Start() {

			base.Start();

			this.scrollSensitivity = WindowSystemInput.GetScrollSensitivity();

		}

		public override void OnScroll(PointerEventData data) {

			base.OnScroll(data);

			this.velocity = data.delta;

		}

		public void DoBeginDrag(PointerEventData eventData) {

			if (this.manualMoving == true) {

				base.OnBeginDrag(eventData);

				this.onBeginDragEvent.Invoke();

			}

		}

		public void DoDrag(PointerEventData eventData) {

			if (this.manualMoving == true) {

				base.OnDrag(eventData);

			}

		}

		public void DoEndDrag(PointerEventData eventData) {

			if (this.manualMoving == true) {

				base.OnEndDrag(eventData);

				this.onEndDragEvent.Invoke();

			}

		}

		public override void OnBeginDrag(PointerEventData eventData) {

			if (this.manualMoving == false) {

				base.OnBeginDrag(eventData);

				this.onBeginDragEvent.Invoke();

			}

		}

		public override void OnDrag(PointerEventData eventData) {

			if (this.manualMoving == false) {

				base.OnDrag(eventData);

			}

		}

		public override void OnEndDrag(PointerEventData eventData) {

			if (this.manualMoving == false) {
				
				base.OnEndDrag(eventData);

				this.onEndDragEvent.Invoke();

			}

		}

		public Vector2 GetSize() {

			if (this.content != null) {

				return this.content.rect.size;

			}

			return Vector2.zero;

		}

		public void MoveTo(Vector2 position) {

			this.content.anchoredPosition = position;

		}

		public void ScrollReposition(RectTransform obj) {

			var padding = new Rect(0f, 0f, 0f, 0f);
			var objPosition = (Vector2)this.transform.InverseTransformPoint(obj.position);
			var scrollHeight = this.content.rect.height;//this.GetComponent<RectTransform>().rect.height;
			var objHeight = obj.rect.height;
			var contentPanel = this.content;

			if (objPosition.y > scrollHeight * 0.5f) {
				
				contentPanel.localPosition = new Vector2(contentPanel.localPosition.x, contentPanel.localPosition.y - objHeight - padding.yMin);
				
			}

			if (objPosition.y < -scrollHeight * 0.5f) {
				
				contentPanel.localPosition = new Vector2(contentPanel.localPosition.x, contentPanel.localPosition.y + objHeight + padding.yMax);
				
			}

		}

		public void Move(Vector2 delta) {
			
			var size = this.GetSize();
			var pos = this.normalizedPosition;
			pos.Scale(size);

			pos += delta;

			this.normalizedPosition = new Vector2(pos.x / size.x, pos.y / size.y);

		}

		protected override void LateUpdate() {

			if (this.transform.lossyScale == Vector3.zero) return;

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