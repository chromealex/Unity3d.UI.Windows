using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Extensions;
using System.Reflection;
using System.Text;
using UnityEngine.Events;
using UnityEngine.UI.Windows.Animations;

namespace UnityEngine.UI.Windows {
	
	public class WindowRoutes {

		private int index;
		private WindowBase window;

		public WindowRoutes(WindowBase window, int index) {

			this.window = window;
			this.index = index;

		}

		public int GetFunctionIterationIndex() {

			return this.index;

		}

		public void Hide() {

			this.window.Hide();

		}

		public void Hide(TransitionBase transition, TransitionInputParameters transitionParameters) {
			
			this.window.Hide(transition, transitionParameters);

		}

	}

	public class WindowSystem : MonoBehaviour {

		public class Functions {

			private Dictionary<int, List<UnityAction<int>>> items = new Dictionary<int, List<UnityAction<int>>>();
			private int iteration = 0;

			public void Register(WindowBase instance, UnityAction<int> action) {
				
				instance.SetFunctionIterationIndex(++this.iteration);

				List<UnityAction<int>> list;
				if (this.items.TryGetValue(this.iteration, out list) == true) {

					list.Add(action);

				} else {

					this.items.Add(this.iteration, new List<UnityAction<int>>() { action });

				}

			}

			public void Call(WindowBase instance) {

				var iteration = instance.GetFunctionIterationIndex();

				List<UnityAction<int>> list;
				if (this.items.TryGetValue(iteration, out list) == true) {

					foreach (var item in list) item.Invoke(iteration);
					this.items.Remove(iteration);

				}

			}

		}

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
			
			public float maxDepthLayer1 {
				
				get {
					
					return this.file != null ? this.file.baseInfo.maxDepthLayer1 : 200f;
					
				}
				
			}
			
			public float maxDepthLayer2 {
				
				get {
					
					return this.file != null ? this.file.baseInfo.maxDepthLayer2 : 300f;
					
				}
				
			}

			public int poolSize {
				
				get {
					
					return this.file != null ? this.file.baseInfo.poolSize : 100;
					
				}
				
			}
			
			public int preallocatedWindowsPoolSize {
				
				get {
					
					return this.file != null ? this.file.baseInfo.preallocatedWindowsPoolSize : 0;
					
				}
				
			}

		}
		
		[Header("Required")]
		public ObjectPool objectPool;
		
		[Header("Settings")]
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

		public Functions functions = new Functions();

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

		private bool disabledCallEvents;

		private static WindowSystem _instance;
		private static WindowSystem instance {

			get {

				#if UNITY_EDITOR
				if (WindowSystem._instance == null) {
					
					WindowSystem._instance = GameObject.FindObjectOfType<WindowSystem>();
					if (WindowSystem._instance == null) {

						return null;

					}

				}
				#endif

				return WindowSystem._instance;

			}

			set {

				WindowSystem._instance = value;

			}

		}

		public void OnValidate() {

			if (this.objectPool == null) {

				this.objectPool = Object.FindObjectOfType<ObjectPool>();

			}

		}

		private void Awake() {

			WindowSystem.instance = this;

			if (this.objectPool == null) {

				Debug.LogError("WindowSystem need ObjectPool reference", this);
				return;

			}

			this.objectPool.Init();

			GameObject.DontDestroyOnLoad(this.gameObject);

			this.Init();

		}

		private WindowBase lastInstance;
		protected virtual void LateUpdate() {

			var lastWindow = this.lastInstance;
			if (lastWindow != null) {

				lastWindow.events.LateUpdate(lastWindow);

			}

		}
		
