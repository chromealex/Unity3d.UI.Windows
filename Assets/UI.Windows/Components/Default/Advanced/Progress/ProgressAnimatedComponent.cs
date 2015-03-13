using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class ProgressAnimatedComponent : ProgressComponent {

		public ProgressAnimationProcessing processing;

		#if UNITY_EDITOR
		public override void OnValidate() {

			base.OnValidate();

		}
		#endif

	}

}