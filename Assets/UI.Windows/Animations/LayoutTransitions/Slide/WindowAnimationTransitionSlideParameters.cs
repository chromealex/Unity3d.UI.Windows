using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowAnimationTransitionSlideParameters : TransitionInputParameters {

		public WindowAnimationTransitionSlide.Parameters parameters;
		
		public override void SetDefaultParameters(TransitionBase.ParametersBase parameters) {
			
			this.parameters = new WindowAnimationTransitionSlide.Parameters(parameters);
			
		}

		public override TransitionBase.ParametersBase GetParameters() {

			return this.parameters;

		}

	}

}