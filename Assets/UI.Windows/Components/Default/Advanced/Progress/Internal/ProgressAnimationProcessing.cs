using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

	public class ProgressAnimationProcessing : BaseVertexEffect {

		public float duration = 1f;

		public override void ModifyVertices(List<UIVertex> verts) {

			if (this.IsActive() == false) return;

			for (int i = 0; i < verts.Count; ++i) {

				var vertex = verts[i];

				vertex.uv0 = Vector3.Lerp(Vector3.zero, Vector3.one, this.duration);

				verts[i] = vertex;

					/*
				uIVertex.uv1 = new Vector2 (verts.get_Item (i).position.x, verts.get_Item (i).position.y);
				verts.set_Item (i, uIVertex);
*/
			}
		}

	}

}