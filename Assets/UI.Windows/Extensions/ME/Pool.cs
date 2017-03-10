using UnityEngine;
using System.Collections.Generic;

namespace MW2.Extensions {

	public interface IPoolItem {
		
		bool inPool { get; set; }
		int poolIndex { get; set; }

		void OnNewPool();

	}

	public class PoolItem : IPoolItem {
		
		public bool inPool { get; set; }
		public int poolIndex { get; set; }

	    public static void CreatePool<T>(int capacity = 0) where T : IPoolItem, new() {

            var type = typeof(T);
            type.CreatePool<T>(capacity);

        }

	    public static T Spawn<T>(int capacity = 0) where T : IPoolItem, new() {

            PoolItem.CreatePool<T>(capacity);
            var type = typeof(T);
            return type.Spawn<T>();

		}

		public virtual void OnNewPool() {}

	}

	public static class PoolExtensions {
		
		public static void AddToAll<T>(this System.Type prefab, T instance) where T : IPoolItem {
			
			Pool.AddToAll<T>(prefab, instance);
			
		}
		
		public static void RecycleAll<T>(this System.Type prefab) where T : IPoolItem {

			Pool.RecycleAll<T>(prefab);

		}
		
		public static void ClearPool<T>(this System.Type prefab) where T : IPoolItem, new() {

			Pool.ClearPool<T>(prefab);

		}
		
		public static void CreatePool<T>(this System.Type prefab, int capacity) where T : IPoolItem, new() {

			Pool.CreatePool<T>(prefab, capacity);

		}

		public static T Spawn<T>(this System.Type prefab) where T : IPoolItem, new() {

			return Pool.Spawn<T>(prefab);

		}
		
		public static void Recycle<T>(this T obj) where T : IPoolItem {

			Pool.Recycle<T>(obj);

		}
		
		public static int Count<T>(System.Type prefab) where T : IPoolItem {

			return Pool.Count<T>(prefab);

		}

	}

	public static class Pool {
		
		private static Dictionary<System.Type, List<IPoolItem>> objectLookup = new Dictionary<System.Type, List<IPoolItem>>();
		private static Dictionary<System.Type, int> objectLookupCounters = new Dictionary<System.Type, int>();
		private static Dictionary<IPoolItem, System.Type> prefabLookup = new Dictionary<IPoolItem, System.Type>();
		private static Dictionary<System.Type, List<IPoolItem>> allLookup = new Dictionary<System.Type, List<IPoolItem>>();

		public static void Clear() {
			
			Pool.objectLookup.Clear();
			Pool.objectLookupCounters.Clear();
			Pool.prefabLookup.Clear();
			Pool.allLookup.Clear();

		}
		
		public static void RecycleAll<T>(System.Type prefab) where T : IPoolItem {

			if (prefab == null) return;
			
			if (Pool.allLookup.ContainsKey(prefab)) {
				
				foreach (var item in Pool.allLookup[prefab]) {
					
					item.Recycle();
					
				}
				
			}
			
		}
		
		public static void ClearPool<T>(System.Type prefab) where T : IPoolItem {

			if (prefab == null) return;
			
			if (Pool.allLookup.ContainsKey(prefab)) {
				
				/*foreach (var item in Pool.allLookup[prefab]) {
					
					if (item != null) {

						// destroy item

					}
					
				}*/
				
				Pool.allLookup[prefab].Clear();
				
			}
			
		}
		
		public static void CreatePool<T>(System.Type prefab, int capacity) where T : IPoolItem, new() {

			if (prefab == null) return;
			
			if (Pool.objectLookup.ContainsKey(prefab) == false) {
				
				Pool.objectLookup.Add(prefab, new List<IPoolItem>());
				Pool.objectLookupCounters.Add(prefab, 0);
				Debug.LogWarning("ADD POOL[" + capacity + "]: " + prefab);

				var preAllocated = new List<IPoolItem>();
				for (int i = 0; i < capacity; ++i) {
					
					var item = prefab.Spawn<T>();
					preAllocated.Add(item);
					
				}
				
				foreach (var item in preAllocated) item.Recycle();
				
			}

		}
		
		public static T Spawn<T>(System.Type prefab) where T : IPoolItem, new() {

			if (prefab == null) return default(T);

			if (Pool.objectLookup.ContainsKey(prefab) == true) {

				T obj = default(T);
				var list = Pool.objectLookup[prefab];
				if (list.Count > 0) {
					
					while (obj == null && list.Count > 0) {
						
						obj = (T)list[0];
						list.RemoveAt(0);
						
					}
					
					if (obj != null) {

						obj.inPool = true;
						if (obj.poolIndex == 0) obj.poolIndex = ++Pool.objectLookupCounters[prefab];
						obj.OnNewPool();

						Pool.prefabLookup.Add(obj, prefab);
						prefab.AddToAll(obj);
						
						return (T)obj;
						
					}
					
				}
				
				obj = new T();
                ME.WeakReferenceInfo.Register(obj);
                obj.inPool = true;
				if (obj.poolIndex == 0) obj.poolIndex = ++Pool.objectLookupCounters[prefab];
				obj.OnNewPool();

				Pool.prefabLookup.Add(obj, prefab);
				prefab.AddToAll(obj);
				
				return (T)obj;

			} else {
				
				T obj = default(T);
				obj = new T();
                ME.WeakReferenceInfo.Register(obj);
                obj.inPool = false;

				prefab.AddToAll(obj);
				
				return (T)obj;

			}

		}
		
		public static void AddToAll<T>(System.Type prefab, T item) where T : IPoolItem {
			
			if (Pool.allLookup.ContainsKey(prefab) == true) {
				
				var list = Pool.allLookup[prefab];
				if (list.Contains(item) == false) list.Add(item);
				
			} else {
				
				Pool.allLookup.Add(prefab, new List<IPoolItem>() { item });
				
			}
			
		}

		public static void Recycle<T>(T obj) where T : IPoolItem {

			if (obj == null) return;
			
			if (Pool.prefabLookup.ContainsKey(obj) == true) {

				Pool.objectLookup[Pool.prefabLookup[obj]].Add(obj);
				Pool.prefabLookup.Remove(obj);

				obj.inPool = false;

			} else {

				obj.inPool = false;
				obj = default(T);

			}

		}
		
		public static int Count<T>(System.Type prefab) where T : IPoolItem {

			if (Pool.objectLookup.ContainsKey(prefab)) {

				return Pool.objectLookup[prefab].Count;

			}

			return 0;

		}

	}

}