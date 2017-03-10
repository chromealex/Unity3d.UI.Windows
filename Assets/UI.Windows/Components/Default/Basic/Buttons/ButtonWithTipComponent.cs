using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Types;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows.Plugins.Localization;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonWithTipComponent : ButtonWithTipComponent<TextTipWindowType> {
		
		public LocalizationKey keyTooltipNormal;
		public LocalizationKey keyTooltipDisabled;

		private string tipText;

		public override void OnShowBegin() {

			base.OnShowBegin();

			if (this.keyTooltipNormal.IsNone() == false) {

				this.SetTextToTip(this.keyTooltipNormal);

			}

		}

		public override void OnInteractableChanged() {

			base.OnInteractableChanged();

			var key = (this.IsInteractable() == true ? this.keyTooltipNormal : this.keyTooltipDisabled);

			if (key.IsNone() == false) {
				
				this.SetTextToTip(key);
				
			}

		}

		public override void OnLocalizationChanged() {

			base.OnLocalizationChanged();

			this.OnInteractableChanged();

		}
		
		public void SetTextToTip(string tipText) {
			
			if (this.tipText != tipText) {
				
				this.tipText = tipText;
				this.UpdateTip();
				
			} else {
				
				this.tipText = tipText;
				
			}
			
		}
		
		public void SetTextToTip(LocalizationKey tipKey, params object[] parameters) {

			this.SetTextToTip(LocalizationSystem.Get(tipKey, parameters));

		}

		public override void OnParametersPass(TextTipWindowType window) {

			window.OnParametersPass(this.tipText);

		}

		public override bool IsValid() {

			return string.IsNullOrEmpty(this.tipText) == false;

		}

	}
	
	public abstract class ButtonWithTipComponent<T> : ButtonComponent where T : TipWindowType {

		public TipWindowType.ShowPriority tipShowPriority = TipWindowType.ShowPriority.Up;

		private TipWindowType infoWindow;

		public void OnDisable() {

			this.SetState(state: false);

		}

		public override void OnInit() {
			
			base.OnInit();
			
			this.SetState(state: false);
			this.SetCallbackHover(this.SetState);
			
		}
		
		public override void OnDeinit(System.Action callback) {

			this.SetState(state: false);

			base.OnDeinit(callback);

		}

		public override void OnShowBegin() {

			base.OnShowBegin();

			this.SetState(state: false);

		}

		public override void OnHideEnd() {

			base.OnHideEnd();
			
			this.SetState(state: false);

		}
		
		public override void OnHideBegin() {
			
			base.OnHideBegin();
			
			this.SetState(state: false);
			
		}

		public override void OnWindowInactive() {

			base.OnWindowInactive();

			this.SetState(state: false);

		}

		public void UpdateTip() {

			if (this.infoWindow != null) {

				this.OnParametersPass(this.infoWindow as T);

			}

		}

		public void SetState(bool state) {

			if (state == true) {
				
				if (this.IsValid() == true) {
					
					this.infoWindow = WindowSystem.Show<T>((window) => window.PrepareFor(this),
					                                                       (window) => this.OnParametersPass(window)
					                                                       ) as TipWindowType;

					if (this.infoWindow != null) this.infoWindow.OnHover(this.transform as RectTransform, this.tipShowPriority);
					
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