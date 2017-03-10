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
using System.Linq;

namespace UnityEngine.Extensions {

	public interface IObjectPoolElement {

		/// <summary>
		/// Raises every time Spawn() called.
		/// </summary>
		void OnSpawn();

		/// <summary>
		/// Raises one time only when Spawn() called and no free elements found.
		/// </summary>
		void OnSpawnOnce();

		/// <summary>
		/// Raises every time Recycle() called.
		/// </summary>
		void OnRecycle();

	}

	public sealed class ObjectPool : MonoBehaviour {

	    public class ReferenceInfo {

            public readonly System.WeakReference weakReference;
            public readonly string typeName;
            public readonly string name;
            public readonly string stackTrace;

            public bool isAlive { get { return this.weakReference.IsAlive; } }

            public ReferenceInfo(Component component) {
	            
                this.weakReference = new System.WeakReference(component);
                this.typeName = component.GetType().FullName;
                this.name = component.name;
                this.stackTrace = (new System.Diagnostics.StackTrace()).ToString();

            }

	    }

		private static ObjectPool _instance;
		private Dictionary<Component, List<Component>> objectLookup = new Dictionary<Component, List<Component>>();
		private Dictionary<Component, Component> prefabLookup = new Dictionary<Component, Component>();
		private Dictionary<Component, Component> sceneLookup = new Dictionary<Component, Component>();
		private ME.SimpleDictionary<Component, string> nonPoolComponents = new ME.SimpleDictionary<Component, string>();

#if UNITY_EDITOR && POOL_TRACE
        public readonly List<ReferenceInfo> referenceInfos = new List<ReferenceInfo>();
#endif

        public void Init() {

			ObjectPool._instance = this;
			
		}

		public Dictionary<Component, List<Component>> GetObjectLookup() {

			return this.objectLookup;

		}

		public Dictionary<Component, Component> GetPrefabLookup() {

			return this.prefabLookup;

		}

        public Dictionary<Component, Component> GetSceneLookup() {

			return this.sceneLookup;

		}

        public ME.SimpleDictionary<Component, string> GetNonPoolComponents() {

            return this.nonPoolComponents;

        }

        public static int LookupCount {

	        get {

	            return (from component in instance.objectLookup let components = component.Value select component.Value.Count(x => x != null)).Sum();

	        }

	    }

		public static bool IsRegisteredInPool(Component prefab) {

			return instance.objectLookup.ContainsKey(prefab);

		}

	    public static void Cleanup() {

	        foreach (var component in instance.objectLookup) {

	            var components = component.Value;

	            components.RemoveAll(x => x == null);

	        }

            instance.nonPoolComponents.RemoveAll(x => x.Key == null);
            instance.prefabLookup = instance.prefabLookup.Where(x => x.Key != null).ToDictionary(x => x.Key, x => x.Value);
            instance.sceneLookup = instance.sceneLookup.Where(x => x.Key != null).ToDictionary(x => x.Key, x => x.Value);

        }

	    public static void ClearPoolByType<T>() where T : Component {

	        var keys = instance.objectLookup.Where(x => (x.Key is T) == true).Select(x => x.Key).ToList();
	        foreach (var key in keys) {

	            var components = instance.objectLookup[key];
	            for (var i = 0; i < components.Count; ++i) {

	                var component = components[i];
	                Destroy(component);
                    Destroy(component.gameObject);

	                if (instance.prefabLookup.ContainsKey(component) == true) {

	                    instance.prefabLookup.Remove(component);

	                }

	            }

	            instance.objectLookup.Remove(key);

	        }

	    }

	    public static void RecycleNonPooled<T>() where T : Component {

	        for (var i = 0; i < instance.nonPoolComponents.Count; ++i) {

                var component = instance.nonPoolComponents.GetKeyAt(i) as T;
                if (component == null) continue;

                Destroy(component.gameObject);

            }

			instance.nonPoolComponents.RemoveAll(x => x.Key is T);

	    }

