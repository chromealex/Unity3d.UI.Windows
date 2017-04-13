using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {

	[System.Serializable]
	public class LinkerComponentProxy {
		
		public WindowComponent component;

		public T Get<T>() where T : WindowComponent {
			
			var linker = this.component as LinkerComponent;
			if (linker != null) {
				
				return linker.Get<T>();

			}

			return this.component as T;

		}

	}

}