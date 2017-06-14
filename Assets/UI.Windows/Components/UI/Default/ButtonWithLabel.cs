using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI {

	public class ButtonWithLabel : ButtonWithReference, IPointerUpHandler, IPointerDownHandler {

		public Text label;
		public ColorBlock labelColor = ColorBlock.defaultColorBlock;

		public bool pointerUpClick;
		public bool pointerDownClick;

		/*protected override void OnStateChanged() {

			base.OnStateChanged();

			this.ApplyState(this.currentSelectionState, false);

		}*/

		public override void OnPointerUp(PointerEventData data) {

			base.OnPointerUp(data);

			if (this.pointerUpClick == true) {

				this.OnPointerClick(data);

			}

		}

		public override void OnPointerDown(PointerEventData data) {

			base.OnPointerDown(data);

			if (this.pointerDownClick == true) {

				this.OnPointerClick(data);

			}

		}

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

			base.DoStateTransition(state, instant);

			this.ApplyState(state, instant);

		}

		/*protected override void OnDidApplyAnimationProperties() {

			base.OnDidApplyAnimationProperties();
			
			this.ApplyState(this.currentSelectionState, false);

		}*/

		private void ApplyState(Selectable.SelectionState state, bool instant) {

			if (this.label != null) {

				switch (state) {

					case SelectionState.Disabled:
						this.label.CrossFadeColor(this.labelColor.disabledColor, instant == true ? 0f : this.labelColor.fadeDuration, false, true);
						break;
						
					case SelectionState.Normal:
						this.label.CrossFadeColor(this.labelColor.normalColor, instant == true ? 0f : this.labelColor.fadeDuration, false, true);
						break;
						
					case SelectionState.Highlighted:
						this.label.CrossFadeColor(this.labelColor.highlightedColor, instant == true ? 0f : this.labelColor.fadeDuration, false, true);
						break;
						
					case SelectionState.Pressed:
						this.label.CrossFadeColor(this.labelColor.pressedColor, instant == true ? 0f : this.labelColor.fadeDuration, false, true);
						break;
						
				}
				
			}

		}

	}

}