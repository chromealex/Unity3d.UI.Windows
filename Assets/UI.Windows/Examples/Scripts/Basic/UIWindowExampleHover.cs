using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;

public class UIWindowExampleHover : MonoBehaviour {

	public string text;

	private TipWindowType infoWindow;

	public void OnHover() {
		
		this.infoWindow = WindowSystem.Show<UIWindowExampleTip>(this.text) as TipWindowType;
		this.infoWindow.OnHover(this.transform as RectTransform);
		
	}
	
	public void OnLeave() {
		
		if (this.infoWindow != null) {

			this.infoWindow.OnLeave();
			this.infoWindow.Hide();
			
		}
		
	}

}
