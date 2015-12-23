using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace UnityEngine.UI.Windows.Components {

	public class TextComponent : ColoredComponent, ITextComponent {
		
		public override void Setup(IComponentParameters parameters) {
			
			base.Setup(parameters);
			
			var inputParameters = parameters as TextComponentParameters;
			#region source macros UI.Windows.Initialization.TextComponent
			{
				
				if (inputParameters != null) inputParameters.Setup(this as ITextComponent);

			}
			#endregion

		}

		public override void OnLocalizationChanged() {

			base.OnLocalizationChanged();

			#region source macros UI.Windows.OnLocalizationChanged.TextComponent
			{

				if (this.lastTextLocalization == true) {

					this.SetText(this.lastTextLocalizationKey, this.lastTextLocalizationParameters);

				} else {

					if (this.text is UnityEngine.UI.Windows.Plugins.Localization.UI.LocalizationText) {

						(this.text as UnityEngine.UI.Windows.Plugins.Localization.UI.LocalizationText).OnLocalizationChanged();

					}

				}

			}
			#endregion

		}

		public override void OnInit() {

			base.OnInit();

			#region source macros UI.Windows.OnInit.TextComponent
			{

				if (this.textLocalizationKey.IsNone() == false) {

					this.SetText(this.textLocalizationKey);

				}

			}
			#endregion

		}

		#region source macros UI.Windows.TextComponent
		[Header("Text Component")]
		[SerializeField]
		private Text text;
		[SerializeField]
		private TextValueFormat valueFormat;
		[SerializeField][BitMask(typeof(RichTextFlags))]
		private RichTextFlags richTextFlags = RichTextFlags.Color | RichTextFlags.Bold | RichTextFlags.Italic | RichTextFlags.Size | RichTextFlags.Material | RichTextFlags.Quad;
		public UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey textLocalizationKey;

		public ITextComponent SetBestFit(bool state, int minSize = 10, int maxSize = 40) {
			
			if (this.text != null) {
				
				this.text.resizeTextForBestFit = state;
				this.text.resizeTextMinSize = minSize;
				this.text.resizeTextMaxSize = maxSize;
				
			}

			return this;

		}
		
		public ITextComponent SetBestFitState(bool state) {
			
			if (this.text != null) this.text.resizeTextForBestFit = state;

			return this;

		}
		
		public ITextComponent SetBestFitMinSize(int size) {
			
			if (this.text != null) this.text.resizeTextMinSize = size;

			return this;

		}
		
		public ITextComponent SetBestFitMaxSize(int size) {
			
			if (this.text != null) this.text.resizeTextMaxSize = size;

			return this;

		}

		public ITextComponent SetFont(Font font) {
			
			if (this.text != null) this.text.font = font;

			return this;

		}

		public ITextComponent SetFontSize(int fontSize) {
			
			if (this.text != null) this.text.fontSize = fontSize;

			return this;

		}

		public ITextComponent SetLineSpacing(float value) {
			
			if (this.text != null) this.text.lineSpacing = value;

			return this;

		}
		
		public ITextComponent SetRichText(bool state) {
			
			if (this.text != null) this.text.supportRichText = state;

			return this;

		}
		
		public ITextComponent SetFontStyle(FontStyle fontStyle) {
			
			if (this.text != null) this.text.fontStyle = fontStyle;

			return this;

		}
		
		public float GetContentHeight(float heightPadding = 0f) {
			
			if (this.text == null) return 0f;
			
			return this.GetContentHeight(this.GetText(), heightPadding);
			
		}
		
		public float GetContentHeight(string text, float heightPadding = 0f) {
			
			if (this.text == null) return 0f;
			
			return this.GetContentHeight(text, (this.transform.root as RectTransform).rect.size) + heightPadding;
			
		}

		public float GetContentHeight(string text, Vector2 containerSize) {

			if (this.text == null) return 0f;

			var settings = this.text.GetGenerationSettings(containerSize);
			return this.text.cachedTextGenerator.GetPreferredHeight(text, settings);

		}

		public ITextComponent SetValueFormat(TextValueFormat format) {

			this.valueFormat = format;

			return this;

		}
		
		public ITextComponent SetValue(long value) {
			
			this.SetValue(value, this.valueFormat);

			return this;

		}
		
		public ITextComponent SetValue(int value) {
			
			this.SetValue(value, this.valueFormat);

			return this;

		}

		public ITextComponent SetValue(long value, TextValueFormat format) {
			
			this.SetText(TextComponent.FormatValue(value, format));

			return this;

		}

		public ITextComponent SetValue(int value, TextValueFormat format) {
			
			this.SetText(TextComponent.FormatValue(value, format));

			return this;

		}

		public ITextComponent SetTextVerticalOverflow(VerticalWrapMode mode) {
			
			if (this.text != null) this.text.verticalOverflow = mode;

			return this;

		}
		
		public ITextComponent SetTextHorizontalOverflow(HorizontalWrapMode mode) {
			
			if (this.text != null) this.text.horizontalOverflow = mode;

			return this;

		}

		public ITextComponent SetTextAlignment(TextAnchor anchor) {
			
			if (this.text != null) this.text.alignment = anchor;

			return this;

		}

		public string GetText() {

			return (this.text != null) ? this.text.text : string.Empty;

		}

		private bool lastTextLocalization = false;
		private Plugins.Localization.LocalizationKey lastTextLocalizationKey;
		private object[] lastTextLocalizationParameters;
		public ITextComponent SetText(Plugins.Localization.LocalizationKey key, params object[] parameters) {

			this.lastTextLocalization = true;
			this.lastTextLocalizationKey = key;
			this.lastTextLocalizationParameters = parameters;

			return this.SetText(Plugins.Localization.LocalizationSystem.Get(key, parameters));

		}

		public ITextComponent SetText(string text) {

			if (this.text != null) {

				if (this.text.supportRichText == true) {

					text = TextComponent.ParseRichText(text, this.richTextFlags);

				}

				this.text.text = text;

			}

			return this;

		}

		public virtual ITextComponent SetTextAlpha(float value) {

			var color = this.GetTextColor();
			color.a = value;
			this.SetTextColor(color);

			return this;

		}

		public virtual ITextComponent SetTextColor(Color color) {
			
			if (this.text != null) this.text.color = color;

			return this;

		}
		
		public virtual Color GetTextColor() {

			if (this.text == null) return Color.white;

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

			if (this.valueFormat != TextValueFormat.None) {

				this.SetValue(999999L);

			}
			#endregion
			
		}
		#endif

		public static string ParseRichText(string text, RichTextFlags flags) {
			
			if ((flags & RichTextFlags.Bold) == 0) {
				
				text = Regex.Replace(text, @"<b>", String.Empty);
				text = Regex.Replace(text, @"</b>", String.Empty);
				
			}
			
			if ((flags & RichTextFlags.Italic) == 0) {
				
				text = Regex.Replace(text, @"<i>", String.Empty);
				text = Regex.Replace(text, @"</i>", String.Empty);
				
			}
			
			if ((flags & RichTextFlags.Size) == 0) {
				
				text = Regex.Replace(text, @"<size=[0-9]+>", String.Empty);
				text = Regex.Replace(text, @"</size>", String.Empty);
				
			}
			
			if ((flags & RichTextFlags.Color) == 0) {
				
				text = Regex.Replace(text, @"<color=[^>]+>", String.Empty);
				text = Regex.Replace(text, @"</color>", String.Empty);
				
			}
			
			if ((flags & RichTextFlags.Material) == 0) {
				
				text = Regex.Replace(text, @"<material=[^>]+>", String.Empty);
				text = Regex.Replace(text, @"</material>", String.Empty);
				
			}
			
			if ((flags & RichTextFlags.Quad) == 0) {
				
				text = Regex.Replace(text, @"<quad [^>]+>", String.Empty);
				
			}

			return text;

		}

		public static string FormatValue(int value, TextValueFormat format) {
			
			return TextComponent.FormatValue((double)value, format);
			
		}
		
		public static string FormatValue(long value, TextValueFormat format) {
			
			return TextComponent.FormatValue((double)value, format);
			
		}

		public static string FormatValue(double value, TextValueFormat format) {
			
			var output = string.Empty;
			
			switch (format) {
				
				case TextValueFormat.None: {
					
					output = value.ToString();
					
					break;

				}
				
				case TextValueFormat.WithSpace: {
					
					if (value < 0f) {
						
						output = string.Format("-{0}", (-value).ToString("# ### ### ##0").Trim());
						
					} else {
						
						output = value.ToString("# ### ### ##0").Trim();
						
					}
					
					break;

				}
				
				case TextValueFormat.WithComma: {
					
					if (value < 0f) {
						
						output = string.Format("-{0}", (-value).ToString("#,### ### ##0").Trim(','));
						
					} else {
						
						output = value.ToString("#,### ### ##0").Trim(',').Trim(' ');
						
					}
					
					break;

				}
				
				case TextValueFormat.DateDMHMS: {

					DateTime date = new DateTime((long)value);
					output = date.ToString("dd MM hh:mm:ss");

					break;

				}

				case TextValueFormat.TimeHMSFromSeconds: {

					var t = TimeSpan.FromSeconds(value);
						output = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
					
					break;

				}
				
				case TextValueFormat.TimeMSFromSeconds: {

					var t = TimeSpan.FromSeconds(value);
						output = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
					
					break;

				}
					
				case TextValueFormat.DateDMHMSFromMilliseconds: {
					
					DateTime date = new DateTime((long)(value / 1000d));
					output = date.ToString("dd MM hh:mm:ss");
					
					break;
					
				}

				case TextValueFormat.TimeHMSmsFromMilliseconds: {

					var t = TimeSpan.FromMilliseconds(value);
						output = string.Format("{0:D2}:{1:D2}:{2:D2}`{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
					
					break;

				}
				
				case TextValueFormat.TimeMSmsFromMilliseconds: {

					var t = TimeSpan.FromMilliseconds(value);
						output = string.Format("{0:D2}:{1:D2}`{2:D2}", t.Minutes, t.Seconds, t.Milliseconds);
					
					break;

				}

				case TextValueFormat.TimeMSFromMilliseconds: {
					
					var t = TimeSpan.FromMilliseconds(value);
					output = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

					break;

				}
				
			}
			
			return output;
			
		}

	}

}