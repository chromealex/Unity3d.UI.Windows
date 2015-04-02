using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Types;

public class UIWindowExampleAlertScreen : LayoutWindowType {
	
	private ButtonComponent button;
	private UIWindowExampleComponent content;

	private string text;

	public void OnParametersPass(string text) {

		this.text = text;

	}

	public override void OnInit() {
		
		this.content = this.GetLayoutComponent<UIWindowExampleComponent>();
		this.button = this.GetLayoutComponent<ButtonComponent>();

		this.button.SetCallback(() => this.Hide());
		
		this.content.SetText(this.text);

	}

}
