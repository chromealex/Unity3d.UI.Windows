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
	[RequireComponent(typeof(Camera))]
	public abstract class WindowBase : WindowObject, IWindowEventsAsync, IWindowEventsController, IFunctionIteration, IWindow, IBeginDragHandler, IDragHandler, IEndDragHandler {

		#if UNITY_EDITOR
		[HideInInspector]
		public bool editorInfoFold = false;
		#endif

		[HideInInspector]
		public Camera workCamera;
		[HideInInspector]
		public bool initialized = false;

		public Preferences preferences = new Preferences();
		new public Audio.Window audio = new Audio.Window();
		public Modules.Modules modules = new Modules.Modules();
		public Events events = new Events();
		#if !TRANSITION_ENABLED
		[ReadOnly]
		#endif
		public Transition transition;

		[SerializeField][HideInInspector]
		private ActiveState activeState = ActiveState.None;
		[SerializeField][HideInInspector]
		private WindowObjectState currentState = WindowObjectState.NotInitialized;
		[SerializeField][HideInInspector]
		private DragState dragState = DragState.None;
		[SerializeField][HideInInspector]
		private bool paused = false;

		private int functionIterationIndex = 0;

		[HideInInspector]
		private bool setup = false;
		[HideInInspector]
		private bool passParams = false;
		[HideInInspector]
		private object[] parameters;
		private System.Action<WindowBase> onParametersPassCall;

		private WindowBase source;

		private GameObject firstSelectedGameObject;
		private GameObject currentSelectedGameObject;

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
		
		public void SetDepth(float depth, float zDepth) {
			
			var pos = this.transform.position;
			pos.z = zDepth;
			this.transform.position = pos;
			
			this.workCamera.depth = depth;
			
		}

		internal void Init(WindowBase source, float depth, float zDepth, int raycastPriority, int orderInLayer) {

			this.source = source;
			this.Init(depth, zDepth, raycastPriority, orderInLayer);

		}

		internal void Init(float depth, float zDepth, int raycastPriority, int orderInLayer) {

			this.currentState = WindowObjectState.Initializing;

			if (this.initialized == false) {
				
				this.currentState = WindowObjectState.NotInitialized;

				Debug.LogError("Can't initialize window instance because of some components were not installed properly.", this);
				return;

			}

			this.SetDepth(depth, zDepth);

			if (this.preferences.IsDontDestroyOnSceneChange() == true) {

				GameObject.DontDestroyOnLoad(this.gameObject);

			}

			if (this.passParams == true) {

				if (this.parameters != null && this.parameters.Length > 0) {

					System.Reflection.MethodInfo methodInfo;
					if (WindowSystem.InvokeMethodWithParameters(out methodInfo, this, "OnParametersPass", this.parameters) == true) {

						// Success
						methodInfo.Invoke(this, this.parameters);

					} else {

						// Method not found
						Debug.LogWarning("Method `OnParametersPass` was not found with input parameters.", this);

					}

				}

				if (this.onParametersPassCall != null) this.onParametersPassCall(this);

			} else {
				
				this.OnEmptyPass();
				
			}

			if (this.setup == false) {

				this.Setup(this);
				
				this.OnPreInit();

				this.DoLayoutInit(depth, raycastPriority, orderInLayer);
				WindowSystem.ApplyToSettingsInstance(this.workCamera, this.GetCanvas());

				this.OnModulesInit();
				this.OnAudioInit();
				this.events.DoInit();
				this.OnTransitionInit();

				if (WindowSystem.IsCallEventsEnabled() == true) {
					
					this.OnInit();
					
				}

				this.setup = true;

			}

			this.currentState = WindowObjectState.Initialized;

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

		/// <summary>
		/// Gets the state.
		/// </summary>
		/// <returns>The state.</returns>
		public WindowObjectState GetState() {
			
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

		public void SetActive() {

			if (this.activeState != ActiveState.Active) {

				this.activeState = ActiveState.Active;
				this.OnActive();

			}

		}

		public void SetInactive() {
			
			if (this.activeState != ActiveState.Inactive) {
				
				this.activeState = ActiveState.Inactive;
				this.OnInactive();
				
			}

		}

		public virtual void OnActive() {

			if (this.preferences.restoreSelectedElement == true && EventSystem.current != null) {

				EventSystem.current.firstSelectedGameObject = this.firstSelectedGameObject;
				EventSystem.current.SetSelectedGameObject(this.currentSelectedGameObject);

			}

		}

		public virtual void OnInactive() {
			
			if (this.preferences.restoreSelectedElement == true && EventSystem.current != null) {

				this.firstSelectedGameObject = EventSystem.current.firstSelectedGameObject;
				this.currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;

			}

		}

		public virtual void OnVersionChanged() {
		}

		public virtual void OnLocalizationChanged() {
		}

		public virtual void OnManualEvent<T>(T data) where T : IManualEvent {
		}

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
		public void Show() {

			this.Show_INTERNAL(null, null);

		}

		/// <summary>
		/// Show the specified onShowEnd.
		/// </summary>
		/// <param name="onShowEnd">On show end.</param>
		public void Show(System.Action onShowEnd) {

			this.Show_INTERNAL(onShowEnd, null);

		}
		
		/// <summary>
		/// Show window with specific transition.
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		public void Show(AttachItem transitionItem) {
			
			this.Show_INTERNAL(null, transitionItem);
			
		}
		
		[System.Obsolete("Use `Tools->Compile UI` command to fix this issue.")]
		public void Show(TransitionBase transition, TransitionInputParameters transitionParams) {
			
			this.Show_INTERNAL(onShowEnd: null, transitionItem: null);
			
		}

		/// <summary>
		/// Show the specified onShowEnd.
		/// </summary>
		/// <param name="onShowEnd">On show end.</param>
		public void Show(System.Action onShowEnd, AttachItem transitionItem) {

			this.Show_INTERNAL(onShowEnd, transitionItem);

		}

		private void Show_INTERNAL(System.Action onShowEnd, AttachItem transitionItem) {

			if (WindowSystem.IsCallEventsEnabled() == false) {

				return;

			}
			
			if (this.currentState == WindowObjectState.Showing || this.currentState == WindowObjectState.Shown) return;
			this.currentState = WindowObjectState.Showing;
			
			WindowSystem.AddToHistory(this);

			if (this.gameObject.activeSelf == false) this.gameObject.SetActive(true);
			this.StartCoroutine(this.Show_INTERNAL_YIELD(onShowEnd, transitionItem));

		}

		private IEnumerator Show_INTERNAL_YIELD(System.Action onShowEnd, AttachItem transitionItem) {

			while (this.paused == true) yield return false;
			
			this.gameObject.SetActive(true);

			var parameters = AppearanceParameters.Default();

			var counter = 0;
			System.Action callback = () => {

				if (this.currentState != WindowObjectState.Showing) return;

				++counter;
				if (counter < 6) return;

				this.DoLayoutShowEnd(parameters);
				this.modules.DoShowEnd(parameters);
				this.audio.DoShowEnd(parameters);
				this.events.DoShowEnd(parameters);
				this.transition.DoShowEnd(parameters);
				this.DoShowEnd(parameters);
				if (onShowEnd != null) onShowEnd();

			    this.currentState = WindowObjectState.Shown;

			};
			
			parameters = parameters.ReplaceCallback(callback);

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
			
			this.DoShowBegin(parameters);

		}

		/// <summary>
		/// Hide this instance.
		/// </summary>
		public bool Hide() {
			
			return this.Hide_INTERNAL(onHideEnd: null, transitionItem: null);

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

		[System.Obsolete("Use `Tools->Compile UI` command to fix this issue.")]
		public bool Hide(TransitionBase transition, TransitionInputParameters transitionParams) {
			
			return this.Hide_INTERNAL(onHideEnd: null, transitionItem: null);

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

		private bool Hide_INTERNAL(System.Action onHideEnd, AttachItem transitionItem) {

			if (this.currentState == WindowObjectState.Hidden || this.currentState == WindowObjectState.Hiding) return false;
			this.currentState = WindowObjectState.Hiding;
			
			if (this.gameObject.activeSelf == false) this.gameObject.SetActive(true);
			this.StartCoroutine(this.Hide_INTERNAL_YIELD(onHideEnd, transitionItem));

			return true;

		}
		
		private IEnumerator Hide_INTERNAL_YIELD(System.Action onHideEnd, AttachItem transitionItem) {
			
			while (this.paused == true) yield return false;

			var parameters = AppearanceParameters.Default();

			var counter = 0;
			System.Action callback = () => {
				
				if (this.currentState != WindowObjectState.Hiding) return;

				++counter;
				if (counter < 6) return;
				
				WindowSystem.AddToHistory(this);

				this.Recycle();

				this.DoLayoutHideEnd(parameters);
				this.modules.DoHideEnd(parameters);
				this.audio.DoHideEnd(parameters);
				this.events.DoHideEnd(parameters);
				this.transition.DoHideEnd(parameters);
				this.DoHideEnd(parameters);
				if (onHideEnd != null) onHideEnd();

				this.currentState = WindowObjectState.Hidden;

				if (this != null && this.gameObject != null) {

					this.gameObject.SetActive(false);

				}

			};

			parameters = parameters.ReplaceCallback(callback);

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
			
			this.DoHideBegin(parameters);

			yield return true;

		}
		
		public void Pause() {

			this.paused = true;

		}
		
		public void Resume() {
			
			this.paused = false;

		}

		private void OnDestroy() {

			if (Application.isPlaying == false) return;

			this.DoLayoutDeinit();
			this.modules.DoDeinit();
			this.audio.DoDeinit();
			this.events.DoDeinit();
			this.transition.DoDeinit();
			this.OnDeinit();

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

		/// <summary>
		/// Raises the layout init event.
		/// </summary>
		/// <param name="depth">Depth.</param>
		/// <param name="raycastPriority">Raycast priority.</param>
		/// <param name="orderInLayer">Order in layer.</param>
		protected virtual void DoLayoutInit(float depth, int raycastPriority, int orderInLayer) {}

		/// <summary>
		/// Raises the layout deinit event.
		/// </summary>
		protected virtual void DoLayoutDeinit() {}

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

		/// <summary>
		/// Raises the init event.
		/// </summary>
		public virtual void DoInit() {

			this.OnInit();

		}

		/// <summary>
		/// Raises the deinit event.
		/// </summary>
		public virtual void DoDeinit() {

			this.OnDeinit();

		}
		
		public virtual void OnPreInit() {}

		/// <summary>
		/// Raises the show begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void DoShowBegin(AppearanceParameters parameters) {

			this.OnShowBegin();
			this.OnShowBegin(parameters);
			#pragma warning disable
			this.OnShowBegin(parameters.callback, parameters.resetAnimation);
			#pragma warning restore

			parameters.Call();

		}

		/// <summary>
		/// Raises the show end event.
		/// </summary>
		public virtual void DoShowEnd(AppearanceParameters parameters) {

			this.OnShowEnd();
			this.OnShowEnd(parameters);
			
		}

		/// <summary>
		/// Raises the hide begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void DoHideBegin(AppearanceParameters parameters) {

			this.OnHideBegin();
			this.OnHideBegin(parameters);
			#pragma warning disable
			this.OnHideBegin(parameters.callback, parameters.immediately);
			#pragma warning restore

			parameters.Call();

		}

		/// <summary>
		/// Raises the hide end event.
		/// </summary>
		public virtual void DoHideEnd(AppearanceParameters parameters) {
			
			this.OnHideEnd();
			this.OnHideEnd(parameters);

		}
		
		public virtual void OnInit() {}
		public virtual void OnDeinit() {}
		
		public virtual void OnShowEnd() {}
		public virtual void OnShowEnd(AppearanceParameters parameters) {}

		public virtual void OnHideEnd() {}
		public virtual void OnHideEnd(AppearanceParameters parameters) {}
		
		public virtual void OnShowBegin() {}
		public virtual void OnShowBegin(AppearanceParameters parameters) {}
		
		public virtual void OnHideBegin() {}
		public virtual void OnHideBegin(AppearanceParameters parameters) {}

		[System.Obsolete("Use OnShowBegin with AppearanceParameters or OnShowBegin without parameters")]
		public virtual void OnShowBegin(System.Action callback, bool resetAnimation = true) {}
		[System.Obsolete("Use OnHideBegin with AppearanceParameters or OnHideBegin without parameters")]
		public virtual void OnHideBegin(System.Action callback, bool immediately = false) {}

		#if UNITY_EDITOR
		/// <summary>
		/// Raises the validate event. Editor only.
		/// </summary>
		public virtual void OnValidate() {

			if (Application.isPlaying == true) return;

			this.preferences.OnValidate();

			this.SetupCamera();

		}

		private void SetupCamera() {
			
			this.workCamera = this.GetComponent<Camera>();
			if (this.workCamera == null) {
				
				this.workCamera = this.gameObject.AddComponent<Camera>();
				
			}
			
			if (this.workCamera != null) {

				// Camera
				WindowSystem.ApplyToSettings(this.workCamera);

				if ((this.workCamera.cullingMask & (1 << this.gameObject.layer)) == 0) {

					this.workCamera.cullingMask = 0x0;
					this.workCamera.cullingMask |= 1 << this.gameObject.layer;

				}

				this.workCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);

			}
			
			this.initialized = (this.workCamera != null);
			
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
						
						foreach (var component in layoutWindow.layout.components) {
							
							var compInstance = layoutWindow.layout.Get<WindowComponent>(component.tag);
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