using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Modules {

	public class BackgroundCloseable : BackgroundBase {

		private System.Action callback;
		private bool overrideAutoHide = false;

		private bool enabledState;

		public void SetEnableState(bool state) {

			this.enabledState = state;

		}

		public void SetCallback(System.Action callback, bool overrideAutoHide = true) {

			this.overrideAutoHide = overrideAutoHide;
			this.callback = callback;

		}

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.callback = null;

		}

		public void HideWindow() {

			if (this.enabledState == false) return;

			if (this.callback != null) {

				this.callback();
				if (this.overrideAutoHide == true) return;

			}

			this.GetWindow().Hide();

		}

	}

}