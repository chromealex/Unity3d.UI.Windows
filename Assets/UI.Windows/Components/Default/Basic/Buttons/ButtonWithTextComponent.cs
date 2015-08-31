using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class ButtonWithTextComponent : ButtonWithImageComponent, ITextComponent {
		
		[Header("Text (Optional)")]

		public Text text;
		
		public float GetContentHeight(string text, Vector2 containerSize) {
			
			var settings = this.text.GetGenerationSettings(containerSize);
			return this.text.cachedTextGenerator.GetPreferredHeight(text, settings);
			
		}
		
		public void SetValue(int value, UnityEngine.UI.Windows.Components.TextComponent.ValueFormat format = UnityEngine.UI.Windows.Components.TextComponent.ValueFormat.None) {
			
			this.SetText(TextComponent.FormatValue(value, format));
			
		}
		
		public void SetTextVerticalOverflow(VerticalWrapMode mode) {
			
			if (this.text != null) this.text.verticalOverflow = mode;
			
		}
		
		public void SetTextHorizontalOverflow(HorizontalWrapMode mode) {
			
			if (this.text != null) this.text.horizontalOverflow = mode;
			
		}
		
		public void SetTextAlignment(TextAnchor anchor) {
			
			if (this.text != null) this.text.alignment = anchor;
			
		}
		
		public string GetText() {
			
			return (this.text != null) ? this.text.text : string.Empty;
			
		}
		
		public void SetText(string text) {
			
			if (this.text != null) this.text.text = text;
			
		}
		
		public virtual void SetTextColor(Color color) {
			
			this.text.color = color;
			
		}
		
		public virtual Color GetTextColor() {
			
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