using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Types {

	public class TextTipWindowType : TipWindowType {
		
		public Text text;

		public void OnParametersPass(string text) {

			this.text.text = text;

		}

	}

}