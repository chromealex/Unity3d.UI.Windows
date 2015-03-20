using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {

	public class ToggleItemComponent : ButtonWithTextComponent {

		public Toggle toggle;

		private ComponentEvent<bool> callback = new ComponentEvent<bool>();
		private ComponentEvent<ToggleItemComponent, bool> callbackButton = new ComponentEvent<ToggleItemComponent, bool>();

		public virtual void SetCallback(UnityAction<bool> callback) {

			this.callback.AddListenerDistinct(callback);
			this.callbackButton.RemoveAllListeners();

			this.SetCallback(() => {

				this.callback.Invoke(this.GetState());

			});

		}
		
		public virtual void SetCallback(UnityAction<ToggleItemComponent, bool> callback) {
			
			this.callbackButton.AddListenerDistinct(callback);
			this.callback.RemoveAllListeners();
			
			this.SetCallback(() => {
				
				this.callbackButton.Invoke(this, this.GetState());
				
			});

		}

		public void Register(ToggleGroup toggleGroup) {

			this.toggle.group = toggleGroup;

		}

		public void Toggle() {

			this.toggle.isOn = !this.toggle.isOn;

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