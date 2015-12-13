using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components {
	
	public enum PointerEventState : byte {
		
		Click,
		Enter,
		Leave,
		
	};

	public enum RichTextFlags : byte {

		None = 0x0,
		Color = 0x1,
		Bold = 0x2,
		Italic = 0x4,
		Size = 0x8,
		Material = 0x10,
		Quad = 0x20,

	};

	public enum TextValueFormat : byte {

		None,		 // 1234567890
		WithSpace,	 // 1 234 567 890
		WithComma,	 // 1,234 567 890
		TimeHMSFromSeconds,				// 00:00:00
		TimeMSFromSeconds,				// 00:00
		TimeHMSmsFromMilliseconds,		// 00:00:00`00
		TimeMSmsFromMilliseconds,		// 00:00`00

		DateDMHMS,						// 12 Aug 00:00:00
		DateDMHMSFromMilliseconds,
		TimeMSFromMilliseconds,			// 00:00

	};

	public interface IComponent {

	}

	public interface IButtonComponent : ITextComponent, IImageComponent, IComponent {

		IButtonComponent SetCallback(UnityAction callback);
		IButtonComponent SetCallback(UnityAction<ButtonComponent> callback);
		Selectable GetSelectable();

		IButtonComponent SetEnabledState(bool state);
		IButtonComponent SetEnabled();
		IButtonComponent SetDisabled();
		IButtonComponent SetHoverState(bool state);
		IButtonComponent SetHoverOnAnyPointerState(bool state);
		IButtonComponent SetHoverOnAnyButtonState(bool state);
		IButtonComponent SetCallbackHover(UnityAction<bool> callback);

		IButtonComponent SetSFX(PointerEventState state, Audio.Component data);

	}
	
	public interface ITextComponent : IComponent {
		
		ITextComponent SetValue(int value, TextValueFormat format = TextValueFormat.None);
		ITextComponent SetText(string text);
		string GetText();
		ITextComponent SetTextColor(Color color);
		ITextComponent SetValueFormat(TextValueFormat format);

		ITextComponent SetFont(Font font);
		ITextComponent SetFontSize(int value);

		ITextComponent SetLineSpacing(float value);
		ITextComponent SetFontStyle(FontStyle style);
		ITextComponent SetRichText(bool state);

		ITextComponent SetTextAlignment(TextAnchor anchor);
		ITextComponent SetTextVerticalOverflow(VerticalWrapMode mode);
		ITextComponent SetTextHorizontalOverflow(HorizontalWrapMode mode);

		ITextComponent SetBestFitState(bool state);
		ITextComponent SetBestFitMinSize(int value);
		ITextComponent SetBestFitMaxSize(int value);

	}
	
	public interface IImageComponent : IComponent {
		
		void ResetImage();
		void SetImage(Sprite sprite, bool withPivotsAndSize = false);
		void SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize = false);
		void SetImage(Texture texture);
		void SetImage(Texture texture, bool preserveAspect);
		void SetMaterial(Material material);
		Color GetColor();
		void SetColor(Color color);
		void SetAlpha(float value);
		void SetPreserveAspectState(bool state);
		void SetPlayOnStart(bool state);
		void SetLoop(bool state);

	}
	
	public interface IProgressComponent {
		
		void SetDuration(float value);
		void SetMinNormalizedValue(float value);
		void SetContiniousState(bool continious);
		void SetContiniousWidth(float continiousWidth);
		void SetContiniousAngleStep(float continiousAngleStep);
		void SetCallback(UnityAction<float> onChanged);
		void SetValue(float value, bool immediately = false);
		
	}

}

