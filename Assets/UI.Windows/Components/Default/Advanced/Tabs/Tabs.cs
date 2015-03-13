using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {
	
	public class Tabs : Components.List {

		private List<WindowComponent> components = new List<WindowComponent>();

		private WindowLayoutElement layoutContent;
		
		private ComponentEvent<WindowComponent, int> onChangeBefore = new ComponentEvent<WindowComponent, int>();
		private ComponentEvent<WindowComponent, int> onChangeAfter = new ComponentEvent<WindowComponent, int>();
		
		public void SetCallback(UnityAction<WindowComponent, int> onChangeBefore = null, UnityAction<WindowComponent, int> onChangeAfter = null) {
			
			if (onChangeBefore != null) this.onChangeBefore.AddListenerDistinct(onChangeBefore);
			if (onChangeAfter != null) this.onChangeAfter.AddListenerDistinct(onChangeAfter);
			
		}

		public void SetCallbacks(UnityAction<WindowComponent, int> onChangeBefore, UnityAction<WindowComponent, int> onChangeAfter) {
			
			this.onChangeBefore.AddListenerDistinct(onChangeBefore);
			this.onChangeAfter.AddListenerDistinct(onChangeAfter);
			
		}

		public void SetContent(WindowLayoutElement layoutContent) {

			this.layoutContent = layoutContent;

		}
		
		public T1 AddItem<T1, T2>(T2 component) where T1 : ButtonComponent
												where T2 : WindowComponent {

			var element = this.AddItem<T1>();
			element.SetCallback((button) => {

				var index = this.GetIndexOf(button);
				this.Load(index, immediately: false);

			});

			this.components.Add(component);

			return element;

		}

		private int lastIndex = -1;
		public void Load(int index, bool immediately = false) {

			if (index < 0 || index >= this.components.Count) return;
			
			if (this.layoutContent.GetCurrentComponent() != null) this.onChangeBefore.Invoke(this.layoutContent.GetCurrentComponent(), this.lastIndex);

			var component = this.components[index];

			this.layoutContent.Hide(() => {

				this.layoutContent.Unload();
				this.layoutContent.Load(component);
				this.layoutContent.Show();

				this.onChangeAfter.Invoke(this.layoutContent.GetCurrentComponent(), index);

				this.lastIndex = index;

			}, immediately);

		}

		public override void OnDeinit() {

			base.OnDeinit();
			
			this.onChangeBefore.RemoveAllListeners();
			this.onChangeAfter.RemoveAllListeners();

		}

	}
	
}