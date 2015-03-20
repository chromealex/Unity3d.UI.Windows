using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ToggleComponent : List {

		public ToggleGroup toggleGroup;
		public bool allowSwitchOff = false;
		
		private ComponentEvent<bool> callback = new ComponentEvent<bool>();
		private ComponentEvent<ToggleItemComponent, bool> callbackButton = new ComponentEvent<ToggleItemComponent, bool>();

		private int lastSelectedIndex = -1;

		public override void OnInit() {

			base.OnInit();

			this.toggleGroup.allowSwitchOff = this.allowSwitchOff;

			this.lastSelectedIndex = -1;
			this.Select(-1);

		}
		
		public virtual void SetCallback(UnityAction<bool> callback) {
			
			this.callback.AddListenerDistinct(callback);
			this.callbackButton.RemoveAllListeners();

		}
		
		public virtual void SetCallback(UnityAction<ToggleItemComponent, bool> callback) {
			
			this.callbackButton.AddListenerDistinct(callback);
			this.callback.RemoveAllListeners();

		}

		public override T AddItem<T>() {

			var instance = base.AddItem<T>();

			if (instance is ToggleItemComponent) {

				var button = instance as ToggleItemComponent;
				button.SetCallback((ToggleItemComponent element, bool state) => {

					this.Select(element);
					
					if (this.callback != null) this.callback.Invoke(element.GetState());
					if (this.callbackButton != null) this.callbackButton.Invoke(element, element.GetState());

				});

				button.Register(this.toggleGroup);
				this.toggleGroup.RegisterToggle(button.toggle);

			}

			return instance;

		}

		public override void Clear() {

			var items = this.GetItems<ToggleItemComponent>();
			foreach (var item in items) {

				this.toggleGroup.UnregisterToggle(item.toggle);

			}

			base.Clear();

		}

		public void Select(IComponent element) {

			this.Select(this.GetIndexOf(element));

		}

		public void Select(int index) {

			if (this.lastSelectedIndex == index) {

				if (this.allowSwitchOff == true) {

					this.lastSelectedIndex = -1;
					this.toggleGroup.SetAllTogglesOff();

				}

				return;

			}

			var item = this.GetItem<ToggleItemComponent>(index);
			if (item != null) item.Toggle();
			
			this.lastSelectedIndex = index;

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			ME.Utilities.FindReference(this, ref this.toggleGroup);

		}
		#endif

	}

}