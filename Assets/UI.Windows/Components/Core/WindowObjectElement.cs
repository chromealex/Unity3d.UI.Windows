using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows {

	[UnityEngine.DisallowMultipleComponent]
	public class WindowObjectElement : WindowObject {
		
		[Header("Sub Components")]
		public bool autoRegisterInRoot = true;
		public bool autoRegisterSubComponents = true;
		
		/// <summary>
		/// Show this component on start showing window or not.
		/// </summary>
		public bool showOnStart = true;

        [SerializeField]
        [ReadOnly]
        protected List<WindowObjectElement> subComponents = new List<WindowObjectElement>();
		
		private WindowObjectState currentState = WindowObjectState.NotInitialized;
		
		/// <summary>
		/// Gets the state of the component.
		/// </summary>
		/// <returns>The component state.</returns>
		public WindowObjectState GetComponentState() {
			
			return this.currentState;
			
		}

		public bool IsVisible() {

			return this.currentState == WindowObjectState.Hidden || this.currentState == WindowObjectState.Hiding;

		}

		public virtual void SetComponentState(WindowObjectState state) {

			this.currentState = state;

			if (this == null) return;

			var go = this.gameObject;

			if (this.currentState == WindowObjectState.Showing ||
				this.currentState == WindowObjectState.Shown) {

				if (go != null) go.SetActive(true);

			} else if (this.currentState == WindowObjectState.Hidden ||
			           this.currentState == WindowObjectState.NotInitialized ||
			           this.currentState == WindowObjectState.Initializing) {
				
				if (go != null && this.NeedToInactive() == true) go.SetActive(false);

			}

		}
		
		public virtual bool NeedToInactive() {

			return true;

		}

        /// <summary>
        /// Gets the sub components.
        /// </summary>
        /// <returns>The sub components.</returns>
        public List<WindowObjectElement> GetSubComponents() {

            return this.subComponents;

        }

        internal virtual void Setup(WindowLayoutBase layoutRoot) {

            for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].Setup(layoutRoot);

        }

        internal override void Setup(WindowBase window) {

            base.Setup(window);

            for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].Setup(window);

        }

	    /// <summary>
	    /// Raises the init event.
	    /// You can override this method but call it's base.
	    /// </summary>
	    public virtual void OnInit() {
			
			this.SetComponentState(WindowObjectState.Initializing);

            for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnInit();
			
			//this.SetComponentState(WindowObjectState.Initialized);

	    }

	    /// <summary>
	    /// Raises the deinit event.
	    /// You can override this method but call it's base.
	    /// </summary>
        public virtual void OnDeinit() {

            for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnDeinit();
			
			this.SetComponentState(WindowObjectState.NotInitialized);

        }

	    /// <summary>
	    /// Raises the show end event.
	    /// You can override this method but call it's base.
	    /// </summary>
        public virtual void OnShowEnd() {

            for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnShowEnd();
			
			this.SetComponentState(WindowObjectState.Shown);

        }

	    /// <summary>
	    /// Raises the hide end event.
	    /// You can override this method but call it's base.
	    /// </summary>
	    public virtual void OnHideEnd() {

            for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].OnHideEnd();
			
			this.SetComponentState(WindowObjectState.Hidden);

	    }
		
		public virtual void OnShowBegin(System.Action callback, bool resetAnimation = true) {

			// At the top of the level - setup Shown state (may be must be declared in OnShowEnd method)
			if (this.GetComponentState() == WindowObjectState.Showing) {
				
				this.SetComponentState(WindowObjectState.Shown);
				
			}

			if (callback != null) callback();

		}

		public virtual void OnHideBegin(System.Action callback, bool immediately = false) {

			// At the top of the level - setup Hidden state (may be must be declared in OnHideEnd method)
			if (this.GetComponentState() == WindowObjectState.Hiding) {
				
				this.SetComponentState(WindowObjectState.Hidden);
				
			}

			//this.SetComponentState(WindowObjectState.Hiding);

			if (callback != null) callback();

		}

        /*public virtual void DeactivateComponents() {

			for (int i = 0; i < this.subComponents.Count; ++i) this.subComponents[i].DeactivateComponents();

            if (this == null) return;
            this.gameObject.SetActive(false);

        }*/

        /// <summary>
        /// Registers the sub component.
        /// If you want to instantiate a new component manualy but wants window events - register this component here.
        /// </summary>
        /// <param name="subComponent">Sub component.</param>
		public virtual void RegisterSubComponent(WindowObjectElement subComponent) {

			//Debug.Log("REGISTER: " + subComponent + " :: " + this.GetComponentState() + "/" + subComponent.GetComponentState());
			switch (this.GetComponentState()) {
				
				case WindowObjectState.Hiding:
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						subComponent.OnInit();
						
					}

					subComponent.SetComponentState(this.GetComponentState());

					break;

				case WindowObjectState.Hidden:
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						subComponent.OnInit();
						
					}

					subComponent.SetComponentState(this.GetComponentState());

					break;

				case WindowObjectState.Initializing:
				case WindowObjectState.Initialized:
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						subComponent.OnInit();
						
					}

	                break;

                case WindowObjectState.Showing:
                    
					// after OnShowBegin
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						subComponent.OnInit();
						
					}

					if (subComponent.showOnStart == true) {

	                    subComponent.OnShowBegin(null);

					}

                    break;

                case WindowObjectState.Shown:

                    // after OnShowEnd
					
					if (subComponent.GetComponentState() == WindowObjectState.NotInitialized) {
						
						subComponent.OnInit();
						
					}

					if (subComponent.showOnStart == true) {

	                    subComponent.OnShowBegin(() => {

	                        subComponent.OnShowEnd();

	                    });

					}

                    break;

            }
			
			if (this.GetWindow() != null) {

                subComponent.Setup(this.GetWindow());
                // subComponent.Setup(this.GetLayoutRoot());

            }

            if (this.subComponents.Contains(subComponent) == false) this.subComponents.Add(subComponent);

        }

        /// <summary>
        /// Unregisters the sub component.
        /// </summary>
        /// <param name="subComponent">Sub component.</param>
		public void UnregisterSubComponent(WindowObjectElement subComponent, System.Action callback = null) {

#if UNITY_EDITOR
			if (Application.isPlaying == false) return;
#endif

			var sendCallback = true;

			//Debug.Log("UNREGISTER: " + subComponent + " :: " + this.GetComponentState() + "/" + subComponent.GetComponentState());
			
			this.subComponents.Remove(subComponent);

			switch (this.GetComponentState()) {

                case WindowObjectState.Shown:

                    // after OnShowEnd
                    subComponent.OnHideBegin(() => {

                        subComponent.OnHideEnd();
                        subComponent.OnDeinit();

						if (callback != null) callback();

                    });

					sendCallback = false;

                    break;

                case WindowObjectState.Hiding:

                    // after OnHideBegin
                    subComponent.OnHideBegin(null);

					sendCallback = false;
					if (callback != null) callback();

                    break;

                case WindowObjectState.Hidden:

                    // after OnHideEnd
                    subComponent.OnHideBegin(() => {

                        subComponent.OnHideEnd();
                        subComponent.OnDeinit();
						
						if (callback != null) callback();

					});

					sendCallback = false;

                    break;

            }

			if (sendCallback == true && callback != null) callback();

        }

		#if UNITY_EDITOR
		/// <summary>
		/// Raises the validate event. Editor Only.
		/// </summary>
		public void OnValidate() {
			
			this.OnValidateEditor();
			
		}

		/// <summary>
		/// Raises the validate editor event.
		/// You can override this method but call it's base.
		/// </summary>
		public virtual void OnValidateEditor() {
			
			this.Update_EDITOR();
			
		}

		private void Update_EDITOR() {

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
		#endif

	}

}