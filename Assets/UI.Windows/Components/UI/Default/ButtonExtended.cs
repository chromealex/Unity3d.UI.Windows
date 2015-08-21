using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace UnityEngine.UI {

	public class ButtonExtended : Button {

		[System.Serializable]
		public struct AlphaBlock {

			//
			// Static Properties
			//
			public static AlphaBlock defaultAlphaBlock {

				get {

					return new AlphaBlock {
						normalAlpha = 0.7f,
						highlightedAlpha = 0.9f,
						pressedAlpha = 1f,
						disabledAlpha = 0.3f,
						alphaMultiplier = 1f,
						fadeDuration = 0.1f
					};

				}

			}
			
			public CanvasGroup canvasGroup;

			public float normalAlpha;
			public float highlightedAlpha;
			public float pressedAlpha;
			public float disabledAlpha;

			[Range(1f, 5f)]
			public float alphaMultiplier;
			public float fadeDuration;

		}

		public enum Transition {

			// Defaults
			None = 0,
			ColorTint = 1,
			SpriteSwap = 2,
			Animation = 3,

			// Addons
			SpriteSwapAndColorTint = 4,
			CanvasGroupAlpha = 5,

		};

		[FormerlySerializedAs("alpha")]
		[SerializeField]
		private AlphaBlock m_Alpha = AlphaBlock.defaultAlphaBlock;
		public AlphaBlock alpha {
			get {
				return this.m_Alpha;
			}
			set {
				this.m_Alpha = value;
			}
		}

		public Transition transitionExtended;
		
		private void StartAlphaTween(float targetAlpha, bool instant) {
			
			if (this.alpha.canvasGroup == null) return;

			if (instant == true) {

				this.alpha.canvasGroup.alpha = targetAlpha;

			} else {

				TweenerGlobal.instance.removeTweens(this.alpha.canvasGroup);
				TweenerGlobal.instance.addTweenAlpha(this.alpha.canvasGroup, this.alpha.fadeDuration, targetAlpha).tag(this.alpha.canvasGroup);

			}

		}
		
		private void StartColorTween(Color targetColor, bool instant) {
			
			if (this.targetGraphic == null) return;
			
			this.targetGraphic.CrossFadeColor(targetColor, instant ? 0f : this.colors.fadeDuration, true, true);
			
		}

		private void DoSpriteSwap(Sprite newSprite) {

			if (this.image == null) return;
			
			this.image.overrideSprite = newSprite;

		}
		
		private void TriggerAnimation(string triggername) {

			if (this.animator == null ||
				this.animator.runtimeAnimatorController == null ||
				string.IsNullOrEmpty(triggername)) {

				return;

			}
			
			this.animator.ResetTrigger(this.animationTriggers.normalTrigger);
			this.animator.ResetTrigger(this.animationTriggers.pressedTrigger);
			this.animator.ResetTrigger(this.animationTriggers.highlightedTrigger);
			this.animator.ResetTrigger(this.animationTriggers.disabledTrigger);
			this.animator.SetTrigger(triggername);

		}

		protected override void InstantClearState() {

			base.InstantClearState();

			switch (this.transitionExtended) {

				case Transition.CanvasGroupAlpha:
					this.StartAlphaTween(this.alpha.normalAlpha, true);
					break;
				case Transition.SpriteSwapAndColorTint:
					this.StartColorTween(Color.white, true);
					this.DoSpriteSwap(null);
					break;
				case Transition.ColorTint:
					this.StartColorTween(Color.white, true);
					break;
				case Transition.SpriteSwap:
					this.DoSpriteSwap(null);
					break;
				case Transition.Animation:
					string triggerName = this.animationTriggers.normalTrigger;
					this.TriggerAnimation(triggerName);
					break;

			}

		}

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

			float alpha;
			Color color;
			Sprite newSprite;
			string triggername;

			switch (state) {

				case Selectable.SelectionState.Normal:
					alpha = this.alpha.normalAlpha;
					color = this.colors.normalColor;
					newSprite = null;
					triggername = this.animationTriggers.normalTrigger;
				break;
				case Selectable.SelectionState.Highlighted:
					alpha = this.alpha.highlightedAlpha;
					color = this.colors.highlightedColor;
					newSprite = this.spriteState.highlightedSprite;
					triggername = this.animationTriggers.highlightedTrigger;
				break;
				case Selectable.SelectionState.Pressed:
					alpha = this.alpha.pressedAlpha;
					color = this.colors.pressedColor;
					newSprite = this.spriteState.pressedSprite;
					triggername = this.animationTriggers.pressedTrigger;
				break;
				case Selectable.SelectionState.Disabled:
					alpha = this.alpha.disabledAlpha;
					color = this.colors.disabledColor;
					newSprite = this.spriteState.disabledSprite;
					triggername = this.animationTriggers.disabledTrigger;
				break;
				default:
					alpha = 0f;
					color = Color.black;
					newSprite = null;
					triggername = string.Empty;
				break;

			}

			if (base.gameObject.activeInHierarchy) {

				switch (this.transitionExtended) {
					
					case Transition.CanvasGroupAlpha:
						this.StartAlphaTween(alpha * this.alpha.alphaMultiplier, instant);
						break;
					case Transition.SpriteSwapAndColorTint:
						this.StartColorTween(color * this.colors.colorMultiplier, instant);
						this.DoSpriteSwap(newSprite);
					break;
					case Transition.ColorTint:
						this.StartColorTween(color * this.colors.colorMultiplier, instant);
					break;
					case Transition.SpriteSwap:
						this.DoSpriteSwap(newSprite);
					break;
					case Transition.Animation:
						this.TriggerAnimation(triggername);
					break;

				}

			}

		}

	}

}