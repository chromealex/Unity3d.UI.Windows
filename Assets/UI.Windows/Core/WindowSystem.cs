using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Extensions;
using System.Reflection;
using System.Text;
using UnityEngine.Events;
using UnityEngine.UI.Windows.Animations;
using UnityEngine.UI.Windows.Audio;

namespace UnityEngine.UI.Windows {

	public interface IFunctionIteration {

		WindowBase GetWindow();
		int GetFunctionIterationIndex();
		bool Hide();
		bool Hide(AttachItem transitionItem);
		
		//[System.Obsolete("Use `Tools->Compile UI` command to fix this issue.")]
		bool Hide(TransitionBase transition, TransitionInputParameters transitionParams);

	}
	
	[System.Serializable]
	public class AttachItem {
		
		public static readonly AttachItem Empty = new AttachItem(-1, 0);
		
		public int targetId;
		public int index = 0;
		
		public TransitionBase transition;
		public TransitionInputParameters transitionParameters;
		
		public TransitionBase audioTransition;
		public TransitionInputParameters audioTransitionParameters;
		
		#if UNITY_EDITOR
		[HideInInspector]
		public IPreviewEditor editor;
		[HideInInspector]
		public IPreviewEditor editorAudio;
		#endif
		
		public AttachItem(int targetId, int index) {
			
			this.targetId = targetId;
			this.index = index;
			
		}
		
	}

	public class WindowRoutes : IFunctionIteration {

		private int index;
		private WindowBase sourceWindow;

		public WindowRoutes(IFunctionIteration sourceWindow, int index) {

			this.sourceWindow = sourceWindow.GetWindow();
			this.index = index;

		}

		public WindowBase GetWindow() {

			return this.sourceWindow;

		}

		public int GetFunctionIterationIndex() {

			return this.index;

		}

		public bool Hide() {

			return this.sourceWindow.Hide();

		}

		public bool Hide(AttachItem transitionItem) {
			
			return this.sourceWindow.Hide(transitionItem);

		}
		
		[System.Obsolete("Use `Tools->Compile UI` command to fix this issue.")]
		public bool Hide(TransitionBase transition, TransitionInputParameters transitionParams) {

			return this.Hide();

		}

	}

	public class WindowSystem : MonoBehaviour {
		
		public class OnDoTransitionEvent : UnityEvent<int, int, int, bool> {}
		public static OnDoTransitionEvent onTransition = new OnDoTransitionEvent();

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
			
			public HistoryItem(WindowBase window) : this(window, window.GetState()) {}
			
			public HistoryItem(WindowBase window, WindowObjectState state) {
				
				this.state = state;
				this.window = window;
				
			}

		}

		[System.Serializable]
		public class Settings {

			public WindowSystemSettings file;

			public int poolSize {
				
				get {
					
					return this.file != null ? this.file.baseInfo.poolSize : 100;
					
				}
				
			}

			public float zDepthStep {

				get {

					return this.file != null ? this.file.baseInfo.zDepthStep : 200f;

				}

			}
			
			public int preallocatedWindowsPoolSize {
				
				get {
					
					return this.file != null ? this.file.baseInfo.preallocatedWindowsPoolSize : 0;
					
				}
				
			}
			
			public float GetMinDepth(int layer) {
				
				return this.file != null ? this.file.baseInfo.GetMinDepth(layer) : 90f;
				
			}
			
			public float GetMaxDepth(int layer) {
				
				return this.file != null ? this.file.baseInfo.GetMaxDepth(layer) : 98f;
				
			}
			
			public float GetMinZDepth(int layer) {
				
				return this.file != null ? this.file.baseInfo.GetMinZDepth(layer) : 0f;
				
			}

		}
		
		[Header("Required")]
		public ObjectPool objectPool;
		
		[Header("Settings")]
		public Settings settings = new Settings();
		
		[Header("Window System")]
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
		
		[System.NonSerialized][HideInInspector]
		public List<WindowBase> currentWindows = new List<WindowBase>();
		
