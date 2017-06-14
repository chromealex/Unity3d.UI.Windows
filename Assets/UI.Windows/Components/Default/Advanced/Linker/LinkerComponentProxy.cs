using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {

	[System.Serializable]
	public class LinkerComponentProxy {
		
		public WindowComponent component;

		public bool showOnStart {

			get {
				
				return (this.component != null ? this.component.showOnStart : false);

			}

			set {

				if (this.component != null) {

					this.component.showOnStart = value;
						
				}

			}

		}

		public T Get<T>() where T : WindowComponent {
			
			var linker = this.component as LinkerComponent;
			if (linker != null) {
				
				return linker.Get<T>();

			}

			return this.component as T;

		}

		public WindowComponent Get() {

			var linker = this.component as LinkerComponent;
			if (linker != null) {

				return linker.Get<WindowComponent>();

			}

			return this.component;

		}

		public bool IsEmpty() {

			var linker = this.component as LinkerComponent;
			if (linker != null) {

				return linker.IsEmpty();

			}

			return (this.component == null);

		}

	}

}