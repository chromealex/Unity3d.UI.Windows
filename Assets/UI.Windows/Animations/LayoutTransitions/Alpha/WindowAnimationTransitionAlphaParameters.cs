using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Animations {

	[RequireComponent(typeof(CanvasGroup))]
	public class WindowAnimationTransitionAlphaParameters : TransitionInputParameters {

		[ReadOnly]
		public CanvasGroup canvasGroup;

		[ReadOnly(fieldName: "useDefault", state: true)]
		public WindowAnimationTransitionAlpha.Parameters parameters;
		
		public override void SetDefaultParameters(TransitionBase.ParametersBase parameters) {
			
			this.parameters = new WindowAnimationTransitionAlpha.Parameters(parameters);
			
		}

		public override TransitionBase.ParametersBase GetParameters() {

			return this.parameters;

		}

		#if UNITY_EDITOR
		public override void OnValidate() {

			base.OnValidate();

			if (this.canvasGroup == null) this.canvasGroup = this.GetComponent<CanvasGroup>();

		}
		#endif

	}

}