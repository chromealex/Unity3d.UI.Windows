using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {

	[System.Serializable]
	public class LinkerComponentProxy<T> where T : WindowComponent {
		
		[SerializeField] private T component;

		public T Get() {
			
			var linker = this.component as LinkerComponent;
			if (linker != null) {
				
				return linker.Get<T>();

			}

			return this.component;

		}

	}

}