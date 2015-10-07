using UnityEngine;
using System;

namespace UnityEngine.UI.Windows.Components {

	public class ImageComponentParameters : WindowComponentParametersBase {

		#region source macros UI.Windows.ImageComponentParameters
		[Header("Image: Base")]
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P1)] public bool preserveAspect;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P2)] public Sprite image;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P3)] public Texture rawImage;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P4)] public Color imageColor = Color.white;

		public void Setup(IImageComponent component) {
			
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P1) == true) component.SetPreserveAspectState(this.preserveAspect);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P2) == true) component.SetImage(this.image);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P3) == true) component.SetImage(this.rawImage);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P4) == true) component.SetColor(this.imageColor);

		}
		#endregion

	}

}