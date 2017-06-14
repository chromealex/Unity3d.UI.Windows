using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ToggleItemComponent : TextComponent {

		[Header("Toggle Item Component")]
		public ToggleExtended toggle;

		private ComponentEvent<bool> callbackToggle = new ComponentEvent<bool>();
		private ComponentEvent<ToggleItemComponent, bool> callbackToggleButton = new ComponentEvent<ToggleItemComponent, bool>();

		public virtual void SetCallback(System.Action<bool> callback) {

			this.callbackToggle.AddListenerDistinct(callback);
			this.callbackToggleButton.RemoveAllListeners();

			this.toggle.onValueChanged.RemoveListener(this.OnValueChanged);
			this.toggle.onValueChanged.AddListener(this.OnValueChanged);

		}
		
		public virtual void SetCallback(System.Action<ToggleItemComponent, bool> callback) {
			
			this.callbackToggleButton.AddListenerDistinct(callback);
			this.callbackToggle.RemoveAllListeners();

			this.toggle.onValueChanged.RemoveListener(this.OnValueChanged);
			this.toggle.onValueChanged.AddListener(this.OnValueChanged);

		}

		private void OnValueChanged(bool state) {

			this.callbackToggle.Invoke(state);
			this.callbackToggleButton.Invoke(this, state);

		}

		public ToggleItemComponent SetEnabledState(bool state) {

			if (state == true) {

				this.SetEnabled();

			} else {

				this.SetDisabled();

			}

			return this;

		}

		public ToggleItemComponent SetEnabled() {

			this.GetSelectable().interactable = true;

			return this;

		}

		public ToggleItemComponent SetDisabled() {

			this.GetSelectable().interactable = false;

			return this;

		}

		public bool IsInteractable() {

			return this.GetSelectable().IsInteractable();

		}

		public Selectable GetSelectable() {
		
			return this.toggle;

		}

		public void Register(ToggleGroupExtended toggleGroup) {

			this.toggle.group = toggleGroup;

		}

		public void Toggle() {

			this.toggle.isOn = !this.toggle.isOn;

		}

		public void SetTurnState(bool state) {

			this.toggle.isOn = state;

		}

		public void TurnOn() {

			this.toggle.isOn = true;

		}

		public void TurnOff() {

			this.toggle.isOn = false;

		}

		public bool GetState() {

			return this.toggle.isOn;

		}
		
		#if UNITY_EDITOR
		public override void OnValidateEditor() {
			
			base.OnValidateEditor();
			
			ME.Utilities.FindReference(this, ref this.toggle);
			
		}
		#endif

	}

}