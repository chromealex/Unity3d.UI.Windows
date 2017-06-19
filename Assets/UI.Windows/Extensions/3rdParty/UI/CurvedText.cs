using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	public class CurvedText : BaseMeshEffect
	{
		public AnimationCurve curveForText = AnimationCurve.Linear (0, 0, 1, 10);
		public float curveMultiplier = 1;
		private RectTransform rectTrans;


		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			if (curveForText [0].time != 0) {
				var tmpRect = curveForText [0];
				tmpRect.time = 0;
				curveForText.MoveKey (0, tmpRect);
			}
			if (rectTrans == null)
				rectTrans = GetComponent<RectTransform> ();
			if (curveForText [curveForText.length - 1].time != rectTrans.rect.width)
				OnRectTransformDimensionsChange ();
		}
		#endif
		protected override void Awake ()
		{
			base.Awake ();
			rectTrans = GetComponent<RectTransform> ();
			OnRectTransformDimensionsChange ();
		}
		protected override void OnEnable ()
		{
			base.OnEnable ();
			rectTrans = GetComponent<RectTransform> ();
			OnRectTransformDimensionsChange ();
		}
		public override void ModifyMesh (Mesh mesh)
		{
			if (!this.IsActive())
				return;

			List<UIVertex> list = new List<UIVertex>();
			using (VertexHelper vertexHelper = new VertexHelper(mesh))
			{
				vertexHelper.GetUIVertexStream(list);
			}

			ModifyVertices(list);  // calls the old ModifyVertices which was used on pre 5.2

			using (VertexHelper vertexHelper2 = new VertexHelper())
			{
				vertexHelper2.AddUIVertexTriangleStream(list);
				vertexHelper2.FillMesh(mesh);
			}
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!this.IsActive())
				return;

			List<UIVertex> vertexList = new List<UIVertex>();
			vh.GetUIVertexStream(vertexList);

			ModifyVertices(vertexList);

			vh.Clear();
			vh.AddUIVertexTriangleStream(vertexList);
		}

		public void ModifyVertices (List<UIVertex> verts)
		{
			if (!IsActive ())
				return;

			for (int index = 0; index < verts.Count; index++) {
				var uiVertex = verts [index];
				//Debug.Log ();
				uiVertex.position.y += curveForText.Evaluate (rectTrans.rect.width * rectTrans.pivot.x + uiVertex.position.x) * curveMultiplier;
				verts [index] = uiVertex;
			}
		}


		protected override void OnRectTransformDimensionsChange ()
		{
			if (rectTrans == null) return;
			var tmpRect = curveForText [curveForText.length - 1];
			tmpRect.time = rectTrans.rect.width;
			curveForText.MoveKey (curveForText.length - 1, tmpRect);
		}
	}
}