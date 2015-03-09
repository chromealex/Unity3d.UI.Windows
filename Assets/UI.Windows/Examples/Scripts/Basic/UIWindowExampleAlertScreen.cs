using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleAlertScreen : LayoutWindowType {
	
	private ButtonComponent button;
	private UIWindowExampleComponent content;

	public void OnParametersPass(string text) {

		this.content.SetText(text);

	}

	public override void OnInit() {
		
		this.content = this.GetLayoutComponent<UIWindowExampleComponent>();
		this.button = this.GetLayoutComponent<ButtonComponent>();

		this.button.SetCallback(this.Hide);

	}

}
