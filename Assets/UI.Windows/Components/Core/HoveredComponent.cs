using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components {

	public class HoveredComponent : WindowComponent, IHoverableComponent, IPointerEnterHandler, IPointerExitHandler {
		
		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.onHover.RemoveAllListeners();

		}

		#region source macros UI.Windows.HoveredComponent.Audio
		[Header("Audio")]
		[SerializeField]
		protected UnityEngine.UI.Windows.Audio.Component sfxOnClick = new UnityEngine.UI.Windows.Audio.Component();
		[SerializeField]
		protected UnityEngine.UI.Windows.Audio.Component sfxOnEnter = new UnityEngine.UI.Windows.Audio.Component();
		[SerializeField]
		protected UnityEngine.UI.Windows.Audio.Component sfxOnLeave = new UnityEngine.UI.Windows.Audio.Component();

		public IHoverableComponent SetSFX(PointerEventState state, UnityEngine.UI.Windows.Audio.Component data) {

			if (state == PointerEventState.Click) {

				this.sfxOnClick = data;

			} else if (state == PointerEventState.Enter) {

				this.sfxOnEnter = data;

			} else if (state == PointerEventState.Leave) {

				this.sfxOnLeave = data;

			}

			return this;

		}
		#endregion

		#region source macros UI.Windows.HoveredComponent.Hover
		[Header("Hover Actions")]
		[SerializeField]
		private bool hoverIsActive = true;
		[SerializeField][ReadOnly("hoverIsActive", state: false)]
		private bool hoverOnAnyPointerState = false;
		public ComponentEvent<bool> onHover = new ComponentEvent<bool>();

		[HideInInspector]
		private bool tempHoverState = false;

		public IHoverableComponent SetHoverState(bool state) {

			this.hoverIsActive = state;

			return this;

		}

		public IHoverableComponent SetHoverOnAnyPointerState(bool state) {

			this.hoverOnAnyPointerState = state;

			return this;

		}

		public virtual IHoverableComponent RemoveCallbackHover(System.Action<bool> onHover) {

			this.onHover.RemoveListener(onHover);

			return this;

		}

		public virtual IHoverableComponent SetCallbackHover(System.Action<bool> onHover) {

			this.onHover.AddListenerDistinct(onHover);

			return this;

		}

		protected virtual bool ValidateHoverPointer() {

			if (this.hoverIsActive == false) return false;
			if (this.hoverOnAnyPointerState == false && WindowSystemInput.GetPointerState() != PointerState.Default) return false;

			return true;

		}

		public void OnPointerEnter(PointerEventData eventData) {

			this.SetHoverEnter();

		}

		public void OnPointerExit(PointerEventData eventData) {

			this.SetHoverExit();

		}

		public bool IsHovered() {

			return this.tempHoverState;

		}

		public IHoverableComponent SetHoverEnter() {

			if (this.tempHoverState == true) {

				this.SetHoverExit();

			}

			this.tempHoverState = false;

			if (this.ValidateHoverPointer() == false) return this;

			this.sfxOnEnter.Play();
			this.tempHoverState = true;
			this.onHover.Invoke(true);

			return this;

		}

		public IHoverableComponent SetHoverExit() {

			if (this.tempHoverState == false) return this;

			this.sfxOnLeave.Play();
			this.onHover.Invoke(false);
			this.tempHoverState = false;

			return this;

		}
		#endregion

	}

}