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

namespace UnityEngine.UI.Windows {

	/// <summary>
	/// Window or component object state.
	/// </summary>
	public enum WindowObjectState : byte {

		NotInitialized = 0,
		Initializing,
		Initialized,

		Showing,
		Shown,

		Hiding,
		Hidden,

	}

	[ExecuteInEditMode()]
	[RequireComponent(typeof(Camera))]
	public class WindowBase : WindowObject, IWindowEventsAsync {

		#if UNITY_EDITOR
		[HideInInspector]
		public bool editorInfoFold = false;
		#endif

		[HideInInspector]
		public Camera workCamera;
		[HideInInspector]
		public bool initialized = false;

		public Preferences preferences = new Preferences();
		public Modules.Modules modules = new Modules.Modules();
		public Events events = new Events();
		#if !TRANSITION_ENABLED
		[ReadOnly]
		#endif
		public Transition transition;

		private int functionIterationIndex = 0;

		[HideInInspector]
		private bool setup = false;
		[HideInInspector]
		private bool passParams = false;
		[HideInInspector]
		private object[] parameters;
		private System.Action<WindowBase> onParametersPassCall;

		private WindowObjectState currentState = WindowObjectState.NotInitialized;
		private WindowBase source;
		
		public bool SourceEquals(WindowBase y) {

			if (y == null) return false;
			
			if (this.source == null) return y.source == this;
			if (y.source == null) return y == this.source;

			return this.source == y.source;
			
		}

		internal void Init(WindowBase source, float depth, float zDepth, int raycastPriority, int orderInLayer) {

			this.source = source;
			this.Init(depth, zDepth, raycastPriority, orderInLayer);

		}

