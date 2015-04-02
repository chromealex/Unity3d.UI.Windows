using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Modules {

	public class BackgroundCloseable : WindowModule {

		private System.Action callback;
		private bool overrideAutoHide = false;

		public void SetCallback(System.Action callback, bool overrideAutoHide = true) {

			this.overrideAutoHide = overrideAutoHide;
			this.callback = callback;

		}

		public override void OnDeinit() {

			this.callback = null;

		}

		public void HideWindow() {

			if (this.callback != null) {

				this.callback();
				if (this.overrideAutoHide == true) return;

			}

			this.GetWindow().Hide();

		}

	}

}