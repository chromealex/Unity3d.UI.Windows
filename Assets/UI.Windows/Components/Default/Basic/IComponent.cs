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
		TimeHMSFromMilliseconds,		// 00:00:00

		DateUniversalFromMilliseconds,	// Universal

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
		bool IsVisible();
		bool IsVisibleSelf();
		RectTransform GetRectTransform();

	}

	public interface IComponent : IComponentElement {


	}

	public interface IHoverableComponent : IComponent {

		IHoverableComponent SetSFX(PointerEventState state, Audio.Component data);

		IHoverableComponent SetHoverState(bool state);
		IHoverableComponent SetHoverOnAnyPointerState(bool state);
		IHoverableComponent SetCallbackHover(System.Action<bool> callback);
		IHoverableComponent RemoveCallbackHover(System.Action<bool> onHover);

		bool IsHovered();
		IHoverableComponent SetHoverEnter();
		IHoverableComponent SetHoverExit();

	}

	public interface IInteractableStateComponent : IWindowNavigation {

		bool IsInteractable();

	}

	public interface IInteractableComponent : IInteractableStateComponent {

		bool IsHoverCursorDefaultOnInactive();

		IInteractableComponent SetEnabledState(bool state);
		IInteractableComponent SetEnabled();
		IInteractableComponent SetDisabled();

		IInteractableComponent SetHoverOnAnyButtonState(bool state);

		void Select();
		Selectable GetSelectable();

		bool IsInteractableAndHasEvents();

	}

	public interface IInteractableControllerComponent : IComponent {

		IInteractableControllerComponent Click();

	}

	public interface IButtonComponent : ITextComponent, IImageComponent, IWindowNavigation, IInteractableControllerComponent, IInteractableComponent {
		
		IButtonComponent RemoveAllCallbacks();
		IButtonComponent RemoveCallback(System.Action callback);
		IButtonComponent RemoveCallback(System.Action<ButtonComponent> callback);
		IButtonComponent SetCallback(System.Action callback);
		IButtonComponent SetCallback(System.Action<ButtonComponent> callback);
		IButtonComponent AddCallback(System.Action callback);
		IButtonComponent AddCallback(System.Action<ButtonComponent> callback);
		void OnClick();
		IButtonComponent SetButtonColor(Color color);
		IButtonComponent SetSelectByDefault(bool state);
		IButtonComponent SetNavigationMode(Navigation.Mode mode);

	}

	public interface IListComponent : IWindowNavigation {

		IListComponent ListMoveUp(int count = 1);
		IListComponent ListMoveDown(int count = 1);
		IListComponent ListMoveLeft(int count = 1);
		IListComponent ListMoveRight(int count = 1);

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

		ITextComponent SetTextLocalizationKey(LocalizationKey key);
		ITextComponent SetText(string text);
		ITextComponent SetText(LocalizationKey key, params object[] parameters);
		string GetText();
		ITextComponent SetTextColor(Color color);
		ITextComponent SetValueFormat(TextValueFormat format);
		ITextComponent SetFullTextFormat(FullTextFormat format);

		ITextComponent SetFontSize(int value);
		int GetFontSize();

		ITextComponent SetLineSpacing(float value);
		ITextComponent SetRichText(bool state);

		ITextComponent SetTextAlignment(TextAnchor anchor);
		ITextComponent SetTextVerticalOverflow(VerticalWrapMode mode);
		ITextComponent SetTextHorizontalOverflow(HorizontalWrapMode mode);

		ITextComponent SetBestFitState(bool state);
		ITextComponent SetBestFitMinSize(int value);
		ITextComponent SetBestFitMaxSize(int value);

		Graphic GetGraphicSource();

	}

	public interface IAlphaComponent {

		IAlphaComponent SetAlpha(float value);

	}

	public interface IImageComponent : IComponent, IAlphaComponent, ILoadableResource, IResourceReference {
		
		IImageComponent ResetImage();

		IImageComponent SetImage(ResourceAuto resource, System.Action onDataLoaded = null, System.Action onComplete = null, System.Action onFailed = null);

		IImageComponent SetImage(Sprite sprite);
		IImageComponent SetImage(Sprite sprite, bool immediately);
		IImageComponent SetImage(Sprite sprite, System.Action onComplete);
		IImageComponent SetImage(Sprite sprite, System.Action onComplete, bool immediately);
		IImageComponent SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize, System.Action onComplete);
		IImageComponent SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize, System.Action onComplete, bool immediately);

		IImageComponent SetImage(Texture texture);
		IImageComponent SetImage(Texture texture, bool immediately);
		IImageComponent SetImage(Texture texture, System.Action onComplete);
		IImageComponent SetImage(Texture texture, System.Action onComplete, bool immediately);
		IImageComponent SetImage(Texture texture, bool preserveAspect, System.Action onComplete);
		IImageComponent SetImage(Texture texture, bool preserveAspect, System.Action onComplete, bool immediately);

		IImageComponent SetImage(LocalizationKey key, params object[] parameters);

		IImageComponent SetImageLocalizationKey(LocalizationKey key);

		IImageComponent SetMaterial(Material material, bool setMainTexture = false, System.Action callback = null);
		Color GetColor();
		void SetColor(Color color);
		IImageComponent SetPreserveAspectState(bool state);

		Texture GetTexture();

		bool IsHorizontalFlip();
		bool IsVerticalFlip();
		bool IsPreserveAspect();

		bool IsMovie();
		bool GetPlayOnShow();
		IImageComponent SetMovieTexture(ResourceAuto resource, System.Action onDataLoaded, System.Action onComplete = null, System.Action onFailed = null);
		IImageComponent SetPlayOnShow(bool state);
		IImageComponent SetLoop(bool state);
		bool IsLoop();
		bool IsPlaying();
		IImageComponent Play();
		IImageComponent Play(bool loop);
		IImageComponent Play(bool loop, System.Action onComplete);
		IImageComponent Stop();
		IImageComponent Pause();
		IImageComponent Rewind(bool pause = true);

		Graphic GetGraphicSource();
		Image GetImageSource();
		RawImage GetRawImageSource();

	}
	
	public interface IProgressComponent : IComponent, IWindowNavigation, IInteractableComponent {
		
		void SetDuration(float value);
		void SetMinNormalizedValue(float value);
		void SetContiniousState(bool continious);
		void SetContiniousWidth(float continiousWidth);
		void SetContiniousAngleStep(float continiousAngleStep);
		void SetCallback(System.Action<float> onChanged);
		IProgressComponent SetValue(float value, bool immediately = false, System.Action callback = null);
		
	}

}

