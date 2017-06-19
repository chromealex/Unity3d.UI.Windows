using UnityEngine;

namespace ME {

	public static class VectorUtilities {
		
		public static Vector2 XY(this Vector3 source) {
			
			return new Vector2(source.x, source.y);
			
		}
		
		public static Vector2 XZ(this Vector3 source) {
			
			return new Vector2(source.x, source.z);
			
		}
		
		public static Vector3 XY(this Vector2 source) {
			
			return new Vector3(source.x, source.y, 0f);
			
		}
		
		public static Vector3 XZ(this Vector2 source) {
			
			return new Vector3(source.x, 0f, source.y);
			
		}

	}

}