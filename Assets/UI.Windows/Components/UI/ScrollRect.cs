using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Extensions {
	
	[ExecuteInEditMode()]
	public class ScrollRect : UnityEngine.UI.ScrollRect {
		
		public LayoutElement layoutElement;

		protected override void Start() {

			base.Start();

			this.scrollSensitivity = WindowSystemInput.GetScrollSensitivity();

		}

		/*private bool horizontalLast;
		private bool verticalLast;

		private bool asDropdown = false;
		private float maxHeight;

		protected override void Start() {

			base.Start();
			
			this.horizontalLast = this.horizontal;
			this.verticalLast = this.vertical;

			this.UpdateScrollBars();

		}

		protected override void LateUpdate() {

			base.LateUpdate();
			
			if (this.horizontalLast != this.horizontal ||
			    this.verticalLast != this.vertical) {
				
				this.UpdateScrollBars();
				
			}

		}

		protected void UpdateScrollBars() {
			
			if (this.horizontalScrollbar != null) this.horizontalScrollbar.interactable = this.horizontal;
			if (this.verticalScrollbar != null) this.verticalScrollbar.interactable = this.vertical;

			this.horizontalLast = this.horizontal;
			this.verticalLast = this.vertical;

		}
		
		protected override void OnRectTransformDimensionsChange() {

			base.OnRectTransformDimensionsChange();
			
			this.UpdateDropdown();
			
		}
		
		protected override void OnTransformParentChanged() {

			base.OnTransformParentChanged();

			this.UpdateDropdown();
			
		}

		public void UpdateDropdown() {

			if (this.asDropdown == false) return;
			if (this.IsActive() == false) return;

			if (this.layoutElement != null) {
				
				var rect = this.transform as RectTransform;
				
				if (rect.sizeDelta.y >= this.maxHeight) {
					
					this.vertical = true;
					
					this.layoutElement.minHeight = maxHeight;
					this.layoutElement.preferredHeight = -1f;
					
				} else {
					
					this.vertical = false;
					
					this.layoutElement.minHeight = -1f;
					this.layoutElement.preferredHeight = maxHeight;
					
				}

				this.UpdateScrollBars();
				
			}

		}

		public void SetupAsDropdown(float maxHeight) {

			this.asDropdown = true;

			this.maxHeight = maxHeight;
			this.UpdateDropdown();

		}*/

		#if UNITY_EDITOR
		protected override void OnValidate() {

			base.OnValidate();

			this.layoutElement = this.GetComponent<LayoutElement>();

		}
		#endif

	}

}