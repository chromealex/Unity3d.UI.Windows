using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace ME {

	public class Math {
		
		public static long Lerp(long a, long b, float t) {
			
			return a + (long)((b - a) * Mathf.Clamp01(t));
			
		}
		
		public static int Lerp(int a, int b, float t) {
			
			return a + (int)((b - a) * Mathf.Clamp01(t));
			
		}

	}

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

			if (root != null) {

				var items = root.GetComponentsInParent<T>(true);
				if (items.Length > 0) return (T)items[0];

			}

			return default(T);

		}

		public static T FindReferenceParent<T>(Component root) {

			if (root != null) {
				
				var items = root.GetComponentsInParent<T>(true);
				if (items.Length > 0) return (T)items[0];

			}

            return default(T);

        }

        public static void FindReferenceParent<T>(Component root, ref T link) where T : Component {
			
			var items = root.GetComponentsInParent<T>(true);
			if (items.Length > 0) link = items[0] as T;
			
		}

		public static void FindReference<T>(Component root, ref T link) where T : Component {
			
			var items = root.GetComponentsInChildren<T>(true);
			if (items.Length == 1) link = items[0] as T;
			
		}

		public static Vector3 GetUIPosition(Transform transform, Vector3 alignToPoint, Camera uiCamera, Camera gameCamera, Vector3 offset = default(Vector3)) {

			var position = alignToPoint + offset;
			return uiCamera.ViewportToWorldPoint(gameCamera.WorldToViewportPoint(position));

		}
		
		public static void CallInSequence<T>(System.Action callback, System.Action<T, System.Action> each, params T[] collection) {

			Utilities.CallInSequence(callback, (IEnumerable<T>)collection, each);

		}

		public static void CallInSequence<T>(System.Action callback, IEnumerable<T> collection, System.Action<T, System.Action> each, bool waitPrevious = false) {

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
