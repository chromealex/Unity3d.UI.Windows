using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	[ExecuteInEditMode]
	public class WindowLayoutRoot : MonoBehaviour {
		
		public enum Mode : byte {
			None,
			FlexibleWidth,
		};

		private RectTransform _rectTransform;
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

		public void Rebuild() {

			if (this.mode == Mode.None) return;

			var rect = this.rectTransform;
			var screenWidth = Screen.width;

			var width = Mathf.Clamp(screenWidth - this.margin * 2f, this.minWidth, this.maxWidth);

			var size = rect.sizeDelta;
			size.x = width;
			rect.sizeDelta = size;

		}

	}

}