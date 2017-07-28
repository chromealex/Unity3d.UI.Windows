using UnityEngine;
using UnityEngine.Extensions;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections;
using ME;

namespace UnityEngine.UI.Windows.Components {
	
	public class ListComponent : WindowComponentNavigation, IListComponent {

		public enum ScrollbarVisibility : byte {
			AlwaysVisible = 0,
			ShowOnMove,
		};

		public List<WindowComponent> list = new List<WindowComponent>();

		[Header("Required")]
		public WindowComponent source;

		[Header("Optional")]
		public Extensions.ScrollRect scrollRect;
		public GameObject content;
		public GameObject noElements;
		public GameObject loading;
		public WindowComponent loader;

		[Header("Faders")]
		public WindowComponent top;
		public WindowComponent bottom;
		public WindowComponent left;
		public WindowComponent right;

		[Header("Scrollbars")]
		public ScrollbarVisibility scrollbarVisibility = ScrollbarVisibility.ShowOnMove;
		public WindowComponent horizontalScrollbar;
		public WindowComponent verticalScrollbar;

		[SerializeField]
		private LayoutGroup layoutGroup;

		public override void OnInit() {

			base.OnInit();

			this.Refresh();
			if (this.noElements != null) this.noElements.SetActive(false);
			if (this.loading != null) this.loading.SetActive(true);
			if (this.loader != null) this.loader.Show();

			if (this.scrollRect != null) {

				this.scrollRect.onMovingBeginEvent.AddListener(this.OnMovingContentBegin);
				this.scrollRect.onMovingEndEvent.AddListener(this.OnMovingContentEnd);
				this.scrollRect.onValueChanged.AddListener(this.OnMovingContent);

			}

		}

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			//this.Clear();

			if (this.scrollRect != null) {
				
				this.scrollRect.onMovingBeginEvent.RemoveListener(this.OnMovingContentBegin);
				this.scrollRect.onMovingEndEvent.RemoveListener(this.OnMovingContentEnd);
				this.scrollRect.onValueChanged.RemoveListener(this.OnMovingContent);

			}

		}

		/*public override void OnDeinit() {

			this.Clear();

			base.OnDeinit();

		}*/

		public override void OnWindowUnload() {

			base.OnWindowUnload();

			Coroutines.StopAll(this);

		}

		public override void OnShowBegin() {

			base.OnShowBegin();

			this.TryToHideScrollbars();

		}

		protected virtual void OnMovingContentBegin() {

			this.TryToShowScrollbars();

		}

		protected virtual void OnMovingContentEnd() {

			this.TryToHideScrollbars();

		}

		protected virtual void OnMovingContent(Vector2 delta) {

			this.UpdateBorderFades();

		}

		protected virtual void UpdateBorderFades() {

			if (this.scrollRect != null) {

				if (this.scrollRect.vertical == true) {

					var valPosition = 1f - this.scrollRect.verticalNormalizedPosition;
					if (valPosition > 0f && this.scrollRect.vScrollingNeeded == true) {

						if (this.top != null) this.top.Show();

					} else {

						if (this.top != null) this.top.Hide();

					}

					if (valPosition < 1f && this.scrollRect.vScrollingNeeded == true) {

						if (this.bottom != null) this.bottom.Show();

					} else {

						if (this.bottom != null) this.bottom.Hide();

					}

				}

				if (this.scrollRect.horizontal == true) {

					var valPosition = this.scrollRect.horizontalNormalizedPosition;
					if (valPosition > 0f && this.scrollRect.hScrollingNeeded == true) {

						if (this.left != null) this.left.Show();

					} else {

						if (this.left != null) this.left.Hide();

					}

					if (valPosition < 1f && this.scrollRect.hScrollingNeeded == true) {

						if (this.right != null) this.right.Show();

					} else {

						if (this.right != null) this.right.Hide();

					}

				}

			}

		}

		public void TryToShowScrollbars() {

			if (this.scrollbarVisibility == ScrollbarVisibility.ShowOnMove) {

				if (this.horizontalScrollbar != null) this.horizontalScrollbar.Show();
				if (this.verticalScrollbar != null) this.verticalScrollbar.Show();

			}

		}

