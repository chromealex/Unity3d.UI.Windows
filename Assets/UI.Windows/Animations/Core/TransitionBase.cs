using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Animations {

	public class TransitionCameraAttribute : System.Attribute {
	};

	public class TransitionBase : ScriptableObject {

		[System.Serializable]
		public abstract class ParametersBase {
			
			[Header("Base")]
			public float inDelay;
			public float outDelay;
			
			public float inDuration;
			public float outDuration;
			
			public ME.Ease.Type inEase;
			public ME.Ease.Type outEase;

			public ParametersBase() {}
			
			public ParametersBase(ParametersBase defaults) {
				
				this.inDuration = defaults.inDuration;
				this.outDuration = defaults.outDuration;
				
				this.inDelay = defaults.inDelay;
				this.outDelay = defaults.outDelay;
				
				this.inEase = defaults.inEase;
				this.outEase = defaults.outEase;

			}
			
			public abstract void Setup(ParametersBase defaults);

		}
		
		[System.Serializable]
		public class ParametersAudioBase : ParametersBase {
			
			public ParametersAudioBase() {}
			
			public ParametersAudioBase(ParametersBase defaults) : base(defaults) {

				this.Setup(defaults);

			}
			
			public override void Setup(ParametersBase defaults) {
			}
			
			public virtual float GetVolumeValueIn(float value) {
				
				return 0f;
				
			}
			
			public virtual float GetVolumeValueOut(float value) {
				
				return 0f;
				
			}

		}

		[System.Serializable]
		public class ParametersVideoBase : ParametersBase {

			public const string MATERIAL_STRENGTH_NAME_DEFAULT = "_Value";
			
			[Header("Material")]
			public Material material;
			public string materialStrengthName;
			public bool materialLerpA = false;
			public bool materialLerpB = false;

			private Material materialInstance;

			public ParametersVideoBase() {}

			public ParametersVideoBase(ParametersBase defaults) : base(defaults) {

				var pars = defaults as ParametersVideoBase;
				if (pars != null) {

					this.material = pars.material;
					this.materialStrengthName = pars.materialStrengthName;
					this.materialLerpA = pars.materialLerpA;
					this.materialLerpB = pars.materialLerpB;

				}

				this.Setup(defaults);

			}
			
			public override void Setup(ParametersBase defaults) {
			}

			public string GetMaterialStrengthName() {

				var name = this.materialStrengthName;
				if (string.IsNullOrEmpty(name) == true) name = ParametersVideoBase.MATERIAL_STRENGTH_NAME_DEFAULT;

				return name;

			}

			public void ResetMaterialInstance() {

				this.materialInstance = null;

			}

			public virtual Material GetMaterialInstance() {
				
				if (this.material == null) return null;
				if (this.materialInstance != null) return this.materialInstance;
				
				this.materialInstance = new Material(this.material);
				return this.materialInstance;
				
			}

		}

		public virtual ParametersBase GetDefaultInputParameters() {

			return null;

		}

		public virtual void SetInState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {

			var tag = this.GetTag(window, parameters);
			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(tag);

		}
		
		public virtual void SetOutState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {

			var tag = this.GetTag(window, parameters);
			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(tag);

		}
		
		public virtual void SetResetState(TransitionInputParameters parameters, WindowBase window, WindowComponentBase root) {

			var tag = this.GetTag(window, parameters);
			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(tag);

		}

		/*public void Play(WindowAnimationBase transitionBase, WindowBase window, bool forward, System.Action callback) {

			var tag = new ME.Tweener.MultiTag() { tag1 = window };//((window != null) ? window.GetInstanceID() : 0).ToString();
			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(tag);

			this.Play(tag, transitionBase, window, null, null, forward, callback);

		}*/

		public void Play(WindowAnimationBase transitionBase, WindowBase window, TransitionInputParameters parameters, bool forward, System.Action callback) {

			var tag = this.GetTag(window, parameters);
			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(tag);

			this.Play(tag, transitionBase, window, parameters, null, forward, callback);

		}

		public virtual void Set(WindowBase window, TransitionInputParameters parameters, WindowComponentBase root, bool forward, float value) {



		}

		public void Play(ME.Tweener.MultiTag tag, WindowAnimationBase transitionBase, WindowBase window, TransitionInputParameters parameters, WindowComponentBase root, bool forward, System.Action callback) {

			var delay = this.GetDelay(parameters, forward);

			if (delay > 0f && TweenerGlobal.instance != null) {

				TweenerGlobal.instance.addTween(this, delay, 0f, 0f).tag(tag).onComplete(() => {

					this.OnPlay(window, tag, parameters, root, forward, callback);

				}).onCancel((obj) => {

					if (callback != null) callback();

				});

			} else {

				this.OnPlay(window, tag, parameters, root, forward, callback);

			}

		}

		private ME.Tweener.MultiTag GetTag(WindowBase window, TransitionInputParameters parameters) {

			//return string.Format("{0}_{1}", (window != null) ? window.GetInstanceID() : 0, (parameters != null) ? parameters.GetInstanceID() : 0);

			return new ME.Tweener.MultiTag() { tag1 = window, tag2 = parameters };

		}

		public virtual void OnInit() {}

		public virtual void OnPlay(WindowBase window, object tag, TransitionInputParameters parameters, WindowComponentBase root, bool forward, System.Action callback) {}

		public virtual float GetDelay(TransitionInputParameters parameters, bool forward) {
			
			var param = this.GetParams<ParametersBase>(parameters);
			
			if (forward == true) {
				
				return param.inDelay;
				
			}
			
			return param.outDelay;
			
		}
		
		public virtual float GetDuration(TransitionInputParameters parameters, bool forward) {
			
			var param = this.GetParams<ParametersBase>(parameters);
			
			if (forward == true) {
				
				return param.inDuration;
				
			}
			
			return param.outDuration;
			
		}

		public T GetParams<T>(TransitionInputParameters parameters) where T : ParametersBase {
			
			T param = null;
			if (parameters == null || parameters.useDefault == true) {

				param = this.GetDefaultInputParameters() as T;
				
			} else {
				
				param = parameters.GetParameters<T>();
				
			}
			
			return param;
			
		}
		
		public virtual void OnRenderTransition(WindowBase window, RenderTexture source, RenderTexture destination) {}

		public virtual bool IsValid(WindowBase window) { return false; }

		public virtual void SetupCamera(Camera camera) {}

	}

}