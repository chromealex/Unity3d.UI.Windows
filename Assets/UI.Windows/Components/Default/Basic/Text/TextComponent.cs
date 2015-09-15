using UnityEngine;
using System;

namespace UnityEngine.UI.Windows.Components {

	public class TextComponent : ColoredComponent, ITextComponent {

		#region source macros UI.Windows.TextComponent
		[SerializeField]
		private Text text;

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
		#endregion
		
		#if UNITY_EDITOR
		public override void OnValidateEditor() {
			
			base.OnValidateEditor();
			
			if (this.gameObject.activeSelf == false) return;
			
			#region source macros UI.Windows.Editor.TextComponent
			var texts = this.GetComponentsInChildren<Text>(true);
			if (texts.Length == 1) this.text = texts[0];
			#endregion
			
		}
		#endif

		public enum ValueFormat : byte {
			
			None,		 // 1234567890
			WithSpace,	 // 1 234 567 890
			WithComma,	 // 1,234 567 890
			TimeHMS,	 // 00:00:00
			TimeMS, 	 // 00:00
			TimeHMSms, 	 // 00:00:00`00
			TimeMSms, 	 // 00:00`00
			
		};
		
		public static string FormatValue(int value, ValueFormat format) {
			
			return TextComponent.FormatValue((float)value, format);
			
		}
		
		public static string FormatValue(float value, ValueFormat format) {
			
			var output = string.Empty;
			
			switch (format) {
				
				case ValueFormat.None: {
					
					output = value.ToString();
					
					break;

				}
				
				case ValueFormat.WithSpace: {
					
					if (value < 0f) {
						
						output = "-" + (-value).ToString("# ### ### ##0").Trim();
						
					} else {
						
						output = value.ToString("# ### ### ##0").Trim();
						
					}
					
					break;

				}
				
				case ValueFormat.WithComma: {
					
					if (value < 0f) {
						
						output = "-" + (-value).ToString("#,### ### ##0").Trim(',');
						
					} else {
						
						output = value.ToString("#,### ### ##0").Trim(',');
						
					}
					
					break;

				}
				
				case ValueFormat.TimeHMS: {

					var t = TimeSpan.FromSeconds(value);
					output = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
					
					break;

				}
				
				case ValueFormat.TimeMS: {

					var t = TimeSpan.FromSeconds(value);
					output = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
					
					break;

				}
				
				case ValueFormat.TimeHMSms: {

					var t = TimeSpan.FromMilliseconds(value);
					output = string.Format("{0:D2}:{1:D2}:{2:D2}`{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
					
					break;

				}
				
				case ValueFormat.TimeMSms: {

					var t = TimeSpan.FromMilliseconds(value);
					output = string.Format("{0:D2}:{1:D2}`{2:D2}", t.Minutes, t.Seconds, t.Milliseconds);
					
					break;

				}
				
			}
			
			return output;
			
		}

	}

}