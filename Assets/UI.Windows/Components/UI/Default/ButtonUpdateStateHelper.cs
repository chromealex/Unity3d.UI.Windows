using UnityEngine;
using System.Collections;

namespace UnityEngine.UI {

	public class ButtonUpdateStateHelper : MonoBehaviour {

		public ButtonExtended button;

		public void OnEnable() {

			if (this.button != null) {
				
				this.button.UpdateState();

			}

		}

	}

}