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
		public float multiplier = 1f;

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

		public float GetChildAngle() {

			var childAngle = 360f;
			if (this.rectChildren.Count > 1) {

				var first = this.rectChildren[0];
				var second = this.rectChildren[1];

				childAngle = Vector3.Angle(first.anchoredPosition3D, second.anchoredPosition3D);

			}

			childAngle = Mathf.Min(childAngle, this.maxAnglePerElement);

			return childAngle;

		}

		public RectTransform GetElementByAngle(float angle, Vector3 normal) {

			angle = Mathf.Abs(angle % 360f);

			var childAngle = this.GetChildAngle() * 0.5f;

			for (int i = 0; i < this.rectChildren.Count; ++i) {

				var child = this.rectChildren[i];
				var childCenterAngle = Vector3.Angle(normal, child.anchoredPosition3D);

				if (angle >= childCenterAngle - childAngle && angle <= childCenterAngle + childAngle) {

					return child;

				}

			}

			return null;

		}

		public int GetCount() {

			return this.rectChildren.Count;

		}

		public Vector3 GetCenterPosition(float rX, float rY, float scale, Vector2 center) {

			return this.GetPosition(0, 2, 0f, rX * scale, rY * scale, bothSided: false, bothSidedSorted: false, startAngle: this.startAngle, maxAnglePerElement: this.maxAnglePerElement, angle: this.angle) + center.XY();

		}

		public Vector3 GetCenterPosition() {

			var r = this.GetRadiusAndCenter();
			return this.GetCenterPosition(r.x, r.y, 1f, new Vector2(r.z, r.w));

		}

		public Vector3 GetPosition(int index, int count, out Vector3 center3d, float scale = 1f) {

			var r = this.GetRadiusAndCenter();
			center3d = new Vector3(r.z, 0f, r.w);
			return this.GetPosition(index, count, 0f, r.x * scale, r.y * scale, this.bothSided, this.bothSidedSorted, this.startAngle, this.maxAnglePerElement, this.angle) + center3d;

		}

		public void ResetPosition() {

			this.lastIndex = 0;

		}

		public void Arrange() {

			var r = this.GetRadiusAndCenter();
			var rX = r.x;
			var rY = r.y;
			var center = new Vector2(r.z, r.w);
			/*
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
			center.y = yOffset;*/

			var cPivot = Vector2.one * 0.5f;

			this.ResetPosition();

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

		public Vector4 GetRadiusAndCenter() {

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

			return new Vector4(rX, rY, center.x, center.y);

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

			var r = this.GetRadiusAndCenter();
			var rX = r.x;
			var rY = r.y;
			var center = new Vector2(r.z, r.w);

			center *= scale;
			var pointRadius = Mathf.Min(rX, rY) * 0.1f;

			var color = Color.yellow;
			color.a = 0.7f;
			UnityEditor.Handles.color = color;

			UnityEditor.Handles.DrawSolidDisc(this.transform.position + center.XY(), Vector3.back, pointRadius * scale);

			this.ResetPosition();
			var prevPos = this.transform.position + this.GetPosition(0, smooth, 0f, rX * scale, rY * scale, bothSided: false, bothSidedSorted: false, startAngle: 0f, maxAnglePerElement: 360f, angle: 360f) + center.XY();
			for (int i = 0; i <= smooth; ++i) {

				var curPos = this.transform.position + this.GetPosition(i, smooth, 0f, rX * scale, rY * scale, bothSided: false, bothSidedSorted: false, startAngle: 0f, maxAnglePerElement: 360f, angle: 360f) + center.XY();
				UnityEditor.Handles.DrawLine(prevPos, curPos);
				prevPos = curPos;

			}

			UnityEditor.Handles.DrawLine(this.transform.position + center.XY(), this.transform.position + this.GetCenterPosition(rX, rY, scale, center));
			
			color = Color.gray;
			color.a = 0.5f;
			UnityEditor.Handles.color = color;

			this.ResetPosition();
			var items = this.rectChildren;
			for (int i = 0; i < items.Count; ++i) {
				
				//var pos = this.GetPosition(i, items.Count, items[i].anchoredPosition3D.z, rX * scale, rY * scale, this.bothSided, this.bothSidedSorted, this.startAngle, this.maxAnglePerElement, this.angle) + center.XY();
				Vector3 center3d;
				var pos = this.GetPosition(i, items.Count, out center3d, scale);

				UnityEditor.Handles.DrawLine(this.transform.position + center3d, this.transform.position + pos);
				UnityEditor.Handles.DrawSolidDisc(this.transform.position + pos, Vector3.back, pointRadius * scale);

			}

		}
		#endif

	}

}
