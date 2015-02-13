using UnityEngine;

namespace UnityEngine.UI.Windows {

	public class WindowComponent : WindowComponentBase, IWindowComponent {
		
		private WindowLayoutBase layoutRoot;
		
		internal void Setup(WindowLayoutBase layoutRoot) {
			
			this.layoutRoot = layoutRoot;
			
		}
		
		public WindowLayoutBase GetLayoutRoot() {
			
			return this.layoutRoot;
			
		}

		public virtual void OnInit() {}
		public virtual void OnDeinit() {}
		public virtual void OnShowBegin() {}
		public virtual void OnShowEnd() {}
		public virtual void OnHideBegin() {}
		public virtual void OnHideEnd() {}

	}

}