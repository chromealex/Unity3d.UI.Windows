using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System;

public class UIWindowExampleComponentsEx5 : WindowComponent {
	
	public LinkerComponent textLinker;
	private InputFieldComponent text;

	public LinkerComponent passwordLinker;
	private InputFieldComponent password;

	public LinkerComponent emailLinker;
	private InputFieldComponent email;

	public LinkerComponent phoneLinker;
	private InputFieldComponent phone;

	public override void OnInit() {
		
		base.OnInit();

		this.textLinker.Get(ref this.text).SetText("Text field default text");
		this.passwordLinker.Get(ref this.password).SetText("password");
		this.emailLinker.Get(ref this.email).SetText("example@email.com");
		this.phoneLinker.Get(ref this.phone).SetText("89112345678");

	}

}
