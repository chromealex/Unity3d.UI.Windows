using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Extensions {

	[ExecuteInEditMode()]
	public class Slider : UnityEngine.UI.Slider {
		
		public bool continuous = false;

		public bool canReceiveEvents = false;

		public RectTransform minMaxHandleRect;
		[Range(0f, 1f)]
		public float _minMaxValue = 0f;
		public float minMaxValue {

			get {
				
				return this._minMaxValue;
				
			}

			set {
				
				this._minMaxValue = Mathf.Clamp01(value);
				
			}

		}

		public RectTransform fillContainerRect;

		#region IF NOT INTERACTABLE
		[Range(0f, 1f)]
		public float continuousWidth = 0.4f;
		public float continuousAngleStep = 0f;
		#endregion

		public Image fillImage;

		private bool currentActiveMaxThumb = true;

		protected virtual void LateUpdate() {

			if (this.continuous == true) {
				
				this.UpdateValues();
				
			}

		}

		public bool IsFilled() {

			return this.fillImage != null && this.fillImage.type == Image.Type.Filled && (this.fillImage.fillMethod != Image.FillMethod.Horizontal && this.fillImage.fillMethod != Image.FillMethod.Vertical);
			
		}

		private void UpdateValues() {
			
			var value = this.normalizedValue;

			if (this.canReceiveEvents == false && this.IsFilled() == true) {

				if (this.continuousAngleStep > 0f) {

					var angle = Mathf.Round((360f * value) / this.continuousAngleStep) * this.continuousAngleStep;

					this.fillRect.localRotation = Quaternion.AngleAxis(angle, Vector3.back);

				} else {

					this.fillRect.localRotation = Quaternion.AngleAxis(360f * value, Vector3.back);

				}

				this.normalizedValue = this.continuousWidth;

			} else {

				var min = Vector2.zero;
				var max = Vector2.zero;

				if (this.canReceiveEvents == false) {

					value = Mathf.Lerp(0f, 1f - this.continuousWidth, value);
					
					this.SetValues(value, ref min, ref max, this.continuousWidth);
					
					this.fillRect.anchorMin = min;
					this.fillRect.anchorMax = max;

				} else {

					var cWidth = this.continuousWidth;
					this.minMaxValue = this.minMaxValue;

					this.normalizedValue = this.minMaxValue;
					var valueMin = this.normalizedValue;
					this.normalizedValue = value;
					var valueMax = value;

					if (valueMin >= valueMax - cWidth) valueMin = valueMax - cWidth;
					if (valueMax < valueMin + cWidth) valueMax = valueMin + cWidth;

					this.minMaxValue = valueMin;
					this.normalizedValue = valueMax;

					this.SetValues(valueMin, ref min, ref max, 0f);
					
					if (this.minMaxHandleRect != null) {
						
						this.minMaxHandleRect.anchorMin = min;
						this.minMaxHandleRect.anchorMax = min;
						
					}

					this.fillRect.anchorMin = min;

					this.SetValues(valueMax, ref min, ref max, 0f);
					
					this.fillRect.anchorMax = max;

				}

			}

		}

		private float tempValue = 0f;
		public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {

			this.tempValue = this.normalizedValue;

			base.OnPointerDown(eventData);

			this.UpdateMinMax(eventData.position, eventData.pressEventCamera, true);

		}

		public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData) {
			
			base.OnDrag(eventData);
			
			this.UpdateMinMax(eventData.position, eventData.pressEventCamera, false);
			
		}

		private void UpdateMinMax(Vector2 eventDataPosition, Camera eventDataCamera, bool auto) {
			
			if (this.continuous == true && this.canReceiveEvents == true) {
				
				var valueAfter = this.normalizedValue;
				
				var d1 = 0f;
				var d2 = 0f;

				if (auto == true) {

					Vector2 offset;
					if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.fillContainerRect, eventDataPosition, eventDataCamera, out offset) == true) {
						
						var r = new Vector2(offset.x - this.fillContainerRect.rect.x, offset.y - this.fillContainerRect.rect.y);
						var normalized = new Vector2(r.x / this.fillContainerRect.rect.width, r.y / this.fillContainerRect.rect.height);
						
						if (this.direction == Direction.LeftToRight ||
						    this.direction == Direction.RightToLeft) {
							
							d1 = Mathf.Abs(this.tempValue - normalized.x);
							d2 = Mathf.Abs(this.minMaxValue - normalized.x);
							
						} else {
							
							d1 = Mathf.Abs(this.tempValue - normalized.y);
							d2 = Mathf.Abs(this.minMaxValue - normalized.y);
							
						}
						
					}

				} else {
					
					d1 = (this.currentActiveMaxThumb == false) ? 1f : 0f;
					d2 = (this.currentActiveMaxThumb == true) ? 1f : 0f;

				}

				if (d1 < d2) {
					
					// Max thumb is active
					if (auto == true) this.currentActiveMaxThumb = true;

				} else {
					
					// Min thumb is active
					if (auto == true) this.currentActiveMaxThumb = false;
					
					var cWidth = this.continuousWidth;
					var valueMin = valueAfter;
					var valueMax = this.tempValue;

					if (valueMin >= valueMax - cWidth) valueMin = valueMax - cWidth;
					if (valueMax < valueMin + cWidth) valueMax = valueMin + cWidth;

					// Return value to max thumb
					this.minMaxValue = valueMin;
					this.normalizedValue = valueMax;

				}
				
			}

		}

		private void SetValues(float value, ref Vector2 min, ref Vector2 max, float width) {
			
			var defaultMinX = 0f;
			var defaultMaxX = 1f;
			var defaultMinY = 0f;
			var defaultMaxY = 1f;

			if (this.direction == Direction.LeftToRight) {

				min.x = value;
				max.x = value + width;
				min.y = defaultMinY;
				max.y = defaultMaxY;

			} else if (this.direction == Direction.RightToLeft) {
				
				value = 1f - value;
				
				min.x = value - width;
				max.x = value;
				min.y = defaultMinY;
				max.y = defaultMaxY;
				
			} else if (this.direction == Direction.TopToBottom) {
				
				value = 1f - value;
				
				min.x = defaultMinX;
				max.x = defaultMaxX;
				min.y = value - width;
				max.y = value;
				
			} else if (this.direction == Direction.BottomToTop) {

				min.x = defaultMinX;
				max.x = defaultMaxX;
				min.y = value;
				max.y = value + width;
				
			}

		}

		#if UNITY_EDITOR
		protected override void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			base.OnValidate();

			if (this.fillRect != null) this.fillImage = this.fillRect.GetComponent<Image>();
			this.fillContainerRect = this.GetComponent<RectTransform>();

		}
		#endif

	}

}