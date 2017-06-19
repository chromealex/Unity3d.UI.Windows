using UnityEngine;
using uiws = UnityEngine.UI.Windows;
using uiwc = UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Components {

	public class CheckboxButtonComponentParameters : uiwc::ButtonComponentParameters {
		
		[Header("Checkbox: Base")]
		[uiwc::ParamFlag(uiwc::ParameterFlag.P20)] public bool @checked = true;

		public override void Setup(uiwc::IButtonComponent component) {

			base.Setup(component);

			if (this.IsChanged(uiwc::ParameterFlag.P20) == true) {

				(component as ICheckboxButtonComponent).SetCheckedState(this.@checked);

			}

		}

	}

}