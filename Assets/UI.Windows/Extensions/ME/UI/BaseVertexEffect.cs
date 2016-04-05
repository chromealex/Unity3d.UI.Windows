using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ME {

	public abstract class BaseVertexEffect : BaseMeshEffect {

		/*public override void ModifyMesh(Mesh mesh) {
			
			Debug.Log("MOD");

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

		}*/

        public abstract void ModifyVertices(List<UIVertex> verteces);

		private List<UIVertex> temp = new List<UIVertex>();
		public override void ModifyMesh(VertexHelper vh) {

			if (this.IsActive() == false) {

				return;

			}

			List<UIVertex> list = this.temp;//ListPool<UIVertex>.Get();
			vh.GetUIVertexStream(list);

			this.ModifyVertices(list);

			vh.Clear();
			vh.AddUIVertexTriangleStream(list);
			//ListPool<UIVertex>.Release(list);
			this.temp.Clear();

		}

    }

}