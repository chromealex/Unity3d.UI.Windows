using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ToggleItemComponent : ButtonComponent {

		[Header("Toggle Item Component")]
		public Toggle toggle;

		private ComponentEvent<bool> callbackToggle = new ComponentEvent<bool>();
		private ComponentEvent<ToggleItemComponent, bool> callbackButton = new ComponentEvent<ToggleItemComponent, bool>();

		public virtual void SetCallback(UnityAction<bool> callback) {

			this.callbackToggle.AddListenerDistinct(callback);
			this.callbackButton.RemoveAllListeners();
			
			this.SetCallback(() => {
				
				this.Toggle();
				this.callbackToggle.Invoke(this.GetState());

			});

		}
		
		public virtual void SetCallback(UnityAction<ToggleItemComponent, bool> callback) {
			
			this.callbackButton.AddListenerDistinct(callback);
			this.callbackToggle.RemoveAllListeners();

			this.SetCallback(() => {

				this.Toggle();
				this.callbackButton.Invoke(this, this.GetState());
				
			});

		}

		public void Register(ToggleGroup toggleGroup) {

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