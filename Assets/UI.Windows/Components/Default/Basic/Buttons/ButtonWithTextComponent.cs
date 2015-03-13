using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonWithTextComponent : ButtonComponent, ITextComponent {

		public Text text;

		public void SetText(string text) {
			
			this.text.text = text;
			
		}

		public string GetText() {
			
			return this.text.text;
			
		}
		
		#if UNITY_EDITOR
		public override void OnValidate() {
			
			base.OnValidate();
			
			if (this.gameObject.activeSelf == false) return;

			var texts = this.GetComponentsInChildren<Text>(true);
			if (texts.Length == 1) this.text = texts[0];

		}
		#endif

	}

}