	    public static void ClearReserved() {

	        foreach (var component in instance.objectLookup) {

	            var components = component.Value;

                for (var i = 0; i < components.Count; ++i) {

	                var comp = components[i];

                    if (instance.prefabLookup.ContainsKey(comp) == true) {

                        instance.prefabLookup.Remove(comp);

                    }

                    if (comp == null) continue;

                    var go = comp.gameObject;
                    if (go == null) continue;

                    Destroy(go);

	            }

                components.Clear();

            }

        }

	    public static void DestroyAll() {

	        for (var i = 0; i < instance.nonPoolComponents.Count; ++i) {

                var key = instance.nonPoolComponents.GetKeyAt(i);
                if (key == null) continue;

                try {

                    GameObject.Destroy(key);

                } catch (System.Exception) {
	                
	            }

	        }

            instance.nonPoolComponents.Clear();

            foreach (var obj in instance.objectLookup) {

	            var list = obj.Value;
	            for (var i = 0; i < list.Count; ++i) {

                    try {

                        GameObject.Destroy(list[i].gameObject);

                    } catch (System.Exception) {

                    }

                }

	        }

            instance.objectLookup.Clear();


            foreach (var component in instance.prefabLookup) {
	            
                if (component.Key == null) continue;

                try {

                    GameObject.Destroy(component.Key.gameObject);

                } catch (System.Exception) {

                }

            }

            instance.prefabLookup.Clear();

            foreach (var component in instance.sceneLookup) {

                if (component.Key == null)
                    continue;

                try {

                    GameObject.Destroy(component.Key.gameObject);

                } catch (System.Exception) {

                }

            }

            instance.sceneLookup.Clear();

        }

	    public static void DestroyByType<T>() where T : Component {

	        var keys = instance.objectLookup.Keys.OfType<T>().ToList();
	        for (var i = 0; i < keys.Count; ++i) {

	            var key = keys[i];
	            var list = instance.objectLookup[key];
                for (var j = 0; j < list.Count; ++j) {

                    GameObject.Destroy(list[j].gameObject);

                }

	            instance.nonPoolComponents.Remove(key);
	            instance.objectLookup.Remove(key);

	        }

	        var prefabKeys = instance.prefabLookup.Keys.OfType<T>().ToList();
	        for (var i = 0; i < prefabKeys.Count; ++i) {

	            var item = prefabKeys[i];
                GameObject.Destroy(item.gameObject);

	            instance.prefabLookup.Remove(item);
                instance.nonPoolComponents.Remove(item);

            }

            var sceneKeys = instance.sceneLookup.Keys.OfType<T>().ToList();
            for (var i = 0; i < sceneKeys.Count; ++i) {

                var item = sceneKeys[i];
                GameObject.Destroy(item.gameObject);

                instance.sceneLookup.Remove(item);
                instance.nonPoolComponents.Remove(item);

            }

        }

		public static void Clear() {

			instance.objectLookup.Clear();
			instance.prefabLookup.Clear();
            instance.sceneLookup.Clear();

        }

		public static void RecycleAll<T>(T prefab) where T : Component {

			if (prefab == null) return;

			if (instance.objectLookup.ContainsKey(prefab) == true) {

				var list = new List<Component>();
				foreach (var item in instance.sceneLookup) {

					if (item.Value == prefab) {

						list.Add(item.Key);

					}

				}

				foreach (var item in list) {
					
					item.Recycle();

				}

				list.Clear();
				list = null;

			}

		}

		public static void ClearPool<T>(T prefab) where T : Component {

			if (prefab == null) return;

			prefab.RecycleAll();

			if (instance.objectLookup.ContainsKey(prefab)) {

				foreach (var item in instance.objectLookup[prefab]) {
					
					if (item != null) GameObject.Destroy(item.gameObject);
					instance.prefabLookup.Remove(item);
					instance.sceneLookup.Remove(item);

				}
				
				instance.objectLookup[prefab].Clear();
				instance.objectLookup.Remove(prefab);

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

						item = spawner.Invoke(prefab);

					}  else {

						item = prefab.Spawn();

					}

					if (root != null) item.transform.SetParent(root);
#if UNITY_EDITOR && POOL_TRACE
                    instance.referenceInfos.Add(new ReferenceInfo(item));
#endif
                    preAllocated.Add(item);

				}
				
