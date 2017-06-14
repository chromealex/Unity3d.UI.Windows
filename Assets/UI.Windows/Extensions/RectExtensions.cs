using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Extensions {

	public static class RectExtensions {
		
		public static Rect PixelPerfect(this Rect rect) {
			
			return new Rect(Mathf.Floor(rect.x), Mathf.Floor(rect.y), Mathf.Floor(rect.width), Mathf.Floor(rect.height));
			
		}
		
		public static Rect PixelPerfect(this Rect rect, float grid) {
			
			return new Rect(Mathf.FloorToInt(rect.x / grid), Mathf.FloorToInt(rect.y / grid), Mathf.FloorToInt(rect.width / grid), Mathf.FloorToInt(rect.height / grid));
			
		}

		public static Vector2 PixelPerfect(this Vector2 value) {
			
			return new Vector2(Mathf.Floor(value.x), Mathf.Floor(value.y));
			
		}
		
		public static Vector2 MiddleMiddle(this Rect rect) {
			
			return new Vector2(rect.center.x, rect.center.y);
			
		}
		
		public static Vector2 TopLeft(this Rect rect) {
			
			return new Vector2(rect.xMin, rect.yMin);
			
		}
		
		public static Rect ScaleSizeBy(this Rect rect, float scale) {
			
			return rect.ScaleSizeBy(1f / scale, rect.MiddleMiddle());
			
		}
		
		public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint) {
			
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale;
			result.xMax *= scale;
			result.yMin *= scale;
			result.yMax *= scale;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
			
		}
		
		public static Rect ScaleSizeBy(this Rect rect, Vector2 scale) {
			
			return rect.ScaleSizeBy(scale, rect.center);
			
		}
		
		public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint) {
			
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale.x;
			result.xMax *= scale.x;
			result.yMin *= scale.y;
			result.yMax *= scale.y;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
			
		}
		
	}

}