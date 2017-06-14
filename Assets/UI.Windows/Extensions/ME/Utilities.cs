using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine.UI.Windows;

namespace ME {

    public class Math {
		
		public static long Lerp(long a, long b, float t) {
			
			return a + (long)((b - a) * Mathf.Clamp01(t));
			
		}
		
		public static int Lerp(int a, int b, float t) {
			
			return a + (int)((b - a) * Mathf.Clamp01(t));
			
		}

	}

	public static class ObjectExt {

		public static int GetID(this Object obj) {

			if (obj == null) return 0;

			return obj.GetInstanceID();

		}

		/*public static int GetID(this Texture obj) {

			if (obj == null) return 0;

			return obj.GetInstanceID();

		}*/

	}

	public static class ReflectionExt {

		public static bool HasBaseType(this System.Type type, System.Type checkType) {

			return type.GetBaseClassesAndInterfaces().Any(x => x == checkType);

		}

		public static IEnumerable<System.Type> GetBaseClassesAndInterfaces(this System.Type type) {
			
			return type.BaseType == typeof(object) 
				? type.GetInterfaces()
					: Enumerable
					.Repeat(type.BaseType, 1)
					.Concat(type.GetInterfaces())
					.Concat(type.BaseType.GetBaseClassesAndInterfaces())
					.Distinct();
			
		}

	}

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

			ME.WeakReferenceInfo.Register(this);

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

	public partial class Utilities {
		
		public static uint GetHash(string s) {
			
			uint hash = 0;
			foreach (char c in s) {
				
				hash = 31 * hash + c;
				
			}
			return hash;
			
		}

		public static string FormatParameter(System.Type type) {

			var pattern = @"(`\d+)";
			var result = type.ToString()
				.Replace("[", "<")
				.Replace("]", ">")
				.Replace("+", ".")
				.Replace(",", ", ");

			result = Regex.Replace(result, pattern, string.Empty);
			result = result.Replace("System.Object<>", "params object[]");
			result = result.Replace("<>", "[]");
			return result;

		}

		public static Vector4 GetDrawingDimensions(Graphic graphic, bool shouldPreserveAspect, Vector2 size, Vector4 padding) {
			
			Vector4 vector = padding;//(!(this.overrideSprite == null)) ? DataUtility.GetPadding (this.overrideSprite) : Vector4.get_zero ();
			Vector2 vector2 = new Vector2(size.x, size.y);
			Rect pixelAdjustedRect = graphic.GetPixelAdjustedRect();
			int num = Mathf.RoundToInt(vector2.x);
			int num2 = Mathf.RoundToInt(vector2.y);
			Vector4 result = new Vector4(vector.x / (float)num, vector.y / (float)num2, ((float)num - vector.z) / (float)num, ((float)num2 - vector.w) / (float)num2);
			if (shouldPreserveAspect && vector2.sqrMagnitude > 0) {
				float num3 = vector2.x / vector2.y;
				float num4 = pixelAdjustedRect.width / pixelAdjustedRect.height;
				if (num3 > num4) {
					float height = pixelAdjustedRect.height;
					pixelAdjustedRect.height = (pixelAdjustedRect.width * (1 / num3));
					pixelAdjustedRect.y = (pixelAdjustedRect.y + (height - pixelAdjustedRect.height) * graphic.rectTransform.pivot.y);
				}
				else {
					float width = pixelAdjustedRect.width;
					pixelAdjustedRect.width = (pixelAdjustedRect.height * num3);
					pixelAdjustedRect.x = (pixelAdjustedRect.x + (width - pixelAdjustedRect.width) * graphic.rectTransform.pivot.x);
				}
			}
			result = new Vector4 (pixelAdjustedRect.x + pixelAdjustedRect.width * result.x, pixelAdjustedRect.y + pixelAdjustedRect.height * result.y, pixelAdjustedRect.x + pixelAdjustedRect.width * result.z, pixelAdjustedRect.y + pixelAdjustedRect.height * result.w);
			return result;

		}

		public static T FindReferenceParent<T>(GameObject root) {

			var result = default(T);

			if (root != null) {

				var list = ListPool<T>.Get();
				root.GetComponentsInParent<T>(true, list);
				if (list.Count > 0) {
					
					result = list[0];

				}
				ListPool<T>.Release(list);

			}

			return result;

		}

		public static T FindReferenceChildren<T>(GameObject root, System.Func<T, bool> callback = null) {

			var result = default(T);

			if (root != null) {

				var list = ListPool<T>.Get();
				root.GetComponentsInChildren<T>(true, list);
				if (list.Count > 0) {

					if (callback != null) {

						for (int i = 0; i < list.Count; ++i) {

							if (callback.Invoke(list[i]) == true) {

								result = list[i];
								break;

							}

						}

					} else {

						result = list[0];

					}

				}
				ListPool<T>.Release(list);

			}

			return result;

		}

