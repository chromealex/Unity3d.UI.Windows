using UnityEngine.UI.Windows;
using UnityEngine.Events;
using UnityEngine.UI.Windows.Components.Events;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

namespace UnityEngine.UI.Windows.Components {

	public class PopupComponent : WindowComponent {
		
		//private List<WindowComponent> components = new List<WindowComponent>();

		public LinkerComponent buttonWithText;
		public LinkerComponent list;

		private ButtonWithTextComponent label;
		private List items;

		private bool opened;

		public override void OnInit() {

			base.OnInit();

			this.opened = false;
			this.selectedIndex = -1;

			this.buttonWithText.Get(ref this.label);
			this.list.Get(ref this.items);

			this.label.SetText(string.Empty);
			this.label.SetCallback(this.Toggle);

			WindowSystemInput.onPointerDown.AddListener(this.OnPressDown);

		}

		public override void OnDeinit() {

			base.OnDeinit();

			WindowSystemInput.onPointerDown.RemoveListener(this.OnPressDown);

		}

		public override void OnShowBegin(System.Action callback) {

			base.OnShowBegin(callback);

			this.SetState(this.opened, immediately: true);

		}

		public void Toggle() {

			this.opened = !this.opened;

			this.SetState(this.opened, immediately: false);

		}

		private void OnPressDown() {

			if (this.opened == false) return;

			var current = EventSystem.current.currentSelectedGameObject;
			if (current == this.label.gameObject) return;

			if (current == null || current.GetComponentsInParent<CanvasGroup>().Contains(this.list.canvas) == false) {
				
				// Close
				this.SetState(opened: false, immediately: false);
				
			}
			
		}

		public void SetState(bool opened, bool immediately) {

			this.opened = opened;

			if (opened == true) {
				
				this.list.gameObject.SetActive(true);

				// Show items
				this.list.Show(() => {

					// shown

				}, resetAnimation: true);

			} else {

				// Hide items
				this.list.Hide(() => {

					this.list.gameObject.SetActive(false);

				}, immediately);

			}

		}

		public T AddItem<T>() where T : ITextComponent {

			return this.items.AddItem<T>();

		}

		public List<T> GetItems<T>() where T : ITextComponent {
			
			return this.items.GetItems<T>();
			
		}
		
		public T GetItem<T>(int index) where T : ITextComponent {
			
			return this.items.GetItem<T>(index);
			
		}
		
		public void SetItems(int capacity, UnityAction<IComponent> onItem = null) {
			
			this.items.SetItems(capacity, onItem);
			
		}
		
		public void SetItems<T>(int capacity, UnityAction<T, int> onItem = null) where T : ITextComponent {

			this.items.SetItems<T>(capacity, (element, index) => {

				if (element is ButtonComponent) {

					(element as ButtonComponent).SetCallback((button) => {

						this.Select(this.items.GetIndexOf(button));

					});

				}

				if (onItem != null) onItem.Invoke(element, index);

			});

		}

		private int selectedIndex = -1;
		public void Select(int index, bool closePopup = true) {

			var text = this.items.GetItem<ITextComponent>(index);

			this.selectedIndex = index;
			this.label.SetText(text.GetText());

			if (closePopup == true) {

				this.SetState(opened: false, immediately: false);

			}

		}

		public int GetSelectedIndex() {

			return this.selectedIndex;

		}

		/*
		public T1 AddItem<T1, T2>(T2 component) where T1 : ButtonComponent
												where T2 : WindowComponent {
			
			var element = this.items.AddItem<T1>();
			element.SetCallback((button) => {
				
				this.Select(this.items.GetIndexOf(button));
				
			});
			
			this.components.Add(component);
			
			return element;
			
		}
		
		public void SetItems<T1, T2>(int capacity, UnityAction<T, int> onItem = null) where T : WindowComponent {

			for (int i = 0; i < capacity; ++i) {
				
				var instance = this.AddItem<T1, T2>();
				if (instance != null && onItem != null) onItem.Invoke(instance, i);
				
			}
			
		}*/

		/*
		private ComponentEvent<WindowComponent, int> onSelect = new ComponentEvent<WindowComponent, int>();

		public void SetCallback(UnityAction<WindowComponent, int> onSelect) {

			this.onSelect.AddListenerDistinct(onSelect);

		}
		
		public T1 AddItem<T1, T2>(T2 component) where T1 : ButtonComponent
												where T2 : WindowComponent {
			
			var element = this.AddItem<T1>();
			element.SetCallback((button) => {
				
				this.Select(this.GetIndexOf(button));
				
			});
			
			this.components.Add(component);
			
			return element;
			
		}

		public void Select(int index) {

			var item = this.GetItem<WindowComponent>(index) as IPopupItem;
			if (item != null) {

				item.GetLabel();

			}

		}*/

	}
	
}