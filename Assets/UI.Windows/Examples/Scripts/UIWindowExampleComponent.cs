using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

public class UIWindowExampleComponent : WindowComponent {

	public Text text;

	public override void OnInit() {

		this.SetText("This is the default behaviour of WindowSystem.ShowDefault() method.");

	}

	public void SetText(string text) {
		
		this.text.text = text;

	}

}
