using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI {

	[AddComponentMenu("UI/Effects/Gradient")]
	public class Gradient : BaseVertexEffect {

		public GradientMode gradientMode = GradientMode.Global;
		public GradientDir gradientDir = GradientDir.Vertical;
		public ColorMode colorMode = ColorMode.Multiply;

		public bool overwriteAllColor = false;

		public Color vertex1 = Color.white;
		public Color vertex2 = Color.black;

		[ReadOnly("gradientDir", GradientDir.DiagonalLeftToRight | GradientDir.DiagonalRightToLeft, bitMask: true)]
		//[Range(0f, 1f)]
		public float lerpPosition = 0.5f;

		private Graphic targetGraphic;

		protected override void Start() {
			targetGraphic = GetComponent<Graphic>();
		}

		public override void ModifyVertices(List<UIVertex> vertexList) {
			if (!IsActive() || vertexList.Count == 0) {
				return;
			}
			int count = vertexList.Count;
			UIVertex uiVertex = vertexList[0];
			if (gradientMode == GradientMode.Global) {
				if (gradientDir == GradientDir.DiagonalLeftToRight || gradientDir == GradientDir.DiagonalRightToLeft) {
#if UNITY_EDITOR				
						Debug.LogError ("Diagonal dir is not supported in Global mode");
#endif
					gradientDir = GradientDir.Vertical;
				}
				float bottomY = gradientDir == GradientDir.Vertical ? vertexList[vertexList.Count - 1].position.y : vertexList[vertexList.Count - 1].position.x;
				float topY = gradientDir == GradientDir.Vertical ? vertexList[0].position.y : vertexList[0].position.x;

				float uiElementHeight = topY - bottomY;

				for (int i = 0; i < count; i++) {
					uiVertex = vertexList[i];
					if (!overwriteAllColor && uiVertex.color != targetGraphic.color)
						continue;
					var targetColor = Color.Lerp(vertex2, vertex1, ((gradientDir == GradientDir.Vertical ? uiVertex.position.y : uiVertex.position.x) - bottomY) / uiElementHeight);
					this.ApplyVertexColor(ref uiVertex, targetColor);
					uiVertex.color *= targetColor;
					vertexList[i] = uiVertex;
				}
			} else {
				for (int i = 0; i < count; i++) {
					uiVertex = vertexList[i];
					if (!overwriteAllColor && !CompareCarefully(uiVertex.color, targetGraphic.color))
						continue;

					Color targetColor = Color.white;

					switch (gradientDir) {
						case GradientDir.Vertical:
							targetColor = (i % 4 == 0 || (i - 3) % 4 == 0) ? vertex1 : vertex2;
							break;
						case GradientDir.Horizontal:
							targetColor = (i % 4 == 0 || (i - 1) % 4 == 0) ? vertex1 : vertex2;
							break;
						case GradientDir.DiagonalLeftToRight:
							targetColor = (i % 4 == 0) ? vertex1 : ((i - 2) % 4 == 0 ? vertex2 : Color.Lerp(vertex2, vertex1, this.lerpPosition));
							break;
						case GradientDir.DiagonalRightToLeft:
							targetColor *= ((i - 1) % 4 == 0) ? vertex1 : ((i - 3) % 4 == 0 ? vertex2 : Color.Lerp(vertex2, vertex1, this.lerpPosition));
							break;

					}
					this.ApplyVertexColor(ref uiVertex, targetColor);
					vertexList[i] = uiVertex;
				}
			}
		}

		private void ApplyVertexColor(ref UIVertex vertex, Color color) {

			switch (this.colorMode) {
				
				case ColorMode.Additive:
					vertex.color += color;
					break;
					
				case ColorMode.Multiply:
					vertex.color *= color;
					break;
					
				case ColorMode.Overwrite:
					vertex.color = color;
					break;

			}

		}

		private bool CompareCarefully(Color col1, Color col2) {
			if (Mathf.Abs(col1.r - col2.r) < 0.003f && Mathf.Abs(col1.g - col2.g) < 0.003f && Mathf.Abs(col1.b - col2.b) < 0.003f && Mathf.Abs(col1.a - col2.a) < 0.003f)
				return true;
			return false;
		}
	}

	public enum GradientMode {
		Global,
		Local
	}

	public enum GradientDir {
		Vertical = 0x1,
		Horizontal = 0x2,
		DiagonalLeftToRight = 0x4,
		DiagonalRightToLeft = 0x8
		//Free
	}

	public enum ColorMode {
		Additive,
		Multiply,
		Overwrite,
	}
	//enum color mode Additive, Multiply, Overwrite

}