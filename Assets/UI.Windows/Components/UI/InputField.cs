using UnityEngine; 
using UnityEngine.UI; 
using UnityEngine.EventSystems; 
using System.Collections;
using System.Linq;

namespace UnityEngine.UI.Windows.Extensions {

	public class InputField : UnityEngine.UI.InputField {

		public override void OnSelect(BaseEventData eventData) {

			base.OnSelect(eventData);

			var caretTransform = (RectTransform)this.transform.parent.FindChild(this.gameObject.name + " Input Caret"); 

			if (caretTransform != null && this.textComponent != null) {

				var alignment = this.textComponent.alignment;
				var offset = Vector2.zero;
				var pivot = new Vector2(0.5f, 0.5f);

				if (alignment == TextAnchor.MiddleLeft ||
				    alignment == TextAnchor.MiddleCenter ||
				    alignment == TextAnchor.MiddleRight) {
					
					offset = Vector2.up * this.textComponent.fontSize;
					pivot.y = 0f;
					
				}
				
				if (alignment == TextAnchor.LowerLeft ||
					alignment == TextAnchor.LowerCenter ||
					alignment == TextAnchor.LowerRight) {
					
					offset = Vector2.up * this.textComponent.fontSize;
					pivot.y = 1f;
					
				}

				var rect = caretTransform;
				rect.pivot = pivot;
				rect.anchoredPosition = offset;

			}

		}

	}

}