using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowAnimationTransitionAlpha : TransitionBase {
		
		[System.Serializable]
		public class Parameters : TransitionBase.ParametersBase {
			
			public Parameters(TransitionBase.ParametersBase baseDefaults) : base(baseDefaults) {}
			
			[System.Serializable]
			public class State {

				public float to;
				
			}
			
			public State resetState;
			public State inState;
			public State outState;

			public override void Setup(TransitionBase.ParametersBase defaults) {

				var param = defaults as Parameters;
				if (param == null) return;

				// Place params installation here
				this.inState = param.inState;
				this.outState = param.outState;
				this.resetState = param.resetState;

			}
			
			public float GetIn() {
				
				return this.inState.to;
				
			}
			
			public float GetOut() {
				
				return this.outState.to;
				
			}

			public float GetReset() {

				return this.resetState.to;
				
			}

			public float GetResult(bool forward) {

				if (forward == true) {

					return this.inState.to;

				}

				return this.outState.to;

			}

		}

		public Parameters defaultInputParams;

		public override TransitionBase.ParametersBase GetDefaultInputParameters() {

			return this.defaultInputParams;

		}
		
		public override void OnPlay(WindowBase window, object tag, TransitionInputParameters parameters, WindowComponentBase root, bool forward, System.Action callback) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null || root == null) {

				if (callback != null) callback();
				return;

			}

			var duration = this.GetDuration(parameters, forward);
			var result = param.GetResult(forward);

			if (TweenerGlobal.instance != null) {

				TweenerGlobal.instance.removeTweens(tag);
				TweenerGlobal.instance.addTweenAlpha(root.canvas, duration, result).onComplete((obj) => { if (callback != null) callback(); }).onCancel((obj) => { if (callback != null) callback(); }).tag(tag);

			} else {

				if (root.canvas != null) root.canvas.alpha = result;
				if (callback != null) callback();

			}

		}

		public override void SetInState(TransitionInputParameters parameters, WindowComponentBase root) {
			
			base.SetInState(parameters, root);

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			if (root != null && root.canvas != null) root.canvas.alpha = param.GetIn();
			
		}
		
		public override void SetOutState(TransitionInputParameters parameters, WindowComponentBase root) {
			
			base.SetOutState(parameters, root);

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			if (root != null && root.canvas != null) root.canvas.alpha = param.GetOut();

		}
		
		public override void SetResetState(TransitionInputParameters parameters, WindowComponentBase root) {

			base.SetResetState(parameters, root);

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			if (root != null && root.canvas != null) root.canvas.alpha = param.GetReset();
			
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Transitions/Alpha")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowAnimationTransitionAlpha>();
			
		}
		#endif

	}

}