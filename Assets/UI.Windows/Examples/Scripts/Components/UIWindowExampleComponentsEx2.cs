using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System;

public class UIWindowExampleComponentsEx2 : WindowComponent {
	
	public TextComponent text;

	public override void OnInit() {
		
		base.OnInit();

		this.text.SetText("Simple text. There is nothing to present here :)");

	}

}
