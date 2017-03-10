using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowAnimationTransitionAlpha : TransitionBase {
		
		[System.Serializable]
		public class Parameters : TransitionBase.ParametersVideoBase {

			public Parameters() : base() {}
			public Parameters(TransitionBase.ParametersBase baseDefaults) : base(baseDefaults) {}
			
			[System.Serializable]
			public class State {

				public float to;

				public State() {
				}

				public State(State state) {

					this.to = state.to;

				}

			}
			
			[Header("States")]
			public State resetState = new State();
			public State inState = new State();
			public State outState = new State();

			public override void Setup(TransitionBase.ParametersBase defaults) {

				var param = defaults as Parameters;
				if (param == null) return;

				// Place params installation here
				this.inState = new State(param.inState);
                this.outState = new State(param.outState);
                this.resetState = new State(param.resetState);

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

			var canvasGroup = this.GetCanvasGroup(parameters);

			var duration = this.GetDuration(parameters, forward);
			var result = param.GetResult(forward);

			if (TweenerGlobal.instance != null) {

				//TweenerGlobal.instance.removeTweens(tag);
				TweenerGlobal.instance.addTweenAlpha(canvasGroup, duration, result).ease(ME.Ease.GetByType(forward == true ? param.inEase : param.outEase)).onComplete((obj) => { if (callback != null) callback(); }).onCancel((obj) => { if (callback != null) callback(); }).tag(tag);

			} else {

				if (canvasGroup != null) canvasGroup.alpha = result;
				if (callback != null) callback();

			}

		}

		private CanvasGroup GetCanvasGroup(TransitionInputParameters parameters) {

			CanvasGroup canvasGroup = null;
			if (parameters is WindowAnimationTransitionAlphaParameters) {

				canvasGroup = (parameters as WindowAnimationTransitionAlphaParameters).canvasGroup;

			}

			return canvasGroup;

		}

		public override void SetInState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {
			
			base.SetInState(parameters, window, root);

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			var canvasGroup = this.GetCanvasGroup(parameters);
			if (canvasGroup != null) canvasGroup.alpha = param.GetIn();
			
		}
		
		public override void SetOutState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {
			
			base.SetOutState(parameters, window, root);

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			var canvasGroup = this.GetCanvasGroup(parameters);
			if (canvasGroup != null) canvasGroup.alpha = param.GetOut();

		}
		
		public override void SetResetState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {

			base.SetResetState(parameters, window, root);

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			var canvasGroup = this.GetCanvasGroup(parameters);
			if (canvasGroup != null) canvasGroup.alpha = param.GetReset();
			
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Transitions/Alpha")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowAnimationTransitionAlpha>();
			
		}
		#endif

	}

}