#define TRANSITION_ENABLED
//#define TRANSITION_POSTEFFECTS_ENABLED
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Animations;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI.Windows {

	/*
	 * Events order: (Any of OnShowBegin, OnHideBegin, etc.)
	 * Layout
	 * Modules
	 * Audio
	 * Events
	 * Transitions
	 * Screen
	 */

	[ExecuteInEditMode()]
	//[RequireComponent(typeof(Camera))]
	public abstract class WindowBase :
		WindowObject,
		IWindowEventsAsync,
		IWindowEventsController,
		IFunctionIteration,
		IWindow,
		IBeginDragHandler,
		IDragHandler,
		IEndDragHandler,
		IDraggableHandler,
		IResourceReference,
		ICanvasElement {
		
		#if UNITY_EDITOR
		[HideInInspector]
		public bool editorInfoFold = false;
		#endif

		[SerializeField][Hidden("_workCamera", state: null, inverseCondition: true)]
		private Camera _workCamera;
		[HideInInspector]
		public Camera workCamera { get { return this._workCamera; } protected set { this._workCamera = value; } }
		[HideInInspector][FormerlySerializedAs("initialized")]
		public bool isReady = false;

		[Header("Window Preferences")]
		public TargetPreferences targetPreferences = new TargetPreferences();
		public Preferences preferences = new Preferences();
		new public Audio.Window audio = new Audio.Window();
		public ModulesList modules = new ModulesList();
		public Events events = new Events();
		#if !TRANSITION_ENABLED
		[ReadOnly]
		#endif
		public Transition transition = new Transition();

		[Header("Logger")]
		public WindowComponentHistoryTracker eventsHistoryTracker = new WindowComponentHistoryTracker();

		[SerializeField][HideInInspector]
		private ActiveState activeState = ActiveState.None;
		//[SerializeField][HideInInspector]
		private int activeIteration = 0;
		[SerializeField][HideInInspector]
		private WindowObjectState _currentState = WindowObjectState.NotInitialized;
		private WindowObjectState currentState {

			set {

				this.lastState = this._currentState;
				this._currentState = value;

			}

			get {

				return this._currentState;

			}

		}
		[SerializeField][HideInInspector]
		private WindowObjectState lastState = WindowObjectState.NotInitialized;
		[SerializeField][HideInInspector]
		private DragState dragState = DragState.None;
		[SerializeField][HideInInspector]
		private bool paused = false;

		private int functionIterationIndex = 0;

		[HideInInspector]
		protected bool setup = false;
		[HideInInspector]
		private bool passParams = false;
		[HideInInspector]
		private object[] parameters;
		private System.Action<WindowBase> onParametersPassCall;

		[HideInInspector][System.NonSerialized]
		public bool skipRecycle = false;

		protected InitialParameters initialParameters;

		private WindowBase source;

		private GameObject firstSelectedGameObject;
		private GameObject currentSelectedGameObject;

		#region ICanvasElement
		Transform ICanvasElement.transform {
			get { return base.transform; }
		}

		void ICanvasElement.GraphicUpdateComplete() {
			
		}

		bool ICanvasElement.IsDestroyed() {

			return this == null;

		}

		void ICanvasElement.LayoutComplete() {

			ME.Coroutines.RunNextFrame(() => {

				if (this.GetState() != WindowObjectState.Hiding &&
					this.GetState() != WindowObjectState.Hidden &&
					this.GetState() != WindowObjectState.Deinitializing &&
					this.GetState() != WindowObjectState.NotInitialized) {

					this.DoLayoutWindowLayoutComplete();
					this.modules.DoWindowLayoutComplete();
					this.audio.DoWindowLayoutComplete();
					this.events.DoWindowLayoutComplete();
					this.transition.DoWindowLayoutComplete();
					(this as IWindowEventsController).DoWindowLayoutComplete();

				}

			});

		}

		void ICanvasElement.Rebuild(CanvasUpdate executing) {
			
		}
		#endregion

		public override bool IsDestroyable() {

			return false;

		}

		public bool SourceEquals(WindowBase y) {

			if (y == null) return false;
			
			if (this.source == null) return y.source == this;
			if (y.source == null) return y == this.source;

			return this.source == y.source;
			
		}

		public WindowBase GetSource() {

			return this.source;

		}
		
		public void Setup(FlowData data) {

			this.Setup(-1, data);

		}

		public void Setup(int id, FlowData data) {

			#if UNITY_EDITOR
			if (id >= 0) this.windowId = id;
			if (this.audio == null) this.audio = new Audio.Window();
			this.audio.flowData = data;
			#endif

		}

		public virtual void OnCameraReset() {
		}

		public void SetAsPerspective() {

			WindowSystem.ApplyToSettings(this.workCamera, mode: WindowSystemSettings.Camera.Mode.Perspective);

		}

		public void SetAsOrthoraphic() {

			WindowSystem.ApplyToSettings(this.workCamera, mode: WindowSystemSettings.Camera.Mode.Orthographic);

		}

		public bool IsVisible() {

			return (this.GetState() == WindowObjectState.Showing || this.GetState() == WindowObjectState.Shown);

		}

		public float GetDepth() {

			return this.workCamera.depth;

		}

		public float GetZDepth() {

			return this.transform.position.z;

		}

		public void SetDepth(float depth, float zDepth) {
			
			var pos = this.transform.position;
			pos.z = zDepth;
			this.transform.position = pos;
			
			this.workCamera.depth = depth;
			
		}

		internal void Init(WindowBase source, float depth, float zDepth, int raycastPriority, int orderInLayer, System.Action onInitialized, bool async) {

			this.source = source;
			this.Init(depth, zDepth, raycastPriority, orderInLayer, onInitialized, async);

		}

		internal void Init(float depth, float zDepth, int raycastPriority, int orderInLayer, System.Action onInitialized, bool async) {

			this.Init(new InitialParameters() { depth = depth, zDepth = zDepth, raycastPriority = raycastPriority, orderInLayer = orderInLayer }, onInitialized, async);

		}

		internal void Init(InitialParameters parameters, System.Action onInitialized, bool async) {

			this.initialParameters = parameters;

			this.currentState = WindowObjectState.Initializing;

			if (this.isReady == false) {
				
				this.currentState = WindowObjectState.NotInitialized;

				Debug.LogError("Can't initialize window instance because of some components were not installed properly.", this);
				return;

			}

			this.SetOrientationChangedDirect();
			this.SetDepth(this.initialParameters.depth, this.initialParameters.zDepth);
			this.events.LateUpdate(this);

			if (Application.isPlaying == true) {
				
				if (this.preferences.IsDontDestroyOnSceneChange() == true) {

					GameObject.DontDestroyOnLoad(this.gameObject);

				}

			}

			if (this.passParams == true) {

				if (this.parameters != null && this.parameters.Length > 0) {

					System.Reflection.MethodInfo methodInfo;
					if (WindowSystem.InvokeMethodWithParameters(out methodInfo, this, "OnParametersPass", this.parameters) == true) {

						// Success
						methodInfo.Invoke(this, this.parameters);

					} else {

						// Method not found
						var prs = new string[this.parameters.Length];
						for (int i = 0; i < prs.Length; ++i) prs[i] = this.parameters[i].ToString();
						Debug.LogWarning(string.Format("Method `OnParametersPass` was not found with input parameters: {0}, Parameters: {1}", this.parameters.Length, string.Join(", ", prs)), this);

					}

				}

                if (this.onParametersPassCall != null) {

                    this.onParametersPassCall.Invoke(this);
                    this.onParametersPassCall = null;
                    this.passParams = false;

                }

            } else {
				
				this.OnEmptyPass();
				
			}

			System.Action callbackInner = () => {

				this.setup = true;

				//Debug.Log("INIT: " + this.activeIteration + " :: " + this);

				this.currentState = WindowObjectState.Initialized;

				this.eventsHistoryTracker.Add(this, HistoryTrackerEventType.Init);

				if (onInitialized != null) onInitialized.Invoke();

			};

			if (this.setup == false) {

				this.Setup(this);

				#if DEBUGBUILD
				Profiler.BeginSample("WindowBase::OnPreInit()");
				#endif

				this.OnPreInit();

				#if DEBUGBUILD
				Profiler.EndSample();
				#endif

				#if DEBUGBUILD
				Profiler.BeginSample("WindowBase::OnInit()");
				#endif

				this.DoLayoutInit(this.initialParameters.depth, this.initialParameters.raycastPriority, this.initialParameters.orderInLayer, () => {

					WindowSystem.ApplyToSettingsInstance(this.workCamera, this.GetCanvas(), this);

					this.OnModulesInit();
					this.OnAudioInit();
					this.events.DoInit();
					this.OnTransitionInit();

					if (WindowSystem.IsCallEventsEnabled() == true) {

						this.OnInit();

					}

					callbackInner.Invoke();

				}, async);

				#if DEBUGBUILD
				Profiler.EndSample();
				#endif

			} else {

				callbackInner.Invoke();

			}

		}

		void IWindowEventsController.DoWindowUnload() {

			//Debug.LogWarningFormat("Unloading window `{0}` with state `{1}`", this.name, this.GetState());

			this.DoLayoutUnload();
			this.modules.DoWindowUnload();
			this.audio.DoWindowUnload();
			this.events.DoWindowUnload();
			this.transition.DoWindowUnload();

			this.OnWindowUnload();

		}

		public void ApplyActiveState() {

			this.activeIteration = -WindowSystem.GetWindowsInFrontCount(this) - 1;
			this.SetActive();
			if (this.preferences.sendActiveState == true) {

				WindowSystem.SendInactiveStateByWindow(this);

			}

		}

		public void SetFunctionIterationIndex(int iteration) {
			
			this.functionIterationIndex = iteration;
			
		}
		
		public int GetFunctionIterationIndex() {
			
			return this.functionIterationIndex;
			
		}

		internal void SetParameters(System.Action<WindowBase> onParametersPassCall, params object[] parameters) {
			
			this.onParametersPassCall = onParametersPassCall;

			if (parameters != null && parameters.Length > 0) {

				this.parameters = parameters;
				this.passParams = true;

			} else {

				this.passParams = (onParametersPassCall != null);

			}

		}

		public virtual Canvas GetCanvas() {

			return null;

		}

		public virtual CanvasScaler GetCanvasScaler() {

			return null;

		}

		/// <summary>
		/// Gets the state.
		/// </summary>
		/// <returns>The state.</returns>
		public virtual WindowObjectState GetLastState() {

			return this.lastState;

		}

		public virtual WindowObjectState GetState() {
			
			return this.currentState;
			
		}

		public virtual Vector2 GetSize() {

			return new Vector2(Screen.width, Screen.height);

		}
		
		public void ApplyInnerCameras(Camera[] cameras, bool front) {
			
			var currentDepth = this.workCamera.depth;
			var depthStep = WindowSystem.GetDepthStep() * 0.5f;
			var camerasCount = cameras.Length;
			
			var innerStep = depthStep / camerasCount;
			for (int i = 0; i < camerasCount; ++i) {
				
				cameras[i].orthographicSize = this.workCamera.orthographicSize;
				cameras[i].depth = currentDepth + innerStep * (i + 1) * (front == true ? 1f : -1f);
				
			}
			
		}

		#if TRANSITION_POSTEFFECTS_ENABLED
		private void OnRenderImage(RenderTexture source, RenderTexture destination) {
			
			this.transition.OnRenderImage(source, destination);
			
		}
		
		private void OnPostRender() {
			
			this.transition.OnPostRender();
			
		}
		
		private void OnPreRender() {
			
			this.transition.OnPreRender();
			
		}
		#endif

		private void OnAudioInit() {

			this.audio.Setup(this);
			this.audio.DoInit();

		}

		private void OnTransitionInit() {
			
			this.workCamera.clearFlags = CameraClearFlags.Depth;
			this.transition.Setup(this);
			this.transition.DoInit();

		}

		private void OnModulesInit() {

			this.modules.Create(this, this.GetLayoutRoot());
			this.modules.DoInit();

		}

		public ActiveState GetActiveState() {

			return this.activeState;

		}

		public void TurnOnRender() {

			this.workCamera.enabled = true;
			//var canvas = this.GetCanvas();
			//if (canvas != null) canvas.enabled = true;
			var canvases = this.GetComponentsInChildren<Canvas>(includeInactive: true);
			for (int i = 0; i < canvases.Length; ++i) canvases[i].enabled = true;

		}

		public void TurnOffRender() {

			this.workCamera.enabled = false;
			//var canvas = this.GetCanvas();
			//if (canvas != null) canvas.enabled = false;
			var canvases = this.GetComponentsInChildren<Canvas>(includeInactive: true);
			for (int i = 0; i < canvases.Length; ++i) canvases[i].enabled = false;

		}

		public void SetActive() {

			//Debug.Log("SetActive: " + this);

			++this.activeIteration;

			if (this.activeIteration == 0) {

				if (this.activeState != ActiveState.Active) {

					this.activeState = ActiveState.Active;

					this.TurnOnRender();

					this.eventsHistoryTracker.Add(this, HistoryTrackerEventType.WindowActive);

					this.DoLayoutWindowActive();
					this.modules.DoWindowActive();
					this.audio.DoWindowActive();
					this.events.DoWindowActive();
					this.transition.DoWindowActive();
					(this as IWindowEventsController).DoWindowActive();

				}

			} else {

				if (WindowSystem.IsWindowsInFrontFullCoverage(this) == false) {

					this.TurnOnRender();

				}

			}

		}

		public void SetInactive(WindowBase newWindow) {

			//Debug.Log("SetInactive: " + this);

			if (newWindow != null && newWindow.preferences.fullCoverage == true) {

				this.TurnOffRender();

			}

			--this.activeIteration;

			if (this.activeIteration == -1) {
				
				if (this.activeState != ActiveState.Inactive) {

					this.activeState = ActiveState.Inactive;

					this.eventsHistoryTracker.Add(this, HistoryTrackerEventType.WindowInactive);

					this.DoLayoutWindowInactive();
					this.modules.DoWindowInactive();
					this.audio.DoWindowInactive();
					this.events.DoWindowInactive();
					this.transition.DoWindowInactive();
					(this as IWindowEventsController).DoWindowInactive();

				}

			}

		}

		void IWindowEventsController.DoWindowLayoutComplete() {
			
			WindowSystem.RunSafe(this.OnWindowLayoutComplete);

		}

		public virtual void OnWindowLayoutComplete() {
		}

		void IWindowEventsController.DoWindowActive() {

			WindowSystem.RunSafe(this.OnWindowActive);

			if (WindowSystem.IsRestoreSelectedElement() == true && this.preferences.restoreSelectedElement == true && EventSystem.current != null) {

				EventSystem.current.firstSelectedGameObject = this.firstSelectedGameObject;
				EventSystem.current.SetSelectedGameObject(this.currentSelectedGameObject);

			}

		}

		public virtual void OnWindowActive() {
		}

		void IWindowEventsController.DoWindowInactive() {

			WindowSystem.RunSafe(this.OnWindowInactive);

			if (WindowSystem.IsRestoreSelectedElement() == true && this.preferences.restoreSelectedElement == true && EventSystem.current != null) {

				this.firstSelectedGameObject = EventSystem.current.firstSelectedGameObject;
				this.currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;

			}

		}

		public virtual void OnWindowInactive() {
		}

		public virtual void OnVersionChanged() {
		}

		public virtual void OnLocalizationChanged() {
		}

		public virtual void OnManualEvent<T>(T data) where T : IManualEvent {
		}

		public virtual void OnBackButtonAction() {
		}

		#region Orientation
		public virtual void SetOrientationChangedDirect() {

			var orientation = WindowSystem.GetOrientation();
			if (orientation == Orientation.Horizontal) {

				this.ApplyOrientationIndexDirect(0);

			} else if (orientation == Orientation.Vertical) {

				this.ApplyOrientationIndexDirect(1);

			}

		}

		public virtual void ApplyOrientationIndexDirect(int index) {
		}

		public virtual void SetOrientationChanged() {

			var orientation = WindowSystem.GetOrientation();
			if (orientation == Orientation.Horizontal) {

				this.ApplyOrientationIndex(0);

			} else if (orientation == Orientation.Vertical) {

				this.ApplyOrientationIndex(1);

			}

			this.OnOrientationChanged();

		}

		public virtual void ApplyOrientationIndex(int index) {
		}

		public virtual void OnOrientationChanged() {
		}
		#endregion

		#region DRAG'n'DROP
		private List<WindowLayoutElement> dragTempParent = new List<WindowLayoutElement>();
		public void OnBeginDrag(PointerEventData eventData) {

			this.SetDragBegin(eventData);

		}

		public void OnDrag(PointerEventData eventData) {
			
			this.SetDrag(eventData);

		}

		public void OnEndDrag(PointerEventData eventData) {
			
			this.SetDragEnd(eventData);

		}

		public bool IsDraggable() {

			return this.preferences.draggable;

		}

		private void SetDragBegin(PointerEventData eventData) {

			if (this.preferences.draggable == false) return;
			if (this.dragState != DragState.None) return;
			if (eventData.pointerCurrentRaycast.gameObject == null) return;

			this.dragTempParent.Clear();
			eventData.pointerCurrentRaycast.gameObject.GetComponentsInParent<WindowLayoutElement>(includeInactive: true, results: this.dragTempParent);

			if (this.dragTempParent.Count > 0) {

				var layoutElement = this.dragTempParent[0];
				if (this.preferences.dragTag == LayoutTag.None || this.preferences.dragTag == layoutElement.tag) {
					
					this.dragState = DragState.Begin;

					this.OnMoveBegin(eventData);

				}

			}

		}
		
		private void SetDrag(PointerEventData eventData) {
			
			if (this.preferences.draggable == false) return;
			if (this.dragState != DragState.Begin && this.dragState != DragState.Move) return;

			this.dragState = DragState.Move;

			var delta = eventData.delta;
			this.SetDrag_INTERNAL(delta);
			
			this.OnMove(eventData);

		}

		private void SetDrag_INTERNAL(Vector2 delta) {

			var k = (this.GetLayoutRoot() as RectTransform).sizeDelta.x / Screen.width;

			if (this.preferences.IsDragViewportRestricted() == true) {

				var screenRect = WindowSystem.GetScreenRect();
				var rect = this.GetRect();
				rect.center += delta;

				if (rect.xMin <= screenRect.xMin) {

					delta.x += screenRect.xMin - rect.xMin;

				}
				if (rect.yMin - rect.height <= screenRect.yMin) {

					delta.y += screenRect.yMin - (rect.yMin - rect.height);

				}
				if (rect.xMax >= screenRect.xMax) {

					delta.x += screenRect.xMax - rect.xMax;

				}
				if (rect.yMax - rect.height >= screenRect.yMax) {

					delta.y += screenRect.yMax - (rect.yMax - rect.height);

				}

			}

			this.MoveLayout(delta * k);

		}
		
		private void SetDragEnd(PointerEventData eventData) {
			
			if (this.preferences.draggable == false) return;
			if (this.dragState != DragState.Begin && this.dragState != DragState.Move) return;
			
			this.dragState = DragState.End;

			this.OnMoveEnd(eventData);

			this.dragState = DragState.None;
			
		}

		public virtual void OnMoveBegin(PointerEventData eventData) {
		}

		public virtual void OnMove(PointerEventData eventData) {
		}

		public virtual void OnMoveEnd(PointerEventData eventData) {
		}
		#endregion

		public virtual Rect GetRect() {

			var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(this.GetLayoutRoot());
			return new Rect(new Vector2(bounds.center.x, bounds.center.y), new Vector2(bounds.size.x, bounds.size.y));

		}

		public bool TurnOff() {

			if (this == null) return false;

			#if DEBUGBUILD
			Profiler.BeginSample("WindowBase::TurnOff()");
			#endif

			var result = false;
			if (WindowSystem.IsCameraRenderDisableOnWindowTurnOff() == true) {

				if (this.workCamera != null) this.workCamera.enabled = false;
				var canvas = this.GetCanvas();
				if (canvas != null) canvas.enabled = false;
				var scaler = this.GetCanvasScaler();
				if (scaler != null) scaler.enabled = false;
				result = false;

			} else {
				
				if (this.gameObject != null) this.gameObject.SetActive(false);
				result = true;

			}

			#if DEBUGBUILD
			Profiler.EndSample();
			#endif

			return result;

		}

		public void TurnOn() {

			if (this == null) return;

			#if DEBUGBUILD
			Profiler.BeginSample("WindowBase::TurnOn()");
			#endif

			if (WindowSystem.IsCameraRenderDisableOnWindowTurnOff() == true) {
				
				if (this.workCamera != null) this.workCamera.enabled = true;
				var canvas = this.GetCanvas();
				if (canvas != null) canvas.enabled = true;
				var scaler = this.GetCanvasScaler();
				if (scaler != null) scaler.enabled = true;

			} else {

				if (this.gameObject != null) this.gameObject.SetActive(true);

			}

			#if DEBUGBUILD
			Profiler.EndSample();
			#endif

		}

		/// <summary>
		/// Gets the name of the sorting layer.
		/// </summary>
		/// <returns>The sorting layer name.</returns>
		public virtual string GetSortingLayerName() {

			return WindowSystem.GetSortingLayerName();

		}

		/// <summary>
		/// Gets the sorting order.
		/// </summary>
		/// <returns>The sorting order.</returns>
		public virtual int GetSortingOrder() {
			
			return 0;

		}

		/// <summary>
		/// Gets the duration of the animation.
		/// </summary>
		/// <returns>The animation duration.</returns>
		/// <param name="forward">If set to <c>true</c> forward.</param>
		public float GetAnimationDuration(bool forward) {
			
			var layoutDuration = this.GetLayoutAnimationDuration(forward);
			var moduleDuration = this.GetModuleAnimationDuration(forward);
			var transitionDuration = this.GetTransitionAnimationDuration(forward);

			return Mathf.Max(layoutDuration, Mathf.Max(moduleDuration, transitionDuration));

		}

		/// <summary>
		/// Gets the duration of the transition animation.
		/// </summary>
		/// <returns>The transition animation duration.</returns>
		/// <param name="forward">If set to <c>true</c> forward.</param>
		public virtual float GetTransitionAnimationDuration(bool forward) {
			
			return this.transition.GetAnimationDuration(forward);
			
		}

		/// <summary>
		/// Gets the duration of the module animation.
		/// </summary>
		/// <returns>The module animation duration.</returns>
		/// <param name="forward">If set to <c>true</c> forward.</param>
		public virtual float GetModuleAnimationDuration(bool forward) {
			
			return this.modules.GetAnimationDuration(forward);
			
		}

		/// <summary>
		/// Gets the module.
		/// </summary>
		/// <returns>The module.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetModule<T>() where T : WindowModule {

			return this.modules.Get<T>();

		}

		/// <summary>
		/// Show this instance.
		/// </summary>
		public bool Show() {

			return this.Show_INTERNAL(null, null);

		}

		/// <summary>
		/// Show the specified onShowEnd.
		/// </summary>
		/// <param name="onShowEnd">On show end.</param>
		public bool Show(System.Action onShowEnd) {

			return this.Show_INTERNAL(onShowEnd, null);

		}
		
		/// <summary>
		/// Show window with specific transition.
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		public bool Show(AttachItem transitionItem) {
			
			return this.Show_INTERNAL(null, transitionItem);
			
		}

		/// <summary>
		/// Show the specified onShowEnd.
		/// </summary>
		/// <param name="onShowEnd">On show end.</param>
		public bool Show(System.Action onShowEnd, AttachItem transitionItem) {

			return this.Show_INTERNAL(onShowEnd, transitionItem);

		}

		private bool Show_INTERNAL(System.Action onShowEnd, AttachItem transitionItem) {

			if (WindowSystem.IsCallEventsEnabled() == false) {

				return false;

			}
			
			if (this.currentState == WindowObjectState.Showing || this.currentState == WindowObjectState.Shown) {

				return false;

			}

			this.currentState = WindowObjectState.Showing;
			this.eventsHistoryTracker.Add(this, HistoryTrackerEventType.ShowBegin);
			
			WindowSystem.AddToHistory(this);

			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);

			if (this.gameObject.activeSelf == false) this.gameObject.SetActive(true);
			ME.Coroutines.Run(this.Show_INTERNAL_YIELD(onShowEnd, transitionItem));

			return true;

		}

		private System.Collections.Generic.IEnumerator<byte> Show_INTERNAL_YIELD(System.Action onShowEnd, AttachItem transitionItem) {
			
			while (this.paused == true) yield return 0;

			this.ApplyActiveState();
			this.TurnOn();

			var parameters = AppearanceParameters.Default();

			this.showWaitCounter = 0;
			var counter = 0;
			System.Action callback = () => {

				if (this.currentState != WindowObjectState.Showing) return;

				++counter;
				if (counter < 6 + this.showWaitCounter) return;

				#if DEBUGBUILD
				Profiler.BeginSample("WindowBase::OnShowEnd()");
				#endif

				this.DoLayoutShowEnd(parameters);
				this.modules.DoShowEnd(parameters);
				this.audio.DoShowEnd(parameters);
				this.events.DoShowEnd(parameters);
				this.transition.DoShowEnd(parameters);
				(this as IWindowEventsController).DoShowEnd(parameters);
				if (onShowEnd != null) onShowEnd();

				#if DEBUGBUILD
				Profiler.EndSample();
				#endif

				this.currentState = WindowObjectState.Shown;
				this.eventsHistoryTracker.Add(this, HistoryTrackerEventType.ShowEnd);

				WindowSystem.RunSafe(this.OnShowEndLate);

			};
			
			parameters = parameters.ReplaceCallback(callback);

			#if DEBUGBUILD
			Profiler.BeginSample("WindowBase::OnShowBegin()");
			#endif

			(this as IWindowEventsController).DoWindowOpen();

			this.DoLayoutShowBegin(parameters);
			
			this.modules.DoShowBegin(parameters);

			if (transitionItem != null && transitionItem.audioTransition != null) {

				this.audio.DoShowBegin(transitionItem.audioTransition, transitionItem.audioTransitionParameters, parameters);

			} else {

				this.audio.DoShowBegin(parameters);

			}

			this.events.DoShowBegin(parameters);
			
			if (transitionItem != null && transitionItem.transition != null) {
				
				this.transition.DoShowBegin(transitionItem.transition, transitionItem.transitionParameters, parameters);
				
			} else {
				
				this.transition.DoShowBegin(parameters);
				
			}
			
			(this as IWindowEventsController).DoShowBegin(parameters);

			#if DEBUGBUILD
			Profiler.EndSample();
			#endif

		}

		private int showWaitCounter = 0;
		public void AddShowWaitCounter() {

			++this.showWaitCounter;

		}

		private int hideWaitCounter = 0;
		public void AddHideWaitCounter() {

			++this.hideWaitCounter;

		}

		/// <summary>
		/// Hide this instance.
		/// </summary>
		public bool Hide() {
			
			return this.Hide_INTERNAL(onHideEnd: null, transitionItem: null);

		}

		public bool Hide(System.Action onHideEnd, bool immediately) {

			return this.Hide_INTERNAL(onHideEnd: onHideEnd, transitionItem: null, immediately: immediately);

		}

		public bool Hide(bool immediately) {

			return this.Hide_INTERNAL(onHideEnd: null, transitionItem: null, immediately: immediately);

		}

		public bool Hide(bool immediately, bool forced) {

			return this.Hide_INTERNAL(onHideEnd: null, transitionItem: null, immediately: immediately, forced: forced);

		}

		public bool Hide(System.Action onHideEnd, bool immediately, bool forced) {

			return this.Hide_INTERNAL(onHideEnd: onHideEnd, transitionItem: null, immediately: immediately, forced: forced);

		}

		/// <summary>
		/// Hide window with specific transition.
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		public bool Hide(AttachItem transitionItem) {
			
			return this.Hide_INTERNAL(null, transitionItem);
			
		}
		/// <summary>
		/// Hide the specified onHideEnd.
		/// Wait while all components, animations, events and modules return the callback.
		/// </summary>
		/// <param name="onHideEnd">On hide end.</param>
		public bool Hide(System.Action onHideEnd) {

			return this.Hide_INTERNAL(onHideEnd, null);

		}

		/// <summary>
		/// Hide the specified onHideEnd with specific transition.
		/// Wait while all components, animations, events and modules return the callback.
		/// </summary>
		/// <param name="onHideEnd">On hide end.</param>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		public bool Hide(System.Action onHideEnd, AttachItem transitionItem) {

			return this.Hide_INTERNAL(onHideEnd, transitionItem);

		}

		private bool Hide_INTERNAL(System.Action onHideEnd, AttachItem transitionItem, bool immediately = false, bool forced = false) {

			return this.Hide_INTERNAL(onHideEnd, transitionItem, AppearanceParameters.Default().ReplaceImmediately(immediately).ReplaceForced(forced));

		}

		private bool Hide_INTERNAL(System.Action onHideEnd, AttachItem transitionItem, AppearanceParameters parameters) {
			
			if (this == null) {

				return false;

			}

			var forced = parameters.GetForced(defaultValue: false);
			if ((this.currentState == WindowObjectState.Hidden || this.currentState == WindowObjectState.Hiding) && forced == false) {

				return false;

			}

			this.currentState = WindowObjectState.Hiding;
			this.eventsHistoryTracker.Add(this, HistoryTrackerEventType.HideBegin);

			//if (this.gameObject.activeSelf == false) this.gameObject.SetActive(true);
			ME.Coroutines.Run(this.Hide_INTERNAL_YIELD(onHideEnd, transitionItem, parameters));

			return true;

		}
		
		private System.Collections.Generic.IEnumerator<byte> Hide_INTERNAL_YIELD(System.Action onHideEnd, AttachItem transitionItem, AppearanceParameters parameters) {

			if (parameters.GetForced(defaultValue: false) == false) {

				while (this.paused == true) yield return 0;

			}

			this.activeIteration = 0;
			this.SetInactive(null);

			if (this.preferences.sendActiveState == true) {

				WindowSystem.SendActiveStateByWindow(this);

			}

			//var parameters = AppearanceParameters.Default();

			//parameters.ReplaceImmediately(immediately);
			//parameters.ReplaceForced(forced);

			this.hideWaitCounter = 0;
			var counter = 0;
			System.Action callback = () => {
				
				if (this.currentState != WindowObjectState.Hiding) return;

				++counter;
				if (counter < 6 + this.hideWaitCounter) return;

				WindowSystem.AddToHistory(this);

				#if DEBUGBUILD
				Profiler.BeginSample("WindowBase::OnHideEnd()");
				#endif

				this.DoLayoutHideEnd(parameters);
				this.modules.DoHideEnd(parameters);
				this.audio.DoHideEnd(parameters);
				this.events.DoHideEnd(parameters);
				this.transition.DoHideEnd(parameters);
				(this as IWindowEventsController).DoHideEnd(parameters);
				if (onHideEnd != null) onHideEnd();

				#if DEBUGBUILD
				Profiler.EndSample();
				#endif

				this.currentState = WindowObjectState.Hidden;
				this.eventsHistoryTracker.Add(this, HistoryTrackerEventType.HideEnd);

				WindowSystem.RunSafe(this.OnHideEndLate);
				this.events.DoHideEndLate(parameters);

				CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

				WindowSystem.Recycle(this, setInactive: this.TurnOff());

			};

			parameters = parameters.ReplaceCallback(callback);

			#if DEBUGBUILD
			Profiler.BeginSample("WindowBase::OnHideBegin()");
			#endif

			(this as IWindowEventsController).DoWindowClose();

			this.DoLayoutHideBegin(parameters);
			
			this.modules.DoHideBegin(parameters);

			if (transitionItem != null && transitionItem.audioTransition != null) {
				
				this.audio.DoHideBegin(transitionItem.audioTransition, transitionItem.audioTransitionParameters, parameters);
				
			} else {
				
				this.audio.DoHideBegin(parameters);
				
			}

			this.events.DoHideBegin(parameters);

			if (transitionItem != null && transitionItem.transition != null) {
				
				this.transition.DoHideBegin(transitionItem.transition, transitionItem.transitionParameters, parameters);
				
			} else {
				
				this.transition.DoHideBegin(parameters);
				
			}
			
			(this as IWindowEventsController).DoHideBegin(parameters);

			#if DEBUGBUILD
			Profiler.EndSample();
			#endif

		}
		
		public void Pause() {

			this.paused = true;

		}
		
		public void Resume() {
			
			this.paused = false;

		}

		public void DoDestroy(System.Action callback) {
			
			if (Application.isPlaying == false) return;

			if (this.GetState() == WindowObjectState.Deinitializing ||
				this.GetState() == WindowObjectState.NotInitialized) {

				callback.Invoke();
				return;

			}

			this.eventsHistoryTracker.Add(this, HistoryTrackerEventType.Deinit);

			#if DEBUGBUILD
			Profiler.BeginSample("WindowBase::OnDeinit()");
			#endif

			this.currentState = WindowObjectState.Deinitializing;

			ME.Utilities.CallInSequence<System.Action<System.Action>>(() => {

				this.currentState = WindowObjectState.NotInitialized;
				if (callback != null) callback.Invoke();

			}, /*waitPrevious:*/ true, (item, c) => {

				item.Invoke(c);

			}, 
				this.DoLayoutDeinit,
				this.modules.DoDeinit,
				this.audio.DoDeinit,
				this.events.DoDeinit,
				this.transition.DoDeinit,
				(this as IWindowEventsController).DoDeinit
			);

			#if DEBUGBUILD
			Profiler.EndSample();
			#endif

		}
		
		public virtual Transform GetLayoutRoot() { return null; }
		protected virtual void MoveLayout(Vector2 delta) {}

		/// <summary>
		/// Gets the duration of the layout animation.
		/// </summary>
		/// <returns>The layout animation duration.</returns>
		/// <param name="forward">If set to <c>true</c> forward.</param>
		public virtual float GetLayoutAnimationDuration(bool forward) {
			
			return 0f;
			
		}

		protected virtual void DoLayoutWindowActive() {}
		protected virtual void DoLayoutWindowInactive() {}

		protected virtual void DoLayoutWindowOpen() {}
		protected virtual void DoLayoutWindowClose() {}

		protected virtual void DoLayoutWindowLayoutComplete() {}

		protected virtual void DoLayoutUnload() {}

		/// <summary>
		/// Raises the layout init event.
		/// </summary>
		/// <param name="depth">Depth.</param>
		/// <param name="raycastPriority">Raycast priority.</param>
		/// <param name="orderInLayer">Order in layer.</param>
		protected virtual void DoLayoutInit(float depth, int raycastPriority, int orderInLayer, System.Action callback, bool async) {}

		/// <summary>
		/// Raises the layout deinit event.
		/// </summary>
		protected virtual void DoLayoutDeinit(System.Action callback) { callback.Invoke(); }

		/// <summary>
		/// Raises the layout show begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		protected virtual void DoLayoutShowBegin(AppearanceParameters parameters) { parameters.Call(); }

		/// <summary>
		/// Raises the layout show end event.
		/// </summary>
		protected virtual void DoLayoutShowEnd(AppearanceParameters parameters) {}

		/// <summary>
		/// Raises the layout hide begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		protected virtual void DoLayoutHideBegin(AppearanceParameters parameters) { parameters.Call(); }

		/// <summary>
		/// Raises the layout hide end event.
		/// </summary>
		protected virtual void DoLayoutHideEnd(AppearanceParameters parameters) {}

		/// <summary>
		/// Raises the parameters pass event.
		/// Don't override this method - use your own.
		/// Window will use reflection to determine your method.
		/// Example: OnParametersPass(T1 param1, T2 param2, etc.)
		/// You can use any types in any order and call window with them.
		/// </summary>
		[CompilerIgnore]
		public virtual void OnParametersPass() {}

		[CompilerIgnore]
		public void OnParametersPass(params object[] objects) {

			throw new UnityException(string.Format("OnParametersPass is not valid for screen `{0}`", this.name));

		}

		/// <summary>
		/// Raises the empty pass event.
		/// </summary>
		public virtual void OnEmptyPass() {}

		public virtual void OnPreInit() {}

		void IWindowEventsController.DoWindowOpen() {

			this.DoLayoutWindowOpen();
			(this.modules as IWindowEventsController).DoWindowOpen();
			(this.audio as IWindowEventsController).DoWindowOpen();
			(this.events as IWindowEventsController).DoWindowOpen();
			(this.transition as IWindowEventsController).DoWindowOpen();
			WindowSystem.RunSafe(this.OnWindowOpen);

		}

		void IWindowEventsController.DoWindowClose() {

			this.DoLayoutWindowClose();
			(this.modules as IWindowEventsController).DoWindowClose();
			(this.audio as IWindowEventsController).DoWindowClose();
			(this.events as IWindowEventsController).DoWindowClose();
			(this.transition as IWindowEventsController).DoWindowClose();
			WindowSystem.RunSafe(this.OnWindowClose);

		}

		/// <summary>
		/// Raises the init event.
		/// </summary>
		void IWindowEventsController.DoInit() {

			WindowSystem.RunSafe(this.OnInit);

		}

		/// <summary>
		/// Raises the deinit event.
		/// </summary>
		void IWindowEventsController.DoDeinit(System.Action callback) {
			
			WindowSystem.RunSafe(this.OnDeinit, callback);

		}

		/// <summary>
		/// Raises the show begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		void IWindowEventsController.DoShowBegin(AppearanceParameters parameters) {

			WindowSystem.RunSafe(this.OnShowBegin);
			WindowSystem.RunSafe(this.OnShowBegin, parameters);

			parameters.Call();

		}

		/// <summary>
		/// Raises the show end event.
		/// </summary>
		void IWindowEventsController.DoShowEnd(AppearanceParameters parameters) {

			WindowSystem.RunSafe(this.OnShowEnd);
			WindowSystem.RunSafe(this.OnShowEnd, parameters);

		}

		/// <summary>
		/// Raises the hide begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		void IWindowEventsController.DoHideBegin(AppearanceParameters parameters) {

			WindowSystem.RunSafe(this.OnHideBegin);
			WindowSystem.RunSafe(this.OnHideBegin, parameters);

			parameters.Call();

		}

		/// <summary>
		/// Raises the hide end event.
		/// </summary>
		void IWindowEventsController.DoHideEnd(AppearanceParameters parameters) {
			
			WindowSystem.RunSafe(this.OnHideEnd);
			WindowSystem.RunSafe(this.OnHideEnd, parameters);

		}

		public virtual void OnWindowOpen() {}
		public virtual void OnWindowClose() {}

		public virtual void OnInit() {}
		//public virtual void OnDeinit() {}
		
		public virtual void OnShowEnd() {}
		public virtual void OnShowEnd(AppearanceParameters parameters) {}

		public virtual void OnHideEnd() {}
		public virtual void OnHideEnd(AppearanceParameters parameters) {}
		
		public virtual void OnShowBegin() {}
		public virtual void OnShowBegin(AppearanceParameters parameters) {}
		
		public virtual void OnHideBegin() {}
		public virtual void OnHideBegin(AppearanceParameters parameters) {}

		public virtual void OnWindowUnload() {}

		/// <summary>
		/// Fires after all states are Shown.
		/// </summary>
		public virtual void OnShowEndLate() {}

		/// <summary>
		/// Fires after all states are Hidden.
		/// </summary>
		public virtual void OnHideEndLate() {}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			this.preferences.OnValidate();

			this.SetupCamera();

		}

		private void SetupCamera() {
			
			this.workCamera = ME.Utilities.FindReferenceChildren<Camera>(this);
			if (this.workCamera == null) {
				
				this.workCamera = this.gameObject.AddComponent<Camera>();
				
			}
			
			if (this.workCamera != null) {

				// Camera
				if (this.preferences.overrideCameraSettings == true) {

					this.preferences.cameraSettings.Apply(this.workCamera);

				} else {
					
					WindowSystem.ApplyToSettings(this.workCamera);

				}

				if ((this.workCamera.cullingMask & (1 << this.gameObject.layer)) == 0) {

					this.workCamera.cullingMask = 0x0;
					this.workCamera.cullingMask |= 1 << this.gameObject.layer;

				}

				this.workCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);

			}
			
			this.isReady = (this.workCamera != null);
			
		}

		[ContextMenu("Create on Scene")]
		public void CreateOnScene() {

			this.CreateOnScene(callEvents: true);

		}

		public void CreateOnScene(bool callEvents) {

			if (callEvents == false) {

				WindowSystem.DisableCallEvents();

			}

			WindowBase window = null;

			try {

				window = WindowSystem.Show<WindowBase>(source: this);

			} catch (UnityException) {
			} finally {

				if (window != null) {
					
					var selection = new List<GameObject>();
					var layoutWindow = window as UnityEngine.UI.Windows.Types.LayoutWindowType;
					if (layoutWindow != null) {

						var currentLayout = layoutWindow.layouts.layouts[0];
						foreach (var component in currentLayout.components) {
							
							var compInstance = currentLayout.Get<WindowComponent>(component.tag);
							if (compInstance != null) selection.Add(compInstance.gameObject);
							
						}
						
					}
					
					if (window != null) {
						
						selection.Add(window.gameObject);
						
					}
					
					UnityEditor.Selection.objects = selection.ToArray();
					
					if (selection.Count > 0) {
						
						if (UnityEditor.SceneView.currentDrawingSceneView != null) {
							
							UnityEditor.SceneView.currentDrawingSceneView.AlignViewToObject(selection[0].transform);
							
						}
						
					}
					
				} else {
					
					Debug.LogError("Create window on scene failed. May be WindowSystem is not exist on scene.");
					
				}

			}

			if (callEvents == false) {
				
				WindowSystem.RestoreCallEvents();
				
			}

		}
		#endif

	}

}