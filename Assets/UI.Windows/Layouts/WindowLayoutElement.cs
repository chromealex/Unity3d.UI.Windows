using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class WindowLayoutElement : WindowLayoutBase {
		
		[ReadOnly]
		new public LayoutTag tag = LayoutTag.None;

		#if UNITY_EDITOR
		public string comment;

		[ReadOnly]
		public WindowComponent tempEditorComponent;
		private bool randomColorSetup;
		private Color randomColor;
		public void OnDrawGizmos() {

			if (this.randomColorSetup == false) {

				this.randomColor = UnityEngine.UI.Windows.Plugins.ColorHSV.GetDistinctColor();
				this.randomColorSetup = true;

			}

			this.OnDrawGUI_EDITOR(false, false);
			
		}
		
		public void OnDrawGizmosSelected() {

			var selected = (UnityEditor.Selection.activeGameObject == this.gameObject);
			this.OnDrawGUI_EDITOR(selected, true);
			
		}

		public void OnEnable() {

			if (Application.isPlaying == true) return;

			this.Reset();

		}

		[ContextMenu("Reset")]
		public void Reset() {

			this.tag = LayoutTag.None;

		}

		private void OnDrawGUI_EDITOR(bool selected, bool selectedHierarchy) {

			var textStyle = new GUIStyle(GUI.skin.label);
			textStyle.fontStyle = FontStyle.Normal;
			textStyle.stretchWidth = true;
			textStyle.fontSize = 12;
			textStyle.richText = true;
			textStyle.alignment = TextAnchor.MiddleCenter;
			textStyle.wordWrap = false;

			var points = new Vector3[4];
			var rect = this.transform as RectTransform;
			rect.GetWorldCorners(points);

			var descr = "<b>" + this.tag.ToString() + "</b>\n" + this.comment + "\n" + "(Animation: " + (this.animation != null ? this.animation.name : "None") + ")" +
				(this.tempEditorComponent != null ? ("\n(Component: " + this.tempEditorComponent.name + ")") : "");

			var center = Vector3.zero;
			for (int i = 0; i < 4; ++i) center += points[i];
			center /= 4f;
			
			textStyle.fixedWidth = rect.rect.width;
			textStyle.fixedHeight = rect.rect.height;
			
			// Hack to draw handles always
			var face = this.randomColor;
			face.a = 0.3f;
			UnityEditor.Handles.DrawSolidRectangleWithOutline(points, face, this.randomColor);

			var shadowOffset = Vector3.one * 1f;
			shadowOffset.z = 0f;

			var color = Color.black;
			color.a = 0.5f;
			textStyle.normal.textColor = color;
			UnityEditor.Handles.Label(center + shadowOffset, descr, textStyle);
			
			color = Color.white;
			color.a = (selected == true) ? 1f : 0.7f;
			textStyle.normal.textColor = color;
			UnityEditor.Handles.Label(center, descr, textStyle);
			
			color = Color.white;
			color.a = selected ? 1f : 0f;
			UnityEditor.Handles.color = color;
			this.DrawLineWithOffset(50f, center, points[0]);
			this.DrawLineWithOffset(50f, center, points[1]);
			this.DrawLineWithOffset(50f, center, points[2]);
			this.DrawLineWithOffset(50f, center, points[3]);

			color = Color.white;
			color.a = 0.5f;
			UnityEditor.Handles.color = color;

		}

		private void DrawLineWithOffset(float offset, Vector3 pos1, Vector3 pos2) {

			var oldColor = UnityEditor.Handles.color;

			var shadowColor = new Color(0f, 0f, 0f, oldColor.a > 0f ? 0.5f : 0f);
			var shadowOffset = Vector3.one * 1f;
			shadowOffset.z = 0f;

			var ray = new Ray(pos1, (pos2 - pos1).normalized);
			pos1 = ray.GetPoint(offset);

			UnityEditor.Handles.color = shadowColor;
			UnityEditor.Handles.DrawDottedLine(pos1 + shadowOffset, pos2 + shadowOffset, 10f);
			UnityEditor.Handles.color = oldColor;
			UnityEditor.Handles.DrawDottedLine(pos1, pos2, 10f);
			//UnityEditor.Handles.DrawLine(pos1, pos2);
			
			UnityEditor.Handles.color = oldColor;

		}
		#endif
		
	}
	
}

