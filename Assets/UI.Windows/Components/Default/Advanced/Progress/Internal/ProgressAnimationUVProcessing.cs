using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

	[ExecuteInEditMode]
	public class ProgressAnimationUVProcessing : ME.BaseVertexEffect {

		public Slider.Direction direction;
		public float duration = 1f;

		private Vector2 offset;

		protected override void Start() {

			base.Start();

			this.offset = Vector2.zero;

		}

		public override void ModifyVertices(List<UIVertex> list) {

			for (int i = 0; i < list.Count; ++i) {

				var item = list[i];
				item.uv0 += this.offset;
				list[i] = item;

			}

		}

		public void LateUpdate() {

			var deltaTime = Time.deltaTime;
			#if UNITY_EDITOR
			if (Application.isPlaying == false) {

				deltaTime = 0.01f;

			}
			#endif

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

			this.offset += step * deltaTime / this.duration;

			if (this.offset.x < 0f) this.offset.x += 1f;
			if (this.offset.x > 1f) this.offset.x -= 1f;
			if (this.offset.y < 0f) this.offset.y += 1f;
			if (this.offset.y > 1f) this.offset.y -= 1f;

			if (this.graphic != null) {

				this.graphic.SetVerticesDirty();

			}

		}

#if UNITY_EDITOR
		protected override void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			base.OnValidate();

			this.Start();
			this.LateUpdate();

		}
#endif

	}

}