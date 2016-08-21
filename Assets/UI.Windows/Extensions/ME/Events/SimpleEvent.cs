using System.Collections.Generic;

namespace ME.Events {

    public delegate void SimpleAction();
    public delegate void SimpleAction<T0>(T0 arg0);
    public delegate void SimpleAction<T0, T1>(T0 arg0, T1 arg1);
    public delegate void SimpleAction<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    public delegate void SimpleAction<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);
    public delegate void SimpleAction<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public abstract class SimpleEventBase<TAction> {

        protected List<TAction> listeners;

        public SimpleEventBase() {

            this.listeners = ListPool<TAction>.Get();

        }

        ~SimpleEventBase() {

            ListPool<TAction>.Release(this.listeners);

        }

        public void AddListener(TAction action) {

            this.listeners.Add(action);

        }

        public void RemoveListener(TAction action) {

            this.listeners.Remove(action);

        }

        public void RemoveAllListeners() {

            this.listeners.Clear();

        }

    }

    public abstract class SimpleEvent : SimpleEventBase<SimpleAction> {

        public void Invoke() {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke();

            }

        }

    }

    public abstract class SimpleEvent<T0> : SimpleEventBase<SimpleAction<T0>> {

        public void Invoke(T0 arg0) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0);

            }

        }

    }

    public abstract class SimpleEvent<T0, T1> : SimpleEventBase<SimpleAction<T0, T1>> {

        public void Invoke(T0 arg0, T1 arg1) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0, arg1);

            }

        }

    }

    public abstract class SimpleEvent<T0, T1, T2> : SimpleEventBase<SimpleAction<T0, T1, T2>> {

        public void Invoke(T0 arg0, T1 arg1, T2 arg2) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0, arg1, arg2);

            }

        }

    }

    public abstract class SimpleEvent<T0, T1, T2, T3> : SimpleEventBase<SimpleAction<T0, T1, T2, T3>> {

        public void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0, arg1, arg2, arg3);

            }

        }

    }

    public abstract class SimpleEvent<T0, T1, T2, T3, T4> : SimpleEventBase<SimpleAction<T0, T1, T2, T3, T4>> {

        public void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {

            for (int i = 0; i < this.listeners.Count; ++i) {

                this.listeners[i].Invoke(arg0, arg1, arg2, arg3, arg4);

            }

        }

    }

}
