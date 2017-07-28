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

	[System.Serializable]
	public class ModulesList : IWindowEventsController {
		
		[System.Serializable]
		public class ModuleInfo : IWindowEventsController {

			public WindowModule moduleSource;
			public int sortingOrder;
			public bool backgroundLayer;

			[HideInInspector][System.NonSerialized]
			private IWindowEventsController instance;
			private WindowModule instanceModule;

			[HideInInspector][System.NonSerialized]
			private WindowBase windowContext;

			public ModuleInfo(WindowModule moduleSource, int sortingOrder, bool backgroundLayer) {

				this.moduleSource = moduleSource;
				this.sortingOrder = sortingOrder;
				this.backgroundLayer = backgroundLayer;

			}

			public void Create(WindowBase window, Transform modulesRoot) {

				this.windowContext = window;

				if (this.moduleSource.IsInstantiate() == true) {

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

					this.instance = instance as IWindowEventsController;
					this.instanceModule = instance as WindowModule;

				} else {

					this.instance = this.moduleSource as IWindowEventsController;
					this.instanceModule = this.moduleSource as WindowModule;

				}

			}
			
			public float GetDuration(bool forward) {
				
				return (this.instance as WindowModule).GetAnimationDuration(forward);
				
			}

			public bool IsSupported() {

				return this.moduleSource != null && this.moduleSource.IsSupported();

			}

			public T Get<T>() where T : WindowModule {
				
				return this.instance as T;
				
			}

			public void DoWindowLayoutComplete() {

				if (this.instance != null) {

					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoWindowLayoutComplete();

				}

			}

			public void DoWindowActive() {

				if (this.instance != null) {

					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoWindowActive();

				}

			}

			public void DoWindowInactive() {

				if (this.instance != null) {

					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoWindowInactive();

				}

			}

			public void DoWindowOpen() {

				if (this.instance != null) {

					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoWindowOpen();

				}

			}

			public void DoWindowClose() {

				if (this.instance != null) {

					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoWindowClose();

				}

			}

			public void DoInit() {

				if (this.instance != null) {

					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoInit();

				}
				
			}

			public void DoDeinit(System.Action callback) {

				if (this.instance != null) {
					
					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoDeinit(callback);

				} else {

					callback.Invoke();

				}

				this.instance = null;
				this.instanceModule = null;
				this.windowContext = null;

			}

			public void DoShowBegin(AppearanceParameters parameters) {

				if (this.instance != null) {

					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoShowBegin(parameters);

				} else {

					parameters.Call();

				}

			}

			public void DoShowEnd(AppearanceParameters parameters) {

				if (this.instance != null) {
					
					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoShowEnd(parameters);

				}

			}

			public void DoHideBegin(AppearanceParameters parameters) {
				
				if (this.instance != null) {
					
					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoHideBegin(parameters);
					
				} else {
					
					parameters.Call();

				}

			}

			public void DoHideEnd(AppearanceParameters parameters) {

				if (this.instance != null) {
					
					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoHideEnd(parameters);

				}

			}

			public void DoWindowUnload() {

				if (this.instance != null) {

					if (this.instanceModule.IsInstantiate() == false) this.instanceModule.Setup(this.windowContext);
					this.instance.DoWindowUnload();

				}

			}
			
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

			for (int i = 0; i < this.elements.Length; ++i) {

				var element = this.elements[i];
				if (element.IsSupported() == true) {

					element.Create(window, modulesRoot);
					if (window.GetActiveState() == ActiveState.Active) {

						element.DoWindowActive();

					}

				}

			}

		}
		
		public float GetAnimationDuration(bool forward) {
			
			var maxDuration = 0f;
			for (int i = 0; i < this.elements.Length; ++i) {

				var element = this.elements[i];
				var d = element.GetDuration(forward);
				if (d >= maxDuration) {
					
					maxDuration = d;
					
				}
				
			}
			
			return maxDuration;
			
		}
		
		public T Get<T>() where T : WindowModule {
			
			for (int i = 0; i < this.elements.Length; ++i) {
				
				var element = this.elements[i];
				var item = element.Get<T>();
				if (item != null) return item;

			}
			
			return default(T);
			
		}
		
		// Events
		public void DoWindowLayoutComplete() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoWindowLayoutComplete(); }
		public void DoWindowActive() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoWindowActive(); }
		public void DoWindowInactive() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoWindowInactive(); }
		public void DoWindowOpen() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoWindowOpen(); }
		public void DoWindowClose() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoWindowClose(); }
		public void DoInit() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoInit(); }
		public void DoDeinit(System.Action callback) {

			ME.Utilities.CallInSequence(callback, this.elements, (e, c) => { e.DoDeinit(c); }, waitPrevious: true);

		}
		public void DoShowBegin(AppearanceParameters parameters) {

			var callback = parameters.callback;
			ME.Utilities.CallInSequence(callback, this.elements, (e, c) => { e.DoShowBegin(parameters.ReplaceCallback(c)); });

		}
		public void DoShowEnd(AppearanceParameters parameters) { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoShowEnd(parameters); }
		public void DoHideBegin(AppearanceParameters parameters) {
			
			var callback = parameters.callback;
			ME.Utilities.CallInSequence(callback, this.elements, (e, c) => { e.DoHideBegin(parameters.ReplaceCallback(c)); });

		}
		public void DoHideEnd(AppearanceParameters parameters) { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoHideEnd(parameters); }

		public void DoWindowUnload() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoWindowUnload(); }

	}

	[System.Serializable]
	public class Events : IWindowEventsController {

		public static void RemoveAllListeners(UnityEvent unityEvent, ME.Events.SimpleUnityEvent simpleEvent) {

			if (unityEvent != null) unityEvent.RemoveAllListeners();
			if (simpleEvent != null) simpleEvent.RemoveAllListeners();

		}

		public static void Register(UnityEvent unityEvent, ME.Events.SimpleUnityEvent simpleEvent, UnityAction callback) {

			if (unityEvent != null) unityEvent.AddListener(callback);
			if (simpleEvent != null) simpleEvent.AddListener(callback);

		}

		public static void Unregister(UnityEvent unityEvent, ME.Events.SimpleUnityEvent simpleEvent, UnityAction callback) {

			if (unityEvent != null) unityEvent.RemoveListener(callback);
			if (simpleEvent != null) simpleEvent.RemoveListener(callback);

		}

		public static void Raise(UnityEvent unityEvent, ME.Events.SimpleUnityEvent simpleEvent) {

			if (unityEvent != null) unityEvent.Invoke();
			if (simpleEvent != null) simpleEvent.Invoke();

		}

		public static void Initialize<T>(ref T unityEvent, ref ME.Events.SimpleUnityEvent simpleEvent) where T : UnityEvent {

			if (unityEvent == null && unityEvent.GetPersistentEventCount() == 0) {

				unityEvent = null;

			} else {

				simpleEvent = null;

			}

		}

		[System.Serializable]
		public class BackButtonBehaviour {

			public enum BackAction : byte {

				None = 0x0,
				HideCurrentWindow = 0x1,
				ShowPreviousWindow = 0x2,
				ShowSpecificWindow = 0x4,

			};

			private WindowBase currentWindow;

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

			public void Call() {
				
				if (this.currentWindow != null) this.currentWindow.OnBackButtonAction();
				if (this.callback != null) {

					this.callback.Invoke();

				} else {

					this.OnClick(this.currentWindow);

				}

			}

			public void LateUpdate(WindowBase window) {

				this.currentWindow = window;

				if (WindowSystem.GetCurrentWindow() == window) {

					if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) == true) {

						if (WindowSystemInput.CanMoveBack(this.currentWindow) == false) return;

						this.Call();

					}

				}

			}

			public void Clear() {

				this.onBackAction.RemoveAllListeners();
				this.currentWindow = null;

			}
			
			public void OnClick(WindowBase window) {
				
				var hideCur = this.IsBackActionHide();
				var showPrev = this.IsBackActionShowPrevious();
				var showSpec = this.IsBackActionShowSpecific();

				if (window.GetState() == WindowObjectState.Shown || window.GetState() == WindowObjectState.Showing) {

					this.onBackAction.Invoke();

					if (hideCur == true || showPrev == true || showSpec == true) {

						if (showPrev == true) {
							
							var prev = WindowSystem.GetPreviousWindow();
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
			public ME.Events.SimpleUnityEvent onInitEvent = new ME.Events.SimpleUnityEvent();
			
			[System.Serializable]
			public class OnDeinit : UnityEvent {}
			public OnDeinit onDeinit = new OnDeinit();
			public ME.Events.SimpleUnityEvent onDeinitEvent = new ME.Events.SimpleUnityEvent();

			public void Initialize() {

				Events.Initialize(ref this.onInit, ref this.onInitEvent);
				Events.Initialize(ref this.onDeinit, ref this.onDeinitEvent);

			}

			public void Clear() {

				Events.RemoveAllListeners(this.onInit, this.onInitEvent);
				Events.RemoveAllListeners(this.onDeinit, this.onDeinitEvent);
				
			}
			
			public void Register(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {
					
					case WindowEventType.OnInit:
						Events.Register(this.onInit, this.onInitEvent, callback);
						break;

					case WindowEventType.OnDeinit:
						Events.Register(this.onDeinit, this.onDeinitEvent, callback);
						break;
						
				}
				
			}
			
			public void Unregister(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {
					
					case WindowEventType.OnInit:
						Events.Unregister(this.onInit, this.onInitEvent, callback);
						break;

					case WindowEventType.OnDeinit:
						Events.Unregister(this.onDeinit, this.onDeinitEvent, callback);
						break;
						
				}
				
			}
			
			public void RaiseEvents(WindowEventType eventType) {
				
				switch (eventType) {

					case WindowEventType.OnInit:
						Events.Raise(this.onInit, this.onInitEvent);
						break;

					case WindowEventType.OnDeinit:
						Events.Raise(this.onDeinit, this.onDeinitEvent);
						break;
						
				}
				
			}

		}

		[System.Serializable]
		public class EveryInstance {

			[System.Serializable]
			public class OnShowBegin : UnityEvent {}
			public OnShowBegin onShowBegin = new OnShowBegin();
			public ME.Events.SimpleUnityEvent onShowBeginEvent = new ME.Events.SimpleUnityEvent();
			
			[System.Serializable]
			public class OnShowEnd : UnityEvent {}
			public OnShowEnd onShowEnd = new OnShowEnd();
			public ME.Events.SimpleUnityEvent onShowEndEvent = new ME.Events.SimpleUnityEvent();
			
			[System.Serializable]
			public class OnHideBegin : UnityEvent {}
			public OnHideBegin onHideBegin = new OnHideBegin();
			public ME.Events.SimpleUnityEvent onHideBeginEvent = new ME.Events.SimpleUnityEvent();

			[System.Serializable]
			public class OnHideEnd : UnityEvent {}
			public OnHideEnd onHideEnd = new OnHideEnd();
			public ME.Events.SimpleUnityEvent onHideEndEvent = new ME.Events.SimpleUnityEvent();

			[System.Serializable]
			public class OnHideEndLate : UnityEvent {}
			public OnHideEndLate onHideEndLate = new OnHideEndLate();
			public ME.Events.SimpleUnityEvent onHideEndLateEvent = new ME.Events.SimpleUnityEvent();

			[System.Serializable]
			public class OnWindowOpen : UnityEvent {}
			public OnWindowOpen onWindowOpen = new OnWindowOpen();
			public ME.Events.SimpleUnityEvent onWindowOpenEvent = new ME.Events.SimpleUnityEvent();

			[System.Serializable]
			public class OnWindowClose : UnityEvent {} 
			public OnWindowClose onWindowClose = new OnWindowClose();
			public ME.Events.SimpleUnityEvent onWindowCloseEvent = new ME.Events.SimpleUnityEvent();

			[System.Serializable]
			public class OnWindowActive : UnityEvent {}
			public OnWindowActive onWindowActive = new OnWindowActive();
			public ME.Events.SimpleUnityEvent onWindowActiveEvent = new ME.Events.SimpleUnityEvent();

			[System.Serializable]
			public class OnWindowInactive : UnityEvent {}
			public OnWindowInactive onWindowInactive = new OnWindowInactive();
			public ME.Events.SimpleUnityEvent onWindowInactiveEvent = new ME.Events.SimpleUnityEvent();

			[System.Serializable]
			public class OnWindowLayoutComplete : UnityEvent {}
			public OnWindowLayoutComplete onWindowLayoutComplete = new OnWindowLayoutComplete();
			public ME.Events.SimpleUnityEvent onWindowLayoutCompleteEvent = new ME.Events.SimpleUnityEvent();

			public void Clear() {

				Events.RemoveAllListeners(this.onShowBegin, this.onShowBeginEvent);
				Events.RemoveAllListeners(this.onShowEnd, this.onShowEndEvent);
				Events.RemoveAllListeners(this.onHideBegin, this.onHideBeginEvent);
				Events.RemoveAllListeners(this.onHideEnd, this.onHideEndEvent);
				Events.RemoveAllListeners(this.onHideEndLate, this.onHideEndLateEvent);
				Events.RemoveAllListeners(this.onWindowOpen, this.onWindowOpenEvent);
				Events.RemoveAllListeners(this.onWindowClose, this.onWindowCloseEvent);
				Events.RemoveAllListeners(this.onWindowActive, this.onWindowActiveEvent);
				Events.RemoveAllListeners(this.onWindowInactive, this.onWindowInactiveEvent);
				Events.RemoveAllListeners(this.onWindowLayoutComplete, this.onWindowLayoutCompleteEvent);

			}

			public void Initialize() {

				Events.Initialize(ref this.onShowBegin, ref this.onShowBeginEvent);
				Events.Initialize(ref this.onShowEnd, ref this.onShowEndEvent);
				Events.Initialize(ref this.onHideBegin, ref this.onHideBeginEvent);
				Events.Initialize(ref this.onHideEnd, ref this.onHideEndEvent);
				Events.Initialize(ref this.onHideEndLate, ref this.onHideEndLateEvent);
				Events.Initialize(ref this.onWindowOpen, ref this.onWindowOpenEvent);
				Events.Initialize(ref this.onWindowClose, ref this.onWindowCloseEvent);
				Events.Initialize(ref this.onWindowActive, ref this.onWindowActiveEvent);
				Events.Initialize(ref this.onWindowInactive, ref this.onWindowInactiveEvent);
				Events.Initialize(ref this.onWindowLayoutComplete, ref this.onWindowLayoutCompleteEvent);

			}

			public void Register(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {

					case WindowEventType.OnShowBegin:
						Events.Register(this.onShowBegin, this.onShowBeginEvent, callback);
						break;

					case WindowEventType.OnShowEnd:
						Events.Register(this.onShowEnd, this.onShowEndEvent, callback);
						break;

					case WindowEventType.OnHideBegin:
						Events.Register(this.onHideBegin, this.onHideBeginEvent, callback);
						break;

					case WindowEventType.OnHideEnd:
						Events.Register(this.onHideEnd, this.onHideEndEvent, callback);
						break;

					case WindowEventType.OnHideEndLate:
						Events.Register(this.onHideEndLate, this.onHideEndLateEvent, callback);
						break;

					case WindowEventType.OnWindowOpen:
						Events.Register(this.onWindowOpen, this.onWindowOpenEvent, callback);
						break;

					case WindowEventType.OnWindowClose:
						Events.Register(this.onWindowClose, this.onWindowCloseEvent, callback);
						break;

					case WindowEventType.OnWindowActive:
						Events.Register(this.onWindowActive, this.onWindowActiveEvent, callback);
						break;

					case WindowEventType.OnWindowInactive:
						Events.Register(this.onWindowInactive, this.onWindowInactiveEvent, callback);
						break;

					case WindowEventType.OnWindowLayoutComplete:
						Events.Register(this.onWindowLayoutComplete, this.onWindowLayoutCompleteEvent, callback);
						break;

				}
				
			}
			
			public void Unregister(WindowEventType eventType, UnityAction callback) {
				
				switch (eventType) {

					case WindowEventType.OnShowBegin:
						Events.Unregister(this.onShowBegin, this.onShowBeginEvent, callback);
						break;

					case WindowEventType.OnShowEnd:
						Events.Unregister(this.onShowEnd, this.onShowEndEvent, callback);
						break;

					case WindowEventType.OnHideBegin:
						Events.Unregister(this.onHideBegin, this.onHideBeginEvent, callback);
						break;

					case WindowEventType.OnHideEnd:
						Events.Unregister(this.onHideEnd, this.onHideEndEvent, callback);
						break;

					case WindowEventType.OnHideEndLate:
						Events.Unregister(this.onHideEndLate, this.onHideEndLateEvent, callback);
						break;

					case WindowEventType.OnWindowOpen:
						Events.Unregister(this.onWindowOpen, this.onWindowOpenEvent, callback);
						break;

					case WindowEventType.OnWindowClose:
						Events.Unregister(this.onWindowClose, this.onWindowCloseEvent, callback);
						break;

					case WindowEventType.OnWindowActive:
						Events.Unregister(this.onWindowActive, this.onWindowActiveEvent, callback);
						break;

					case WindowEventType.OnWindowInactive:
						Events.Unregister(this.onWindowInactive, this.onWindowInactiveEvent, callback);
						break;

					case WindowEventType.OnWindowLayoutComplete:
						Events.Unregister(this.onWindowLayoutComplete, this.onWindowLayoutCompleteEvent, callback);
						break;

				}
				
			}

			public void Unregister(WindowEventType eventType) {

				switch (eventType) {

					case WindowEventType.OnShowBegin:
						Events.RemoveAllListeners(this.onShowBegin, this.onShowBeginEvent);
						break;

					case WindowEventType.OnShowEnd:
						Events.RemoveAllListeners(this.onShowEnd, this.onShowEndEvent);
						break;

					case WindowEventType.OnHideBegin:
						Events.RemoveAllListeners(this.onHideBegin, this.onHideBeginEvent);
						break;

					case WindowEventType.OnHideEnd:
						Events.RemoveAllListeners(this.onHideEnd, this.onHideEndEvent);
						break;

					case WindowEventType.OnHideEndLate:
						Events.RemoveAllListeners(this.onHideEndLate, this.onHideEndLateEvent);
						break;

					case WindowEventType.OnWindowOpen:
						Events.RemoveAllListeners(this.onWindowOpen, this.onWindowOpenEvent);
						break;

					case WindowEventType.OnWindowClose:
						Events.RemoveAllListeners(this.onWindowClose, this.onWindowCloseEvent);
						break;

					case WindowEventType.OnWindowActive:
						Events.RemoveAllListeners(this.onWindowActive, this.onWindowActiveEvent);
						break;

					case WindowEventType.OnWindowInactive:
						Events.RemoveAllListeners(this.onWindowInactive, this.onWindowInactiveEvent);
						break;

					case WindowEventType.OnWindowLayoutComplete:
						Events.RemoveAllListeners(this.onWindowLayoutComplete, this.onWindowLayoutCompleteEvent);
						break;

				}

			}

			public void RaiseEvents(WindowEventType eventType) {
				
				switch (eventType) {

					case WindowEventType.OnShowBegin:
						Events.Raise(this.onShowBegin, this.onShowBeginEvent);
						break;

					case WindowEventType.OnShowEnd:
						Events.Raise(this.onShowEnd, this.onShowEndEvent);
						break;

					case WindowEventType.OnHideBegin:
						Events.Raise(this.onHideBegin, this.onHideBeginEvent);
						break;

					case WindowEventType.OnHideEnd:
						Events.Raise(this.onHideEnd, this.onHideEndEvent);
						break;

					case WindowEventType.OnHideEndLate:
						Events.Raise(this.onHideEndLate, this.onHideEndLateEvent);
						break;

					case WindowEventType.OnWindowOpen:
						Events.Raise(this.onWindowOpen, this.onWindowOpenEvent);
						break;

					case WindowEventType.OnWindowClose:
						Events.Raise(this.onWindowClose, this.onWindowCloseEvent);
						break;

					case WindowEventType.OnWindowActive:
						Events.Raise(this.onWindowActive, this.onWindowActiveEvent);
						break;

					case WindowEventType.OnWindowInactive:
						Events.Raise(this.onWindowInactive, this.onWindowInactiveEvent);
						break;

					case WindowEventType.OnWindowLayoutComplete:
						Events.Raise(this.onWindowLayoutComplete, this.onWindowLayoutCompleteEvent);
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

		private void RaiseEvents(WindowEventType eventType) {

			WindowSystem.RunSafe(() => {

				this.once.RaiseEvents(eventType);
				this.onEveryInstance.RaiseEvents(eventType);
			
			});

		}

		public void RegisterOnce(WindowEventType eventType, UnityAction action) {

			UnityAction _action = null;
			_action = () => {

				if (eventType == WindowEventType.OnInit ||
					eventType == WindowEventType.OnDeinit) {

					this.once.Unregister(eventType, _action);

				} else {

					this.onEveryInstance.Unregister(eventType, _action);

				}

				if (action != null) action.Invoke();

			};

			if (eventType == WindowEventType.OnInit ||
			    eventType == WindowEventType.OnDeinit) {

				this.once.Register(eventType, _action);

			} else {

				this.onEveryInstance.Register(eventType, _action);

			}

		}

		// Events
		public void DoWindowLayoutComplete() {

			this.RaiseEvents(WindowEventType.OnWindowLayoutComplete);

		}

		public void DoWindowActive() {

			this.RaiseEvents(WindowEventType.OnWindowActive);

		}

		public void DoWindowInactive() {

			this.RaiseEvents(WindowEventType.OnWindowInactive);

		}

		public void DoInit() {

			this.once.Initialize();
			this.onEveryInstance.Initialize();

			this.RaiseEvents(WindowEventType.OnInit);

		}

		public void DoDeinit(System.Action callback) {

			this.RaiseEvents(WindowEventType.OnDeinit);
			this.once.Clear();

			callback.Invoke();

		}

		public void DoShowBegin(AppearanceParameters parameters) {

			this.RaiseEvents(WindowEventType.OnShowBegin);
			parameters.Call();

		}

		public void DoShowEnd(AppearanceParameters parameters) {

			this.RaiseEvents(WindowEventType.OnShowEnd);

		}
		public void DoHideBegin(AppearanceParameters parameters) {

			this.RaiseEvents(WindowEventType.OnHideBegin);
			parameters.Call();

		}
		public void DoHideEnd(AppearanceParameters parameters) {

			this.RaiseEvents(WindowEventType.OnHideEnd);

		}

		public void DoHideEndLate(AppearanceParameters parameters) {

			this.RaiseEvents(WindowEventType.OnHideEndLate);
			this.onEveryInstance.Clear();

		}

		public void DoWindowOpen() {

			this.RaiseEvents(WindowEventType.OnWindowOpen);

		}

		public void DoWindowClose() {

			this.RaiseEvents(WindowEventType.OnWindowClose);

		}

		public void DoWindowUnload() {

			this.backButtonBehaviour.Clear();
			this.once.Clear();
			this.onEveryInstance.Clear();

		}

	}
	
	public enum DepthLayer : int {
		
		BackLayer4 = -4,
		BackLayer3 = -3,
		BackLayer2 = -2,
		BackLayer1 = -1,
		
		Auto = 0,
		
		TopLayer1 = 1,
		TopLayer2 = 2,
		TopLayer3 = 3,
		TopLayer4 = 4,
		
	};
	
	public enum DontDestroy : byte {
		
		Auto = 0x0,
		OnSceneChange = 0x1,
		OnClean = 0x2,
		Ever = 0x4,

	};
	
	public enum History : byte {
		
		Auto = 0x0,
		DontSave = 0x1,
		
	};

	[System.Serializable]
	public class TargetPreferences {

		[System.Serializable]
		public class TargetInfo {

			[Header("Platform Filter")]
			public bool platform;
			[ReadOnly("platform", state: false)]
			public RuntimePlatform[] anyOfPlatform;

			[Header("Aspect Ratio Filter")]
			public bool aspect;
			[ReadOnly("aspect", state: false)]
			public Vector2 aspectFrom;
			[ReadOnly("aspect", state: false)]
			public Vector2 aspectTo;

			public bool IsValid() {

				var result = true;
				if (result == true && this.platform == true) {

					result = false;
					var platform = WindowSystem.GetCurrentRuntimePlatform();
					for (int i = 0; i < this.anyOfPlatform.Length; ++i) {

						if (this.anyOfPlatform[i] == platform) {

							result = true;
							break;

						}

					}

				}

				if (result == true && this.aspect == true) {

					var delta = 0.001f;
					var aspectSize = Screen.width / (float)Screen.height;
					var checkTo = this.aspectFrom.x / this.aspectFrom.y;
					var checkFrom = this.aspectTo.x / this.aspectTo.y;

					//Debug.LogError("---------- CHECK: " + aspectSize + " :: " + Screen.width + " :: " + Screen.height + " // " + checkFrom + " // " + checkTo);
					result = ((aspectSize >= checkFrom - delta) && (aspectSize <= checkTo + delta));

				}

				return result;

			}

		}

		[Header("Preferences File")]
		public WindowTargetPreferences preferencesFile;

		[Header("-- Or --")]
		[ReadOnly("preferencesFile", state: null, inverseCondition: true)]
		public bool runOnAnyTarget = true;
		[Hidden("runOnAnyTarget", state: true)]
		public TargetInfo[] targets;

		/*[System.NonSerialized]
		private bool isDirty = true;
		[System.NonSerialized]
		private bool lastResult = false;*/

		public bool GetRunOnAnyTarget() {

			if (this.preferencesFile != null) {

				return this.preferencesFile.preferences.runOnAnyTarget;

			}

			return this.runOnAnyTarget;

		}

		public bool IsValid() {

			if (this.preferencesFile != null) {

				return this.preferencesFile.preferences.IsValid();

			}

			/*if (this.isDirty == true) {
				
				this.isDirty = false;
				this.lastResult = false;*/

			for (int i = 0; i < this.targets.Length; ++i) {

				if (this.targets[i].IsValid() == true) {

					//this.lastResult = true;
					//break;
					return true;

				}

			}

			//}

			//return this.lastResult;
			return false;

		}

		public void SetDirty() {
			
			if (this.preferencesFile != null) {

				this.preferencesFile.preferences.SetDirty();
				return;

			}

			//this.isDirty = true;

		}

	}

	[System.Serializable]
	public class Preferences {

		[Header("Base")]
		public DepthLayer layer;
		[BitMask(typeof(DontDestroy))]
		public DontDestroy dontDestroy = DontDestroy.OnSceneChange;
		[BitMask(typeof(History))]
		public History history = History.Auto;

		[Tooltip("This window will be turned off immediately after CleanWindow call or will be using deactivateMaxTimeout to become inactive.")]
		public bool deactivateOnRecycleImmediately = true;
		[MinMaxSlider(2f, 5f, "deactivateOnRecycleImmediately", state: true)]
		public Vector2 deactivateMaxTimeout = new Vector2(2f, 5f);

		[Tooltip("If `true` this type of windows will be show after previous window of this type. If `false` window will shown normaly.")]
		public bool showInSequence = false;
		[Tooltip("Send Active/Inactive states on Show/Hide events.")]
		public bool sendActiveState = true;
		[Tooltip("This window cover all previous content. If `true` previous windows will be disabled.")]
		public bool fullCoverage = false;
		[Tooltip("Forces one instance only on scene.")]
		public bool forceSingleInstance = false;
		[ReadOnly("forceSingleInstance", state: false)][Tooltip("Ignores `new` instance initialize and layout events.")]
		public bool singleInstanceIgnoreActions = false;
		[Tooltip("Restores UI.EventSystem last selected element on window activation.")]
		public bool restoreSelectedElement = true;

		[Header("Canvas")]
		[Tooltip("Should we override default canvas PixelPerfect parameter. If WindowSystemSettings used, this parameter override these too.")]
		public bool overrideCanvasPixelPerfect = false;
		[Hidden("overrideCanvasPixelPerfect", false)]
		public bool canvasPixelPerfect = true;

		[Header("Pool")]
		public bool createPool = true;
		[Hidden("createPool", state: false)]
		public int preallocatedCount = 0;

		[Header("Draggable")]
		public bool draggable = false;
		[Hidden("draggable", false)]
		public LayoutTag dragTag;
		[Hidden("draggable", false)]
		public DragFlag dragFlags;

		[Header("Other")]
		public bool overrideCameraSettings = false;
		[Hidden("overrideCameraSettings", false)]
		public WindowSystemSettings.Camera cameraSettings;

		public Preferences() {
		}

		public Preferences(Preferences other) {

			this.layer = other.layer;
			this.dontDestroy = other.dontDestroy;
			this.history = other.history;

			this.deactivateOnRecycleImmediately = other.deactivateOnRecycleImmediately;
			this.deactivateMaxTimeout = other.deactivateMaxTimeout;

			this.showInSequence = other.showInSequence;
			this.sendActiveState = other.sendActiveState;
			this.fullCoverage = other.fullCoverage;
			this.forceSingleInstance = other.forceSingleInstance;
			this.singleInstanceIgnoreActions = other.singleInstanceIgnoreActions;
			this.restoreSelectedElement = other.restoreSelectedElement;

			this.createPool = other.createPool;
			this.preallocatedCount = other.preallocatedCount;

			this.draggable = other.draggable;
			this.dragTag = other.dragTag;
			this.dragFlags = other.dragFlags;

			this.overrideCameraSettings = other.overrideCameraSettings;
			this.cameraSettings = new WindowSystemSettings.Camera(other.cameraSettings);

		}

		public bool IsDragViewportRestricted() {

			return (this.dragFlags & DragFlag.ViewportRestricted) != 0;

		}

		public bool IsHistoryActive() {

			return (this.history & History.DontSave) == 0;

		}

		public bool IsDontDestroyAuto() {
			
			return (this.dontDestroy & DontDestroy.Auto) != 0;
			
		}
		
		public bool IsDontDestroyOnSceneChange() {
			
			return (this.dontDestroy & DontDestroy.OnSceneChange) != 0;
			
		}
		
		public bool IsDontDestroyOnClean() {
			
			return (this.dontDestroy & DontDestroy.OnClean) != 0;
			
		}

		public bool IsDontDestroyEver() {

			return (this.dontDestroy & DontDestroy.Ever) != 0;

		}

		#if UNITY_EDITOR
		public void OnValidate() {

		}
		#endif

	}
	
	[System.Serializable]
	public class Transition : IWindowEventsController {

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
		public void DoWindowLayoutComplete() { }
		public void DoWindowActive() { }
		public void DoWindowInactive() { }
		public void DoWindowOpen() { }
		public void DoWindowClose() { }
		public void DoWindowUnload() { }

		public void DoInit() {

			if (this.transition != null) this.transition.OnInit();

		}

		public void DoDeinit(System.Action callback) { callback.Invoke(); }
		public void DoShowEnd(AppearanceParameters parameters) {}
		public void DoHideEnd(AppearanceParameters parameters) {}
		
		public void DoShowBegin(AppearanceParameters parameters) {

			this.DoShowBegin(this.transition, this.transitionParameters, parameters);

		}

		public void DoShowBegin(TransitionBase transition, TransitionInputParameters transitionParameters, AppearanceParameters parameters) {

			if (transition != null) {
				
				transition.SetResetState(transitionParameters, this.window, null);
				transition.Play(null, this.window, transitionParameters, forward: true, callback: parameters.callback);
				
			} else {

				// Reset to defaults
				var layoutWindow = (this.window as LayoutWindowType);
				if (layoutWindow != null) {

					layoutWindow.GetCurrentLayout().GetLayoutInstance().root.Reset();

				}

				parameters.Call();
				
			}
			
		}
		
		public void DoHideBegin(AppearanceParameters parameters) {
			
			this.DoHideBegin(this.transition, this.transitionParameters, parameters);

		}

		public void DoHideBegin(TransitionBase transition, TransitionInputParameters transitionParameters, AppearanceParameters parameters) {

			if (transition != null) {

				transition.Play(null, this.window, transitionParameters, forward: false, callback: parameters.callback);
				
			} else {
				
				parameters.Call();

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

				var canvas = layoutWindow.GetCurrentLayout().GetLayoutInstance().canvas;

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
			if (layoutWindow != null) layoutWindow.GetCurrentLayout().GetLayoutInstance().canvas.renderMode = RenderMode.ScreenSpaceCamera;

		}

	}

}
