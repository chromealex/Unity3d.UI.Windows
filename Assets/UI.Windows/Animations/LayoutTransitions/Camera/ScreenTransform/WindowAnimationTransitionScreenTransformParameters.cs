using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowAnimationTransitionScreenTransformParameters : TransitionVideoInputTemplateParameters {
		
		[Header("Parameters")]
		[ReadOnly(fieldName: "useDefault", state: true)]
		public WindowAnimationTransitionScreenTransform.Parameters parameters;
		
		public override void SetDefaultParameters(TransitionBase.ParametersBase parameters) {
			
			this.parameters = new WindowAnimationTransitionScreenTransform.Parameters(parameters);
			
		}

		public override TransitionBase.ParametersBase GetParameters() {

			return this.parameters;

		}

	}

}