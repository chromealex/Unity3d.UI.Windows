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

		protected override void OnRectTransformDimensionsChange() {

			base.OnRectTransformDimensionsChange();

			this.CorrectCaret();

		}

		public bool HasKeyboard() {

			return (this.m_Keyboard != null);

		}

		public override void Select() {

			this.ActivateInputField();

		}

		public void MoveTextEndFix(bool shift) {

			this.StartCoroutine(this.MoveTextEnd_YIELD(shift));

		}

		public System.Collections.Generic.IEnumerator<byte> MoveTextEnd_YIELD(bool shift) {

			//yield return new WaitForEndOfFrame();
			yield return 0;

			this.MoveTextEnd(shift);

		}

		private RectTransform caret;
		private void CorrectCaret() {

			if (this.caret == null && this.textComponent != null) this.caret = this.transform.Find(this.gameObject.name + " Input Caret") as RectTransform;

			if (this.caret != null && this.textComponent != null) {

				var alignment = this.textComponent.alignment;
				var offset = this.textComponent.rectTransform.anchoredPosition + Vector2.up * 2f;
				var pivot = new Vector2(0.5f, 0.5f);

				if (alignment == TextAnchor.MiddleLeft ||
				    alignment == TextAnchor.MiddleCenter ||
				    alignment == TextAnchor.MiddleRight) {
					
					offset += Vector2.up * this.textComponent.fontSize * this.textComponent.lineSpacing;
					pivot.y = 0f;
					
				}
				
				if (alignment == TextAnchor.LowerLeft ||
					alignment == TextAnchor.LowerCenter ||
					alignment == TextAnchor.LowerRight) {
					
					offset += Vector2.up * this.textComponent.fontSize * this.textComponent.lineSpacing;
					pivot.y = 1f;
					
				}

				var rect = this.caret;
				rect.sizeDelta = (this.transform as RectTransform).rect.size;
				rect.pivot = pivot;
				rect.anchoredPosition = offset;

			}

		}

	}

}