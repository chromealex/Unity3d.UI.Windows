using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class ProgressComponent : WindowComponent {
		
		public float duration = 0f;
		public float minNormalizedValue = 0f;
		public Slider bar;

		private float currentValue = 0f;

		public override void OnInit() {

			base.OnInit();

			this.currentValue = (this.bar != null) ? this.bar.value : 0f;

		}

		public void SetValue(float value) {

			value = Mathf.Clamp01(value);

			if (this.bar != null) {

				if (this.duration > 0f) {

					TweenerGlobal.instance.removeTweens(this.bar);
					TweenerGlobal.instance.addTween(this.bar, this.duration, this.currentValue, value).onUpdate((obj, val) => {

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
			this.bar.value = value;
			this.bar.normalizedValue = Mathf.Clamp(this.bar.normalizedValue, this.minNormalizedValue, 1f);

		}

		public float GetValue() {

			return (this.bar != null) ? this.bar.value : 0f;

		}

		#if UNITY_EDITOR
		public override void OnValidate() {

			base.OnValidate();

			ME.EditorUtilities.FindReference<Slider>(this, ref this.bar);

			if (this.bar != null) {
				
				this.bar.minValue = 0f;
				this.bar.maxValue = 1f;
				this.bar.wholeNumbers = false;
				
			}

		}
		#endif

	}

}