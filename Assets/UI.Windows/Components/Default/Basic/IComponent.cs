using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {
	
	public enum PointerEventState : byte {
		
		Click,
		Enter,
		Leave,
		
	};

	public interface IComponent {

	}

	public interface ISelectable : IComponent {

		void Select();
		void Deselect();
		void SetCallback(UnityAction callback);
		void SetCallback(UnityAction<ButtonComponent> callback);
		Selectable GetSelectable();

		void SetEnabledState(bool state);
		void SetHoverState(bool state);
		void SetHoverOnAnyPointerState(bool state);
		void SetHoverOnAnyButtonState(bool state);
		void SetCallbackHover(UnityAction<bool> callback);

		void SetSFX(PointerEventState state, Audio.Component data);

	}
	
	public interface ITextComponent : IComponent {
		
		void SetValue(int value, UnityEngine.UI.Windows.Components.TextComponent.ValueFormat format = UnityEngine.UI.Windows.Components.TextComponent.ValueFormat.None);
		void SetText(string text);
		string GetText();
		void SetTextColor(Color color);
		void SetValueFormat(UnityEngine.UI.Windows.Components.TextComponent.ValueFormat format);

		void SetFont(Font font);
		void SetFontSize(int value);

		void SetLineSpacing(float value);
		void SetFontStyle(FontStyle style);
		void SetRichText(bool state);

		void SetTextAlignment(TextAnchor anchor);
		void SetTextVerticalOverflow(VerticalWrapMode mode);
		void SetTextHorizontalOverflow(HorizontalWrapMode mode);

		void SetBestFitState(bool state);
		void SetBestFitMinSize(int value);
		void SetBestFitMaxSize(int value);

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

