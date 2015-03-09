using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

	public class TextComponent : WindowComponent {

		[SerializeField]
		private Text text;

		public string GetText() {

			return (this.text != null) ? this.text.text : string.Empty;

		}

		public void SetText(string text) {

			if (this.text != null) this.text.text = text;

		}

		#if UNITY_EDITOR
		public override void OnValidate() {

			base.OnValidate();

			if (this.gameObject.activeSelf == false) return;

			var texts = this.GetComponentsInChildren<Text>(true);
			if (texts.Length == 1) this.text = texts[0];

		}
		#endif

	}

}