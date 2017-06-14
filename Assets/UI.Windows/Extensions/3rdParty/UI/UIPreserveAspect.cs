using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI {

	[/*DisallowMultipleComponent, */AddComponentMenu("UI/Preserve Aspect")]
	public class UIPreserveAspect : MonoBehaviour, IMeshModifier {

		public RawImage rawImage;
		[SerializeField]
		private bool m_PreserveAspect = false;

		public bool preserveAspect {
			get { return this.m_PreserveAspect; }
			set { this.m_PreserveAspect = value; }
		}

		protected void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif

			this.rawImage = this.GetComponent<RawImage>();
			this.GetComponent<Graphic>().SetVerticesDirty();

		}

		//private System.Collections.Generic.List<UIVertex> modifyVertsTemp = new System.Collections.Generic.List<UIVertex>();
		public void ModifyMesh(VertexHelper helper) {

			if (this.rawImage == null || this.rawImage.texture == null) return;

			/*this.modifyVertsTemp.Clear();
			helper.GetUIVertexStream(this.modifyVertsTemp);

			this.ModifyVertices(this.modifyVertsTemp);

			helper.AddUIVertexTriangleStream(this.modifyVertsTemp);*/

			if (this.rawImage != null && this.preserveAspect == true) {

				var vh = helper;
				var drawingDimensions = ME.Utilities.GetDrawingDimensions(this.rawImage, this.preserveAspect, this.rawImage.texture == null ? Vector2.zero : new Vector2(this.rawImage.texture.width, this.rawImage.texture.height), Vector4.zero);
				var vector = new Vector4(this.rawImage.uvRect.x, this.rawImage.uvRect.y, this.rawImage.uvRect.width, this.rawImage.uvRect.height);
				var color = this.rawImage.color;
				/*
				vh.SetUIVertex(new UIVertex() { position = new Vector3(drawingDimensions.x, drawingDimensions.y), color = vh.ver, uv0 = new Vector2(vector.x, vector.y) }, 0);
				vh.SetUIVertex(new UIVertex() { position = new Vector3(drawingDimensions.x, drawingDimensions.w), color = color, uv0 = new Vector2(vector.x, vector.w) }, 1);
				vh.SetUIVertex(new UIVertex() { position = new Vector3(drawingDimensions.z, drawingDimensions.w), color = color, uv0 = new Vector2(vector.z, vector.w) }, 2);
				vh.SetUIVertex(new UIVertex() { position = new Vector3(drawingDimensions.z, drawingDimensions.y), color = color, uv0 = new Vector2(vector.z, vector.y) }, 3);
*/
				vh.Clear();
				vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.y), color, new Vector2(vector.x, vector.y));
				vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.w), color, new Vector2(vector.x, vector.w));
				vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.w), color, new Vector2(vector.z, vector.w));
				vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.y), color, new Vector2(vector.z, vector.y));
				vh.AddTriangle(0, 1, 2);
				vh.AddTriangle(2, 3, 0);

			}

		}

		public void ModifyMesh(Mesh mesh) {}

		public void ModifyVertices(System.Collections.Generic.List<UIVertex> verts) {

			//for (var i = 0; i < verts.Count; ++i) {

			var drawingDimensions = ME.Utilities.GetDrawingDimensions(this.rawImage, this.preserveAspect, this.rawImage.texture == null ? Vector2.zero : new Vector2(this.rawImage.texture.width, this.rawImage.texture.height), Vector4.zero);
			var vector = new Vector4(this.rawImage.uvRect.x, this.rawImage.uvRect.y, this.rawImage.uvRect.width, this.rawImage.uvRect.height);

			var v = verts[0];
			v.position = new Vector3(drawingDimensions.x, drawingDimensions.y);
			v.uv0 = new Vector2(vector.x, vector.y);
			verts[0] = v;

			v = verts[1];
			v.position = new Vector3(drawingDimensions.x, drawingDimensions.w);
			v.uv0 = new Vector2(vector.x, vector.w);
			verts[1] = v;

			v = verts[2];
			v.position = new Vector3(drawingDimensions.z, drawingDimensions.w);
			v.uv0 = new Vector2(vector.z, vector.w);
			verts[2] = v;

			v = verts[3];
			v.position = new Vector3(drawingDimensions.z, drawingDimensions.y);
			v.uv0 = new Vector2(vector.z, vector.y);
			verts[3] = v;

			//}

		}

	}
}