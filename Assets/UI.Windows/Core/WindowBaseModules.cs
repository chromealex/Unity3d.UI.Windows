﻿using UnityEngine;
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
		public class Modules : IWindowEventsController {
			
			[System.Serializable]
			public class ModuleInfo : IWindowEventsController {

				public WindowModule moduleSource;
				public int sortingOrder;
				public bool backgroundLayer;

				[HideInInspector][System.NonSerialized]
				private WindowModule instance;

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

						this.instance = instance;

					} else {

						this.instance = this.moduleSource;

					}

				}
				
				public float GetDuration(bool forward) {
					
					return this.instance.GetAnimationDuration(forward);
					
				}

				public bool IsSupported() {

					return this.moduleSource != null && this.moduleSource.IsSupported();

				}

				public T Get<T>() where T : WindowModule {
					
					return this.instance as T;
					
				}

				public void DoWindowActive() {

					if (this.instance != null) {
						
						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
						this.instance.DoWindowActive();

					}

				}

				public void DoWindowInactive() {

					if (this.instance != null) {
						
						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
						this.instance.DoWindowInactive();

					}

				}

				public void DoInit() {

					if (this.instance != null) {

						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
						this.instance.DoInit();

					}
					
				}

				public void DoDeinit() {

					if (this.instance != null) {
						
						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
						this.instance.DoDeinit();

					}

				}

				public void DoShowBegin(AppearanceParameters parameters) {

					if (this.instance != null) {

						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
						this.instance.DoShowBegin(parameters);

					} else {

						parameters.Call();

					}

				}

				public void DoShowEnd(AppearanceParameters parameters) {

					if (this.instance != null) {
						
						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
						this.instance.DoShowEnd(parameters);

					}

				}

				public void DoHideBegin(AppearanceParameters parameters) {
					
					if (this.instance != null) {
						
						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
						this.instance.DoHideBegin(parameters);
						
					} else {
						
						parameters.Call();

					}

				}

				public void DoHideEnd(AppearanceParameters parameters) {

					if (this.instance != null) {
						
						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
						this.instance.DoHideEnd(parameters);

					}

				}

				public void DoWindowUnload() {

					if (this.instance != null) {

						if (this.instance.IsInstantiate() == false) this.instance.Setup(this.windowContext);
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
				
				foreach (var element in this.elements) {

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
			public void DoWindowActive() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoWindowActive(); }
			public void DoWindowInactive() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoWindowInactive(); }
			public void DoInit() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoInit(); }
			public void DoDeinit() { for (int i = 0; i < this.elements.Length; ++i) this.elements[i].DoDeinit(); }
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

	}

	[System.Serializable]
	public class Events : IWindowEventsController {
		
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

				if (this.callback != null) {

					this.callback();

				} else {

					this.OnClick(this.currentWindow);

				}

			}

			public void LateUpdate(WindowBase window) {

				this.currentWindow = window;

				if (WindowSystem.GetCurrentWindow() == window) {

					if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) == true) {

						this.Call();

					}

				}

			}
			
			public void OnClick(WindowBase window) {
				
				var hideCur = this.IsBackActionHide();
				var showPrev = this.IsBackActionShowPrevious();
				var showSpec = this.IsBackActionShowSpecific();

				if (window.GetState() == WindowObjectState.Shown || window.GetState() == WindowObjectState.Showing) {

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

			[System.Serializable]
			public class OnWindowOpen : UnityEvent {}
			public OnWindowOpen onWindowOpen = new OnWindowOpen();

			[System.Serializable]
			public class OnWindowActive : UnityEvent {}
			public OnWindowActive onWindowActive = new OnWindowActive();

			[System.Serializable]
			public class OnWindowInactive : UnityEvent {}
			public OnWindowInactive onWindowInactive = new OnWindowInactive();

			public void Clear() {

				this.onShowBegin.RemoveAllListeners();
				this.onShowEnd.RemoveAllListeners();
				this.onHideBegin.RemoveAllListeners();
				this.onHideEnd.RemoveAllListeners();
				this.onWindowOpen.RemoveAllListeners();
				this.onWindowActive.RemoveAllListeners();
				this.onWindowInactive.RemoveAllListeners();

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
						
					case WindowEventType.OnWindowOpen:
						this.onWindowOpen.AddListener(callback);
						break;

					case WindowEventType.OnWindowActive:
						this.onWindowActive.AddListener(callback);
						break;

					case WindowEventType.OnWindowInactive:
						this.onWindowInactive.AddListener(callback);
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
						
					case WindowEventType.OnWindowOpen:
						this.onWindowOpen.RemoveListener(callback);
						break;

					case WindowEventType.OnWindowActive:
						this.onWindowActive.RemoveListener(callback);
						break;

					case WindowEventType.OnWindowInactive:
						this.onWindowInactive.RemoveListener(callback);
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

					case WindowEventType.OnWindowOpen:
						this.onWindowOpen.Invoke();
						break;

					case WindowEventType.OnWindowActive:
						this.onWindowActive.Invoke();
						break;

					case WindowEventType.OnWindowInactive:
						this.onWindowInactive.Invoke();
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
		public void DoWindowActive() {

			this.ReleaseEvents(WindowEventType.OnWindowActive);

		}

		public void DoWindowInactive() {

			this.ReleaseEvents(WindowEventType.OnWindowInactive);

		}

		public void DoInit() {

			this.ReleaseEvents(WindowEventType.OnInit);

		}

		public void DoDeinit() {

			this.ReleaseEvents(WindowEventType.OnDeinit);
			this.once.Clear();

		}

		public void DoShowBegin(AppearanceParameters parameters) {

			this.ReleaseEvents(WindowEventType.OnShowBegin);
			parameters.Call();

		}

		public void DoShowEnd(AppearanceParameters parameters) {

			this.ReleaseEvents(WindowEventType.OnShowEnd);

		}
		public void DoHideBegin(AppearanceParameters parameters) {

			this.ReleaseEvents(WindowEventType.OnHideBegin);
			parameters.Call();

		}
		public void DoHideEnd(AppearanceParameters parameters) {

			this.ReleaseEvents(WindowEventType.OnHideEnd);
			this.onEveryInstance.Clear();

		}

		public void DoWindowOpen() {

			this.ReleaseEvents(WindowEventType.OnWindowOpen);

		}

		public void DoWindowUnload() {



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
		public bool runOnAnyTarget = true;
		[ReadOnly("runOnAnyTarget", state: true)]
		public TargetInfo[] targets;

		[System.NonSerialized]
		private bool isDirty = true;
		[System.NonSerialized]
		private bool lastResult = false;

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

			if (this.isDirty == true) {
				
				this.isDirty = false;
				this.lastResult = false;

				for (int i = 0; i < this.targets.Length; ++i) {

					if (targets[i].IsValid() == true) {

						this.lastResult = true;
						break;

					}

				}

			}

			return this.lastResult;

		}

		public void SetDirty() {
			
			if (this.preferencesFile != null) {

				this.preferencesFile.preferences.SetDirty();
				return;

			}

			this.isDirty = true;

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

		[Tooltip("Send Active/Inactive states on Show/Hide")]
		public bool sendActiveState = true;

		[Tooltip("Forces one instance only on scene")]
		public bool forceSingleInstance = false;
		[ReadOnly("forceSingleInstance", state: false)][Tooltip("Ignores `new` instance initialize and layout events")]
		public bool singleInstanceIgnoreActions = false;
		[Tooltip("Restores UI.EventSystem last selected element on window activation")]
		public bool restoreSelectedElement = true;

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
		public void DoWindowActive() { }
		public void DoWindowInactive() { }
		public void DoInit() {

			if (this.transition != null) this.transition.OnInit();

		}

		public void DoDeinit() {}
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

					layoutWindow.layout.GetLayoutInstance().root.Reset();

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

		public void DoWindowUnload() {



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
