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
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEngine.UI.Windows {

	public interface IFunctionIteration {

		WindowBase GetWindow();
		int GetFunctionIterationIndex();
		bool Hide();
		bool Hide(AttachItem transitionItem);
		bool Hide(System.Action onHideEnd, AttachItem transitionItem);

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

	public struct WindowSystemShowParameters<T> where T : WindowBase {

		public System.Action<T> afterGetInstance;
		public System.Action onInitialized;
		public AttachItem transitionItem;
		public T source;
		public System.Action<T> onParametersPassCall;
		public System.Action<T> onReady;
		public bool async;
		public object[] parameters;

		public bool overridePreferences;
		public Preferences prefereces;

		public Preferences GetPreferences() {

			return (this.overridePreferences == true) ? this.prefereces : null;

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

		public bool Hide(System.Action onHideEnd, AttachItem transitionItem) {

			return this.sourceWindow.Hide(onHideEnd, transitionItem);

		}

	}

	public class WindowSystem : MonoBehaviour {
		
		public class OnDoTransitionEvent : ME.Events.SimpleEvent<int, int, int, bool> {}
		public static OnDoTransitionEvent onTransition = new OnDoTransitionEvent();

	    protected virtual void OnDestory() {

	        WindowSystem.onTransition = null;

	    }

		[System.Serializable]
		public class HistoryTracker {

			[System.Serializable]
			public class Item {

				public WindowObject handler;
				public WindowComponentHistoryTracker tracker = new WindowComponentHistoryTracker();

			}

			public List<Item> trackers = new List<Item>();

			public void Clear(WindowObject component) {

				if (WindowSystemLogger.IsActiveComponents() == true) {

					this.trackers.RemoveAll(x => x.handler == component);

				}

			}

			public void Add(WindowObject component, HistoryTrackerEventType eventType) {

				if (WindowSystemLogger.IsActiveComponents() == true) {

					var item = this.GetItem(component);
					item.tracker.Add(component, eventType);

				}

			}

			public void Add(WindowObject component, AppearanceParameters parameters, HistoryTrackerEventType eventType) {

				if (WindowSystemLogger.IsActiveComponents() == true) {

					var item = this.GetItem(component);
					item.tracker.Add(component, parameters, eventType);

				}

			}

			private Item GetItem(WindowObject handler) {

				var item = this.trackers.FirstOrDefault(x => x.handler.GetInstanceID() == handler.GetInstanceID());
				if (item == null) {

					item = new Item();
					item.handler = handler;
					this.trackers.Add(item);

				}

				return item;

			}

		}

		public class Events {

			private Dictionary<long, List<System.Action>> actions = new Dictionary<long, List<System.Action>>();
			private int eventsCount;

			public Events() {

				this.eventsCount = System.Enum.GetValues(typeof(WindowEventType)).Length;

			}

			public void Clear(WindowObject handler) {

				for (int i = 0; i < this.eventsCount; ++i) {

					var list = this.GetList(handler, (WindowEventType)i, addOnNull: false);
					if (list != null) list.Clear();

				}

			}

			public void Raise(WindowObject handler, WindowEventType eventType) {

				var list = this.GetList(handler, eventType, addOnNull: false);
				if (list != null) {

					for (int i = 0; i < list.Count; ++i) {
						
						list[i].Invoke();

					}

				}

			}

			public void Register(WindowObject handler, WindowEventType eventType, System.Action callback) {

				var list = this.GetList(handler, eventType, addOnNull: true);
				if (list != null) {

					list.Add(callback);

				}

			}

			public void Unregister(WindowObject handler, WindowEventType eventType, System.Action callback) {

				var list = this.GetList(handler, eventType, addOnNull: false);
				if (list != null) {

					list.Remove(callback);

				}

			}

			private List<System.Action> GetList(WindowObject handler, WindowEventType eventType, bool addOnNull = false) {

				var key = (((long)handler.GetInstanceID() << 32) | (long)eventType);

				List<System.Action> list;
				if (this.actions.TryGetValue(key, out list) == true) {

					return list;

				}

				if (addOnNull == true) {

					list = new List<System.Action>();
					this.actions.Add(key, list);

				}

				return list;

			}

		}

		public class Functions {

			private Dictionary<int, List<System.Action<int>>> items = new Dictionary<int, List<System.Action<int>>>();
			private int iteration = 0;

			public void Register(WindowBase instance, System.Action<int> action) {
				
				instance.SetFunctionIterationIndex(++this.iteration);

				List<System.Action<int>> list;
				if (this.items.TryGetValue(this.iteration, out list) == true) {

					list.Add(action);

				} else {

					this.items.Add(this.iteration, new List<System.Action<int>>() { action });

				}

			}

			public void Call(WindowBase instance, bool reusable) {

				var iteration = instance.GetFunctionIterationIndex();

				List<System.Action<int>> list;
				if (this.items.TryGetValue(iteration, out list) == true) {

					foreach (var item in list) item.Invoke(iteration);
					if (reusable == false) this.items.Remove(iteration);

				}

			}

			public void Remove(WindowBase instance) {
				
				var iteration = instance.GetFunctionIterationIndex();
				this.items.Remove(iteration);

			}

		}

		[System.Serializable]
		public class HistoryItem {

			public WindowBase window;
			public WindowObjectState state;
			
			public HistoryItem(WindowBase window) : this(window, window.GetState()) {}
			
			public HistoryItem(WindowBase window, WindowObjectState state) {
				
				this.state = state;
				this.window = window.GetSource();
				
			}

			public bool IsValid(WindowBase window) {

				return this.window == window.GetSource();

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

			public bool IsRestoreSelectedElement() {

				return this.file != null ? this.file.baseInfo.IsRestoreSelectedElement() : true;

			}

		}
		
		[Header("Required")]
		public ObjectPool objectPool;
		
		[Header("Settings")]
		public Settings settings = new Settings();

		public bool emulateRuntimePlatform = false;
		[ReadOnly("emulateRuntimePlatform", state: false)]
		public bool emulateRuntimePlatformEditorOnly = false;
		[ReadOnly("emulateRuntimePlatform", state: false)]
		public RuntimePlatform currentRuntimePlatform;

		[Header("Window System")]
		/// <summary>
		/// The root screen.
		/// Use WindowSystem.ShowRoot() to show it.
		/// </summary>
		public WindowBase rootScreen;

		/// <summary>
		/// The async loading window. (Optional)
		/// </summary>
		public FlowWindow asyncLoadingWindow;

		/// <summary>
		/// Default windows list.
		/// Use WindowSystem.ShowDefault() to show them.
		/// </summary>
		public List<WindowItem> defaults = new List<WindowItem>();
		
		/// <summary>
		/// All registered windows.
		/// If you want to use WindowSystem.Show<T>() you must register window here.
		/// </summary>
		public List<WindowItem> windows = new List<WindowItem>();

		//[System.NonSerialized]
		//[HideInInspector]
		public List<WindowBase> currentWindows = new List<WindowBase>();
		
		//[HideInInspector]
		/// <summary>
		/// The history of windows.
		/// </summary>
		public List<HistoryItem> history = new List<HistoryItem>();
		public bool historyEnabled = false;

		public Functions functions = new Functions();
		new public Audio.Source audio = new Audio.Source();
		public Events events = new Events();
		public HistoryTracker historyTracker = new HistoryTracker();

		public bool debugWeakReferences;

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

		private WindowLayoutPreferences customLayoutPreferences;

		private List<DebugWeakReference> debugWeakReferencesList = new List<DebugWeakReference>();
		private Transform debugRoot;

	    protected virtual void OnDestroy() {

	        WindowSystem._instance = null;

	    }

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

		public static bool IsDebugWeakReferences() {

			if (WindowSystem._instance == null) return false;

			return WindowSystem.instance.debugWeakReferences;

		}

		public static void RemoveDebugWeakReference(WindowObject obj) {

			if (WindowSystem.IsDebugWeakReferences() == true) {
				
			}

		}

		public static void AddDebugWeakReference(WindowObject obj) {

			if (WindowSystem.IsDebugWeakReferences() == true) {

				if (WindowSystem.instance.debugRoot == null) {

					WindowSystem.instance.debugRoot = new GameObject("Debug").transform;

				}

				var goRef = new GameObject(obj.gameObject.name, typeof(DebugWeakReference));
				goRef.transform.SetParent(WindowSystem.instance.debugRoot);
				var rf = goRef.GetComponent<DebugWeakReference>();
				rf.Setup(obj);

				WindowSystem.instance.debugWeakReferencesList.Add(rf);

				ME.WeakReferenceInfo.Register(obj);

			}

		}

		public static RuntimePlatform GetCurrentRuntimePlatform() {
			
			if (WindowSystem.instance.emulateRuntimePlatform == true) {

				if (WindowSystem.instance.emulateRuntimePlatformEditorOnly == false || Application.isEditor == true) {

					return WindowSystem.instance.currentRuntimePlatform;

				}

			}

			return Application.platform;

		}

		public static bool IsRestoreSelectedElement() {

			if (WindowSystem.instance == null) return false;

			return WindowSystem.instance.settings.IsRestoreSelectedElement();

		}

		#if UNITY_EDITOR
		public virtual void OnValidate() {

			if (Application.isPlaying == true) return;
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			
			if (this.objectPool == null) {

				this.objectPool = Object.FindObjectOfType<ObjectPool>();

			}

		}
		#endif

		private void Awake() {

			WindowSystem.instance = this;

			if (this.objectPool == null) {

				Debug.LogError("WindowSystem need `ObjectPool` reference", this);
				return;

			}

			this.objectPool.Init();

			Debug.LogFormat("[ WindowSystem ] Initialized with platform `{0}`", WindowSystem.GetCurrentRuntimePlatform());

			GameObject.DontDestroyOnLoad(this.gameObject);

			this.Init();

		}

		private WindowBase lastInstance;
		//private WindowBase previousInstance;
		protected virtual void LateUpdate() {

			WindowSystem.UpdateDeviceOrientation();

			var lastWindow = this.lastInstance;
			if (lastWindow != null) {

				lastWindow.events.LateUpdate(lastWindow);

			}

		}

		public static Events GetEvents() {

			if (WindowSystem.instance == null) return new Events();

			return WindowSystem.instance.events;

		}

		public static HistoryTracker GetHistoryTracker() {

			if (WindowSystem.instance == null) return new HistoryTracker();

			return WindowSystem.instance.historyTracker;

		}

		public static void RunSafe(System.Action method) {

			try {

				if (method != null) method.Invoke();

			} catch (System.Exception ex) {

				Debug.LogError(string.Format("RunSafe Exception: {0}\nStack: {1}", ex.Message, ex.StackTrace));

			}

		}

		public static void RunSafe<T>(System.Action<T> method, T obj) {

			try {

				if (method != null) method.Invoke(obj);

			} catch (System.Exception ex) {

				Debug.LogError(string.Format("RunSafe Exception: {0}\nStack: {1}", ex.Message, ex.StackTrace));

			}

		}

		public static void RegisterWindow(WindowBase window) {

			WindowSystem.instance.windows.Add(new WindowItem() { loaded = window });

		}

		public static void OnDoTransition(int index, int fromScreenId, int toScreenId, bool hide = true) {
			
			WindowSystem.onTransition.Invoke(index, fromScreenId, toScreenId, hide);
			
		}

		public static void DestroyWindow(WindowBase window) {

			if (WindowSystem.instance == null) return;

			WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(window, null, null);

		}

		public static void SendManualEvent<T>(T data, bool includeInactive) where T : IManualEvent {

			WindowSystem.ForEachWindow(w => w.OnManualEvent<T>(data), includeInactive);

		}

		public static void ForEachWindow(System.Action<WindowBase> onEach, bool includeInactive) {

			for (int i = 0; i < WindowSystem.instance.currentWindows.Count; ++i) {
				
				var window = WindowSystem.instance.currentWindows[i];
				if (includeInactive == true || 
					(window.GetState() == WindowObjectState.Showing ||
					window.GetState() == WindowObjectState.Shown)) {

					onEach.Invoke(window);

				}

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

		public static void SetCustomLayoutPreferences(WindowLayoutPreferences preferences) {

			WindowSystem.instance.customLayoutPreferences = preferences;

			WindowSystem.ForEachWindow(w => {

				var window = w as LayoutWindowType;
				if (window != null) {

					window.GetCurrentLayout().SetCustomLayoutPreferences(WindowSystem.instance.customLayoutPreferences);

				}

			}, includeInactive: false);

		}

		public static WindowLayoutPreferences GetCustomLayoutPreferences() {

			return WindowSystem.instance.customLayoutPreferences;

		}

		public static void AudioMute() {

			WindowSystem.instance.audio.Mute();

		}

		public static void AudioUnmute() {

			WindowSystem.instance.audio.Unmute();

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

		public static void SendInactiveStateByWindow(WindowBase newWindow) {

			if (WindowSystem.instance == null) return;

			//Debug.Log("SendInactiveStateByWindow: " + newWindow);

			var layer = newWindow.preferences.layer;
			var depth = newWindow.GetDepth();

			var allWindows = WindowSystem.instance.currentWindows;
			for (int i = 0; i < allWindows.Count; ++i) {

				var window = allWindows[i];
				if (window == null) continue;
				if (window.IsVisibile() == false) continue;
				if (window.preferences.layer < layer || (window.preferences.layer == layer && window.GetDepth() < depth)) {

					//Debug.Log("SetInactive: " + window);
					window.SetInactive(newWindow);

				}

			}

		}

		public static void SendActiveStateByWindow(WindowBase closingWindow) {

			if (WindowSystem.instance == null) return;

			var layer = closingWindow.preferences.layer;
			var depth = closingWindow.GetDepth();

			//Debug.Log("SendActiveStateByWindow: " + closingWindow);

			var allWindows = WindowSystem.instance.currentWindows;
			for (int i = 0; i < allWindows.Count; ++i) {

				var window = allWindows[i];
				if (window == null) continue;
				if (window.IsVisibile() == false) continue;
				if (window.preferences.layer < layer || (window.preferences.layer == layer && window.GetDepth() < depth)) {
					
					window.SetActive();

				}

			}

		}

		public static int GetWindowsInFrontCount(WindowBase targetWindow) {

			var layer = targetWindow.preferences.layer;
			var depth = targetWindow.GetDepth();

			//Debug.Log("GetWindowsInFrontCount: " + targetWindow);

			var count = 0;
			var allWindows = WindowSystem.instance.currentWindows;
			for (int i = 0; i < allWindows.Count; ++i) {

				var window = allWindows[i];
				if (window == null || window.preferences.sendActiveState == false) continue;
				if (window.IsVisibile() == false) continue;
				if (/*window.preferences.fullCoverage == true &&*/ (window.preferences.layer > layer || (window.preferences.layer == layer && window.GetDepth() > depth))) {

					//Debug.Log("++count: " + window);
					++count;

				}

			}

			return count;

		}

		public static bool IsWindowsInFrontFullCoverage(WindowBase targetWindow) {

			var layer = targetWindow.preferences.layer;
			var depth = targetWindow.GetDepth();

			var found = false;
			var allWindows = WindowSystem.instance.currentWindows;
			for (int i = 0; i < allWindows.Count; ++i) {

				var window = allWindows[i];
				if (window == null || window.preferences.sendActiveState == false) continue;
				if (window.IsVisibile() == false) continue;
				if ((window.preferences.layer > layer || (window.preferences.layer == layer && window.GetDepth() > depth))) {

					if (window.preferences.fullCoverage == true) {

						found = true;
						break;

					}

				}

			}

			return found;

		}

		public static Orientation GetOrientation() {

			return Screen.width > Screen.height ? Orientation.Horizontal : Orientation.Vertical;

		}

		private static bool deviceOrientationChecked = false;
		private static Orientation previousDeviceOrientation;
		public static void UpdateDeviceOrientation() {

			var currentOrientation = WindowSystem.GetOrientation();
			if (WindowSystem.deviceOrientationChecked == false || WindowSystem.previousDeviceOrientation != currentOrientation) {

				if (WindowSystem.deviceOrientationChecked == true) {

					WindowSystem.ForEachWindow(x => {

						x.SetOrientationChanged();

					}, includeInactive: true);

				}

				WindowSystem.previousDeviceOrientation = currentOrientation;
				WindowSystem.deviceOrientationChecked = true;

			}

		}

		public static void UpdateLastInstance() {

			if (WindowSystem.instance == null) return;

			WindowBase lastInstance = null;

			/*if (WindowSystem.instance.historyEnabled == true) {

				lastInstance = WindowSystem.GetWindow((item) => item.window != null && (item.state == WindowObjectState.Shown || item.state == WindowObjectState.Showing), (item) => (item.GetState() == WindowObjectState.Shown || item.GetState() == WindowObjectState.Showing));

			} else {*/

			lastInstance = WindowSystem.FindOpened<WindowBase>((item) => item != null /*&& item.preferences.IsHistoryActive() == true*/ && (item.GetState() == WindowObjectState.Shown || item.GetState() == WindowObjectState.Showing), last: true);

			//}

			/*if (lastInstance != WindowSystem.instance.lastInstance) {

				WindowSystem.instance.previousInstance = WindowSystem.instance.lastInstance;

			}*/

			WindowSystem.instance.lastInstance = lastInstance;

			if (WindowSystem.instance.lastInstance == null) WindowSystem.instance.lastInstance = null;
			//if (WindowSystem.instance.previousInstance == null) WindowSystem.instance.previousInstance = null;

			//WindowSystem.instance.LateUpdate();

			/*if (WindowSystem.instance.lastInstance != null) {
				
				WindowSystem.instance.lastInstance.SetActive();
				
			}*/

		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		protected virtual void Init() {

			if (this.asyncLoadingWindow != null) {

				this.asyncLoaderSource = this.asyncLoadingWindow.GetScreen(runtime: true).Load<WindowBase>();

			}

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

				this.CreatePoolForWindowSource(window.Load<WindowBase>());

			}

		}

		public static void SetAsyncLoaderState(bool state) {

			WindowSystem.instance.asyncLoaderState = state;

		}

		private bool asyncLoaderState = true;
		private WindowBase asyncLoaderSource;
		private WindowBase asyncLoader;
		internal static void ShowAsyncLoader() {

			if (WindowSystem.instance.asyncLoaderState == true && WindowSystem.instance.asyncLoaderSource != null) {

				WindowSystem.instance.asyncLoader = WindowSystem.Show(WindowSystem.instance.asyncLoaderSource);

			}

		}

		internal static void HideAsyncLoader() {

			if (WindowSystem.instance.asyncLoader != null) {
				
				WindowSystem.instance.asyncLoader.Hide();

			}

		}

		internal void CreatePoolForWindowSource(WindowBase window) {

			if (window.preferences.createPool == false) return;

			if (window.preferences.preallocatedCount > 0) {

				window.CreatePool(window.preferences.preallocatedCount, (source) => { return WindowSystem.instance.Create_INTERNAL(source, null, null, null, false); });

			} else {

				window.CreatePool(this.settings.preallocatedWindowsPoolSize);

			}

		}

		internal static void WaitCoroutine(System.Collections.Generic.IEnumerator<byte> routine) {

			WindowSystem.instance.StartCoroutine(routine);

		}
		
		public static void RegisterFunctionCallback(WindowBase instance, System.Action<int> onFunctionEnds) {

			WindowSystem.instance.functions.Register(instance, onFunctionEnds);

		}
		
		public static void CallFunction(WindowRoutes routes) {

			throw new UnityException("Routes can't call a function");

		}
		
		public static void CallFunction(WindowBase instance, bool reusable) {
			
			WindowSystem.instance.functions.Call(instance, reusable);
			
		}
		
		public static void RemoveFunction(WindowBase instance) {
			
			WindowSystem.instance.functions.Remove(instance);
			
		}

		public static T GetByType<T>() where T : WindowBase {

			return WindowSystem.instance.windows.FirstOrDefault((w) => w.IsType<T>()).Load<T>();

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

		public static bool IsCameraRenderDisableOnWindowTurnOff() {

			if (WindowSystem.instance == null || WindowSystem.instance.settings.file == null) {

				// no settings file

			} else {

				return WindowSystem.instance.settings.file.baseInfo.turnOffCameraRenderOnly;

			}

			return false;

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

				// no settings file

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

				// no settings file

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

			if (WindowSystem.instance == null || Application.isPlaying == false) return;

			if (WindowSystem.instance.historyEnabled == true) {
				
				if (forced == true || window.preferences.IsHistoryActive() == true) {

					var lastItem = WindowSystem.instance.history.LastOrDefault();
					if (lastItem == null || lastItem.IsValid(window) == false) {

						var item = new HistoryItem(window, state);
						WindowSystem.instance.history.Add(item);

					} else if (lastItem != null && lastItem.IsValid(window) == true) {

						lastItem.state = state;

						if (
							state == WindowObjectState.Hiding ||
							state == WindowObjectState.Hidden) {

							WindowSystem.instance.history.Remove(lastItem);

						}

					}

				}

			}

			WindowSystem.UpdateLastInstance();

		}

		public static void RemoveFromHistory(WindowBase window) {

			if (WindowSystem.instance.historyEnabled == true) {

				WindowSystem.instance.history.RemoveAll((item) => item.window == window);

			}

			WindowSystem.RefreshHistory();

		}

		/// <summary>
		/// Refreshes the history.
		/// </summary>
		public static void RefreshHistory() {

			if (WindowSystem.instance.historyEnabled == true) {

				WindowSystem.instance.history.RemoveAll((item) => item.window == null);

			}

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
			
			return WindowSystem.GetPreviousWindow(WindowSystem.GetCurrentWindow());//WindowSystem.instance.previousInstance;
			
		}

		/// <summary>
		/// Gets the last window in history with the predicate.
		/// </summary>
		/// <returns>The previous window.</returns>
		/// <param name="current">Current.</param>
		public static WindowBase GetPreviousWindow(WindowBase current, System.Func<HistoryItem, bool> predicate = null) {
			
			WindowBase prev = null;
			foreach (var item in WindowSystem.instance.history) {

				if (item.IsValid(current) == true) break;

				if (predicate == null || predicate(item) == true) {

					prev = item.window;

				}

			}

			return WindowSystem.FindOpened<WindowBase>(x => x.GetSource() == prev);

		}
		
		/// <summary>
		/// Gets the last window in history.
		/// </summary>
		public static T GetWindow<T>() where T : WindowBase {

			return WindowSystem.GetWindow((item) => item.window is T) as T;

		}

		/// <summary>
		/// Gets the last window in history with the predicate.
		/// </summary>
		public static WindowBase GetWindow(System.Func<HistoryItem, bool> predicateHistory = null, System.Func<WindowBase, bool> predicateFilter = null) {

			WindowBase last = null;
			last = WindowSystem.instance.history
				.Where(item => predicateHistory.Invoke(item) == true)
				.Select(item => WindowSystem.FindOpened<WindowBase>((x) => item.IsValid(x) == true && predicateFilter.Invoke(x) == true, last: true))
				.LastOrDefault();

			return last;
			
		}

		public static List<WindowBase> GetActiveWindows() {
			
			return WindowSystem.instance.currentWindows.Where((item) => item.GetState() == WindowObjectState.Showing || item.GetState() == WindowObjectState.Shown).ToList();

		}

		/// <summary>
		/// Finds the opened window.
		/// </summary>
		public static T FindOpened<T>() where T : WindowBase {

			if (WindowSystem.instance == null) return default(T);

			return WindowSystem.instance.currentWindows.FirstOrDefault((item) => item is T) as T;

		}
		
		/// <summary>
		/// Finds the opened window with a predicate.
		/// </summary>
		/// <param name="predicate">Predicate to filter</param>
		/// <param name="last">Use LastOrDefault()?</param>
		public static T FindOpened<T>(System.Func<T, bool> predicate, bool last = false) where T : WindowBase {
			
			if (last == true) {
				
				return WindowSystem.instance.currentWindows.LastOrDefault(x => x is T && predicate(x as T)) as T;
				
			} else {
				
				return WindowSystem.instance.currentWindows.FirstOrDefault(x => x is T && predicate(x as T)) as T;
				
			}
			
		}

		/// <summary>
		/// Shows the default list windows.
		/// </summary>
		/// <param name="parameters">Parameters.</param>
		public static void ShowDefault(params object[] parameters) {

			WindowSystem.instance.ShowDefault_INTERNAL(null, parameters);

		}

		public static void ShowDefault(System.Action onReady, params object[] parameters) {

			WindowSystem.instance.ShowDefault_INTERNAL(onReady, parameters);

		}

		private void ShowDefault_INTERNAL(System.Action onReady, params object[] parameters) {

			ME.Utilities.CallInSequence(onReady, this.defaults.Select(x => x.Load<WindowBase>()).ToArray(), (window, c) => {

				this.Create_INTERNAL(window, customPreferences: null, onParametersPassCall: null, onInitialized: (w) => {

					w.Show(c);

				}, async: false, parameters: parameters);

			});

		}

		public static void SetDefaultsForRuntime(params WindowBase[] windows) {

			var list = new List<WindowItem>();
			for (int i = 0; i < windows.Length; ++i) {

				var type = windows[i].GetType();
				var window = WindowSystem.instance.windows.FirstOrDefault(x => {
					
					return type.IsAssignableFrom(x.GetWindowType());
					
				});
				if (window != null) {

					list.Add(window);

				}

			}

			WindowSystem.instance.defaults.Clear();
			WindowSystem.instance.defaults.AddRange(list);

		}

		/// <summary>
		/// Shows the default list windows.
		/// </summary>
		/// <param name="parameters">Parameters.</param>
		public static void ShowRoot(params object[] parameters) {

			var root = WindowSystem.instance.rootScreen;
			if (root != null) WindowSystem.Show(root, parameters);

		}

		internal static void ResetDepth() {

			var items = System.Enum.GetValues(typeof(DepthLayer));
			var instance = WindowSystem.instance;

			for (int i = 0; i < items.Length; ++i) {
				
				var item = (int)items.GetValue(i);
				instance.currentDepth[item] = instance.settings.GetMinDepth(item);
				instance.currentZDepth[item] = instance.settings.GetMinZDepth(item);

			}

		}

		internal static void WaitEndOfFrame(System.Action callback) {

			WindowSystem.WaitCoroutine(WindowSystem.instance.EndOfFrame_INTERNAL(callback));

		}

		private System.Collections.Generic.IEnumerator<byte> EndOfFrame_INTERNAL(System.Action callback) {

			//Debug.Log("Wait for 1");

			//yield return new WaitForEndOfFrame();
			//yield return new WaitForEndOfFrame();

			//Debug.Log("Wait for 2");

			yield return 0;
			yield return 0;

			if (callback != null) {

				callback.Invoke();

			}

		}

		public static void HideAllAndClean() {

			WindowSystem.HideAllAndClean(except: (WindowBase)null, callback: null, forceAll: false, immediately: false);

		}

		public static void HideAllAndClean(WindowBase except) {

			WindowSystem.HideAllAndClean(except, callback: null, forceAll: false, immediately: false);

		}

		public static void HideAllAndClean(List<WindowBase> exceptList) {

			WindowSystem.HideAllAndClean(exceptList, callback: null, forceAll: false, immediately: false);

		}

		public static void HideAllAndClean(WindowBase except, System.Action callback) {

			WindowSystem.HideAllAndClean(except, callback, forceAll: false, immediately: false);

		}

		public static void HideAllAndClean(WindowBase except, System.Action callback, bool forceAll) {

			WindowSystem.HideAllAndClean(except, callback, forceAll, immediately: false);

		}

		public static void HideAllAndClean(System.Action callback, bool immediately) {

			WindowSystem.HideAllAndClean(except: (WindowBase)null, callback: callback, forceAll: false, immediately: immediately);

		}

		public static void HideAllAndClean(List<WindowBase> exceptList, System.Action callback) {

			WindowSystem.HideAllAndClean(exceptList, callback, forceAll: false, immediately: false);

		}

		public static void HideAllAndClean(List<WindowBase> exceptList, System.Action callback, bool forceAll) {

			WindowSystem.HideAllAndClean(exceptList, callback, forceAll, immediately: false);

		}

		/// <summary>
		/// Hides all and clean.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAllAndClean(WindowBase except, System.Action callback, bool forceAll, bool immediately) {

			//Debug.Log("HideAllAndClean: " + (except != null ? except.name : "null"));

			WindowSystem.HideAll(except, () => {

				//WindowSystem.WaitEndOfFrame(() => {
					
					WindowSystem.Clean(except, forceAll);

					//WindowSystem.WaitEndOfFrame(() => {

						if (callback != null) callback();
						
					//});

				//});

			}, forceAll, immediately);
			
		}

		/// <summary>
		/// Hides all and clean.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAllAndClean(List<WindowBase> exceptList, System.Action callback, bool forceAll, bool immediately) {

			//Debug.Log("HideAllAndClean: " + (exceptList != null ? exceptList.Count : 0));

			WindowSystem.HideAll(exceptList, () => {

				//WindowSystem.WaitEndOfFrame(() => {

					WindowSystem.Clean(exceptList, forceAll);

					//WindowSystem.WaitEndOfFrame(() => {
						
						if (callback != null) callback();

					//});

				//});

			}, forceAll, immediately);
			
		}

		/// <summary>
		/// Clean all windows.
		/// </summary>
		public static void Clean() {

			WindowSystem.Clean(except: (WindowBase)null, forceAll: false);

		}

		/// <summary>
		/// Clean the specified forceAll.
		/// </summary>
		/// <param name="forceAll">If set to <c>true</c> force all.</param>
		public static void Clean(bool forceAll) {

			WindowSystem.Clean(except: (WindowBase)null, forceAll: forceAll);

		}

		/// <summary>
		/// Clean the specified except.
		/// </summary>
		/// <param name="except">Except.</param>
		public static void Clean(WindowBase except) {

			WindowSystem.Clean(except: except, forceAll: false);

		}

		/// <summary>
		/// Clean the specified exceptList.
		/// </summary>
		/// <param name="exceptList">Except list.</param>
		public static void Clean(List<WindowBase> exceptList) {

			WindowSystem.Clean(exceptList: exceptList, forceAll: false);

		}

		/// <summary>
		/// Clean the specified except and forceAll.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="forceAll">If set to <c>true</c> force all.</param>
		public static void Clean(WindowBase except, bool forceAll) {

			//Debug.LogWarning("Clean!");
			WindowSystem.instance.currentWindows.RemoveAll((window) => {
				
				var result = WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(window, null, except, forceAll);
				//Debug.LogWarning("Try: " + window.name + " :: " + result);
				if (result == true) {
					
					if (window != null) {

						WindowSystem.CleanWindow(window, removeFromList: false);

					}
					
				}

				return result;

			});

            WindowSystem.ResetDepth();

			if (except != null) {

				except.SetDepth(WindowSystem.instance.GetNextDepth(except.preferences, except.workCamera.depth), WindowSystem.instance.GetNextZDepth(except.preferences));
				except.OnCameraReset();

			}

			WindowSystem.RefreshHistory();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();

        }

        /// <summary>
        /// Clean the specified except.
        /// </summary>
        /// <param name="except">Except.</param>
        public static void Clean(List<WindowBase> exceptList, bool forceAll) {

			//Debug.LogWarning("Clean!");
			WindowSystem.instance.currentWindows.RemoveAll((window) => {

				var result = WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(window, exceptList, null, forceAll);
				//Debug.LogWarning("Try: " + window.name + " :: " + result);
				if (result == true) {

					if (window != null) {

						WindowSystem.CleanWindow(window, removeFromList: false);

					}

				}
                
				return result;


			});

			//ObjectPool.ClearReserved();

            WindowSystem.ResetDepth();

			if (exceptList != null) {

				for (int i = 0; i < exceptList.Count; ++i) {

					var instance = exceptList[i];
					if (instance == null) continue;
					instance.SetDepth(WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth), WindowSystem.instance.GetNextZDepth(instance.preferences));
					instance.OnCameraReset();

				}

			}

			WindowSystem.RefreshHistory();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();

        }
		
		public static void CleanWindow(WindowBase window, bool removeFromList = true, System.Action callback = null) {
			
			if (WindowSystem.instance == null) return;
			if (window == null) return;
			
			System.Action callbackInner = () => {

				window.gameObject.SetActive(false);

				if (removeFromList == true) WindowSystem.instance.currentWindows.Remove(window);
				(window as IWindowEventsController).DoWindowUnload();
				window.DoDestroy(() => {

					//window.Recycle();
					ObjectPool.ClearPool(window.GetSource());
					WindowSystem.instance.CreatePoolForWindowSource(window.GetSource());

					if (WindowSystem.IsDebugWeakReferences() == true) {

						System.GC.Collect();
						System.GC.WaitForPendingFinalizers();

					}
					
					if (callback != null) callback.Invoke();
					
				});

			};
			
			if (window.preferences.deactivateOnRecycleImmediately == true) {
				
				callbackInner.Invoke();
				
			} else {
				
				TweenerGlobal.instance.addTween(window, Random.Range(window.preferences.deactivateMaxTimeout.x, window.preferences.deactivateMaxTimeout.y), 0f, 0f).tag(window).onComplete(callbackInner);
				
			}
			
		}
		
		public static void CleanWindows<T>(List<T> windows) where T : WindowBase {

			for (var i = 0; i < windows.Count; ++i) {
				
				var window = windows[i];
				WindowSystem.CleanWindow(window);

			}

		}

		public static List<WindowBase> GetCurrentList() {

			return WindowSystem.instance.currentWindows;

		}

		public static void Recycle(WindowBase window, bool setInactive) {
			
			if (window.skipRecycle == false) {

				if (window.preferences.createPool == false ||
				    ObjectPool.IsRegisteredInPool(window.GetSource()) == false) {
					
					WindowSystem.CleanWindow(window, removeFromList: true, callback: () => {
						
						window.Recycle(setInactive);
						
					});
					
				} else {
					
					window.Recycle(setInactive);

				}
				
			}

		}

        private bool DestroyWindowCheckOnClean_INTERNAL(WindowBase window, List<WindowBase> exceptList, WindowBase exceptItem, bool forceAll = false) {
			
			if (window != null) {

				if (window.preferences.IsDontDestroyEver() == true) {

					return false;

				}

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
		public static void HideAll(WindowBase except = null, System.Action callback = null, bool forceAll = false, bool immediately = false) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);

			var list = WindowSystem.instance.currentWindows.Where((w) => {

				return WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(w, null, except, forceAll);

			}).ToList();
			ME.Utilities.CallInSequence(callback, list, (window, wait) => {
				
				window.Hide(onHideEnd: null, immediately: immediately);
				//Debug.Log(window.name);
				if (window.GetState() != WindowObjectState.Hidden) {
					
					//Debug.Log(window.name + " :: " + window.GetState());
					UnityAction action = null;
					action = () => {
						
						//Debug.Log(window.name + " :: " + window.GetState());
						window.events.onEveryInstance.Unregister(WindowEventType.OnHideEndLate, action);
						wait.Invoke();
						
					};
					window.events.onEveryInstance.Register(WindowEventType.OnHideEndLate, action);
					
				} else {
					
					wait.Invoke();
					
				}
				
			});

		}

		/// <summary>
		/// Hides all.
		/// </summary>
		/// <param name="except">Except.</param>
		/// <param name="callback">Callback.</param>
		public static void HideAll(List<WindowBase> exceptList, System.Action callback, bool forceAll = false, bool immediately = false) {
			
			WindowSystem.instance.currentWindows.RemoveAll((window) => window == null);
			
			var list = WindowSystem.instance.currentWindows.Where((w) => {

				return WindowSystem.instance.DestroyWindowCheckOnClean_INTERNAL(w, exceptList, null, forceAll);

			}).ToList();
			ME.Utilities.CallInSequence(callback, list, (window, wait) => {
				
				window.Hide(onHideEnd: null, immediately: immediately);
				if (window.GetState() != WindowObjectState.Hidden) {
					
					//Debug.Log(window.name + " :: " + window.GetState());
					UnityAction action = null;
					action = () => {
						
						//Debug.Log(window.name + " :: " + window.GetState());
						window.events.onEveryInstance.Unregister(WindowEventType.OnHideEndLate, action);
						wait.Invoke();

					};
					window.events.onEveryInstance.Register(WindowEventType.OnHideEndLate, action);
					
				} else {
					
					wait.Invoke();
					
				}
				
			});
			
		}

		public static T GetSource<T>() where T : WindowBase {

			var source = WindowSystem.instance.windows.FirstOrDefault(w => w.IsType<T>() == true).Load<T>();
			return source;

		}

		/// <summary>
		/// Create the specified parameters.
		/// </summary>
		/// <param name="parameters">Parameters.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Create<T>(Preferences prefs, System.Action<WindowBase> onParametersPassCall, System.Action<WindowBase> onInitialized, out bool ignoreActions, bool async, params object[] parameters) where T : WindowBase {

			ignoreActions = false;
			
			var source = WindowSystem.GetSource<T>();
			if (source == null) return null;

			return WindowSystem.CreateWithIgnore_INTERNAL(source, prefs, onParametersPassCall, onInitialized, out ignoreActions, async, parameters) as T;

		}
		
		internal WindowBase Create_INTERNAL(WindowBase source, Preferences customPreferences, System.Action<WindowBase> onParametersPassCall, System.Action<WindowBase> onInitialized, bool async, params object[] parameters) {

			bool ignoreActions;
			return WindowSystem.CreateWithIgnore_INTERNAL(source, customPreferences, onParametersPassCall, onInitialized, out ignoreActions, async, parameters);

		}

		internal static WindowBase CreateWithIgnore_INTERNAL(WindowBase source, Preferences customPreferences, System.Action<WindowBase> onParametersPassCall, System.Action<WindowBase> onInitialized, out bool ignoreActions, bool async, params object[] parameters) {

			ignoreActions = false;
			
			WindowBase instance = null;
			if (source.preferences.forceSingleInstance == true) {
				
				instance = WindowSystem.instance.currentWindows.FirstOrDefault(w => w.windowId == source.windowId);
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

			if (customPreferences != null) {

				instance.preferences = customPreferences;

			}

			if (WindowSystem.instance.currentWindows.Contains(instance) == false) {
				
				WindowSystem.instance.currentWindows.Add(instance);
				
			}
			
			if (ignoreActions == false) {
				
				instance.SetParameters(onParametersPassCall, parameters);
				instance.Init(
					source,
					WindowSystem.instance.GetNextDepth(instance.preferences, instance.workCamera.depth),
					WindowSystem.instance.GetNextZDepth(instance.preferences),
					WindowSystem.instance.GetNextRaycastPriority(),
					WindowSystem.instance.GetNextOrderInLayer(),
					onInitialized: () => {
						
						if (onInitialized != null) onInitialized.Invoke(instance);
						
					},
					async: async
				);
				
			}
			
			return instance;
			
		}

		public static T ShowAsync<T>(System.Action<T> onReady) where T : WindowBase {
			
			WindowSystem.ShowAsyncLoader();
			
			return WindowSystem.ShowWithParameters<T>(null, null, null, null, null, (w) => {
				
				WindowSystem.HideAsyncLoader();
				if (onReady != null) onReady.Invoke(w);
				
			}, true);
			
		}
		
		public static T ShowAsync<T>(System.Action<T> onParametersPassCall, System.Action<T> onReady) where T : WindowBase {
			
			WindowSystem.ShowAsyncLoader();
			
			var newWindow = WindowSystem.ShowWithParameters<T>(null, null, null, null, onParametersPassCall, onReady, true);
			
			UnityEngine.Events.UnityAction action = null;
			action = () => {

				newWindow.events.onEveryInstance.Unregister(WindowEventType.OnShowEnd, action);
				WindowSystem.HideAsyncLoader();
				
			};
			newWindow.events.onEveryInstance.Register(WindowEventType.OnShowEnd, action);
			
			return newWindow;
			
		}
		
		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, null, null, null, null, null, false, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {

			return WindowSystem.ShowWithParameters<T>(null, null, null, null, onParametersPassCall, null, false, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(T source, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, null, null, source, null, null, false, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(T source, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, null, null, source, onParametersPassCall, null, false, parameters);
			
		}

		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="afterGetInstance">On create predicate.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(System.Action<T> afterGetInstance, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {

			return WindowSystem.ShowWithParameters<T>(afterGetInstance, null, null, null, onParametersPassCall, null, false, parameters);
			
		}
		
		/// <summary>
		/// Shows window of T type with specific transition.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="transition">Transition Item.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(AttachItem transitionItem, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {
			
			return WindowSystem.ShowWithParameters<T>(null, null, transitionItem, null, onParametersPassCall, null, false, parameters);
			
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
			
			return WindowSystem.ShowWithParameters<T>(null, null, transitionItem, source, onParametersPassCall, null, false, parameters);
			
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

			return WindowSystem.ShowWithParameters<T>(afterGetInstance, null, transitionItem, null, onParametersPassCall, null, false, parameters);

		}
		
		/// <summary>
		/// Shows window of T type.
		/// Returns null if window not registered.
		/// </summary>
		/// <param name="afterGetInstance">On create predicate.</param>
		/// <param name="transition">Transition Item.</param>
		/// <param name="parameters">OnParametersPass() values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Show<T>(bool async, System.Action<T> afterGetInstance, AttachItem transitionItem, System.Action<T> onParametersPassCall, params object[] parameters) where T : WindowBase {

			return WindowSystem.ShowWithParameters<T>(afterGetInstance, null, transitionItem, null, onParametersPassCall, null, async, parameters);

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
		public static T ShowWithParameters<T>(
			System.Action<T> afterGetInstance,
			System.Action onInitialized,
			AttachItem transitionItem,
			T source,
			System.Action<T> onParametersPassCall,
			System.Action<T> onReady,
			bool async,
			params object[] parameters
			) where T : WindowBase {

			var p = new WindowSystemShowParameters<T>() {
				afterGetInstance = afterGetInstance,
				onInitialized = onInitialized,
				transitionItem = transitionItem,
				source = source,
				onParametersPassCall = onParametersPassCall,
				onReady = onReady,
				@async = async,
				parameters = parameters,
			};
			return WindowSystem.ShowWithParameters<T>(p);

		}

		public static T ShowWithParameters<T>(WindowSystemShowParameters<T> parameters) where T : WindowBase {

			System.Action<WindowBase> parametersPassCall = null;
			if (parameters.onParametersPassCall != null) {

				parametersPassCall = (WindowBase window) => {

					if (parameters.onParametersPassCall != null) {
						
						parameters.onParametersPassCall.Invoke(window as T);
						
					}

				};

			}

			T instance = default(T);

			var ignoreActions = false;
			System.Action<WindowBase> onInitializedInner = (WindowBase windowInstance) => {

				if (parameters.onInitialized != null) {

					parameters.onInitialized.Invoke();

				}

				if (parameters.onReady != null) {

					parameters.onReady.Invoke(windowInstance as T);

				}


				if (parameters.afterGetInstance != null) {

					parameters.afterGetInstance.Invoke(windowInstance as T);

				}

				if (ignoreActions == false) {
					
					System.Action showInstance = () => {
						
						if (parameters.transitionItem != null && parameters.transitionItem.transition != null && parameters.transitionItem.transitionParameters != null) {
							
							windowInstance.Show(parameters.transitionItem);
							
						} else {
							
							windowInstance.Show();
							
						}
						
					};
					
					if (windowInstance.preferences.showInSequence == true) {
						
						var openedInstance = WindowSystem.FindOpened<T>(x => x != instance && x.IsVisibile() == true, last: true);
						if (openedInstance != null) {
							
							UnityAction action = null;
							action = () => {
								
								showInstance.Invoke();
								
								openedInstance.events.onEveryInstance.Unregister(WindowEventType.OnHideEnd, action);
								
							};
							openedInstance.events.onEveryInstance.Register(WindowEventType.OnHideEnd, action);
							
						} else {
							
							showInstance.Invoke();
							
						}
						
					} else {
						
						showInstance.Invoke();
						
					}
					
				}

			};

			instance = (parameters.source != null) ?
				WindowSystem.CreateWithIgnore_INTERNAL(parameters.source, parameters.GetPreferences(), parametersPassCall, onInitializedInner, out ignoreActions, parameters.async, parameters.parameters) as T : 
				WindowSystem.Create<T>(parameters.GetPreferences(), parametersPassCall, onInitializedInner, out ignoreActions, parameters.async, parameters.parameters);
			
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

        public static Vector2 ConvertPointWindowToGLScreen(Vector2 point, WindowObject handler) {

			var size = (handler.GetWindow<UnityEngine.UI.Windows.Types.LayoutWindowType>().GetCurrentLayout().GetLayoutInstance().transform as RectTransform).sizeDelta;
            var screenSize = new Vector2(Screen.width, Screen.height);

            var result = new Vector2(screenSize.x / size.x, screenSize.y / size.y);
            result.Scale(new Vector2(size.x * 0.5f + point.x, size.y * 0.5f - point.y));

            return result;

        }

        public static Vector2 ConvertPointWindowToUnityScreen(Vector2 point, WindowObject handler) {

			var size = (handler.GetWindow<UnityEngine.UI.Windows.Types.LayoutWindowType>().GetCurrentLayout().GetLayoutInstance().transform as RectTransform).sizeDelta;
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

				var scaleFrom = fromLayout.GetCurrentLayout().GetLayoutInstance().transform.localScale.x;
				var scaleTo = toLayout.GetCurrentLayout().GetLayoutInstance().transform.localScale.x;

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