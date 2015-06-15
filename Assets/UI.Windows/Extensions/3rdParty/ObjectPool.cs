//Object Pool ( http://unitypatterns.com/resource/objectpool/ ) is licensed under:
/* The MIT License (MIT)
Copyright (c) 2013 UnityPatterns
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the “Software”), to deal in the
Software without restriction, including without limitation the rights to use, copy,
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Extensions {

	public sealed class ObjectPool : MonoBehaviour {
		static ObjectPool _instance;
		Dictionary<Component, List<Component>> objectLookup = new Dictionary<Component, List<Component>>();
		Dictionary<Component, Component> prefabLookup = new Dictionary<Component, Component>();
		Dictionary<Component, List<Component>> allLookup = new Dictionary<Component, List<Component>>();

		public void Awake() {

			ObjectPool._instance = this;
			
		}

		public static void Clear() {
			instance.objectLookup.Clear();
			instance.prefabLookup.Clear();
			instance.allLookup.Clear();
		}

		public static void RecycleAll<T>(T prefab) where T : Component {
			if (prefab == null) return;

			if (instance.allLookup.ContainsKey(prefab)) {

				foreach (var item in instance.allLookup[prefab]) {
					
					item.Recycle();
					
				}

			}

		}

		public static void ClearPool<T>(T prefab) where T : Component {
			if (prefab == null) return;
			
			if (instance.allLookup.ContainsKey(prefab)) {

				foreach (var item in instance.allLookup[prefab]) {
					
					if (item != null) GameObject.Destroy(item.gameObject);
					
				}
				
				instance.allLookup[prefab].Clear();
				
			}

		}

		public static void CreatePool<T>(T prefab, int capacity, Transform root = null) where T : Component {
			if (prefab == null) return;
			
			if (!instance.objectLookup.ContainsKey(prefab)) {
				
				instance.objectLookup.Add(prefab, new List<Component>());
				
				var preAllocated = new List<Component>();
				for (int i = 0; i < capacity; ++i) {

					var item = prefab.Spawn();
					if (root != null) item.transform.SetParent(root);
					preAllocated.Add(item);
					
				}
				
				foreach (var item in preAllocated) item.Recycle();
				
			}
		}

		public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component {
			if (prefab == null) return null;
			if (instance.objectLookup.ContainsKey(prefab)) {
				T obj = null;
				var list = instance.objectLookup[prefab];
				if (list.Count > 0) {
					
					while (obj == null && list.Count > 0) {
						
						obj = list[0] as T;
						list.RemoveAt(0);
						
					}
					
					if (obj != null) {
						
						obj.transform.SetParent(prefab.transform.parent);
						obj.SetTransformAs(prefab);
						obj.transform.localPosition = position;
						obj.transform.localRotation = rotation;
						obj.gameObject.SetActive(true);
						
						instance.prefabLookup.Add(obj, prefab);
						prefab.AddToAll(obj);
						
						return (T)obj;
						
					}
					
				}

				obj = Object.Instantiate<T>(prefab);
				obj.transform.position = position;
				obj.transform.rotation = rotation;
				obj.transform.SetParent(prefab.transform.parent);
				obj.SetTransformAs(prefab);
				obj.name = prefab.name;
				obj.gameObject.SetActive(true);
				instance.prefabLookup.Add(obj, prefab);
				prefab.AddToAll(obj);

				return (T)obj;
			} else {
				
				T obj = null;
				obj = Object.Instantiate<T>(prefab);
				obj.transform.position = position;
				obj.transform.rotation = rotation;
				obj.transform.SetParent(prefab.transform.parent);
				obj.SetTransformAs(prefab);
				obj.name = prefab.name;
				prefab.AddToAll(obj);

				return (T)obj;
			}
		}

		public static void AddToAll<T>(T prefab, T item) where T : Component {

			if (instance.allLookup.ContainsKey(prefab) == true) {

				var list = instance.allLookup[prefab];
				if (list.Contains(item) == false) list.Add(item);

			} else {

				instance.allLookup.Add(prefab, new List<Component>() { item });

			}

		}

		public static T Spawn<T>(T prefab, Vector3 position) where T : Component {
			return Spawn(prefab, position, Quaternion.identity);
		}

		public static T Spawn<T>(T prefab) where T : Component {
			return Spawn(prefab, Vector3.zero, Quaternion.identity);
		}

		public static void Recycle<T>(T obj) where T : Component {
			if (obj == null) return;

			if (instance.prefabLookup.ContainsKey(obj)) {
				instance.objectLookup[instance.prefabLookup[obj]].Add(obj);
				instance.prefabLookup.Remove(obj);
				//obj.transform.parent = instance.transform;
				obj.gameObject.SetActive(false);
			} else {

				#if UNITY_EDITOR

				if (Application.isPlaying == false) {

					Object.DestroyImmediate(obj.gameObject);

				} else {

					Object.Destroy(obj.gameObject);

				}

				#else

				Object.Destroy(obj.gameObject);

				#endif

			}
		}

		public static int Count<T>(T prefab) where T : Component {
			if (instance.objectLookup.ContainsKey(prefab)) return instance.objectLookup[prefab].Count;
			else return 0;
		}

		public static ObjectPool instance {
			
			get {
				
				#if UNITY_EDITOR
				if (Application.isPlaying == false) {
					
					var items = ObjectPool.FindObjectsOfType<ObjectPool>();
					if (items.Length > 0) {

						foreach (var item in items) GameObject.DestroyImmediate(item);

					}

				}

				if (_instance == null) _instance = ObjectPool.FindObjectOfType<ObjectPool>();
				#endif

				if (_instance != null) return _instance;

				var obj = new GameObject("[A] ObjectPool", typeof(ObjectPool));
				obj.transform.localPosition = Vector3.zero;

				_instance = obj.GetComponent<ObjectPool>();

				return _instance;

			}

		}

	}

	public static class ObjectPoolExtensions {

		public static void AddToAll<T>(this T prefab, T instance) where T : Component {
			
			ObjectPool.AddToAll(prefab, instance);
			
		}

		public static void RecycleAll<T>(this T prefab) where T : Component {
			ObjectPool.RecycleAll(prefab);
		}

		public static void ClearPool<T>(this T prefab) where T : Component {
			ObjectPool.ClearPool(prefab);
		}

		public static void CreatePool<T>(this T prefab, int capacity, Transform root = null) where T : Component {
			ObjectPool.CreatePool(prefab, capacity, root);
		}

		public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component {
			return ObjectPool.Spawn(prefab, position, rotation);
		}

		public static T Spawn<T>(this T prefab, Vector3 position) where T : Component {
			return ObjectPool.Spawn(prefab, position, Quaternion.identity);
		}

		public static T Spawn<T>(this T prefab) where T : Component {
			return ObjectPool.Spawn(prefab, Vector3.zero, Quaternion.identity);
		}

		public static void Recycle<T>(this T obj) where T : Component {
			ObjectPool.Recycle(obj);
		}

		public static int Count<T>(T prefab) where T : Component {
			return ObjectPool.Count(prefab);
		}
	}

	public static class TransformExtensions {
		
		public static void SetParent<T1, T2>(this T1 instance,
		                                     T2 source,
		                                     bool setTransformAsSource = true,
		                                     bool worldPositionStays = true) where T1 : Component where T2 : Component {
			
			if (source != null) {

				instance.transform.SetParent(source.transform, worldPositionStays);

			} else {

				instance.transform.SetParent(null, worldPositionStays);

			}

			if (setTransformAsSource == true) {

				instance.SetTransformAs(source);

			}/* else {

				instance.SetTransformAs();

			}*/

		}

		public static void SetTransformAs<T>(this T instance) where T : Component {
			
			instance.transform.SetTransformAs(null);
			
		}

		public static void SetTransformAs<T1, T2>(this T1 instance, T2 source) where T1 : Component where T2: Component {
			
			instance.transform.SetTransformAs(source.transform);
			
		}

		public static void SetTransformAs(this Transform instance, Transform source) {
			
			if (source == null) {
				
				instance.localPosition = Vector3.zero;
				instance.localRotation = Quaternion.identity;
				instance.localScale = Vector3.one;
				
			} else {
				
				instance.localPosition = source.localPosition;
				instance.localRotation = source.localRotation;
				instance.localScale = source.localScale;

				if (instance is RectTransform && source is RectTransform) {

					var instanceRect = instance as RectTransform;
					var sourceRect = source as RectTransform;
					
					instanceRect.anchoredPosition3D = sourceRect.anchoredPosition3D;
					instanceRect.anchorMin = sourceRect.anchorMin;
					instanceRect.anchorMax = sourceRect.anchorMax;
					instanceRect.sizeDelta = sourceRect.sizeDelta;

				}

			}
			
		}

	}

}
