using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Animations;

namespace UnityEngine.UI.Windows {

	public class WindowComponentBase : WindowObject, IWindowAnimation {
		
		[Header("Animation Info")]
		new public WindowAnimationBase animation;
		[HideInInspector]
		public List<TransitionInputParameters> animationInputParams = new List<TransitionInputParameters>();
		[HideInInspector]
		public CanvasGroup canvas;
		[HideInInspector]
		public bool animationRefresh = false;
		
		private WindowObjectState currentState = WindowObjectState.NotInitialized;

		/// <summary>
		/// Gets the state of the component.
		/// </summary>
		/// <returns>The component state.</returns>
		public WindowObjectState GetComponentState() {
			
			return this.currentState;
			
		}

		/// <summary>
		/// Show component.
		/// Animation component can use current layout element root or current component root.
		/// </summary>
		/// <param name="resetAnimation">If set to <c>true</c> reset animation.</param>
		public void Show(bool resetAnimation = true) {
			
			this.Show(null, resetAnimation);
			
		}

		/// <summary>
		/// Show component with callback after end.
		/// Animation component can use current layout element root or current component root.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="resetAnimation">If set to <c>true</c> reset animation.</param>
		public virtual void Show(System.Action callback, bool resetAnimation) {
			
			this.OnShowBegin(callback, resetAnimation);
			
		}

		/// <summary>
		/// Hide this instance.
		/// Animation component can use current layout element root or current component root.
		/// </summary>
		public void Hide() {
			
			this.Hide(null, immediately: false);
			
		}

		/// <summary>
		/// Hide the specified immediately.
		/// Animation component can use current layout element root or current component root.
		/// </summary>
		/// <param name="immediately">If set to <c>true</c> immediately.</param>
		public void Hide(bool immediately) {

			this.Hide(null, immediately);

		}

		/// <summary>
		/// Hide the specified callback and immediately.
		/// Animation component can use current layout element root or current component root.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="immediately">If set to <c>true</c> immediately.</param>
		public virtual void Hide(System.Action callback, bool immediately) {

			this.OnHideBegin(callback, immediately);
			
		}

		/// <summary>
		/// Set up `reset` state to animation.
		/// </summary>
		public void SetResetState() {
			
			if (this.animation != null) this.animation.SetResetState(this.animationInputParams, this);
			
		}
		
		/// <summary>
		/// Set up `in` state to animation.
		/// </summary>
		public void SetInState() {
			
			if (this.animation != null) this.animation.SetInState(this.animationInputParams, this);
			
		}
		
		/// <summary>
		/// Set up `out` state to animation.
		/// </summary>
		public void SetOutState() {
			
			if (this.animation != null) this.animation.SetOutState(this.animationInputParams, this);
			
		}

		/// <summary>
		/// Gets the duration of the animation.
		/// </summary>
		/// <returns>The animation duration.</returns>
		/// <param name="forward">If set to <c>true</c> forward.</param>
		public virtual float GetAnimationDuration(bool forward) {
			
			if (this.animation != null) {
				
				return this.animation.GetDuration(this.animationInputParams, forward);
				
			}
			
			return 0f;
			
		}

		/// <summary>
		/// Raises the show begin event.
		/// Wait while all sub components return the callback.
		/// You can override this method but call it's base.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void OnShowBegin(System.Action callback) {
			
			this.OnShowBegin(callback, resetAnimation: true);
			
		}

		/// <summary>
		/// Raises the show begin event.
		/// Wait while all sub components return the callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="resetAnimation">If set to <c>true</c> reset animation.</param>
		private void OnShowBegin(System.Action callback, bool resetAnimation) {
			
			System.Action callbackInner = () => {
				
				this.currentState = WindowObjectState.Shown;
				
				if (callback != null) callback();
				
			};

			this.currentState = WindowObjectState.Showing;
			
			if (this.animation != null) {
				
				if (resetAnimation == true) this.SetResetState();
				this.animation.Play(this.animationInputParams, this, true, callbackInner);
				
			} else {
				
				callbackInner();
				
			}
			
		}

		/// <summary>
		/// Raises the hide begin event.
		/// Wait while all sub components return the callback.
		/// You can override this method but call it's base.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void OnHideBegin(System.Action callback) {

			this.OnHideBegin(callback, immediately: false);

		}

		/// <summary>
		/// Raises the hide begin event.
		/// Wait while all sub components return the callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="immediately">If set to <c>true</c> immediately.</param>
		private void OnHideBegin(System.Action callback, bool immediately) {

			System.Action callbackInner = () => {
				
				this.currentState = WindowObjectState.Hidden;

				if (callback != null) callback();

			};
			
			this.currentState = WindowObjectState.Hiding;

			if (this.animation != null) {

				if (immediately == true) {

					this.animation.SetOutState(this.animationInputParams, this);
					if (callback != null) callback();

				} else {

					this.animation.Play(this.animationInputParams, this, false, callbackInner);

				}

			} else {

				callbackInner();
				
			}
			
		}

		/// <summary>
		/// Raises the init event.
		/// You can override this method but call it's base.
		/// </summary>
		public virtual void OnInit() {

			this.currentState = WindowObjectState.Initialized;

		}

		/// <summary>
		/// Raises the deinit event.
		/// You can override this method but call it's base.
		/// </summary>
		public virtual void OnDeinit() {

			this.currentState = WindowObjectState.NotInitialized;

		}

		/// <summary>
		/// Raises the show end event.
		/// You can override this method but call it's base.
		/// </summary>
		public virtual void OnShowEnd() {}

		/// <summary>
		/// Raises the hide end event.
		/// You can override this method but call it's base.
		/// </summary>
		public virtual void OnHideEnd() {}

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
		
		private List<TransitionInputParameters> componentsToDestroy = new List<TransitionInputParameters>();
		
		[HideInInspector][SerializeField]
		public WindowAnimationBase lastAnimation;
		public virtual void Update_EDITOR() {

			this.animationInputParams = this.animationInputParams.Where((i) => i != null).ToList();
			
			this.canvas = this.GetComponent<CanvasGroup>();

			if (ME.EditorUtilities.IsPrefab(this.gameObject) == true) return;

			this.componentsToDestroy.Clear();

			if (this.animation == null || this.lastAnimation != this.animation || this.animationRefresh == false) {
				
				var ps = this.GetComponents<TransitionInputParameters>();
				foreach (var p in ps) {
					
					this.componentsToDestroy.Add(p);
					
				}
				
				this.animationRefresh = false;
				
			}
			
			if (this.animationRefresh == false && this.animation != null) {
				
				this.animation.ApplyInputParameters(this);
				this.animationRefresh = true;
				
			}
			
			this.lastAnimation = this.animation;
			
			UnityEditor.EditorApplication.delayCall = () => {
				
				foreach (var c in this.componentsToDestroy) {

					Component.DestroyImmediate(c);
					this.animationInputParams = this.animationInputParams.Where((i) => i != null).ToList();
					
				}
				
			};

		}
		#endif

	}

}