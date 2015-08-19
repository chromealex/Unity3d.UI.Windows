using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Animations;

namespace UnityEngine.UI.Windows {

    public class WindowComponentBase : WindowObjectElement, IWindowAnimation {

		public enum ChildsBehaviourMode : byte {

			Simultaneously = 0,		// Call Show/Hide methods on all childs
			Consequentially = 1,	// Call Show/Hide after all childs complete hide

		};

		[Header("Animation Info")]
		[SceneEditOnly]
		public WindowAnimationBase animation;
		
		public ChildsBehaviourMode childsShowMode = ChildsBehaviourMode.Simultaneously;
		public ChildsBehaviourMode childsHideMode = ChildsBehaviourMode.Simultaneously;

		[HideInInspector]
		public List<TransitionInputParameters> animationInputParams = new List<TransitionInputParameters>();
		[HideInInspector]
		public CanvasGroup canvas;
		[HideInInspector]
		public bool animationRefresh = false;

		private bool tempNeedToInactive = false;

		public override bool NeedToInactive() {

			return this.tempNeedToInactive == true || this.animation != null;

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
			
			this.OnShowBegin(() => {

				base.OnShowBegin(callback, resetAnimation);
				this.OnShowEnd();

			}, resetAnimation);
			
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

			this.Hide(callback, immediately, setupTempNeedInactive: true);
			
		}

		public void Hide(System.Action callback, bool immediately, bool setupTempNeedInactive) {
			
			if (setupTempNeedInactive == true) this.tempNeedToInactive = true;
			
			this.OnHideBegin_INTERNAL(() => {
				
				base.OnHideBegin(callback, immediately);
				this.OnHideEnd();
				
				if (setupTempNeedInactive == true) this.tempNeedToInactive = false;
				
			}, immediately);
			
		}

		/// <summary>
		/// Set up `reset` state to animation.
		/// </summary>
		public void SetResetState() {
			
			if (this.animation != null) this.animation.SetResetState(this.animationInputParams, this.GetWindow(), this);
			
		}
		
		/// <summary>
		/// Set up `in` state to animation.
		/// </summary>
		public void SetInState() {
			
			if (this.animation != null) this.animation.SetInState(this.animationInputParams, this.GetWindow(), this);
			
		}
		
		/// <summary>
		/// Set up `out` state to animation.
		/// </summary>
		public void SetOutState() {
			
			if (this.animation != null) this.animation.SetOutState(this.animationInputParams, this.GetWindow(), this);
			
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
		public override void OnShowBegin(System.Action callback, bool resetAnimation = true) {
			
			this.SetComponentState(WindowObjectState.Showing);

			if (this.childsShowMode == ChildsBehaviourMode.Simultaneously) {

				var counter = 0;
				System.Action callbackItem = () => {

					++counter;
					if (counter < 2) return;

					base.OnShowBegin(callback, resetAnimation);

				};

				this.OnShowBegin_INTERNAL(callback, resetAnimation);
				ME.Utilities.CallInSequence(callbackItem, this.subComponents, (e, c) => { e.OnShowBegin(c, resetAnimation); });

			} else if (this.childsShowMode == ChildsBehaviourMode.Consequentially) {

				ME.Utilities.CallInSequence(() => this.OnShowBegin_INTERNAL(() => base.OnShowBegin(callback, resetAnimation), resetAnimation), this.subComponents, (e, c) => e.OnShowBegin(c, resetAnimation));
				
			}

		}

		/// <summary>
		/// Raises the show begin event.
		/// Wait while all sub components return the callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="resetAnimation">If set to <c>true</c> reset animation.</param>
		private void OnShowBegin_INTERNAL(System.Action callback, bool resetAnimation, bool immediately = false) {

			System.Action callbackInner = () => {

				if (callback != null) callback();

			};

			if (this.animation != null) {
				
				if (resetAnimation == true) this.SetResetState();

				if (immediately == true) {

					this.animation.SetInState(this.animationInputParams, this.GetWindow(), this);
					if (callbackInner != null) callbackInner();

				} else {

					this.animation.Play(this.GetWindow(), this.animationInputParams, this, true, callbackInner);

				}

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
		public override void OnHideBegin(System.Action callback, bool immediately = false) {
			
			this.SetComponentState(WindowObjectState.Hiding);

            if (this.childsHideMode == ChildsBehaviourMode.Simultaneously) {

				var counter = 0;
				System.Action callbackItem = () => {

					++counter;
					if (counter < 2) return;
					
					base.OnHideBegin(callback, immediately);

				};

				this.OnHideBegin_INTERNAL(callbackItem, immediately);
				ME.Utilities.CallInSequence(callbackItem, this.subComponents, (e, c) => { e.OnHideBegin(c, immediately); });

			} else if (this.childsHideMode == ChildsBehaviourMode.Consequentially) {

				ME.Utilities.CallInSequence(() => this.OnHideBegin_INTERNAL(() => base.OnHideBegin(callback, immediately), immediately), this.subComponents, (e, c) => e.OnHideBegin(c, immediately));

			}

		}

		/// <summary>
		/// Raises the hide begin event.
		/// Wait while all sub components return the callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="immediately">If set to <c>true</c> immediately.</param>
        private void OnHideBegin_INTERNAL(System.Action callback, bool immediately) {

			if (this == null) {
				
				if (callback != null) callback();

				return;

			}

			System.Action callbackInner = () => {

				if (callback != null) callback();

			};
			
			this.SetComponentState(WindowObjectState.Hiding);
			
			if (this.animation != null) {
				
				if (immediately == true) {
					
					this.animation.SetOutState(this.animationInputParams, this.GetWindow(), this);
					callbackInner();
					
				} else {
					
					this.animation.Play(this.GetWindow(), this.animationInputParams, this, false, callbackInner);
					
				}
				
			} else {
				
				callbackInner();
				
			}

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			this.Update_EDITOR();

		}
		
		[HideInInspector]
		private List<TransitionInputParameters> componentsToDestroy = new List<TransitionInputParameters>();

		[HideInInspector][SerializeField]
		private WindowAnimationBase lastAnimation;

		private void Update_EDITOR() {

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