using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows {

	public class WindowLayoutBase : WindowComponentBase, IWindowComponentLayout {

		public void Unload() {

			if (this.component != null) {

				this.component.OnDeinit();
				this.component.Recycle();

			}

		}

		private Layout.Component activatorInstance;
		private WindowComponent component;
		public void Load(WindowComponent component) {

			this.activatorInstance.component = component;
			var instance = this.activatorInstance.Create(this.GetWindow(), this as WindowLayoutElement);
			if (instance != null) instance.OnInit();

		}
		
		public virtual void Setup(WindowComponent component, Layout.Component activatorInstance) {
			
			this.activatorInstance = activatorInstance;
			this.component = component;
			if (this.component != null) this.component.Setup(this);
			
		}

		public virtual WindowComponent GetCurrentComponent() {
			
			return this.component;
			
		}
		
		public override void Show(System.Action callback, bool resetAnimation) {

			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < 2) return;
				
				if (callback != null) callback();
				
			};

			if (this.component != null) {

				this.component.OnShowBegin(callbackItem);

			} else {
				
				callbackItem();
				
			}

			base.Show(() => {

				if (this.component != null) this.component.OnShowEnd();
				callbackItem();
				
			}, resetAnimation);
			
		}
		
		public override void Hide(System.Action callback, bool immediately) {
			
			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < 2) return;
				
				if (callback != null) callback();
				
			};

			if (this.component != null) {

				this.component.OnHideBegin(callbackItem);

			} else {

				callbackItem();

			}

			base.Hide(() => {
				
				if (this.component != null) this.component.OnHideEnd();
				callbackItem();
				
			}, immediately);
			
		}

	}

}