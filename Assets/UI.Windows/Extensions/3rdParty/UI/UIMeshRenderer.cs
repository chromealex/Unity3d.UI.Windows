using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(CanvasRenderer))]
public class UIMeshRenderer : MaskableGraphic {

	public Mesh mesh;

	protected override void OnPopulateMesh(VertexHelper vh) {

		if (this.mesh == null) return;

		var color32 = this.color;

		vh.Clear();

		var vs = this.mesh.vertices;

		for (int i = 0; i < vs.Length; ++i) {

			var v = vs[i];
			vh.AddVert(new Vector3(v.x, v.y), color32, Vector2.zero);

		}

		var ts = this.mesh.triangles;
		
		for (int i = 0; i < ts.Length; i += 3) {

			var t1 = ts[i];
			var t2 = ts[i + 1];
			var t3 = ts[i + 2];

			vh.AddTriangle(t1, t2, t3);

		}

	}

}