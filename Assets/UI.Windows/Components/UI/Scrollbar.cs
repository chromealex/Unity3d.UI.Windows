using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Extensions {

	[ExecuteInEditMode()]
	public class Scrollbar : UnityEngine.UI.Scrollbar {

		public bool hideRectOnInactive = true;

		private bool lastInteractable;

		protected virtual void LateUpdate() {

			if (this.interactable != this.lastInteractable) {

				if (this.hideRectOnInactive == true) {

					this.handleRect.gameObject.SetActive(this.interactable);

				} else {

					this.handleRect.gameObject.SetActive(true);

				}

			}

			this.lastInteractable = this.interactable;

		}

	}

}