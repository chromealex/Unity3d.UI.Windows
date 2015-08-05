using UnityEngine;
using System.Collections;

namespace UnityEngine.UI {

	public class ButtonWithLabel : ButtonWithReference {

		public Text label;
		public ColorBlock labelColor = ColorBlock.defaultColorBlock;

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

			base.DoStateTransition(state, instant);

			if (this.label != null) {

				switch (this.currentSelectionState) {
					
					case SelectionState.Disabled:
						this.label.CrossFadeColor(this.labelColor.disabledColor, this.labelColor.fadeDuration, false, true);
						break;
						
					case SelectionState.Normal:
						this.label.CrossFadeColor(this.labelColor.normalColor, this.labelColor.fadeDuration, false, true);
						break;
						
					case SelectionState.Highlighted:
						this.label.CrossFadeColor(this.labelColor.highlightedColor, this.labelColor.fadeDuration, false, true);
						break;
						
					case SelectionState.Pressed:
						this.label.CrossFadeColor(this.labelColor.pressedColor, this.labelColor.fadeDuration, false, true);
						break;

				}

			}

		}

	}

}