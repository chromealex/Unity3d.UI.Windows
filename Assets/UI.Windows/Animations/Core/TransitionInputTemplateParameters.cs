using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Animations {

	public class TransitionInputTemplateParameters : TransitionInputParameters {

		#if UNITY_EDITOR
		[Header("Transition Template Parameters (Editor-only)")]
		public bool useAsTemplate = false;
		[ReadOnly(fieldName: "useAsTemplate", state: false)]
		public TransitionBase transition;
		#endif

	}

}