		public static void UpdateLastInstance() {

			if (WindowSystem.instance == null) return;

			WindowSystem.instance.lastInstance = WindowSystem.GetWindow((item) => item.window != null && (item.window.GetState() == WindowObjectState.Shown || item.window.GetState() == WindowObjectState.Showing));

		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected virtual void Init() {

			foreach (var window in this.windows) {

				if (window.preferences.preallocatedCount > 0) {

					window.CreatePool(window.preferences.preallocatedCount, (source) => { return WindowSystem.instance.Create_INTERNAL(source); });

				} else {

					window.CreatePool(this.settings.preallocatedWindowsPoolSize);

				}

			}

			this.functions = new Functions();

			this.depthStep = (this.settings.maxDepth - this.settings.minDepth) / this.settings.poolSize;
			this.zDepthStep = 200f;
			WindowSystem.ResetDepth();

		}

		internal static void WaitCoroutine(IEnumerator routine) {

			WindowSystem.instance.StartCoroutine(routine);

		}
		
		public static void RegisterFunctionCallback(WindowBase instance, UnityAction<int> onFunctionEnds) {

			WindowSystem.instance.functions.Register(instance, onFunctionEnds);

		}

		public static void CallFunction(WindowBase instance) {

			WindowSystem.instance.functions.Call(instance);

		}

		public static WindowBase GetByType<T>() where T : WindowBase {

			return WindowSystem.instance.windows.FirstOrDefault((w) => w is T) as T;

		}

		public static void DisableCallEvents() {

			WindowSystem.instance.disabledCallEvents = true;

		}

		public static void RestoreCallEvents() {
			
			WindowSystem.instance.disabledCallEvents = false;

		}

		public static bool IsCallEventsEnabled() {

			return WindowSystem.instance.disabledCallEvents;

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
			
			if (WindowSystem.instance == null || WindowSystem.instance.settings.file == null) {
				
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

			if (WindowSystem.instance == null || WindowSystem.instance.settings.file == null) {
				
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

			if (Application.isPlaying == false) return;

			if (window.preferences.IsHistoryActive() == true) {

				if (WindowSystem.instance != null) WindowSystem.instance.history.Add(new HistoryItem(window));
				WindowSystem.UpdateLastInstance();

			}

		}

		/// <summary>
		/// Refreshes the history.
		/// </summary>
		public static void RefreshHistory() {

			WindowSystem.instance.history.RemoveAll((item) => item.window == null);
			WindowSystem.UpdateLastInstance();

		}

		/// <summary>
		/// Gets the current window (Basicaly the last window in history).
		/// </summary>
		/// <returns>The current window.</returns>
		public static WindowBase GetCurrentWindow() {

			var last = WindowSystem.instance.history.LastOrDefault();
			if (last == null) return null;

			return last.window;

		}

		/// <summary>
		/// Gets the last window in history with the predicate.
		/// </summary>
		/// <returns>The previous window.</returns>
		/// <param name="current">Current.</param>
		public static WindowBase GetPreviousWindow(WindowBase current, System.Func<HistoryItem, bool> predicate = null) {

			WindowBase prev = null;
			foreach (var item in WindowSystem.instance.history) {

				if (item.window == current) break;

				if (predicate == null || predicate(item) == true) {

					prev = item.window;

				}

			}

			return prev;

		}
		
		/// <summary>
		/// Gets the last window in history with the predicate.
		/// </summary>
		/// <returns>The previous window.</returns>
		/// <param name="current">Current.</param>
		public static WindowBase GetWindow(System.Func<HistoryItem, bool> predicate = null) {
			
			WindowBase last = null;
			foreach (var item in WindowSystem.instance.history) {

				if (predicate == null || predicate(item) == true) {
					
					last = item.window;
					
				}
				
			}
			
			return last;
			
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
				
				var result = WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(window, null, except);
				
				if (result == true) {
					
					if (window != null) WindowBase.DestroyImmediate(window.gameObject);
					
				}
				
				return result;

			});
			
			WindowSystem.RefreshHistory();
			
		}

		/// <summary>
		/// Clean the specified except.
		/// </summary>
		/// <param name="except">Except.</param>
		public static void Clean(List<WindowBase> except) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => {

				var result = WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(window, except, null);

				if (result == true) {

					if (window != null) WindowBase.DestroyImmediate(window.gameObject);

				}

				return result;


			});
			
