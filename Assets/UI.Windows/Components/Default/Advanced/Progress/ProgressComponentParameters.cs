using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ProgressComponentParameters : WindowComponentParametersBase {

		[Header("Progress: Base")]
		[ParamFlag(ParameterFlag.P1)] public float duration = 1f;
		[ParamFlag(ParameterFlag.P2)] public float minNormalizedValue = 0f;
		
		[Header("Progress: Continious")]
		[ParamFlag(ParameterFlag.P3)] public bool continious;
		[Range(0f, 1f)]
		[ParamFlag(ParameterFlag.P4)] public float continiousWidth = 0.4f;
		[ParamFlag(ParameterFlag.P5)] public float continiousAngleStep = 0f;
		
		[ParamFlag(ParameterFlag.P6)] public ComponentEvent onChanged = new ComponentEvent();
		[Range(0f, 1f)]
		[ParamFlag(ParameterFlag.P7)] public float currentValueNormalized = 0f;

		public void Setup(IProgressComponent component) {
			
			if (this.IsChanged(ParameterFlag.P1) == true) component.SetDuration(this.duration);
			if (this.IsChanged(ParameterFlag.P2) == true) component.SetMinNormalizedValue(this.minNormalizedValue);
			if (this.IsChanged(ParameterFlag.P3) == true) component.SetContiniousState(this.continious);
			if (this.IsChanged(ParameterFlag.P4) == true) component.SetContiniousWidth(this.continiousWidth);
			if (this.IsChanged(ParameterFlag.P5) == true) component.SetContiniousAngleStep(this.continiousAngleStep);
			if (this.IsChanged(ParameterFlag.P6) == true) {

				component.SetCallback((value) => this.onChanged.Invoke());
				
			}
			if (this.IsChanged(ParameterFlag.P7) == true) component.SetValue(this.currentValueNormalized, immediately: true);

		}

	}

}