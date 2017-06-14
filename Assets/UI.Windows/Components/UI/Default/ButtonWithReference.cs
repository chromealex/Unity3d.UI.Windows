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
			
			if (this.rootButton != null) {

				if (this.rootButton != this)
					this.rootButton.OnDeselect(eventData);

			} else {

				if (this.referenceButton != null)
					this.referenceButton.OnDeselect(eventData);

			}

			base.OnDeselect(eventData);

			this.OnStateChanged();

		}

		public override void OnMove(AxisEventData eventData) {

			if (this.rootButton != null) {

				if (this.rootButton != this)
					this.rootButton.OnMove(eventData);

			} else {

				if (this.referenceButton != null)
					this.referenceButton.OnMove(eventData);

			}

			base.OnMove(eventData);

			this.OnStateChanged();

		}

		public override void OnPointerClick(PointerEventData eventData) {

			if (this.rootButton != null) {

				if (this.rootButton != this) this.rootButton.OnPointerClick(eventData);

			} else {

				if (this.referenceButton != null) this.referenceButton.OnPointerClick(eventData);

			}

			base.OnPointerClick(eventData);

			this.OnStateChanged();

		}

		public override void OnPointerDown(PointerEventData eventData) {

			if (this.rootButton != null) {

				if (this.rootButton != this)
					this.rootButton.OnPointerDown(eventData);

			} else {

				if (this.referenceButton != null)
					this.referenceButton.OnPointerDown(eventData);

			}

			base.OnPointerDown(eventData);

			this.OnStateChanged();

		}

		public override void OnPointerEnter(PointerEventData eventData) {

			if (this.rootButton != null) {

				if (this.rootButton != this)
					this.rootButton.OnPointerEnter(eventData);

			} else {

				if (this.referenceButton != null)
					this.referenceButton.OnPointerEnter(eventData);

			}

			base.OnPointerEnter(eventData);

			this.OnStateChanged();

		}

		public override void OnPointerExit(PointerEventData eventData) {

			if (this.rootButton != null) {

				if (this.rootButton != this)
					this.rootButton.OnPointerExit(eventData);

			} else {

				if (this.referenceButton != null)
					this.referenceButton.OnPointerExit(eventData);

			}

			base.OnPointerExit(eventData);

			this.OnStateChanged();

		}

		public override void OnPointerUp(PointerEventData eventData) {

			if (this.rootButton != null) {

				if (this.rootButton != this)
					this.rootButton.OnPointerUp(eventData);

			} else {

				if (this.referenceButton != null)
					this.referenceButton.OnPointerUp(eventData);

			}

			base.OnPointerUp(eventData);

			this.OnStateChanged();

		}

		public override void OnSelect(BaseEventData eventData) {

			if (this.rootButton != null) {

				if (this.rootButton != this)
					this.rootButton.OnSelect(eventData);

			} else {

				if (this.referenceButton != null)
					this.referenceButton.OnSelect(eventData);

			}

			base.OnSelect(eventData);

			this.OnStateChanged();

		}

		new public bool interactable {

			set {
				
				base.interactable = value;

				if (this.rootButton != null && this.rootButton != this) {

					this.rootButton.interactable = value;

				} else {

					if (this.referenceButton != null) {

						this.referenceButton.interactable = value;

					}

				}

			}

			get {
	
				if (this.rootButton != null && this.rootButton != this) return this.rootButton.interactable;

				return base.interactable;

			}

		}

		public override bool IsInteractable() {

			if (this.rootButton != null && this.rootButton != this) return this.rootButton.IsInteractable();

			return base.IsInteractable();

		}

		protected virtual void OnStateChanged() {}

	}

}