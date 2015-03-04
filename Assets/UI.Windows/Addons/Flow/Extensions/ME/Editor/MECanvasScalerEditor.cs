using UnityEngine;
using System.Collections;

namespace UnityEditor.UI {

	[CustomEditor(typeof(MECanvasScaler), true)]
	[CanEditMultipleObjects]
	public class MECanvasScalerEditor : CanvasScalerEditor {

		protected override void OnEnable() {

			if (serializedObject != null) {

				base.OnEnable();

			}

		}

	}

}