//Author: Melang http://forum.unity3d.com/members/melang.593409/
using System;
using System.Collections.Generic;

namespace UnityEngine.UI {

	[AddComponentMenu ("UI/Effects/BestFit Outline", 15)]
	public class BestFitOutline : ME.Shadow {

		private Text foundtext;

		//
		// Constructors
		//
		protected override void Start() {

			base.Start();

			this.foundtext = this.GetComponent<Text>();

		}
		
		//
		// Methods
		//
		public override void ModifyVertices(List<UIVertex> verts) {

			if (!this.IsActive()) {
				return;
			}

			float best_fit_adjustment = 1f;

			if (this.foundtext && this.foundtext.resizeTextForBestFit) {

				best_fit_adjustment = (float)this.foundtext.cachedTextGenerator.fontSizeUsedForBestFit / (this.foundtext.resizeTextMaxSize - 1); //max size seems to be exclusive 
				//Debug.Log("best_fit_adjustment:"+best_fit_adjustment);
			
			}
			
			int start = 0;
			int count = verts.Count;
			base.ApplyShadow(verts, base.effectColor, start, verts.Count, base.effectDistance.x * best_fit_adjustment, base.effectDistance.y * best_fit_adjustment);

			start = count;
			count = verts.Count;
			base.ApplyShadow(verts, base.effectColor, start, verts.Count, base.effectDistance.x * best_fit_adjustment, -base.effectDistance.y * best_fit_adjustment);

			start = count;
			count = verts.Count;
			base.ApplyShadow(verts, base.effectColor, start, verts.Count, -base.effectDistance.x * best_fit_adjustment, base.effectDistance.y * best_fit_adjustment);

			start = count;
			count = verts.Count;
			base.ApplyShadow(verts, base.effectColor, start, verts.Count, -base.effectDistance.x * best_fit_adjustment, -base.effectDistance.y * best_fit_adjustment);

		}

	}

}
