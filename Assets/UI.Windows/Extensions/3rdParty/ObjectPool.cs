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

using System.Collections.Generic;

namespace UnityEngine.Extensions {

	public sealed class ObjectPool : MonoBehaviour {

		static ObjectPool _instance;
		Dictionary<Component, List<Component>> objectLookup = new Dictionary<Component, List<Component>>();
		Dictionary<Component, Component> prefabLookup = new Dictionary<Component, Component>();
        Dictionary<Component, Component> sceneLookup = new Dictionary<Component, Component>();

		public void Init() {

			ObjectPool._instance = this;
			
		}

		public static void Clear() {

			instance.objectLookup.Clear();
			instance.prefabLookup.Clear();
            instance.sceneLookup.Clear();

        }

		public static void RecycleAll<T>(T prefab) where T : Component {

			if (prefab == null) return;

			if (instance.objectLookup.ContainsKey(prefab)) {

				foreach (var item in instance.objectLookup[prefab]) {
					
					item.Recycle();
					
				}

			}

		}

		public static void ClearPool<T>(T prefab) where T : Component {

			if (prefab == null) return;
			
			if (instance.objectLookup.ContainsKey(prefab)) {

				foreach (var item in instance.objectLookup[prefab]) {
					
					if (item != null) GameObject.Destroy(item.gameObject);
					
				}
				
				instance.objectLookup[prefab].Clear();
				
			}

		}

		public static void CreatePool<T>(T prefab, int capacity, System.Func<T, T> spawner = null, Transform root = null) where T : Component {

			if (prefab == null) return;
			
			if (!instance.objectLookup.ContainsKey(prefab)) {
				
				instance.objectLookup.Add(prefab, new List<Component>());
				
				var preAllocated = new List<Component>();
				for (int i = 0; i < capacity; ++i) {

					T item = null;
					if (spawner != null) {

						item = spawner(prefab);

					}  else {

						item = prefab.Spawn();

					}

					if (root != null) item.transform.SetParent(root);
					preAllocated.Add(item);

				}
				
				foreach (var item in preAllocated) item.Recycle();
				
			}

		}

		public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component {

			if (prefab == null) return null;

			T obj = null;

			if (instance.objectLookup.ContainsKey(prefab) == true) {
				
				#if UNITY_EDITOR
				if (Application.isPlaying == true) {
				#endif

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
						
						instance.prefabLookup[obj] = prefab;
                        instance.sceneLookup.Add(obj, prefab);
						
						return (T)obj;
						
					}
					
				}

				#if UNITY_EDITOR
				}
				#endif
				
				obj = ObjectPool.InstantiateSource<T>(prefab);
				obj.transform.position = position;
				obj.transform.rotation = rotation;
				obj.transform.SetParent(prefab.transform.parent);
				obj.SetTransformAs(prefab);
				obj.name = prefab.name;
				obj.gameObject.hideFlags = HideFlags.None;
				obj.gameObject.SetActive(true);
				instance.prefabLookup[obj] = prefab;
                instance.sceneLookup.Add(obj, prefab);

