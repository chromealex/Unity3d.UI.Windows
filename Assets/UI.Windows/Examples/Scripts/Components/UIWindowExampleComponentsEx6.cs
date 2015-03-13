using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleComponentsEx6 : WindowComponent {

	public LinkerComponent buttonLinker;
	private ButtonWithTextComponent button;
	
	public LinkerComponent[] bars;

	public override void OnInit() {
		
		base.OnInit();

		this.buttonLinker.Get(ref this.button).SetText("Randomize!");
		this.buttonLinker.Get(ref this.button).SetCallback(() => {

			this.SetRandom();

		});

		this.SetRandom();

	}

	private void SetRandom() {

		foreach (var bar in this.bars) {

			bar.Get<ProgressComponent>().SetValue(Random.Range(0f, 1f));

		}

	}

}
