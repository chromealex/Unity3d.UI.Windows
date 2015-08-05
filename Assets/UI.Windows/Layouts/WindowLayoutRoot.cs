using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	[ExecuteInEditMode]
	public class WindowLayoutRoot : MonoBehaviour {
		
		public enum Mode : byte {
			None,
			FlexibleWidth,
		};

		[HideInInspector]
		public RectTransform _rectTransform;
		public RectTransform rectTransform {

			get {

				if (this._rectTransform == null) this._rectTransform = this.transform as RectTransform;

				return this._rectTransform;

			}

		}

		public Mode mode = Mode.None;

		public float minWidth = 1024f;
		public float maxWidth = 1920f;
		public float margin = 100f;

		[HideInInspector]
		public Rect editorRect;

		#if UNITY_EDITOR
		public void OnValidate() {

			this._rectTransform = this.transform as RectTransform;

		}
		#endif

		public void Rebuild() {

			if (this.mode == Mode.None) return;

			var rect = this.rectTransform;
			var screenWidth = Screen.width;

			var width = Mathf.Clamp(screenWidth - this.margin * 2f, this.minWidth, this.maxWidth);

			var size = rect.sizeDelta;
			size.x = width;
			rect.sizeDelta = size;

		}
		
		public Vector2 GetOffsetNormalized() {

			var pos = this.rectTransform.anchoredPosition;
			return new Vector2(pos.x / Screen.width, pos.y / Screen.height);
			
		}

		public Vector2 GetOffset() {

			return this.rectTransform.anchoredPosition;

		}

		public void SetOffset(Vector2 offset) {

			this.rectTransform.anchoredPosition = offset;

		}

		public void SetOffsetNormalized(Vector2 offset) {

			this.SetOffset(new Vector2(Screen.width * offset.x, Screen.height * offset.y));

		}

	}

}