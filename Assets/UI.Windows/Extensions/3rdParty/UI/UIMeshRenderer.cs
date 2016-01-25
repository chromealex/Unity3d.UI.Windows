using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(CanvasRenderer))]
public class UIMeshRenderer : MaskableGraphic {

	//public CanvasRenderer canvasRenderer;

	//public Material material;
	//public float scale = 1f;
	
	private Mesh _mesh;
	public Mesh mesh {
		
		set {
			
			this._mesh = value;
			//this.SetMesh();
			
		}
		
		get {
			
			return this._mesh;
			
		}
		
	}

	#if UNITY_EDITOR // only compile in editor
	private Mesh currentMesh;
	private Material currentMaterial;
	private float currentScale;
	#endif
	
	/*public void OnEnable() {

		this.SetMesh();

	}

	public void OnDisable() {

		//this.canvasRenderer.Clear();

	}*/

	/*#if UNITY_EDITOR // only compile in editor
	public void Update() {

		if (this.mesh != this.currentMesh || this.material != this.currentMaterial || !Mathf.Approximately(this.scale, this.currentScale)) {

			this.SetMesh();

		}

	}
	#endif*/

	protected override void OnPopulateMesh(VertexHelper vh) {

		if (this.mesh == null) return;

		var color32 = this.color;
		var uv = Vector4.zero;

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

	/*public void SetMesh() {

		//this.canvasRenderer.Clear();
		
		#if UNITY_EDITOR // only compile in editor
		this.currentMesh = this.mesh;
		this.currentMaterial = this.material;
		this.currentScale = this.scale;
		#endif
		
		if (this.mesh == null) {

			Debug.LogWarning("Mesh is null.");
			return;

		} else if (this.material == null) {

			Debug.LogWarning("Material is null.");
			return;

		}

		//this.canvasRenderer.SetMaterial(this.material, null);
		//this.canvasRenderer.SetMesh(this.mesh);

	}*/
	
	/*public void ConvertMesh() {

		Vector3[] vertices = this.mesh.vertices;

		for (int i = 0; i < vertices.Length; ++i) {

			vertices[i] = (vertices[i] - mesh.bounds.center) * scale;

		}

		this.mesh.vertices = vertices;

	}*/

}