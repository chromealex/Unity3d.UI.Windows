using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI {

	public class ButtonExtended : Button {
		
		[System.Serializable]
		public struct ScaleBlock {
			
			//
			// Static Properties
			//
			public static ScaleBlock defaultScaleBlock {
				
				get {
					
					return new ScaleBlock {
						normalScale = 1f,
						highlightedScale = 1.1f,
						pressedScale = 0.8f,
						disabledScale = 1f,
						scaleMultiplier = 1f,
						fadeDuration = 0.1f
					};
					
				}
				
			}
			
			public Transform transform;
			
			public float normalScale;
			public float highlightedScale;
			public float pressedScale;
			public float disabledScale;
			
			[Range(1f, 5f)]
			public float scaleMultiplier;
			public float fadeDuration;
			
		}

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

		new public enum Transition : byte {

			// Defaults
			None = 0x0,

			ColorTint = 0x1,
			SpriteSwap = 0x2,
			Animation = 0x4,

			// Addons
			SpriteSwapAndColorTint = 0x8,
			CanvasGroupAlpha = 0x10,
			Scale = 0x20,
			TargetGraphics = 0x40,

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
		
		[FormerlySerializedAs("scale")]
		[SerializeField]
		private ScaleBlock m_Scale = ScaleBlock.defaultScaleBlock;
		public ScaleBlock scale {
			get {
				return this.m_Scale;
			}
			set {
				this.m_Scale = value;
			}
		}

		[BitMask(typeof(Transition))]
		public Transition transitionExtended;

		public Graphic[] m_TargetGraphics;

		[System.Serializable]
		public class GraphicItem {

			public bool colorEnabled = true;
			[ReadOnly("colorEnabled", state: false)]
			public Graphic targetGraphic;
			[FormerlySerializedAs("colors")]
			[ReadOnly("colorEnabled", state: false)][SerializeField]
			private ColorBlock m_Colors = ColorBlock.defaultColorBlock;
			public ColorBlock colors {
				get {
					return this.m_Colors;
				}
				set {
					this.m_Colors = value;
				}
			}

			public bool scaleEnabled = false;
			[FormerlySerializedAs("scale")]
			[ReadOnly("scaleEnabled", state: false)][SerializeField]
			private ScaleBlock m_Scale = ScaleBlock.defaultScaleBlock;
			public ScaleBlock scale {
				get {
					return this.m_Scale;
				}
				set {
					this.m_Scale = value;
				}
			}

			public bool spritesEnabled = false;
			[FormerlySerializedAs("sprite")]
			[ReadOnly("spritesEnabled", state: false)][SerializeField]
			private SpriteState m_Sprite;
			public SpriteState sprite {
				get {
					return this.m_Sprite;
				}
				set {
					this.m_Sprite = value;
				}
			}

		}

		public GraphicItem[] graphicItems;

		private IHoverableComponent containerComponent;
		public void Setup(IHoverableComponent component) {

			this.containerComponent = component;

		}

		public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData) {

			base.OnPointerEnter(eventData);

			if (this.containerComponent != null) this.containerComponent.SetHoverEnter();

		}

		public override void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData) {

			base.OnPointerExit(eventData);

			if (this.containerComponent != null) this.containerComponent.SetHoverExit();

		}

		public void Select(bool forced) {

			this.DoStateTransition(this.currentSelectionState, instant: false);

		}

		public void UpdateState() {
			
			this.OnDidApplyAnimationProperties();

		}

		private void StartScaleTween(float targetScale, bool instant) {
			
			if (this.scale.transform == null) return;

			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(this.scale.transform);

			if (instant == true) {
				
				this.scale.transform.localScale = Vector3.one * targetScale;
				
			} else {
				
				TweenerGlobal.instance.addTweenScale(this.scale.transform, this.scale.fadeDuration, Vector3.one * targetScale).tag(this.scale.transform);
				
			}
			
		}

		private void StartAlphaTween(float targetAlpha, bool instant) {

			if (this.alpha.canvasGroup == null) return;

			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(this.alpha.canvasGroup);

			if (instant == true) {

				this.alpha.canvasGroup.alpha = targetAlpha;

			} else {

				TweenerGlobal.instance.addTweenAlpha(this.alpha.canvasGroup, this.alpha.fadeDuration, targetAlpha).tag(this.alpha.canvasGroup);

			}

		}

		private void StartColorTweenGraphics(SelectionState state, bool instant) {

			if (this.graphicItems == null) return;

			var targetColor = Color.white;
			var targetScale = 1f;
			Sprite newSprite = null;
			for (int i = 0; i < this.graphicItems.Length; ++i) {

				var item = this.graphicItems[i];
				if (item == null || item.targetGraphic == null) continue;

				switch (state) {

					case SelectionState.Normal:
						targetColor = item.colors.normalColor;
						targetScale = item.scale.normalScale;
						newSprite = null;
						break;

					case SelectionState.Disabled:
						targetColor = item.colors.disabledColor;
						targetScale = item.scale.disabledScale;
						newSprite = item.sprite.disabledSprite;
						break;

					case SelectionState.Highlighted:
						targetColor = item.colors.highlightedColor;
						targetScale = item.scale.highlightedScale;
						newSprite = item.sprite.highlightedSprite;
						break;

					case SelectionState.Pressed:
						targetColor = item.colors.pressedColor;
						targetScale = item.scale.pressedScale;
						newSprite = item.sprite.pressedSprite;
						break;

				}

				if (item.colorEnabled == true) {

					item.targetGraphic.CrossFadeColor(targetColor * item.colors.colorMultiplier, instant ? 0f : item.colors.fadeDuration, true, true);

				}

				if (item.scaleEnabled == true) {

					if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(item.scale.transform);

					if (instant == true) {

						item.scale.transform.localScale = Vector3.one * targetScale * item.scale.scaleMultiplier;

					} else {

						TweenerGlobal.instance.addTweenScale(item.scale.transform, item.scale.fadeDuration, Vector3.one * targetScale * item.scale.scaleMultiplier).tag(item.scale.transform);

					}

				}

				if (item.spritesEnabled == true) {

					var image = (item.targetGraphic as Image);
					if (image != null) image.overrideSprite = newSprite;

				}

			}

		}

		private void StartColorTween(Color targetColor, bool instant) {

			if (this == null || this.IsDestroyed() == true) return;

			if (this.m_TargetGraphics != null) {

				for (int i = 0; i < this.m_TargetGraphics.Length; ++i) {

					if (this.m_TargetGraphics[i] != null) this.m_TargetGraphics[i].CrossFadeColor(targetColor, instant ? 0f : this.colors.fadeDuration, true, true);

				}

			}

			if (this.targetGraphic == null) return;
			
			if (this.targetGraphic != null) this.targetGraphic.CrossFadeColor(targetColor, instant ? 0f : this.colors.fadeDuration, true, true);

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

			if ((this.transitionExtended & Transition.Scale) != 0) {
				
				this.StartScaleTween(this.scale.normalScale, true);
				
			}
			
			if ((this.transitionExtended & Transition.CanvasGroupAlpha) != 0) {
				
				this.StartAlphaTween(this.alpha.normalAlpha, true);
				
			}
			
			if ((this.transitionExtended & Transition.ColorTint) != 0) {
				
				this.StartColorTween(this.colors.normalColor, true);
				
			}
			
			if ((this.transitionExtended & Transition.SpriteSwap) != 0) {
				
				this.DoSpriteSwap(null);
				
			}
			
			if ((this.transitionExtended & Transition.Animation) != 0) {
				
				string triggerName = this.animationTriggers.normalTrigger;
				this.TriggerAnimation(triggerName);

			}

			if ((this.transitionExtended & Transition.TargetGraphics) != 0) {

				this.StartColorTweenGraphics(Selectable.SelectionState.Normal, true);

			}

		}

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {
			
			float scale;
			float alpha;
			Color color;
			Sprite newSprite;
			string triggername;

			switch (state) {

				case Selectable.SelectionState.Normal:
					scale = this.scale.normalScale;
					alpha = this.alpha.normalAlpha;
					color = this.colors.normalColor;
					newSprite = null;
					triggername = this.animationTriggers.normalTrigger;
				break;
				case Selectable.SelectionState.Highlighted:
					scale = this.scale.highlightedScale;
					alpha = this.alpha.highlightedAlpha;
					color = this.colors.highlightedColor;
					newSprite = this.spriteState.highlightedSprite;
					triggername = this.animationTriggers.highlightedTrigger;
				break;
				case Selectable.SelectionState.Pressed:
					scale = this.scale.pressedScale;
					alpha = this.alpha.pressedAlpha;
					color = this.colors.pressedColor;
					newSprite = this.spriteState.pressedSprite;
					triggername = this.animationTriggers.pressedTrigger;
				break;
				case Selectable.SelectionState.Disabled:
					scale = this.scale.disabledScale;
					alpha = this.alpha.disabledAlpha;
					color = this.colors.disabledColor;
					newSprite = this.spriteState.disabledSprite;
					triggername = this.animationTriggers.disabledTrigger;
				break;
				default:
					scale = 0f;
					alpha = 0f;
					color = Color.black;
					newSprite = null;
					triggername = string.Empty;
				break;

			}

			//if (base.gameObject.activeInHierarchy == true) {
				
				if ((this.transitionExtended & Transition.Scale) != 0) {
					
					this.StartScaleTween(scale * this.scale.scaleMultiplier, instant);
					
				}
				
				if ((this.transitionExtended & Transition.CanvasGroupAlpha) != 0) {
					
					this.StartAlphaTween(alpha * this.alpha.alphaMultiplier, instant);
					
				}
				
				if ((this.transitionExtended & Transition.ColorTint) != 0) {
					
					this.StartColorTween(color * this.colors.colorMultiplier, instant);
					
				}
				
				if ((this.transitionExtended & Transition.SpriteSwap) != 0) {
					
					this.DoSpriteSwap(newSprite);
					
				}
				
				if ((this.transitionExtended & Transition.Animation) != 0) {
					
					this.TriggerAnimation(triggername);
					
				}

				if ((this.transitionExtended & Transition.TargetGraphics) != 0) {

					this.StartColorTweenGraphics(state, instant);

				}

			//}

		}

		/*new public bool interactable {

			get {
				
				return base.interactable;

			}

			set {

				base.interactable = value;

				//this.DoStateTransition(SelectionState.Disabled, Application.isPlaying == false);

			}

		}*/

	}

}