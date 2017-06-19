using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Extensions;

namespace UnityEngine.UI.Windows.Animations {

	[TransitionCamera]
	public class WindowAnimationTransitionScreenTransform : TransitionBase {
		
		[System.Serializable]
		public class Parameters : TransitionBase.ParametersVideoBase {

			public Parameters() : base() {}
			public Parameters(TransitionBase.ParametersBase baseDefaults) : base(baseDefaults) {}

			[System.Serializable]
			public class State {
				
				[Header("Canvas Group")]
				public float alpha = 1f;

				[Header("Rect Transform")]
				public Vector2 anchorMin;
				public Vector2 anchorMax;
				public Vector2 anchoredPosition;
				public Vector2 sizeDelta;
				public Vector2 pivot;
				
				[Header("Transform")]
				public Quaternion localRotation = Quaternion.identity;
				public Vector3 localScale = Vector3.one;
				
				[Header("Material")]
				public float materialStrength = 0f;

				public State() {
				}

				public State(WindowLayoutRoot root) {

					this.alpha = root.alpha;

					this.anchorMin = root.rectTransform.anchorMin;
					this.anchorMax = root.rectTransform.anchorMax;
					this.anchoredPosition = root.rectTransform.anchoredPosition;
					this.sizeDelta = root.rectTransform.sizeDelta;
					this.pivot = root.rectTransform.pivot;

					this.localRotation = root.rectTransform.localRotation;
					this.localScale = root.rectTransform.localScale;

					this.materialStrength = 0f;

				}

				public State(State source) {
					
					this.alpha = source.alpha;

					this.anchorMin = source.anchorMin;
					this.anchorMax = source.anchorMax;
					this.anchoredPosition = source.anchoredPosition;
					this.sizeDelta = source.sizeDelta;
					this.pivot = source.pivot;
					
					this.localRotation = source.localRotation;
					this.localScale = source.localScale;

					this.materialStrength = source.materialStrength;

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

			public void Apply(Material material, WindowLayoutRoot root, State startState, State resultState, float value) {

				root.alpha = Mathf.Lerp(startState.alpha, resultState.alpha, value);

				root.rectTransform.anchorMin = Vector2.Lerp(startState.anchorMin, resultState.anchorMin, value);
				root.rectTransform.anchorMax = Vector2.Lerp(startState.anchorMax, resultState.anchorMax, value);
				root.rectTransform.anchoredPosition = Vector2.Lerp(startState.anchoredPosition, resultState.anchoredPosition, value);
				root.rectTransform.sizeDelta = Vector2.Lerp(startState.sizeDelta, resultState.sizeDelta, value);
				root.rectTransform.pivot = Vector2.Lerp(startState.pivot, resultState.pivot, value);
				
				root.rectTransform.localRotation = Quaternion.Slerp(startState.localRotation, resultState.localRotation, value);
				root.rectTransform.localScale = Vector3.Lerp(startState.localScale, resultState.localScale, value);

				if (material != null) {

					material.SetFloat(this.GetMaterialStrengthName(), Mathf.Lerp(startState.materialStrength, resultState.materialStrength, value));

				}

			}
			
			public void Apply(Material material, WindowLayoutRoot root, State state) {

				root.alpha = state.alpha;

				root.rectTransform.anchorMin = state.anchorMin;
				root.rectTransform.anchorMax = state.anchorMax;
				root.rectTransform.anchoredPosition = state.anchoredPosition;
				root.rectTransform.sizeDelta = state.sizeDelta;
				root.rectTransform.pivot = state.pivot;
				
				root.rectTransform.localRotation = state.localRotation;
				root.rectTransform.localScale = state.localScale;

				if (material != null) {
					
					material.SetFloat(this.GetMaterialStrengthName(), state.materialStrength);
					
				}

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

			return (window as LayoutWindowType).GetCurrentLayout().GetLayoutInstance().root;

		}

		public override TransitionBase.ParametersBase GetDefaultInputParameters() {

			return this.defaultInputParams;

		}

		public override void Set(WindowBase window, TransitionInputParameters parameters, WindowComponentBase root, bool forward, float value) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) {

				return;
				
			}
			
			//var duration = this.GetDuration(parameters, forward);
			var rect = this.GetRoot(param, window);

			var state = new Parameters.State(rect);
			var resultState = param.GetResult(forward);
			
			var material = param.GetMaterialInstance();

			param.Apply(material, rect, state, resultState, ME.Ease.GetByType(forward == true ? param.inEase : param.outEase).interpolate(0f, 1f, value, 1f));

		}
		
		public override void OnPlay(WindowBase window, ME.Tweener.MultiTag tag, TransitionInputParameters parameters, WindowComponentBase root, bool forward, System.Action callback) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) {

				if (callback != null) callback();
				return;

			}

			var duration = this.GetDuration(parameters, forward);
			var resultState = param.GetResult(forward);

			var rect = this.GetRoot(param, window);
			var state = new Parameters.State(rect);

			var material = param.GetMaterialInstance();

			if (TweenerGlobal.instance != null) {

				//TweenerGlobal.instance.removeTweens(tag);
				TweenerGlobal.instance.addTween(rect, duration, 0f, 1f).onUpdate((obj, value) => {

					if (obj != null) {

						param.Apply(material, obj, state, resultState, value);

					}

				}).onComplete((obj) => {

					if (callback != null) callback();

				}).onCancel((obj) => {

					if (callback != null) callback();

				}).tag(tag).ease(ME.Ease.GetByType(forward == true ? param.inEase : param.outEase));

			} else {
				
				param.Apply(material, rect, resultState);
				if (callback != null) callback();
				
			}

		}
		
		public override void SetInState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			param.Apply(param.GetMaterialInstance(), this.GetRoot(param, window), param.GetIn());

		}
		
		public override void SetOutState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			param.Apply(param.GetMaterialInstance(), this.GetRoot(param, window), param.GetOut());

		}
		
		public override void SetResetState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			param.Apply(param.GetMaterialInstance(), this.GetRoot(param, window), param.GetReset());

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Transitions/Screen/Transform")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowAnimationTransitionScreenTransform>();
			
		}
		#endif

	}

}