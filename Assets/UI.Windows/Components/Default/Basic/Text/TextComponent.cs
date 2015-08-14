using UnityEngine;
using System;

namespace UnityEngine.UI.Windows.Components {

	public class TextComponent : ColoredComponent, ITextComponent {

		public enum ValueFormat : byte {

			None,		 // 1234567890
			WithSpace,	 // 1 234 567 890
			WithComma,	 // 1,234 567 890
			Time,		 // 00:00:00
			TimeMinutes, // 00:00
			TimeMiliseconds, // 00:00:00`00
			TimeMinutesMiliseconds, // 00:00`00

		};

		[SerializeField]
		private Text text;

		public static string FormatValue(int value, ValueFormat format) {
			
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
					
				case ValueFormat.Time:

					{
						var t = TimeSpan.FromSeconds(value);
						output = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
					}

					break;
					
				case ValueFormat.TimeMinutes:
					
					{
						var t = TimeSpan.FromSeconds(value);
						output = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
					}

					break;
					
				case ValueFormat.TimeMiliseconds:

					{
						var t = TimeSpan.FromMilliseconds(value);
						output = string.Format("{0:D2}:{1:D2}:{2:D2}`{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
					}

					break;
					
				case ValueFormat.TimeMinutesMiliseconds:

					{
						var t = TimeSpan.FromMilliseconds(value);
						output = string.Format("{0:D2}:{1:D2}`{2:D2}", t.Minutes, t.Seconds, t.Milliseconds);
					}

					break;

			}

			return output;

		}
		
		public void SetValue(int value, ValueFormat format = ValueFormat.None) {
			
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