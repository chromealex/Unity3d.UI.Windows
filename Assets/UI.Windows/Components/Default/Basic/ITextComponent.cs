using UnityEngine;

namespace UnityEngine.UI.Windows.Components {
	
	public interface IComponent {

	}

	public interface ISelectable : IComponent {

		void Select();
		void Deselect();

	}
	
	public interface ITextComponent : IComponent {
		
		void SetValue(int value, UnityEngine.UI.Windows.Components.TextComponent.ValueFormat format = UnityEngine.UI.Windows.Components.TextComponent.ValueFormat.None);
		void SetText(string text);
		string GetText();
		
	}
	
	public interface IImageComponent : IComponent {
		
		void SetImage(Sprite sprite, bool withPivotsAndSize = false);
		void SetImage(Texture2D texture);

	}

}