using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleComponentsEx7 : WindowComponent {

	public LinkerComponent popupLinker;
	private PopupComponent popup;

	public override void OnInit() {
		
		base.OnInit();

		this.popupLinker.Get(ref this.popup);

		this.popup.SetItems<ButtonComponent>(10, (element, index) => {

			element.SetText("Item " + index.ToString());

		});

		this.popup.Select(0, closePopup: false);

	}

}
