using UnityEngine;

namespace UnityEngine.UI.Windows.Components {
	
	public interface IComponent {

	}

	public interface ISelectable : IComponent {

		void Select();
		void Deselect();

	}

	public interface ITextComponent : IComponent {

		string GetText();

	}

}