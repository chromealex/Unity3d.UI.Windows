using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Modules {

	public class CloseButton : WindowModule {

		private System.Action callback;

		public void SetManualCallback(System.Action callback) {

			this.callback = callback;

		}

		public void OnAction() {

			if (this.callback != null) {

				this.callback();
				return;

			}

			this.GetWindow().Hide();

		}

	}

}