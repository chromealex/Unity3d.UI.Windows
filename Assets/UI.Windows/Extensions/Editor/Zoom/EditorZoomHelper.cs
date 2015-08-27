using UnityEngine;

namespace UnityEditor.UI.Windows.Extensions {

	// Helper Rect extension methods

	public static class RectExtensions {
		
		public static Vector2 MiddleMiddle(this Rect rect) {
			
			return new Vector2(rect.center.x, rect.center.y);
			
		}
		
		public static Vector2 TopLeft(this Rect rect) {
			
			return new Vector2(rect.xMin, rect.yMin);
			
		}

		public static Rect ScaleSizeBy(this Rect rect, float scale) {

			return rect.ScaleSizeBy(scale, rect.center);

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

	public class EditorZoomArea {

		private const float ZOOM_MIN = 0.05f;
		private const float ZOOM_MAX = 1f;

		private const float EDITOR_TAB_HEIGHT = 21f;

		private Matrix4x4 prevGuiMatrix;

		private Rect screenCoordsArea;
		private float zoomValue = 1f;
		private Vector2 zoomCoordsOrigin = Vector2.zero;

		private EditorWindow editor;

		public EditorZoomArea(EditorWindow editor) {

			this.editor = editor;

		}

		public void SetZoom(float value) {

			this.zoomValue = value;

		}

		public float GetZoom() {

			return this.zoomValue;

		}

		public Vector2 GetOrigin() {

			return this.zoomCoordsOrigin;

		}

		public Vector2 SetRect(Rect rect, Vector2 origin) {

			this.screenCoordsArea = rect;
			this.zoomCoordsOrigin = origin;
			
			this.HandleEvents();

			return this.zoomCoordsOrigin;

		}

		public Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords, bool topLeft = false) {

			if (topLeft == true) {
				
				return (screenCoords - this.screenCoordsArea.TopLeft()) / this.zoomValue + this.zoomCoordsOrigin;

			}

			return (screenCoords - this.screenCoordsArea.MiddleMiddle()) / this.zoomValue + this.zoomCoordsOrigin;

		}

		public void HandleEvents() {

			if (Event.current.type == EventType.ScrollWheel) {

				Vector2 screenCoordsMousePos = Event.current.mousePosition;
				Vector2 delta = Event.current.delta;
				Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
				float zoomDelta = -delta.y / 150f;
				float oldZoom = this.zoomValue;
				this.zoomValue += zoomDelta;
				this.zoomValue = Mathf.Clamp(this.zoomValue, EditorZoomArea.ZOOM_MIN, EditorZoomArea.ZOOM_MAX);
				this.zoomCoordsOrigin += (zoomCoordsMousePos - this.zoomCoordsOrigin) - (oldZoom / this.zoomValue) * (zoomCoordsMousePos - this.zoomCoordsOrigin);
				
				Event.current.Use();
				
				this.editor.Repaint();

			}
			
			// Allow moving the zoom area's origin by dragging with the middle mouse button or dragging
			// with the left mouse button with Alt pressed.
			if (Event.current.type == EventType.MouseDrag &&
				(Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
				Event.current.button == 2) {

				Vector2 delta = Event.current.delta;
				delta /= this.zoomValue;
				this.zoomCoordsOrigin += delta;

				this.editor.Repaint();

				//Event.current.Use();

			}

		}

		public Vector2 Begin(Rect screenRect, Vector2 scrollPos, Rect contentRect) {

			this.zoomCoordsOrigin = scrollPos;
			var origin = this.zoomCoordsOrigin;

			this.HandleEvents();
			this.screenCoordsArea = screenRect;
			
			//GUI.BeginScrollView(screenRect, scrollPos, contentRect);
			this.Begin(screenRect, contentRect);

			scrollPos += this.zoomCoordsOrigin - origin;

			return scrollPos;

		}

		public Rect Begin(Rect area, Rect contentSize) {

			GUI.EndGroup();        // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.
			
			Rect clippedArea = area.ScaleSizeBy(1f / this.zoomValue, this.screenCoordsArea.MiddleMiddle());
			//this.screenCoordsArea.ScaleSizeBy(1f / this.zoomValue, this.screenCoordsArea.TopLeft());
			clippedArea.y += EditorZoomArea.EDITOR_TAB_HEIGHT;
			GUI.BeginGroup(clippedArea);
			
			this.prevGuiMatrix = GUI.matrix;
			Matrix4x4 translation = Matrix4x4.TRS(clippedArea.MiddleMiddle(), Quaternion.identity, Vector3.one);
			Matrix4x4 scale = Matrix4x4.Scale(new Vector3(this.zoomValue, this.zoomValue, 1f));
			GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
			
			GUI.BeginGroup(new Rect(this.zoomCoordsOrigin.x, this.zoomCoordsOrigin.y, contentSize.width, contentSize.height));

			return clippedArea;

		}
		
		public void End() {

			GUI.EndGroup();

			GUI.matrix = this.prevGuiMatrix;
			GUI.EndGroup();
			GUI.BeginGroup(new Rect(0f, EditorZoomArea.EDITOR_TAB_HEIGHT, Screen.width, Screen.height));
			//GUI.EndScrollView();

		}

		private Matrix4x4 prevUnzoomGuiMatrix;
		public void BeginUnzoom() {

			this.prevUnzoomGuiMatrix = GUI.matrix;

			GUI.matrix = this.prevGuiMatrix;

			GUI.BeginGroup(new Rect(0f, 0f, Screen.width, Screen.height));

		}

		public void EndUnzoom() {
			
			GUI.EndGroup();

			GUI.matrix = this.prevUnzoomGuiMatrix;

		}

	}

}