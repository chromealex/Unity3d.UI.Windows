using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {
	
	public interface IComponent {

	}

	public interface ISelectable : IComponent {

		void Select();
		void Deselect();
		void SetCallback(UnityAction callback);
		void SetCallback(UnityAction<ButtonComponent> callback);
		Selectable GetSelectable();

	}
	
	public interface ITextComponent : IComponent {
		
		void SetValue(int value, UnityEngine.UI.Windows.Components.TextComponent.ValueFormat format = UnityEngine.UI.Windows.Components.TextComponent.ValueFormat.None);
		void SetText(string text);
		string GetText();
		
	}
	
	public interface IImageComponent : IComponent {
		
		void SetImage(Sprite sprite, bool withPivotsAndSize = false);
		void SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize = false);
		void SetImage(Texture texture);
		void SetImage(Texture texture, bool preserveAspect);
		void SetMaterial(Material material);

	}

}