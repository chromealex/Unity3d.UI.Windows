using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonWithTextComponent : ButtonComponent {

	public Text text;

	public void SetText(string text) {

		this.text.text = text;

	}

}
