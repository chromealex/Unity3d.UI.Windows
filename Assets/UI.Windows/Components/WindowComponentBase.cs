using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

		public void Show(bool resetAnimation = true) {
			
			this.Show(null, resetAnimation);
			
		}
		
		public virtual void Show(System.Action callback, bool resetAnimation) {
			
			this.OnShowBegin(callback, resetAnimation);
			
		}
		
		public void Hide() {
			
			this.Hide(null, immediately: false);
			
		}

		public void Hide(bool immediately) {

			this.Hide(null, immediately);

		}
		
		public virtual void Hide(System.Action callback, bool immediately) {

			this.OnHideBegin(callback, immediately);
			
		}
		
		public void SetResetState() {
			
			if (this.animation != null) this.animation.SetResetState(this.animationInputParams, this);
			
		}
		
		public void SetInState() {
			
			if (this.animation != null) this.animation.SetInState(this.animationInputParams, this);
			
		}
		
		public void SetOutState() {
			
			if (this.animation != null) this.animation.SetOutState(this.animationInputParams, this);
			
		}
		
		public virtual float GetAnimationDuration(bool forward) {
			
			if (this.animation != null) {
				
				return this.animation.GetDuration(this.animationInputParams, forward);
				
			}
			
			return 0f;
			
		}
		
		public virtual void OnShowBegin(System.Action callback) {
			
			this.OnShowBegin(callback, resetAnimation: true);
			
		}

		protected virtual void OnShowBegin(System.Action callback, bool resetAnimation) {

			if (this.animation != null) {
				
				if (resetAnimation == true) this.SetResetState();
				this.animation.Play(this.animationInputParams, this, true, callback);
				
			} else {
				
				if (callback != null) callback();
				
			}
			
		}

		public virtual void OnHideBegin(System.Action callback) {

			this.OnHideBegin(callback, immediately: false);

		}

		protected virtual void OnHideBegin(System.Action callback, bool immediately) {

			if (this.animation != null) {

				if (immediately == true) {

					this.animation.SetOutState(this.animationInputParams, this);
					if (callback != null) callback();

				} else {

					this.animation.Play(this.animationInputParams, this, false, callback);

				}

			} else {

				if (callback != null) callback();
				
			}
			
		}
		
		public virtual void OnInit() {}
		public virtual void OnDeinit() {}
		public virtual void OnShowEnd() {}
		public virtual void OnHideEnd() {}
		
		#if UNITY_EDITOR
		public void OnValidate() {

			this.OnValidateEditor();

		}

		public virtual void OnValidateEditor() {
			
			this.Update_EDITOR();
			
		}
		
		private List<TransitionInputParameters> componentsToDestroy = new List<TransitionInputParameters>();
		
		[HideInInspector][SerializeField]
		public WindowAnimationBase lastAnimation;
		public virtual void Update_EDITOR() {

			this.animationInputParams = this.animationInputParams.Where((i) => i != null).ToList();

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
			
			this.canvas = this.GetComponent<CanvasGroup>();

		}
		#endif

	}

}