		internal void Init(float depth, float zDepth, int raycastPriority, int orderInLayer) {
			
			this.currentState = WindowObjectState.Initializing;

			if (this.initialized == false) {
				
				this.currentState = WindowObjectState.NotInitialized;

				Debug.LogError("Can't initialize window instance because of some components was not installed properly.", this);
				return;

			}

			var pos = this.transform.position;
			pos.z = zDepth;
			this.transform.position = pos;

			this.workCamera.depth = depth;
			if (this.preferences.IsDontDestroySceneChange() == true) {

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

				this.onParametersPassCall(this);

			} else {
				
				this.OnEmptyPass();
				
			}

			if (this.setup == false) {
				
				this.Setup(this);

				this.OnTransitionInit();
				this.OnLayoutInit(depth, raycastPriority, orderInLayer);
				this.OnModulesInit();

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

		private void OnTransitionInit() {
			
			this.workCamera.clearFlags = CameraClearFlags.Depth;
			this.transition.Setup(this);
			this.transition.OnInit();

		}

		private void OnModulesInit() {

			this.modules.Create(this, this.GetLayoutRoot());
			this.modules.OnInit();

		}

		/// <summary>
		/// Gets the name of the sorting layer.
		/// </summary>
		/// <returns>The sorting layer name.</returns>
		public virtual string GetSortingLayerName() {

			return string.Empty;

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

			this.Show(null);

		}

		/// <summary>
		/// Show the specified onShowEnd.
		/// </summary>
		/// <param name="onShowEnd">On show end.</param>
		public void Show(System.Action onShowEnd) {

			this.Show(onShowEnd, null, null);

		}
		
		/// <summary>
		/// Show window with specific transition.
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		public void Show(TransitionBase transition, TransitionInputParameters transitionParameters) {
			
			this.Show(null, transition, transitionParameters);
			
		}

		/// <summary>
		/// Show the specified onShowEnd.
		/// </summary>
		/// <param name="onShowEnd">On show end.</param>
		public void Show(System.Action onShowEnd, TransitionBase transition, TransitionInputParameters transitionParameters) {

			if (WindowSystem.IsCallEventsEnabled() == false) {

				return;

			}
			
			if (this.currentState == WindowObjectState.Showing || this.currentState == WindowObjectState.Shown) return;
			this.currentState = WindowObjectState.Showing;
			
			WindowSystem.AddToHistory(this);

			var counter = 0;
			System.Action callback = () => {

				if (this.currentState != WindowObjectState.Showing) return;

				++counter;
				if (counter < 5) return;

				this.OnShowEnd();
				this.OnLayoutShowEnd();
				this.modules.OnShowEnd();
				this.events.OnShowEnd();
				this.transition.OnShowEnd();
				if (onShowEnd != null) onShowEnd();

				CanvasUpdater.ForceUpdate();

			    this.currentState = WindowObjectState.Shown;

			};

			this.OnLayoutShowBegin(callback);
			this.modules.OnShowBegin(callback);

			if (transition != null) {

				this.transition.OnShowBegin(transition, transitionParameters, callback);

			} else {

				this.transition.OnShowBegin(callback);

			}

			this.events.OnShowBegin(callback);
			this.OnShowBegin(callback);

			this.gameObject.SetActive(true);
			
		}

		/// <summary>
		/// Hide this instance.
		/// </summary>
		public bool Hide() {
			
			return this.Hide(null);
			
		}
		
		/// <summary>
		/// Hide window with specific transition.
		/// </summary>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		public bool Hide(TransitionBase transition, TransitionInputParameters transitionParameters) {
			
			return this.Hide(null, transition, transitionParameters);
			
		}
		/// <summary>
		/// Hide the specified onHideEnd.
		/// Wait while all components, animations, events and modules return the callback.
		/// </summary>
		/// <param name="onHideEnd">On hide end.</param>
		public bool Hide(System.Action onHideEnd) {

			return this.Hide(onHideEnd, null, null);

		}

		/// <summary>
		/// Hide the specified onHideEnd with specific transition.
		/// Wait while all components, animations, events and modules return the callback.
		/// </summary>
		/// <param name="onHideEnd">On hide end.</param>
		/// <param name="transition">Transition.</param>
		/// <param name="transitionParameters">Transition parameters.</param>
		public bool Hide(System.Action onHideEnd, TransitionBase transition, TransitionInputParameters transitionParameters) {

			if (this.currentState == WindowObjectState.Hidden || this.currentState == WindowObjectState.Hiding) return false;
			this.currentState = WindowObjectState.Hiding;
			
			var counter = 0;
			System.Action callback = () => {
				
				if (this.currentState != WindowObjectState.Hiding) return;

				++counter;
				if (counter < 5) return;
				
				WindowSystem.AddToHistory(this);

				this.Recycle();
				
				this.OnHideEnd();
				this.OnLayoutHideEnd();
				this.modules.OnHideEnd();
				this.events.OnHideEnd();
				this.transition.OnHideEnd();
				if (onHideEnd != null) onHideEnd();

				this.currentState = WindowObjectState.Hidden;

			};

			this.OnLayoutHideBegin(callback);
			this.modules.OnHideBegin(callback);
			
			if (transition != null) {
				
				this.transition.OnHideBegin(transition, transitionParameters, callback);
				
			} else {
				
				this.transition.OnHideBegin(callback);
				
			}

			this.events.OnHideBegin(callback);
			this.OnHideBegin(callback, immediately: false);

			return true;

		}

		private void OnDestroy() {

			if (Application.isPlaying == false) return;

			this.OnLayoutDeinit();
			this.events.OnDeinit();
			this.modules.OnDeinit();
			this.transition.OnDeinit();

			this.events.OnDeinit();

			this.OnDeinit();

		}
		
		protected virtual Transform GetLayoutRoot() { return null; }

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
		protected virtual void OnLayoutInit(float depth, int raycastPriority, int orderInLayer) {}

		/// <summary>
		/// Raises the layout deinit event.
		/// </summary>
		protected virtual void OnLayoutDeinit() {}

		/// <summary>
		/// Raises the layout show begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		protected virtual void OnLayoutShowBegin(System.Action callback) { if (callback != null) callback(); }

		/// <summary>
		/// Raises the layout show end event.
		/// </summary>
		protected virtual void OnLayoutShowEnd() {}

		/// <summary>
		/// Raises the layout hide begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		protected virtual void OnLayoutHideBegin(System.Action callback) { if (callback != null) callback(); }

		/// <summary>
		/// Raises the layout hide end event.
		/// </summary>
		protected virtual void OnLayoutHideEnd() {}

		/// <summary>
		/// Raises the parameters pass event.
		/// Don't override this method - use your own.
		/// Window will use reflection to determine your method.
		/// Example: OnParametersPass(T1 param1, T2 param2, etc.)
		/// You can use any types in any order and call window with them.
		/// </summary>
		public virtual void OnParametersPass() {}

		/// <summary>
		/// Raises the empty pass event.
		/// </summary>
		public virtual void OnEmptyPass() {}

		/// <summary>
		/// Raises the init event.
		/// </summary>
		public virtual void OnInit() {}

		/// <summary>
		/// Raises the deinit event.
		/// </summary>
		public virtual void OnDeinit() {}

		/// <summary>
		/// Raises the show begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void OnShowBegin(System.Action callback, bool resetAnimation = true) { if (callback != null) callback(); }

		/// <summary>
		/// Raises the show end event.
		/// </summary>
		public virtual void OnShowEnd() {}

		/// <summary>
		/// Raises the hide begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void OnHideBegin(System.Action callback, bool immediately = false) { if (callback != null) callback(); }

		/// <summary>
		/// Raises the hide end event.
		/// </summary>
		public virtual void OnHideEnd() {}
		
		#if UNITY_EDITOR
		/// <summary>
		/// Raises the validate event. Editor only.
		/// </summary>
		public virtual void OnValidate() {

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