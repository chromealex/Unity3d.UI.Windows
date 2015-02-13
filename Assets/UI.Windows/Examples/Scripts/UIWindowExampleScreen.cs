using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

public class UIWindowExampleScreen : LayoutWindowType {

	private UIWindowExampleComponent contentComponent;
	private UIWindowExampleButtonComponent button;
	private bool closeOnButtonPress;

	public void OnParametersPass(bool closeOnButtonPress) {

		this.closeOnButtonPress = closeOnButtonPress;

		if (this.closeOnButtonPress == true) {
			
			this.contentComponent.SetText("This is the second instance of UIWindowExampleScreen. Look for this behaviour in UIWindowExampleScreen->OnInit() method.");
			this.button.SetTextToTip("<b>Click here to close current instance.</b>\nYou can easy edit this text or pass it from your code. See UIWindowExampleTip.");

		}

	}

	public override void OnInit() {

		this.contentComponent = this.GetWindow().GetLayoutComponent<UIWindowExampleComponent>();

		this.button = this.GetWindow().GetLayoutComponent<UIWindowExampleButtonComponent>();
		this.button.SetCallback(this.OnClick);

		this.button.SetTextToTip("<b>Click here to open new instance.</b>\nYou can easy edit this text or pass it from your code. See UIWindowExampleTip.");

	}

	private void OnClick() {

		if (this.closeOnButtonPress == true) {

			this.Hide();

		} else {

			WindowSystem.Show<UIWindowExampleScreen>(true);

		}

	}

}
