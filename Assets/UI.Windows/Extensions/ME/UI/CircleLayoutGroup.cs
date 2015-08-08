using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions {

	public class CircleLayoutGroup : LayoutGroup {

		public bool bothSided = false;
		public float startAngle = 0f;
		public float maxAnglePerElement = 360f;
		public float angle = 360f;
		public float radius = 100f;

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
			
			var items = this.rectChildren;
			for (int i = 0; i < items.Count; ++i) {

				items[i].anchoredPosition3D = this.GetPosition(i, items.Count, items[i].anchoredPosition3D.z);
				this.m_Tracker.Add(this, items[i], DrivenTransformProperties.AnchoredPosition3D);

			}
			
		}
		
		public Vector2 ArrangeByCircle(int iteration, int count, float angle, float radius, float startAngle = 0f) {
			
			angle = angle * Mathf.Deg2Rad;
			startAngle = startAngle * Mathf.Deg2Rad;
			
			Vector2 v = Vector2.zero;
			v.x = Mathf.Sin(angle / count * iteration + startAngle) * radius;
			v.y = Mathf.Cos(angle / count * iteration + startAngle) * radius;
			
			return v;
			
		}

		private int lastIndex = 0;
		private Vector3 GetPosition(int index, int count, float depth) {
			
			float startAngle = 0f;
			
			switch (this.childAlignment) {
				
				case TextAnchor.LowerCenter:
					startAngle = 180f;
					break;
					
				case TextAnchor.UpperCenter:
					startAngle = 0f;
					break;
					
				case TextAnchor.MiddleLeft:
					startAngle = -90f;
					break;
					
				case TextAnchor.MiddleRight:
					startAngle = 90f;
					break;
					
				case TextAnchor.UpperLeft:
					startAngle = -45f;
					break;
					
				case TextAnchor.UpperRight:
					startAngle = 45f;
					break;
					
				case TextAnchor.LowerLeft:
					startAngle = -135f;
					break;
					
				case TextAnchor.LowerRight:
					startAngle = 135f;
					break;
					
			}
			
			startAngle += this.startAngle;

			var angle = this.angle;
			var radius = this.radius;
			var elementAngle = angle / count;
			var offset = false;
			
			var non360 = angle < 360f;
			
			if (elementAngle >= this.maxAnglePerElement) offset = true;
			var elementAngleWithOffset = offset ? this.maxAnglePerElement : elementAngle;
			
			if (this.bothSided == true) {
				
				if (index % 2 == 0) {
					
					index = this.lastIndex - index;
					
				} else {
					
					index = this.lastIndex + index;
					
				}
				
				if (count % 2 == 0) startAngle -= elementAngleWithOffset * 0.5f;
				
			} else {
				
				if (non360) count--;
				
			}
			
			if (offset == true) {
				
				angle *= this.maxAnglePerElement / elementAngle;
				
			}

			var position = this.ArrangeByCircle(index, count, angle, radius, startAngle);
			this.lastIndex = index;
			
			return new Vector3(position.x, position.y, depth);
			
		}

	}

}