                return (T)obj;

			} else {
				
				obj = ObjectPool.InstantiateSource<T>(prefab);
				obj.transform.position = position;
				obj.transform.rotation = rotation;
				obj.transform.SetParent(prefab.transform.parent);
				obj.SetTransformAs(prefab);
				obj.name = prefab.name;
				obj.gameObject.hideFlags = HideFlags.None;
				obj.gameObject.SetActive(true);

                return (T)obj;

			}

		}

		public static T InstantiateSource<T>(T source) where T : Component {

			#if UNITY_EDITOR
			if (Application.isPlaying == false) {

				if (ME.EditorUtilities.IsPrefab(source.gameObject) == true) {

					#if UNITY_5_3_0
					Debug.LogWarning("Unity 5.3.0 bug: Creating through editor-mode from prefabs causes hidden gameobjects. You must create it manual: Screen first, Layout the second and components at last.");
					#endif
					
					#if UNITY_5_2
					var go = UnityEditor.PrefabUtility.InstantiatePrefab(source.gameObject) as GameObject;
					#else
					var go = UnityEditor.PrefabUtility.InstantiatePrefab(source.gameObject, SceneManagement.SceneManager.GetActiveScene()) as GameObject;
					#endif
					return go.GetComponent<T>();

				}

			}
			#endif

			return Object.Instantiate<T>(source);

		}
        
		public static T Spawn<T>(T prefab, Vector3 position) where T : Component {

			return Spawn(prefab, position, Quaternion.identity);

		}

		public static T Spawn<T>(T prefab) where T : Component {

			return Spawn(prefab, Vector3.zero, Quaternion.identity);

		}

		public static void Recycle<T>(T obj) where T : Component {

			if (obj == null) return;
			if (instance == null) return;

		    if (instance.sceneLookup.ContainsKey(obj) == true) {

		        instance.objectLookup[instance.prefabLookup[obj]].Add(obj);
		        instance.sceneLookup.Remove(obj);
		        obj.gameObject.SetActive(false);

		    } else if (instance.prefabLookup.ContainsKey(obj) == false) {

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

			if (instance.objectLookup.ContainsKey(prefab)) {

				return instance.objectLookup[prefab].Count;

			}

			return 0;

		}

		public static ObjectPool instance {
			
			get {
				
				#if UNITY_EDITOR
				if (ObjectPool._instance == null) {

					ObjectPool._instance = ObjectPool.FindObjectOfType<ObjectPool>();

				}
				#endif

				return ObjectPool._instance;
				/*
				var go = GameObject.Find("WindowSystemInitializer/ObjectPool");
				if (go != null) _instance = go.GetComponent<ObjectPool>();
				
				if (_instance != null) return _instance;

				go = new GameObject("[A] ObjectPool", typeof(ObjectPool));
				go.transform.localPosition = Vector3.zero;

				_instance = go.GetComponent<ObjectPool>();

				return _instance;*/

			}

		}

	}

	public static class ObjectPoolExtensions {
        
		public static void RecycleAll<T>(this T prefab) where T : Component {

			ObjectPool.RecycleAll(prefab);

		}

		public static void ClearPool<T>(this T prefab) where T : Component {

			ObjectPool.ClearPool(prefab);

		}
		
		public static void CreatePool<T>(this T prefab, int capacity, Transform root = null) where T : Component {

			ObjectPool.CreatePool(prefab, capacity, root: root);

		}
		
		public static void CreatePool<T>(this T prefab, int capacity, System.Func<T, T> spawner, Transform root = null) where T : Component {

			ObjectPool.CreatePool(prefab, capacity, spawner, root);

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

	public enum TransformEntity : uint {
		PositionX = 0x1,
		PositionY = 0x2,
		PositionZ = 0x4,
		Position = PositionX | PositionY | PositionZ,
		RotationX = 0x8,
		RotationY = 0x10,
		RotationZ = 0x20,
		Rotation = RotationX | RotationY | RotationZ,
		ScaleX = 0x40,
		ScaleY = 0x80,
		ScaleZ = 0x100,
		Scale = ScaleX | ScaleY | ScaleZ,
		All = Position | Rotation | Scale,
	};

	public static class TransformExtensions {

		public static void ResetTransform<T>(this T source, TransformEntity entities = TransformEntity.All) where T : Component {

			source.transform.ResetTransform(entities);

		}

		public static void ResetTransform(this Transform source, TransformEntity entities = TransformEntity.All) {

			var position = source.localPosition;
			var rotation = source.localRotation.eulerAngles;
			var scale = source.localScale;

			if ((entities & TransformEntity.PositionX) != 0) position.Scale(new Vector3(0f, 1f, 1f));
			if ((entities & TransformEntity.PositionY) != 0) position.Scale(new Vector3(1f, 0f, 1f));
			if ((entities & TransformEntity.PositionZ) != 0) position.Scale(new Vector3(1f, 1f, 0f));

			if ((entities & TransformEntity.RotationX) != 0) rotation.Scale(new Vector3(0f, 1f, 1f));
			if ((entities & TransformEntity.RotationY) != 0) rotation.Scale(new Vector3(1f, 0f, 1f));
			if ((entities & TransformEntity.RotationZ) != 0) rotation.Scale(new Vector3(1f, 1f, 0f));

			if ((entities & TransformEntity.ScaleX) != 0) scale.x = 1f;
			if ((entities & TransformEntity.ScaleY) != 0) scale.y = 1f;
			if ((entities & TransformEntity.ScaleZ) != 0) scale.z = 1f;

			source.localPosition = position;
			source.localRotation = Quaternion.Euler(rotation);
			source.localScale = scale;

		}

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
