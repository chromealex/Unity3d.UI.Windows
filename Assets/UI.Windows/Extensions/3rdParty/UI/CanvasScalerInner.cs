using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows;

namespace ME {

	public class CanvasScalerInner : UIBehaviour, ILayoutController {

		public Vector2 targetSize;
		[Range(0f, 1f)]
		public float match;

		public RectTransform rect;

		[Header("Optional")]
		public WindowObject windowObject;
		[Header("-- OR --")]
		public CanvasScaler rootScaler;
		public RectTransform rootTransform;

		protected DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();

		private float lastScaleFactor = 0f;

		protected override void OnRectTransformDimensionsChange() {

			base.OnRectTransformDimensionsChange();

			this.Calculate();

		}

		protected override void OnTransformParentChanged() {

			base.OnTransformParentChanged();

			this.Calculate();

		}

		protected override void OnCanvasHierarchyChanged() {

			base.OnCanvasHierarchyChanged();

			this.Calculate();

		}

		protected override void OnCanvasGroupChanged() {

			base.OnCanvasGroupChanged();

			this.Calculate();

		}

		protected override void OnDidApplyAnimationProperties() {

			base.OnDidApplyAnimationProperties();

			this.Calculate();

		}

		[ContextMenu("Calculate")]
		public void Calculate() {

			if (this.windowObject != null) {

				var window = this.windowObject.GetWindow<UnityEngine.UI.Windows.Types.LayoutWindowType>();
				if (window != null) {

					var currentLayout = window.GetCurrentLayout();
					this.rootScaler = window.GetCanvasScaler();
					if (currentLayout != null) this.rootTransform = currentLayout.GetRoot() as RectTransform;

				}

			}

			if (this.rect == null || this.rootScaler == null || this.rootTransform == null) return;

			this.tracker.Clear();

			var scaleFactor = Mathf.Lerp(this.rootTransform.rect.width, this.rootTransform.rect.height, this.match) / Mathf.Lerp(this.targetSize.x, this.targetSize.y, this.match);

			if (this.lastScaleFactor != scaleFactor) {

				this.lastScaleFactor = scaleFactor;

				this.rect.localScale = Vector3.one * scaleFactor;
				this.rect.anchoredPosition = Vector2.zero;
				//this.rect.localRotation = Quaternion.identity;
				//this.rect.anchorMin = Vector2.one * 0.5f;
				//this.rect.anchorMax = Vector2.one * 0.5f;
				//this.rect.pivot = Vector2.one * 0.5f;
				this.rect.sizeDelta = new Vector2(Mathf.Lerp(this.targetSize.x, this.rootScaler.referenceResolution.x * (1f / scaleFactor), this.match), Mathf.Lerp(this.rootScaler.referenceResolution.y * (1f / scaleFactor), this.targetSize.y, this.match));

			}

			this.tracker.Add(this, this.rect, DrivenTransformProperties.Scale | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta | DrivenTransformProperties.Pivot | DrivenTransformProperties.Anchors);

		}

		protected override void OnDisable() {
					
			base.OnDisable();

			this.tracker.Clear();
			
		}

		void ILayoutController.SetLayoutHorizontal() {

			this.Calculate();

		}

		void ILayoutController.SetLayoutVertical() {

			this.Calculate();

		}

		#if UNITY_EDITOR
		protected override void OnValidate() {

			base.OnValidate();

			if (Application.isPlaying == false) return;

			if (this.windowObject == null) this.windowObject = this.GetComponent<WindowObject>();

			if (this.windowObject == null && this.rootScaler == null) {

				this.rootScaler = ME.Utilities.FindReferenceParent<CanvasScaler>(this);
				if (this.rootScaler != null) this.rootTransform = this.rootScaler.transform as RectTransform;

			}

			if (this.rect == null) {

				this.rect = this.GetComponent<RectTransform>();
				this.rect.anchorMin = Vector2.one * 0.5f;
				this.rect.anchorMax = Vector2.one * 0.5f;
				this.rect.pivot = Vector2.one * 0.5f;

			}

			this.Calculate();

		}
		#endif

	}

}