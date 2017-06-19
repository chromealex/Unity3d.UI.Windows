/*
 * CircleLayoutGroup is a part of UI.Windows System
 * https://github.com/chromealex/Unity3d.UI.Windows
 * Alex Feer chrome.alex@gmail.com MIT License
 */
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ME;

namespace UnityEngine.UI.Extensions {

	public class CircleLayoutGroup : LayoutGroup {

		public bool bothSided = false;
		[ReadOnly("bothSided", false)] public bool bothSidedSorted = false;
		public float startAngle = 0f;
		public float maxAnglePerElement = 360f;
		public float angle = 360f;
		public bool reverse = false;

		public bool manualSize = false;
		[ReadOnly("manualSize", false)] public float radiusX = 100f;
		[ReadOnly("manualSize", false)] public float radiusY = 100f;

		public override void CalculateLayoutInputHorizontal() {

			base.CalculateLayoutInputHorizontal();

			this.Arrange();

		}

		public override void CalculateLayoutInputVertical() {
			
			this.Arrange();

		}

		public override void SetLayoutHorizontal() {

			this.Arrange();

		}

		public override void SetLayoutVertical() {

			this.Arrange();

		}

		public void Arrange() {

			var rX = this.radiusX;
			var rY = this.radiusY;

			if (this.manualSize == false) {
				
				rX = this.rectTransform.rect.width * 0.5f;
				rY = this.rectTransform.rect.height * 0.5f;

			}

			var rYOffset = this.padding.bottom + this.padding.top;
			rY -= rYOffset;
			var rXOffset = this.padding.left + this.padding.right;
			rX -= rXOffset;
			var center = Vector2.zero;
			var xOffset = rXOffset - (float)this.padding.right * 2f;
			var yOffset = rYOffset - (float)this.padding.top * 2f;
			center.x = xOffset;
			center.y = yOffset;

			var cPivot = Vector2.one * 0.5f;

			this.lastIndex = 0;

			var items = this.rectChildren;
			var count = items.Count;

			for (int i = 0; i < count; ++i) {

				var item = items[i];
				if (item == null) continue;

				item.anchoredPosition3D = this.GetPosition((this.reverse == true ? count - i - 1 : i), count, item.anchoredPosition3D.z, rX, rY, this.bothSided, this.bothSidedSorted, this.startAngle, this.maxAnglePerElement, this.angle) + center.XY();
				#if UNITY_EDITOR
				if (Application.isPlaying == false) {

					this.m_Tracker.Add(this, item, DrivenTransformProperties.AnchoredPosition3D | DrivenTransformProperties.Pivot | DrivenTransformProperties.Anchors);

				}
				#endif

				if (item.anchorMin != cPivot) item.anchorMin = cPivot;
				if (item.anchorMax != cPivot) item.anchorMax = cPivot;
				if (item.pivot != cPivot) item.pivot = cPivot;

			}

			#if UNITY_EDITOR
			if (Application.isPlaying == false) {

				this.m_Tracker.Add(this, this.rectTransform, DrivenTransformProperties.Pivot);

			}
			#endif
			
			if (this.rectTransform.pivot != cPivot) this.rectTransform.pivot = cPivot;

		}
		
		public Vector2 ArrangeByCircle(int iteration, int count, float angle, float radiusX, float radiusY, float startAngle = 0f) {
			
			angle = angle * Mathf.Deg2Rad;
			startAngle = startAngle * Mathf.Deg2Rad;
			
			Vector2 v = Vector2.zero;
			v.x = Mathf.Sin(angle / count * iteration + startAngle) * radiusX;
			v.y = Mathf.Cos(angle / count * iteration + startAngle) * radiusY;

			return v;
			
		}

