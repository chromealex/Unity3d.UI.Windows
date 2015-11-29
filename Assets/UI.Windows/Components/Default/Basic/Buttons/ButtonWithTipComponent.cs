using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Types;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonWithTipComponent : ButtonWithTipComponent<TextTipWindowType> {
		
		private string tipText;
		
		public void SetTextToTip(string tipText) {
			
			if (this.tipText != tipText) {
				
				this.tipText = tipText;
				this.SetState(state: false);
				
			} else {
				
				this.tipText = tipText;
				
			}
			
		}

		public override void OnParametersPass(TextTipWindowType window) {

			window.OnParametersPass(this.tipText);

		}

		public override bool IsValid() {

			return string.IsNullOrEmpty(this.tipText) == false;

		}

	}
	
	public abstract class ButtonWithTipComponent<T> : ButtonComponent where T : TipWindowType {

		private TipWindowType infoWindow;
		
		public override void OnInit() {
			
			base.OnInit();
			
			this.SetState(state: false);
			this.SetCallbackHover(this.SetState);
			
		}
		
		public override void OnDeinit() {

			this.SetState(state: false);
			
			base.OnDeinit();

		}

		public override void OnShowBegin(System.Action callback, bool resetAnimation) {

			base.OnShowBegin(callback, resetAnimation);

			this.SetState(state: false);

		}

		public override void OnHideEnd() {

			base.OnHideEnd();
			
			this.SetState(state: false);

		}
		
		public override void OnHideBegin(System.Action callback, bool immediately = false) {
			
			base.OnHideBegin(callback, immediately);
			
			this.SetState(state: false);
			
		}

		public void SetState(bool state) {

			if (state == true) {
				
				if (this.IsValid() == true) {
					
					this.infoWindow = WindowSystem.Show<T>((window) => window.PrepareFor(this),
					                                                       (window) => this.OnParametersPass(window)
					                                                       ) as TipWindowType;
					
					if (this.infoWindow != null) this.infoWindow.OnHover(this.transform as RectTransform);
					
				}
				
			} else {
				
				if (this.infoWindow != null) {
					
					this.infoWindow.OnLeave();
					this.infoWindow.Hide();
					this.infoWindow = null;
					
				}
				
			}
			
		}

		public abstract void OnParametersPass(T window);

		public virtual bool IsValid() {

			return true;

		}

	}

}