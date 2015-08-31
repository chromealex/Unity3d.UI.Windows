using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonHoverComponent : ButtonWithTextComponent, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler {

		public ComponentEvent<bool> onHover = new ComponentEvent<bool>();

		private bool hoverIsActive = true;

		public void SetHoverState(bool state) {

			this.hoverIsActive = state;

		}

		public virtual void SetCallbackHover(UnityAction<bool> onHover) {

			this.onHover.AddListenerDistinct(onHover);

		}

		public void OnPointerEnter(PointerEventData eventData) {

			if (this.button.interactable == false) return;
			if (this.hoverIsActive == false) return;

			if (this.onHover != null) this.onHover.Invoke(true);

		}

		public void OnPointerExit(PointerEventData eventData) {
			
			if (this.onHover != null) this.onHover.Invoke(false);

		}

		public override void OnHideBegin(System.Action callback, bool immediately = false) {

			base.OnHideBegin(callback, immediately);

			this.OnPointerExit(null);

		}

		public override void OnDeinit() {

			base.OnDeinit();

			this.onHover = null;

		}

	}

}