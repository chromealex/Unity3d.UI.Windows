using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows {

	public class WindowComponent : WindowComponentBase, IWindowComponent {

		public bool autoRegisterSubComponents = true;
		[SerializeField]
		private List<WindowComponent> subComponents = new List<WindowComponent>();

		private WindowLayoutBase layoutRoot;
		
		internal void Setup(WindowLayoutBase layoutRoot) {
			
			this.layoutRoot = layoutRoot;
			
			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].Setup(this.layoutRoot);
			
		}
		
		internal override void Setup(WindowBase window) {
			
			base.Setup(window);
			
			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].Setup(window);
			
		}

		public WindowLayoutBase GetLayoutRoot() {
			
			return this.layoutRoot;
			
		}
		
		public void RegisterSubComponent(WindowComponent subComponent) {

			if (this.subComponents.Contains(subComponent) == false) this.subComponents.Add(subComponent);
			
		}
		
		public void UnregisterSubComponent(WindowComponent subComponent) {

			this.subComponents.Remove(subComponent);
			
		}

		public virtual void OnInit() {

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnInit();

		}

		public virtual void OnDeinit() {

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnDeinit();

		}

		public virtual void OnShowBegin() {

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnShowBegin();

		}

		public virtual void OnShowEnd() {

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnShowEnd();

		}

		public virtual void OnHideBegin() {		

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnHideBegin();

		}

		public virtual void OnHideEnd() {

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnHideEnd();
			
		}

		#if UNITY_EDITOR
		public virtual void OnValidate() {

			if (this.autoRegisterSubComponents == true) {

				var components = this.GetComponentsInChildren<WindowComponent>(true).ToList();

				this.subComponents.Clear();
				foreach (var component in components) {

					if (component == this) continue;

					var parents = component.GetComponentsInParent<WindowComponent>(true).ToList();
					parents.Remove(component);

					if (parents.Count > 0 && parents[0] != this) continue;

					this.subComponents.Add(component);

				}

			} else {

				this.subComponents.Clear();

			}

		}
		#endif

	}

}