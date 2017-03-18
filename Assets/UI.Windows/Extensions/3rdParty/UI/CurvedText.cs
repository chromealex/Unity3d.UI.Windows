/// Credit Breyer
/// Sourced from - http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/#post-1777407

namespace UnityEngine.UI.Windows.Extensions
{
	[RequireComponent(typeof(Text), typeof(RectTransform))]
	[AddComponentMenu("UI/Effects/Extensions/Curved Text")]
	public class CurvedText : BaseMeshEffect
	{
		public float curveMultiplierX = 1f;

		public AnimationCurve curveForTextUpper;
		public float curveMultiplierUpper = 1f;
		public AnimationCurve curveForText;
		public float curveMultiplier = 1f;
		private RectTransform rectTrans;

		#if UNITY_EDITOR
		protected override void OnValidate()
		{

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			base.OnValidate();

			if (rectTrans == null)
				rectTrans = GetComponent<RectTransform>();
			if (curveForText[curveForText.length - 1].time != rectTrans.rect.width)
				OnRectTransformDimensionsChange();
		}
		#endif
		protected override void Awake()
		{
			base.Awake();
			rectTrans = GetComponent<RectTransform>();
			OnRectTransformDimensionsChange();
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			rectTrans = GetComponent<RectTransform>();
			OnRectTransformDimensionsChange();
		}
		public override void ModifyMesh(VertexHelper vh)
		{
			int count = vh.currentVertCount;
			if (!IsActive() || count == 0)
			{
				return;
			}

			var k = 0;
			var upCount = 0;
			float progressMax = vh.currentVertCount * 0.5f;
			for (int index = 0; index < vh.currentVertCount; ++index) {

				UIVertex uiVertex = new UIVertex();
				vh.PopulateUIVertex(ref uiVertex, index);

				if (k < 2) {

					var progress = upCount++ - progressMax * 0.5f;
					uiVertex.position.y += curveForTextUpper.Evaluate((rectTrans.rect.width * rectTrans.pivot.x + uiVertex.position.x) / rectTrans.rect.width) * curveMultiplierUpper;
					uiVertex.position.x += progress * curveMultiplierX;

				} else if (k < 4) {

					uiVertex.position.y += curveForText.Evaluate((rectTrans.rect.width * rectTrans.pivot.x + uiVertex.position.x) / rectTrans.rect.width) * curveMultiplier;

				}

				++k;

				if (k >= 4) k = 0;

				vh.SetUIVertex(uiVertex, index);

			}

		}

	}
}