using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System;

public class UIWindowExampleComponentsEx5 : WindowComponent {
	
	public LinkerComponent textLinker;
	private InputFieldComponent text;

	public LinkerComponent indicatorLinker;
	private InputFieldComponent indicator;

	public override void OnInit() {
		
		base.OnInit();

		this.textLinker.Get(ref this.text).SetText("Default text field");
		this.indicatorLinker.Get(ref this.indicator).SetText("Field with indicator");

	}

}
