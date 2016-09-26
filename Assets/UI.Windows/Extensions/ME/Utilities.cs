using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Events;

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

	#region POOL
	public static class ListPool<T> {

		private static ObjectPoolInternal<List<T>> listPool = new ObjectPoolInternal<List<T>>(null, l => l.Clear());

		public static void Allocate(int capacity) {

			ListPool<T>.listPool = new ObjectPoolInternal<List<T>>(null, l => l.Clear(), capacity);

		}

		public static List<T> Get() {

			return ListPool<T>.listPool.Get();

		}

		public static void Release(List<T> toRelease) {

			ListPool<T>.listPool.Release(toRelease);

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
		private readonly UnityAction<T> actionOnGet;
		private readonly UnityAction<T> actionOnRelease;

		public int countAll { get; private set; }
		public int countActive { get { return this.countAll - this.countInactive; } }
		public int countInactive { get { return this.stack.Count; } }

		public ObjectPoolInternal(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease, int capacity = 1) {
			
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
			return result;

		}

		public static void PreserveAspect(RawImage image) {

			var texture = image.texture;
			if (texture != null) {

				var width = texture.width;
				var height = texture.height;

				var size = new Vector2(width, height);
				if (size.sqrMagnitude > 0f) {

					//var r = image.GetPixelAdjustedRect();

					/*
					var spriteRatio = size.x / size.y;
					var rectRatio = r.width / r.height;

					if (spriteRatio > rectRatio) {
						
						var oldHeight = r.height;
						r.height = r.width * (1f / spriteRatio);
						r.y += (oldHeight - r.height) * image.rectTransform.pivot.y;

					} else {
						
						var oldWidth = r.width;
						r.width = r.height * spriteRatio;
						r.x += (oldWidth - r.width) * image.rectTransform.pivot.x;

					}

					r.width /= size.x;
					r.x /= size.x;
					r.height /= size.y;
					r.y /= size.y;

					image.uvRect = r;
*/
				}

			}

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

		public static T FindReferenceChildren<T>(GameObject root) {

			var result = default(T);

			if (root != null) {

				var list = ListPool<T>.Get();
				root.GetComponentsInChildren<T>(true, list);
				if (list.Count > 0) {

					result = list[0];

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

}
