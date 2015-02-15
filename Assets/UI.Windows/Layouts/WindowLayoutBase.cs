using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	//[ExecuteInEditMode()]
	public class WindowLayoutBase : WindowComponentBase, IWindowAnimation {
		
		public CanvasGroup canvas;
		new public WindowAnimationBase animation;
		public List<TransitionInputParameters> animationInputParams = new List<TransitionInputParameters>();
		public bool animationRefresh = false;

		protected void Show(bool resetAnimation = true) {
			
			this.OnShowBegin_INTERNAL(null, resetAnimation);

		}

		protected void Hide() {

			this.OnHideBegin(null);

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
			
			this.OnShowBegin_INTERNAL(callback, resetAnimation: true);
			
		}
		
		private void OnShowBegin_INTERNAL(System.Action callback, bool resetAnimation = true) {
			
			if (this.animation != null) {

				if (resetAnimation == true) this.SetResetState();
				this.animation.Play(this.animationInputParams, this, true, callback);
				
			} else {
				
				if (callback != null) callback();
				
			}
			
		}

		public virtual void OnHideBegin(System.Action callback) {
			
			if (this.animation != null) {
				
				this.animation.Play(this.animationInputParams, this, false, callback);
				
			} else {
				
				if (callback != null) callback();
				
			}
			
		}
		
		private WindowComponent component;
		public virtual void Setup(WindowComponent component) {
			
			this.component = component;
			this.component.Setup(this);
			
		}
		
		public virtual WindowComponent GetCurrentComponent() {
			
			return this.component;
			
		}

		public virtual void OnInit() {}
		public virtual void OnDeinit() {}
		public virtual void OnShowEnd() {}
		public virtual void OnHideEnd() {}
		
		#if UNITY_EDITOR
		protected virtual void Update() {
			
			if (Application.isPlaying == true) return;

			this.Update_EDITOR();
			
		}

		[HideInInspector][SerializeField]
		public WindowAnimationBase lastAnimation;
		public virtual void Update_EDITOR() {

			if (this.animation == null || this.lastAnimation != this.animation || this.animationRefresh == false) {
				
				var ps = this.GetComponents<TransitionInputParameters>();
				foreach (var p in ps) Component.DestroyImmediate(p);
				
				this.animationRefresh = false;
				
			}
			
			if (this.animationRefresh == false && this.animation != null) {
				
				this.animation.ApplyInputParameters(this);
				this.animationRefresh = true;
				
			}
			
			this.lastAnimation = this.animation;

		}
		#endif

	}

}