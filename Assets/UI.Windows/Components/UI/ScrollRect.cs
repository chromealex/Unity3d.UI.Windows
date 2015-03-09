using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Extensions {
	
	[ExecuteInEditMode()]
	public class ScrollRect : UnityEngine.UI.ScrollRect {
		
		private bool horizontalLast;
		private bool verticalLast;

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

			this.horizontalLast = this.horizontal;
			this.verticalLast = this.vertical;

		}

		protected void UpdateScrollBars() {
			
			if (this.horizontalScrollbar != null) this.horizontalScrollbar.interactable = this.horizontal;
			if (this.verticalScrollbar != null) this.verticalScrollbar.interactable = this.vertical;

		}

	}

}