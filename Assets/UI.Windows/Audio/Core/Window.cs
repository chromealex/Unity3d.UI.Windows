using UnityEngine;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Animations;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows.Audio {
	
	[System.Serializable]
	public class Component {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		[FlowDataPopup]
		public FlowData flowData;
		#endif
		
		[SerializeField]
		private bool randomize = false;
		
		[SerializeField][ReadOnly("randomize", state: true, order = 1)][AudioPopup(ClipType.SFX, order = 2)]
		private int id = 0;
		
		[SerializeField][ReadOnly("randomize", state: false, order = 1)][AudioPopup(ClipType.SFX, order = 2)]
		public int[] randomIds = new int[0];
		
		public void Play() {
			
			WindowSystem.AudioPlayFX(this.id, this.randomIds, this.randomize);
			
		}
		
	}

	[System.Serializable]
	public class Window : IWindowEventsController {
		
		[HideInInspector]
		private WindowBase window;

		public void Setup(WindowBase window) {
			
			this.window = window;

		}

		public enum PlayType : byte {

			KeepCurrent,
			Replace,

		};

		#if UNITY_EDITOR
		[Header("Source")]
		//[HideInInspector]
		[FlowDataPopup]
		public FlowData flowData;
		#endif

		[ReadOnly("flowData", null)]
		public PlayType playType = PlayType.KeepCurrent;
		[ReadOnly("flowData", null)]
		public ClipType clipType;
		[AudioPopup("clipType")]
		public int id = 0;
		
		[Header("Transition")]
		public TransitionBase transition;
		public TransitionInputParameters transitionParameters;

		#region Audio Settings
		[Header("Settings")]
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
		#endregion

		public void ApplySettings() {

			WindowSystem.AudioChange(this.window, this.clipType, this.id, this);

		}

		// Events
		public void DoWindowLayoutComplete() { }
		public void DoWindowActive() { }
		public void DoWindowInactive() { }
		public void DoWindowOpen() { }
		public void DoWindowClose() { }
		public void DoInit() { }
		public void DoDeinit(System.Action callback) { callback.Invoke(); }
		public void DoShowEnd(AppearanceParameters parameters) { }
		public void DoHideEnd(AppearanceParameters parameters) { }
		
		public void DoShowBegin(AppearanceParameters parameters) {
			
			this.DoShowBegin(this.transition, this.transitionParameters, parameters);
			
		}

		public void DoShowBegin(TransitionBase transition, TransitionInputParameters transitionParameters, AppearanceParameters parameters) {

			var needToPlay = (this.id > 0 || this.playType == PlayType.Replace);

			if (this.playType == PlayType.Replace) WindowSystem.AudioStop(null, this.clipType, this.id);

			if (transition != null) {

				if (needToPlay == true) {

					WindowSystem.AudioPlay(this.window, this.clipType, this.id, this.playType == PlayType.Replace);
					transition.SetResetState(transitionParameters, this.window, null);
					transition.Play(null, this.window, transitionParameters, forward: true, callback: () => {
						
						parameters.Call();
						
					});

				}

			} else {

				if (needToPlay == true) WindowSystem.AudioPlay(this.window, this.clipType, this.id, this.playType == PlayType.Replace);
				parameters.Call();

			}
			
		}
		
		public void DoHideBegin(AppearanceParameters parameters) {
			
			this.DoHideBegin(this.transition, this.transitionParameters, parameters);
			
		}
		
		public void DoHideBegin(TransitionBase transition, TransitionInputParameters transitionParameters, AppearanceParameters parameters) {
			
			parameters.Call();

		}

		public void DoWindowUnload() {



		}

		public void Apply(TransitionBase transition, TransitionInputParameters parameters, bool forward, float value, bool reset) {
			
			if (reset == true) transition.SetResetState(parameters, this.window, null);
			transition.Set(this.window, parameters, null, forward: forward, value: value);
			
		}

	}

}