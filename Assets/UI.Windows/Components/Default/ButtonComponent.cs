using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

public class ButtonComponent : WindowComponent {

	public Button button;
	private System.Action callback;
	private System.Action<ButtonComponent> callbackButton;

	public void SetDisabled() {
	
		if (this.button != null) this.button.interactable = false;

	}

	public void SetEnabled() {
		
		if (this.button != null) this.button.interactable = true;

	}
	
	public void SetCallback(System.Action callback) {
		
		this.callback = callback;
		this.callbackButton = null;
		
	}
	
	public void SetCallback(System.Action<ButtonComponent> callback) {
		
		this.callbackButton = callback;
		this.callback = null;
		
	}

	public override void OnDeinit() {

		this.callback = null;
		this.callbackButton = null;

	}

	public void OnClick() {

		if (this.callback != null) this.callback();
		if (this.callbackButton != null) this.callbackButton(this);

	}

}
