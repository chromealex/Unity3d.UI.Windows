using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using UnityEngine.UI.Windows;
using ME;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class GUIDrawer {

		public const float DRAW_EVERY_ARROW = 140f;

		private readonly FlowSystemEditorWindow editor;

		public GUIDrawer(FlowSystemEditorWindow editor) {

			this.editor = editor;

		}

		#if UNITY_EDITOR
		public void DrawComponentCurve(UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow from, ref UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.ComponentLink link, UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow to) {

			if (from.IsEnabled() == false || to.IsEnabled() == false) return;
			
			var component = from.GetLayoutComponent(link.sourceComponentTag);
			if (component != null) {
				
				var rect = component.tempEditorRect;
				
				var start = new Rect(from.rect.x + rect.x, from.rect.y + rect.y, rect.width, rect.height);
				var end = to.rect;

				var zOffset = -4f;
				
				var offset = Vector2.zero;
				var startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, zOffset);
				var endPos = new Vector3(end.center.x + offset.x, end.center.y + offset.y, zOffset);
				
				var scale = FlowSystem.GetData().flowWindowWithLayoutScaleFactor;
				
				var side1 = from.rect.size.x * 0.5f;
				var side2 = from.rect.size.y * 0.5f;
				var stopDistance = Mathf.Sqrt(side1 * side1 + side2 * side2);
				
				var color = Color.white;
				if (from.GetContainer() != to.GetContainer()) {
					
					color = Color.gray;
					if (to.GetContainer() != null) color = to.GetContainer().randomColor;
					
				}
				var comment = this.DrawComponentCurve(startPos, endPos, color, stopDistance + 50f * scale, link.comment);
				if (link.comment != comment) {
					
					link.comment = comment;
					FlowSystem.SetDirty();
					
				}
				
			}
			
		}
		
		private string DrawComponentCurve(Vector3 startPos, Vector3 endPos, Color color, float stopDistance, string label) {

			var shadowColor = new Color(0f, 0f, 0f, 0.5f);
			var lineColor = color;//new Color(1f, 1f, 1f, 1f);
			
			var shadowOffset = Vector3.one * 2f;
			shadowOffset.z = 0f;
			
			var ray = new Ray(startPos, (endPos - startPos).normalized);
			var rot = Quaternion.LookRotation(endPos - startPos);
			
			var centerPoint = (startPos + endPos) * 0.5f;
			
			endPos = ray.GetPoint(stopDistance);
			var fullDistance = Vector3.Distance(endPos, startPos);
			
			var width = 4f;
			width /= FlowSystem.GetZoom();

			Handles.BeginGUI();
			
			Handles.color = shadowColor;
			Handles.DrawSolidDisc(startPos + shadowOffset, Vector3.back, 5f);
			
			Handles.color = lineColor;
			Handles.DrawSolidDisc(startPos, Vector3.back, 5f);
			
			Handles.color = shadowColor;
			Handles.DrawAAPolyLine(width, new Vector3[] { startPos + shadowOffset, endPos + shadowOffset });
			
			Handles.color = lineColor;
			Handles.DrawAAPolyLine(width, new Vector3[] { startPos, endPos });
			
			var labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.stretchWidth = false;
			
			var backOverStyle = new GUIStyle("flow node 0");
			backOverStyle.stretchWidth = false;
			backOverStyle.stretchHeight = false;
			
			var backStyle = new GUIStyle("flow node hex 5");
			
			var style = new GUIStyle(GUI.skin.textField);
			style.stretchWidth = true;
			style.stretchHeight = true;
			style.wordWrap = true;
			//style.contentOffset = new Vector2(0f, -15f);
			style.contentOffset = new Vector2(0f, 5f);
			style.alignment = TextAnchor.MiddleCenter;
			style.border = backStyle.border;
			style.normal = backStyle.normal;
			style.focused = backStyle.normal;
			style.active = backStyle.normal;
			style.hover = backStyle.normal;
			
			var content = new GUIContent(label);
			var contentRect = GUILayoutUtility.GetRect(content, labelStyle);
			
			contentRect.width = Mathf.Max(contentRect.width, 40f);
			
			contentRect.width += 40f;
			contentRect.height += 20f;
			
			centerPoint.x -= contentRect.width * 0.5f;
			centerPoint.y -= contentRect.height * 0.5f;
			
			var boxRect = new Rect(centerPoint.x, centerPoint.y, contentRect.width, contentRect.height);
			
			style.fixedWidth = contentRect.width;// + 20f;
			style.fixedHeight = contentRect.height;
			
			var boxRectBack = boxRect;
			backOverStyle.fixedWidth = style.fixedWidth;
			backOverStyle.fixedHeight = style.fixedHeight + 10f;
			
			var oldColor = GUI.backgroundColor;
			var bColor = color;
			bColor.a = 0.4f;
			GUI.backgroundColor = bColor;
			GUI.Box(boxRectBack, "Test", backOverStyle);
			GUI.backgroundColor = oldColor;
			
			label = GUI.TextField(boxRect, label, style);
			
			var every = DRAW_EVERY_ARROW;
			if (fullDistance < every * 2f) {
				
				var pos = ray.GetPoint(fullDistance * 0.5f);
				
				this.DrawCap(pos, startPos, endPos, rot, shadowColor, lineColor);
				
				//var arrow = (new GUIStyle("StaticDropdown")).normal.background;
				//GUIExt.DrawTextureRotated(pos, arrow, Vector3.Angle(startPos, endPos));
				
			} else {
				
				for (float distance = every; distance < fullDistance; distance += every) {
					
					var pos = ray.GetPoint(distance);
					
					this.DrawCap(pos, startPos, endPos, rot, shadowColor, lineColor);
					
					//var arrow = (new GUIStyle("StaticDropdown")).normal.background;
					//GUIExt.DrawTextureRotated(pos, arrow, rot);
					
				}
				
			}
			
			Handles.EndGUI();
			
			return label;
			
		}
		
		public void DrawNodeCurve(UnityEngine.UI.Windows.AttachItem attachItem, UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow from, UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow to, bool doubleSide) {
			
			if (from.IsEnabled() == false || to.IsEnabled() == false) return;
			
			var fromRect = from.rect;
			var toRect = to.rect;
			
			if (from != null && to != null) {
				
				var delta = Flow.OnDrawNodeCurveOffset(this.editor, attachItem, from, to, doubleSide);
				fromRect.center += delta;

			}

			Rect centerStart = fromRect;
			Rect centerEnd = toRect;

			var fromComponent = false;
			var toComponent = false;

			if (from.IsFunction() == true &&
			    from.IsContainer() == false) {
				
				var func = FlowSystem.GetWindow(from.GetFunctionId());
				if (func != null) {
					
					var selected = FlowSystem.GetSelected();
					var isSelected = selected.Contains(from.id) || (selected.Count == 0 && this.editor.focusedGUIWindow == from.id);
					if (isSelected == true) {
						
						var color = new Color(0f, 0f, 0f, 0.1f);
						var backColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
						
						this.DrawPolygon(new Vector3(from.rect.xMin, from.rect.yMin, 0f),
						                 new Vector3(func.rect.xMin, func.rect.yMin, 0f),
						                 new Vector3(func.rect.xMin, func.rect.yMax, 0f),
						                 new Vector3(from.rect.xMin, from.rect.yMax, 0f),
						                 backColor);
						
						this.DrawPolygon(new Vector3(from.rect.xMin, from.rect.yMin, 0f),
						                 new Vector3(func.rect.xMin, func.rect.yMin, 0f),
						                 new Vector3(func.rect.xMax, func.rect.yMin, 0f),
						                 new Vector3(from.rect.xMax, from.rect.yMin, 0f),
						                 backColor);
						
						this.DrawPolygon(new Vector3(from.rect.xMax, from.rect.yMin, 0f),
						                 new Vector3(func.rect.xMax, func.rect.yMin, 0f),
						                 new Vector3(func.rect.xMax, func.rect.yMax, 0f),
						                 new Vector3(from.rect.xMax, from.rect.yMax, 0f),
						                 backColor);
						
						this.DrawPolygon(new Vector3(from.rect.xMax, from.rect.yMax, 0f),
						                 new Vector3(func.rect.xMax, func.rect.yMax, 0f),
						                 new Vector3(func.rect.xMin, func.rect.yMax, 0f),
						                 new Vector3(from.rect.xMin, from.rect.yMax, 0f),
						                 backColor);
						
						this.DrawNodeCurveDotted(new Vector3(from.rect.xMin, from.rect.yMin, 0f),
						                         new Vector3(func.rect.xMin, func.rect.yMin, 0f),
						                         color
						                         );
						
						this.DrawNodeCurveDotted(new Vector3(from.rect.xMin, from.rect.yMax, 0f),
						                         new Vector3(func.rect.xMin, func.rect.yMax, 0f),
						                         color
						                         );
						
						this.DrawNodeCurveDotted(new Vector3(from.rect.xMax, from.rect.yMin, 0f),
						                         new Vector3(func.rect.xMax, func.rect.yMin, 0f),
						                         color
						                         );
						
						this.DrawNodeCurveDotted(new Vector3(from.rect.xMax, from.rect.yMax, 0f),
						                         new Vector3(func.rect.xMax, func.rect.yMax, 0f),
						                         color
						                         );
						
					}
					
				}
				
			}

			if (FlowSystem.GetData().HasView(FlowView.Layout) == true) {
				
				var comps = from.attachedComponents.Where((c) => c.targetWindowId == to.id && c.sourceComponentTag != LayoutTag.None);
				foreach (var comp in comps) {
					
					var component = from.GetLayoutComponent(comp.sourceComponentTag);
					if (component != null) {
						
						fromRect = centerStart;
						
						var rect = component.tempEditorRect;
						fromRect = new Rect(fromRect.x + rect.x, fromRect.y + rect.y, rect.width, rect.height);
						
						this.DrawNodeCurve(from.GetContainer(), to.GetContainer(), from, to, centerStart, centerEnd, fromRect, toRect, doubleSide, 0f);
						
						fromComponent = true;
						
					}
					
				}
				
				if (doubleSide == true) {
					
					comps = to.attachedComponents.Where((c) => c.targetWindowId == from.id && c.sourceComponentTag != LayoutTag.None);
					foreach (var comp in comps) {
						
						var component = to.GetLayoutComponent(comp.sourceComponentTag);
						if (component != null) {
							
							toRect = centerEnd;
							
							var rect = component.tempEditorRect;
							toRect = new Rect(toRect.x + rect.x, toRect.y + rect.y, rect.width, rect.height);
							
							this.DrawNodeCurve(from.GetContainer(), to.GetContainer(), from, to, centerStart, centerEnd, fromRect, toRect, doubleSide, 0f);
							
							toComponent = true;
							
						}
						
					}
					
				}
				
			}
			
			if (fromComponent == false && toComponent == false) this.DrawNodeCurve(from.GetContainer(), to.GetContainer(), from, to, centerStart, centerEnd, fromRect, toRect, doubleSide);
			
		}

		private void DrawNodeCurve(UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow fromContainer, UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow toContainer, UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow fromWindow, UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow toWindow, Rect centerStart, Rect centerEnd, Rect fromRect, Rect toRect, bool doubleSide, float size = 6f) {
			
			Rect start = fromRect;
			Rect end = toRect;
			
			var color1 = Color.white;
			var color2 = Color.white;
			
			if (fromContainer != toContainer) {
				
				color1 = Color.gray;
				color2 = Color.gray;
				
				if (toContainer != null) color1 = toContainer.randomColor;
				if (fromContainer != null) color2 = fromContainer.randomColor;
				
			}
			
			var zOffset = -4f;
			
			if (doubleSide == true) {
				
				var rot = Quaternion.AngleAxis(90f, Vector3.back);
				var ray = new Ray(Vector3.zero, (rot * (end.center - start.center)).normalized);
				
				var offset = ray.GetPoint(size);

				var startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, zOffset);
				var endPos = new Vector3(centerEnd.center.x + offset.x, centerEnd.center.y + offset.y, zOffset);
				
				this.DrawNodeCurve(startPos, endPos, color1);
				
				offset = ray.GetPoint(-size);
				startPos = new Vector3(centerStart.center.x + offset.x, centerStart.center.y + offset.y, zOffset);
				endPos = new Vector3(end.center.x + offset.x, end.center.y + offset.y, zOffset);
				
				this.DrawNodeCurve(endPos, startPos, color2);
				
			} else {
				
				var offset = Vector2.zero;

				var startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, zOffset);
				var endPos = new Vector3(centerEnd.center.x + offset.x, centerEnd.center.y + offset.y, zOffset);
				
				this.DrawNodeCurve(startPos, endPos, color1);
				
			}
			
		}
		
		public void DrawNodeCurveDotted(Vector3 startPos, Vector3 endPos, Color color) {
			
			//Handles.BeginGUI();
			
			Handles.color = color;
			//Handles.DrawAAPolyLine(3f, new Vector3[] { startPos, endPos });
			
			var space = 5f;
			var length = 10f;
			var ray = new Ray(startPos, (endPos - startPos).normalized);
			var fullDistance = Vector3.Distance(endPos, startPos);
			
			var width = 2f;
			width /= FlowSystem.GetZoom();

			for (float distance = 0f; distance < fullDistance; distance += space) {
				
				var pos = ray.GetPoint(distance);
				distance += length;
				if (distance > fullDistance) distance = fullDistance;
				var toPos = ray.GetPoint(distance);
				
				Handles.DrawAAPolyLine(width, new Vector3[] { pos, toPos });
				
			}
			
			//Handles.EndGUI();
			
		}
		
		public void DrawPolygon(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color) {
			
			//Handles.BeginGUI();
			
			Handles.color = color;
			
			Handles.DrawAAConvexPolygon(new Vector3[4] { p1, p2, p3, p4 });
			
			//Handles.EndGUI();
			
		}
		
		public void DrawNodeCurve(Vector3 startPos, Vector3 endPos, Color color) {
			
			var shadowColor = new Color(0f, 0f, 0f, 0.5f);
			var lineColor = color;//new Color(1f, 1f, 1f, 1f);
			
			var shadowOffset = Vector3.one * 1f;
			shadowOffset.z = 0f;

			var width = 3f;
			width /= FlowSystem.GetZoom();
			
			//Handles.BeginGUI();
			
			Handles.color = shadowColor;
			Handles.DrawAAPolyLine(width, new Vector3[] { startPos + shadowOffset, endPos + shadowOffset });
			
			Handles.color = lineColor;
			Handles.DrawAAPolyLine(width, new Vector3[] { startPos, endPos });
			
			var ray = new Ray(startPos, (endPos - startPos).normalized);
			var rot = Quaternion.LookRotation(endPos - startPos);
			
			var every = DRAW_EVERY_ARROW;
			var fullDistance = Vector3.Distance(endPos, startPos);
			if (fullDistance < every * 2f) {
				
				var pos = ray.GetPoint(fullDistance * 0.5f);
				
				this.DrawCap(pos, startPos, endPos, rot, shadowColor, lineColor);
				
				//var arrow = (new GUIStyle("StaticDropdown")).normal.background;
				//GUIExt.DrawTextureRotated(pos, arrow, Vector3.Angle(startPos, endPos));
				
			} else {
				
				for (float distance = every; distance < fullDistance; distance += every) {
					
					var pos = ray.GetPoint(distance);
					
					this.DrawCap(pos, startPos, endPos, rot, shadowColor, lineColor);
					
					//var arrow = (new GUIStyle("StaticDropdown")).normal.background;
					//GUIExt.DrawTextureRotated(pos, arrow, rot);
					
				}
				
			}
			
			//Handles.EndGUI();
			
		}
		
		//private Texture2D arrow;
		public void DrawCap(Vector3 pos, Vector3 startPos, Vector3 endPos, Quaternion rot, Color shadowColor, Color lineColor) {

			if (FlowSystem.GetZoom() < 1f) return;

			//if (this.arrow == null) this.arrow = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Flow/Arrow");
			
			var size = 12f;
			size /= FlowSystem.GetZoom();

			//pos = this.editor.zoomDrawer.ConvertScreenCoordsToZoomCoords(pos, topLeft: true);

			var scrollPos = -FlowSystem.GetScrollPosition();
			var offset = -8f;
			
			if (this.editor.scrollRect.Contains(new Vector3(pos.x - scrollPos.x + FlowSystemEditorWindow.GetSettingsWidth() + offset, pos.y - scrollPos.y + FlowSystemEditorWindow.TOOLBAR_HEIGHT + offset, 0f)) == false) return;

			var shadowOffset = Vector3.one * 1f;
			shadowOffset.z = 0f;
			
			/*var v1 = startPos - endPos;
			v1.x = -v1.x;
			var v2 = Vector3.left;
			
			var angle = Mathf.Atan2(
				Vector3.Dot(Vector3.back, Vector3.Cross(v1, v2)),
				Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;

			var oldColor = GUI.color;
			
			GUI.color = shadowColor;
			GUIExt.DrawTextureRotated(new Rect(pos.x - size * 0.5f + shadowOffset.x, pos.y - size * 0.5f + shadowOffset.y, size, size), arrow, -angle + 180f);
			GUI.color = lineColor;
			GUIExt.DrawTextureRotated(new Rect(pos.x - size * 0.5f, pos.y - size * 0.5f, size, size), arrow, -angle + 180f);
			
			GUI.color = oldColor;*/
			
			Handles.color = shadowColor;
			Handles.ConeHandleCap(-1, pos + shadowOffset, rot, 15f, EventType.Repaint);
			Handles.color = lineColor;
			Handles.ConeHandleCap(-1, pos, rot, 15f, EventType.Repaint);
			
		}
		#endif

	}

}