using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Animations {

	public class TransitionVideoInputTemplateParameters : TransitionInputTemplateParameters {
		
		[ContextMenu("Reset Material Instance")]
		public void ResetMaterialInstance() {
			
			this.GetParameters<TransitionBase.ParametersVideoBase>().ResetMaterialInstance();
			
		}

	}

}