		public void TryToHideScrollbars() {

			if (this.scrollbarVisibility == ScrollbarVisibility.ShowOnMove) {

				if (this.horizontalScrollbar != null) this.horizontalScrollbar.Hide();
				if (this.verticalScrollbar != null) this.verticalScrollbar.Hide();

			}

		}

		public void InitPool(int capacity) {

			this.source.CreatePool(capacity, this.transform);
			if (this.source is LinkerComponent) {

				(this.source as LinkerComponent).InitPool(capacity);

			}

		}

		/*public void SetupAsDropdown(float maxHeight) {

			this.scrollRect.SetupAsDropdown(maxHeight);

		}*/

		public override bool IsNavigationControlledSide(NavigationSide side) {

			if (this.scrollRect != null) {

				if (this.scrollRect.vertical == true) {

					if (side == NavigationSide.Down ||
					    side == NavigationSide.Up) {

						return true;

					}

				}

				if (this.scrollRect.horizontal == true) {

					if (side == NavigationSide.Left ||
						side == NavigationSide.Right) {

						return true;

					}

				}

			}

			return false;

		}

		public override void OnNavigate(WindowComponentNavigation source, NavigationSide side) {

			switch (side) {

				case NavigationSide.Up:
					this.ListMoveUp();
					break;

				case NavigationSide.Down:
					this.ListMoveDown();
					break;

				case NavigationSide.Right:
					this.ListMoveRight();
					break;

				case NavigationSide.Left:
					this.ListMoveLeft();
					break;

			}

		}

		public virtual IListComponent ListMoveUp(int count = 1) {

			if (this.scrollRect == null) return this;

			count = Mathf.Clamp(count, 1, count);
			var size = this.GetRowHeight(0) * count;
			var allSize = this.GetContentHeight();

			var pos = this.scrollRect.verticalNormalizedPosition * allSize;
			pos += size;
			this.scrollRect.verticalNormalizedPosition = pos / allSize;

			return this;

		}

		public virtual IListComponent ListMoveDown(int count = 1) {

			if (this.scrollRect == null) return this;

			count = Mathf.Clamp(count, 1, count);
			var size = this.GetRowHeight(0) * count;
			var allSize = this.GetContentHeight();

			var pos = this.scrollRect.verticalNormalizedPosition * allSize;
			pos -= size;
			this.scrollRect.verticalNormalizedPosition = pos / allSize;

			return this;

		}

		public virtual IListComponent ListMoveRight(int count = 1) {

			if (this.scrollRect == null) return this;

			count = Mathf.Clamp(count, 1, count);
			var size = this.GetRowWidth(0) * count;
			var allSize = this.GetContentWidth();

			var pos = this.scrollRect.horizontalNormalizedPosition * allSize;
			pos += size;
			this.scrollRect.horizontalNormalizedPosition = pos / allSize;

			return this;

		}

		public virtual IListComponent ListMoveLeft(int count = 1) {

			if (this.scrollRect == null) return this;

			count = Mathf.Clamp(count, 1, count);
			var size = this.GetRowWidth(0) * count;
			var allSize = this.GetContentWidth();

			var pos = this.scrollRect.horizontalNormalizedPosition * allSize;
			pos -= size;
			this.scrollRect.horizontalNormalizedPosition = pos / allSize;

			return this;

		}

		public virtual float GetContentWidth() {

			return this.scrollRect.content.rect.width;

		}

		public virtual float GetContentHeight() {

			return this.scrollRect.content.rect.height;

		}

		public virtual float GetRowSpacing() {

			var layoutGroup = this.layoutGroup as HorizontalOrVerticalLayoutGroup;
			if (layoutGroup == null) return 0f;

			return layoutGroup.spacing;

		}

		public virtual float GetRowWidth(int row) {

			var index = row;
			if (index >= 0) {

				return (this.GetItem(index).transform as RectTransform).rect.width;

			}

			return (this.source.transform as RectTransform).rect.width;

		}

