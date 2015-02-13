using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

public class ButtonComponent : WindowComponent {

	public Button button;
	private System.Action callback;

	public void SetDisabled() {
	
		if (this.button != null) this.button.interactable = false;

	}

	public void SetEnabled() {
		
		if (this.button != null) this.button.interactable = true;

	}

	public void SetCallback(System.Action callback) {

		this.callback = callback;

	}

	public override void OnDeinit() {

		this.callback = null;

	}

	public void OnClick() {

		if (this.callback != null) this.callback();

	}

}
