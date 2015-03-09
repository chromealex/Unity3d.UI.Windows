using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleButtonComponent : ButtonComponent {

	public UIWindowExampleHover hover;

	public override void OnHideBegin() {

		base.OnHideBegin();
		this.hover.OnLeave();

	}

	public void SetTextToTip(string text) {

		this.hover.text = text;

	}

}
