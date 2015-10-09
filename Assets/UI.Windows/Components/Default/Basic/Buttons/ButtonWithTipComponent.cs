using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Types;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonWithTipComponent : ButtonComponent {

		private string tipText;
		private TipWindowType infoWindow;

		public override void OnInit() {
			
			base.OnInit();

			this.SetCallbackHover(this.OnStateChanged);
			
		}

		public override void OnDeinit() {

			base.OnDeinit();

			this.OnStateChanged(state: false);

		}

		public override void OnHideBegin(System.Action callback, bool immediately = false) {

			base.OnHideBegin(callback, immediately);

			this.OnStateChanged(state: false);

		}

		public void OnStateChanged(bool state) {

			if (state == true) {

				this.infoWindow = WindowSystem.Show<TextTipWindowType>((window) => window.PrepareFor(this),
				                                                       (window) => window.OnParametersPass(this.tipText)
				                                                       ) as TipWindowType;

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