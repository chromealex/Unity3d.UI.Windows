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
			public ListComponent list;
			public bool reverse;

			public Vector2 anchorMin;
			public Vector2 anchorMax;

			public State(RectTransform rect, ListComponent list, Vector2 anchorMin, Vector2 anchorMax, bool reverse) {

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

		public CanvasGroup canvasGroup;

		[Header("Popup Component")]
		public LinkerComponent buttonWithText;
		public LinkerComponent list;

		private ITextComponent label;
		private ListComponent items;

		private bool opened;

		private State stateDown;
		private State stateUp;

		public DropState dropState = DropState.Auto;
		//public float maxHeight = 200f;

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

			this.label.SetText(string.Empty);
			if (this.label is IButtonComponent) (this.label as IButtonComponent).SetCallback(this.Toggle);

			//this.items.SetupAsDropdown(this.maxHeight);

			WindowSystemInput.onPointerDown.AddListener(this.OnPressDown);

			this.UpdateDropState();

		}
		
		public override void OnShowBegin() {
			
			base.OnShowBegin();
			
			this.SetState(this.opened, immediately: true);
			
		}

		public override void OnShowEnd() {

			base.OnShowEnd();
			
			this.SetState(this.opened, immediately: true);

		}

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.callback.RemoveAllListeners();
			this.callbackButton.RemoveAllListeners();

			WindowSystemInput.onPointerDown.RemoveListener(this.OnPressDown);

		}

		public void Toggle() {

			this.opened = !this.opened;

			this.SetState(this.opened, immediately: false);

		}

		private void OnPressDown(int id) {

			if (this.opened == false) return;

			var raycast = (EventSystem.current.currentInputModule as WindowSystemInputModule).GetCurrentRaycast(id);
			var current = raycast.gameObject;

			if (current == (this.label as Component).gameObject) return;
			if (current == null || current.GetComponentsInParent<CanvasGroup>().Contains(this.canvasGroup) == false) {

				this.StartCoroutine(this.OnPressDown_YIELD());

			}
			
		}

		private System.Collections.IEnumerator OnPressDown_YIELD() {

			yield return false;
			yield return false;
			yield return false;

			if (this.opened == false) yield break;

			// Close
			this.SetState(opened: false, immediately: false);

		}

		public void SetState(bool opened, bool immediately) {

			this.opened = opened;

			if (opened == true) {

				//this.list.gameObject.SetActive(true);

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

					//this.list.gameObject.SetActive(false);

				}, immediately);

			}

		}

		public virtual void SetCallback(System.Action callback) {

			this.callback.AddListenerDistinct(callback);
			this.callbackButton.RemoveAllListeners();

		}

		public virtual void SetCallback(System.Action<ButtonComponent> callback) {

			this.callbackButton.AddListenerDistinct(callback);
			this.callback.RemoveAllListeners();

		}

		public int Count() {
			
			return this.items.Count();

		}

		public T AddItem<T>() where T : ITextComponent {

			return this.items.AddItem<T>();

		}

		public List<T> GetItems<T>() where T : IComponent {

			return this.items.GetItems<T>();
			
		}
		
		public List<WindowComponent> GetItems() {
			
			return this.items.GetItems();
			
		}

		public T GetItem<T>(int index) where T : ITextComponent {
			
			return (T)(this.items.GetItem<IComponent>(index) as ITextComponent);
			
		}

		public void Clear() {

			this.items.Clear();

		}

		public void SetItems<T>(int capacity, System.Action<T, int> onItem = null) where T : ITextComponent {

			this.items.BeginLoad();
			this.items.SetItems<T>(capacity, (element, index) => {

				if (element is ButtonComponent) {

					(element as ButtonComponent).SetCallback((button) => {

						this.Select(this.items.GetIndexOf(button));

					});

				}

				if (onItem != null) onItem.Invoke(element, index);

			});
			this.items.EndLoad();

		}

		private int selectedIndex = -1;
		public void Select(int index, bool closePopup = true) {

			if (index < 0) {

				this.label.SetText(string.Empty);
				this.selectedIndex = -1;
				return;

			}

			IButtonComponent prevText = null;
			if (this.selectedIndex >= 0) prevText = this.items.GetItem<IButtonComponent>(this.selectedIndex);
			var text = this.items.GetItem<ITextComponent>(index);

			this.selectedIndex = index;
			this.label.SetText(text.GetText());

			if (prevText != null) {

				prevText.SetEnabled();

			}

			var selectable = text as IButtonComponent;
			if (selectable != null) {

				selectable.SetDisabled();

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

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			if (this.canvasGroup == null) this.canvasGroup = this.GetComponent<CanvasGroup>();

		}
		#endif

	}
	
}