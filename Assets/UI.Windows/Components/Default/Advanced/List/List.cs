using UnityEngine;
using UnityEngine.Extensions;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {
	
	public class List : WindowComponent {
		
		private List<WindowComponent> list = new List<WindowComponent>();

		[Header("Required")]
		public WindowComponent source;

		[Header("Optional")]
		public Extensions.ScrollRect scrollRect;
		public GameObject content;
		public GameObject noElements;

		public override void OnInit() {

			base.OnInit();

			this.Refresh();

		}

		public void InitPool(int capacity) {

			this.source.CreatePool(capacity, this.transform);
			if (this.source is LinkerComponent) {

				(this.source as LinkerComponent).InitPool(capacity);

			}

		}

		public void SetupAsDropdown(float maxHeight) {

			this.scrollRect.SetupAsDropdown(maxHeight);

		}

		public virtual T AddItem<T>() where T : IComponent {

			if (this.source == null) return default(T);

			var instance = this.source.Spawn();
			instance.Setup(this.GetLayoutRoot());
			instance.Setup(this.GetWindow());

			if (this.scrollRect != null && this.scrollRect.content != null) instance.SetParent(this.scrollRect.content, setTransformAsSource: false);

			this.list.Add(instance);
			
			this.RegisterSubComponent(instance);

			if (instance is LinkerComponent) {

				//instance.OnInit();
				instance.gameObject.SetActive(true);

				instance = (instance as LinkerComponent).Get<WindowComponent>();

			}

			instance.gameObject.SetActive(true);

			this.Refresh();

			return (T)(instance as IComponent);

		}

		public int GetIndexOf<T>(T item) where T : IComponent {
			
			return this.GetItems().FindIndex((c) => {

				if (c is LinkerComponent) {

					return (c as LinkerComponent).Get<WindowComponent>() == (item as WindowComponent);

				}

				return (item as WindowComponent) == c;

			});

		}

		public List<WindowComponent> GetItems() {
			
			return this.list;
			
		}
		
		public List<T> GetItems<T>() where T : IComponent {
			
			return this.list.Cast<T>().ToList();
			
		}
		
		public T GetItem<T>(int index) where T : IComponent {

			if (this.list[index] is LinkerComponent) {

				return (this.list[index] as LinkerComponent).Get<T>();

			}

			return (T)(this.list[index] as IComponent);
			
		}

		public virtual void SetItems(int capacity, UnityAction<IComponent> onItem = null) {

			this.SetItems<IComponent>(capacity, (element, index) => {

				if (onItem != null) onItem(element as WindowComponent);

			});

		}
		
		public virtual void SetItems<T>(int capacity, UnityAction<T, int> onItem = null) where T : IComponent {
			
			this.Clear();
			
			for (int i = 0; i < capacity; ++i) {
				
				var instance = this.AddItem<T>();
				if (instance != null && onItem != null) onItem.Invoke(instance, i);
				
			}
			
		}
		
		public virtual void SetItemsAsync<T>(int capacity, UnityAction onComplete, UnityAction<T, int> onItem = null) where T : IComponent {
			
			this.Clear();

			this.StopAllCoroutines();
			this.StartCoroutine(this.SetItemsAsync_INTERNAL(capacity, onComplete, onItem));

		}

		private IEnumerator SetItemsAsync_INTERNAL<T>(int capacity, UnityAction onComplete, UnityAction<T, int> onItem = null) where T : IComponent {

			for (int i = 0; i < capacity; ++i) {

				var instance = this.AddItem<T>();
				if (instance != null && onItem != null) onItem.Invoke(instance, i);

				yield return false;
				
			}

			if (onComplete != null) onComplete.Invoke();
			
		}

		public virtual void Clear() {

			this.StopAllCoroutines();

			foreach (var element in this.list) {

				this.UnregisterSubComponent(element);
				element.Recycle();

			}
			this.list.Clear();

			this.Refresh();

		}

		public override void OnHideBegin(System.Action callback) {
			
			this.Clear();

			base.OnHideBegin(callback);

		}

		public void Refresh() {

			if (this.noElements != null) this.noElements.SetActive(this.list.Count == 0);
			if (this.content != null) this.content.SetActive(this.list.Count > 0);
			if (this.scrollRect != null) this.scrollRect.UpdateDropdown();

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();
			//this.scrollRect = this.GetComponentInChildren<ScrollRect>();

		}
		#endif
		
	}
	
}