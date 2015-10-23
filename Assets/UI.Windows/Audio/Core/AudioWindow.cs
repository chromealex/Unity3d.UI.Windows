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
	public class Window : IWindowEventsAsync {
		
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
		public void OnInit() { }
		public void OnDeinit() { }
		public void OnShowEnd() { }
		public void OnHideEnd() { }
		
		public void OnShowBegin(System.Action callback, bool resetAnimation = true) {
			
			this.OnShowBegin(this.transition, this.transitionParameters, callback);
			
		}

		public void OnShowBegin(TransitionBase transition, TransitionInputParameters transitionParameters, System.Action callback) {

			if (transition != null) {

				if (this.id > 0 || this.playType == PlayType.Replace) {

					WindowSystem.AudioPlay(this.window, this.clipType, this.id);
					transition.SetResetState(transitionParameters, this.window, null);
					transition.Play(this.window, transitionParameters, null, forward: true, callback: () => {
						
						if (callback != null) callback();
						
					});

				}

			} else {

				if (this.id > 0 || this.playType == PlayType.Replace) WindowSystem.AudioPlay(this.window, this.clipType, this.id);
				if (callback != null) callback();

			}
			
		}
		
		public void OnHideBegin(System.Action callback, bool immediately = false) {
			
			this.OnHideBegin(this.transition, this.transitionParameters, callback);
			
		}
		
		public void OnHideBegin(TransitionBase transition, TransitionInputParameters transitionParameters, System.Action callback) {

			var newWindow = WindowSystem.GetCurrentWindow();
			if (newWindow == null) return;

			if (transition != null) {

				if (newWindow.audio.id > 0 || newWindow.audio.playType == PlayType.Replace) {

					transition.Play(this.window, transitionParameters, null, forward: false, callback: () => {

						WindowSystem.AudioStop(this.window, this.clipType, this.id);
						if (callback != null) callback();

					});

				}

			} else {

				if (newWindow.audio.id > 0 || newWindow.audio.playType == PlayType.Replace) WindowSystem.AudioStop(this.window, this.clipType, this.id);
				if (callback != null) callback();
				
			}
			
		}
		/*
		private void TryToPlay(System.Action callback) {
			
			var equals = false;

			var prevWindow = WindowSystem.GetPreviousWindow();
			if (prevWindow != null) {
				
				equals = (prevWindow.audio.id == this.id || prevWindow.audio.id == 0);
				
				if (this.playType == PlayType.RestartIfEquals && equals == true) {

					WindowSystem.AudioStop(this.window, this.clipType, prevWindow.audio.id);

				} else if (equals == false) {

					WindowSystem.AudioStop(this.window, this.clipType, prevWindow.audio.id);

				}
				
			}

			if (this.playType == PlayType.KeepCurrent && equals == true) {
				
				// Keep current
				
			} else {
				
				if (this.id > 0) {

					WindowSystem.AudioPlay(this.window, this.clipType, this.id);

				}
				
			}
			
			if (callback != null) callback();

		}*/

		public void Apply(TransitionBase transition, TransitionInputParameters parameters, bool forward, float value, bool reset) {
			
			if (reset == true) transition.SetResetState(parameters, this.window, null);
			transition.Set(this.window, parameters, null, forward: forward, value: value);
			
		}

	}

}