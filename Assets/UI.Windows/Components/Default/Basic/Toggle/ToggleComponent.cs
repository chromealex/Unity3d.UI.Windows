
using UnityEngine.Events;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ToggleComponent : ListComponent {

		public ToggleGroupExtended toggleGroup;
		public bool allowSwitchOff = false;
		
		private ComponentEvent<bool> callback = new ComponentEvent<bool>();
		private ComponentEvent<ToggleItemComponent, bool> callbackButton = new ComponentEvent<ToggleItemComponent, bool>();

		private int lastSelectedIndex = -1;

		public override void OnInit() {

			base.OnInit();

			this.toggleGroup.allowSwitchOff = this.allowSwitchOff;

			this.lastSelectedIndex = -1;
			this.Toggle(-1);

			foreach (var item in this.GetItems()) {

				this.Setup(item as ToggleItemComponent);

			}

		}
		
		public virtual void SetCallback(System.Action<bool> callback) {
			
			this.callback.AddListenerDistinct(callback);
			this.callbackButton.RemoveAllListeners();

		}
		
		public virtual void SetCallback(System.Action<ToggleItemComponent, bool> callback) {
			
			this.callbackButton.AddListenerDistinct(callback);
			this.callback.RemoveAllListeners();

		}

		public override T AddItem<T>(bool autoRefresh = true) {

			var instance = base.AddItem<T>(autoRefresh);
			this.Setup(instance as ToggleItemComponent);

			return instance;

		}

		public void Setup(ToggleItemComponent instance) {
			
			if (instance != null) {

				var button = instance;
				button.SetCallback((ToggleItemComponent element, bool state) => {

					if (this.callback != null) this.callback.Invoke(element.GetState());
					if (this.callbackButton != null) this.callbackButton.Invoke(element, element.GetState());
					
				});
				
				button.Register(this.toggleGroup);
				this.toggleGroup.RegisterToggle(button.toggle);
				
			}

		}

		public override void Clear() {

			var items = this.GetItems<ToggleItemComponent>();
			foreach (var item in items) {

				this.toggleGroup.UnregisterToggle(item.toggle);

			}

			base.Clear();

		}

		public void SwitchOff() {
			
			var items = this.GetItems<ToggleItemComponent>();
			foreach (var item in items) {
				
				item.TurnOff();
				
			}

		}

		public void Toggle(IComponent element) {

			this.Toggle(this.GetIndexOf(element));

		}

		public void Toggle(int index) {

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
		
		public void Select(int index, bool state) {

			var item = this.GetItem<ToggleItemComponent>(index);
			if (item != null) {

				if (state == true) {

					item.TurnOn();

				} else {

					item.TurnOff();

				}

			}
			
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