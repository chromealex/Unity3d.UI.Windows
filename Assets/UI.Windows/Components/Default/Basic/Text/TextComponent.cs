using UnityEngine;
using System;
using System.Text.RegularExpressions;
using ME;

namespace UnityEngine.UI.Windows.Components {

	public class TextComponent : ColoredComponent, ITextComponent {

		public const int MAX_CHARACTERS_COUNT = 2000;

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
		[BeginGroup]
		[SerializeField]
		private MaskableGraphic text;
		[SerializeField]
		private TextValueFormat valueFormat;
		[SerializeField][BitMask(typeof(FullTextFormat))]
		private FullTextFormat fullTextFormat;
		[SerializeField][BitMask(typeof(RichTextFlags))]
		private RichTextFlags richTextFlags = RichTextFlags.Color | RichTextFlags.Bold | RichTextFlags.Italic | RichTextFlags.Size | RichTextFlags.Material | RichTextFlags.Quad;

		public UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey textLocalizationKey;
		
		[SerializeField]
		private bool valueAnimate = false;
		[SerializeField][EndGroupReadOnly("valueAnimate", state: false)]
		private float valueAnimateDuration = 2f;
		private long valueAnimateLastValue;
		private long tempLastValue = long.MinValue;
		private TextValueFormat tempLastFormat = TextValueFormat.None;

		public ITextComponent SetBestFit(bool state, int minSize = 10, int maxSize = 40) {
			
			if (this.text != null) {
				
				this.SetBestFitState(state);
				this.SetBestFitMinSize(minSize);
				this.SetBestFitMaxSize(maxSize);
				
			}

			return this;

		}

		public ITextComponent SetTextLocalizationKey(UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey key) {

			this.textLocalizationKey = key;

			return this;

		}

		public ITextComponent SetBestFitState(bool state) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetBestFitState(this.text, state);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetBestFitState(this.text, state);

			return this;

		}
		
		public ITextComponent SetBestFitMinSize(int size) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetBestFitMinSize(this.text, size);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetBestFitMinSize(this.text, size);

