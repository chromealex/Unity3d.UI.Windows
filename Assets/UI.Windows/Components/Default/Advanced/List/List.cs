using UnityEngine;
using UnityEngine.Extensions;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

namespace UnityEngine.UI.Windows.Components.List {
	
	public class List : WindowComponent {
		
		private List<WindowComponent> list = new List<WindowComponent>();

		public ScrollRect scrollRect;
		public WindowComponent source;

		public T AddItem<T>() where T : WindowComponent {

			var instance = this.source.Spawn();
			instance.Setup(this.GetLayoutRoot());

			this.list.Add(instance);
			
			instance.gameObject.SetActive(true);

			this.RegisterSubComponent(instance);

			return instance as T;

		}

		public int GetIndexOf<T>(T item) where T : WindowComponent {
			
			return this.GetItems().IndexOf(item as WindowComponent);

		}

		public List<WindowComponent> GetItems() {
			
			return this.list;
			
		}
		
		public List<T> GetItems<T>() where T : WindowComponent {
			
			return this.list.Cast<T>().ToList();
			
		}
		
		public T GetItem<T>(int index) where T : WindowComponent {
			
			return this.list[index] as T;
			
		}

		public void SetItems(int capacity, UnityAction<WindowComponent> onItem = null) {

			this.SetItems(capacity, onItem);

		}

		public void SetItems<T>(int capacity, UnityAction<T, int> onItem = null) where T : WindowComponent {

			foreach (var element in this.list) element.Recycle();
			this.list.Clear();

			for (int i = 0; i < capacity; ++i) {

				var instance = this.AddItem<T>();
				if (onItem != null) onItem.Invoke(instance, i);

			}

		}

		#if UNITY_EDITOR
		public override void OnValidate() {

			base.OnValidate();
			//this.scrollRect = this.GetComponentInChildren<ScrollRect>();

		}
		#endif
		
	}
	
}