using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows {

	/*public static class DelegateUtils {

		private class Item {

			public UnityEvent unityEvent;
			//public UnityAction unityAction;
			public System.Action @event;
			public System.Action action;

			public void Setup() {

				if (this.unityEvent != null) this.unityEvent.AddListener(this.FireAction);
				if (this.@event != null) this.@event += this.FireAction;

			}

			public void Release() {

				if (this.unityEvent != null) this.unityEvent.RemoveListener(this.FireAction);
				if (this.@event != null) this.@event -= this.FireAction;

			}

			public void FireAction() {

				//if (this.unityAction != null) this.unityAction.Invoke();
				if (this.action != null) this.action.Invoke();

			}

		};

		private static Dictionary<object, List<Item>> items = new Dictionary<object, List<Item>>();
*/
		/*public static void Register(object handler, UnityEvent @event, System.Action action, bool callAction = true) {

			var item = new Item();
			item.action = action;
			item.unityEvent = @event;

			DelegateUtils.Register_INTERNAL(handler, item, callAction);

		}*/
		/*
		public static void Register(object handler, UnityEventBase @event, UnityAction action, bool callAction = true) {

			var item = new Item();
			item.unityAction = action;
			item.unityEvent = @event;

			DelegateUtils.Register_INTERNAL(handler, item, callAction);

		}*/

		/*public static void Register(object handler, System.Action @event, System.Action action, bool callAction = true) {

			var item = new Item();
			item.action = action;
			item.@event = @event;

			DelegateUtils.Register_INTERNAL(handler, item, callAction);

		}*/
/*
		private static void Register_INTERNAL(object handler, Item item, bool callAction) {

			List<Item> list;
			if (DelegateUtils.items.TryGetValue(handler, out list) == true) {

				list.Add(item);

			} else {

				DelegateUtils.items.Add(handler, new List<Item>() { item });

			}

			item.Setup();

			if (callAction == true) {

				item.FireAction();

			}

		}

		public static void Unregister(object handler) {

			List<Item> list;
			if (DelegateUtils.items.TryGetValue(handler, out list) == true) {
				
				for (int i = 0; i < list.Count; ++i) {

					list[i].Release();

				}

				DelegateUtils.items.Remove(handler);

			}

		}

	}*/

}