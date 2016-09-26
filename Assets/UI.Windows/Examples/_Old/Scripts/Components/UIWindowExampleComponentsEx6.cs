using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleComponentsEx6 : WindowComponent {
	
	public LinkerComponent buttonValueLinker;
	private ButtonComponent buttonValue;
	
	public LinkerComponent buttonColorLinker;
	private ButtonComponent buttonColor;

	public LinkerComponent[] bars;

	public override void OnInit() {
		
		base.OnInit();

		this.buttonValueLinker.Get(ref this.buttonValue).SetText("Randomize value");
		this.buttonValueLinker.Get(ref this.buttonValue).SetCallback(() => {
			
			this.SetRandomValue();
			
		});
		
		this.buttonColorLinker.Get(ref this.buttonColor).SetText("Randomize color");
		this.buttonColorLinker.Get(ref this.buttonColor).SetCallback(() => {
			
			this.SetRandomColor();
			
		});

		this.SetRandomValue();

	}
	
	private void SetRandomValue() {
		
		foreach (var bar in this.bars) {
			
			bar.Get<ProgressComponent>().SetValue(Random.Range(0f, 1f));
			
		}
		
	}
	
	private void SetRandomColor() {
		
		foreach (var bar in this.bars) {
			
			bar.Get<ProgressComponent>().SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f));
			
		}
		
	}

}
