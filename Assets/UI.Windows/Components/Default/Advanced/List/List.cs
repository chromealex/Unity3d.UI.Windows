using UnityEngine;
using UnityEngine.Extensions;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components {
	
	public class List : WindowComponent {
		
		private List<WindowComponent> list = new List<WindowComponent>();

		public ScrollRect scrollRect;
		public WindowComponent source;

		public T AddItem<T>() where T : IComponent {

			if (this.source == null) return default(T);

			var instance = this.source.Spawn();

			if (instance is LinkerComponent) {

				instance.OnInit();
				instance.gameObject.SetActive(true);

				instance = (instance as LinkerComponent).Get<WindowComponent>();

			}
			
			instance.Setup(this.GetLayoutRoot());
			instance.Setup(this.GetWindow());
			this.RegisterSubComponent(instance);

			this.list.Add(instance);
			instance.gameObject.SetActive(true);

			return (T)(instance as IComponent);

		}

		public int GetIndexOf<T>(T item) where T : IComponent {
			
			return this.GetItems().IndexOf(item as WindowComponent);

		}

		public List<WindowComponent> GetItems() {
			
			return this.list;
			
		}
		
		public List<T> GetItems<T>() where T : IComponent {
			
			return this.list.Cast<T>().ToList();
			
		}
		
		public T GetItem<T>(int index) where T : IComponent {
			
			return (T)(this.list[index] as IComponent);
			
		}

		public void SetItems(int capacity, UnityAction<IComponent> onItem = null) {

			this.SetItems<IComponent>(capacity, (element, index) => {

				if (onItem != null) onItem(element as WindowComponent);

			});

		}

		public void SetItems<T>(int capacity, UnityAction<T, int> onItem = null) where T : IComponent {

			foreach (var element in this.list) element.Recycle();
			this.list.Clear();

			for (int i = 0; i < capacity; ++i) {

				var instance = this.AddItem<T>();
				if (instance != null && onItem != null) onItem.Invoke(instance, i);

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