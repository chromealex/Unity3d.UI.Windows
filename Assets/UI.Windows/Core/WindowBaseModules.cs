using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Animations;
using UnityEngine.UI.Windows.Types;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows {

	namespace Modules {

		[System.Serializable]
		public class Modules : IWindowEventsAsync {
			
			[System.Serializable]
			public class ModuleInfo : IWindowEventsAsync {

				public WindowModule moduleSource;
				public int sortingOrder;
				public bool backgroundLayer;

				[HideInInspector][System.NonSerialized]
				private IWindowAnimation instance;

				public ModuleInfo(WindowModule moduleSource, int sortingOrder, bool backgroundLayer) {

					this.moduleSource = moduleSource;
					this.sortingOrder = sortingOrder;
					this.backgroundLayer = backgroundLayer;

				}

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
				public void OnShowBegin(System.Action callback, bool resetAnimation = true) {

					if (this.instance != null) {

						this.instance.OnShowBegin(callback, resetAnimation);

					} else {

						if (callback != null) callback();

					}

				}

				public void OnShowEnd() { if (this.instance != null) this.instance.OnShowEnd(); }
				public void OnHideBegin(System.Action callback, bool immediately = false) {
					
					if (this.instance != null) {
						
						this.instance.OnHideBegin(callback, immediately);
						
					} else {
						
						if (callback != null) callback();
						
					}

				}

				public void OnHideEnd() { if (this.instance != null) this.instance.OnHideEnd(); }
				
			}

			[SerializeField]
			private ModuleInfo[] elements;

	#if UNITY_EDITOR
			public void RemoveModule(ModuleInfo moduleInfo) {

				var list = this.elements.ToList();
				list.Remove(moduleInfo);
				this.elements = list.ToArray();

			}

			public ModuleInfo AddModuleInfo(WindowModule moduleSource, int sortingOrder, bool backgroundLayer) {

				var instance = new ModuleInfo(moduleSource, sortingOrder, backgroundLayer);

				var list = this.elements.ToList();
				list.Add(instance);
				this.elements = list.ToArray();

				return instance;

			}

			public ModuleInfo[] GetModulesInfo() {

				return this.elements;

			}
	#endif

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
			public void OnShowBegin(System.Action callback, bool resetAnimation = true) {
				
				ME.Utilities.CallInSequence(callback, this.elements, (e, c) => { e.OnShowBegin(c, resetAnimation); });

			}
			public void OnShowEnd() { foreach (var element in this.elements) element.OnShowEnd(); }
			public void OnHideBegin(System.Action callback, bool immediately = false) {
				
				ME.Utilities.CallInSequence(callback, this.elements, (e, c) => { e.OnHideBegin(c, immediately); });

			}
			public void OnHideEnd() { foreach (var element in this.elements) element.OnHideEnd(); }
			
		}

	}

	[System.Serializable]
	public class Events : IWindowEventsAsync {

		[System.Serializable]
		public class Element {

			[System.Serializable]
			public class OnInit : UnityEvent {}
			public OnInit onInit = new OnInit();
			
			[System.Serializable]
			public class OnShowBegin : UnityEvent {}
			public OnShowBegin onShowBegin = new OnShowBegin();
			
			[System.Serializable]
			public class OnShowEnd : UnityEvent {}
			public OnShowEnd onShowEnd = new OnShowEnd();
			
			[System.Serializable]
			public class OnHideBegin : UnityEvent {}
			public OnHideBegin onHideBegin = new OnHideBegin();
			
			[System.Serializable]
			public class OnHideEnd : UnityEvent {}
			public OnHideEnd onHideEnd = new OnHideEnd();
			
			[System.Serializable]
			public class OnDeinit : UnityEvent {}
			public OnDeinit onDeinit = new OnDeinit();

			public void Clear() {
				
				this.onInit.RemoveAllListeners();
				this.onShowBegin.RemoveAllListeners();
				this.onShowEnd.RemoveAllListeners();
				this.onHideBegin.RemoveAllListeners();
				this.onHideEnd.RemoveAllListeners();
				this.onDeinit.RemoveAllListeners();

			}
			
			public void Register(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {
					
					case WindowEventType.OnInit:
						this.onInit.AddListener(callback);
						break;
						
					case WindowEventType.OnShowBegin:
						this.onShowBegin.AddListener(callback);
						break;
						
					case WindowEventType.OnShowEnd:
						this.onShowEnd.AddListener(callback);
						break;
						
					case WindowEventType.OnHideBegin:
						this.onHideBegin.AddListener(callback);
						break;
						
					case WindowEventType.OnHideEnd:
						this.onHideEnd.AddListener(callback);
						break;
						
					case WindowEventType.OnDeinit:
						this.onDeinit.AddListener(callback);
						break;
						
				}
				
			}
			
			public void Unregister(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {
					
					case WindowEventType.OnInit:
						this.onInit.RemoveListener(callback);
						break;
						
					case WindowEventType.OnShowBegin:
						this.onShowBegin.RemoveListener(callback);
						break;
						
					case WindowEventType.OnShowEnd:
						this.onShowEnd.RemoveListener(callback);
						break;
						
					case WindowEventType.OnHideBegin:
						this.onHideBegin.RemoveListener(callback);
						break;
						
					case WindowEventType.OnHideEnd:
						this.onHideEnd.RemoveListener(callback);
						break;
						
					case WindowEventType.OnDeinit:
						this.onDeinit.RemoveListener(callback);
						break;
						
				}
				
			}

			public void ReleaseEvents(WindowEventType eventType) {
				
				switch (eventType) {
					
					case WindowEventType.OnInit:
						this.onInit.Invoke();
						break;
						
					case WindowEventType.OnShowBegin:
						this.onShowBegin.Invoke();
						break;
						
					case WindowEventType.OnShowEnd:
						this.onShowEnd.Invoke();
						break;
						
					case WindowEventType.OnHideBegin:
						this.onHideBegin.Invoke();
						break;
						
					case WindowEventType.OnHideEnd:
						this.onHideEnd.Invoke();
						break;
						
					case WindowEventType.OnDeinit:
						this.onDeinit.Invoke();
						break;

				}

			}

		}
		
		public Element onEveryInstance = new Element();
		public Element onType = new Element();

		public void Clear() {
			
			this.onType.Clear();
			
		}
		
		public void OnDestroy() {
			
			this.onEveryInstance.Clear();
			
		}
		
		private void ReleaseEvents(WindowEventType eventType) {

			this.onType.ReleaseEvents(eventType);
			this.onEveryInstance.ReleaseEvents(eventType);
			
		}
		
		// Events
		public void OnInit() { this.ReleaseEvents(WindowEventType.OnInit); }
		public void OnDeinit() { this.ReleaseEvents(WindowEventType.OnDeinit); }
		public void OnShowBegin(System.Action callback, bool resetAnimation = true) { this.ReleaseEvents(WindowEventType.OnShowBegin); if (callback != null) callback(); }
		public void OnShowEnd() { this.ReleaseEvents(WindowEventType.OnShowEnd); }
		public void OnHideBegin(System.Action callback, bool immediately = false) { this.ReleaseEvents(WindowEventType.OnHideBegin); if (callback != null) callback(); }
		public void OnHideEnd() { this.ReleaseEvents(WindowEventType.OnHideEnd); }
		
	}
	
	[System.Serializable]
	public class Preferences {
		
		public enum Depth : byte {
			Auto = 0,
			AlwaysTop = 1,
			AlwaysBack = 2,
		};

		public enum DontDestroy : byte {
			Auto = 0x0,
			OnSceneChange = 0x1,
			OnClean = 0x2,
			OnCleanAndSceneChange = OnSceneChange | OnClean,
		};

		public Depth depth;
		[HideInInspector]
		public bool dontDestroyOnLoad = true;
		[HideInInspector]
		public bool editorValidate = false;

		public DontDestroy dontDestroy = DontDestroy.OnSceneChange;

		public bool IsDontDestroyAuto() {
			
			return (this.dontDestroy & DontDestroy.Auto) != 0;
			
		}
		
		public bool IsDontDestroySceneChange() {
			
			return (this.dontDestroy & DontDestroy.OnSceneChange) != 0;
			
		}
		
		public bool IsDontDestroyClean() {
			
			return (this.dontDestroy & DontDestroy.OnClean) != 0;
			
		}
		
		#if UNITY_EDITOR
		public void OnValidate() {
			
			if (this.editorValidate == false) {
				
				this.dontDestroy = this.dontDestroyOnLoad == true ? DontDestroy.OnSceneChange : DontDestroy.Auto;
				this.editorValidate = true;
				
			}
			
		}
		#endif

	}
	
	[System.Serializable]
	public class Transition : IWindowEventsAsync {
		
		public TransitionBase transition;
		
		[HideInInspector]
		private WindowBase window;
		
		private bool renderingToTexture = false;

		public void Setup(WindowBase window) {
			
			this.window = window;
			
			if (this.transition!= null) {
				
				this.transition.SetupCamera(this.window.workCamera);
				
			}
			
		}
		
		// Events
		public void OnInit() { if (this.transition != null) this.transition.OnInit(); }
		public void OnDeinit() {}
		public void OnShowEnd() {}
		public void OnHideEnd() {}
		public void OnShowBegin(System.Action callback, bool resetAnimation = true) {
			
			if (this.transition != null) {
				
				this.transition.SetResetState(null, null);
				this.transition.Play(this.window, forward: true, callback: callback);
				
			} else {
				
				if (callback != null) callback();
				
			}
			
		}
		public void OnHideBegin(System.Action callback, bool immediately = false) {
			
			if (this.transition != null) {
				
				this.transition.Play(this.window, forward: false, callback: callback);
				
			} else {
				
				if (callback != null) callback();
				
			}
			
		}
		
		private bool waitCapture = false;
		private bool grabEveryFrame = false;
		private Texture2D screen;
		private System.Action callback;
		public void SaveToCache(Texture2D screen, System.Action callback, bool grabEveryFrame = false) {
			
			this.callback = callback;
			this.screen = screen;
			this.grabEveryFrame = grabEveryFrame;
			this.waitCapture = true;
			
		}
		
		public void OnPostRender() {
			
			if (this.waitCapture == true) {
				
				ME.Utilities.ScreenToTexture(this.screen);
				
				if (this.callback != null) this.callback();
				this.callback = null;
				
				if (this.grabEveryFrame == true) {
					
					if (this.transition != null && this.transition.IsValid(this.window) == false) {
						
						this.waitCapture = false;
						
					}
					
				} else {
					
					this.waitCapture = false;
					
				}
				
			}
			
			/*if (this.renderingToTexture == true && this.transition == null) {
				
				// Unity bug
				
				var layoutWindow = (this.window as LayoutWindowType);
				if (layoutWindow != null) layoutWindow.layout.GetLayoutInstance().canvas.renderMode = RenderMode.ScreenSpaceCamera;
				
			}*/

		}

		public void OnPreRender() {

			/*if (this.renderingToTexture == true && this.transition == null) {

				// Unity bug

				var layoutWindow = (this.window as LayoutWindowType);
				if (layoutWindow != null) layoutWindow.layout.GetLayoutInstance().canvas.renderMode = RenderMode.WorldSpace;

			}*/

		}
		
		public void OnRenderImage(RenderTexture source, RenderTexture destination) {
			
			if (this.transition != null) {

				this.transition.OnRenderTransition(this.window, source, destination);

			} else {

				if (this.renderingToTexture == true) {

					Graphics.Blit(source, destination);

				}

			}
			
		}
		
		public float GetAnimationDuration(bool forward) {
			
			if (this.transition != null) return this.transition.GetDuration(null, forward);
			
			return 0f;
			
		}

		public void StartRenderToTexture() {

			this.renderingToTexture = true;

			// Unity bug
			
			var layoutWindow = (this.window as LayoutWindowType);
			if (layoutWindow != null) {

				var canvas = layoutWindow.layout.GetLayoutInstance().canvas;

				canvas.renderMode = RenderMode.WorldSpace;
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				Canvas.ForceUpdateCanvases();
				
				//var localScale = canvas.transform.localScale;
				//var localPosition = canvas.transform.localPosition;
				//var sizeDelta = (canvas.transform as RectTransform).sizeDelta;
				
				canvas.renderMode = RenderMode.WorldSpace;
				
				Canvas.ForceUpdateCanvases();

				//canvas.transform.localScale = localScale;
				//canvas.transform.localPosition = localPosition;
				//(canvas.transform as RectTransform).sizeDelta = sizeDelta;

			}

		}
		
		public void StopRenderToTexture() {
			
			this.renderingToTexture = false;
			
			// Unity bug
			
			var layoutWindow = (this.window as LayoutWindowType);
			if (layoutWindow != null) layoutWindow.layout.GetLayoutInstance().canvas.renderMode = RenderMode.ScreenSpaceCamera;

		}

	}

}
