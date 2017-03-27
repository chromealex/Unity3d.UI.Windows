using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Events;

namespace ME.UAB.Extensions {
	
	#region POOL
	public abstract class ListPoolBase {

        protected static List<ListPoolBase> allocatedPools = new List<ListPoolBase>();

        public static void Destroy() {

            for (var i = 0; i < ListPoolBase.allocatedPools.Count; ++i) {

                ListPoolBase.allocatedPools[i].Reset();

            }

            ListPoolBase.allocatedPools.Clear();

        }

        protected abstract void Reset();

    }

	public class ListPool<T> : ListPoolBase {

		private readonly ObjectPoolInternal<List<T>> listPool = new ObjectPoolInternal<List<T>>(null, l => l.Clear());
	    private static ListPool<T> instance;

        /*
        public static void Allocate(int capacity) {

			ListPool<T>.listPool = new ObjectPoolInternal<List<T>>(null, l => l.Clear(), capacity);

		}
        */

	    private static void CreateInstance() {

	        if (ListPool<T>.instance != null) return;

            ListPool<T>.instance = new ListPool<T>();
            ListPoolBase.allocatedPools.Add(ListPool<T>.instance);


        }

	    private List<T> GetAllocated() {

            return this.listPool.Get();

        }

        public static List<T> Get() {

            ListPool<T>.CreateInstance();

            return ListPool<T>.instance.GetAllocated();

		}

	    public void ReleaseAllocated(List<T> toRelease) {

			if (toRelease == null) return;

			this.listPool.Release(toRelease);

        }


	    public static void Release(List<T> toRelease) {

            ListPool<T>.CreateInstance();

			ListPool<T>.instance.ReleaseAllocated(toRelease);

		}

	    protected override void Reset() {

	        ListPool<T>.instance = null;

	    }

	}

	public static class ObjectPool<T> where T : new() {

		private static ObjectPoolInternal<T> pool = new ObjectPoolInternal<T>(null, null);

		public static void Allocate(int capacity) {

			ObjectPool<T>.pool = new ObjectPoolInternal<T>(null, null, capacity);

		}

		public static T Get() {

			return ObjectPool<T>.pool.Get();

		}

		public static void Release(T toRelease) {

			ObjectPool<T>.pool.Release(toRelease);

		}

	}

	public interface IObjectPoolItem {

		void OnPoolGet();
		void OnPoolRelease();

	}

	public static class ObjectPoolEventable<T> where T : IObjectPoolItem, new() {

		private static ObjectPoolInternal<T> pool = new ObjectPoolInternal<T>(x => x.OnPoolGet(), x => x.OnPoolRelease());

		public static void Allocate(int capacity) {

			ObjectPoolEventable<T>.pool = new ObjectPoolInternal<T>(x => x.OnPoolGet(), x => x.OnPoolRelease(), capacity);

		}

		public static T Get() {

			return ObjectPoolEventable<T>.pool.Get();

		}

		public static void Release(T toRelease) {

			ObjectPoolEventable<T>.pool.Release(toRelease);

		}

	}

	internal class ObjectPoolInternal<T> where T : new() {
		
		private readonly Stack<T> stack = new Stack<T>();
		private readonly System.Action<T> actionOnGet;
		private readonly System.Action<T> actionOnRelease;

		public int countAll { get; private set; }
		public int countActive { get { return this.countAll - this.countInactive; } }
		public int countInactive { get { return this.stack.Count; } }

		public ObjectPoolInternal(System.Action<T> actionOnGet, System.Action<T> actionOnRelease, int capacity = 1) {
			
			this.actionOnGet = actionOnGet;
			this.actionOnRelease = actionOnRelease;

			if (capacity > 0) {

				this.stack = new Stack<T>(capacity);

			}

		}

		public T Get() {
			
			T element;
			if (this.stack.Count == 0) {
				
				element = new T();
				++this.countAll;

			} else {

				element = this.stack.Pop();
				if (element == null) element = new T();

			}

			if (this.actionOnGet != null) this.actionOnGet.Invoke(element);

			return element;

		}

		public void Release(T element) {
			
			if (this.stack.Count > 0 && ObjectPool<T>.ReferenceEquals(this.stack.Peek(), element) == true) Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");

			if (this.actionOnRelease != null) this.actionOnRelease.Invoke(element);
			this.stack.Push(element);

		}

	}
	#endregion

}
