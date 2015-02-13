using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows {
	
	[System.Serializable]
	public class Modules : IWindowEventsAsync {
		
		[System.Serializable]
		public class ModuleInfo : IWindowEventsAsync {

			public WindowModule moduleSource;
			public int sortingOrder;
			public bool backgroundLayer;

			[HideInInspector][System.NonSerialized]
			private IWindowAnimation instance;

			public void Create(WindowBase window, Transform modulesRoot) {

				var instance = this.moduleSource.Spawn();
				instance.transform.SetParent(modulesRoot);
				instance.transform.localPosition = Vector3.zero;
				instance.transform.localRotation = Quaternion.identity;
				instance.transform.localScale = Vector3.one;

				var rect = instance.transform as RectTransform;
				rect.sizeDelta = (this.moduleSource.transform as RectTransform).sizeDelta;
				rect.anchoredPosition = (this.moduleSource.transform as RectTransform).anchoredPosition;

				instance.transform.SetSiblingIndex(this.backgroundLayer == true ? -this.sortingOrder : this.sortingOrder + 1);

				instance.Setup(window);

				this.instance = instance;

			}

			public float GetDuration(bool forward) {

				return this.instance.GetAnimationDuration(forward);

			}

			public T Get<T>() where T : WindowModule {
				
				return this.instance as T;
				
			}

			public void OnInit() { if (this.instance != null) this.instance.OnInit(); }
			public void OnDeinit() { if (this.instance != null) this.instance.OnDeinit(); }
			public void OnShowBegin(System.Action callback) { if (this.instance != null) this.instance.OnShowBegin(callback); }
			public void OnShowEnd() { if (this.instance != null) this.instance.OnShowEnd(); }
			public void OnHideBegin(System.Action callback) { if (this.instance != null) this.instance.OnHideBegin(callback); }
			public void OnHideEnd() { if (this.instance != null) this.instance.OnHideEnd(); }

		}

		public ModuleInfo[] elements;

		public void Create(WindowBase window, Transform modulesRoot) {

			foreach (var element in this.elements) {

				element.Create(window, modulesRoot);

			}

		}

		public float GetAnimationDuration(bool forward) {
			
			var maxDuration = 0f;
			foreach (var element in this.elements) {
				
				var d = element.GetDuration(forward);
				if (d >= maxDuration) {
					
					maxDuration = d;
					
				}
				
			}
			
			return maxDuration;
			
		}

		public T Get<T>() where T : WindowModule {

			foreach (var element in this.elements) {

				var item = element.Get<T>();
				if (item != null) return item;

			}

			return default(T);

		}

		// Events
		public void OnInit() { foreach (var element in this.elements) element.OnInit(); }
		public void OnDeinit() { foreach (var element in this.elements) element.OnDeinit(); }
		public void OnShowBegin(System.Action callback) {
			
			ModuleInfo callbacker = null;
			var maxDuration = 0f;
			foreach (var element in this.elements) {
				
				var d = element.GetDuration(true);
				if (d >= maxDuration) {
					
					maxDuration = d;
					callbacker = element;
					
				}
				
			}
			
			foreach (var element in this.elements) {
				
				if (callbacker == element) {
					
					element.OnShowBegin(callback);
					
				} else {
					
					element.OnShowBegin(null);
					
				}
				
			}
			
		}
		public void OnShowEnd() { foreach (var element in this.elements) element.OnShowEnd(); }
		public void OnHideBegin(System.Action callback) {
			
			ModuleInfo callbacker = null;
			var maxDuration = 0f;
			foreach (var element in this.elements) {
				
				var d = element.GetDuration(true);
				if (d >= maxDuration) {
					
					maxDuration = d;
					callbacker = element;
					
				}
				
			}
			
			foreach (var element in this.elements) {
				
				if (callbacker == element) {
					
					element.OnHideBegin(callback);
					
				} else {
					
					element.OnHideBegin(null);
					
				}
				
			}
			
		}
		public void OnHideEnd() { foreach (var element in this.elements) element.OnHideEnd(); }

	}

	[System.Serializable]
	public class Events : IWindowEvents {

		public EventsAction<WindowEventType> events = new EventsAction<WindowEventType>();
		public EventsAction<WindowEventType> eventsInstance = new EventsAction<WindowEventType>();

		public void Clear() {

			this.events.Clear();

		}

		public void OnDestroy() {

			this.eventsInstance.Clear();

		}

		private void ReleaseEvents(WindowEventType eventType) {
			
			this.events.Call(eventType);
			this.eventsInstance.Call(eventType);

		}

		// Events
		public void OnInit() { this.ReleaseEvents(WindowEventType.OnInit); }
		public void OnDeinit() { this.ReleaseEvents(WindowEventType.OnDeinit); }
		public void OnShowBegin() { this.ReleaseEvents(WindowEventType.OnShowBegin); }
		public void OnShowEnd() { this.ReleaseEvents(WindowEventType.OnShowEnd); }
		public void OnHideBegin() { this.ReleaseEvents(WindowEventType.OnHideBegin); }
		public void OnHideEnd() { this.ReleaseEvents(WindowEventType.OnHideEnd); }

	}

	[System.Serializable]
	public class Preferences {

		public enum Depth {
			Auto = 0,
			AlwaysTop = 1,
			AlwaysBack = 2,
		};

		public Depth depth;

	}

	[ExecuteInEditMode()]
	[RequireComponent(typeof(Camera))]
	public class WindowBase : WindowObject, IWindowEvents {

		[HideInInspector][SerializeField]
		public Camera workCamera;
		[HideInInspector][SerializeField]
		public bool initialized = false;

		public Preferences preferences;
		public Modules modules;
		public Events events;

		private bool setup = false;
		private bool passParams = false;
		private object[] parameters;

		internal void Init(float depth, int raycastPriority, int orderInLayer) {

			if (this.initialized == false) {

				Debug.LogError("Can't initialize window instance because of some components was not installed properly.");
				return;

			}

			this.workCamera.depth = depth;

			if (this.setup == false) {
				
				this.Setup(this);

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

		}
		
		internal void SetParameters(params object[] parameters) {

			if (parameters != null && parameters.Length > 0) {

				this.parameters = parameters;
				this.passParams = true;

			} else {

				this.passParams = false;

			}

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

			return Mathf.Max(layoutDuration, moduleDuration);

		}
		
		public virtual float GetModuleAnimationDuration(bool forward) {
			
			return this.modules.GetAnimationDuration(forward);
			
		}

		public T GetModule<T>() where T : WindowModule {

			return this.modules.Get<T>();

		}

		public void Show() {

			System.Action callback = () => {
				
				this.OnShowEnd();
				this.OnLayoutShowEnd();
				this.modules.OnShowEnd();
				this.events.OnShowEnd();
				
			};

			var layoutCallback = (this.GetLayoutAnimationDuration(true) >= this.GetModuleAnimationDuration(true));

			this.OnLayoutShowBegin(layoutCallback == true ? callback : null);
			this.modules.OnShowBegin(layoutCallback == false ? callback : null);
			this.events.OnShowBegin();
			this.OnShowBegin();

			this.gameObject.SetActive(true);

		}
		
		public void Hide() {
			
			this.Hide(null);
			
		}

		public void Hide(System.Action onHideEnd) {

			System.Action callback = () => {
				
				this.Recycle();
				
				this.OnHideEnd();
				this.OnLayoutHideEnd();
				this.modules.OnHideEnd();
				this.events.OnHideEnd();
				if (onHideEnd != null) onHideEnd();
				
				this.events.Clear();
				
			};
			
			var layoutCallback = (this.GetLayoutAnimationDuration(false) >= this.GetModuleAnimationDuration(false));

			this.OnLayoutHideBegin(layoutCallback == true ? callback : null);
			this.modules.OnHideBegin(layoutCallback == false ? callback : null);
			this.events.OnHideBegin();
			this.OnHideBegin();

		}

		public void OnDestroy() {

			if (Application.isPlaying == false) return;

			this.OnLayoutDeinit();
			this.events.OnDeinit();
			this.modules.OnDeinit();

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
				this.workCamera.orthographic = true;
				this.workCamera.orthographicSize = 5f;
				this.workCamera.nearClipPlane = -100f;
				this.workCamera.farClipPlane = 100f;
				this.workCamera.useOcclusionCulling = false;
				this.workCamera.hdr = false;
				
				this.workCamera.cullingMask = 0x0;
				this.workCamera.cullingMask |= 1 << this.gameObject.layer;
				
			}
			
			this.initialized = (this.workCamera != null);
			
		}
		#endif

	}

}