		private int lastIndex = 0;
		private Vector3 GetPosition(int index, int count, float depth, float radiusX, float radiusY, bool bothSided, bool bothSidedSorted, float startAngle, float maxAnglePerElement, float angle) {

			if (count <= 0) {

				return Vector3.zero;

			}

			float _startAngle = 0f;
			
			switch (this.childAlignment) {

				case TextAnchor.MiddleCenter:
					_startAngle = 0f;
					break;

				case TextAnchor.LowerCenter:
					_startAngle = 180f;
					break;
					
				case TextAnchor.UpperCenter:
					_startAngle = 0f;
					break;
					
				case TextAnchor.MiddleLeft:
					_startAngle = -90f;
					break;
					
				case TextAnchor.MiddleRight:
					_startAngle = 90f;
					break;
					
				case TextAnchor.UpperLeft:
					_startAngle = -45f;
					break;
					
				case TextAnchor.UpperRight:
					_startAngle = 45f;
					break;
					
				case TextAnchor.LowerLeft:
					_startAngle = -135f;
					break;
					
				case TextAnchor.LowerRight:
					_startAngle = 135f;
					break;
					
			}
			
			_startAngle += startAngle;

			var elementAngle = angle / count;
			var offset = false;
			
			var non360 = angle < 360f;
			
			offset = (elementAngle >= maxAnglePerElement);
			
			if (bothSided == true) {

				if (bothSidedSorted == true) {

					_startAngle -= elementAngle * count * 0.5f - elementAngle * 0.5f;

				} else {

					if (index % 2 == 0) {
						
						index = this.lastIndex - index;
						
					} else {
						
						index = this.lastIndex + index;
						
					}
					
					var elementAngleWithOffset = (offset == true ? maxAnglePerElement : elementAngle);
					if (count % 2 == 0) _startAngle -= elementAngleWithOffset * 0.5f;

				}

			} else {
				
				if (non360 == true) --count;
				
			}
			
			if (offset == true) {
				
				angle *= maxAnglePerElement / elementAngle;
				
			}

			if (count == 0) {

				count = 1;

			}

			var position = this.ArrangeByCircle(index, count, angle, radiusX, radiusY, _startAngle);
			this.lastIndex = index;
			
			return new Vector3(position.x, position.y, depth);
			
		}

		#if UNITY_EDITOR
		protected override void OnValidate() {

			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			base.OnValidate();
			
			if (this.angle <= 0f) {
				
				this.angle = 0.01f;
				
			}

		}

		public void OnDrawGizmosSelected() {
			
			if (Application.isPlaying == true) return;

			const int smooth = 50;
			
			var scale = 1f;
			var canvas = this.GetComponentsInParent<Canvas>().FirstOrDefault((c) => c.isRootCanvas);
			if (canvas != null) {
				
				scale = canvas.transform.localScale.x;
				
			}

			var rX = this.radiusX;
			var rY = this.radiusY;
			
			if (this.manualSize == false) {
				
				rX = this.rectTransform.rect.width * 0.5f;
				rY = this.rectTransform.rect.height * 0.5f;
				
			}
			
			var rYOffset = this.padding.bottom + this.padding.top;
			rY -= rYOffset;
			var rXOffset = this.padding.left + this.padding.right;
			rX -= rXOffset;
			var center = Vector2.zero;
			var xOffset = rXOffset - this.padding.right * 2f;
			var yOffset = rYOffset - this.padding.top * 2f;
			center.x = xOffset;
			center.y = yOffset;

			center *= scale;
			var pointRadius = Mathf.Min(rX, rY) * 0.1f;

			var color = Color.yellow;
			color.a = 0.7f;
			UnityEditor.Handles.color = color;

			UnityEditor.Handles.DrawSolidDisc(this.transform.position + center.XY(), Vector3.back, pointRadius * scale);

			var prevPos = this.transform.position + this.GetPosition(0, smooth, 0f, rX * scale, rY * scale, bothSided: false, bothSidedSorted: false, startAngle: 0f, maxAnglePerElement: 360f, angle: 360f) + center.XY();
			for (int i = 0; i <= smooth; ++i) {

				var curPos = this.transform.position + this.GetPosition(i, smooth, 0f, rX * scale, rY * scale, bothSided: false, bothSidedSorted: false, startAngle: 0f, maxAnglePerElement: 360f, angle: 360f) + center.XY();
				UnityEditor.Handles.DrawLine(prevPos, curPos);
				prevPos = curPos;

			}

			UnityEditor.Handles.DrawLine(this.transform.position + center.XY(), this.transform.position + this.GetPosition(0, 2, 0f, rX * scale, rY * scale, bothSided: false, bothSidedSorted: false, startAngle: this.startAngle, maxAnglePerElement: this.maxAnglePerElement, angle: this.angle) + center.XY());
			
			color = Color.gray;
			color.a = 0.5f;
			UnityEditor.Handles.color = color;

			var items = this.rectChildren;
			for (int i = 0; i < items.Count; ++i) {
				
				var pos = this.GetPosition(i, items.Count, items[i].anchoredPosition3D.z, rX * scale, rY * scale, this.bothSided, this.bothSidedSorted, this.startAngle, this.maxAnglePerElement, this.angle) + center.XY();
				
				UnityEditor.Handles.DrawLine(this.transform.position + center.XY(), this.transform.position + pos);
				UnityEditor.Handles.DrawSolidDisc(this.transform.position + pos, Vector3.back, pointRadius * scale);

			}

		}
		#endif

	}

}
