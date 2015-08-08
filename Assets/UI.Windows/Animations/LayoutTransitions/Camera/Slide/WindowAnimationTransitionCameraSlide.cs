using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Extensions;

namespace UnityEngine.UI.Windows.Animations {

	[TransitionCamera]
	public class WindowAnimationTransitionCameraSlide : TransitionBase {
		
		[System.Serializable]
		public class Parameters : TransitionBase.ParametersBase {

			public Parameters(TransitionBase.ParametersBase baseDefaults) : base(baseDefaults) {}

			[System.Serializable]
			public class State {

				public Vector2 anchorMin;
				public Vector2 anchorMax;
				public Vector2 anchoredPosition;
				public Vector2 sizeDelta;
				public Vector2 pivot;

				public State() {
				}

				public State(WindowLayoutRoot root) {
					
					this.anchorMin = root.rectTransform.anchorMin;
					this.anchorMax = root.rectTransform.anchorMax;
					this.anchoredPosition = root.rectTransform.anchoredPosition;
					this.sizeDelta = root.rectTransform.sizeDelta;
					this.pivot = root.rectTransform.pivot;

				}

				public State(State source) {
					
					this.anchorMin = source.anchorMin;
					this.anchorMax = source.anchorMax;
					this.anchoredPosition = source.anchoredPosition;
					this.sizeDelta = source.sizeDelta;
					this.pivot = source.pivot;

				}

			}

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

			public void Apply(WindowLayoutRoot root, State startState, State resultState, float value) {
				
				CanvasUpdater.ForceUpdate();

				root.rectTransform.anchorMin = Vector2.Lerp(startState.anchorMin, resultState.anchorMin, value);
				root.rectTransform.anchorMax = Vector2.Lerp(startState.anchorMax, resultState.anchorMax, value);
				root.rectTransform.anchoredPosition = Vector2.Lerp(startState.anchoredPosition, resultState.anchoredPosition, value);
				root.rectTransform.sizeDelta = Vector2.Lerp(startState.sizeDelta, resultState.sizeDelta, value);
				root.rectTransform.pivot = Vector2.Lerp(startState.pivot, resultState.pivot, value);
				
				CanvasUpdater.ForceUpdate();

			}
			
			public void Apply(WindowLayoutRoot root, State state) {
				
				CanvasUpdater.ForceUpdate();

				root.rectTransform.anchorMin = state.anchorMin;
				root.rectTransform.anchorMax = state.anchorMax;
				root.rectTransform.anchoredPosition = state.anchoredPosition;
				root.rectTransform.sizeDelta = state.sizeDelta;
				root.rectTransform.pivot = state.pivot;

				CanvasUpdater.ForceUpdate();

			}

			public State GetIn() {
				
				return this.inState;
				
			}

			public State GetOut() {
				
				return this.outState;
				
			}

			public State GetReset() {

				return this.resetState;

			}

			public State GetResult(bool forward) {
				
				if (forward == true) {
					
					return this.GetIn();
					
				}
				
				return this.GetOut();
				
			}

		}

		public Parameters defaultInputParams;

		private WindowLayoutRoot GetRoot(Parameters parameters, WindowBase window) {

			return (window as LayoutWindowType).layout.GetLayoutInstance().root;

		}

		public override TransitionBase.ParametersBase GetDefaultInputParameters() {

			return this.defaultInputParams;

		}

		public override void Set(WindowBase window, TransitionInputParameters parameters, WindowComponentBase root, bool forward, float value) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) {

				return;
				
			}
			
			var duration = this.GetDuration(parameters, forward);
			var rect = this.GetRoot(param, window);

			var state = new Parameters.State(rect);
			var resultState = param.GetResult(forward);

			param.Apply(rect, state, resultState, ME.Ease.GetByType(forward == true ? param.inEase : param.outEase).interpolate(0f, 1f, value, 1f));

		}
		
		public override void OnPlay(WindowBase window, object tag, TransitionInputParameters parameters, WindowComponentBase root, bool forward, System.Action callback) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) {

				if (callback != null) callback();
				return;

			}

			var duration = this.GetDuration(parameters, forward);
			var resultState = param.GetResult(forward);

			var rect = this.GetRoot(param, window);
			var state = new Parameters.State(rect);

			if (TweenerGlobal.instance != null) {

				TweenerGlobal.instance.removeTweens(tag);
				TweenerGlobal.instance.addTween(rect, duration, 0f, 1f).onUpdate((obj, value) => {

					if (obj != null) {

						param.Apply(obj, state, resultState, value);

					}

				}).onComplete((obj) => {

					if (callback != null) callback();
					CanvasUpdater.ForceUpdate();

				}).onCancel((obj) => {

					if (callback != null) callback();
					CanvasUpdater.ForceUpdate();

				}).tag(tag).ease(ME.Ease.GetByType(forward == true ? param.inEase : param.outEase));

			} else {
				
				param.Apply(rect, resultState);
				if (callback != null) callback();
				CanvasUpdater.ForceUpdate();
				
			}

		}
		
		public override void SetInState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			param.Apply(this.GetRoot(param, window), param.GetIn());

		}
		
		public override void SetOutState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			param.Apply(this.GetRoot(param, window), param.GetOut());

		}
		
		public override void SetResetState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			param.Apply(this.GetRoot(param, window), param.GetReset());

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Transitions/Camera/Slide")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowAnimationTransitionCameraSlide>();
			
		}
		#endif

	}

}