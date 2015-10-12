using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowAudioTransitionFadeParameters : TransitionAudioInputTemplateParameters {
		
		[Header("Parameters")]
		[ReadOnly(fieldName: "useDefault", state: true)]
		public WindowAudioTransitionFade.Parameters parameters;
		
		public override void SetDefaultParameters(TransitionBase.ParametersBase parameters) {
			
			this.parameters = new WindowAudioTransitionFade.Parameters(parameters);
			
		}

		public override TransitionBase.ParametersBase GetParameters() {

			return this.parameters;

		}

	}

}