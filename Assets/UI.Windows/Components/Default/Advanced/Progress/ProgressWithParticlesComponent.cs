using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class ProgressWithParticlesComponent : ProgressComponent {

		public ParticleSystemCached effect;

		public override void OnInit() {

			base.OnInit();

			if (this.bar != null) {

				this.bar.onValueChanged.RemoveListener(this.OnChanged);
				this.bar.onValueChanged.AddListener(this.OnChanged);

				this.OnChanged(this.bar.value);

			}

		}

		private void OnChanged(float value) {

			if (this.bar == null) return;

			var sx = (this.bar.direction == Slider.Direction.TopToBottom ||
			          this.bar.direction == Slider.Direction.BottomToTop) ? 0f : 1f;
			
			var sy = (this.bar.direction == Slider.Direction.LeftToRight ||
			          this.bar.direction == Slider.Direction.RightToLeft) ? 0f : 1f;

			var rect = this.bar.fillRect.rect;
			var size = new Vector2(rect.width, rect.height) + this.bar.fillRect.sizeDelta;
			size.x *= sx;
			size.y *= sy;
			if (size.x < 1f) size.x = 1f;
			if (size.y < 1f) size.y = 1f;

			if (this.effect != null) this.effect.transform.localScale = new Vector3(size.x, size.y, 1f);

		}

		public override void OnShowEnd() {

			base.OnShowEnd();

			if (this.effect != null) this.effect.Play();
			
			this.OnChanged(this.bar.value);

		}

		public override void OnHideBegin() {

			base.OnHideBegin();

			if (this.effect != null) this.effect.Stop(true, this.GetWindow().GetAnimationDuration(forward: false));
			
		}

		#if UNITY_EDITOR
		public override void OnValidate() {

			base.OnValidate();

		}
		#endif

	}

}