				foreach (var item in preAllocated) item.Recycle();
				
			}

		}

#if UNITY_EDITOR && POOL_TRACE

        public void CleanupReferenceInfos() {

            this.referenceInfos.RemoveAll(x => x.isAlive == false);

        }

#endif

		public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, bool activeByDefault) where T : Component {

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
							obj.gameObject.SetActive(activeByDefault);
							
							instance.prefabLookup[obj] = prefab;
	                        instance.sceneLookup.Add(obj, prefab);

							if (obj is IObjectPoolElement) (obj as IObjectPoolElement).OnSpawn();

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
				obj.gameObject.SetActive(activeByDefault);
				instance.prefabLookup[obj] = prefab;
                instance.sceneLookup.Add(obj, prefab);
#if UNITY_EDITOR && POOL_TRACE
                instance.referenceInfos.Add(new ReferenceInfo(obj));
#endif

				if (obj is IObjectPoolElement) {

					var objElement = (obj as IObjectPoolElement);
					objElement.OnSpawn();
					objElement.OnSpawnOnce();

				}

                return (T)obj;

			} else {
				
				obj = ObjectPool.InstantiateSource<T>(prefab);
				obj.transform.position = position;
				obj.transform.rotation = rotation;
				obj.transform.SetParent(prefab.transform.parent);
				obj.SetTransformAs(prefab);
				obj.name = prefab.name;
				obj.gameObject.hideFlags = HideFlags.None;
				obj.gameObject.SetActive(activeByDefault);

#if UNITY_EDITOR
                var stackTrace = new System.Diagnostics.StackTrace();
				instance.nonPoolComponents[obj] = stackTrace.ToString() + " :: " + (obj != null ? (obj.name + " :: " + ((obj is UnityEngine.UI.Windows.WindowObject) ? ((obj as UnityEngine.UI.Windows.WindowObject).GetWindow() != null ? (obj as UnityEngine.UI.Windows.WindowObject).GetWindow().name : "Window is NULL") : "Not WindowObject")) : "Null");
#else
                instance.nonPoolComponents[obj] = null;
#endif

#if UNITY_EDITOR && POOL_TRACE
                instance.referenceInfos.Add(new ReferenceInfo(obj));
#endif

				if (obj is IObjectPoolElement) (obj as IObjectPoolElement).OnSpawn();

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

			return Spawn(prefab, position, Quaternion.identity, activeByDefault: true);

		}

		public static T Spawn<T>(T prefab) where T : Component {

			return Spawn(prefab, Vector3.zero, Quaternion.identity, activeByDefault: true);

		}

		public static void Recycle<T>(T obj, bool setInactive = true) where T : Component {

			if (obj == null) return;
			if (instance == null) return;

			if (obj is IObjectPoolElement) (obj as IObjectPoolElement).OnRecycle();

		    if (instance.sceneLookup.ContainsKey(obj) == true) {

		        instance.objectLookup[instance.prefabLookup[obj]].Add(obj);
		        instance.sceneLookup.Remove(obj);
				if (setInactive == true) {

					obj.transform.SetParent(null);
					obj.gameObject.SetActive(false);

				}

		    } else if (instance.prefabLookup.ContainsKey(obj) == false) {

		        instance.nonPoolComponents.Remove(obj);

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

			return ObjectPool.Spawn(prefab, position, rotation, activeByDefault: true);

		}

		public static T Spawn<T>(this T prefab, Vector3 position) where T : Component {

			return ObjectPool.Spawn(prefab, position, Quaternion.identity, activeByDefault: true);

		}

		public static T Spawn<T>(this T prefab, bool activeByDefault) where T : Component {

			return ObjectPool.Spawn(prefab, Vector3.zero, Quaternion.identity, activeByDefault);

		}

		public static T Spawn<T>(this T prefab) where T : Component {

			return ObjectPool.Spawn(prefab, Vector3.zero, Quaternion.identity, activeByDefault: true);

		}

		public static void Recycle<T>(this T obj) where T : Component {

			ObjectPool.Recycle(obj);

		}

		public static void Recycle<T>(this T obj, bool setInactive) where T : Component {

			ObjectPool.Recycle(obj, setInactive);

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
