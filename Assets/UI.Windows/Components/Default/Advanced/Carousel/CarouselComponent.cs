using UnityEngine;
using UnityEngine.UI.Windows;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components {

	public interface ICarouselItem : IComponent {

		void OnSelect(int index, System.Action<int> callback);
		void OnElementSelected(int index);
		void OnElementDeselected(int index);

	};

	public class CarouselComponent : ListComponent, ILayoutSelfController, ILayoutGroup, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler {

		public enum Axis : byte {
			Horizontal,
			Vertical
		};

		GridLayoutGroup f;
		public RectTransform contentRect;
		public float value = 0f;

		[Header("Parameters")]
		public Axis axis;
		public bool cyclical = false;
		public float maxVisualCount = 1f;
		public float maxAngle = 0f;
		public Vector3 rotationAxis = new Vector3(1f, 1f, -0.5f);

		[Header("Transitions")]
		public ME.Ease.Type easePosition = ME.Ease.Type.InOutQuad;
		public ME.Ease.Type easeRotation = ME.Ease.Type.InOutQuad;
		public ME.Ease.Type easeAlpha = ME.Ease.Type.InOutQuad;
		public ME.Ease.Type easeScale = ME.Ease.Type.InOutQuad;

		[Header("Movement")]
		public float scrollSpeed = 3f;
		public float movementSpeed = 1f;
		public float dragSpeed = 3f;
		public float slowdownSpeed = 10f;

		private int lastCurrentIndex = -1;
		private bool isDragging;
		private float targetValue;
		private float visualCount;
		private float lastValue = -1f;

		public override void OnNewItem(WindowComponent instance) {

			base.OnNewItem(instance);

			var carouselItem = (instance as ICarouselItem);
			if (carouselItem != null) {

				var index = this.GetIndexOf(instance);
				carouselItem.OnSelect(index, this.MoveTo);

			}

			this.ArrangeItems();

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

		public override void OnShowEnd() {

			base.OnShowEnd();

			this.ArrangeItems();

		}

		public override void OnShowBegin() {

			base.OnShowBegin();

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

				elementRect.localScale = Vector3.zero;
				elementRect.gameObject.SetActive(false);
				return false;

			}

			elementRect.gameObject.SetActive(true);

			var s = normalizedValue - 0.5f;
			var abs = Mathf.Abs(s) * 2f;
			var sign = Mathf.Sign(s);

			var easePosition = ME.Ease.GetByType(this.easePosition);
			var easeRotation = ME.Ease.GetByType(this.easeRotation);
			var easeAlpha = ME.Ease.GetByType(this.easeAlpha);
			var easeScale = ME.Ease.GetByType(this.easeScale);

			var normalizedValuePosition = sign * easePosition.interpolate(0f, 1f, abs, 1f);
			var pos = this.GetSize() * 0.5f * normalizedValuePosition;

			var normalizedValueAlpha = easeAlpha.interpolate(0f, 1f, abs, 1f);
			var alpha = 1f - normalizedValueAlpha;

			var normalizedValueScale = easeScale.interpolate(0f, 1f, abs, 1f);
			var scale = 1f - normalizedValueScale;

			var normalizedValueRotation = sign * easeRotation.interpolate(0f, 1f, abs, 1f);
			var rotation = normalizedValueRotation;

			elementRect.localScale = Vector3.one * scale;
			elementRect.anchoredPosition3D = this.GetAxisVector3(pos);
			elementRect.localRotation = Quaternion.AngleAxis(rotation * this.maxAngle, this.rotationAxis);

			var item = (component as IAlphaComponent);
			if (item != null) {

				item.SetAlpha(alpha);

			}

			var carouselItem = (component as ICarouselItem);
			if (carouselItem != null) {

				if (index == currentIndex && this.lastCurrentIndex != currentIndex) {

					if (this.lastCurrentIndex >= 0) {

						(this.list[this.lastCurrentIndex] as ICarouselItem).OnElementDeselected(this.lastCurrentIndex);

					}

					carouselItem.OnElementSelected(index);
					this.lastCurrentIndex = currentIndex;

				}

			}

			if (rebuildState == false) elementRect.SetAsLastSibling();

			return true;

		}

		public void SetLayoutHorizontal() {

			this.ArrangeItems(rebuildState: false);

		}

		public void SetLayoutVertical() {

			this.ArrangeItems(rebuildState: false);

		}

		#region Drag Handlers
		public void OnBeginDrag(PointerEventData eventData) {

			this.isDragging = true;

		}

		public void OnDrag(PointerEventData eventData) {

			if (this.isDragging == true) {
				
				var windowDelta = this.GetAxisValue(WindowSystem.ConvertPointScreenToWindow(eventData.delta, this));
				this.Move(-windowDelta / this.GetSize() * this.dragSpeed, immediately: true);

			}

		}

		public void OnEndDrag(PointerEventData eventData) {
			
			var windowDelta = this.GetAxisValue(WindowSystem.ConvertPointScreenToWindow(eventData.delta, this));
			this.Move(-windowDelta / this.GetSize() * this.dragSpeed, immediately: false);
			this.targetValue = Mathf.RoundToInt(this.targetValue);
			this.isDragging = false;

		}

		public void OnScroll(PointerEventData eventData) {

			this.Move(eventData.scrollDelta.y * this.scrollSpeed, minValue: 1f);

		}
		#endregion

		public void Move(float delta, bool immediately = false, float minValue = 0f) {

			var value = delta * this.movementSpeed;
			if (Mathf.Abs(value) <= minValue) {

				value = Mathf.Sign(value) * minValue;

			}

			if (immediately == true) {

				this.targetValue += value;

				if (this.cyclical == false) {
					
					this.targetValue = Mathf.Clamp(this.targetValue, 0f, this.list.Count - 1);

				}

				this.value = this.targetValue;
				this.ArrangeItems();

			} else {

				this.targetValue += value;

				if (this.cyclical == false) {
					
					this.targetValue = Mathf.Clamp(this.targetValue, 0f, this.list.Count - 1);

				}

			}

		}

		public void MovePrev() {

			--this.targetValue;

		}

		public void MoveNext() {

			++this.targetValue;

		}

		public void MoveTo(int index) {

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