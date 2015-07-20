#define FLOW_PLUGIN_HEATMAP
using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonComponent : ColoredComponent, ISelectable {

		[SerializeField]
		protected Button button;

		private ComponentEvent callback = new ComponentEvent();
		private ComponentEvent<ButtonComponent> callbackButton = new ComponentEvent<ButtonComponent>();

		private System.Func<bool> onState;
		private bool oldState = false;
		private bool onStateActive = false;

		public override void OnInit() {

			base.OnInit();

			#if FLOW_PLUGIN_HEATMAP
			//this.button.onClick.AddListener(this.HeatmapSendComponentClick);
			#endif

		}

		public override void OnDeinit() {

			base.OnDeinit();

			this.onState = null;

			if (this.button != null) this.button.onClick.RemoveListener(this.OnClick);
			this.callback.RemoveAllListeners();
			this.callbackButton.RemoveAllListeners();

			#if FLOW_PLUGIN_HEATMAP
			//this.button.onClick.RemoveListener(this.HeatmapSendComponentClick);
			#endif

		}

		public override void OnShowBegin(System.Action callback, bool resetAnimation = true) {

			base.OnShowBegin(callback, resetAnimation);

			this.onStateActive = true;

		}

		public override void OnHideEnd() {

			base.OnHideEnd();

			this.onStateActive = false;

		}

		public void SetEnabledState(System.Func<bool> onState) {

			this.onState = onState;
			this.oldState = this.onState();
			this.onStateActive = true;

		}

		public void LateUpdate() {

			if (this.onStateActive == true && this.onState != null) {

				var newState = this.onState();
				if (newState != this.oldState) this.SetEnabledState(newState);
				this.oldState = newState;

			}

		}

		public virtual void SetEnabledState(bool state) {

			if (state == true) {

				this.SetEnabled();

			} else {

				this.SetDisabled();

			}

		}

		public virtual void SetDisabled() {
		
			if (this.button != null) this.button.interactable = false;

		}

		public virtual void SetEnabled() {
			
			if (this.button != null) this.button.interactable = true;

		}

		public void Select() {

			this.button.interactable = false;

		}

		public void Deselect() {

			this.button.interactable = true;

		}
		
		public override void SetColor(Color color) {

			base.SetColor(color);

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

		public void OnClick() {

			if (this.GetWindow().GetState() != WindowObjectState.Shown &&
			    this.GetWindow().GetState() != WindowObjectState.Showing) {

				#if UNITY_EDITOR
				Debug.LogWarning("Can't send the click on " + this.GetWindow().GetState() + " state.");
				#endif
				return;

			}

			if (this.callback != null) this.callback.Invoke();
			if (this.callbackButton != null) this.callbackButton.Invoke(this);

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			if (this.gameObject.activeSelf == false) return;
			
			var buttons = this.GetComponentsInChildren<Button>(true);
			if (buttons.Length == 1) this.button = buttons[0];

		}
		#endif

	}

}
