using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Types;
using ME;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowAnimationTransitionSlide : TransitionBase {
		
		[System.Serializable]
		public class Parameters : TransitionBase.ParametersVideoBase {
			
			public enum ApplyTo : byte {
				
				None = 0x0,
				Position = 0x1,
				AnchorMin = 0x2,
				AnchorMax = 0x4,
				Scale = 0x8,
				Rotation = 0x10,
				Size = 0x20,
				Pivot = 0x40,
				
			};

			public Parameters() : base() {}
			public Parameters(TransitionBase.ParametersBase baseDefaults) : base(baseDefaults) {}

			[System.Serializable]
			public class State {
				
				[ReadOnly("<stateApply", state: (byte)ApplyTo.AnchorMin, bitMask: true)]
				public Vector2 anchorMin;
				[ReadOnly("<stateApply", state: (byte)ApplyTo.AnchorMax, bitMask: true)]
				public Vector2 anchorMax;
				[ReadOnly("<stateApply", state: (byte)ApplyTo.Position, bitMask: true)]
				public Vector2 to;
				[ReadOnly("<stateApply", state: (byte)ApplyTo.Scale, bitMask: true)]
				public Vector2 scale;
				[ReadOnly("<stateApply", state: (byte)ApplyTo.Rotation, bitMask: true)]
				public Vector3 rotation;
				[ReadOnly("<stateApply", state: (byte)ApplyTo.Size, bitMask: true)]
				public Vector2 size;
				[ReadOnly("<stateApply", state: (byte)ApplyTo.Pivot, bitMask: true)]
				public Vector2 pivot;

				public State() {
				}

				public State(RectTransform rect) {

					this.anchorMin = rect.anchorMin;
					this.anchorMax = rect.anchorMax;
					this.to = rect.anchoredPosition;
					this.scale = rect.localScale.XY();
					this.rotation = rect.localRotation.eulerAngles;
					this.size = rect.sizeDelta;
					this.pivot = rect.pivot;

				}

				public State(State source) {

					this.anchorMin = source.anchorMin;
					this.anchorMax = source.anchorMax;
					this.to = source.to;
					this.scale = source.scale;
					this.rotation = source.rotation;
					this.size = source.size;
					this.pivot = source.pivot;

				}

			}

			[Header("Root")]
			public bool moveRoot = false;
			public RectTransform root;

			[Header("States")]
			[BitMask(typeof(ApplyTo))]
			public ApplyTo stateApply = ApplyTo.Position;
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

				this.moveRoot = param.moveRoot;

				this.stateApply = param.stateApply;

			}

			public void Apply(RectTransform rect, State startState, State resultState, float value) {

				if ((this.stateApply & ApplyTo.AnchorMax) != 0) {

					rect.anchorMax = Vector2.Lerp(startState.anchorMax, resultState.anchorMax, value);

				}
				
				if ((this.stateApply & ApplyTo.AnchorMin) != 0) {
					
					rect.anchorMin = Vector2.Lerp(startState.anchorMin, resultState.anchorMin, value);
					
				}
				
				if ((this.stateApply & ApplyTo.Scale) != 0) {
					
					var scale = Vector2.Lerp(startState.scale, resultState.scale, value);
					rect.localScale = new Vector3(scale.x, scale.y, 1f);
					
				}
				
				if ((this.stateApply & ApplyTo.Rotation) != 0) {
					
					rect.localRotation = Quaternion.Euler(Vector3.Lerp(startState.rotation, resultState.rotation, value));
					
				}

				if ((this.stateApply & ApplyTo.Position) != 0) {

					rect.anchoredPosition = Vector2.Lerp(startState.to, resultState.to, value);

				}

				if ((this.stateApply & ApplyTo.Size) != 0) {

					rect.sizeDelta = Vector2.Lerp(startState.size, resultState.size, value);

				}

				if ((this.stateApply & ApplyTo.Pivot) != 0) {
					
					rect.SetPivot(Vector2.Lerp(startState.pivot, resultState.pivot, value));

				}

			}
			
			public void Apply(RectTransform rect, State state) {
				
				if ((this.stateApply & ApplyTo.AnchorMax) != 0) {
					
					rect.anchorMax = state.anchorMax;
					
				}

				if ((this.stateApply & ApplyTo.AnchorMin) != 0) {
					
					rect.anchorMin = state.anchorMin;
					
				}
				
				if ((this.stateApply & ApplyTo.Scale) != 0) {

					rect.localScale = new Vector3(state.scale.x, state.scale.y, 1f);
					
				}
				
				if ((this.stateApply & ApplyTo.Rotation) != 0) {
					
					rect.localRotation = Quaternion.Euler(state.rotation);
					
				}

				if ((this.stateApply & ApplyTo.Position) != 0) {

					rect.anchoredPosition = state.to;

				}

				if ((this.stateApply & ApplyTo.Size) != 0) {

					rect.sizeDelta = state.size;

				}

				if ((this.stateApply & ApplyTo.Pivot) != 0) {

					rect.SetPivot(state.pivot);

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

		private RectTransform GetRoot(Parameters parameters, WindowComponentBase root) {

			if (parameters != null && parameters.root != null) return parameters.root;

			WindowComponent component = null;
			if (root is LinkerComponent && component == null) {

				component = (root as LinkerComponent).Get<WindowComponent>();
				
			}
			
			if (root is WindowLayoutBase && component == null) {
				
				component = (root as WindowLayoutBase).GetCurrentComponent();
				
			}

			if (root is WindowComponent && component == null) {
				
				component = root as WindowComponent;
				
			}

			if (component == null && parameters == null) return null;
			if (component != null && parameters == null) return component.transform as RectTransform;
			if (component == null || (parameters != null && parameters.moveRoot == true)) {

				if (root == null) {

					Debug.LogWarning(string.Format("Root is null, GetRoot() returns {0}, {1}.", root, component));
					return null;

				}

				return root.transform as RectTransform;

			}

			return component.transform as RectTransform;

		}

		public override TransitionBase.ParametersBase GetDefaultInputParameters() {

			return this.defaultInputParams;

		}
		
		public override void OnPlay(WindowBase window, ME.Tweener.MultiTag tag, TransitionInputParameters parameters, WindowComponentBase root, bool forward, System.Action callback) {

			var param = this.GetParams<Parameters>(parameters);
			var rect = this.GetRoot(param, root);
			if (param == null || root == null || rect == null) {

				if (callback != null) callback();
				return;

			}

			var duration = this.GetDuration(parameters, forward);
			var resultState = param.GetResult(forward);

			var state = new Parameters.State(rect);

			if (TweenerGlobal.instance != null) {

				//TweenerGlobal.instance.removeTweens(tag);
				TweenerGlobal.instance.addTween<RectTransform>(rect, duration, 0f, 1f).ease(ME.Ease.GetByType(forward == true ? param.inEase : param.outEase)).onUpdate((obj, value) => {

					if (obj != null) {

						param.Apply(obj, state, resultState, value);

					}

				}).onComplete((obj) => { if (callback != null) callback(); }).onCancel((obj) => { if (callback != null) callback(); }).tag(tag);

			} else {
				
				param.Apply(rect, resultState);
				if (callback != null) callback();
				
			}

		}
		
		public override void SetInState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			param.Apply(this.GetRoot(param, root), param.GetIn());

		}
		
		public override void SetOutState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			param.Apply(this.GetRoot(param, root), param.GetOut());

		}
		
		public override void SetResetState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;

			param.Apply(this.GetRoot(param, root), param.GetReset());

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Transitions/Slide")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowAnimationTransitionSlide>();
			
		}
		#endif

	}

}