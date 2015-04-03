using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;

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

		[HideInInspector]
		public Camera workCamera;
		[HideInInspector]
		public bool initialized = false;

		public Preferences preferences;
		public Transition transition;
		public Modules.Modules modules;
		public Events events;
		
		[HideInInspector]
		private bool setup = false;
		[HideInInspector]
		private bool passParams = false;
		[HideInInspector]
		private object[] parameters;

		private WindowObjectState currentState = WindowObjectState.NotInitialized;

		internal void Init(float depth, float zDepth, int raycastPriority, int orderInLayer) {
			
			this.currentState = WindowObjectState.Initializing;

			if (this.initialized == false) {
				
				this.currentState = WindowObjectState.NotInitialized;

				Debug.LogError("Can't initialize window instance because of some components was not installed properly.");
				return;

			}

			var pos = this.transform.position;
			pos.z = zDepth;
			this.transform.position = pos;

			this.workCamera.depth = depth;
			if (this.preferences.dontDestroyOnLoad == true) GameObject.DontDestroyOnLoad(this.gameObject);
			
			if (this.passParams == true) {

				System.Reflection.MethodInfo methodInfo;
				if (WindowSystem.InvokeMethodWithParameters(out methodInfo, this, "OnParametersPass", this.parameters) == true) {

					// Success
					methodInfo.Invoke(this, this.parameters);

				} else {

					// Method not found
					Debug.LogWarning("Method `OnParametersPass` was not found with input parameters.");

				}

			} else {
				
				this.OnEmptyPass();
				
			}

			if (this.setup == false) {
				
				this.Setup(this);

				this.OnTransitionInit();
				this.OnLayoutInit(depth, raycastPriority, orderInLayer);
				this.OnModulesInit();
				this.OnInit();

				this.setup = true;

			}

			this.currentState = WindowObjectState.Initialized;

		}

		internal void SetParameters(params object[] parameters) {

			if (parameters != null && parameters.Length > 0) {

				this.parameters = parameters;
				this.passParams = true;

			} else {

				this.passParams = false;

			}

		}

		/// <summary>
		/// Gets the state.
		/// </summary>
		/// <returns>The state.</returns>
		public WindowObjectState GetState() {
			
			return this.currentState;
			
		}
		
		private void OnRenderImage(RenderTexture source, RenderTexture destination) {
			
			this.transition.OnRenderImage(source, destination);
			
		}
		
		private void OnPostRender() {
			
			this.transition.OnPostRender();
			
		}
		
		private void OnPreRender() {
			
			this.transition.OnPreRender();
			
		}

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

			    this.currentState = WindowObjectState.Shown;

			};

			this.OnLayoutShowBegin(callback);
			this.modules.OnShowBegin(callback);
			this.transition.OnShowBegin(callback);
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
		/// Hide the specified onHideEnd.
		/// Wait while all components, animations, events and modules return the callback.
		/// </summary>
		/// <param name="onHideEnd">On hide end.</param>
		public bool Hide(System.Action onHideEnd) {

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
				
				this.events.Clear();
						
				this.currentState = WindowObjectState.Hidden;

			};

			this.OnLayoutHideBegin(callback);
			this.modules.OnHideBegin(callback);
			this.transition.OnHideBegin(callback);
			this.events.OnHideBegin(callback);
			this.OnHideBegin(callback);

			return true;

		}

		private void OnDestroy() {

			if (Application.isPlaying == false) return;

			this.OnLayoutDeinit();
			this.events.OnDeinit();
			this.modules.OnDeinit();
			this.transition.OnDeinit();

			this.events.OnDestroy();

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
		public virtual void OnShowBegin(System.Action callback) { if (callback != null) callback(); }

		/// <summary>
		/// Raises the show end event.
		/// </summary>
		public virtual void OnShowEnd() {}

		/// <summary>
		/// Raises the hide begin event.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void OnHideBegin(System.Action callback) { if (callback != null) callback(); }

		/// <summary>
		/// Raises the hide end event.
		/// </summary>
		public virtual void OnHideEnd() {}
		
		#if UNITY_EDITOR
		/// <summary>
		/// Raises the validate event. Editor only.
		/// </summary>
		public virtual void OnValidate() {

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
				
				this.workCamera.cullingMask = 0x0;
				this.workCamera.cullingMask |= 1 << this.gameObject.layer;
				
				this.workCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);

			}
			
			this.initialized = (this.workCamera != null);
			
		}
		#endif

	}

}