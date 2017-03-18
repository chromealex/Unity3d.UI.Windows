using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Extensions {

	[InitializeOnLoad]
	public static class ProjectEditor {

		private static Dictionary<IProjectDependency, Rect> list = new Dictionary<IProjectDependency, Rect>();

		private static int cirlceCount = 0;
		private static string prevGuid;
		private static bool cleared;
		private static int frameCheck = 0;
		private static int frameCheckCircle = 0;
		private static int lastReset = 0;

		static ProjectEditor() {

			EditorApplication.projectWindowItemOnGUI -= ProjectEditor.OnProjectItemGUI;
			EditorApplication.projectWindowItemOnGUI += ProjectEditor.OnProjectItemGUI;
			EditorApplication.projectWindowChanged -= ProjectEditor.OnProjectChanged;
			EditorApplication.projectWindowChanged += ProjectEditor.OnProjectChanged;

		}

		private static void OnProjectChanged() {

			list.Clear();

		}

		private static void OnProjectItemGUI(string guid, Rect rect) {

			if ((Event.current.type == EventType.KeyUp ||
				Event.current.type == EventType.MouseUp || Event.current.type == EventType.ScrollWheel) && cleared == false) {

				frameCheck = Time.renderedFrameCount;
				frameCheckCircle = cirlceCount;
				cleared = true;

			}

			if (prevGuid == guid) {
				
				++cirlceCount;

				if (cirlceCount == 2) {

					// Circle is done
					list.Clear();
					cirlceCount = 0;
					cleared = false;

				}

			}

			if (frameCheck != Time.renderedFrameCount) {

				if (frameCheckCircle == cirlceCount || Time.renderedFrameCount >= lastReset + 100) {

					lastReset = Time.renderedFrameCount;
					prevGuid = null;
					frameCheckCircle = -1;

				}

			}

			if (string.IsNullOrEmpty(prevGuid) == true) prevGuid = guid;

			var path = AssetDatabase.GUIDToAssetPath(guid);
			var orig = AssetDatabase.LoadAssetAtPath<Object>(path);
			var obj = orig as IProjectDependency;
			if (obj == null) {

				if (orig is GameObject) {

					obj = (orig as GameObject).GetComponent<IProjectDependency>();

				}

			}
			if (obj != null) {

				Rect r;
				if (list.TryGetValue(obj, out r) == false) {

					list.Add(obj, rect);

				} else {

					list[obj] = rect;

				}

				ProjectEditor.Draw(obj, Selection.activeObject == obj as Object);

			}

		}

		private static void Draw(IProjectDependency forObject, bool selected) {

			//Debug.LogWarning("BEGIN~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

			var deps = forObject.GetProjectDependencies();
			if (deps == null || deps.Length == 0) return;

			Rect fromRect;
			if (list.TryGetValue(forObject, out fromRect) == true) {
				
				for (int i = 0; i < deps.Length; ++i) {

					var dep = deps[i];
					if (dep == null) continue;

					Rect toRect;
					if (list.TryGetValue(dep, out toRect) == true) {

						//Debug.Log("Draw Arrow: " + fromRect + " >> " + toRect);
						// Draw arrow
						ProjectEditor.DrawNavigationArrow(selected, Vector2.left, fromRect, toRect);

					}

				}

			}

			//Debug.LogWarning("END~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

		}

		private static Vector2 GetPointOnRectEdge(Rect rect, Vector2 dir) {

			if (dir != Vector2.zero) dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
			dir = rect.center + Vector2.Scale(rect.size, dir * 0.5f);

			return dir;

		}

		const float kArrowThickness = 2f;
		const float kArrowHeadSize = 3f;
		const float kTargetOffset = 2f;
		const float kMinSplineForce = 0.2f;
		const float kMaxSplineForce = 0.8f;
		const float maxLength = 300f;

		private static void DrawNavigationArrow(bool selected, Vector2 direction, Rect fromRect, Rect toRect) {

			UnityEditor.Handles.color = new Color(1.0f, 0.9f, 0.1f, selected == true ? 1f : 0.3f);

			Vector2 sideDir = new Vector2(direction.y, -direction.x);

			/*var pfromPoint = ProjectEditor.GetPointOnRectEdge(fromRect, direction);
			var ptoPoint = ProjectEditor.GetPointOnRectEdge(toRect, direction);
			float plength = Vector2.Distance(pfromPoint, ptoPoint);

			if (plength > maxLength) {

				direction = -direction;

			}*/

			var fromPoint = ProjectEditor.GetPointOnRectEdge(fromRect, direction);
			var toPoint = ProjectEditor.GetPointOnRectEdge(toRect, direction);
			fromPoint.y += kTargetOffset;
			toPoint.y -= kTargetOffset;
			float fromSize = UnityEditor.HandleUtility.GetHandleSize(fromPoint) * 0.05f;
			float toSize = UnityEditor.HandleUtility.GetHandleSize(toPoint) * 0.05f;
			fromPoint += sideDir * fromSize;
			toPoint += sideDir * toSize;
			float length = Vector2.Distance(fromPoint, toPoint);
			var force = Mathf.Lerp(kMaxSplineForce, kMinSplineForce, length / maxLength);
			Vector2 fromTangent = direction * length * force;
			Vector2 toTangent = direction * length * force;

			UnityEditor.Handles.DrawBezier(fromPoint, toPoint, fromPoint + fromTangent, toPoint + toTangent, UnityEditor.Handles.color, null, kArrowThickness);
			var color = UnityEditor.Handles.color;
			color.a = selected == true ? 1f : 0.5f;
			UnityEditor.Handles.color = color;
			UnityEditor.Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + (direction - sideDir) * toSize * kArrowHeadSize);
			UnityEditor.Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + (direction + sideDir) * toSize * kArrowHeadSize);

		}

	}

}