using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Extensions;
using System.Reflection;
using System.Text;

namespace UnityEngine.UI.Windows {

	public class WindowSystem : MonoBehaviour {

		[System.Serializable]
		public class HistoryItem {

			public WindowBase window;
			public WindowObjectState state;

			public HistoryItem(WindowBase window) {

				this.state = window.GetState();
				this.window = window;

			}

		}

		[System.Serializable]
		public class Settings {

			public WindowSystemSettings file;

			public float minZDepth {
				
				get {
					
					return this.file != null ? this.file.baseInfo.minZDepth : 0f;
					
				}

			}

			public float minDepth {

				get {

					return this.file != null ? this.file.baseInfo.minDepth : 90f;

				}

			}

			public float maxDepth {
				
				get {
					
					return this.file != null ? this.file.baseInfo.maxDepth : 98f;
					
				}
				
			}
			public int poolSize {
				
				get {
					
					return this.file != null ? this.file.baseInfo.poolSize : 100;
					
				}
				
			}

		}

		public Settings settings = new Settings();

		/// <summary>
		/// Default windows list.
		/// Use WindowSystem.ShowDefault() to show them.
		/// </summary>
		public List<WindowBase> defaults = new List<WindowBase>();
		
		/// <summary>
		/// All registered windows.
		/// If you want to use WindowSystem.Show<T>() you must register window here.
		/// </summary>
		public List<WindowBase> windows = new List<WindowBase>();
		
		[HideInInspector]
		public List<WindowBase> currentWindows = new List<WindowBase>();
		
		//[HideInInspector]
		/// <summary>
		/// The history of windows.
		/// </summary>
		public List<HistoryItem> history = new List<HistoryItem>();

		[HideInInspector]
		private float depthStep;
		[HideInInspector]
		private float currentDepth;
		[HideInInspector]
		private int currentOrderInLayer;
		[HideInInspector]
		private int currentRaycastPriority;
		
		[HideInInspector]
		private float zDepthStep;
		[HideInInspector]
		private float currentZDepth;

		private static WindowSystem _instance;
		private static WindowSystem instance {

			get {

				#if UNITY_EDITOR
				if (WindowSystem._instance == null) {

					WindowSystem._instance = GameObject.FindObjectOfType<WindowSystem>();
					if (WindowSystem._instance == null) {

						var go = new GameObject("WindowSystem");
						WindowSystem._instance = go.AddComponent<WindowSystem>();

					}

				}
				#endif

				return WindowSystem._instance;

			}

			set {

				WindowSystem._instance = value;

			}

		}

