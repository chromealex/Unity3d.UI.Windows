using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Components {

	public class AnimatorComponent : WindowComponent {

		[Header("Base")]
		public Animator animator;

		[Header("Triggers")]
		public string playTriggerParameter;
		public string stopTriggerParameter;
		private int playTriggerParameterId;
		private int stopTriggerParameterId;

		[Header("States")]
		public string[] animatorStates;
		private int[] states;
		public int playStateIndex = -1;
		public int stopStateIndex = -1;

		public override void OnInit() {

			base.OnInit();

			this.CacheStates();

		}

		public void SetAnimator(Animator animator, string[] states) {

			this.animator = animator;
			this.animatorStates = states;

			this.CacheStates();

		}

		private void CacheStates() {

			if (this.animator != null) {
				
				// Cache states
				this.states = new int[this.animatorStates.Length];
				for (int i = 0; i < this.states.Length; ++i) {

					this.states[i] = Animator.StringToHash(this.animatorStates[i]);

				}

				// Cache triggers
				this.playTriggerParameterId = Animator.StringToHash(this.playTriggerParameter);
				this.stopTriggerParameterId = Animator.StringToHash(this.stopTriggerParameter);

			}

		}

		public void Play() {

			if (this.playStateIndex >= 0) {

				this.PlayState(this.playStateIndex);

			} else if (string.IsNullOrEmpty(this.playTriggerParameter) == false) {

				this.StopTrigger(this.stopTriggerParameterId);
				this.PlayTrigger(this.playTriggerParameterId);

			}

		}

		public void Stop() {

			if (this.stopStateIndex >= 0) {

				this.PlayState(this.stopStateIndex);

			} else if (string.IsNullOrEmpty(this.stopTriggerParameter) == false) {

				this.StopTrigger(this.playTriggerParameterId);
				this.PlayTrigger(this.stopTriggerParameterId);

			}

		}

		public void PlayState(int index) {

			if (index >= 0 && index < this.states.Length) {

				if (this.animator != null) this.animator.Play(this.states[index]);

			}

		}

		public void StopTrigger(int triggerId) {

			if (this.animator != null) {

				this.animator.ResetTrigger(triggerId);

			}

		}

		public void PlayTrigger(int triggerId) {

			if (this.animator != null) {

				this.animator.SetTrigger(triggerId);

			}

		}

	}

}