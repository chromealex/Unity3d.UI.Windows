using UnityEngine.UI.Windows;
using UnityEngine.Events;
using UnityEngine.UI.Windows.Components.Events;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

namespace UnityEngine.UI.Windows.Components {

	public class PopupComponent : WindowComponent {

		public enum DropState : byte {

			Auto,
			Down,
			Up,

		}

		[System.Serializable]
		public class State {

			public RectTransform rect;
			public List list;
			public bool reverse;

			public Vector2 anchorMin;
			public Vector2 anchorMax;

			public State(RectTransform rect, List list, Vector2 anchorMin, Vector2 anchorMax, bool reverse) {

				this.rect = rect;
				this.list = list;
				this.reverse = reverse;

				this.anchorMin = anchorMin;
				this.anchorMax = anchorMax;

			}

			public void Set() {
				
				this.rect.anchorMin = this.anchorMin;
				this.rect.anchorMax = this.anchorMax;
				this.rect.anchoredPosition = Vector2.zero;

				var scale = (this.reverse == true) ? new Vector3(1f, -1f, 1f) : new Vector3(1f, 1f, 1f);
				
				//this.rect.localScale = scale;
				this.list.transform.localScale = scale;
				if (this.list.content != null) this.list.content.transform.localScale = scale;
				if (this.list.noElements != null) this.list.noElements.transform.localScale = scale;

			}

		}

		[Header("Popup Component")]
		public LinkerComponent buttonWithText;
		public LinkerComponent list;

		private ITextComponent label;
		private List items;

		private bool opened;

		private State stateDown;
		private State stateUp;

		public DropState dropState = DropState.Auto;
		public float maxHeight = 200f;

		private Extensions.ScrollRect scrollRect;
		private Transform currentParent;
		private int currentParentIndex;

		private ComponentEvent callback = new ComponentEvent();
		private ComponentEvent<ButtonComponent> callbackButton = new ComponentEvent<ButtonComponent>();

		public override void OnInit() {

			base.OnInit();

			this.opened = false;
			this.selectedIndex = -1;

			this.buttonWithText.Get(ref this.label);
			this.list.Get(ref this.items);

			ME.Utilities.FindReferenceParent(this, ref this.scrollRect);

			this.stateDown = new State(this.list.transform as RectTransform, this.items, Vector2.zero, new Vector2(1f, 0f), false);
			this.stateUp = new State(this.list.transform as RectTransform, this.items, new Vector2(0f, 1f), new Vector2(1f, 1f), true);

			var canvas = this.items.GetComponent<Canvas>();
			canvas.overrideSorting = true;
			canvas.sortingOrder = this.GetWindow().GetSortingOrder() + 1;
			canvas.sortingLayerName = this.GetWindow().GetSortingLayerName();

			this.label.SetText(string.Empty);
			if (this.label is ISelectable) (this.label as ISelectable).SetCallback(this.Toggle);

			this.items.SetupAsDropdown(this.maxHeight);

			WindowSystemInput.onPointerDown.AddListener(this.OnPressDown);

			this.UpdateDropState();

		}

		public override void OnDeinit() {
			
			base.OnDeinit();

			this.callback.RemoveAllListeners();
			this.callbackButton.RemoveAllListeners();

			WindowSystemInput.onPointerDown.RemoveListener(this.OnPressDown);

		}

		public override void OnShowBegin(System.Action callback, bool resetAnimation = true) {

			base.OnShowBegin(callback, resetAnimation);

			this.SetState(this.opened, immediately: true);

		}

		public void Toggle() {

			this.opened = !this.opened;

			this.SetState(this.opened, immediately: false);

		}

		private void OnPressDown() {

			if (this.opened == false) return;

			var current = EventSystem.current.currentSelectedGameObject;
			if (current == (this.label as Component).gameObject) return;

			if (current == null || current.GetComponentsInParent<CanvasGroup>().Contains(this.list.canvas) == false) {
				
				// Close
				this.SetState(opened: false, immediately: false);
				
			}
			
		}

		public void SetState(bool opened, bool immediately) {

			this.opened = opened;

			if (opened == true) {

				this.list.gameObject.SetActive(true);

				this.UpdateDropState();

				// Show items
				this.items.Show(() => {

					// shown
					this.UpdateDropState();
					
					if (this.scrollRect != null) {
						
						this.currentParent = this.list.transform.parent;
						this.currentParentIndex = this.list.transform.GetSiblingIndex();
						
						this.list.transform.SetParent(this.scrollRect.transform.parent);
						
					}

				}, resetAnimation: true);

			} else {
				
				if (this.scrollRect != null && this.currentParent != null) {
					
					this.list.transform.SetParent(this.currentParent);
					this.list.transform.SetSiblingIndex(this.currentParentIndex);
					
				}

				this.UpdateDropState();

				// Hide items
				this.items.Hide(() => {

					this.list.gameObject.SetActive(false);

				}, immediately, inactiveOnEnd: true);

			}

		}

		public virtual void SetCallback(UnityAction callback) {

			this.callback.AddListenerDistinct(callback);
			this.callbackButton.RemoveAllListeners();

		}

		public virtual void SetCallback(UnityAction<ButtonComponent> callback) {

			this.callbackButton.AddListenerDistinct(callback);
			this.callback.RemoveAllListeners();

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
			
			ISelectable prevText = null;
			if (this.selectedIndex >= 0) prevText = this.items.GetItem<ISelectable>(this.selectedIndex);
			var text = this.items.GetItem<ITextComponent>(index);

			this.selectedIndex = index;
			this.label.SetText(text.GetText());

			if (prevText != null) {

				prevText.Deselect();

			}

			var selectable = text as ISelectable;
			if (selectable != null) {

				selectable.Select();

			}

			if (closePopup == true) {

				this.SetState(opened: false, immediately: false);

			}

			if (this.callback != null) this.callback.Invoke();
			if (this.callbackButton != null) this.callbackButton.Invoke(this.GetItem<ITextComponent>(index) as ButtonComponent);

		}

		public int GetSelectedIndex() {

			return this.selectedIndex;

		}

		public void UpdateDropState() {
			
			if (this.dropState == DropState.Down) {
				
				this.stateDown.Set();
				
			} else if (this.dropState == DropState.Up) {
				
				this.stateUp.Set();
				
			} else if (this.dropState == DropState.Auto) {
				
				var tr = this.list.transform as RectTransform;
				var itemsTr = this.items.transform as RectTransform;

				var size = tr.sizeDelta;

				Vector3 worldPoint = tr.localToWorldMatrix * (tr.localPosition - new Vector3(0f, size.y * 0.5f, 0f) - new Vector3(0f, itemsTr.sizeDelta.y, 0f));
				worldPoint += tr.parent.position;

				var point = RectTransformUtility.WorldToScreenPoint(this.GetWindow().workCamera, worldPoint);
				var viewport = this.GetWindow().workCamera.ScreenToViewportPoint(point);

				if (viewport.y < 0f) {

					this.stateUp.Set();

				} else {

					this.stateDown.Set();

				}

			}

		}

	}
	
}