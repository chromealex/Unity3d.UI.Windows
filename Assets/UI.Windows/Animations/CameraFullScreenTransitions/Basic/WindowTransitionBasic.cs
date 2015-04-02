using UnityEngine;
using UnityEngine.UI.Windows;

namespace UnityEngine.UI.Windows.Animations {

	public class WindowTransitionBasic : TransitionBase {
		
		[System.Serializable]
		public class Parameters : TransitionBase.ParametersBase {
			
			public Parameters(TransitionBase.ParametersBase baseDefaults) : base(baseDefaults) {}
			
			[System.Serializable]
			public class State {
				
				public float to;
				
			}
			
			public Material material;

			public State resetState;
			public State inState;
			public State outState;
			
			public override void Setup(TransitionBase.ParametersBase defaults) {
				
				var param = defaults as Parameters;
				if (param == null) return;
				
				// Place params installation here
				this.inState = param.inState;
				this.outState = param.outState;
				this.resetState = param.resetState;

				this.material = param.material;
				
			}
			
			public float GetIn() {
				
				return this.inState.to;
				
			}
			
			public float GetOut() {
				
				return this.outState.to;
				
			}
			
			public float GetReset() {
				
				return this.resetState.to;
				
			}
			
			public float GetResult(bool forward) {
				
				if (forward == true) {
					
					return this.inState.to;
					
				}
				
				return this.outState.to;
				
			}
			
		}
		
		public Parameters defaultInputParams;

		public Material material {

			get {

				return this.defaultInputParams.material;

			}

		}

		public bool grabEveryFrame = false;
		public CameraClearFlags clearFlags = CameraClearFlags.Depth;

		private Texture2D clearScreen;
		private WindowBase currentWindow;
		private WindowBase prevWindow;

		public override TransitionBase.ParametersBase GetDefaultInputParameters() {
			
			return this.defaultInputParams;
			
		}

		public override void OnInit() {
			
			this.clearScreen = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);

		}

		public override void SetupCamera(Camera camera) {

			camera.clearFlags = this.clearFlags;

		}

		public override bool IsValid(WindowBase window) {

			return this.currentWindow == window || this.prevWindow == window;

		}

		public override void OnPlay(WindowBase window, object tag, TransitionInputParameters parameters, WindowComponentBase root, bool forward, System.Action callback) {

			var param = this.GetParams<Parameters>(parameters);
			if (param == null) {
				
				if (callback != null) callback();
				return;
				
			}

			this.currentWindow = window;

			this.prevWindow = WindowSystem.GetPreviousWindow(window);
			if (this.prevWindow == null) {

				window.transition.SaveToCache(this.clearScreen, () => {

					this.material.SetTexture("_ClearScreen", this.clearScreen);
					
				}, this.grabEveryFrame);

			} else {

				if (forward == true) {

					// Take screenshot from current view
					this.prevWindow.transition.SaveToCache(this.clearScreen, () => {

						this.material.SetTexture("_ClearScreen", this.clearScreen);
						
					}, this.grabEveryFrame);

				} else {

					// Take screenshot from previous view
					this.prevWindow.transition.SaveToCache(this.clearScreen, () => {

						this.material.SetTexture("_ClearScreen", this.clearScreen);

					}, this.grabEveryFrame);

				}
				
			}

			var duration = this.GetDuration(parameters, forward);
			var result = param.GetResult(forward);

			TweenerGlobal.instance.removeTweens(tag);
			TweenerGlobal.instance.addTween(this, duration, this.material.GetFloat("_Value"), result).onUpdate((obj, value) => {

				this.material.SetFloat("_Value", value);

			}).onComplete((obj) => { if (callback != null) callback(); }).onCancel((obj) => { if (callback != null) callback(); }).tag(tag);
			
		}
		
		public override void SetInState(TransitionInputParameters parameters, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			this.material.SetFloat("_Value", param.GetIn());
			
		}
		
		public override void SetOutState(TransitionInputParameters parameters, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			this.material.SetFloat("_Value", param.GetOut());
			
		}
		
		public override void SetResetState(TransitionInputParameters parameters, WindowComponentBase root) {
			
			var param = this.GetParams<Parameters>(parameters);
			if (param == null) return;
			
			this.material.SetFloat("_Value", param.GetReset());

		}

		public override void OnRenderTransition(WindowBase window, RenderTexture source, RenderTexture destination) {

			if (this.currentWindow == window) {

				Graphics.Blit(source, destination, this.material);

			}

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Transitions/Screen/Basic")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowTransitionBasic>();
			
		}
		#endif

	}

}