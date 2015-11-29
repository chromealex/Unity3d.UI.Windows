using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Types;

public class UIWindowExampleComponents : LayoutWindowType {

	public WindowComponent[] components;

	private Tabs tabs;
	private WindowLayoutElement content;

	public override void OnInit() {

		base.OnInit();

		this.content = this.GetLayoutContainer(LayoutTag.Tag2);

		this.tabs = this.GetLayoutComponent<Tabs>();
		this.tabs.SetContent(this.content);

		var i = 0;
		foreach (var component in this.components) {

			var button = this.tabs.AddItem<ButtonComponent, WindowComponent>(component);
			button.SetText((++i).ToString() + ". " + component.name);

		}

	}

	public override void OnShowBegin(System.Action callback, bool resetAnimation = true) {

		base.OnShowBegin(callback, resetAnimation);
		
		this.tabs.Load(0, immediately: true);

	}

}
