using UnityEngine;
using UnityEngine.UI.Windows;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components {

	public interface ICarouselItemCanvas : IComponent {

		void SetSiblingIndex(int index);

	}

	public interface ICarouselItemAlpha : IComponent {

		void SetCarouselAlpha(float value);

	};

	public interface ICarouselItemSelectable : IComponent {

		void OnElementSelected(int index);
		void OnElementDeselected(int index);

	};

	public interface ICarouselItem : ICarouselItemSelectable, ICarouselItemAlpha, IComponent {

		void OnSelect(int index, System.Action<int> callback);

	};

	public class CarouselComponent : ListComponent, ILayoutSelfController, ILayoutGroup, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler, ICanvasElement {

		public enum SwitchBehaviour : byte {
			HideContent,
			ScaleContent,
		};

		public enum Axis : byte {
			Horizontal,
			Vertical
		};

		public enum EaseFlags : byte {
			None = 0x0,
			Position = 0x1,
			Rotation = 0x2,
			Alpha = 0x4,
			Scale = 0x8,
		};

		public bool interactable = true;

		public RectTransform contentRect;
		public float value = 0f;

		[Header("Modules")]
		public LinkerComponent dotsLinker;
		public DotsComponent dots;

		public LinkerComponent arrowsLinker;
		public ArrowsComponent arrows;

		[Header("Parameters")]
		public SwitchBehaviour switchBehaviour = SwitchBehaviour.ScaleContent;
		public Axis axis;
		public bool cyclical = false;
		public bool draggable = true;
		public bool scrollable = true;
		public float maxVisualCount = 1f;
		public float maxAngle = 0f;
		public Vector3 rotationAxis = new Vector3(1f, 1f, -0.5f);
		public float positionScale = 0.5f;

		[Header("Easings")]
		[BitMask(typeof(EaseFlags))]
		public EaseFlags easeFlags = EaseFlags.Position | EaseFlags.Rotation | EaseFlags.Alpha | EaseFlags.Scale;
		public ME.Ease.Type easePosition = ME.Ease.Type.InOutQuad;
		public ME.Ease.Type easeRotation = ME.Ease.Type.InOutQuad;
		public ME.Ease.Type easeAlpha = ME.Ease.Type.InOutQuad;
		public ME.Ease.Type easeScale = ME.Ease.Type.InOutQuad;

		[Header("Movement")]
		public float scrollSpeed = 3f;
		public float movementSpeed = 1f;
		public float dragSpeed = 3f;
		public float slowdownSpeed = 10f;
		public float movementTime = 0.3f;

		public System.Action<int, WindowComponent> onItemSelected;
		public System.Action<int, WindowComponent> onItemDeselected;

		private int lastCurrentIndex = -1;
		private bool isDragging;
		private float dragStartTime = 0f;
		private float lastDragSide = 0f;

		private float _targetValue;
		private float targetValue {

			set {

				if (this._targetValue != value) {
					
					this._targetValue = value;
					this.Clamp();

				}

			}

			get {

				return this._targetValue;

			}

		}

		private float visualCount;
		private float lastValue = -1f;
		private System.Action<int> onSelect;

		#region Rebuild
		Transform ICanvasElement.transform {
			get { return base.transform; }
		}

		void ICanvasElement.GraphicUpdateComplete() {
		}

		bool ICanvasElement.IsDestroyed() {

			return this == null;

		}

		void ICanvasElement.LayoutComplete() {

			this.ArrangeItems(rebuildState: false);

		}

		void ICanvasElement.Rebuild(CanvasUpdate executing) {

			var rebuildState = (executing == CanvasUpdate.Layout);
			this.ArrangeItems(rebuildState);

		}

		public void SetLayoutHorizontal() {

			this.ArrangeItems(rebuildState: false);

		}

		public void SetLayoutVertical() {

			this.ArrangeItems(rebuildState: false);

		}
		#endregion

		public void SetDisabled() {

			this.interactable = false;

		}

		public void SetEnabled() {

			this.interactable = true;

		}

		public void SetCallback(System.Action<int> onSelect) {

			this.onSelect = onSelect;

		}

		public override void BeginLoad() {

			base.BeginLoad();

			if (this.dots != null) this.dots.Hide(AppearanceParameters.Default().ReplaceImmediately(immediately: true).ReplaceForced(forced: true).ReplaceResetAnimation(resetAnimation: true));
			if (this.arrows != null) this.arrows.Hide(AppearanceParameters.Default().ReplaceImmediately(immediately: true).ReplaceForced(forced: true).ReplaceResetAnimation(resetAnimation: true));

		}

		public override void EndLoad() {

			base.EndLoad();

			if (this.dots != null) this.dots.Show(AppearanceParameters.Default().ReplaceForced(forced: true).ReplaceResetAnimation(resetAnimation: true));
			if (this.arrows != null) this.arrows.Show(AppearanceParameters.Default().ReplaceForced(forced: true).ReplaceResetAnimation(resetAnimation: true));

		}

		public override void OnNewItem(WindowComponent instance) {

			base.OnNewItem(instance);

			var carouselItem = (instance as ICarouselItem);
			if (carouselItem != null) {

				var index = this.GetIndexOf(instance);
				carouselItem.OnSelect(index, this.MoveTo);

			}

			if (this.dots != null) this.dots.Refresh(this);
			if (this.arrows != null) this.arrows.Refresh(this);

			this.ArrangeItems();

		}

		public override void RemoveItem(WindowComponent instance) {

			base.RemoveItem(instance);

			if (this.lastCurrentIndex >= this.list.Count) {

				this.MoveTo(this.lastCurrentIndex - 1);

			}

			this.lastCurrentIndex = Mathf.Clamp(this.lastCurrentIndex, 0, this.list.Count - 1);

			if (this.dots != null) this.dots.Refresh(this);
			if (this.arrows != null) this.arrows.Refresh(this);

			this.ArrangeItems();

		}

		public override bool IsNavigationPreventChildEvents(NavigationSide side) {
			/*
			if (this.axis == Axis.Horizontal) {

				if (side == NavigationSide.Left ||
					side == NavigationSide.Right) return true;

			} else  if (this.axis == Axis.Vertical) {

				if (side == NavigationSide.Up ||
					side == NavigationSide.Down) return true;
				
			}*/

			return false;

		}

		public override bool IsNavigationControlledSide(NavigationSide side) {

			if (this.axis == Axis.Horizontal) {

				if (side == NavigationSide.Left ||
					side == NavigationSide.Right) return true;
				
			} else  if (this.axis == Axis.Vertical) {

				if (side == NavigationSide.Up ||
					side == NavigationSide.Down) return true;
				
			}

			return false;

		}

		public override void OnNavigateUp() {

			base.OnNavigateUp();

			if (this.axis == Axis.Vertical) {

				this.MovePrev();

			}

		}

		public override void OnNavigateDown() {

			base.OnNavigateDown();

			if (this.axis == Axis.Vertical) {

				this.MoveNext();

			}

		}

		public override void OnNavigateLeft() {

			base.OnNavigateLeft();

			if (this.axis == Axis.Horizontal) {

				this.MovePrev();

			}

		}

		public override void OnNavigateRight() {

			base.OnNavigateRight();

			if (this.axis == Axis.Horizontal) {

				this.MoveNext();

			}

		}

		public override void OnNavigate(WindowComponentNavigation source, NavigationSide side) {
			
		}

		public override void Clear() {

			base.Clear();

			this.Reset();

		}

		public void Reset() {

			this.value = 0f;
			this.targetValue = 0f;
			this.isDragging = false;
			this.visualCount = 0f;
			this.lastCurrentIndex = -1;
			this.lastValue = -1f;

		}

		public override void OnInit() {

			base.OnInit();

			if (this.dotsLinker != null) this.dotsLinker.Get(ref this.dots);
			if (this.dots != null) this.dots.showOnStart = false;

			if (this.arrowsLinker != null) this.arrowsLinker.Get(ref this.arrows);
			if (this.arrows != null) this.arrows.showOnStart = false;

		}

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.onSelect = null;

		}

		public override void OnShowBegin() {

			base.OnShowBegin();

			this.ArrangeItems();

		}

		public override void OnShowEnd() {

			base.OnShowEnd();

			this.ArrangeItems();

		}

		public void ArrangeItems(bool rebuildState = false) {

			var count = this.list.Count;

			var currentIndex = -1;
			if (this.cyclical == true) {
				
				currentIndex = Mathf.RoundToInt(Mathf.Repeat(Mathf.Round(this.value), count));

			} else {

				currentIndex = Mathf.Clamp(Mathf.RoundToInt(this.value), 0, count);

			}

			this.visualCount = (this.maxVisualCount > count) ? count : this.maxVisualCount;

			if (this.visualCount < 1f) {

				this.visualCount = 1f;

			}

			if (count > 0) {
				
				var ch = Mathf.FloorToInt(count * 0.5f);
				for (int i = 0; i < ch; ++i) {

					var index = Mathf.RoundToInt(Mathf.Repeat(currentIndex - ch + i, count));
					this.CalculateElement(index, currentIndex, rebuildState);

				}

				for (int i = count - 1; i >= ch; --i) {

					var index = Mathf.RoundToInt(Mathf.Repeat(currentIndex - ch + i, count));
					this.CalculateElement(index, currentIndex, rebuildState);

				}

			}

		}

		private bool CalculateElement(int index, int currentIndex, bool rebuildState) {
			
			var component = this.list[index];
			var elementRect = (component.transform as RectTransform);

			var normalizedValue = 0f;
			if (this.cyclical == true) {

				normalizedValue = Mathf.Clamp01(Mathf.Repeat(0.5f + (index - this.value) / this.visualCount, this.list.Count / this.visualCount));

			} else {

				normalizedValue = Mathf.Clamp01(0.5f + (index - this.value) / this.visualCount);

			}

			if (normalizedValue <= 0f || normalizedValue >= 1f) {

				this.SetActive(elementRect, component, false, rebuildState);
				return false;

			}

			this.SetActive(elementRect, component, true, rebuildState);

			var s = normalizedValue - 0.5f;
			var abs = Mathf.Abs(s) * 2f;
			var sign = Mathf.Sign(s);

			var noInterpolationValue = 0f;

			var normalizedValuePosition = sign * ((this.easeFlags & EaseFlags.Position) != 0 ? ME.Ease.GetByType(this.easePosition).interpolate(0f, 1f, abs, 1f) : noInterpolationValue);
			var pos = this.GetSize() * this.positionScale * normalizedValuePosition;

			var normalizedValueAlpha = ((this.easeFlags & EaseFlags.Alpha) != 0 ? ME.Ease.GetByType(this.easeAlpha).interpolate(0f, 1f, abs, 1f) : noInterpolationValue);
			var alpha = 1f - normalizedValueAlpha;

			var normalizedValueScale = ((this.easeFlags & EaseFlags.Scale) != 0 ? ME.Ease.GetByType(this.easeScale).interpolate(0f, 1f, abs, 1f) : noInterpolationValue);
			var scale = 1f - normalizedValueScale;

			var normalizedValueRotation = sign * ((this.easeFlags & EaseFlags.Rotation) != 0 ? ME.Ease.GetByType(this.easeRotation).interpolate(0f, 1f, abs, 1f) : noInterpolationValue);
			var rotation = normalizedValueRotation;

			elementRect.localScale = Vector3.one * scale;
			elementRect.anchoredPosition3D = this.GetAxisVector3(pos);
			elementRect.localRotation = Quaternion.AngleAxis(rotation * this.maxAngle, this.rotationAxis);

			var eventComponent = this.GetEventComponent(index);
			var carouselItemAlpha = eventComponent as ICarouselItemAlpha;
			if (carouselItemAlpha != null) {

				carouselItemAlpha.SetCarouselAlpha(alpha);

			}

			if (index == currentIndex && this.lastCurrentIndex != currentIndex) {

				if (this.lastCurrentIndex >= 0) {

					var prevItem = this.list[this.lastCurrentIndex] as ICarouselItemSelectable;
					if (prevItem != null) {

						prevItem.OnElementDeselected(this.lastCurrentIndex);

					}

					if (this.onItemDeselected != null) this.onItemDeselected.Invoke(this.lastCurrentIndex, this.list[this.lastCurrentIndex]);

				}

				var carouselItem = eventComponent as ICarouselItemSelectable;
				if (carouselItem != null) {

					carouselItem.OnElementSelected(index);

				}

				if (this.onItemSelected != null) this.onItemSelected.Invoke(index, eventComponent);

				if (this.onSelect != null) this.onSelect.Invoke(index);
				if (this.dots != null) this.dots.OnSelect(index);
				if (this.arrows != null) this.arrows.OnSelect(index);
				this.lastCurrentIndex = currentIndex;

			}

			if (rebuildState == false) {

				var carouselItemCanvas = component as ICarouselItemCanvas;
				if (carouselItemCanvas != null) {

					carouselItemCanvas.SetSiblingIndex(index != currentIndex ? -1 : 0);//currentIndex - elementRect.parent.childCount);

				} else {

					if (elementRect.GetSiblingIndex() != elementRect.parent.childCount) elementRect.SetAsLastSibling();

				}

			}

			return true;

		}

		public WindowComponent GetEventComponent(int index) {

			var component = this.list[index];
			if (component is LinkerComponent) {

				return (component as LinkerComponent).Get<WindowComponent>();

			}

			return component;

		}

		public int GetCurrentIndex() {

			return this.lastCurrentIndex;

		}

		public void SetActive(RectTransform obj, WindowComponent component, bool state, bool rebuildState) {

			if (rebuildState == true) return;

			if (state == true) {

				if (this.switchBehaviour == SwitchBehaviour.HideContent) {

					component.Show(AppearanceParameters.Default().ReplaceImmediately(immediately: true));

				} else if (this.switchBehaviour == SwitchBehaviour.ScaleContent) {

					if (obj.localScale != Vector3.one) {

						obj.localScale = Vector3.one;
						(component as IWindowEventsController).DoWindowActive();
						var canvas = obj.GetComponent<Canvas>();
						if (canvas != null) canvas.enabled = true;

					}

				}

			} else {

				if (this.switchBehaviour == SwitchBehaviour.HideContent) {

					component.Hide(AppearanceParameters.Default().ReplaceImmediately(immediately: true));

				} else if (this.switchBehaviour == SwitchBehaviour.ScaleContent) {

					if (obj.localScale != Vector3.zero) {
						
						var canvas = obj.GetComponent<Canvas>();
						if (canvas != null) canvas.enabled = false;
						(component as IWindowEventsController).DoWindowInactive();
						obj.localScale = Vector3.zero;

					}

				}

			}

		}

		#region Drag Handlers
		public void OnBeginDrag(PointerEventData eventData) {

			if (this.interactable == false) return;
			if (this.draggable == false) return;

			this.dragStartTime = Time.time;
			this.lastDragSide = 0f;
			this.isDragging = true;

		}

		public void OnDrag(PointerEventData eventData) {

			if (this.interactable == false) return;

			if (this.isDragging == true) {
				
				var windowDelta = this.GetAxisValue(WindowSystem.ConvertPointScreenToWindow(eventData.delta, this));
				this.lastDragSide = Mathf.Sign(windowDelta);
				this.Move(-windowDelta / this.GetSize() * this.dragSpeed, immediately: true);

			}

		}

		public void OnEndDrag(PointerEventData eventData) {

			if (this.interactable == false) return;

			if ((Time.time - this.dragStartTime) <= this.movementTime) {

				this.targetValue = this.RoundToInt((this.lastDragSide > 0f ? this.targetValue - 1f : this.targetValue + 1f), this.lastDragSide > 0f ? 0.8f : 0.2f);

			} else {

				this.targetValue = this.RoundToInt(this.targetValue, this.lastDragSide > 0f ? 0.8f : 0.2f);

			}

			this.isDragging = false;

		}

		public void OnScroll(PointerEventData eventData) {

			if (this.interactable == false) return;
			if (this.scrollable == false) return;

			this.Move(eventData.scrollDelta.y * this.scrollSpeed, minValue: 1f);

		}
		#endregion

		public void Move(float delta, bool immediately = false, float minValue = 0f) {

			var value = delta * this.movementSpeed;
			if (Mathf.Abs(value) <= minValue) {

				value = Mathf.Sign(value) * minValue;

			}

			this.targetValue += value;

			if (immediately == true) {

				this.value = this.targetValue;
				this.ArrangeItems();

			}

		}

		public int RoundToInt(float value, float weight) {

			var floor = Mathf.FloorToInt(value);
			var dec = value - floor;

			if (dec >= weight) {

				return Mathf.CeilToInt(value);

			} else {

				return floor;

			}

		}

		public void Clamp() {

			if (this.cyclical == false) {

				this._targetValue = Mathf.Clamp(this._targetValue, 0f, this.list.Count - 1);

			}

		}

		public void MovePrev() {

			--this.targetValue;

		}

		public void MoveNext() {

			++this.targetValue;

		}

		public void MoveTo(int index) {

			this.MoveTo(index, immediately: false);

		}

		public void MoveTo(int index, bool immediately) {

			if (this.cyclical == true) {

				var count = this.list.Count;
				var currentIndex = Mathf.RoundToInt(Mathf.Repeat(this.value, count));
				var leftDistance = Mathf.Repeat(currentIndex - index + count, count);
				var rightDistance = Mathf.Repeat(index - currentIndex + count, count);

				if (rightDistance > leftDistance) {

					this.targetValue = Mathf.RoundToInt(this.value) - leftDistance;

				} else {

					this.targetValue = Mathf.RoundToInt(this.value) + rightDistance;

				}

			} else {

				this.targetValue = index;

			}

			if (immediately == true) {

				this.value = this.targetValue;
				this.ArrangeItems();

			}

			/*if (this.value == this.targetValue) {

				this.ArrangeItems();
				var carouselItem = this.GetItem<ICarouselItem>(index);
				if (carouselItem != null) carouselItem.OnElementSelected(index);

				this.lastValue = this.value - 1f;

			}*/

		}

		public void LateUpdate() {

			if (this.isDragging == false) {

				var deltaTime = Time.unscaledDeltaTime;

				this.value = Mathf.Lerp(this.value, Mathf.RoundToInt(this.targetValue), deltaTime * this.slowdownSpeed);
				var abs = Mathf.Abs(this.value);
				var changed = (this.value != this.lastValue);

				if (abs < 0.001f) {

					if (this.value != 0f) changed = true;
					this.value = 0f;

				}

				if (changed == true) {

					this.ArrangeItems();
					this.lastValue = this.value;

				}

			}

			if (this.interactable == false) return;

			#if UNITY_STANDALONE
			if (this.axis == Axis.Horizontal) {
				
				if (Input.GetKeyDown(KeyCode.LeftArrow) == true) {

					this.MovePrev();

				}

				if (Input.GetKeyDown(KeyCode.RightArrow) == true) {

					this.MoveNext();

				}

			} else if (this.axis == Axis.Vertical) {

				if (Input.GetKeyDown(KeyCode.DownArrow) == true) {

					this.MovePrev();

				}

				if (Input.GetKeyDown(KeyCode.UpArrow) == true) {

					this.MoveNext();

				}

			}
			#endif

		}

		#region Utils
		private Vector3 GetAxisVector3(float value) {

			var result = Vector3.zero;
			switch (this.axis) {

				case Axis.Horizontal:
					result.x = value;
					break;

				case Axis.Vertical:
					result.y = value;
					break;

			}

			return result;

		}

		private float GetAxisValue(Vector2 vector) {

			var result = 0f;
			switch (this.axis) {

				case Axis.Horizontal:
					result = vector.x;
					break;

				case Axis.Vertical:
					result = vector.y;
					break;

			}

			return result;

		}

		private float GetSize() {

			var result = 0f;
			var contentRect = this.contentRect.rect;
			switch (this.axis) {

				case Axis.Horizontal:
					result = contentRect.width;
					break;

				case Axis.Vertical:
					result = contentRect.height;
					break;

			}

			return result;

		}
		#endregion

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			this.ArrangeItems();

		}
		#endif

	}

}