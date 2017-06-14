using UnityEngine;
using System;

namespace UnityEngine.UI.Windows.Components {

	public class TextComponentParameters : WindowComponentParametersBase {

		#region source macros UI.Windows.TextComponentParameters
		[Header("Text: Base")]
		[Multiline]
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P1)] public string text;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P15)] public UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey localizationKey;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P2)] public Color color = Color.white;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P3)] public TextValueFormat format = TextValueFormat.None;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P16)] public FullTextFormat fullTextFormat = FullTextFormat.None;

		[Header("Text: Character")]
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P4)] public Font font;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P5)] public int fontSize = 16;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P6)] public FontStyle fontStyle = FontStyle.Normal;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P7)] public float lineSpacing = 1f;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P8)] public bool richText = false;

		[Header("Text: Paragraph")]
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P9)] public TextAnchor alignment = TextAnchor.UpperLeft;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P10)] public VerticalWrapMode verticalWrap = VerticalWrapMode.Truncate;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P11)] public HorizontalWrapMode horizontalWrap = HorizontalWrapMode.Wrap;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P12)] public bool bestFit = false;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P13)] public int bestMinSize = 10;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P14)] public int bestMaxSize = 40;
		
		[Header("Text: Animation")]
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P17)] public bool valueAnimate = false;
		[ParamFlag(ParameterFlag./*{flagPrefix}*/P18)] public float valueAnimateDuration = 2f;

		public void Setup(ITextComponent component) {

			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P1) == true) component.SetText(this.text);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P15) == true) component.SetTextLocalizationKey(this.localizationKey);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P2) == true) component.SetTextColor(this.color);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P3) == true) component.SetValueFormat(this.format);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P16) == true) component.SetFullTextFormat(this.fullTextFormat);

			//if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P4) == true) component.SetFont(this.font);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P5) == true) component.SetFontSize(this.fontSize);
			//if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P6) == true) component.SetFontStyle(this.fontStyle);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P7) == true) component.SetLineSpacing(this.lineSpacing);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P8) == true) component.SetRichText(this.richText);

			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P9) == true) component.SetTextAlignment(this.alignment);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P10) == true) component.SetTextVerticalOverflow(this.verticalWrap);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P11) == true) component.SetTextHorizontalOverflow(this.horizontalWrap);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P12) == true) component.SetBestFitState(this.bestFit);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P13) == true) component.SetBestFitMinSize(this.bestMinSize);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P14) == true) component.SetBestFitMaxSize(this.bestMaxSize);
			
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P17) == true) component.SetValueAnimate(this.valueAnimate);
			if (this.IsChanged(ParameterFlag./*{flagPrefix}*/P18) == true) component.SetValueAnimateDuration(this.valueAnimateDuration);

		}
		#endregion

	}

}