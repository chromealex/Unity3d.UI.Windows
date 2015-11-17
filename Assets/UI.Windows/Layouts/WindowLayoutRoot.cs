using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	[ExecuteInEditMode]
	public class WindowLayoutRoot : WindowObject, ICanvasElement {
		
		public enum Mode : byte {
			None,
			FlexibleWidth,
		};
		
		public CanvasGroup canvasGroup;

		[HideInInspector]
		public RectTransform _rectTransform;
		public RectTransform rectTransform {

			get {

				if (this._rectTransform == null) this._rectTransform = this.transform as RectTransform;

				return this._rectTransform;

			}

		}

		public float alpha {

			set {

				this.canvasGroup.alpha = value;

			}

			get {

				return this.canvasGroup.alpha;

			}

		}

		public Mode mode = Mode.None;

		public float minWidth = 1024f;
		public float maxWidth = 1920f;
		public float margin = 100f;

		#if UNITY_EDITOR
		//[HideInInspector]
		public Rect editorRectLocal;
		
		//[HideInInspector]
		public Rect editorRect;

		public void OnValidate() {

			if (Application.isPlaying == true) return;

			this._rectTransform = this.transform as RectTransform;

			if (this.canvasGroup == null) {
				
				this.canvasGroup = this.GetComponent<CanvasGroup>();
				if (this.canvasGroup == null) this.gameObject.AddComponent<CanvasGroup>();
				this.canvasGroup = this.GetComponent<CanvasGroup>();
				
			}

		}
		#endif
		/*
		new private Camera camera;
		internal override void Setup(WindowBase window) {

			base.Setup(window);

			this.camera = this.GetWindow().workCamera;

		}*/

		public void Reset() {
			
			this.rectTransform.anchoredPosition3D = Vector3.zero;
			this.rectTransform.anchorMin = Vector2.zero;
			this.rectTransform.anchorMax = Vector2.one;
			this.rectTransform.pivot = Vector2.one * 0.5f;
			this.rectTransform.sizeDelta = Vector2.zero;
			this.rectTransform.localRotation = Quaternion.identity;
			this.rectTransform.localScale = Vector3.one;

		}

		public void LayoutComplete() {
		}
		
		public void GraphicUpdateComplete() {
		}

		public bool IsDestroyed() {

			return this == null;

		}

		public void Rebuild(UI.CanvasUpdate state) {

			this.Rebuild();

		}

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