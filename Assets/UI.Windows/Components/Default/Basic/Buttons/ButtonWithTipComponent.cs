using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonWithTipComponent : ButtonHoverComponent {

		private string tipText;
		private TipWindowType infoWindow;

		public override void OnInit() {
			
			base.OnInit();

			this.SetCallbackHover(this.OnStateChanged);
			
		}

		public override void OnDeinit() {

			base.OnDeinit();

			this.OnStateChanged(state: false);
			this.infoWindow = null;

		}

		public void OnStateChanged(bool state) {

			if (state == true) {

				this.infoWindow = WindowSystem.Show<TextTipWindowType>(this.tipText) as TipWindowType;
				if (this.infoWindow != null) this.infoWindow.OnHover(this.transform as RectTransform);

			} else {

				if (this.infoWindow != null) {

					this.infoWindow.OnLeave();
					this.infoWindow.Hide();
					this.infoWindow = null;

				}

			}

		}

		public void SetTextToTip(string tipText) {

			this.tipText = tipText;

		}

	}

}