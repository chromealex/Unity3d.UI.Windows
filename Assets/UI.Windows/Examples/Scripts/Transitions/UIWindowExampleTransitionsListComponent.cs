using UnityEngine;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleTransitionsListComponent : ListComponent {
	
	public override void OnInit() {

		base.OnInit();

		var window = this.GetWindow() as UIWindowExampleTransitions;
		foreach (var winSource in window.windows) {
			
			var item = this.AddItem<UIWindowExampleTransitionsListItemComponent>();
			item.SetText(winSource.name);
			item.SetCallback(this.OnClick);
			
		}
		
	}
	
	private void OnClick(ButtonComponent button) {

		var index = this.GetIndexOf(button);
		if (index < 0) return;
		
		var window = this.GetWindow() as UIWindowExampleTransitions;
		window.ShowWindow(index);
		
	}

}