		private void Awake() {

			WindowSystem.instance = this;

			GameObject.DontDestroyOnLoad(this.gameObject);

			this.Init();

		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected virtual void Init() {

			foreach (var window in this.windows) window.CreatePool(0);
			
			this.depthStep = (this.settings.maxDepth - this.settings.minDepth) / this.settings.poolSize;
			this.zDepthStep = 200f;
			WindowSystem.ResetDepth();

		}

		internal static void WaitCoroutine(IEnumerator routine) {

			WindowSystem.instance.StartCoroutine(routine);

		}

		/// <summary>
		/// Determines if is user interface hovered.
		/// </summary>
		/// <returns><c>true</c> if is user interface hovered; otherwise, <c>false</c>.</returns>
		public static bool IsUIHovered() {

			return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

		}

		/// <summary>
		/// Applies settings file (or default) to camera.
		/// </summary>
		/// <param name="camera">Camera.</param>
		public static void ApplyToSettings(Camera camera) {
			
			if (WindowSystem.instance.settings.file == null) {
				
				camera.orthographic = true;
				camera.orthographicSize = 5f;
				camera.nearClipPlane = -100f;
				camera.farClipPlane = 100f;
				camera.useOcclusionCulling = false;
				camera.hdr = false;
				
			} else {
				
				WindowSystem.instance.settings.file.Apply(camera: camera);
				
			}
			
		}

		/// <summary>
		/// Applies settings file (or default) to canvas.
		/// </summary>
		/// <param name="canvas">Canvas.</param>
		public static void ApplyToSettings(Canvas canvas) {

			if (WindowSystem.instance.settings.file == null) {
				
				canvas.overrideSorting = true;
				canvas.sortingLayerName = "Windows";
				canvas.sortingOrder = 0;

			} else {
				
				WindowSystem.instance.settings.file.Apply(canvas: canvas);
				
			}
			
		}

		/// <summary>
		/// Adds to history.
		/// </summary>
		/// <param name="window">Window.</param>
		public static void AddToHistory(WindowBase window) {

			WindowSystem.instance.history.Add(new HistoryItem(window));

		}

		/// <summary>
		/// Refreshes the history.
		/// </summary>
		public static void RefreshHistory() {

			WindowSystem.instance.history.RemoveAll((item) => item.window == null);

		}

		/// <summary>
		/// Gets the last window in history with the `Shown` state.
		/// </summary>
		/// <returns>The previous window.</returns>
		/// <param name="current">Current.</param>
		public static WindowBase GetPreviousWindow(WindowBase current) {

			WindowBase prev = null;
			foreach (var item in WindowSystem.instance.history) {

				if (item.window == current) break;

				if (item.window.GetState() == WindowObjectState.Shown) {

					prev = item.window;

				}

			}

			return prev;

		}

		/// <summary>
		/// Shows the default list windows.
		/// </summary>
		/// <param name="parameters">Parameters.</param>
		public static void ShowDefault(params object[] parameters) {

			WindowSystem.instance.ShowDefault_INTERNAL(parameters);

		}

		internal void ShowDefault_INTERNAL(params object[] parameters) {

			foreach (var window in this.defaults) {

				var instance = this.GetInstance(window, parameters);

				instance.SetParameters(parameters);
				instance.Init(WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth), WindowSystem.instance.GetNextZDepth(), WindowSystem.instance.GetNextRaycastPriority(), WindowSystem.instance.GetNextOrderInLayer());
				
				instance.Show();

			}

		}

		private WindowBase GetInstance(WindowBase window, params object[] parameters) {

			var instance = this.currentWindows.FirstOrDefault((w) => w != null && w.GetType().IsInstanceOfType(window));
			if (instance == null) {

				instance = this.Create_INTERNAL(window, parameters);

			}

			return instance;

		}

		internal static void ResetDepth() {

			WindowSystem.instance.currentDepth = WindowSystem.instance.settings.minDepth;
			WindowSystem.instance.currentZDepth = WindowSystem.instance.settings.minZDepth;

		}

		/// <summary>
		/// Hides all and clean.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAllAndClean(WindowBase except = null, System.Action callback = null) {
			
			WindowSystem.HideAll(except, () => {
				
				WindowSystem.Clean(except);
				
				if (callback != null) callback();
				
			});
			
		}

		/// <summary>
		/// Hides all and clean.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAllAndClean(List<WindowBase> except, System.Action callback = null) {
			
