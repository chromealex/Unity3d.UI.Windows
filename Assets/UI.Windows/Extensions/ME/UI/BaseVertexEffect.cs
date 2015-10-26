#if UNITY_5_0 || UNITY_5_1
#define PRE_UNITY_5_2
#endif
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ME {

	public abstract class BaseVertexEffect : 
	#if !PRE_UNITY_5_2
	BaseMeshEffect
	#else
	UnityEngine.UI.BaseVertexEffect
	#endif
	{

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

        public abstract void ModifyVertices(List<UIVertex> verteces);
		
		public override void ModifyMesh(VertexHelper helper) {



		}

    #endif

    }

}