
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace ME {

    public abstract class ActionBase {

        protected int idx;

		public ActionBase() {

			this.idx = CallbackTable.Register(null);

		}

        ~ActionBase() {

            CallbackTable.Unregister(this.idx);

        }

    }

    public class Action : ActionBase {

		public Action() : base() {
		}

        public Action(System.Action action) {

            this.idx = CallbackTable.Register(action);

        }

        public void Invoke() {

            var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder;
            holder.Invoke();

        }

		public void AddListenerDistinct(System.Action action) {

			this.RemoveListener(action);
			this.AddListener(action);

		}

		public void AddListener(System.Action action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder;
			holder.AddCallback(action);

		}

		public void RemoveListener(System.Action action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder;
			holder.RemoveCallback(action);

		}

		public void RemoveAllListeners() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder;
			holder.RemoveAll();

		}

		public int Count() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder;
			return holder.Count();

		}

        public static Action operator +(Action action0, System.Action action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder;
            holder.AddCallback(action1);

            return action0;

        }

        public static Action operator -(Action action0, System.Action action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder;
            holder.RemoveCallback(action1);

            return action0;

        }

    }

    public class Action<T> : ActionBase {

		public Action() : base() {
		}

        public Action(System.Action<T> action) {

            this.idx = CallbackTable.Register(action);

        }

        public void Invoke(T param) {

            var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T>;
            holder.Invoke(param);

        }

		public void AddListenerDistinct(System.Action<T> action) {

			this.RemoveListener(action);
			this.AddListener(action);

		}

		public void AddListener(System.Action<T> action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T>;
			holder.AddCallback(action);

		}

		public void RemoveListener(System.Action<T> action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T>;
			holder.RemoveCallback(action);

		}

		public void RemoveAllListeners() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T>;
			holder.RemoveAll();

		}

		public int Count() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T>;
			return holder.Count();

		}

        public static Action<T> operator +(Action<T> action0, System.Action<T> action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder<T>;
            holder.AddCallback(action1);

            return action0;

        }

        public static Action<T> operator -(Action<T> action0, System.Action<T> action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder<T>;
            holder.RemoveCallback(action1);

            return action0;

        }

    }

    public class Action<T0, T1> : ActionBase {

		public Action() : base() {
		}

        public Action(System.Action<T0, T1> action) {

            this.idx = CallbackTable.Register(action);

        }

        public void Invoke(T0 param0, T1 param1) {

            var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1>;
            holder.Invoke(param0, param1);

        }

		public void AddListenerDistinct(System.Action<T0, T1> action) {

			this.RemoveListener(action);
			this.AddListener(action);

		}

		public void AddListener(System.Action<T0, T1> action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1>;
			holder.AddCallback(action);

		}

		public void RemoveListener(System.Action<T0, T1> action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1>;
			holder.RemoveCallback(action);

		}

		public void RemoveAllListeners() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1>;
			holder.RemoveAll();

		}

		public int Count() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1>;
			return holder.Count();

		}

        public static Action<T0, T1> operator +(Action<T0, T1> action0, System.Action<T0, T1> action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder<T0, T1>;
            holder.AddCallback(action1);

            return action0;

        }

        public static Action<T0, T1> operator -(Action<T0, T1> action0, System.Action<T0, T1> action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder<T0, T1>;
            holder.RemoveCallback(action1);

            return action0;

        }

    }

    public class Action<T0, T1, T2> : ActionBase {

		public Action() : base() {
		}

        public Action(System.Action<T0, T1, T2> action) {

            this.idx = CallbackTable.Register(action);

        }

        public void Invoke(T0 param0, T1 param1, T2 param2) {

            var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2>;
            holder.Invoke(param0, param1, param2);

        }

		public void AddListenerDistinct(System.Action<T0, T1, T2> action) {

			this.RemoveListener(action);
			this.AddListener(action);

		}

		public void AddListener(System.Action<T0, T1, T2> action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2>;
			holder.AddCallback(action);

		}

		public void RemoveListener(System.Action<T0, T1, T2> action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2>;
			holder.RemoveCallback(action);

		}

		public void RemoveAllListeners() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2>;
			holder.RemoveAll();

		}

		public int Count() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2>;
			return holder.Count();

		}

        public static Action<T0, T1, T2> operator +(Action<T0, T1, T2> action0, System.Action<T0, T1, T2> action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder<T0, T1, T2>;
            holder.AddCallback(action1);

            return action0;

        }

        public static Action<T0, T1, T2> operator -(Action<T0, T1, T2> action0, System.Action<T0, T1, T2> action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder<T0, T1, T2>;
            holder.RemoveCallback(action1);

            return action0;

        }

    }

    public class Action<T0, T1, T2, T3> : ActionBase {

		public Action() : base() {
		}

        public Action(System.Action<T0, T1, T2, T3> action) {

            this.idx = CallbackTable.Register(action);

        }

        public void Invoke(T0 param0, T1 param1, T2 param2, T3 param3) {

            var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2, T3>;
            holder.Invoke(param0, param1, param2, param3);

        }

		public void AddListenerDistinct(System.Action<T0, T1, T2, T3> action) {

			this.RemoveListener(action);
			this.AddListener(action);

		}

		public void AddListener(System.Action<T0, T1, T2, T3> action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2, T3>;
			holder.AddCallback(action);

		}

		public void RemoveListener(System.Action<T0, T1, T2, T3> action) {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2, T3>;
			holder.RemoveCallback(action);

		}

		public void RemoveAllListeners() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2, T3>;
			holder.RemoveAll();

		}

		public int Count() {

			var holder = CallbackTable.GetHolder(this.idx) as CallbackTable.CallbackHolder<T0, T1, T2, T3>;
			return holder.Count();

		}

        public static Action<T0, T1, T2, T3> operator +(Action<T0, T1, T2, T3> action0, System.Action<T0, T1, T2, T3> action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder<T0, T1, T2, T3>;
            holder.AddCallback(action1);

            return action0;

        }

        public static Action<T0, T1, T2, T3> operator -(Action<T0, T1, T2, T3> action0, System.Action<T0, T1, T2, T3> action1) {

            var holder = CallbackTable.GetHolder(action0.idx) as CallbackTable.CallbackHolder<T0, T1, T2, T3>;
            holder.RemoveCallback(action1);

            return action0;

        }

    }

    public class CallbackTable {

        public abstract class CallbackHolderBase {

            public readonly string stackTrace;

            public CallbackHolderBase() {

                var stack = new System.Diagnostics.StackTrace(true);
                this.stackTrace = stack.ToString();

            }

        }

        public class CallbackHolder : CallbackHolderBase {

            private readonly List<System.Action> callbacks = new List<System.Action>();

            public CallbackHolder(System.Action callback) {

                this.AddCallback(callback);

            }

            public void AddCallback(System.Action callback) {

                this.callbacks.Add(callback);

            }

            public void RemoveCallback(System.Action callback) {

                this.callbacks.Remove(callback);

            }

			public void RemoveAll() {

				this.callbacks.Clear();

			}

			public int Count() {

				return this.callbacks.Count;

			}

            public void Invoke() {

                for (var i = 0; i < this.callbacks.Count; ++i) {

                    this.callbacks[i].Invoke();

                }

            }

        }

        public class CallbackHolder<T> : CallbackHolderBase {

            private readonly List<System.Action<T>> callbacks = new List<System.Action<T>>();

            public CallbackHolder(System.Action<T> callback) {

                this.AddCallback(callback);

            }

            public void AddCallback(System.Action<T> callback) {

                this.callbacks.Add(callback);

            }

            public void RemoveCallback(System.Action<T> callback) {

                this.callbacks.Remove(callback);

            }

			public void RemoveAll() {

				this.callbacks.Clear();

			}

			public int Count() {

				return this.callbacks.Count;

			}

            public void Invoke(T param) {

                for (var i = 0; i < this.callbacks.Count; ++i) {

                    this.callbacks[i].Invoke(param);

                }

            }

        }

        public class CallbackHolder<T0, T1> : CallbackHolderBase {

            private readonly List<System.Action<T0, T1>> callbacks = new List<System.Action<T0, T1>>();

            public CallbackHolder(System.Action<T0, T1> callback) {

                this.AddCallback(callback);

            }

            public void AddCallback(System.Action<T0, T1> callback) {

                this.callbacks.Add(callback);

            }

            public void RemoveCallback(System.Action<T0, T1> callback) {

                this.callbacks.Remove(callback);

            }

			public void RemoveAll() {

				this.callbacks.Clear();

			}

			public int Count() {

				return this.callbacks.Count;

			}

            public void Invoke(T0 param0, T1 param1) {

                for (var i = 0; i < this.callbacks.Count; ++i) {

                    this.callbacks[i].Invoke(param0, param1);

                }

            }

        }

        public class CallbackHolder<T0, T1, T2> : CallbackHolderBase {

            private readonly List<System.Action<T0, T1, T2>> callbacks = new List<System.Action<T0, T1, T2>>();

            public CallbackHolder(System.Action<T0, T1, T2> callback) {

                this.AddCallback(callback);

            }

            public void AddCallback(System.Action<T0, T1, T2> callback) {

                this.callbacks.Add(callback);

            }

            public void RemoveCallback(System.Action<T0, T1, T2> callback) {

                this.callbacks.Remove(callback);

            }

			public void RemoveAll() {

				this.callbacks.Clear();

			}

			public int Count() {

				return this.callbacks.Count;

			}

            public void Invoke(T0 param0, T1 param1, T2 param2) {

                for (var i = 0; i < this.callbacks.Count; ++i) {

                    this.callbacks[i].Invoke(param0, param1, param2);

                }

            }

        }

        public class CallbackHolder<T0, T1, T2, T3> : CallbackHolderBase {

            private readonly List<System.Action<T0, T1, T2, T3>> callbacks = new List<System.Action<T0, T1, T2, T3>>();

            public CallbackHolder(System.Action<T0, T1, T2, T3> callback) {

                this.AddCallback(callback);

            }

            public void AddCallback(System.Action<T0, T1, T2, T3> callback) {

                this.callbacks.Add(callback);

            }

            public void RemoveCallback(System.Action<T0, T1, T2, T3> callback) {

                this.callbacks.Remove(callback);

            }

			public void RemoveAll() {

				this.callbacks.Clear();

			}

			public int Count() {

				return this.callbacks.Count;

			}

            public void Invoke(T0 param0, T1 param1, T2 param2, T3 param3) {

                for (var i = 0; i < this.callbacks.Count; ++i) {

                    this.callbacks[i].Invoke(param0, param1, param2, param3);

                }

            }

        }

        public const int MAX_CALLBACKS_COUNT = 4096;
        private readonly static List<CallbackHolderBase> holders = new List<CallbackHolderBase>(CallbackTable.MAX_CALLBACKS_COUNT);
        private readonly static Stack<int> freeIndexes = new Stack<int>(CallbackTable.MAX_CALLBACKS_COUNT);
        private static readonly object locker = new object();

        public static int Count { get { return CallbackTable.holders.Count - CallbackTable.freeIndexes.Count; } }
        
        public int lastId { get; private set; }

        private static int InsertHolder(CallbackHolderBase holder) {

            int idx;

            lock (CallbackTable.locker) {
                
                if (CallbackTable.freeIndexes.Count == 0) {

                    idx = CallbackTable.holders.Count;
                    CallbackTable.holders.Add(holder);

                } else {

                    idx = CallbackTable.freeIndexes.Pop();
                    CallbackTable.holders[idx] = holder;

                }

            }

            return idx;

        }

        public static void Unregister(int idx) {

            lock (CallbackTable.locker) {

                CallbackTable.holders[idx] = null;
                CallbackTable.freeIndexes.Push(idx);

            }

        }

        public static int Register(System.Action callback) {

            var holder = new CallbackTable.CallbackHolder(callback);

            return CallbackTable.InsertHolder(holder);

        }

        public static int Register<T>(System.Action<T> callback) {

            var holder = new CallbackTable.CallbackHolder<T>(callback);

            return CallbackTable.InsertHolder(holder);

        }

        public static int Register<T0, T1>(System.Action<T0, T1> callback) {

            var holder = new CallbackTable.CallbackHolder<T0, T1>(callback);

            return CallbackTable.InsertHolder(holder);

        }

        public static int Register<T0, T1, T2>(System.Action<T0, T1, T2> callback) {

            var holder = new CallbackTable.CallbackHolder<T0, T1, T2>(callback);

            return CallbackTable.InsertHolder(holder);

        }

        public static int Register<T0, T1, T2, T3>(System.Action<T0, T1, T2, T3> callback) {

            var holder = new CallbackTable.CallbackHolder<T0, T1, T2, T3>(callback);

            return CallbackTable.InsertHolder(holder);

        }

        public static CallbackHolderBase GetHolder(int idx) {

            return CallbackTable.holders[idx];

        }

        public static Dictionary<string, int> Analyze() {

            var counter = new Dictionary<string, int>();

            for (var i = 0; i < CallbackTable.holders.Count; ++i) {

                var holder = CallbackTable.holders[i];
                if (holder == null || string.IsNullOrEmpty(holder.stackTrace) == true) continue;

                if (counter.ContainsKey(holder.stackTrace) == true) {

                    ++counter[holder.stackTrace];

                } else {

                    counter[holder.stackTrace] = 1;

                }

            }

            counter = counter.Where(x => x.Value > 1).OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            return counter;

        }

        public static void ShowCallbacksCount() {

            UnityEngine.Resources.UnloadUnusedAssets();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            UnityEngine.Debug.Log("Callbacks count: " + ME.CallbackTable.Count);

        }

    }

}