			return this;

		}
		
		public ITextComponent SetBestFitMaxSize(int size) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetBestFitMaxSize(this.text, size);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetBestFitMaxSize(this.text, size);

			return this;

		}

		public int GetFontSize() {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) return TextComponentUGUIAddon.GetFontSize(this.text);
			if (TextComponentTMPAddon.IsValid(this.text) == true) return TextComponentTMPAddon.GetFontSize(this.text);

			return 0;
			
		}

		public ITextComponent SetFontSize(int fontSize) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetFontSize(this.text, fontSize);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetFontSize(this.text, fontSize);

			return this;

		}

		public ITextComponent SetLineSpacing(float value) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetLineSpacing(this.text, value);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetLineSpacing(this.text, value);

			return this;

		}
		
		public ITextComponent SetRichText(bool state) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetRichText(this.text, state);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetRichText(this.text, state);

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

			if (TextComponentUGUIAddon.IsValid(this.text) == true) return TextComponentUGUIAddon.GetContentHeight(this.text, text, containerSize);
			if (TextComponentTMPAddon.IsValid(this.text) == true) return TextComponentTMPAddon.GetContentHeight(this.text, text, containerSize);

			return 0f;

		}

		public ITextComponent SetTextAlignByGeometry(bool state) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetTextAlignByGeometry(this.text, state);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetTextAlignByGeometry(this.text, state);

			return this;

		}

		public ITextComponent SetTextVerticalOverflow(VerticalWrapMode mode) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetTextVerticalOverflow(this.text, mode);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetTextVerticalOverflow(this.text, mode);

			return this;

		}

		public ITextComponent SetTextHorizontalOverflow(HorizontalWrapMode mode) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetTextHorizontalOverflow(this.text, mode);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetTextHorizontalOverflow(this.text, mode);

			return this;

		}

		public ITextComponent SetTextAlignment(TextAnchor anchor) {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetTextAlignment(this.text, anchor);
			if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetTextAlignment(this.text, anchor);

			return this;

		}

		public string GetText() {

			if (TextComponentUGUIAddon.IsValid(this.text) == true) return TextComponentUGUIAddon.GetText(this.text);
			if (TextComponentTMPAddon.IsValid(this.text) == true) return TextComponentTMPAddon.GetText(this.text);

			return string.Empty;

		}

		public ITextComponent SetValueAnimate(bool state) {
			
			this.valueAnimate = state;
			
			return this;
			
		}
		
		public ITextComponent SetValueAnimateDuration(float duration) {
			
			this.valueAnimateDuration = duration;
			
			return this;
			
		}

		public ITextComponent SetFullTextFormat(FullTextFormat format) {
			
			this.fullTextFormat = format;
			
			return this;
			
		}

		public ITextComponent SetValueFormat(TextValueFormat format) {

			this.valueFormat = format;

			return this;

		}
		
		public ITextComponent SetValue(int value) {
			
			this.SetValue(value, this.valueFormat);
			
			return this;
			
		}
		
		public ITextComponent SetValue(int value, TextValueFormat format) {
			
			return this.SetValue(value, format, this.valueAnimate);
			
		}
		
		public ITextComponent SetValue(int value, TextValueFormat format, bool animate) {
			
			return this.SetValue((long)value, format, animate);
			
		}

		public ITextComponent SetValue(long value) {
			
			this.SetValue(value, this.valueFormat);

			return this;

		}

		public ITextComponent SetValue(long value, TextValueFormat format) {

			return this.SetValue(value, format, this.valueAnimate);

		}

		public ITextComponent SetValue(long value, TextValueFormat format, bool animate) {

			return this.SetValue_INTERNAL(value, format, animate, fromTweener: false);

		}

		private ITextComponent SetValue_INTERNAL(long value, TextValueFormat format, bool animate, bool fromTweener) {

			if (fromTweener == true && this.tempLastValue == value && this.tempLastFormat == format) return this;
			this.tempLastValue = value;
			this.tempLastFormat = format;

			var tag = this.GetCustomTag(this.text);
			if (fromTweener == false && TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(tag);

			if (animate == true && TweenerGlobal.instance != null) {

				this.tempLastValue = value - 1L;

				var duration = this.valueAnimateDuration;
				if (Mathf.Abs(this.valueAnimateLastValue - value) < 2) duration = 0.2f;
				TweenerGlobal.instance.addTweenCount(this, duration, this.valueAnimateLastValue, value, format, (v) => { this.valueAnimateLastValue = v; this.SetValue_INTERNAL(v, format, animate: false, fromTweener: true); }).tag(tag);
				
			} else {
				
				this.SetText_INTERNAL(TextComponent.FormatValue(value, format));
				
			}
			
			return this;

		}

		public ITextComponent SetHyphenSymbol() {

			return this.SetText("\u2014");

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
			
			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(this);

			this.SetText_INTERNAL(text);

			return this;

		}

		private void SetText_INTERNAL(string text) {

			if (text == null) text = string.Empty;

			if (this.text != null) {

				bool supportRichText = false;
				if (TextComponentUGUIAddon.IsValid(this.text) == true) supportRichText = TextComponentUGUIAddon.IsRichtextSupported(this.text);
				if (TextComponentTMPAddon.IsValid(this.text) == true) supportRichText = TextComponentTMPAddon.IsRichtextSupported(this.text);

				if (supportRichText == true) {
					
					text = TextComponent.ParseRichText(text, this.GetFontSize(), this.richTextFlags);
					
				}
				
				text = TextComponent.FullTextFormat(text, this.fullTextFormat);

				if (TextComponentUGUIAddon.IsValid(this.text) == true) TextComponentUGUIAddon.SetText(this.text, text);
				if (TextComponentTMPAddon.IsValid(this.text) == true) TextComponentTMPAddon.SetText(this.text, text);

			}

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
			if (this.text == null) this.text = ME.Utilities.FindReferenceChildren<Text>(this);
			#endregion

		}
		#endif

		public static string FullTextFormat(string text, FullTextFormat flags) {
			
			if ((flags & UnityEngine.UI.Windows.Components.FullTextFormat.LowerAll) != 0) {
				
				text = text.ToLower();
				
			}
			
			if ((flags & UnityEngine.UI.Windows.Components.FullTextFormat.UpperAll) != 0) {
				
				text = text.ToUpper();
				
			}
			
			if ((flags & UnityEngine.UI.Windows.Components.FullTextFormat.TrimLeft) != 0 ||
			    (flags & UnityEngine.UI.Windows.Components.FullTextFormat.Trim) != 0) {
				
				text = text.TrimStart();
				
			}
			
			if ((flags & UnityEngine.UI.Windows.Components.FullTextFormat.TrimRight) != 0 ||
			    (flags & UnityEngine.UI.Windows.Components.FullTextFormat.Trim) != 0) {
				
				text = text.TrimEnd();
				
			}
			
			if ((flags & UnityEngine.UI.Windows.Components.FullTextFormat.Percent) != 0) {
				
				text = string.Format("{0}%", text);
				
			}

			if ((flags & UnityEngine.UI.Windows.Components.FullTextFormat.UpperFirstLetter) != 0) {
				
				text = text.UppercaseFirst();
				
			}
			
			if ((flags & UnityEngine.UI.Windows.Components.FullTextFormat.UppercaseWords) != 0) {
				
				text = text.UppercaseWords();
				
			}

			return text;

		}

		private const string RICHTEXT_B_BEGIN = @"<b>";
		private const string RICHTEXT_B_END = @"</b>";

		private const string RICHTEXT_I_BEGIN = @"<i>";
		private const string RICHTEXT_I_END = @"</i>";

		private const string RICHTEXT_SIZE_BEGIN = @"<size=[0-9]+>";
		private const string RICHTEXT_SIZE_END = @"</size>";
		private const string RICHTEXT_SIZEPERCENT = @"<size=(?<Percent>[0-9]+)%>";
		private const string RICHTEXT_SIZEPERCENT_GROUP = @"Percent";
		private const string RICHTEXT_SIZEPERCENT_GROUPRESULT = @"<size={0}>";

		private const string RICHTEXT_COLOR_BEGIN = @"<color=[^>]+>";
		private const string RICHTEXT_COLOR_END = @"</color>";

		private const string RICHTEXT_MATERIAL_BEGIN = @"<material=[^>]+>";
		private const string RICHTEXT_MATERIAL_END = @"</material>";

		private const string RICHTEXT_QUAD = @"<quad [^>]+>";

		public static string ParseRichText(string text, int fontSize, RichTextFlags flags) {

			if (text == null) return text;

			if ((flags & RichTextFlags.Bold) == 0) {
				
				text = Regex.Replace(text, TextComponent.RICHTEXT_B_BEGIN, string.Empty);
				text = Regex.Replace(text, TextComponent.RICHTEXT_B_END, string.Empty);
				
			}
			
			if ((flags & RichTextFlags.Italic) == 0) {
				
				text = Regex.Replace(text, TextComponent.RICHTEXT_I_BEGIN, string.Empty);
				text = Regex.Replace(text, TextComponent.RICHTEXT_I_END, string.Empty);
				
			}
			
			if ((flags & RichTextFlags.Size) == 0) {

				text = Regex.Replace(text, TextComponent.RICHTEXT_SIZE_BEGIN, string.Empty);
				text = Regex.Replace(text, TextComponent.RICHTEXT_SIZE_END, string.Empty);
				
			} else {

				text = Regex.Replace(text, TextComponent.RICHTEXT_SIZEPERCENT, new MatchEvaluator((Match match) => {

					var value = match.Groups[TextComponent.RICHTEXT_SIZEPERCENT_GROUP].Value;
					var newValue = value;

					float fValue;
					if (float.TryParse(value, out fValue) == true) {

						newValue = ((int)(fontSize * (fValue / 100f))).ToString();

					}

					return string.Format(TextComponent.RICHTEXT_SIZEPERCENT_GROUPRESULT, newValue);

				}));

			}
			
			if ((flags & RichTextFlags.Color) == 0) {
				
				text = Regex.Replace(text, TextComponent.RICHTEXT_COLOR_BEGIN, string.Empty);
				text = Regex.Replace(text, TextComponent.RICHTEXT_COLOR_END, string.Empty);
				
			}
			
			if ((flags & RichTextFlags.Material) == 0) {
				
				text = Regex.Replace(text, TextComponent.RICHTEXT_MATERIAL_BEGIN, string.Empty);
				text = Regex.Replace(text, TextComponent.RICHTEXT_MATERIAL_END, string.Empty);
				
			}
			
			if ((flags & RichTextFlags.Quad) == 0) {
				
				text = Regex.Replace(text, TextComponent.RICHTEXT_QUAD, string.Empty);
				
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

					var minus = false;
					if (value < 0f) {

						value = -value;
						minus = true;
						

					}
							
					output = value.ToString("# ### ### ##0").Trim();

					if (minus == true) {
						
						output = string.Format("-{0}", value);
						
					}
					
					break;

				}
				
				case TextValueFormat.WithComma: {

					var minus = false;
					if (value < 0f) {

						value = -value;
						minus = true;


					}
						
					output = value.ToString("#,### ### ##0").Trim(',').Trim();
					
					if (minus == true) {

						output = string.Format("-{0}", value);

					}

					break;

				}

				case TextValueFormat.DateUniversalFromMilliseconds: {

					DateTime date = TextComponent.TimeStampToDateTime(value, milliseconds: true);
					output = date.ToString("U", System.Globalization.CultureInfo.CreateSpecificCulture(UnityEngine.UI.Windows.Plugins.Localization.LocalizationSystem.GetCultureNameByLanguage(UnityEngine.UI.Windows.Plugins.Localization.LocalizationSystem.GetCurrentLanguage())));

					break;

				}

				case TextValueFormat.DateDMHMS: {

					DateTime date = TextComponent.TimeStampToDateTime(value, milliseconds: false);
					output = date.ToString("dd MM HH:mm:ss");

					break;

				}

				case TextValueFormat.DateDMHMSFromMilliseconds: {
						
					DateTime date = TextComponent.TimeStampToDateTime(value, milliseconds: true);
					output = date.ToString("dd MM HH:mm:ss");

					break;

				}

				case TextValueFormat.TimeHMSFromSeconds: {

					var t = TimeSpan.FromSeconds(value);
					output = string.Format("{0:D2}:{1:D2}:{2:D2}", Mathf.FloorToInt((float)t.TotalHours), t.Minutes, t.Seconds);
					
					break;

				}
				
				case TextValueFormat.TimeMSFromSeconds: {

					var t = TimeSpan.FromSeconds(value);
					output = string.Format("{0:D2}:{1:D2}", Mathf.FloorToInt((float)t.TotalMinutes), t.Seconds);
					
					break;

				}

				case TextValueFormat.TimeHMSmsFromMilliseconds: {

					var t = TimeSpan.FromMilliseconds(value);
					output = string.Format("{0:D2}:{1:D2}:{2:D2}`{3:D2}", Mathf.FloorToInt((float)t.TotalHours), t.Minutes, t.Seconds, t.Milliseconds);
					
					break;

				}
				
				case TextValueFormat.TimeMSmsFromMilliseconds: {

					var t = TimeSpan.FromMilliseconds(value);
					output = string.Format("{0:D2}:{1:D2}`{2:D2}", Mathf.FloorToInt((float)t.TotalMinutes), t.Seconds, t.Milliseconds);
					
					break;

				}

				case TextValueFormat.TimeMSFromMilliseconds: {
					
					var t = TimeSpan.FromMilliseconds(value);
					output = string.Format("{0:D2}:{1:D2}", Mathf.FloorToInt((float)t.TotalMinutes), t.Seconds);

					break;

				}
					
				case TextValueFormat.TimeHMSFromMilliseconds: {
					
					var t = TimeSpan.FromMilliseconds(value);
					output = string.Format("{0:D2}:{1:D2}:{2:D2}", Mathf.FloorToInt((float)t.TotalHours), t.Minutes, t.Seconds);
					
					break;
					
				}

			}
			
			return output;
			
		}

		public static DateTime TimeStampToDateTime(double unixTime, bool milliseconds) {
			
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(milliseconds == true ? System.Math.Round(unixTime / 1000d) : unixTime).ToLocalTime();
			return dtDateTime;

		}

	}

}