			WindowSystem.HideAll(except, () => {
				
				WindowSystem.Clean(except);
				
				if (callback != null) callback();
				
			});
			
		}

		/// <summary>
		/// Clean the specified except.
		/// </summary>
		/// <param name="except">Except.</param>
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
			
			WindowSystem.RefreshHistory();
			
		}

		/// <summary>
		/// Clean the specified except.
		/// </summary>
		/// <param name="except">Except.</param>
		public static void Clean(List<WindowBase> except) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => {
				
				if (window != null) {
					
					if (except == null || !except.Contains(window)) {
						
						WindowBase.DestroyImmediate(window.gameObject);
						return true;
						
					}
					
					return false;
					
				}
				
				return true;
				
			});
			
			WindowSystem.RefreshHistory();
			
		}

		/// <summary>
		/// Hides all.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAll(WindowBase except = null, System.Action callback = null) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);
			
			ME.Utilities.CallInSequence(callback, WindowSystem.instance.currentWindows.Where((w) => except == null || w != except), (window, wait) => {
				
				if (window.Hide(wait) == false) wait.Invoke();
				
			});
			
			WindowSystem.ResetDepth();
			
		}

		/// <summary>
		/// Hides all.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAll(List<WindowBase> except, System.Action callback) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);
			
			ME.Utilities.CallInSequence(callback, WindowSystem.instance.currentWindows.Where((w) => except == null || !except.Contains(w)), (window, wait) => {
				
				if (window.Hide(wait) == false) wait.Invoke();
				
			});
			
			WindowSystem.ResetDepth();
			
		}

		/// <summary>
		/// Create the specified parameters.
		/// </summary>
		/// <param name="parameters">Parameters.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
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

			#if UNITY_EDITOR
			instance.gameObject.name = "Screen-" + source.GetType().Name;
			#endif

			instance.SetParameters(parameters);
			instance.Init(WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth), WindowSystem.instance.GetNextZDepth(), WindowSystem.instance.GetNextRaycastPriority(), WindowSystem.instance.GetNextOrderInLayer());

			if (WindowSystem.instance.currentWindows.Contains(instance) == false) WindowSystem.instance.currentWindows.Add(instance);

			return instance;
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(params object[] parameters) where T : WindowBase {

			var instance = WindowSystem.Create<T>(parameters);
			if (instance != null) instance.Show();

			return instance;
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(T source, params object[] parameters) where T : WindowBase {

			var instance = WindowSystem.instance.Create_INTERNAL(source, parameters) as T;
			if (instance != null) instance.Show();

			return instance;

		}

		private int GetNextOrderInLayer() {
			
			return ++this.currentOrderInLayer;

		}
		
		private int GetNextRaycastPriority() {

			return ++this.currentRaycastPriority;

		}
		
		private float GetNextZDepth() {

			this.currentZDepth += this.zDepthStep;
			return this.currentZDepth;
			
		}

		/// <summary>
		/// Gets the depth step.
		/// </summary>
		/// <returns>The depth step.</returns>
		public static float GetDepthStep() {

			return WindowSystem.instance.depthStep;

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
		
		private Dictionary<string, MethodInfo> methodsCache = new Dictionary<string, MethodInfo>();
		internal static bool InvokeMethodWithParameters(out MethodInfo methodInfo, WindowBase window, string methodName, params object[] inputParameters) {

			var instance = WindowSystem.instance;
			
			const string comma = ",";
			var key = new StringBuilder();
			
			foreach (var input in inputParameters) {
				
				key.Append(input.GetType().Name);
				key.Append(comma);
				
			}
			
			key.Append(methodName);
			key.Append(comma);
			key.Append(window.GetType().Name);
			
			var keyStr = key.ToString();

			if (instance.methodsCache.TryGetValue(keyStr, out methodInfo) == false) {
				
				instance.methodsCache.Clear();
				
				var methods = window.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
				foreach (var method in methods) {
					
					if (method.Name == methodName) {
						
						var count = 0;
						var parameters = method.GetParameters();
						
						if (inputParameters.Length != parameters.Length) continue;
						
						for (int i = 0; i < parameters.Length; ++i) {
							
							var parameter = parameters[i];
							var par = inputParameters[i];
							
							var equal = (parameter.ParameterType == par.GetType());
							if (equal == true) {
								
								++count;
								
							}
							
						}
						
						if (count == parameters.Length) {
							
							// Invoke and break
							methodInfo = method;
							
							instance.methodsCache.Add(keyStr, methodInfo);
							
							return true;
							
						}
						
					}
					
				}
				
			} else {
				
				return true;
				
			}
			
			return false;
			
		}

	}

}