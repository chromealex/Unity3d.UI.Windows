using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleComponentsEx8 : WindowComponent {
	
	public LinkerComponent radioLinker;
	private ToggleComponent radio;
	
	public LinkerComponent checkboxLinker;
	private ToggleComponent checkbox;

	public LinkerComponent popupLinker;
	private PopupComponent popup;

	public override void OnInit() {
		
		base.OnInit();
		
		this.popupLinker.Get(ref this.popup);
		this.popup.SetItems<ButtonComponent>(10, (element, index) => {
			
			element.SetText("Item " + index.ToString());
			
		});
		this.popup.Select(0, closePopup: false);
		
		this.radioLinker.Get(ref this.radio);
		this.radio.SetItems<ToggleItemComponent>(4, (element, index) => {
			
			element.SetText("Variant " + (index + 1).ToString());
			
		});
		this.radio.Select(0);
		
		this.checkboxLinker.Get(ref this.checkbox);
		var check = this.checkbox.AddItem<ToggleItemComponent>();
		check.SetText("Checkbox");
		this.checkbox.Select(0);

	}

}
