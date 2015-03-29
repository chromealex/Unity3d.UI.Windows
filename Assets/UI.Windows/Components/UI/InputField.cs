using UnityEngine; 
using UnityEngine.UI; 
using UnityEngine.EventSystems; 
using System.Collections;
using System.Linq;

namespace UnityEngine.UI.Windows.Extensions {

	public class InputField : UnityEngine.UI.InputField {

		public override void OnSelect(BaseEventData eventData) {

			base.OnSelect(eventData);

			this.CorrectCaret();

		}

		private RectTransform caret;
		private void CorrectCaret() {

			if (this.caret == null) this.caret = (RectTransform)this.transform.parent.FindChild(this.gameObject.name + " Input Caret"); 

			if (this.caret != null && this.textComponent != null) {

				var alignment = this.textComponent.alignment;
				var offset = this.textComponent.rectTransform.anchoredPosition + Vector2.up * 2f;
				var pivot = new Vector2(0.5f, 0.5f);

				if (alignment == TextAnchor.MiddleLeft ||
				    alignment == TextAnchor.MiddleCenter ||
				    alignment == TextAnchor.MiddleRight) {
					
					offset += Vector2.up * this.textComponent.fontSize;
					pivot.y = 0f;
					
				}
				
				if (alignment == TextAnchor.LowerLeft ||
					alignment == TextAnchor.LowerCenter ||
					alignment == TextAnchor.LowerRight) {
					
					offset += Vector2.up * this.textComponent.fontSize;
					pivot.y = 1f;
					
				}

				var rect = this.caret;
				rect.pivot = pivot;
				rect.anchoredPosition = offset;

			}

		}

	}

}