			WindowSystem.RefreshHistory();
			
		}

		private bool DestroyWindowCheckOnClean_INTERNAL(WindowBase window, List<WindowBase> exceptList, WindowBase exceptItem) {
			
			if (window != null) {
				
				if (window.preferences.IsDontDestroyClean() == true) {

					return false;

				}

				if ((exceptItem == null || window != exceptItem) &&
				    (exceptList == null || exceptList.Contains(window) == false)) {

					return true;
					
				}
				
				return false;
				
			}

			return true;

		}

		/// <summary>
		/// Hides all.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAll(WindowBase except = null, System.Action callback = null) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);

			var list = WindowSystem.instance.currentWindows.Where((w) => {

				return WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(w, null, except);

			});

			ME.Utilities.CallInSequence(() => {
				
				WindowSystem.ResetDepth();
				
				if (callback != null) callback();
				
			}, list, (window, wait) => {

				if (window.Hide(wait) == false) wait.Invoke();
				
			});

		}

		/// <summary>
		/// Hides all.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAll(List<WindowBase> except, System.Action callback) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);
			
			ME.Utilities.CallInSequence(() => {
				
				WindowSystem.ResetDepth();

				if (callback != null) callback();

			}, WindowSystem.instance.currentWindows.Where((w) => {

				return WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(w, except, null);

			}), (window, wait) => {
				
				if (window.Hide(wait) == false) wait.Invoke();
				
			});

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
			instance.Init(source, WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth), WindowSystem.instance.GetNextZDepth(), WindowSystem.instance.GetNextRaycastPriority(), WindowSystem.instance.GetNextOrderInLayer());

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

			return WindowSystem.Show<T>(null, null, null, null, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(T source, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.Show<T>(null, null, null, source, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="onCreatePredicate">On create predicate.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(System.Action<T> onCreatePredicate, params object[] parameters) where T : WindowBase {

			return WindowSystem.Show<T>(onCreatePredicate, null, null, null, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type with specific transition.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(TransitionBase transition, TransitionInputParameters transitionParameters, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.Show<T>(null, transition, transitionParameters, null, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(TransitionBase transition, TransitionInputParameters transitionParameters, T source, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.Show<T>(null, transition, transitionParameters, source, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="onCreatePredicate">On create predicate.</param>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(System.Action<T> onCreatePredicate, TransitionBase transition, TransitionInputParameters transitionParameters, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.Show<T>(onCreatePredicate, transition, transitionParameters, null, parameters);

		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="onCreatePredicate">On create predicate.</param>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(System.Action<T> onCreatePredicate, TransitionBase transition, TransitionInputParameters transitionParameters, T source, params object[] parameters) where T : WindowBase {
			
			var instance = (source != null) ? WindowSystem.instance.Create_INTERNAL(source, parameters) as T : WindowSystem.Create<T>(parameters);
			if (instance != null) {
				
				if (onCreatePredicate != null) {
					
					onCreatePredicate(instance);
					
				}
				
				if (transition != null && transitionParameters != null) {
					
					instance.Show(transition, transitionParameters);
					
				} else {
					
					instance.Show();
					
				}
				
			}
			
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
				
			} else if (preferences.depth == Preferences.Depth.AlwaysTopLayer1) {
				
				depth = this.settings.maxDepthLayer1;
				
			} else if (preferences.depth == Preferences.Depth.AlwaysTopLayer2) {
				
				depth = this.settings.maxDepthLayer2;
				
			} else {

				this.currentDepth += this.depthStep;
				depth = this.currentDepth;

			}
			
			return depth;

		}

		public static void CollectCallVariations(WindowBase screen, System.Action<System.Type[], string[]> onEveryVariation) {
			
			if (screen != null) {
				
				var methods = screen.GetType().GetMethods().Where((info) => info.IsVirtual == false && info.Name == "OnParametersPass").ToList();
				for (int i = 0; i < methods.Count; ++i) {
					
					var method = methods[i];
					var parameters = method.GetParameters();
					
					var listTypes = new List<System.Type>();
					var listNames = new List<string>();
					foreach (var p in parameters) {
						
						listTypes.Add(p.ParameterType);
						listNames.Add(p.Name);
						
					}
					
					onEveryVariation(listTypes.ToArray(), listNames.ToArray());
					
				}
				
			}
			
		}

		private Dictionary<string, MethodInfo> methodsCache = new Dictionary<string, MethodInfo>();
		internal static bool InvokeMethodWithParameters(out MethodInfo methodInfo, WindowBase window, string methodName, params object[] inputParameters) {

			var instance = WindowSystem.instance;
			
			const string comma = ",";
			var key = new StringBuilder();
			
			foreach (var input in inputParameters) {
				
				key.Append(input == null ?  null : input.GetType().Name);
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

							var equal = par == null || par.GetType() == parameter.ParameterType || par.GetType().IsSubclassOf(parameter.ParameterType);
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

		public static Vector3 ConvertPoint(Vector3 point, WindowBase from, WindowBase to, bool withoutCamera = false) {

			var fromCamera = from.workCamera;
			var toCamera = to.workCamera;

			var fromLayout = from as UnityEngine.UI.Windows.Types.LayoutWindowType;
			var toLayout = to as UnityEngine.UI.Windows.Types.LayoutWindowType;

			if (fromLayout != null && toLayout != null) {

				var scaleFrom = fromLayout.layout.GetLayoutInstance().transform.localScale.x;
				var scaleTo = toLayout.layout.GetLayoutInstance().transform.localScale.x;

				var scaleK = 1f;
				if (withoutCamera == true) {

					scaleK = scaleTo / scaleFrom;

				} else {

					var orthoK = toCamera.orthographicSize / fromCamera.orthographicSize;
					scaleK = orthoK / (scaleTo / scaleFrom);

				}

				return new Vector3(point.x * scaleK, point.y * scaleK, point.z * scaleK);

			}

			return point;

		}

		public static Vector2 ConvertPoint(Vector2 point, WindowBase from, WindowBase to, bool withoutCamera = false) {
			
			var p = WindowSystem.ConvertPoint(new Vector3(point.x, point.y, 0f), from, to, withoutCamera);
			return new Vector2(p.x, p.y);

		}

	}

}