		public virtual float GetRowHeight(int row) {

			var index = row;
			if (index >= 0) {

				return (this.GetItem(index).transform as RectTransform).rect.height;

			}

			return (this.source.transform as RectTransform).rect.height;

		}

		public virtual WindowComponent GetInstance(int index) {

			var instance = this.source.Spawn();
			instance.Setup(this.GetLayoutRoot());
			instance.Setup(this.GetWindow());
			this.list.Add(instance);
			this.RegisterSubComponent(instance);
			//if (this.loading != null) this.loading.SetActive(false);
			//if (this.loader != null) this.loader.Hide();

			return instance;

		}

		public virtual void RemoveItem(WindowComponent instance) {

			if (this.list.Remove(instance) == true) {
				
				this.UnregisterSubComponent(instance);
				instance.Recycle();

			}

		}

		public virtual T AddItem<T>(bool autoRefresh = true) where T : IComponent {

			if (this.source == null) return default(T);

			var instance = this.GetInstance(this.Count());
			if (instance == null) return default(T);

			if (this.scrollRect != null && this.scrollRect.content != null) {

				instance.SetParent(this.scrollRect.content, setTransformAsSource: false);

			} else if (this.content != null) {

				instance.SetParent(this.content.transform, setTransformAsSource: false);

			}

			if (instance != null && instance.transform.localPosition.z != 0f) {

				var pos = instance.transform.localPosition;
				pos.z = 0f;
				instance.transform.localPosition = pos;

			}

			if (instance != null && instance.transform.localScale.x != 1f) {

				instance.transform.localScale = Vector3.one;

			}

			if (instance is LinkerComponent && typeof(T) != typeof(LinkerComponent)) {

				//instance.OnInit();
				//instance.gameObject.SetActive(true);
				var linkerComponent = (instance as LinkerComponent).Get<WindowComponent>();
				if (linkerComponent != null) instance = linkerComponent;

			}

			/*if (instance != null) {

				instance.gameObject.SetActive(true);

			}*/

			if (autoRefresh == true) this.Refresh();

			this.OnNewItem(instance);

			return (T)(instance as IComponent);

		}

		public virtual void OnNewItem(WindowComponent instance) {
			
		}

		public virtual int GetIndexOf<T>(T item) where T : IComponent {

			var winComp = (item as WindowComponent);
			return this.GetItems().FindIndex((c) => {

				if (c is LinkerComponent) {

					return (c as LinkerComponent).Get<WindowComponent>() == winComp;

				}

				return winComp == c;

			});

		}

		public virtual bool IsEmpty() {

			return this.Count() == 0;

		}

		public int Count() {

			return this.list.Count;

		}

		public List<WindowComponent> GetItems() {
			
			return this.list;
			
		}
		
		public List<T> GetItems<T>() where T : IComponent {
			
			return this.list.Cast<T>().ToList();
			
		}
		
		public virtual WindowComponent GetItem(int index, bool linkerLookup = true) {

			if (index < 0 || index >= this.list.Count) {
				
				return null;

			}

			if (linkerLookup == true) {

				if (this.list[index] is LinkerComponent) {
					
					return (this.list[index] as LinkerComponent).Get<WindowComponent>();
					
				}

			}

			return this.list[index];
			
		}

		public virtual T FindItem<T>() where T : IComponent {

			var items = this.GetItems();
			for (int i = 0; i < items.Count; ++i) {

				if (items[i] is T) return (T)(items[i] as IComponent);

			}

			return default(T);

		}

		public virtual T GetItem<T>(int index) where T : IComponent {

			return (T)(this.GetItem(index, typeof(T) != typeof(LinkerComponent)) as IComponent);
			
		}
		
		public void ForEach<T>(System.Action<T, int> onItem = null) where T : IComponent {

			this.ForEach((element, index) => {

				if (onItem != null) onItem.Invoke((T)element, index);

			});

		}

		public void ForEach(System.Action<IComponent, int> onItem = null) {

			for (int i = 0, capacity = this.Count(); i < capacity; ++i) {

				var instance = this.GetItem(i);
				if (instance != null && onItem != null) onItem.Invoke(instance, i);

			}

		}

