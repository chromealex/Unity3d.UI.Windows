using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

	[ExecuteInEditMode]
	public class ProgressAnimationProcessing : BaseVertexEffect {

		[ReadOnly]
		public Slider.Direction direction;
		[ReadOnly]
		public float duration = 1f;

		private Vector2 offset;
		private Vector2 size;

		private RectTransform rectTransform;

		protected override void Start() {

			base.Start();

			this.offset = Vector2.zero;

			this.rectTransform = this.transform as RectTransform;

			var image = this.graphic as Image;
			var imageRect = image.sprite.textureRect;
			this.size = new Vector2(imageRect.width, imageRect.height);

			this.rectTransform.sizeDelta = this.size * 2f;
			this.rectTransform.anchoredPosition = Vector2.zero;

		}

		public override void ModifyVertices(List<UIVertex> list) {
		}

		public void LateUpdate() {

			if (this.duration <= 0f) return;

			var step = Vector2.zero;
			
			if (this.direction == Slider.Direction.BottomToTop) {
				
				step = Vector2.up;
				
			} else if (this.direction == Slider.Direction.TopToBottom) {
				
				step = -Vector2.up;
				
			} else if (this.direction == Slider.Direction.LeftToRight) {
				
				step = Vector2.right;
				
			} else if (this.direction == Slider.Direction.RightToLeft) {
				
				step = -Vector2.right;
				
			}

			this.offset += step * Time.deltaTime / this.duration;
			if (this.offset.x < 0f) this.offset.x += 1f;
			if (this.offset.x > 1f) this.offset.x -= 1f;
			if (this.offset.y < 0f) this.offset.y += 1f;
			if (this.offset.y > 1f) this.offset.y -= 1f;

			this.rectTransform.sizeDelta = new Vector2(this.size.x * Mathf.Abs(step.x) * 2f, this.size.y * Mathf.Abs(step.y) * 2f);
			this.rectTransform.anchoredPosition = new Vector2(this.offset.x * this.size.x, this.offset.y * this.size.y);

		}

		protected override void OnValidate() {

			base.OnValidate();

			this.Start();

		}

	}

}