using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonWithTextComponent : ButtonWithImageComponent, ITextComponent {
		
		[Header("Text (Optional)")]

		public Text text;

		public void SetValue(int value, TextComponent.ValueFormat format = TextComponent.ValueFormat.None) {

			this.SetText(TextComponent.FormatValue(value, format));
			
		}
		
		public string GetText() {
			
			return (this.text != null) ? this.text.text : string.Empty;
			
		}
		
		public void SetText(string text) {
			
			if (this.text != null) this.text.text = text;
			
		}
		
		#if UNITY_EDITOR
		public override void OnValidateEditor() {
			
			base.OnValidateEditor();
			
			if (this.gameObject.activeSelf == false) return;

			var texts = this.GetComponentsInChildren<Text>(true);
			if (texts.Length == 1) this.text = texts[0];

		}
		#endif

	}

}