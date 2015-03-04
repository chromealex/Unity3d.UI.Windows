using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows {

	//[ExecuteInEditMode()]
	public class WindowLayoutBase : WindowComponentBase, IWindowAnimation {

		[Header("Animation Info")]
		new public WindowAnimationBase animation;
		[HideInInspector]
		public List<TransitionInputParameters> animationInputParams = new List<TransitionInputParameters>();
		[HideInInspector]
		public CanvasGroup canvas;
		[HideInInspector]
		public bool animationRefresh = false;

		public void Show(bool resetAnimation = true) {
			
			this.OnShowBegin_INTERNAL(null, resetAnimation);

		}
		
		public void Hide() {
			
			this.OnHideBegin(null);
			
		}
		
		public void Hide(System.Action callback) {
			
			this.OnHideBegin(callback);
			
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
		public virtual void OnValidate() {

			this.Update_EDITOR();

		}

		private List<TransitionInputParameters> componentsToDestroy = new List<TransitionInputParameters>();

		[HideInInspector][SerializeField]
		public WindowAnimationBase lastAnimation;
		public virtual void Update_EDITOR() {

			this.canvas = this.GetComponent<CanvasGroup>();

			this.animationInputParams = this.animationInputParams.Where((i) => i != null).ToList();

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
			
			UnityEditor.EditorApplication.delayCall += () => {

				foreach (var c in this.componentsToDestroy) {

					Component.DestroyImmediate(c);
					this.animationInputParams = this.animationInputParams.Where((i) => i != null).ToList();

				}

			};

		}
		#endif

	}

}