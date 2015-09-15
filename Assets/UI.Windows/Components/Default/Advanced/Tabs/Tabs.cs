using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components {
	
	public class Tabs : Components.ListComponent {

		private List<WindowComponent> components = new List<WindowComponent>();

		private WindowLayoutElement layoutContent;
		
		private ComponentEvent<WindowComponent, int> onChangeBefore = new ComponentEvent<WindowComponent, int>();
		private ComponentEvent<WindowComponent, int> onChangeAfter = new ComponentEvent<WindowComponent, int>();
		
		private int lastIndex = -1;

		public override void OnInit() {

			base.OnInit();

			this.lastIndex = -1;

		}

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

		public T1 AddItem<T1, T2>(T2 component, UnityAction<T1> onClick = null, bool autoEvents = true) where T1 : ButtonComponent where T2 : WindowComponent {
			
			var element = this.AddItem<T1>();
			element.SetCallback((button) => {
				
				if (autoEvents == true) {

					var index = this.GetIndexOf(button);
					this.Load(index, immediately: false, callback: () => {
						
						if (onClick != null) onClick.Invoke(element);

					});
					
				} else {
					
					if (onClick != null) onClick.Invoke(element);

				}

			});
			
			this.components.Add(component);
			
			return element;
			
		}
		
		public T1 AddItem<T1>(UnityAction<ButtonComponent, int> onClick, bool autoEvents) where T1 : ButtonComponent {
			
			var element = this.AddItem<T1>();
			element.SetCallback((button) => {
				
				var index = this.GetIndexOf(button);

				if (autoEvents == true) {

					this.Load(index, immediately: false, callback: () => {

						if (onClick != null) onClick.Invoke(button, index);

					});

				} else {

					this.Select(index);
					if (onClick != null) onClick.Invoke(button, index);

				}

			});

			return element;
			
		}

		public void Select(int index, bool forced = false) {

			if (this.lastIndex >= 0) {
				
				var lastItem = this.GetItem<ButtonComponent>(this.lastIndex);
				if (lastItem != null) lastItem.SetEnabled();
				
			}
			
			var prevLastIndex = this.lastIndex;
			this.lastIndex = index;

			if (this.lastIndex >= 0) {
				
				var lastItem = this.GetItem<ButtonComponent>(this.lastIndex);
				if (lastItem != null) {
					
					lastItem.SetDisabled();
					if (forced == true || prevLastIndex != index) {

						lastItem.OnClick();

					}
					
				}
				
			}

		}

		public void Load(int index, bool immediately = false, UnityAction callback = null) {

			if (this.lastIndex >= 0) {

				var lastItem = this.GetItem<ButtonComponent>(this.lastIndex);
				if (lastItem != null) lastItem.SetEnabled();

			}

			if (this.layoutContent.GetCurrentComponent() != null) this.onChangeBefore.Invoke(this.layoutContent.GetCurrentComponent(), this.lastIndex);

			WindowComponent component = null;

			if (this.components.Count > 0) {

				if (index < 0 || index >= this.components.Count) return;
				component = this.components[index];

			} else {

				component = this.layoutContent.GetCurrentComponent();

			}

			this.layoutContent.Hide(() => {

				this.layoutContent.Unload(() => {

					this.layoutContent.Load(component);
					this.layoutContent.Show();
					
					this.onChangeAfter.Invoke(this.layoutContent.GetCurrentComponent(), index);
					
					//var prevLastIndex = this.lastIndex;
					this.lastIndex = index;
					
					if (this.lastIndex >= 0) {
						
						var lastItem = this.GetItem<ButtonComponent>(this.lastIndex);
						if (lastItem != null) {
							
							lastItem.SetDisabled();
							//if (prevLastIndex != index) lastItem.OnClick();
							
						}
						
					}
					
					if (callback != null) callback.Invoke();

				});

			}, immediately);

		}

		public override void OnDeinit() {

			base.OnDeinit();
			
			this.onChangeBefore.RemoveAllListeners();
			this.onChangeAfter.RemoveAllListeners();

		}

	}
	
}