using UnityEngine;
using System.Collections;

namespace UnityEngine.UI {

	public class ButtonWithLabel : ButtonWithReference {

		public Text label;
		public ColorBlock labelColor = ColorBlock.defaultColorBlock;

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

			base.DoStateTransition(state, instant);

			if (this.label != null) {

				if (this.interactable == false) {

					this.label.color = this.labelColor.disabledColor;

				} else {

					this.label.color = this.labelColor.normalColor;

				}

			}

		}
		/*
		new public bool interactable {
			
			set {

				base.interactable = value;

				// On Changed

			}
			
			get {

				return base.interactable;
				
			}
			
		}*/

	}

}