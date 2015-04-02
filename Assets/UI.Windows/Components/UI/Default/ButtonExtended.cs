using UnityEngine;
using System.Collections;

namespace UnityEngine.UI {

	public class ButtonExtended : Button {

		public enum Transition {

			// Defaults
			None = 0,
			ColorTint = 1,
			SpriteSwap = 2,
			Animation = 3,

			// Addons
			SpriteSwapAndColorTint = 4,

		};

		public Transition transitionExtended;

		void StartColorTween(Color targetColor, bool instant) {

			if (this.targetGraphic == null)
				return;
			
			this.targetGraphic.CrossFadeColor(targetColor, instant ? 0f : this.colors.fadeDuration, true, true);

		}
		
		void DoSpriteSwap(Sprite newSprite) {

			if (this.image == null)
				return;
			
			this.image.overrideSprite = newSprite;

		}
		
		void TriggerAnimation(string triggername) {

			if (this.animator == null || this.animator.runtimeAnimatorController == null || string.IsNullOrEmpty(triggername))
				return;
			
			this.animator.ResetTrigger(this.animationTriggers.normalTrigger);
			this.animator.ResetTrigger(this.animationTriggers.pressedTrigger);
			this.animator.ResetTrigger(this.animationTriggers.highlightedTrigger);
			this.animator.ResetTrigger(this.animationTriggers.disabledTrigger);
			this.animator.SetTrigger(triggername);

		}

		protected override void InstantClearState()
		{
			base.InstantClearState();

			string triggerName = this.animationTriggers.normalTrigger;

			switch (this.transitionExtended)
			{
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
					this.TriggerAnimation(triggerName);
					break;
			}
		}

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

			Color color;
			Sprite newSprite;
			string triggername;

			switch (state) {

				case Selectable.SelectionState.Normal:
					color = this.colors.normalColor;
					newSprite = null;
					triggername = this.animationTriggers.normalTrigger;
				break;
				case Selectable.SelectionState.Highlighted:
					color = this.colors.highlightedColor;
					newSprite = this.spriteState.highlightedSprite;
					triggername = this.animationTriggers.highlightedTrigger;
				break;
				case Selectable.SelectionState.Pressed:
					color = this.colors.pressedColor;
					newSprite = this.spriteState.pressedSprite;
					triggername = this.animationTriggers.pressedTrigger;
				break;
				case Selectable.SelectionState.Disabled:
					color = this.colors.disabledColor;
					newSprite = this.spriteState.disabledSprite;
					triggername = this.animationTriggers.disabledTrigger;
				break;
				default:
					color = Color.black;
					newSprite = null;
					triggername = string.Empty;
				break;

			}

			if (base.gameObject.activeInHierarchy) {

				switch (this.transitionExtended) {

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