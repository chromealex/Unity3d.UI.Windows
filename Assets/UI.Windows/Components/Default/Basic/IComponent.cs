using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows.Plugins.Localization;

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
		TimeHMSFromMilliseconds,			// 00:00:00

	};

	public enum FullTextFormat : byte {

		None = 0x0,
		UpperFirstLetter = 0x1,
		LowerAll = 0x2,
		UpperAll = 0x4,
		UppercaseWords = 0x8,
		TrimLeft = 0x10,
		TrimRight = 0x20,
		Trim = TrimLeft | TrimRight,
		Percent = 0x40,

	};

	public interface IComponentElement : IWindowObject {

		WindowObjectState GetComponentState();
		void SetComponentState(WindowObjectState state, bool dontInactivate = false);

	}

	public interface IComponent : IComponentElement {

	}

	public interface IButtonComponent : ITextComponent, IImageComponent, IComponent {
		
		IButtonComponent RemoveAllCallbacks();
		IButtonComponent RemoveCallback(UnityAction callback);
		IButtonComponent RemoveCallback(UnityAction<ButtonComponent> callback);
		IButtonComponent SetCallback(UnityAction callback);
		IButtonComponent SetCallback(UnityAction<ButtonComponent> callback);
		Selectable GetSelectable();
		IButtonComponent SetSelectByDefault(bool state);
		IButtonComponent SetNavigationMode(Navigation.Mode mode);

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
		
		ITextComponent SetValue(int value);
		ITextComponent SetValue(long value);
		ITextComponent SetValue(int value, TextValueFormat format);
		ITextComponent SetValue(long value, TextValueFormat format);
		ITextComponent SetValue(int value, TextValueFormat format, bool animate);
		ITextComponent SetValue(long value, TextValueFormat format, bool animate);
		
		ITextComponent SetValueAnimate(bool state);
		ITextComponent SetValueAnimateDuration(float duration);

		ITextComponent SetText(string text);
		ITextComponent SetText(LocalizationKey key, params object[] parameters);
		string GetText();
		ITextComponent SetTextColor(Color color);
		ITextComponent SetValueFormat(TextValueFormat format);
		ITextComponent SetFullTextFormat(FullTextFormat format);

		ITextComponent SetFont(Font font);
		ITextComponent SetFontSize(int value);
		int GetFontSize();

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

	public interface IAlphaComponent {

		IAlphaComponent SetAlpha(float value);

	}

	public interface IImageComponent : IComponent, IAlphaComponent, ILoadableResource {
		
		Texture GetTexture();

		IImageComponent ResetImage();

		IImageComponent SetImage(AutoResourceItem resource, System.Action onDataLoaded = null, System.Action onComplete = null);

		IImageComponent SetImage(Sprite sprite, System.Action onComplete = null);
		IImageComponent SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize, System.Action onComplete = null);

		IImageComponent SetImage(Texture texture, System.Action onComplete = null);
		IImageComponent SetImage(Texture texture, bool preserveAspect, System.Action onComplete = null);

		IImageComponent SetImage(LocalizationKey key, params object[] parameters);

		IImageComponent SetMaterial(Material material);
		Color GetColor();
		void SetColor(Color color);
		IImageComponent SetPreserveAspectState(bool state);

		bool IsMovie();
		bool GetPlayOnShow();
		IImageComponent SetMovieTexture(AutoResourceItem resource, System.Action onDataLoaded, System.Action onComplete = null);
		IImageComponent SetPlayOnShow(bool state);
		IImageComponent SetLoop(bool state);
		bool IsPlaying();
		IImageComponent Play();
		IImageComponent Play(bool loop);
		IImageComponent Stop();
		IImageComponent Pause();

		Graphic GetGraphicSource();
		Image GetImageSource();
		RawImage GetRawImageSource();

	}
	
	public interface IProgressComponent {
		
		void SetDuration(float value);
		void SetMinNormalizedValue(float value);
		void SetContiniousState(bool continious);
		void SetContiniousWidth(float continiousWidth);
		void SetContiniousAngleStep(float continiousAngleStep);
		void SetCallback(UnityAction<float> onChanged);
		IProgressComponent SetValue(float value, bool immediately = false, System.Action callback = null);
		
	}

}

