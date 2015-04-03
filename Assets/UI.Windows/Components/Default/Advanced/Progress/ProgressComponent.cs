using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ProgressComponent : ColoredComponent {
		
		[Header("Base")]
		public float duration = 0f;
		public float minNormalizedValue = 0f;
		public Extensions.Slider bar;
		
		[Header("Continious")]
		public bool continious;
		[Range(0f, 1f)]
		public float continiousWidth = 0.4f;
		public float continiousAngleStep = 0f;

		private ComponentEvent<float> callback = new ComponentEvent<float>();
		private ComponentEvent<ProgressComponent, float> callbackButton = new ComponentEvent<ProgressComponent, float>();
		private float currentValue = 0f;

		public override void OnInit() {

			base.OnInit();

			this.currentValue = (this.bar != null) ? this.bar.value : 0f;
			this.bar.continuousAngleStep = this.continiousAngleStep;

			if (this.continious == false) {

				this.SetAsDefault();

			} else {

				this.SetAsContinuous(this.continiousWidth);

			}
			
			this.bar.onValueChanged.RemoveListener(this.OnValueChanged_INTERNAL);
			this.bar.onValueChanged.AddListener(this.OnValueChanged_INTERNAL);

			this.bar.value = 0f;

		}

		public override void OnHideBegin(System.Action callback) {

			base.OnHideBegin(callback);

			this.getValue = null;

		}
		
		public virtual void SetCallback(UnityAction<float> callback) {

			this.callback.AddListenerDistinct(callback);
			this.callbackButton.RemoveAllListeners();

		}
		
		public virtual void SetCallback(UnityAction<ProgressComponent, float> callback) {
			
			this.callbackButton.AddListenerDistinct(callback);
			this.callback.RemoveAllListeners();

		}

		private void OnValueChanged_INTERNAL(float value) {
			
			this.currentValue = value;

			if (this.callback != null) this.callback.Invoke(this.currentValue);
			if (this.callbackButton != null) this.callbackButton.Invoke(this, this.currentValue);

		}

		public void SetStep(float minValue, float maxValue, bool roundToInt) {

			this.bar.minValue = minValue;
			this.bar.maxValue = maxValue;
			this.bar.wholeNumbers = roundToInt;

		}

		public void SetAsContinuous(float width = 0.4f) {
			
			this.bar.continuous = true;
			this.bar.continuousWidth = width;

			this.SetAnimation();

		}

		public void SetAsDefault() {

			this.bar.continuous = false;

			this.BreakAnimation();

		}

		public void BreakAnimation() {

			TweenerGlobal.instance.removeTweens(this);

		}

		public void SetAnimation() {

			if (this.continious == true && this.bar.canReceiveEvents == false) {

				TweenerGlobal.instance.removeTweens(this);

				if (this.bar.IsFilled() == true) {
					
					TweenerGlobal.instance.addTween(this, this.duration, 0f, 1f).tag(this).ease(ME.Ease.Linear).repeat().onUpdate((obj, value) => {
						
						if (obj != null) {
							
							obj.SetValue(value, immediately: true);
							
						}
						
					});

				} else {

					TweenerGlobal.instance.addTween(this, this.duration, 0f, 1f).tag(this).ease(ME.Ease.InOutElastic).reflect().repeat().onUpdate((obj, value) => {

						if (obj != null) {

							obj.SetValue(value, immediately: true);

						}

					});

				}

			}

			if (this.bar != null && this.bar.continuous == true) {

				var delta = Time.unscaledDeltaTime;

				var value = this.bar.normalizedValue;
				value += delta;


			}

		}

		private System.Func<float> getValue;
		private bool getValueImmediately;
		public void SetValue(System.Func<float> getValue, bool immediately = false) {

			this.getValue = getValue;
			this.getValueImmediately = immediately;

		}

		public virtual void LateUpdate() {

			if (this.getValue != null) {

				this.SetValue(this.getValue(), this.getValueImmediately);

			}

		}

		public void SetValue(float value, bool immediately = false) {

			if (this.continious == true && immediately == false) return;

			value = Mathf.Clamp01(value);

			if (this.bar != null) {

				if (immediately == false && this.duration > 0f) {

					var currentValueNormalized = this.bar.normalizedValue;

					TweenerGlobal.instance.removeTweens(this.bar);
					TweenerGlobal.instance.addTween(this.bar, this.duration, currentValueNormalized, value).onUpdate((obj, val) => {

						if (obj != null) {

							this.SetValue_INTERNAL(val);

						}

					}).tag(this.bar);

				} else {

					this.SetValue_INTERNAL(value);

				}

			}

		}

		private void SetValue_INTERNAL(float value) {
			
			this.currentValue = value;
			this.bar.normalizedValue = value;

			if (this.continious == false) {

				this.bar.normalizedValue = Mathf.Clamp(this.bar.normalizedValue, this.minNormalizedValue, 1f);

			}

		}

		public float GetValue() {

			return (this.bar != null) ? this.bar.value : 0f;

		}

		public float GetValueNormalized() {
			
			return (this.bar != null) ? this.bar.normalizedValue : 0f;

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			ME.Utilities.FindReference<Extensions.Slider>(this, ref this.bar);

		}
		#endif

	}

}