﻿using UnityEngine;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleComponentsEx4 : WindowComponent {

	public ListComponent list;

	public override void OnInit() {

		base.OnInit();

		this.list.SetItems<TextComponent>(10, (element, i) => {

			element.SetText("List element #" + i.ToString());

		});

	}

}
