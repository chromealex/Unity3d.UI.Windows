using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

public class UIWindowExampleComponentsEx6 : WindowComponent {

	public LinkerComponent buttonLinker;
	private ButtonWithTextComponent button;
	
	public LinkerComponent barLinker;
	private ProgressComponent bar;
	
	public LinkerComponent barAnimatedLinker;
	private ProgressWithParticlesComponent barAnimated;
	
	public LinkerComponent barAnimated2Linker;
	private ProgressAnimatedComponent barAnimated2;

	public override void OnInit() {
		
		base.OnInit();

		this.buttonLinker.Get(ref this.button).SetText("Randomize!");
		this.buttonLinker.Get(ref this.button).SetCallback(() => {

			this.SetRandom();

		});

		this.SetRandom();

	}

	private void SetRandom() {

		this.barLinker.Get(ref this.bar).SetValue(Random.Range(0f, 1f));
		this.barAnimatedLinker.Get(ref this.barAnimated).SetValue(Random.Range(0f, 1f));
		this.barAnimated2Linker.Get(ref this.barAnimated2).SetValue(Random.Range(0f, 1f));

	}

}
