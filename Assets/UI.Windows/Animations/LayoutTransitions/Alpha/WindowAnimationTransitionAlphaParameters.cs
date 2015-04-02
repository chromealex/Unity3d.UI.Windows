using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Animations {

	[RequireComponent(typeof(CanvasGroup))]
	public class WindowAnimationTransitionAlphaParameters : TransitionInputParameters {

		public WindowAnimationTransitionAlpha.Parameters parameters;
		
		public override void SetDefaultParameters(TransitionBase.ParametersBase parameters) {
			
			this.parameters = new WindowAnimationTransitionAlpha.Parameters(parameters);
			
		}

		public override TransitionBase.ParametersBase GetParameters() {

			return this.parameters;

		}

	}

}