using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows.Components {

	public class InputFieldComponent : WindowComponent {

		public bool convertToUppercase = false;

		[SerializeField]
		protected Text text;
		
		[SerializeField]
		protected InputField inputField;
		
		[SerializeField]
		protected Text placeholder;

		private ComponentEvent<string> onChange = new ComponentEvent<string>();
		private ComponentEvent<string> onEditEnd = new ComponentEvent<string>();
		private ComponentEvent<bool> onFocus = new ComponentEvent<bool>();
		
		private bool lastFocusValue = false;

		public string GetText() {

			return (this.inputField != null) ? this.inputField.text : string.Empty;

		}

		public void SetText(string text) {
			
			if (this.inputField != null) {
				
				this.inputField.enabled = false;
				this.inputField.text = string.Empty;
				this.inputField.text = text;
				this.inputField.enabled = true;
				
			}
			
		}
		
		public void SetPlaceholderText(string text) {
			
			if (this.placeholder != null) {

				this.placeholder.text = text;
				
			}
			
		}
		
		public bool HasFocus() {
			
			if (this.inputField != null) {
				
				return this.inputField.isFocused;

			}

			return false;
			
		}

		public void SetFocus() {

			if (this.inputField != null) {

				this.inputField.Select();
				this.inputField.ActivateInputField();

			}

		}

		public void SetContentType(InputField.ContentType contentType) {

			if (this.inputField != null) this.inputField.contentType = contentType;

		}
		
		public void SetLineType(InputField.LineType lineType) {
			
			if (this.inputField != null) this.inputField.lineType = lineType;
			
		}
		
		public void SetCharacterLimit(int length = 0) {
			
			if (this.inputField != null) this.inputField.characterLimit = length;
			
		}
		
		public void SetOnChangeCallback(UnityAction<string> onChange) {
			
			if (onChange != null) this.onChange.AddListenerDistinct(onChange);
			
		}
		
		public void SetOnEditEndCallback(UnityAction<string> onEditEnd) {
			
			if (onEditEnd != null) this.onEditEnd.AddListenerDistinct(onEditEnd);
			
		}
		
		public void SetOnFocusChangedCallback(UnityAction<bool> onFocus) {
			
			if (onFocus != null) this.onFocus.AddListenerDistinct(onFocus);
			
		}

		public void SetCallbacks(UnityAction<string> onChange, UnityAction<string> onEditEnd) {
			
			this.SetOnChangeCallback(onChange);
			this.SetOnEditEndCallback(onEditEnd);

		}
		
		private void OnChange(string input) {
			
			this.onChange.Invoke(input);
			
		}
		
		private void OnEditEnd(string input) {

			this.onEditEnd.Invoke(input);
			
		}

		public override void OnInit() {

			base.OnInit();

			this.inputField.onValidateInput = this.OnValidateChar;
			this.inputField.onValueChange.AddListener(this.OnChange);
			this.inputField.onEndEdit.AddListener(this.OnEditEnd);

			this.lastFocusValue = this.HasFocus();

		}

		public override void OnDeinit() {

			base.OnDeinit();

			this.inputField.onValidateInput = null;
			this.inputField.onValueChange.RemoveListener(this.OnChange);
			this.inputField.onEndEdit.RemoveListener(this.OnEditEnd);

			this.onChange.RemoveAllListeners();
			this.onEditEnd.RemoveAllListeners();
			this.onFocus.RemoveAllListeners();

		}

		public char OnValidateChar(string text, int index, char addedChar) {

			if (this.convertToUppercase == true) {

				return char.ToUpper(addedChar);

			}

			return addedChar;

		}

		public virtual void LateUpdate() {

			if (this.lastFocusValue != this.HasFocus()) {

				this.onFocus.Invoke(this.HasFocus());
				this.lastFocusValue = this.HasFocus();

			}

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			if (this.gameObject.activeSelf == false) return;

			var texts = this.GetComponentsInChildren<Text>(true);
			if (texts.Length == 1) this.text = texts[0];

			if (this.text != null) this.text.supportRichText = false;

			var inputFields = this.GetComponentsInChildren<InputField>(true);
			if (inputFields.Length == 1) this.inputField = inputFields[0];

			if (this.inputField != null && this.text != null) {

				this.inputField.textComponent = this.text;
				this.inputField.targetGraphic = this.text;

			}

		}
		#endif

	}

}