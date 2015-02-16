using UnityEngine;
using System.Collections;

namespace ME {

	public partial class Utilities {

		public static Vector3 GetUIPosition(Transform transform, Vector3 alignToPoint, Camera uiCamera, Camera gameCamera, Vector3 offset = default(Vector3)) {

			var position = alignToPoint + offset;
			return uiCamera.ViewportToWorldPoint(gameCamera.WorldToViewportPoint(position));

		}

	}

}
