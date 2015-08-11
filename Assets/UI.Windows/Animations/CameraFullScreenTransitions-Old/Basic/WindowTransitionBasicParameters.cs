using UnityEngine;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowTransitionBasicParameters : TransitionInputTemplateParameters {
		
		[ReadOnly(fieldName: "useDefault", state: false)]
		public WindowTransitionBasic.Parameters parameters;
		
		public override void SetDefaultParameters(TransitionBase.ParametersBase parameters) {
			
			this.parameters = new WindowTransitionBasic.Parameters(parameters);
			
		}
		
		public override TransitionBase.ParametersBase GetParameters() {
			
			return this.parameters;
			
		}
		
	}

}