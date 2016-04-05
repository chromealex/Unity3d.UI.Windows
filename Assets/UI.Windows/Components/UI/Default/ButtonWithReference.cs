using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI {

	public class ButtonWithReference : ButtonExtended {

		public Button rootButton;
		public Button referenceButton;

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

			base.DoStateTransition(state, instant);

			this.interactable = base.interactable;

		}

		public override void OnDeselect(BaseEventData eventData) {
			
			if (this.referenceButton != null) this.referenceButton.OnDeselect(eventData);
			base.OnDeselect(eventData);

		}

		public override void OnMove(AxisEventData eventData) {

			if (this.referenceButton != null) this.referenceButton.OnMove(eventData);
			base.OnMove(eventData);

		}

		public override void OnPointerDown(PointerEventData eventData) {

			if (this.referenceButton != null) this.referenceButton.OnPointerDown(eventData);
			base.OnPointerDown(eventData);

		}

		public override void OnPointerEnter(PointerEventData eventData) {

			if (this.referenceButton != null) this.referenceButton.OnPointerEnter(eventData);
			base.OnPointerEnter(eventData);

		}

		public override void OnPointerExit(PointerEventData eventData) {

			if (this.referenceButton != null) this.referenceButton.OnPointerExit(eventData);
			base.OnPointerExit(eventData);

		}

		public override void OnPointerUp(PointerEventData eventData) {

			if (this.referenceButton != null) this.referenceButton.OnPointerUp(eventData);
			base.OnPointerUp(eventData);

		}

		public override void OnSelect(BaseEventData eventData) {

			if (this.referenceButton != null) this.referenceButton.OnSelect(eventData);
			base.OnSelect(eventData);

		}

		new public bool interactable {

			set {
				
				base.interactable = value;

				if (this.rootButton != null) {

					this.rootButton.interactable = value;

				} else {

					if (this.referenceButton != null) {

						this.referenceButton.interactable = value;

					}

				}

			}

			get {
	
				if (this.rootButton != null) return this.rootButton.interactable;

				return base.interactable;

			}

		}

		public override bool IsInteractable() {

			if (this.rootButton != null) return this.rootButton.IsInteractable();

			return base.IsInteractable();

		}

	}

}