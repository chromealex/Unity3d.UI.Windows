using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Animations {

	public class TransitionInputParameters : MonoBehaviour {

		public bool useDefault = false;

		public virtual void SetDefaultParameters(TransitionBase.ParametersBase parameters) {}
		
		public virtual T GetParameters<T>() where T : TransitionBase.ParametersBase {
			
			return this.GetParameters() as T;
			
		}
		
		public virtual TransitionBase.ParametersBase GetParameters() {
			
			return null;
			
		}

	}

}