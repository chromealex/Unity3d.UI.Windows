using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Types {

	[ExecuteInEditMode()]
	public class TipWindowType : WindowBase {

		public RectTransform root;
		public WindowLayoutBase animationRoot;

		[HideInInspector][SerializeField]
		public Canvas canvas;
		[HideInInspector][SerializeField]
		public UnityEngine.EventSystems.BaseRaycaster raycaster;
		[HideInInspector][SerializeField]
		public bool initializedChild = false;

		private HUDItem tempHud;

		public void OnHover(Transform worldElement, Vector3 worldPoint, Camera gameCamera, Vector3 offset = default(Vector3)) {
			
			if (this.animationRoot != null) this.animationRoot.SetInState();

			if (this.tempHud == null) this.tempHud = this.root.GetComponent<HUDItem>();
			if (this.tempHud == null) this.tempHud = this.root.gameObject.AddComponent<HUDItem>();

			this.tempHud.InitHUD(worldElement, this.workCamera, gameCamera, offset);
			this.tempHud.enabled = true;

			if (this.animationRoot != null) this.animationRoot.SetResetState();
			
		}

		public void OnHover(RectTransform uiElement) {
			
			if (this.tempHud != null) this.tempHud.enabled = false;

			if (this.animationRoot != null) this.animationRoot.SetInState();

			var canvas = uiElement.root.GetComponent<Canvas>();
			if (canvas == null) canvas = uiElement.GetComponentsInParent<Canvas>()[0];

			var scaleOut = canvas.transform.localScale.x;

			var pos = uiElement.position / scaleOut;
			this.root.position = pos;

			var anchor = this.root.anchoredPosition3D;
			anchor.z = 0f;
			this.root.anchoredPosition3D = anchor + Vector3.up * uiElement.sizeDelta.y * 0.5f;
			
			if (this.animationRoot != null) this.animationRoot.SetResetState();

		}
		
		public void OnLeave() {
			
			
			
		}
		
		protected override void OnLayoutInit(float depth, int raycastPriority, int orderInLayer) {

			if (this.animationRoot != null) this.animationRoot.OnInit();

		}
		protected override void OnLayoutDeinit() {

			if (this.animationRoot != null) this.animationRoot.OnDeinit();
			
		}
		protected override void OnLayoutHideEnd() {

			if (this.animationRoot != null) this.animationRoot.OnHideEnd();

		}
		protected override void OnLayoutShowEnd() {

			if (this.animationRoot != null) this.animationRoot.OnShowEnd();

		}

		protected override void OnLayoutShowBegin(System.Action callback) {

			if (this.animationRoot != null) {
				
				this.animationRoot.OnShowBegin(callback);
				
			} else {
				
				if (callback != null) callback();
				
			}
			
		}

		protected override void OnLayoutHideBegin(System.Action callback) {

			if (this.animationRoot != null) {

				this.animationRoot.OnHideBegin(callback);

			} else {

				if (callback != null) callback();

			}

		}

		protected override Transform GetLayoutRoot() {

			return this.root;

		}

		public override float GetLayoutAnimationDuration(bool forward) {

			if (this.animationRoot == null) return 0f;

			return this.animationRoot.GetAnimationDuration(forward);

		}

		#if UNITY_EDITOR
		public override void OnValidate() {
			
			base.OnValidate();

			this.Update_EDITOR();
			
		}

		private void Update_EDITOR() {

			if (this.animationRoot != null) this.animationRoot.Update_EDITOR();

			#region COMPONENTS
			this.canvas = this.GetComponentsInChildren<Canvas>(true)[0];
			var raycasters = this.GetComponentsInChildren<UnityEngine.EventSystems.BaseRaycaster>(true);
			if (raycasters != null && raycasters.Length > 0) this.raycaster = raycasters[0];
			#endregion
			
			this.initializedChild = (this.canvas != null);
			
			#region SETUP
			if (this.initializedChild == true) {
				
				// Canvas
				this.canvas.overrideSorting = true;
				this.canvas.sortingLayerName = "Windows";
				this.canvas.sortingOrder = 0;
				
				// Raycaster
				if ((this.raycaster as GraphicRaycaster) != null) {
					
					(this.raycaster as GraphicRaycaster).GetType().GetField("m_BlockingMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue((this.raycaster as GraphicRaycaster), (LayerMask)(1 << this.gameObject.layer));
					
				}
				
			}
			#endregion

		}
		#endif

	}
	
}