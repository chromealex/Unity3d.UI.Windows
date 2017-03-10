using System.Collections.Generic;
using UnityEngine.UI.Windows;
using UnityEngine.Events;

namespace MW2.Extensions {

	public class ValuesBase {

		#if DEBUGBUILD
		public ValuesBase() {

			ME.WeakReferenceInfo.Register(this);

		}
		#endif

	}

}

namespace ME.Events {

    /*public delegate void SimpleAction();
    public delegate void SimpleAction<T0>(T0 arg0);
    public delegate void SimpleAction<T0, T1>(T0 arg0, T1 arg1);
    public delegate void SimpleAction<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    public delegate void SimpleAction<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);
    public delegate void SimpleAction<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
*/

	public interface ISimpleEvent {
	};

	public static class SimpleEventDebug {

		public class Item {

			public System.Action action;
			public string trace;

		}

		private static List<Item> listeners = new List<Item>();

		public static List<Item> GetActions() {

			return SimpleEventDebug.listeners;

		}

		public static void AddListener(System.Action action) {

			SimpleEventDebug.listeners.Add(new Item() { action = action, trace = UnityEngine.StackTraceUtility.ExtractStackTrace() });

		}

		public static void RemoveListener(System.Action action) {

			SimpleEventDebug.listeners.RemoveAll(x => x.action == action);

		}

	}

	public abstract class SimpleEventBase<TAction> : MW2.Extensions.ValuesBase, ISimpleEvent {

		protected List<TAction> listeners;

		public SimpleEventBase() {

			this.listeners = ListPool<TAction>.Get();

		}

		~SimpleEventBase() {

			ListPool<TAction>.Release(this.listeners);

		}

		public int Count() {

			return this.listeners.Count;

		}

		public void AddListener(TAction action) {

			this.listeners.Add(action);
			if (WindowSystem.IsDebugWeakReferences() == true) SimpleEventDebug.AddListener(action as System.Action);

		}

		public void RemoveListener(TAction action) {

			this.listeners.Remove(action);
			if (WindowSystem.IsDebugWeakReferences() == true) SimpleEventDebug.RemoveListener(action as System.Action);

		}

		public void AddListenerDistinct(TAction action) {

			this.RemoveListener(action);
			this.AddListener(action);

		}

		public void RemoveAllListeners() {

			if (WindowSystem.IsDebugWeakReferences() == true) {

				foreach (var action in this.listeners) {

					SimpleEventDebug.RemoveListener(action as System.Action);

				}

			}

			this.listeners.Clear();

		}

	}

	public abstract class SimpleUnityEventBase<TAction> : MW2.Extensions.ValuesBase, ISimpleEvent {

		protected List<TAction> listeners;

		public SimpleUnityEventBase() {

			this.listeners = ListPool<TAction>.Get();

		}

		~SimpleUnityEventBase() {

			ListPool<TAction>.Release(this.listeners);

		}

		public int Count() {

			return this.listeners.Count;

		}

		public void AddListener(TAction action) {

			this.listeners.Add(action);

		}

		public void RemoveListener(TAction action) {

			this.listeners.Remove(action);

		}

		public void AddListenerDistinct(TAction action) {

			this.RemoveListener(action);
			this.AddListener(action);

		}

		public void RemoveAllListeners() {
			
			this.listeners.Clear();

		}

	}

	public class SimpleUnityEvent : SimpleUnityEventBase<UnityAction> {

		public void Invoke() {

			for (int i = 0; i < this.listeners.Count; ++i) {

				this.listeners[i].Invoke();

			}

		}

	}

    public abstract class SimpleEvent : SimpleEventBase<System.Action> {

        public void Invoke() {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke();

            }

        }

    }

	public abstract class SimpleEvent<T0> : SimpleEventBase<System.Action<T0>> {

        public void Invoke(T0 arg0) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0);

            }

        }

    }

	public abstract class SimpleEvent<T0, T1> : SimpleEventBase<System.Action<T0, T1>> {

        public void Invoke(T0 arg0, T1 arg1) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0, arg1);

            }

        }

    }

	public abstract class SimpleEvent<T0, T1, T2> : SimpleEventBase<System.Action<T0, T1, T2>> {

        public void Invoke(T0 arg0, T1 arg1, T2 arg2) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0, arg1, arg2);

            }

        }

    }

	public abstract class SimpleEvent<T0, T1, T2, T3> : SimpleEventBase<System.Action<T0, T1, T2, T3>> {

        public void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0, arg1, arg2, arg3);

            }

        }

    }
	/*
	public abstract class SimpleEvent<T0, T1, T2, T3, T4> : SimpleEventBase<System.Action<T0, T1, T2, T3, T4>> {

        public void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0, arg1, arg2, arg3, arg4);

            }

        }

    }*/

}
