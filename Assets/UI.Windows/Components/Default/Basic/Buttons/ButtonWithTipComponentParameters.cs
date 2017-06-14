using UnityEngine;
using System;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.UI.Windows.Plugins.Localization;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonWithTipComponentParameters : ButtonComponentParameters {

		[Header("Button: Tip")]
		[ParamFlag(ParameterFlag.P13)] public LocalizationKey keyTooltipNormal;
		[ParamFlag(ParameterFlag.P14)] public LocalizationKey keyTooltipDisabled;
		[ParamFlag(ParameterFlag.P15)] public UnityEngine.UI.Windows.Types.TipWindowType.ShowPriority tipShowPriority = UnityEngine.UI.Windows.Types.TipWindowType.ShowPriority.Up;

		public override void Setup(IButtonComponent component) {

			var tipButton = component as ButtonWithTipComponent;
			if (tipButton == null) return;

			if (this.IsChanged(ParameterFlag.P13) == true) tipButton.keyTooltipNormal = this.keyTooltipNormal;
			if (this.IsChanged(ParameterFlag.P14) == true) tipButton.keyTooltipDisabled = this.keyTooltipDisabled;
			if (this.IsChanged(ParameterFlag.P15) == true) tipButton.tipShowPriority = this.tipShowPriority;

		}

	}

}