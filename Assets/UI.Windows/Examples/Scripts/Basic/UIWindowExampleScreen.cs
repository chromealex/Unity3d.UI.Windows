using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

public class UIWindowExampleScreen : LayoutWindowType {

	private UIWindowExampleComponent contentComponent;
	private ButtonComponent buttonAlert;
	private UIWindowExampleButtonComponent button;
	private bool closeOnButtonPress;

	public void OnParametersPass(bool closeOnButtonPress) {

		this.closeOnButtonPress = closeOnButtonPress;

		if (this.closeOnButtonPress == true) {
			
			this.contentComponent.SetText("This is the second instance of UIWindowExampleScreen. Look for this behaviour in UIWindowExampleScreen->OnInit() method.");
			this.button.SetTextToTip("<b>Click here to close current instance.</b>\nYou can simply edit this text or pass it from your code. See UIWindowExampleTip and UIWindowExampleScreen.");

		}

	}

	public override void OnInit() {

		this.contentComponent = this.GetWindow().GetLayoutComponent<UIWindowExampleComponent>();

		this.buttonAlert = this.GetWindow().GetLayoutComponent<ButtonComponent>(LayoutTag.Tag4);
		this.buttonAlert.SetCallback(this.OnAlert);

		this.button = this.GetWindow().GetLayoutComponent<UIWindowExampleButtonComponent>();
		this.button.SetCallback(this.OnClick);

		this.button.SetTextToTip("<b>Click here to open new instance.</b>\nYou can simply edit this text or pass it from your code. See UIWindowExampleTip and UIWindowExampleScreen.");

	}

	private void OnAlert() {

		WindowSystem.Show<UIWindowExampleAlertScreen>("Some text to the alert message.\nYou can click on the background now to close this window. This is an example of module usage.");

	}

	private void OnClick() {

		if (this.closeOnButtonPress == true) {

			this.Hide();

		} else {

			WindowSystem.Show<UIWindowExampleScreen>(true);

		}

	}

}
