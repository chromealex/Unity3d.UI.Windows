using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

public class ButtonAnimatedComponent : WindowLayoutBase {

	public Button button;
	public WindowComponent animatedComponent;

	public void Start() {

		this.Setup(this.animatedComponent);

	}

	public void OnHover() {

		if (this.button.interactable == false) return;

		this.Show(resetAnimation: false);

	}

	public void OnLeave() {

		this.Hide();

	}

}
