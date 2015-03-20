using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows.Components {

	public class InputFieldComponent : WindowComponent {

		[SerializeField]
		protected Text text;
		
		[SerializeField]
		protected InputField inputField;
		
		private ComponentEvent<string> onChange = new ComponentEvent<string>();
		private ComponentEvent<string> onEditEnd = new ComponentEvent<string>();

		public string GetText() {

			return (this.inputField != null) ? this.inputField.text : string.Empty;

		}

		public void SetText(string text) {

			if (this.inputField != null) this.inputField.text = text;

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

		public void SetCallbacks(UnityAction<string> onChange, UnityAction<string> onEditEnd) {
			
			this.onChange.AddListenerDistinct(onChange);
			this.onEditEnd.AddListenerDistinct(onEditEnd);

		}

		public override void OnDeinit() {

			base.OnDeinit();
			
			this.onChange.RemoveAllListeners();
			this.onEditEnd.RemoveAllListeners();

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