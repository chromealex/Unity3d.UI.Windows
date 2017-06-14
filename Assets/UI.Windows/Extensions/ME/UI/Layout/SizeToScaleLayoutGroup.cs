using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions {

	public class SizeToScaleLayoutGroup : LayoutGroup {

		public enum Adjust : byte {

			BySize,
			ByWidth,
			ByHeight,
			Expand,
			Shrink,

		};

		public Adjust adjust = Adjust.Shrink;
		public float scaleFactor = 1f;

		public bool overwriteDepth = false;
		[ReadOnly("overwriteDepth", state: true)]
		public float depth;

		public override void CalculateLayoutInputVertical() {

			this.Arrange();

		}

		public override void SetLayoutHorizontal() {

			this.Arrange();

		}

		public override void SetLayoutVertical() {

			this.Arrange();

		}

		#if UNITY_EDITOR
		protected override void OnValidate() {

			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			base.OnValidate();
			
			this.Arrange();
			
		}
		#endif

		public void Arrange() {

			if (this.rectTransform.rect.width < 0f ||
				this.rectTransform.rect.height < 0f) return;

			var scale = 0f;

			switch (this.adjust) {
				
				case Adjust.BySize:
					
					this.SetScale(this.rectTransform.rect.size * 0.5f);

					break;
					
				case Adjust.ByWidth:

					scale = this.rectTransform.rect.width * 0.5f;
					this.SetScale(new Vector3(scale, scale, scale));

					break;
					
				case Adjust.ByHeight:
					
					scale = this.rectTransform.rect.height * 0.5f;
					this.SetScale(new Vector3(scale, scale, scale));
					
					break;
					
				case Adjust.Expand:
					
					scale = (this.rectTransform.rect.width > this.rectTransform.rect.height ? this.rectTransform.rect.width : this.rectTransform.rect.height) * 0.5f;
					this.SetScale(new Vector3(scale, scale, scale));
					
					break;
					
				case Adjust.Shrink:
					
					scale = (this.rectTransform.rect.width > this.rectTransform.rect.height ? this.rectTransform.rect.height : this.rectTransform.rect.width) * 0.5f;
					this.SetScale(new Vector3(scale, scale, scale));
					
					break;

			}

		}

		public void SetScale(Vector3 scale) {

			var x = 0f;
			var y = 0f;
			switch (this.childAlignment) {
				
				case TextAnchor.UpperLeft:
					x = 0f;
					y = 1f;
					break;
					
				case TextAnchor.UpperCenter:
					x = 0.5f;
					y = 1f;
					break;
					
				case TextAnchor.UpperRight:
					x = 1f;
					y = 1f;
					break;

				case TextAnchor.MiddleLeft:
					x = 0f;
					y = 0.5f;
					break;
					
				case TextAnchor.MiddleCenter:
					x = 0.5f;
					y = 0.5f;
					break;
					
				case TextAnchor.MiddleRight:
					x = 1f;
					y = 0.5f;
					break;

				case TextAnchor.LowerLeft:
					x = 0f;
					y = 0f;
					break;
					
				case TextAnchor.LowerCenter:
					x = 0.5f;
					y = 0f;
					break;
					
				case TextAnchor.LowerRight:
					x = 1f;
					y = 0f;
					break;

			}

			for (int i = 0; i < this.rectChildren.Count; ++i) {
				
				var rect = this.rectChildren[i];
				
				this.m_Tracker.Add(this, rect, DrivenTransformProperties.Scale | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Anchors | DrivenTransformProperties.SizeDelta);

				var s = scale * this.scaleFactor;
				if (this.overwriteDepth == true) {

					s.z = this.depth;

				}

				rect.localScale = s;

				rect.pivot = new Vector2(x, y);
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.sizeDelta = Vector2.zero;
				rect.anchoredPosition = new Vector2(this.padding.left, this.padding.top);

			}

		}

	}

}
