using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Animations;
using UnityEngine.UI.Windows.Types;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows {

	public enum MessageType : byte {

		None,
		Info,
		Warning,
		Error,

	};

	public enum DragFlag : byte {

		None = 0x0,
		ViewportRestricted = 0x1,

	};

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
		public class BackButtonBehaviour {

			public enum BackAction : byte {

				None = 0x0,
				HideCurrentWindow = 0x1,
				ShowPreviousWindow = 0x2,
				ShowSpecificWindow = 0x4,

			};
			
			[BitMask(typeof(BackAction))]
			public BackAction backAction = BackAction.None;
			
			[ReadOnly(fieldName: "backAction", state: (int)BackAction.ShowSpecificWindow, bitMask: true)]
			public WindowBase window;

			[System.Serializable]
			public class OnBackAction : UnityEvent {}
			public OnBackAction onBackAction = new OnBackAction();

			private System.Action callback;

			public bool IsBackActionShowSpecific() {
				
				return (this.backAction & BackAction.ShowSpecificWindow) != 0;
				
			}
			
			public bool IsBackActionShowPrevious() {
				
				return (this.backAction & BackAction.ShowPreviousWindow) != 0;
				
			}
			
			public bool IsBackActionHide() {
				
				return (this.backAction & BackAction.HideCurrentWindow) != 0;
				
			}

			public void SetCallback(System.Action callback) {

				this.callback = callback;

			}

			public void LateUpdate(WindowBase window) {

				if (WindowSystem.GetCurrentWindow() == window) {

					if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) == true) {
						
						if (this.callback != null) {
							
							this.callback();
							
						} else {
							
							this.OnClick(window);
							
						}
						
					}

				}

			}
			
			public void OnClick(WindowBase window) {
				
				var hideCur = this.IsBackActionHide();
				var showPrev = this.IsBackActionShowPrevious();
				var showSpec = this.IsBackActionShowSpecific();

				if (window.GetState() == WindowObjectState.Shown) {

					this.onBackAction.Invoke();

					if (hideCur == true || showPrev == true || showSpec == true) {

						if (showPrev == true) {
							
							var prev = WindowSystem.GetPreviousWindow(window);
							if (prev != null) {
								
								prev.Show();
								
							} else {
								
								#if UNITY_EDITOR || DEBUGBUILD
								Debug.LogWarning("Previous HistoryItem is null. Make sure your history was not cleared.");
								#endif
								
							}
							
						}
						
						if (hideCur == true) {
							
							window.Hide();
							
						}
						
						if (showSpec == true) {
							
							WindowSystem.Show(this.window);

						}

					}

				}

			}
			
		}

		[System.Serializable]
		public class Once {
			
			[System.Serializable]
			public class OnInit : UnityEvent {}
			public OnInit onInit = new OnInit();
			
			[System.Serializable]
			public class OnDeinit : UnityEvent {}
			public OnDeinit onDeinit = new OnDeinit();
			
			public void Clear() {
				
				this.onInit.RemoveAllListeners();
				this.onDeinit.RemoveAllListeners();
				
			}
			
			public void Register(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {
					
					case WindowEventType.OnInit:
						this.onInit.AddListener(callback);
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

					case WindowEventType.OnDeinit:
						this.onDeinit.Invoke();
						break;
						
				}
				
			}

		}

		[System.Serializable]
		public class EveryInstance {

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

			public void Clear() {

				this.onShowBegin.RemoveAllListeners();
				this.onShowEnd.RemoveAllListeners();
				this.onHideBegin.RemoveAllListeners();
				this.onHideEnd.RemoveAllListeners();

			}
			
			public void Register(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {

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

				}
				
			}
			
			public void Unregister(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {

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

				}
				
			}

			public void ReleaseEvents(WindowEventType eventType) {
				
				switch (eventType) {

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

				}

			}

		}
		
		[Header("Back Button Behaviour")]
		public BackButtonBehaviour backButtonBehaviour = new BackButtonBehaviour();

		[Header("Events")]
		public Once once = new Once();
		public EveryInstance onEveryInstance = new EveryInstance();

		public void LateUpdate(WindowBase window) {
			
			this.backButtonBehaviour.LateUpdate(window);

		}

		private void ReleaseEvents(WindowEventType eventType) {
			
			this.once.ReleaseEvents(eventType);
			this.onEveryInstance.ReleaseEvents(eventType);
			
		}

		// Events
		public void OnInit() {

			this.ReleaseEvents(WindowEventType.OnInit);

		}

		public void OnDeinit() {

			this.ReleaseEvents(WindowEventType.OnDeinit);
			
			this.once.Clear();
			this.onEveryInstance.Clear();

		}

		public void OnShowBegin(System.Action callback, bool resetAnimation = true) {

			this.ReleaseEvents(WindowEventType.OnShowBegin);
			if (callback != null) callback();

		}

		public void OnShowEnd() {

			this.ReleaseEvents(WindowEventType.OnShowEnd);

		}
		public void OnHideBegin(System.Action callback, bool immediately = false) {

			this.ReleaseEvents(WindowEventType.OnHideBegin);
			if (callback != null) callback();

		}
		public void OnHideEnd() {

			this.ReleaseEvents(WindowEventType.OnHideEnd);

		}
		
	}
	
	[System.Serializable]
	public class Preferences {

		public enum Depth : byte {

			Auto = 0,
			AlwaysTop = 1,
			AlwaysBack = 2,
			
			AlwaysTopLayer1 = 3,
			AlwaysTopLayer2 = 4,

		};

		public enum DontDestroy : byte {
			Auto = 0x0,
			OnSceneChange = 0x1,
			OnClean = 0x2,
		};
		
		public enum History : byte {
			Auto = 0x0,
			DontSave = 0x1,
		};

		[Header("Base")]
		public Depth depth;
		[BitMask(typeof(DontDestroy))]
		public DontDestroy dontDestroy = DontDestroy.OnSceneChange;
		[BitMask(typeof(History))]
		public History history = History.Auto;

		public bool forceSingleInstance = false;

		[Header("Pool")]
		public bool createPool = true;
		[ReadOnly("createPool", state: false)]
		public int preallocatedCount = 0;

		[Header("Draggable")]
		public bool draggable = false;
		[Hidden("draggable", false)]
		public LayoutTag dragTag;
		[Hidden("draggable", false)]
		public DragFlag dragFlags;

		public bool IsDragViewportRestricted() {

			return (this.dragFlags & DragFlag.ViewportRestricted) != 0;

		}

		public bool IsHistoryActive() {

			return (this.history & History.DontSave) == 0;

		}

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

		}
		#endif

	}
	
	[System.Serializable]
	public class Transition : IWindowEventsAsync {

		[Header("Default")]
		public TransitionBase transition;
		public TransitionInputParameters transitionParameters;

		[HideInInspector]
		private WindowBase window;
		
		private bool renderingToTexture = false;

		public void Setup(WindowBase window) {
			
			this.window = window;
			
			if (this.transition!= null) this.transition.SetupCamera(this.window.workCamera);

		}

		// Events
		public void OnInit() {

			if (this.transition != null) this.transition.OnInit();

		}

		public void OnDeinit() {}
		public void OnShowEnd() {}
		public void OnHideEnd() {}
		
		public void OnShowBegin(System.Action callback, bool resetAnimation = true) {

			this.OnShowBegin(this.transition, this.transitionParameters, callback);

		}

		public void OnShowBegin(TransitionBase transition, TransitionInputParameters transitionParameters, System.Action callback) {

			if (transition != null) {
				
				transition.SetResetState(transitionParameters, this.window, null);
				transition.Play(this.window, transitionParameters, null, forward: true, callback: callback);
				
			} else {

				// Reset to defaults
				var layoutWindow = (this.window as LayoutWindowType);
				if (layoutWindow != null) {

					layoutWindow.layout.GetLayoutInstance().root.Reset();

				}

				if (callback != null) callback();
				
			}
			
		}
		
		public void OnHideBegin(System.Action callback, bool immediately = false) {
			
			this.OnHideBegin(this.transition, this.transitionParameters, callback);

		}

		public void OnHideBegin(TransitionBase transition, TransitionInputParameters transitionParameters, System.Action callback) {

			if (transition != null) {

				transition.Play(this.window, transitionParameters, null, forward: false, callback: callback);
				
			} else {
				
				if (callback != null) callback();
				
			}
			
		}

		public void Apply(TransitionBase transition, TransitionInputParameters parameters, bool forward, float value, bool reset) {

			if (reset == true) transition.SetResetState(parameters, this.window, null);
			transition.Set(this.window, parameters, null, forward: forward, value: value);

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