		public static T FindReferenceParent<T>(Component root) {

			if (root == null) return default(T);

			return Utilities.FindReferenceParent<T>(root.gameObject);

		}

		public static T FindReferenceChildren<T>(Component root) {

			if (root == null) return default(T);

			return Utilities.FindReferenceChildren<T>(root.gameObject);

		}

		public static T FindReferenceChildren<T>(Component root, System.Func<T, bool> callback) {

			if (root == null) return default(T);

			return Utilities.FindReferenceChildren<T>(root.gameObject, callback);

		}

        public static void FindReferenceParent<T>(Component root, ref T link) where T : Component {

			link = Utilities.FindReferenceParent<T>(root);

		}

		public static void FindReference<T>(Component root, ref T link) where T : Component {

			link = Utilities.FindReferenceChildren<T>(root);

		}

		public static Vector3 GetUIPosition(Transform transform, Vector3 alignToPoint, Camera uiCamera, Camera gameCamera, Vector3 offset = default(Vector3)) {

			var position = alignToPoint + offset;
			return uiCamera.ViewportToWorldPoint(gameCamera.WorldToViewportPoint(position));

		}

		public static System.Diagnostics.Stopwatch StartWatch() {

			return new System.Diagnostics.Stopwatch();

		}

		public static void ResetWatch(System.Diagnostics.Stopwatch timer) {

			if (timer != null) {

				timer.Reset();
				timer.Start();

			}

		}

		public static bool StopWatch(System.Diagnostics.Stopwatch timer) {

			if (timer != null) {

				timer.Stop();
				var milliseconds = timer.ElapsedMilliseconds;
				if (milliseconds < 1000L - 1000L / Application.targetFrameRate) {

					return true;

				}

				return false;

			}

			return true;

		}

		public static void CallInSequence<T>(System.Action callback, System.Action<T, System.Action> each, params T[] collection) {

			Utilities.CallInSequence(callback, (IEnumerable<T>)collection, each);

		}

		public static void CallInSequence<T>(System.Action callback, bool waitPrevious, System.Action<T, System.Action> each, params T[] collection) {

			Utilities.CallInSequence(callback, (IEnumerable<T>)collection, each, waitPrevious);

		}

		public static void CallInSequence<T>(System.Action callback, IEnumerable<T> collection, System.Action<T, System.Action> each, bool waitPrevious = false) {

			if (collection == null) {

				if (callback != null) callback.Invoke();
				return;

			}

			var count = collection.Count();

			var completed = false;
			var counter = 0;
			System.Action callbackItem = () => {

				++counter;
				if (counter < count) return;

				completed = true;

				if (callback != null) callback();
				
			};

			if (waitPrevious == true) {

				var ie = collection.GetEnumerator();

				System.Action doNext = null;
				doNext = () => {

					if (ie.MoveNext() == true) {

						if (ie.Current != null) {

							each(ie.Current, () => {
								
								callbackItem();
								doNext();

							});

						} else {

							callbackItem();
							doNext();

						}

					}

				};
				doNext();

			} else {

				var ie = collection.GetEnumerator();
				while (ie.MoveNext() == true) {

					if (ie.Current != null) {

						each(ie.Current, callbackItem);

					} else {

						callbackItem();

					}

					if (completed == true) break;

				}

			}

			if (count == 0 && callback != null) callback();

		}
		
		public static Texture2D ScreenToTexture() {

			var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			texture.Apply();

			return texture;

		}

		public static void ScreenToTexture(Texture2D texture) {

			texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			texture.Apply();

		}

		public static void CameraToTexture(Camera camera, Texture2D texture) {

			if (camera == null) {

				texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
				texture.Apply();

			} else {

				var camGo = new GameObject("Cam");
				camGo.transform.SetParent(camera.transform);
				camGo.transform.localPosition = Vector3.zero;
				var cam = camGo.AddComponent<Camera>();
				cam.CopyFrom(camera);

				RenderTexture rt = new RenderTexture(texture.width, texture.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				cam.targetTexture = rt;
				cam.Render();

				texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
				texture.Apply();
				cam.targetTexture = null;

				RenderTexture.active = null;
				Object.Destroy(rt);
				GameObject.DestroyImmediate(camGo);

			}

		}

		public static Texture2D GetRTPixels(RenderTexture rt) {

			RenderTexture currentActiveRT = RenderTexture.active;
			RenderTexture.active = rt;
			Texture2D tex = new Texture2D(rt.width, rt.height);
			tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			RenderTexture.active = currentActiveRT;
			return tex;

		}

	}

    public class WeakReferenceInfo {

        private static readonly List<WeakReferenceInfo> weakReferences = new List<WeakReferenceInfo>();
		private static readonly List<WeakReferenceInfo> weakReferencesBattle = new List<WeakReferenceInfo>();
        private static List<WeakReferenceInfo> weakReferencesTemp = new List<WeakReferenceInfo>();

        private static Dictionary<string, WeakReferenceInfo> dbWeakReferences = new Dictionary<string, WeakReferenceInfo>();

