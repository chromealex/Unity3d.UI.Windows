using UnityEngine;

namespace UnityEngine.UI.Windows.Components {
	
	public interface IComponent {

	}

	public interface ITextComponent : IComponent {

		string GetText();

	}

}