		//[HideInInspector]
		/// <summary>
		/// The history of windows.
		/// </summary>
		public List<HistoryItem> history = new List<HistoryItem>();

		public Functions functions = new Functions();
		new public Audio.Source audio = new Audio.Source();

		[HideInInspector]
		private float depthStep;
		[HideInInspector]
		private int currentOrderInLayer;
		[HideInInspector]
		private int currentRaycastPriority;
		
		private Dictionary<int, float> currentDepth = new Dictionary<int, float>();
		private Dictionary<int, float> currentZDepth = new Dictionary<int, float>();

		[HideInInspector]
		private float zDepthStep;

		private bool disabledCallEvents = false;

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

				Debug.LogError("WindowSystem need `ObjectPool` reference", this);
				return;

			}

			this.objectPool.Init();

			GameObject.DontDestroyOnLoad(this.gameObject);

			this.Init();

		}
		
		private WindowBase lastInstance;
		private WindowBase previousInstance;
		protected virtual void LateUpdate() {

			var lastWindow = this.lastInstance;
			if (lastWindow != null) {

				lastWindow.events.LateUpdate(lastWindow);

			}

		}

		public static void OnDoTransition(int index, int fromScreenId, int toScreenId, bool hide = true) {
			
			WindowSystem.onTransition.Invoke(index, fromScreenId, toScreenId, hide);
			
		}

		public static void DestroyWindow(WindowBase window) {

			if (WindowSystem.instance == null) return;

			WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(window, null, null);

		}

		public static void ForEachWindow(System.Action<WindowBase> onEach) {

			foreach (var window in WindowSystem.instance.currentWindows) {

				onEach.Invoke(window);

			}

		}

		public static void AudioPlayFX(int id, int[] randomIds, bool randomize) {
			
			if (randomize == true) {
				
				var index = Random.Range(0, randomIds.Length);
				if (index >= 0 && index < randomIds.Length) {
					
					WindowSystem.AudioPlay(null, ClipType.SFX, randomIds[index]);
					
				}
				
			} else {
				
				if (id > 0) {
					
					WindowSystem.AudioPlay(null, ClipType.SFX, id);
					
				}
				
			}
			
		}
		
		public static float GetVolume(ClipType clipType) {
			
			if (WindowSystem.instance == null) return 0f;
			
			return WindowSystem.instance.audio.GetVolume(clipType);
			
		}

		public static void SetVolume(ClipType clipType, float value) {
			
			if (WindowSystem.instance == null) return;

			WindowSystem.instance.audio.SetVolume(clipType, value);

		}

		public static void AudioStop(WindowBase window, ClipType clipType, int id) {

			if (WindowSystem.instance == null) return;

			//Debug.Log("STOP: " + id);
			Audio.Manager.Stop(window, WindowSystem.instance.audio, clipType, id);

		}
		
		public static void AudioPlay(WindowBase window, ClipType clipType, int id, bool replaceOnEquals = false) {
			
			if (WindowSystem.instance == null) return;
			
			//Debug.Log("PLAY: " + id + ", " + replaceOnEquals);
			Audio.Manager.Play(window, WindowSystem.instance.audio, clipType, id, replaceOnEquals);
			
		}

		public static void AudioChange(WindowBase window, ClipType clipType, int id, Audio.Window audioSettings) {
			
			if (WindowSystem.instance == null) return;

			//Debug.Log("CHANGE: " + id + " :: " + audioSettings.volume);
			Audio.Manager.Change(window, WindowSystem.instance.audio, clipType, id, audioSettings);
			
		}

		public static void UpdateLastInstance() {

			if (WindowSystem.instance == null) return;

			var lastInstance = WindowSystem.GetWindow((item) => item.window != null && (item.window.GetState() == WindowObjectState.Shown || item.window.GetState() == WindowObjectState.Showing));
			if (lastInstance != WindowSystem.instance.lastInstance) {

				WindowSystem.instance.previousInstance = WindowSystem.instance.lastInstance;
				if (WindowSystem.instance.previousInstance != null) {

					WindowSystem.instance.previousInstance.SetInactive();

				}

			}

			WindowSystem.instance.lastInstance = lastInstance;

			if (WindowSystem.instance.lastInstance != null) {
				
				WindowSystem.instance.lastInstance.SetActive();
				
			}

		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected virtual void Init() {

			this.currentWindows.Clear();
			this.functions = new Functions();
			this.audio.Init();

			this.depthStep = (this.settings.GetMaxDepth(0) - this.settings.GetMinDepth(0)) / this.settings.poolSize;
			this.zDepthStep = this.settings.zDepthStep;
			WindowSystem.ResetDepth();

			foreach (var window in this.windows) {

				if (window == null) {

					Debug.LogWarning("Some windows was not setup correctly. Be sure all windows have their screens.");
					continue;

				}

				if (window.preferences.createPool == false) continue;

				if (window.preferences.preallocatedCount > 0) {
					
					window.CreatePool(window.preferences.preallocatedCount, (source) => { return WindowSystem.instance.Create_INTERNAL(source, null); });
					
				} else {
					
					window.CreatePool(this.settings.preallocatedWindowsPoolSize);
					
				}
				
			}

		}

		internal static void WaitCoroutine(IEnumerator routine) {

			WindowSystem.instance.StartCoroutine(routine);

		}
		
		public static void RegisterFunctionCallback(WindowBase instance, UnityAction<int> onFunctionEnds) {

			WindowSystem.instance.functions.Register(instance, onFunctionEnds);

		}
		
		public static void CallFunction(WindowRoutes routes) {

			throw new UnityException("Routes can't call a function");

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

			return WindowSystem.instance.disabledCallEvents == false;

		}

		/// <summary>
		/// Determines if is user interface hovered.
		/// </summary>
		/// <returns><c>true</c> if is user interface hovered; otherwise, <c>false</c>.</returns>
		public static bool IsUIHovered() {

			return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

		}
		
		public static string GetSortingLayerName() {
			
			if (WindowSystem.instance == null || WindowSystem.instance.settings.file == null) {
				
				// no settings file
				
			} else {
				
				return WindowSystem.instance.settings.file.canvas.sortingLayerName;
				
			}
				
			return string.Empty;
				
		}

		public static void ApplyToSettingsInstance(Camera camera, Canvas canvas) {

			if (WindowSystem.instance == null || WindowSystem.instance.settings.file == null) {

				// no settings file

			} else {

				WindowSystem.instance.settings.file.ApplyEveryInstance(camera, canvas);

			}

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

			WindowSystem.AddToHistory(window, window.GetState());

		}

		public static void AddToHistory(WindowBase window, WindowObjectState state, bool forced = false) {
			
			if (Application.isPlaying == false) return;

			if (forced == true || window.preferences.IsHistoryActive() == true) {
				
				if (WindowSystem.instance != null) WindowSystem.instance.history.Add(new HistoryItem(window, state));
				WindowSystem.UpdateLastInstance();
				
			}
			
		}

		public static void RemoveFromHistory(WindowBase window) {

			WindowSystem.instance.history.RemoveAll((item) => item.window == window);
			WindowSystem.RefreshHistory();

		}

		/// <summary>
		/// Refreshes the history.
		/// </summary>
		public static void RefreshHistory() {

			WindowSystem.instance.history.RemoveAll((item) => item.window == null);
			WindowSystem.UpdateLastInstance();

		}
		
		/// <summary>
		/// Gets the current window
		/// </summary>
		/// <returns>The current window.</returns>
		public static WindowBase GetCurrentWindow() {

			if (WindowSystem.instance == null) return null;

			return WindowSystem.instance.lastInstance;
			
		}
		
		/// <summary>
		/// Gets the previous window (Last opened)
		/// </summary>
		/// <returns>The previous window.</returns>
		public static WindowBase GetPreviousWindow() {
			
			if (WindowSystem.instance == null) return null;

			return WindowSystem.instance.previousInstance;
			
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
		public static T GetWindow<T>() where T : WindowBase {

			return WindowSystem.GetWindow((item) => item.window is T) as T;

		}

		/// <summary>
		/// Gets the last window in history with the predicate.
		/// </summary>
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
		/// Finds the opened window.
		/// </summary>
		public static T FindOpened<T>() where T : WindowBase {

			return WindowSystem.instance.currentWindows.FirstOrDefault((item) => item is T) as T;

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

				instance.SetParameters(onParametersPassCall: null, parameters: parameters);
				instance.Init(WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth),
				              WindowSystem.instance.GetNextZDepth(instance.preferences),
				              WindowSystem.instance.GetNextRaycastPriority(),
				              WindowSystem.instance.GetNextOrderInLayer()
				              );
				
				instance.Show();

			}

		}

		private WindowBase GetInstance(WindowBase window, params object[] parameters) {

			var instance = this.currentWindows.FirstOrDefault((w) => w != null && w.GetType().IsInstanceOfType(window));
			if (instance == null) {

				instance = this.Create_INTERNAL(window, onParametersPassCall: null, parameters: parameters);

			}

			return instance;

		}

		internal static void ResetDepth() {

			var items = System.Enum.GetValues(typeof(DepthLayer));
			var instance = WindowSystem.instance;

			foreach (int item in items) {
				
				instance.currentDepth[item] = instance.settings.GetMinDepth(item);
				instance.currentZDepth[item] = instance.settings.GetMinZDepth(item);

			}

		}

		/// <summary>
		/// Hides all and clean.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAllAndClean(WindowBase except = null, System.Action callback = null, bool forceAll = false) {
			
			WindowSystem.HideAll(except, () => {
				
				WindowSystem.Clean(except, forceAll);
				
				if (callback != null) callback();
				
			}, forceAll);
			
		}

		/// <summary>
		/// Hides all and clean.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAllAndClean(List<WindowBase> except, System.Action callback = null, bool forceAll = false) {
			
			WindowSystem.HideAll(except, () => {
				
				WindowSystem.Clean(except, forceAll);
				
				if (callback != null) callback();
				
			}, forceAll);
			
		}

		/// <summary>
		/// Clean the specified except.
		/// </summary>
		/// <param name="except">Except.</param>
		public static void Clean(WindowBase except = null, bool forceAll = false) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => {
				
				var result = WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(window, null, except, forceAll);
				if (result == true) {
					
					if (window != null) WindowBase.DestroyImmediate(window.gameObject);
					
				}
				
				return result;

			});
			
			WindowSystem.ResetDepth();

			if (except != null) {

				except.SetDepth(WindowSystem.instance.GetNextDepth(except.preferences, except.workCamera.depth), WindowSystem.instance.GetNextZDepth(except.preferences));
				except.OnCameraReset();

			}

			WindowSystem.RefreshHistory();
			
		}

		/// <summary>
		/// Clean the specified except.
		/// </summary>
		/// <param name="except">Except.</param>
		public static void Clean(List<WindowBase> except, bool forceAll = false) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => {

				var result = WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(window, except, null, forceAll);
				if (result == true) {

					if (window != null) WindowBase.DestroyImmediate(window.gameObject);

				}

				return result;


			});
			
			WindowSystem.ResetDepth();

			if (except != null) {

				for (int i = 0; i < except.Count; ++i) {

					var instance = except[i];
					if (instance == null) continue;
					instance.SetDepth(WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth), WindowSystem.instance.GetNextZDepth(instance.preferences));
					instance.OnCameraReset();

				}

			}

			WindowSystem.RefreshHistory();
			
		}

		private bool DestroyWindowCheckOnClean_INTERNAL(WindowBase window, List<WindowBase> exceptList, WindowBase exceptItem, bool forceAll = false) {
			
			if (window != null) {
				
				if (forceAll == false && window.preferences.IsDontDestroyOnClean() == true) {

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
		public static void HideAll(WindowBase except = null, System.Action callback = null, bool forceAll = false) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);

			var list = WindowSystem.instance.currentWindows.Where((w) => {

				return WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(w, null, except, forceAll);

			});

			ME.Utilities.CallInSequence(() => {

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
		public static void HideAll(List<WindowBase> except, System.Action callback, bool forceAll = false) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);

			ME.Utilities.CallInSequence(() => {
				
				if (callback != null) callback();

			}, WindowSystem.instance.currentWindows.Where((w) => {

				return WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(w, except, null, forceAll);

			}), (window, wait) => {
				
				if (window.Hide(wait) == false) wait.Invoke();
				
			});

		}

		/// <summary>
		/// Create the specified parameters.
		/// </summary>
		/// <param name="parameters">Parameters.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Create<T>(System.Action<WindowBase> onParametersPassCall, out bool ignoreActions, params object[] parameters) where T : WindowBase {

			ignoreActions = false;

			var source = WindowSystem.instance.windows.FirstOrDefault(w => w is T) as T;
			if (source == null) return null;

			return WindowSystem.instance.CreateWithIgnore_INTERNAL(source, onParametersPassCall, out ignoreActions, parameters) as T;

		}
		
		internal WindowBase Create_INTERNAL(WindowBase source, System.Action<WindowBase> onParametersPassCall, params object[] parameters) {

			bool ignoreActions;
			return this.CreateWithIgnore_INTERNAL(source, onParametersPassCall, out ignoreActions, parameters);

		}

		internal WindowBase CreateWithIgnore_INTERNAL(WindowBase source, System.Action<WindowBase> onParametersPassCall, out bool ignoreActions, params object[] parameters) {

			ignoreActions = false;

			WindowBase instance = null;
			if (source.preferences.forceSingleInstance == true) {

				instance = this.currentWindows.FirstOrDefault(w => w.windowId == source.windowId);
				if (instance != null) ignoreActions = source.preferences.singleInstanceIgnoreActions;

			}

			if (instance == null) {

				instance = source.Spawn();
				instance.transform.SetParent(null);
				instance.transform.localPosition = Vector3.zero;
				instance.transform.localRotation = Quaternion.identity;
				instance.transform.localScale = Vector3.one;

				#if UNITY_EDITOR
				instance.gameObject.name = string.Format("[Screen] {0}", source.GetType().Name);
				#endif

			}
			
			if (ignoreActions == false) {

				instance.SetParameters(onParametersPassCall, parameters);
				instance.Init(source,
				              WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth),
				              WindowSystem.instance.GetNextZDepth(instance.preferences),
				              WindowSystem.instance.GetNextRaycastPriority(),
				              WindowSystem.instance.GetNextOrderInLayer()
				);

			}

			if (WindowSystem.instance.currentWindows.Contains(instance) == false) {

				WindowSystem.instance.currentWindows.Add(instance);

			}

			return instance;
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, null, null, null, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {

			return WindowSystem.ShowWithParameters<T>(null, null, null, onParametersPassCall, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(T source, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, null, source, null, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(T source, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, null, source, onParametersPassCall, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="afterGetInstance">On create predicate.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(System.Action<T> afterGetInstance, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {

			return WindowSystem.ShowWithParameters<T>(afterGetInstance, null, null, onParametersPassCall, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type with specific transition.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="transition">Transition Item.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(AttachItem transitionItem, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, transitionItem, null, onParametersPassCall, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="transition">Transition Item.</param>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(AttachItem transitionItem, T source, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, transitionItem, source, onParametersPassCall, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="afterGetInstance">On create predicate.</param>
		/// <param name="transition">Transition Item.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(System.Action<T> afterGetInstance, AttachItem transitionItem, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(afterGetInstance, transitionItem, null, onParametersPassCall, parameters);

		}
		
		[System.Obsolete("Use `Tools->Compile UI` command to fix this issue.")]
		public static T Show<T>(TransitionBase transition, TransitionInputParameters transitionParams, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, null, null, onParametersPassCall, parameters);
			
		}

		[System.Obsolete("Use `Tools->Compile UI` command to fix this issue.")]
		public static T Show<T>(System.Action<T> afterGetInstance, TransitionBase transition, TransitionInputParameters transitionParams, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(afterGetInstance, null, null, onParametersPassCall, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="afterGetInstance">On create predicate.</param>
		/// <param name="transition">Transition.</param>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T ShowWithParameters<T>(System.Action<T> afterGetInstance,
		                        AttachItem transitionItem,
		                        T source,
		                        System.Action<T> onParametersPassCall,
		                        params object[] parameters) where T : WindowBase {

			System.Action<WindowBase> parametersPassCall = null;
			if (onParametersPassCall != null) {

				parametersPassCall = (WindowBase window) => {

					if (onParametersPassCall != null) {

						onParametersPassCall(window as T);

					}

				};

			}

			var ignoreActions = false;
			var instance = (source != null) ? WindowSystem.instance.CreateWithIgnore_INTERNAL(source, parametersPassCall, out ignoreActions, parameters) as T : WindowSystem.Create<T>(parametersPassCall, out ignoreActions, parameters);
			if (instance != null) {
				
				if (afterGetInstance != null) {
					
					afterGetInstance(instance);
					
				}

				if (ignoreActions == false) {

					if (transitionItem != null && transitionItem.transition != null && transitionItem.transitionParameters != null) {

						instance.Show(transitionItem);
						
					} else {
						
						instance.Show();
						
					}

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
		
		private float GetNextZDepth(Preferences preferences) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) return 0f;
			#endif

			var layer = (int)preferences.layer;

			var depth = 0f;
			this.currentZDepth[layer] += this.zDepthStep;
			depth = this.currentZDepth[layer];

			return depth;
			
		}

		/// <summary>
		/// Gets the depth step.
		/// </summary>
		/// <returns>The depth step.</returns>
		public static float GetDepthStep() {

			return WindowSystem.instance.depthStep;

		}

		private float GetNextDepth(Preferences preferences, float windowDepth) {

			#if UNITY_EDITOR
			if (Application.isPlaying == false) return 0f;
			#endif

			var layer = (int)preferences.layer;
			
			var depth = 0f;
			this.currentDepth[layer] += this.depthStep;
			depth = this.currentDepth[layer];

			return depth;

		}

		public static void CollectCallVariations(WindowBase screen, System.Action<System.Type[], string[]> onEveryVariation) {
			
			if (screen != null) {
				
				var methods = screen.GetType().GetMethods().Where((info) => info.IsVirtual == false && info.Name == "OnParametersPass").ToList();
				for (int i = 0; i < methods.Count; ++i) {

					var notValid = false;
					var attrs = methods[i].GetCustomAttributes(true);
					foreach (var attr in attrs) {

						if (attr is CompilerIgnore) {

							notValid = true;
							break;

						}

					}

					if (notValid == true) continue;

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
		
		public static Rect GetScreenRect() {
			
			var size = new Vector2(Screen.width, Screen.height);
			return new Rect(Vector2.zero, size);
			
		}

		public static Vector2 ConvertPointWindowToUnityScreen(Vector2 point, WindowObject handler) {

			var size = (handler.GetWindow<UnityEngine.UI.Windows.Types.LayoutWindowType>().layout.GetLayoutInstance().transform as RectTransform).sizeDelta;
			return new Vector2(size.x * 0.5f + point.x, size.y * 0.5f + point.y);

		}

		public static Vector2 ConvertPointWindowToScreen(Vector2 screenPoint, WindowObject handler) {
			
			var k = (handler.GetWindow().GetLayoutRoot() as RectTransform).sizeDelta.x / Screen.width;
			return screenPoint / k;
			
		}

		public static Vector2 ConvertPointScreenToWindow(Vector2 screenPoint, WindowObject handler) {

			var k = (handler.GetWindow().GetLayoutRoot() as RectTransform).sizeDelta.x / Screen.width;
			return screenPoint * k;

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