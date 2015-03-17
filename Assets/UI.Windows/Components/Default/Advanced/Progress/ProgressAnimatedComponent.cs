using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class ProgressAnimatedComponent : ProgressComponent {

		public float tileDuration = 2f;

		public ProgressAnimationProcessing processing;

		#if UNITY_EDITOR
		public override void OnValidate() {

			base.OnValidate();

			if (this.bar != null) {

				this.processing.direction = this.bar.direction;
				this.processing.duration = this.tileDuration;

			}

		}
		#endif

	}

}