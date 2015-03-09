using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System;

public class UIWindowExampleComponentsEx3 : WindowComponent {
	
	public ImageComponent image;
	public TextComponent text;

	public override void OnInit() {
		
		base.OnInit();

		this.text.SetText(this.text.GetText() + " (This text was added from the code)");

	}

}
