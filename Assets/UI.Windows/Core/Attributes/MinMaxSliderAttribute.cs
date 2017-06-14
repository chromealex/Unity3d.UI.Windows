using System;
using UnityEngine;

public class MinMaxSliderAttribute : ReadOnlyAttribute {

	public readonly float max;
	public readonly float min;

	public MinMaxSliderAttribute(float min, float max, string prop, object state) : base(prop, state) {
		this.min = min;
		this.max = max;
	}
}