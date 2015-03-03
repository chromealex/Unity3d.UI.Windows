using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows {

	public enum WindowState : byte {

		NotInitialized = 0,
		Initializing,
		Initialized,

		Showing,
		Shown,

		Hiding,
		Hided,

	}

	[ExecuteInEditMode()]
	[RequireComponent(typeof(Camera))]
	public class WindowBase : WindowObject, IWindowEvents {

		[HideInInspector]
		public Camera workCamera;
		[HideInInspector]
		public bool initialized = false;

		public Preferences preferences;
		public Transition transition;
		public Modules modules;
		public Events events;
		
		[HideInInspector]
		private bool setup = false;
		[HideInInspector]
		private bool passParams = false;
		[HideInInspector]
		private object[] parameters;

		private WindowState currentState = WindowState.NotInitialized;

		internal void Init(float depth, float zDepth, int raycastPriority, int orderInLayer) {
			
			this.currentState = WindowState.Initializing;

			if (this.initialized == false) {
				
				this.currentState = WindowState.NotInitialized;

				Debug.LogError("Can't initialize window instance because of some components was not installed properly.");
				return;

			}

			var pos = this.transform.position;
			pos.z = zDepth;
			this.transform.position = pos;

			this.workCamera.depth = depth;
			if (this.preferences.dontDestroyOnLoad == true) GameObject.DontDestroyOnLoad(this.gameObject);

			if (this.setup == false) {
				
				this.Setup(this);

				this.OnTransitionInit();
				this.OnLayoutInit(depth, raycastPriority, orderInLayer);
				this.OnModulesInit();
				this.OnInit();

				this.setup = true;

			}
			
			if (this.passParams == true) {
				
				var method = this.GetType().GetMethod("OnParametersPass");
				if (method != null) {
					
					method.Invoke(this, this.parameters);
					
				}
				
			} else {
				
				this.OnEmptyPass();
				
			}
			
			this.currentState = WindowState.Initialized;

		}
		
		internal void SetParameters(params object[] parameters) {

			if (parameters != null && parameters.Length > 0) {

				this.parameters = parameters;
				this.passParams = true;

			} else {

				this.passParams = false;

			}

		}
		
		public WindowState GetState() {
			
			return this.currentState;
			
		}
		
		public void OnRenderImage(RenderTexture source, RenderTexture destination) {
			
			this.transition.OnRenderImage(source, destination);
			
		}
		
		public void OnPostRender() {
			
			this.transition.OnPostRender();
			
		}
		
		public void OnPreRender() {
			
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

		public virtual string GetSortingLayerName() {

			return string.Empty;

		}

		public virtual int GetSortingOrder() {
			
			return 0;

		}
		
		public float GetAnimationDuration(bool forward) {
			
			var layoutDuration = this.GetLayoutAnimationDuration(forward);
			var moduleDuration = this.GetModuleAnimationDuration(forward);
			var transitionDuration = this.GetTransitionAnimationDuration(forward);

			return Mathf.Max(layoutDuration, Mathf.Max(moduleDuration, transitionDuration));

		}
		
		public virtual float GetTransitionAnimationDuration(bool forward) {
			
			return this.transition.GetAnimationDuration(forward);
			
		}

		public virtual float GetModuleAnimationDuration(bool forward) {
			
			return this.modules.GetAnimationDuration(forward);
			
		}

		public T GetModule<T>() where T : WindowModule {

			return this.modules.Get<T>();

		}

		public void Show() {
			
			if (this.currentState == WindowState.Showing || this.currentState == WindowState.Shown) return;
			this.currentState = WindowState.Showing;
			
			WindowSystem.AddToHistory(this);

			var counter = 0;
			System.Action callback = () => {

				if (this.currentState != WindowState.Showing) return;

				++counter;
				if (counter < 3) return;

				this.OnShowEnd();
				this.OnLayoutShowEnd();
				this.modules.OnShowEnd();
				this.events.OnShowEnd();
				this.transition.OnShowEnd();

			    this.currentState = WindowState.Shown;

			};

			this.OnLayoutShowBegin(callback);
			this.modules.OnShowBegin(callback);
			this.transition.OnShowBegin(callback);
			this.events.OnShowBegin();
			this.OnShowBegin();

			this.gameObject.SetActive(true);
			
		}
		
		public void Hide() {
			
			this.Hide(null);
			
		}

		public void Hide(System.Action onHideEnd) {

			if (this.currentState == WindowState.Hided || this.currentState == WindowState.Hiding) return;
			this.currentState = WindowState.Hiding;
			
			var counter = 0;
			System.Action callback = () => {
				
				if (this.currentState != WindowState.Hiding) return;

				++counter;
				if (counter < 3) return;
				
				WindowSystem.RemoveFromHistory(this);

				this.Recycle();
				
				this.OnHideEnd();
				this.OnLayoutHideEnd();
				this.modules.OnHideEnd();
				this.events.OnHideEnd();
				this.transition.OnHideEnd();
				if (onHideEnd != null) onHideEnd();
				
				this.events.Clear();
						
				this.currentState = WindowState.Hided;

			};

			this.OnLayoutHideBegin(callback);
			this.modules.OnHideBegin(callback);
			this.transition.OnHideBegin(callback);
			this.events.OnHideBegin();
			this.OnHideBegin();
			
		}

		public void OnDestroy() {

			if (Application.isPlaying == false) return;

			this.OnLayoutDeinit();
			this.events.OnDeinit();
			this.modules.OnDeinit();
			this.transition.OnDeinit();

			this.events.OnDestroy();

			this.OnDeinit();

		}
		
		protected virtual Transform GetLayoutRoot() { return null; }
		public virtual float GetLayoutAnimationDuration(bool forward) {
			
			return 0f;
			
		}

		protected virtual void OnLayoutInit(float depth, int raycastPriority, int orderInLayer) {}
		protected virtual void OnLayoutDeinit() {}
		protected virtual void OnLayoutShowBegin(System.Action callback) { if (callback != null) callback(); }
		protected virtual void OnLayoutShowEnd() {}
		protected virtual void OnLayoutHideBegin(System.Action callback) { if (callback != null) callback(); }
		protected virtual void OnLayoutHideEnd() {}

		public virtual void OnEmptyPass() {}
		public virtual void OnInit() {}
		public virtual void OnDeinit() {}
		public virtual void OnShowBegin() {}
		public virtual void OnShowEnd() {}
		public virtual void OnHideBegin() {}
		public virtual void OnHideEnd() {}
		
		#if UNITY_EDITOR
		protected virtual void Update() {

			if (Application.isPlaying == true) return;

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