using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Extensions;

namespace UnityEngine.UI.Windows.Animations {

	[TransitionCamera]
	public class WindowAudioTransitionFade : TransitionBase {
		
		[System.Serializable]
		public class Parameters : TransitionBase.ParametersAudioBase {

			public Parameters(TransitionBase.ParametersBase baseDefaults) : base(baseDefaults) {}

			[System.Serializable]
			public class State {
				
				public bool bypassEffect = false;
				public bool bypassListenerEffect = false;
				public bool bypassReverbEffect = false;
				public bool loop = true;

				[Range(0, 256)]
				public int priority = 128;
				[Range(0f, 1f)]
				public float volume = 1f;
				[Range(-3f, 3f)]
				public float pitch = 1f;
				[Range(-1f, 1f)]
				public float panStereo = 0f;
				[Range(0f, 1f)]
				public float spatialBlend = 0f;
				[Range(0f, 1.1f)]
				public float reverbZoneMix = 1f;

				public State() {
				}

				public State(Audio.Window source) {
					
					this.bypassEffect = source.bypassEffect;
					this.bypassListenerEffect = source.bypassListenerEffect;
					this.bypassReverbEffect = source.bypassReverbEffect;
					this.loop = source.loop;

					this.priority = source.priority;
					this.volume = source.volume;
					this.pitch = source.pitch;
					this.panStereo = source.panStereo;
					this.spatialBlend = source.spatialBlend;
					this.reverbZoneMix = source.reverbZoneMix;

				}

				public State(State source) {
					
					this.bypassEffect = source.bypassEffect;
					this.bypassListenerEffect = source.bypassListenerEffect;
					this.bypassReverbEffect = source.bypassReverbEffect;
					this.loop = source.loop;
					
					this.priority = source.priority;
					this.volume = source.volume;
					this.pitch = source.pitch;
					this.panStereo = source.panStereo;
					this.spatialBlend = source.spatialBlend;
					this.reverbZoneMix = source.reverbZoneMix;

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
			
			public override float GetVolumeValueIn(float value) {

				return ME.Ease.GetByType(this.inEase).interpolate(this.resetState.volume, this.inState.volume - this.resetState.volume, value, this.inDuration);
				
			}
			
			public override float GetVolumeValueOut(float value) {
				
				return ME.Ease.GetByType(this.outEase).interpolate(this.inState.volume, this.outState.volume - this.inState.volume, value, this.outDuration);

			}

			public void Apply(Audio.Window source, State startState, State resultState, float value) {
				
				source.bypassEffect = resultState.bypassEffect;
				source.bypassListenerEffect = resultState.bypassListenerEffect;
				source.bypassReverbEffect = resultState.bypassReverbEffect;
				source.loop = resultState.loop;
				
				source.priority = (int)Mathf.Lerp(startState.priority, resultState.priority, value);
				source.volume = Mathf.Lerp(startState.volume, resultState.volume, value);
				source.pitch = Mathf.Lerp(startState.pitch, resultState.pitch, value);
				source.panStereo = Mathf.Lerp(startState.panStereo, resultState.panStereo, value);
				source.spatialBlend = Mathf.Lerp(startState.spatialBlend, resultState.spatialBlend, value);
				source.reverbZoneMix = Mathf.Lerp(startState.reverbZoneMix, resultState.reverbZoneMix, value);
				
				source.ApplySettings();

			}
			
			public void Apply(Audio.Window source, State resultState) {
				
				source.bypassEffect = resultState.bypassEffect;
				source.bypassListenerEffect = resultState.bypassListenerEffect;
				source.bypassReverbEffect = resultState.bypassReverbEffect;
				source.loop = resultState.loop;
				
				source.priority = resultState.priority;
				source.volume = resultState.volume;
				source.pitch = resultState.pitch;
				source.panStereo = resultState.panStereo;
				source.spatialBlend = resultState.spatialBlend;
				source.reverbZoneMix = resultState.reverbZoneMix;
				
				source.ApplySettings();

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

		private Audio.Window GetRoot(Parameters parameters, WindowBase window) {

			return window.audio;

		}

		public override TransitionBase.ParametersBase GetDefaultInputParameters() {

			return this.defaultInputParams;

		}

		public override void Set(WindowBase window, TransitionInputParameters parameters, WindowComponentBase root, bool forward, float value) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) {

				return;
				
			}

			var source = this.GetRoot(param, window);

			var state = new Parameters.State(source);
			var resultState = param.GetResult(forward);

			param.Apply(source, state, resultState, ME.Ease.GetByType(forward == true ? param.inEase : param.outEase).interpolate(0f, 1f, value, 1f));

		}
		
		public override void OnPlay(WindowBase window, object tag, TransitionInputParameters parameters, WindowComponentBase root, bool forward, System.Action callback) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) {

				if (callback != null) callback();
				return;

			}

			var duration = this.GetDuration(parameters, forward);
			var resultState = param.GetResult(forward);

			var source = this.GetRoot(param, window);
			var state = new Parameters.State(source);

			if (TweenerGlobal.instance != null) {

				//TweenerGlobal.instance.removeTweens(tag);
				TweenerGlobal.instance.addTween(source, duration, 0f, 1f).onUpdate((obj, value) => {

					if (obj != null) {

						param.Apply(obj, state, resultState, value);

					}

				}).onComplete((obj) => {

					if (callback != null) callback();

				}).onCancel((obj) => {

					if (callback != null) callback();

				}).tag(tag).ease(ME.Ease.GetByType(forward == true ? param.inEase : param.outEase));

			} else {
				
				param.Apply(source, resultState);
				if (callback != null) callback();
				
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
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Transitions/Screen/Transform")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowAnimationTransitionScreenTransform>();
			
		}
		#endif

	}

}