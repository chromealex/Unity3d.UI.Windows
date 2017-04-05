using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

	[UnityEngine.DisallowMultipleComponent]
	public class WindowObjectElement : WindowObject, IWindowEventsAsync, IWindowEventsController, IComponentElement {
		
		[Header("Sub Components")]
		public bool autoRegisterInRoot = true;
		public bool autoRegisterSubComponents = true;
		public bool setInactiveOnHiddenState = true;

		/// <summary>
		/// Show this component on start showing window or not.
		/// </summary>
		public bool showOnStart = true;

		[SerializeField][ReadOnly]
		protected WindowObjectElement rootComponent;
        [SerializeField][ReadOnly]
		protected List<WindowObjectElement> subComponents = new List<WindowObjectElement>();

		[SerializeField][HideInInspector]
		private WindowObjectState currentState = WindowObjectState.NotInitialized;

		private RectTransform _rectTransform;

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this._rectTransform = null;
			this.rootComponent = null;
			//this.subComponents = null;

		}

		public RectTransform GetRectTransform() {

			if (this._rectTransform == null) this._rectTransform = this.transform as RectTransform;
			return this._rectTransform;

		}

		public WindowObjectElement GetRootComponent() {

			#if UNITY_EDITOR
			if (Application.isPlaying == false) {

				if (this.rootComponent == null && this.transform.parent != null) this.rootComponent = ME.Utilities.FindReferenceParent<WindowObjectElement>(this.transform.parent.gameObject);

			}
			#endif

			return this.rootComponent;

		}

		/// <summary>
		/// Gets the state of the component.
		/// </summary>
		/// <returns>The component state.</returns>
		public WindowObjectState GetComponentState() {
			
			return this.currentState;
			
		}

		/// <summary>
		/// Determines whether this instance is in Showing/Shown state.
		/// </summary>
		/// <returns><c>true</c> if this instance is visible; otherwise, <c>false</c>.</returns>
		public bool IsVisible() {

			#if UNITY_EDITOR
			if (Application.isPlaying == false) {

				return this.gameObject.activeInHierarchy;

			}
			#endif

			return this.IsVisibleSelf() == true && (this.rootComponent != null ? this.rootComponent.IsVisible() : true);

		}

		public bool IsVisibleSelf() {

			#if UNITY_EDITOR
			if (Application.isPlaying == false) {

				return this.gameObject.activeSelf;

			}
			#endif

			return (this.currentState == WindowObjectState.Shown || this.currentState == WindowObjectState.Showing);

		}

		public virtual void SetComponentState(WindowObjectState state, bool dontInactivate = false) {
			
			if (this == null) return;

			this.currentState = state;

			var go = this.gameObject;

			if (this.currentState == WindowObjectState.Showing ||
				this.currentState == WindowObjectState.Shown) {

				if (go != null) go.SetActive(true);
				/*if (go != null) {

					//var r = go.GetComponent<CanvasRenderer>();
					//if (r != null) r.EnableRectClipping(new Rect(0f, 0f, 0f, 0f));
					go.transform.localScale = Vector3.one;

				}*/

			} else if (this.currentState == WindowObjectState.Hidden ||
			           this.currentState == WindowObjectState.NotInitialized ||
			           this.currentState == WindowObjectState.Initializing) {
				
				if (go != null && this.NeedToInactive() == true && dontInactivate == false) {

					//var r = go.GetComponent<CanvasRenderer>();
					//if (r != null) r.DisableRectClipping();
					//go.transform.localScale = Vector3.zero;
					go.SetActive(false);

				}

			}

		}
		
		public virtual bool NeedToInactive() {

			return this.setInactiveOnHiddenState;

		}

		public T GetSubComponentInChildren<T>(System.Predicate<T> predicate = null) where T : IComponent {
			
			for (int i = 0; i < this.subComponents.Count; ++i) {

				var component = this.subComponents[i];
				if (component is T) {

					var result = (T)(component as IComponent);
					if (predicate == null || predicate(result) == true) return result;

				}

				var comp = component.GetSubComponentInChildren<T>(predicate);
				if (comp != null) return comp;

			}

			return default(T);

		}

		public T GetSubComponent<T>(System.Predicate<T> predicate = null) where T : WindowObjectElement {

			return this.subComponents.FirstOrDefault(x => x is T && (predicate == null || predicate(x as T))) as T;

		}

        /// <summary>
        /// Gets the sub components.
        /// </summary>
        /// <returns>The sub components.</returns>
        public List<WindowObjectElement> GetSubComponents() {

            return this.subComponents;

        }

		public virtual void Setup(WindowLayoutBase layoutRoot) {

            for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].Setup(layoutRoot);

        }

        public override void Setup(WindowBase window) {

            base.Setup(window);

            for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].Setup(window);

        }

		public virtual void OnLocalizationChanged() {

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnLocalizationChanged();

		}

		public virtual void OnManualEvent<T>(T data) where T : IManualEvent {

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnManualEvent<T>(data);

		}

		/// <summary>
		/// Raises the window open/close event.
		/// Fires before OnShowBegin/OnHideBegin, but after OnInit/OnParametersPass/OnEmptyPass
		/// </summary>
		#region OnWindowOpen/Close
		void IWindowEventsController.DoWindowActive() {

			WindowSystem.RunSafe(this.OnWindowActive);

			for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoWindowActive();

		}

		void IWindowEventsController.DoWindowInactive() {

			WindowSystem.RunSafe(this.OnWindowInactive);

			for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoWindowInactive();

		}

		public virtual void OnWindowActive() {}
		public virtual void OnWindowInactive() {}
		#endregion

		/// <summary>
		/// Raises the window open/close event.
		/// Fires before OnShowBegin/OnHideBegin, but after OnInit/OnParametersPass/OnEmptyPass
		/// </summary>
		#region OnWindowOpen/Close
		void IWindowEventsController.DoWindowOpen() {
			
			WindowSystem.RunSafe(this.OnWindowOpen);
			
			for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoWindowOpen();
			
		}
		
		void IWindowEventsController.DoWindowClose() {
			
			WindowSystem.RunSafe(this.OnWindowClose);
			
			for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoWindowClose();
			
		}

		public virtual void OnWindowOpen() {}
		public virtual void OnWindowClose() {}
		#endregion

		#region OnWindowLayoutComplete
		void IWindowEventsController.DoWindowLayoutComplete() {

			WindowSystem.RunSafe(this.OnWindowLayoutComplete);

			for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoWindowLayoutComplete();

		}

		public virtual void OnWindowLayoutComplete() {}
		#endregion

		#region Base Events
	    /// <summary>
	    /// Raises the init event.
	    /// You can override this method but call it's base.
	    /// </summary>
		void IWindowEventsController.DoInit() {
			
			this.SetComponentState(WindowObjectState.Initializing);

			for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoInit();

			this.SetComponentState(WindowObjectState.Initialized);
			WindowSystem.RunSafe(this.OnInit);

	    }

	    /// <summary>
	    /// Raises the deinit event.
	    /// You can override this method but call it's base.
	    /// </summary>
		void IWindowEventsController.DoDeinit(System.Action callback) {
			
			this.SetComponentState(WindowObjectState.Deinitializing);

			ME.Utilities.CallInSequence(() => {

				this.SetComponentState(WindowObjectState.NotInitialized);
				WindowSystem.RunSafe(this.OnDeinit, callback);

			},
				this.subComponents.ToArray(),
				(item, cb) => (item as IWindowEventsController).DoDeinit(cb),
				waitPrevious: true
			);

        }

	    /// <summary>
	    /// Raises the show end event.
	    /// You can override this method but call it's base.
	    /// </summary>
		public virtual void DoShowEnd(AppearanceParameters parameters) {
			
			var includeChilds = parameters.GetIncludeChilds(defaultValue: true);
			if (includeChilds == true) {

				for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoShowEnd(parameters);

			}

			this.SetComponentState(WindowObjectState.Shown);
			WindowSystem.RunSafe(this.OnShowEnd);
			WindowSystem.RunSafe(this.OnShowEnd, parameters);

        }

	    /// <summary>
	    /// Raises the hide end event.
	    /// You can override this method but call it's base.
	    /// </summary>
		public virtual void DoHideEnd(AppearanceParameters parameters) {

			var includeChilds = parameters.GetIncludeChilds(defaultValue: true);
			if (includeChilds == true) {

				for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoHideEnd(parameters);

			}

			this.SetComponentState(WindowObjectState.Hidden);
			WindowSystem.RunSafe(this.OnHideEnd);
			WindowSystem.RunSafe(this.OnHideEnd, parameters);

	    }

		public virtual void DoShowBegin(AppearanceParameters parameters) {
			
			this.SetComponentState(WindowObjectState.Showing);
			WindowSystem.RunSafe(this.OnShowBegin);
			WindowSystem.RunSafe(this.OnShowBegin, parameters);

			var includeChilds = parameters.GetIncludeChilds(defaultValue: true);
			if (includeChilds == true) {

				var callback = parameters.callback;
				ME.Utilities.CallInSequence(callback, this.subComponents, (item, cb) => (item as IWindowEventsController).DoShowBegin(parameters.ReplaceCallback(cb)));

			} else {

				parameters.Call();

			}

		}

		public virtual void DoHideBegin(AppearanceParameters parameters) {
			
			this.SetComponentState(WindowObjectState.Hiding);
			WindowSystem.RunSafe(this.OnHideBegin);
			WindowSystem.RunSafe(this.OnHideBegin, parameters);

			var includeChilds = parameters.GetIncludeChilds(defaultValue: true);
			if (includeChilds == true) {

				var callback = parameters.callback;
				ME.Utilities.CallInSequence(callback, this.subComponents, (item, cb) => (item as IWindowEventsController).DoHideBegin(parameters.ReplaceCallback(cb)));

			} else {

				parameters.Call();

			}

		}

		void IWindowEventsController.DoWindowUnload() {

			for (int i = 0; i < this.subComponents.Count; ++i) (this.subComponents[i] as IWindowEventsController).DoWindowUnload();

			WindowSystem.RunSafe(this.OnWindowUnload);

		}

		public virtual void DoLoad(bool async, System.Action<WindowObjectElement> onItem, System.Action callback) {

			ME.Utilities.CallInSequence(callback, this.subComponents, (item, cb) => {

				item.DoLoad(async, null, () => {

					if (onItem != null) onItem.Invoke(item);
					cb.Invoke();

				});

			}, waitPrevious: async); 

		}

		public virtual void OnInit() {}
		//public virtual void OnDeinit() {}
		public virtual void OnShowEnd() {}
		public virtual void OnHideEnd() {}
		public virtual void OnShowEnd(AppearanceParameters parameters) {}
		public virtual void OnHideEnd(AppearanceParameters parameters) {}
		public virtual void OnShowBegin() {}
		public virtual void OnHideBegin() {}
		public virtual void OnShowBegin(AppearanceParameters parameters) {}
		public virtual void OnHideBegin(AppearanceParameters parameters) {}
		public virtual void OnWindowUnload() {}
		#endregion

        /// <summary>
        /// Registers the sub component.
        /// If you want to instantiate a new component manualy but wants window events - register this component here.
        /// </summary>
        /// <param name="subComponent">Sub component.</param>
		public virtual void RegisterSubComponent(WindowObjectElement subComponent) {
			
			//Debug.Log("TRY REGISTER: " + subComponent + " :: " + this.GetComponentState() + "/" + subComponent.GetComponentState(), this);
			if (this.subComponents.Contains(subComponent) == false) {

				subComponent.rootComponent = this;
				this.subComponents.Add(subComponent);
				
			} else {
				
				WindowSystemLogger.Warning(this, "RegisterSubComponent can't complete because of duplicate item.");
				return;

			}

			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			var controller = (subComponent as IWindowEventsController);

			switch (this.GetComponentState()) {
				
				case WindowObjectState.Hiding:
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						controller.DoInit();
						WindowSystem.RunSafe(subComponent.OnWindowActive);
						
					}

					subComponent.SetComponentState(this.GetComponentState());

					break;

				case WindowObjectState.Hidden:
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						controller.DoInit();
						WindowSystem.RunSafe(subComponent.OnWindowActive);
						
					}

					subComponent.SetComponentState(this.GetComponentState());

					break;

				case WindowObjectState.Initializing:
				case WindowObjectState.Initialized:
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						controller.DoInit();
						WindowSystem.RunSafe(subComponent.OnWindowActive);
						
					}

	                break;

                case WindowObjectState.Showing:
                    
					// after OnShowBegin
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						controller.DoInit();
						WindowSystem.RunSafe(subComponent.OnWindowActive);
						
					}

					if (subComponent.showOnStart == true) {

						controller.DoShowBegin(AppearanceParameters.Default());

					}

                    break;

                case WindowObjectState.Shown:

                    // after OnShowEnd
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						controller.DoInit();
						WindowSystem.RunSafe(subComponent.OnWindowActive);
						
					}

					if (subComponent.showOnStart == true) {

						controller.DoShowBegin(AppearanceParameters.Default().ReplaceCallback(() => {

							controller.DoShowEnd(AppearanceParameters.Default());

	                    }));

					}

                    break;

            }
			
			if (this.GetWindow() != null) {

                subComponent.Setup(this.GetWindow());
                // subComponent.Setup(this.GetLayoutRoot());

            }

        }

        /// <summary>
        /// Unregisters the sub component.
        /// </summary>
        /// <param name="subComponent">Sub component.</param>
		public virtual void UnregisterSubComponent(WindowObjectElement subComponent, System.Action callback = null, bool immediately = true) {

			#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
			#endif

			var sendCallback = true;

			//Debug.Log("UNREGISTER: " + subComponent + " :: " + this.GetComponentState() + "/" + subComponent.GetComponentState());

			subComponent.rootComponent = null;
			this.subComponents.Remove(subComponent);

			var controller = (subComponent as IWindowEventsController);

			switch (subComponent.GetComponentState()) {

				case WindowObjectState.Showing:
				case WindowObjectState.Shown:

                    // after OnShowEnd
					controller.DoWindowClose();
					controller.DoWindowInactive();
					controller.DoHideBegin(AppearanceParameters.Default().ReplaceForced(forced: true).ReplaceImmediately(immediately));
					controller.DoHideEnd(AppearanceParameters.Default().ReplaceForced(forced: true).ReplaceImmediately(immediately));
					controller.DoWindowUnload();
					controller.DoDeinit(() => { if (callback != null) callback(); });

					sendCallback = false;

                    break;

                case WindowObjectState.Hiding:

					// after OnHideBegin
					controller.DoWindowClose();
					controller.DoWindowInactive();
					controller.DoHideEnd(AppearanceParameters.Default().ReplaceForced(forced: true).ReplaceImmediately(immediately));
					controller.DoWindowUnload();
					controller.DoDeinit(() => { if (callback != null) callback(); });

					sendCallback = false;

                    break;

                case WindowObjectState.Hidden:

					// after OnHideEnd
					controller.DoWindowClose();
					controller.DoWindowInactive();
					controller.DoWindowUnload();
					controller.DoDeinit(() => { if (callback != null) callback(); });

					sendCallback = false;

                    break;

            }

			if (sendCallback == true && callback != null) callback();

        }

		/*public virtual void OnDestroy() {

			if (this.rootComponent != null) this.rootComponent.UnregisterSubComponent(this);

		}*/

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			this.Update_EDITOR();
			
		}

		private void Update_EDITOR() {

			if (Application.isPlaying == true) return;

			this.rootComponent = ME.Utilities.FindReferenceParent<WindowObjectElement>(this.transform.parent);

			this.SetComponentState(WindowObjectState.NotInitialized, dontInactivate: true);

			if (this.autoRegisterSubComponents == true) {
				
				var components = this.GetComponentsInChildren<WindowObjectElement>(true).ToList();
				
				this.subComponents.Clear();
				foreach (var component in components) {
					
					if (component == this) continue;
					
					var parents = component.GetComponentsInParent<WindowObjectElement>(true).ToList();
					parents.Remove(component);
					
					if (parents.Count > 0 && parents[0] != this) continue;
					if (component.autoRegisterInRoot == false) continue;
					
					this.subComponents.Add(component);
					
				}
				
			} else {
				
				//this.subComponents.Clear();
				
			}
			
			this.subComponents = this.subComponents.Where((c) => c != null).ToList();
			
		}

		public virtual void OnDrawGizmos() {

			//this.OnDrawGUI_EDITOR(false, false);

		}

		public virtual void OnDrawGizmosSelected() {

			/*var selected = (UnityEditor.Selection.activeGameObject == this.gameObject);
			this.OnDrawGUI_EDITOR(selected, true);*/

		}
		#endif

	}

}