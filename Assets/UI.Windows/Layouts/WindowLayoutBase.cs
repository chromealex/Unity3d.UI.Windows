using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows {

	public class WindowLayoutBase : WindowComponentBase, IWindowComponentLayout {

		private Layout.Component activatorInstance;
		private WindowComponent component;

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.component = null;
			this.activatorInstance = null;

		}

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

		public void Load<T>(T component, System.Action<WindowObjectElement> onItem = null, bool async = false) where T : WindowComponent {

			this.Load(component as WindowComponent, callback: (WindowComponent comp) => {}, onItem: onItem, async: async);

		}

		public void Load<T>(T component, System.Action<T> callback, System.Action<WindowObjectElement> onItem = null, bool async = false) where T : WindowComponent {

			this.Load(component as WindowComponent, (WindowComponent comp) => callback.Invoke(comp as T), onItem: onItem, async: async);

		}

		public void Load<T>(ResourceMono resource, System.Action<WindowObjectElement> onItem = null, bool async = false) where T : WindowComponent {

			this.Load(resource, callback: (WindowComponent comp) => {}, onItem: onItem, async: async);

		}

		public void Load<T>(ResourceMono resource, System.Action<T> callback, System.Action<WindowObjectElement> onItem = null, bool async = false) where T : WindowComponent {

			this.Load(resource, callback: (WindowComponent comp) => callback.Invoke(comp as T), onItem: onItem, async: async);

		}

		public void Load(ResourceMono resource, System.Action<WindowObjectElement> onItem = null, bool async = false) {

			this.Load(resource, callback: (WindowComponent comp) => {}, onItem: onItem, async: async);

		}

		public void Load(WindowComponent component, System.Action<WindowObjectElement> onItem = null, bool async = false) {

			this.Load(component, callback: (WindowComponent comp) => {}, onItem: onItem, async: async);

		}

		public void Load(ResourceMono resource, System.Action<WindowComponent> callback, System.Action<WindowObjectElement> onItem = null, bool async = false) {

			this.activatorInstance.SetComponent(resource);
			this.activatorInstance.Create(this.GetWindow(), this as WindowLayoutElement, callback, async, onItem);

		}

		public void Load(WindowComponent component, System.Action<WindowComponent> callback, System.Action<WindowObjectElement> onItem = null, bool async = false) {

			this.activatorInstance.SetComponent(component);
			component.SetComponentState(WindowObjectState.NotInitialized);
			this.activatorInstance.Create(this.GetWindow(), this as WindowLayoutElement, callback, async, onItem);

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