using UnityEngine;
using uiws = UnityEngine.UI.Windows;
using uiwc = UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Components {

	public interface ICheckboxButtonComponent {

		void SetChecked();
		void SetUnchecked();
		void SetCheckedState(bool state);

	}

	public class CheckboxButtonComponent : uiwc::ButtonComponent, ICheckboxButtonComponent {
		
		public uiwc::LinkerComponentProxy selectedObject;

		[SerializeField]
		protected bool @checked;

		public bool autoToggle = false;

		private System.Action<bool> onStateChanged;
		private System.Action<ICheckboxButtonComponent, bool> onButtonStateChanged;

		public override void OnInit() {

			base.OnInit();

			if (this.autoToggle == true) {

				this.SetCallback(this.Toggle);

			}

		}

		public void SetCallback(System.Action<bool> onStateChanged) {

			this.onStateChanged = onStateChanged;
			this.onButtonStateChanged = null;

		}

		public void SetCallback(System.Action<ICheckboxButtonComponent, bool> onButtonStateChanged) {

			this.onStateChanged = null;
			this.onButtonStateChanged = onButtonStateChanged;

		}

		public void AddCallback(System.Action<bool> onStateChanged) {

			this.onStateChanged += onStateChanged;

		}

		public void AddCallback(System.Action<ICheckboxButtonComponent, bool> onButtonStateChanged) {

			this.onButtonStateChanged += onButtonStateChanged;

		}

		public void RemoveCallback(System.Action<bool> onStateChanged) {

			this.onStateChanged -= onStateChanged;

		}

		public void RemoveCallback(System.Action<ICheckboxButtonComponent, bool> onButtonStateChanged) {

			this.onButtonStateChanged -= onButtonStateChanged;

		}

		public override IButtonComponent RemoveAllCallbacks() {

			this.onStateChanged = null;
			this.onButtonStateChanged = null;

			return base.RemoveAllCallbacks();

		}

		private void SetChecked_INTERNAL(bool state) {

			var changed = (this.@checked == !state);

			this.@checked = state;
			if (this.selectedObject.IsEmpty() == false) this.selectedObject.Get().ShowHide(this.@checked);

			if (changed == true) {

				if (this.onStateChanged != null) this.onStateChanged.Invoke(this.@checked);
				if (this.onButtonStateChanged != null) this.onButtonStateChanged.Invoke(this, this.@checked);

			}

		}

		public void SetChecked() {

			this.SetChecked_INTERNAL(state: true);

		}

		public void SetUnchecked() {

			this.SetChecked_INTERNAL(state: false);

		}

		public void SetCheckedState(bool state) {

			this.SetChecked_INTERNAL(state);

		}

		public void Toggle() {

			this.SetCheckedState(!this.@checked);

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			this.selectedObject.showOnStart = false;

			this.SetCheckedState(this.@checked);

		}
		#endif

	}

}