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

		public T Get<T>() where T : IComponent {
			
			var linker = this.component as LinkerComponent;
			if (linker != null) {
				
				return linker.Get<T>();

			}

			if ((this.component is T) == false) {

				Debug.LogWarningFormat("LinkerComponentProxy: Cast will fail because of wrong type. Requested type: {0}, Source component: {1}, Source type: {2} ({3})", typeof(T), this.component, (linker == null ? null : linker.GetType()), linker);

			}

			return (T)(this.component as IComponent);

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