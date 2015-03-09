using UnityEngine;
using UnityEngine.UI.Windows.Components.List;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components.Tabs {
	
	public class Tabs : Components.List.List {

		private List<WindowComponent> components = new List<WindowComponent>();

		private WindowLayoutElement layoutContent;

		public void SetContent(WindowLayoutElement layoutContent) {

			this.layoutContent = layoutContent;

		}
		
		public T1 AddItem<T1, T2>(T2 component) where T1 : ButtonComponent
												where T2 : WindowComponent {

			var element = this.AddItem<T1>();
			element.SetCallback((button) => {

				this.Load(this.GetIndexOf(button));

			});

			this.components.Add(component);

			return element;

		}

		public void Load(int index) {

			if (index < 0 || index >= this.components.Count) return;

			var component = this.components[index];
			
			this.layoutContent.Hide(() => {

				this.layoutContent.Unload();
				this.layoutContent.Load(component);
				this.layoutContent.Show();

			});

		}

	}
	
}