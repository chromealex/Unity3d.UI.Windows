using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

	public class TextComponent : WindowComponent, ITextComponent {

		public enum ValueFormat : byte {

			None,		// 1234567890
			WithSpace,	// 1,234 567 890
			WithComma,	// 1 234 567 890

		};

		[SerializeField]
		private Text text;

		public void SetValue(int value, ValueFormat format = ValueFormat.None) {

			var output = string.Empty;

			switch (format) {
				
			case ValueFormat.None:
				output = value.ToString();
				break;
				
			case ValueFormat.WithSpace:
				output = value.ToString("# ### ### ##0").Trim();
				break;
				
			case ValueFormat.WithComma:
				output = value.ToString("#,### ### ##0").Trim(',');
				break;

			}

			this.SetText(output);

		}

		public string GetText() {

			return (this.text != null) ? this.text.text : string.Empty;

		}

		public void SetText(string text) {

			if (this.text != null) this.text.text = text;

		}
		
		public void SetColor(Color color) {
			
			this.text.color = color;
			
		}
		
		public Color GetColor() {
			
			return this.text.color;
			
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