using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Extensions;

public class UIWindowExampleTransitionsListComponent : WindowComponent {

	public ButtonWithTextComponent source;

	private List<ButtonWithTextComponent> buttons = new List<ButtonWithTextComponent>();

	public override void OnInit() {

		this.buttons.Clear();

		var window = this.GetWindow() as UIWindowExampleTransitions;
		foreach (var winSource in window.windows) {

			var button = this.source.Spawn();
			button.SetText(winSource.name);
			button.SetCallback(this.OnClick);
			button.gameObject.SetActive(true);

			this.buttons.Add(button);

		}

	}

	private void OnClick(ButtonComponent button) {

		var index = this.buttons.IndexOf(button as ButtonWithTextComponent);
		if (index < 0) return;
		
		var window = this.GetWindow() as UIWindowExampleTransitions;
		window.ShowWindow(index);

	}

}
