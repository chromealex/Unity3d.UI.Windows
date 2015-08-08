using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowAnimationTransitionCameraSlideParameters : TransitionInputTemplateParameters {
		
		[Header("Parameters")]
		[ReadOnly(fieldName: "useDefault", state: false)]
		public WindowAnimationTransitionCameraSlide.Parameters parameters;
		
		public override void SetDefaultParameters(TransitionBase.ParametersBase parameters) {
			
			this.parameters = new WindowAnimationTransitionCameraSlide.Parameters(parameters);
			
		}

		public override TransitionBase.ParametersBase GetParameters() {

			return this.parameters;

		}

	}

}