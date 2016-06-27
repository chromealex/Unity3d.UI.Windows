using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows {

	public class WindowLayoutBase : WindowComponentBase, IWindowComponentLayout {

		public void Unload(System.Action callback = null) {

			if (this.component != null) {

				this.UnregisterSubComponent(this.component, () => {

					this.component.Recycle();
					this.component = null;

					if (callback != null) callback.Invoke();

				});

			} else {
				
				if (callback != null) callback.Invoke();

			}

		}

		private Layout.Component activatorInstance;
		private WindowComponent component;
		public T Load<T>(T component) where T : WindowComponent {

			return this.Load(component as WindowComponent) as T;

		}

		public WindowComponent Load(WindowComponent component) {

			this.activatorInstance.SetComponent(component);
			component.SetComponentState(WindowObjectState.NotInitialized);
			return this.activatorInstance.Create(this.GetWindow(), this as WindowLayoutElement) as WindowComponent;

		}
		
		public virtual void Setup(WindowComponent component, Layout.Component activatorInstance) {
			
			this.activatorInstance = activatorInstance;
			this.component = component;
			if (this.component != null) this.component.Setup(this);
			
		}

		public virtual WindowComponent GetCurrentComponent() {
			
			return this.component;
			
		}

	}

}