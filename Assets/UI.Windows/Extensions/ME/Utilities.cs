using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ME {

	public partial class Utilities {

		public static Vector3 GetUIPosition(Transform transform, Vector3 alignToPoint, Camera uiCamera, Camera gameCamera, Vector3 offset = default(Vector3)) {

			var position = alignToPoint + offset;
			return uiCamera.ViewportToWorldPoint(gameCamera.WorldToViewportPoint(position));

		}
		
		public static void CallInSequence<T>(System.Action callback, System.Action<T, System.Action> each, params T[] collection) {

			Utilities.CallInSequence(callback, (IEnumerable<T>)collection, each);

		}

		public static void CallInSequence<T>(System.Action callback, IEnumerable<T> collection, System.Action<T, System.Action> each) {

			var count = collection.Count();

			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < count) return;
				
				if (callback != null) callback();
				
			};

			var ie = collection.GetEnumerator();
			while (ie.MoveNext() == true) {

				if (ie.Current != null) {

					each(ie.Current, callbackItem);

				} else {

					callbackItem();

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
