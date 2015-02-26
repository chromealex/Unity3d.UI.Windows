using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

public class UIWindowExampleAlertScreen : LayoutWindowType {

	private UIWindowExampleComponent content;

	public void OnParametersPass(string text) {

		this.content.SetText(text);

	}

	public override void OnInit() {
		
		this.content = this.GetLayoutComponent<UIWindowExampleComponent>();

	}

}
