using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowAnimationTransitionSlide : TransitionBase {
		
		[System.Serializable]
		public class Parameters : TransitionBase.ParametersBase {
			
			public Parameters(TransitionBase.ParametersBase baseDefaults) : base(baseDefaults) {}

			[System.Serializable]
			public class State {

				public Vector2 to;

			}

			public bool moveRoot = false;

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

				this.moveRoot = param.moveRoot;

			}
			
			public Vector2 GetIn() {
				
				return this.inState.to;
				
			}

			public Vector2 GetOut() {
				
				return this.outState.to;
				
			}

			public Vector2 GetReset() {

				return this.resetState.to;

			}

			public Vector2 GetResult(bool forward) {
				
				if (forward == true) {
					
					return this.GetIn();
					
				}
				
				return this.GetOut();
				
			}

		}

		public Parameters defaultInputParams;

		private RectTransform GetRoot(Parameters parameters, WindowComponentBase root) {

			WindowComponent component = null;
			if (root is LinkerComponent && !component) {

				component = (root as LinkerComponent).Get<WindowComponent>();
				
			}
			
			if (root is WindowLayoutBase && !component) {
				
				component = (root as WindowLayoutBase).GetCurrentComponent();
				
			}

			if (root is WindowComponent && !component) {
				
				component = root as WindowComponent;
				
			}

			//var component = (root is LinkerComponent ? (root as LinkerComponent).Get<WindowComponent>() : (root is WindowLayoutBase ? (root as WindowLayoutBase).GetCurrentComponent() : (root as WindowComponent ? root is WindowComponent : null)));
			if (component == null && parameters == null) return null;


			if (component == null || parameters.moveRoot == true) return root.transform as RectTransform;

			if (parameters == null) return component.transform as RectTransform;

			return component.transform as RectTransform;

		}

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

			var rect = this.GetRoot(param, root);
			var startPosition = rect.anchoredPosition;
			var endPosition = result;
			
			if (TweenerGlobal.instance != null) {

				TweenerGlobal.instance.removeTweens(tag);
				TweenerGlobal.instance.addTween<RectTransform>(rect, duration, 0f, 1f).onUpdate((obj, value) => {

					if (obj != null) {

						obj.anchoredPosition = Vector2.Lerp(startPosition, endPosition, value);

					}

				}).onComplete((obj) => { if (callback != null) callback(); }).onCancel((obj) => { if (callback != null) callback(); }).tag(tag);

			} else {
				
				rect.anchoredPosition = endPosition;
				if (callback != null) callback();
				
			}

		}
		
		public override void SetInState(TransitionInputParameters parameters, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			this.GetRoot(param, root).anchoredPosition = param.GetIn();

		}
		
		public override void SetOutState(TransitionInputParameters parameters, WindowComponentBase root) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			this.GetRoot(param, root).anchoredPosition = param.GetOut();

		}
		
		public override void SetResetState(TransitionInputParameters parameters, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			this.GetRoot(param, root).anchoredPosition = param.GetReset();

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Transitions/Slide")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowAnimationTransitionSlide>();
			
		}
		#endif

	}

}