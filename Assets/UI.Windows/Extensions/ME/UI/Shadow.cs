#if UNITY_5_0 || UNITY_5_1
#define PRE_UNITY_5_2
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ME {

	public abstract class Shadow : UnityEngine.UI.Shadow {

		#if !PRE_UNITY_5_2
		public override void ModifyMesh(Mesh mesh) {

			if (this.IsActive() == false) {
				
				return;
				
			}
			
			var list = new List<UIVertex>();
			using (var vertexHelper = new VertexHelper(mesh)) {
				
				vertexHelper.GetUIVertexStream(list);
				
			}
			
			this.ModifyVertices(list);  // calls the old ModifyVertices which was used on pre 5.2
			
			using (var vertexHelper = new VertexHelper()) {
				
				vertexHelper.AddUIVertexTriangleStream(list);
				vertexHelper.FillMesh(mesh);
				
			}
			
		}
		#endif
		
		public abstract void ModifyVertices(List<UIVertex> vertices);

	}

}