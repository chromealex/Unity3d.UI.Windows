using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {

	public class AccelerometerComponent : WindowComponent {

		public RectTransform targetTransform;
		public float speed = 10f;
		public Vector2 tiltSides = new Vector2(1f, 1f);
		public Vector2 tiltMax = new Vector2(50f, 50f);

		private Vector3 prevMousePosition;
		public Vector2 GetInput() {

			#if UNITY_EDITOR
			var pos = new Vector2(Input.mousePosition.x - this.prevMousePosition.x, Input.mousePosition.y - this.prevMousePosition.y);
			this.prevMousePosition = Input.mousePosition;
			return pos;
			#else
			return new Vector2(Input.acceleration.x, Input.acceleration.y);
			#endif

		}

		public virtual void LateUpdate() {

			var input = this.GetInput();
			var dir = Vector3.zero;
			dir.x = -input.x * this.tiltSides.x;
			dir.y = -input.y * this.tiltSides.y;
			if (dir.sqrMagnitude > 1f) {

				dir.Normalize();

			}

			dir *= Time.deltaTime;
			var pos = this.targetTransform.localPosition;
			pos += dir * speed;
			if (Mathf.Abs(pos.x) > this.tiltMax.x) pos.x = Mathf.Sign(pos.x) * this.tiltMax.x;
			if (Mathf.Abs(pos.y) > this.tiltMax.y) pos.y = Mathf.Sign(pos.y) * this.tiltMax.y;
			this.targetTransform.localPosition = pos;

		}

	}

}