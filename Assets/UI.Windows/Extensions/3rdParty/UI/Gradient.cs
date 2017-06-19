using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI {

	[AddComponentMenu("UI/Effects/Gradient")]
	public class Gradient : ME.BaseVertexEffect {

		public enum DIRECTION : byte {

			Vertical,
			Horizontal,
			Both,

		};
		
		public DIRECTION direction = DIRECTION.Both;
		public Color colorTop = Color.white;
		public Color colorBottom = Color.black;
		public Color colorLeft = Color.red;
		public Color colorRight = Color.blue;

		public override void ModifyVertices(List<UIVertex> vList) {

			if (IsActive() == false || vList == null || vList.Count == 0) {
				return;
			}
			
			float topX = 0f, topY = 0f, bottomX = 0f, bottomY = 0f;
			
			for (int i = 0; i < vList.Count; ++i) {
				var vertex = vList[i];
				topX = Mathf.Max(topX, vertex.position.x);
				topY = Mathf.Max(topY, vertex.position.y);
				bottomX = Mathf.Min(bottomX, vertex.position.x);
				bottomY = Mathf.Min(bottomY, vertex.position.y);
			}
			
			float width = topX - bottomX;
			float height = topY - bottomY;
			
			UIVertex tempVertex = vList[0];
			for (int i = 0; i < vList.Count; ++i) {
				tempVertex = vList[i];
				//byte orgAlpha = tempVertex.color.a;
				Color colorOrg = tempVertex.color;
				Color colorV = Color.Lerp(colorBottom, colorTop, (tempVertex.position.y - bottomY) / height);
				Color colorH = Color.Lerp(colorLeft, colorRight, (tempVertex.position.x - bottomX) / width);
				switch (direction) {
					case DIRECTION.Both:
						tempVertex.color = colorOrg * colorV * colorH;
						break;
					case DIRECTION.Vertical:
						tempVertex.color = colorOrg * colorV;
						break;
					case DIRECTION.Horizontal:
						tempVertex.color = colorOrg * colorH;
						break;
				}
				//tempVertex.color.a *= orgAlpha;
				vList[i] = tempVertex;
			}
		}
		
		/// <summary>
		/// Refresh Gradient Color on playing.
		/// </summary>
		public void Refresh() {
			
			if (this.graphic != null) {
				
				this.graphic.SetVerticesDirty();
				
			}
			
		}

	}

}