		public System.Collections.Generic.IEnumerator<byte> ForEachAsync<T>(System.Action onComplete, System.Action<T, int> onItem = null) where T : IComponent {

			for (int i = 0, capacity = this.Count(); i < capacity; ++i) {

				if (this == null) yield break;

				var instance = this.GetItem<T>(i);
				if (instance != null && onItem != null) onItem.Invoke(instance, i);

				yield return 0;

			}

			if (onComplete != null) onComplete.Invoke();

		}

		public void SetItems<T>(int capacity, System.Action<T, int> onItem = null) where T : IComponent {

			this.SetItems(capacity, (element, index) => {

				if (onItem != null) onItem.Invoke((T)element, index);

			});

		}
		
		protected virtual void SetItems(int capacity, System.Action<IComponent, int> onItem) {

			var existCount = this.Count();
			if (existCount > capacity) {

				for (int i = existCount - 1; i >= capacity; --i) {

					this.RemoveItem(this.GetItem(i, linkerLookup: false));

				}

			} else if (existCount < capacity) {

				for (int i = existCount; i < capacity; ++i) {

					this.AddItem<IComponent>(autoRefresh: false);

				}

			}

			this.ForEach(onItem);

			this.Refresh(withNoElements: true);

		}

		public virtual void SetItemsAsync<T>(int capacity, System.Action onComplete, System.Action<T, int> onItem = null) where T : IComponent {

			Coroutines.StopAll(this);

			if (capacity == this.Count() && capacity > 0) {

				Coroutines.Run(this.ForEachAsync<T>(onComplete, onItem), this);
				return;

			}

			this.Clear();

			Coroutines.Run(this.SetItemsAsync_INTERNAL(capacity, onComplete, onItem), this);

		}

		private System.Collections.Generic.IEnumerator<byte> SetItemsAsync_INTERNAL<T>(int capacity, System.Action onComplete, System.Action<T, int> onItem = null) where T : IComponent {
			
			var timer = ME.Utilities.StartWatch();
			for (int i = 0; i < capacity; ++i) {

				ME.Utilities.ResetWatch(timer);

				var instance = this.AddItem<T>(autoRefresh: false);
				if (instance != null && onItem != null) onItem.Invoke(instance, i);

				if (ME.Utilities.StopWatch(timer) == true) {

					yield return 0;

				}

			}

			if (onComplete != null) onComplete.Invoke();
			
			this.Refresh(withNoElements: true);

		}

		public virtual void Clear() {

			Coroutines.StopAll(this);

			for (int i = 0; i < this.list.Count; ++i) {
				
				var element = this.list[i];
				this.UnregisterSubComponent(element, immediately: true);
				element.Recycle();

			}
			this.list.Clear();
			
			this.Refresh(withNoElements: true);

		}

		public virtual void BeginLoad() {

			if (this.noElements != null) this.noElements.SetActive(false);
			if (this.content != null) this.content.SetActive(false);

			if (this.loading != null) this.loading.SetActive(true);
			if (this.loader != null) this.loader.Show(AppearanceParameters.Default().ReplaceForced(forced: true).ReplaceResetAnimation(resetAnimation: true));

		}

		public virtual void EndLoad() {
			
			if (this.loading != null) this.loading.SetActive(false);
			if (this.loader != null) this.loader.Hide(AppearanceParameters.Default().ReplaceForced(forced: true).ReplaceResetAnimation(resetAnimation: true));

			if (this.noElements != null) this.noElements.SetActive(this.IsEmpty() == true);
			if (this.content != null) this.content.SetActive(this.IsEmpty() == false);

		}

		public void Refresh(bool withNoElements = false) {

			if (this.layoutGroup != null) {

				this.layoutGroup.CalculateLayoutInputHorizontal();
				this.layoutGroup.CalculateLayoutInputVertical();

			}

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			if (this.autoRegisterSubComponents == true) {

				this.UnregisterSubComponent(this.source);

			}

			if (this.layoutGroup == null) ME.Utilities.FindReference(this, ref this.layoutGroup);

			//this.scrollRect = this.GetComponentInChildren<ScrollRect>();

		}
		#endif
		
	}
	
}