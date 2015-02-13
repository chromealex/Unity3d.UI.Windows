#if UNITY_IPHONE || UNITY_ANDROID
#define UNITY_MOBILE
#endif

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class WindowSystem : MonoBehaviour {

		[System.Serializable]
		public class Settings {
			
			public float minDepth = 90f;
			public float maxDepth = 98f;
			public int poolSize = 100;

		}

		public Settings settings;

		public List<WindowBase> defaults = new List<WindowBase>();
		#if UNITY_EDITOR || UNITY_MOBILE
		public List<WindowBase> defaultsMobileOnly = new List<WindowBase>();
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		public List<WindowBase> defaultsStandaloneOnly = new List<WindowBase>();
		#endif
		
		public List<WindowBase> windows = new List<WindowBase>();
		#if UNITY_EDITOR || UNITY_MOBILE
		public List<WindowBase> mobileOnly = new List<WindowBase>();
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		public List<WindowBase> standaloneOnly = new List<WindowBase>();
		#endif
		public List<WindowBase> currentWindows = new List<WindowBase>();

		private float depthStep;
		private float currentDepth;
		private int currentOrderInLayer;
		private int currentRaycastPriority;

		private static WindowSystem instance;

		private void Awake() {

			WindowSystem.instance = this;
			
			this.Init();

		}

		private void Init() {

			#if UNITY_MOBILE
			this.defaults.AddRange(this.defaultsMobileOnly);
			#endif
			#if UNITY_STANDALONE
			this.defaults.AddRange(this.defaultsStandaloneOnly);
			#endif
			
			#if UNITY_MOBILE
			this.windows.AddRange(this.mobileOnly);
			#endif
			#if UNITY_STANDALONE
			this.windows.AddRange(this.standaloneOnly);
			#endif
			
			foreach (var window in this.windows) window.CreatePool(0);
			
			this.depthStep = (this.settings.maxDepth - this.settings.minDepth) / this.settings.poolSize;
			WindowSystem.ResetDepth();

		}

		public static void ShowDefault(params object[] parameters) {

			WindowSystem.instance.ShowDefault_INTERNAL(parameters);

		}

		internal void ShowDefault_INTERNAL(params object[] parameters) {

			foreach (var window in this.defaults) {

				var instance = this.GetInstance(window, parameters);

				instance.SetParameters(parameters);
				instance.Init(WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth), WindowSystem.instance.GetNextRaycastPriority(), WindowSystem.instance.GetNextOrderInLayer());
				
				instance.Show();

			}

		}

		private WindowBase GetInstance(WindowBase window, params object[] parameters) {

			var instance = this.currentWindows.FirstOrDefault((w) => w.GetType().IsInstanceOfType(window));
			if (instance == null) {

				instance = this.Create_INTERNAL(window, parameters);

			}

			return instance;

		}

		internal static void ResetDepth() {

			WindowSystem.instance.currentDepth = WindowSystem.instance.settings.minDepth;

		}

		public static void HideAllAndClean(WindowBase except = null) {

			WindowSystem.HideAll(except, () => {

				WindowSystem.Clean(except);

			});

		}

		public static void Clean(WindowBase except = null) {

			WindowSystem.instance.currentWindows.RemoveAll((window) => {

				if (window != null) {

					if (except == null || window != except) {

						WindowBase.DestroyImmediate(window.gameObject);
						return true;

					}

					return false;

				}

				return true;
				
			});

		}

		public static void HideAll(WindowBase except = null, System.Action callback = null) {

			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);

			var imax = WindowSystem.instance.currentWindows.Count;
			if (imax > 0) {

				WindowBase maxExcept = null;
				var maxDuration = 0f;
				foreach (var window in WindowSystem.instance.currentWindows) {

					if (except == window) continue;

					var d = window.GetAnimationDuration(false);
					if (d >= maxDuration) {

						maxDuration = d;
						maxExcept = window;

					}

				}

				for (int i = 0; i < imax; ++i) {

					if ((except == null || WindowSystem.instance.currentWindows[i] != except) &&
					    (maxExcept == null || WindowSystem.instance.currentWindows[i] != maxExcept)) {

						WindowSystem.instance.currentWindows[i].Hide();

					}

				}

				if (maxExcept != null) {

					maxExcept.Hide(callback);

				} else {

					if (callback != null) callback();

				}

			} else {

				if (callback != null) callback();

			}

			WindowSystem.ResetDepth();

		}

		public static T Create<T>(params object[] parameters) where T : WindowBase {
			
			var source = WindowSystem.instance.windows.FirstOrDefault((w) => w is T) as T;
			if (source == null) return null;

			return WindowSystem.instance.Create_INTERNAL(source, parameters) as T;

		}

		internal WindowBase Create_INTERNAL(WindowBase source, params object[] parameters) {

			var instance = source.Spawn();
			instance.transform.SetParent(null);
			instance.transform.localPosition = Vector3.zero;
			instance.transform.localRotation = Quaternion.identity;
			instance.transform.localScale = Vector3.one;
			
			instance.SetParameters(parameters);
			instance.Init(WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth), WindowSystem.instance.GetNextRaycastPriority(), WindowSystem.instance.GetNextOrderInLayer());

			if (WindowSystem.instance.currentWindows.Contains(instance) == false) WindowSystem.instance.currentWindows.Add(instance);

			return instance;
			
		}
		
		public static T Show<T>(params object[] parameters) where T : WindowBase {

			var instance = WindowSystem.Create<T>(parameters);
			if (instance != null) instance.Show();

			return instance;
			
		}
		
		private int GetNextOrderInLayer() {
			
			return ++this.currentOrderInLayer;

		}
		
		private int GetNextRaycastPriority() {

			return ++this.currentRaycastPriority;

		}
		
		private float GetNextDepth(Preferences preferences, float windowDepth) {

			var depth = 0f;

			if (preferences.depth == Preferences.Depth.AlwaysBack) {
				
				depth = windowDepth;
				
			} else if (preferences.depth == Preferences.Depth.AlwaysTop) {

				depth = this.settings.maxDepth;

			} else {

				this.currentDepth += this.depthStep;
				depth = this.currentDepth;

			}
			
			return depth;

		}

	}

}