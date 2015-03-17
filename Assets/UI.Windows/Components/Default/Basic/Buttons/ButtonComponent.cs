using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonComponent : WindowComponent, ISelectable {

		[SerializeField]
		protected Button button;

		private ComponentEvent callback = new ComponentEvent();
		private ComponentEvent<ButtonComponent> callbackButton = new ComponentEvent<ButtonComponent>();

		public void SetDisabled() {
		
			if (this.button != null) this.button.interactable = false;

		}

		public void SetEnabled() {
			
			if (this.button != null) this.button.interactable = true;

		}
		
		public Color GetColor() {
			
			return this.button != null ? this.button.targetGraphic.color : Color.white;
			
		}

		public void Select() {

			this.button.interactable = false;

		}

		public void Deselect() {

			this.button.interactable = true;

		}
		
		public void SetColor(Color color) {
			
			if (this.button != null) {

				this.button.targetGraphic.color = color;

			}

		}

		public virtual void SetCallback(UnityAction callback) {
			
			this.callback.AddListenerDistinct(callback);
			this.callbackButton.RemoveAllListeners();

			this.button.onClick.RemoveListener(this.OnClick);
			this.button.onClick.AddListener(this.OnClick);

		}
		
		public virtual void SetCallback(UnityAction<ButtonComponent> callback) {
			
			this.callbackButton.AddListenerDistinct(callback);
			this.callback.RemoveAllListeners();

			this.button.onClick.RemoveListener(this.OnClick);
			this.button.onClick.AddListener(this.OnClick);

		}

		public override void OnDeinit() {

			base.OnDeinit();

			this.button.onClick.RemoveListener(this.OnClick);
			this.callback.RemoveAllListeners();
			this.callbackButton.RemoveAllListeners();

		}

		public void OnClick() {

			if (this.callback != null) this.callback.Invoke();
			if (this.callbackButton != null) this.callbackButton.Invoke(this);

		}
		
		#if UNITY_EDITOR
		public override void OnValidate() {

			base.OnValidate();

			if (this.gameObject.activeSelf == false) return;
			
			var buttons = this.GetComponentsInChildren<Button>(true);
			if (buttons.Length == 1) this.button = buttons[0];

		}
		#endif

	}

}