        public readonly System.WeakReference weakReference;
        public readonly string comment;
        public readonly System.Type type;

        public WeakReferenceInfo(System.WeakReference weakReference, System.Type type, string comment = "") {

            this.weakReference = weakReference;
            this.comment = comment;
            this.type = type;

        }

        public static void Cleanup() {

            Resources.UnloadUnusedAssets();

            WeakReferenceInfo.weakReferences.RemoveAll(x => x.weakReference.IsAlive == false);
			WeakReferenceInfo.weakReferencesBattle.RemoveAll(x => x.weakReference.IsAlive == false);
            WeakReferenceInfo.weakReferencesTemp.RemoveAll(x => x.weakReference.IsAlive == false);

        }

        public static int GetCount() {

            WeakReferenceInfo.Cleanup();
            return WeakReferenceInfo.weakReferences.Count;

        }

        public static void Clear() {

			WeakReferenceInfo.weakReferences.Clear();
			WeakReferenceInfo.weakReferencesBattle.Clear();

        }

		public static List<string> GetAliveTypes() {

			WeakReferenceInfo.Cleanup();

			var list = WeakReferenceInfo.weakReferences.Select(x => {

				var typeName = x.type.FullName;
				return string.IsNullOrEmpty(x.comment) == true ? typeName : typeName + " = " + x.comment;

			}).ToList();

			return list;

		}

        public static int GetAliveTypesCount() {

            WeakReferenceInfo.Cleanup();

            return WeakReferenceInfo.weakReferences.Count;

        }


        public static List<string> GetAliveBattleTypes() {

			WeakReferenceInfo.Cleanup();

			var list = WeakReferenceInfo.weakReferencesBattle.Select(x => {

				var typeName = x.type.FullName;
				return string.IsNullOrEmpty(x.comment) == true ? typeName : typeName + " = " + x.comment;

			}).ToList();

			return list;

		}

        public static List<string> GetAlliveDBTypes() {

            WeakReferenceInfo.dbWeakReferences = WeakReferenceInfo.dbWeakReferences.Where(x => x.Value.weakReference.IsAlive == false).ToDictionary(x => x.Key, x => x.Value);

            var list = WeakReferenceInfo.dbWeakReferences.Select(x => {

                var path = x.Key;
                var val = x.Value;
                var typeName = val.type.FullName;

                return "[" + path + "]" + (string.IsNullOrEmpty(val.comment) == true ? typeName : typeName + " = " + val.comment);

            }).ToList();

            return list;

        }

        public static void RegisterDB<T>(T obj, string path) {

            if (WindowSystem.IsDebugWeakReferences() == false) return;

			var stack = new System.Diagnostics.StackTrace(fNeedFileInfo: true);
			var info = new WeakReferenceInfo(new System.WeakReference(obj), obj.GetType(), stack.ToString());
            WeakReferenceInfo.dbWeakReferences[path] = info;

        }

        public static void Register<T>(T obj) {

			if (WindowSystem.IsDebugWeakReferences() == true) {

				var stack = new System.Diagnostics.StackTrace(fNeedFileInfo: true);
				var info = new WeakReferenceInfo(new System.WeakReference(obj), obj.GetType(), stack.ToString());
				/*if (MW2.Gameplay.Battle.current != null) {

					WeakReferenceInfo.weakReferencesBattle.Add(info);

				} else {
					
					WeakReferenceInfo.weakReferences.Add(info);

				}*/
				WeakReferenceInfo.weakReferences.Add(info);

			}

        }

        public static void Save() {

            WeakReferenceInfo.Cleanup();

            WeakReferenceInfo.weakReferencesTemp = new List<WeakReferenceInfo>(WeakReferenceInfo.weakReferences);

        }

        public static Dictionary<string, string> CompareTypenames() {

            WeakReferenceInfo.Cleanup();

            var dict1 = new Dictionary<System.Type, int>();
            var dict2 = new Dictionary<System.Type, int>();

            for (var i = 0; i < WeakReferenceInfo.weakReferences.Count; ++i) {

                var type = WeakReferenceInfo.weakReferences[i].type;
                if (dict1.ContainsKey(type) == true) {

                    ++dict1[type];

                } else {

                    dict1.Add(type, 1);

                }

            }

            for (var i = 0; i < WeakReferenceInfo.weakReferencesTemp.Count; ++i) {

                var type = WeakReferenceInfo.weakReferencesTemp[i].type;
                if (dict2.ContainsKey(type) == true) {

                    ++dict2[type];

                } else {

                    dict2.Add(type, 1);

                }

            }

            var compare = new Dictionary<string, string>();
            foreach (var pair in dict2) {

                int value;
                if (dict1.TryGetValue(pair.Key, out value) == false) continue;
                var diff = value - pair.Value;
                if (diff == 0) continue;

                compare.Add(pair.Key.FullName, string.Format("{0}-{1}={2}", value, pair.Value, diff));

            }

            return compare;

        }

    }

}
