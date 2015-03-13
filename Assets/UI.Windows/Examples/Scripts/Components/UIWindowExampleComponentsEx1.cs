using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System;

public class UIWindowExampleComponentsEx1 : WindowComponent {
	
	public ButtonComponent button1;
	public ButtonWithTextComponent button2;
	public ButtonHoverComponent button3;
	public ButtonWithTipComponent button4;

	public override void OnInit() {
		
		base.OnInit();

		this.button1.SetCallback(() => {

			Debug.Log("Button 1 was clicked");

		});

		this.button2.SetCallback(() => {

			var rnd = new string[] { "Hello", "World", "Nothing", "To", "Do" };
			rnd = rnd.OrderBy((elem) => Guid.NewGuid()).ToArray();

			this.button2.SetText(string.Join(" ", rnd));

		});

		var oldColor = this.button3.GetColor();
		this.button3.SetCallbackHover((state) => {

			if (state == true) {

				this.button3.SetColor(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f));

			} else {

				this.button3.SetColor(oldColor);

			}

		});

		this.button4.SetTextToTip("Tip text may be set here");

	}

}
