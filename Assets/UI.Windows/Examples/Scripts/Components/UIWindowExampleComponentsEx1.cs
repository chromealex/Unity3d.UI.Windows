using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System;

public class UIWindowExampleComponentsEx1 : WindowComponent {
	
	public LinkerComponent button1Linker;
	private ButtonComponent button1;
	
	public LinkerComponent button2Linker;
	private ButtonComponent button2;
	
	public LinkerComponent button3Linker;
	private ButtonComponent button3;
	
	public LinkerComponent button4Linker;
	private ButtonWithTipComponent button4;

	public override void OnInit() {
		
		base.OnInit();

		this.button1Linker.Get(ref this.button1).SetCallback(() => {

			Debug.Log("Button 1 was clicked");

		});

		this.button2Linker.Get(ref this.button2).SetText("Click to change text");
		this.button2Linker.Get(ref this.button2).SetCallback(() => {

			var rnd = new string[] { "Hello", "World", "Nothing", "To", "Do" };
			rnd = rnd.OrderBy((elem) => Guid.NewGuid()).ToArray();

			this.button2Linker.Get(ref this.button2).SetText(string.Join(" ", rnd));

		});

		this.button3Linker.Get(ref this.button3);
		var oldColor = this.button3.GetColor();
		this.button3.SetText("Hover to change color");
		this.button3.SetCallbackHover((state) => {

			if (state == true) {

				this.button3.SetColor(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f));

			} else {

				this.button3.SetColor(oldColor);

			}

		});

		this.button4Linker.Get(ref this.button4).SetText("Button with tooltip");
		this.button4Linker.Get(ref this.button4).SetTextToTip("Tip text may be set here");

	}

}
