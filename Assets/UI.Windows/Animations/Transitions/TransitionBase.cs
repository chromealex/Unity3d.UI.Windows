using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public class TransitionBase : ScriptableObject {

		[System.Serializable]
		public class ParametersBase {
			
			public float inDelay;
			public float outDelay;

			public float inDuration;
			public float outDuration;

			public ParametersBase() {}

			public ParametersBase(ParametersBase defaults) {
				
				this.inDuration = defaults.inDuration;
				this.outDuration = defaults.outDuration;
				
				this.inDelay = defaults.inDelay;
				this.outDelay = defaults.outDelay;

				this.Setup(defaults);

			}
			
			public virtual void Setup(ParametersBase defaults) {

			}

		}

		public virtual ParametersBase GetDefaultInputParameters() {

			return null;

		}

		public virtual void SetInState(TransitionInputParameters parameters, WindowLayoutBase root) {

		}
		
		public virtual void SetOutState(TransitionInputParameters parameters, WindowLayoutBase root) {

		}
		
		public virtual void SetResetState(TransitionInputParameters parameters, WindowLayoutBase root) {

		}

		public void Play(TransitionInputParameters parameters, WindowLayoutBase root, bool forward, System.Action callback) {

			var delay = this.GetDelay(parameters, forward);
			var tag = this.GetInstanceID().ToString() + "_" + root.GetInstanceID().ToString();

			if (delay > 0f) {

				TweenerGlobal.instance.removeTweens(tag);
				TweenerGlobal.instance.addTween(this, delay, 0f, 0f).tag(tag).onComplete(() => {

					this.OnPlay(tag, parameters, root, forward, callback);

				});

			} else {

				this.OnPlay(tag, parameters, root, forward, callback);

			}

		}
		
		public virtual void OnPlay(object tag, TransitionInputParameters parameters, WindowLayoutBase root, bool forward, System.Action callback) {}

		public virtual float GetDelay(TransitionInputParameters parameters, bool forward) {
			
			var param = this.GetParams<ParametersBase>(parameters);
			
			if (forward == true) {
				
				return param.inDelay;
				
			}
			
			return param.outDelay;
			
		}
		
		public virtual float GetDuration(TransitionInputParameters parameters, bool forward) {
			
			var param = this.GetParams<ParametersBase>(parameters);
			
			if (forward == true) {
				
				return param.inDuration;
				
			}
			
			return param.outDuration;
			
		}

		public T GetParams<T>(TransitionInputParameters parameters) where T : ParametersBase {
			
			T param = null;
			if (parameters.useDefault == true) {
				
				param = this.GetDefaultInputParameters() as T;
				
			} else {
				
				param = parameters.GetParameters() as T;
				
			}
			
			return param;
			
		}

	}

}