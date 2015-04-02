using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;

public class UIWindowExampleTransitions : LayoutWindowType {

	public WindowBase[] windows;

	public void ShowWindow(int index) {

		WindowSystem.Show(this.windows[index], "This is <b>" + this.windows[index].name + "</b> transition. Close this window and choose another one.");

	}

}
