using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Extensions {

	public static class SpriteExt {

		public static Vector2 GetPivot(this Sprite sprite) {

			var bounds = sprite.bounds;

			var pivotX = - bounds.center.x / bounds.extents.x / 2 + 0.5f;
			var pivotY = - bounds.center.y / bounds.extents.y / 2 + 0.5f;

			return new Vector2(pivotX, pivotY);

		}

	}

}