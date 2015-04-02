using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Types;

public class UIWindowExampleTip : TipWindowType {
	
	public Text text;

	public void OnParametersPass(string text) {

		this.text.text = text;

	}

}
