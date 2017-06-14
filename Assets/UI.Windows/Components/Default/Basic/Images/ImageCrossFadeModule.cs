using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using UnityEngine.Extensions;
using ME;

namespace UnityEngine.UI.Windows.Components.Modules {

	[System.Serializable]
	public class ImageCrossFadeModule : ComponentModuleBase {

		public enum DataType : byte {

			Sprite,
			Texture,
			Material,

		};

		public float duration = 0.5f;
		//[ReadOnly("enabled", state: false)]
		public bool fadeIfWasNull = false;
		//[ReadOnly("enabled", state: false)]
		public bool fadeOutSource = false;
		//[ReadOnly("enabled", state: false)]
		public ME.Ease.Type easeType;

		//[ReadOnly("enabled", state: false)]
		public Color startColor = new Color(1f, 1f, 1f, 0f);
		//[ReadOnly("enabled", state: false)]
		[ReadOnlyEndGroup("enabled", state: false)]
		public Color targetColor = new Color(1f, 1f, 1f, 1f);

		private IImageComponent image;
		private Graphic source;

		public override void Init(IComponent source) {

			base.Init(source);

			this.image = source as IImageComponent;

		}

		public override void SetDisabled() {

			base.SetDisabled();

			this.source.color = this.targetColor;

		}

		public override void Prepare(IComponent source) {

			base.Prepare(source);

			this.source = this.image.GetGraphicSource();

			if (this.source.mainTexture == null || this.source.mainTexture == Texture2D.whiteTexture) {

				this.source.color = this.startColor;

			}

		}

		public void FadeTo(IImageComponent source, Sprite to, System.Action callback = null) {

			this.FadeTo<Image>(source, to, this.duration, this.fadeIfWasNull, callback, DataType.Sprite);

		}

		public void FadeTo(IImageComponent source, Texture to, System.Action callback = null) {

			this.FadeTo<RawImage>(source, to, this.duration, this.fadeIfWasNull, callback, DataType.Texture);

		}

		public void FadeTo(IImageComponent source, Sprite to, float duration, System.Action callback = null) {

			this.FadeTo<Image>(source, to, duration, this.fadeIfWasNull, callback, DataType.Sprite);

		}

		public void FadeTo(IImageComponent source, Texture to, float duration, System.Action callback = null) {

			this.FadeTo<RawImage>(source, to, duration, this.fadeIfWasNull, callback, DataType.Texture);

		}

		public void FadeTo<T>(IImageComponent source, Object to, System.Action callback, DataType dataType) where T : Graphic {

			this.FadeTo<T>(source, to, this.duration, this.fadeIfWasNull, callback, dataType);

		}

		private void FadeTo<T>(IImageComponent imageSource, Object to, float duration, bool fadeIfWasNull, System.Action callback, DataType dataType) where T : Graphic {
			
			var isSprite = (dataType == DataType.Sprite);
			var isTexture = (dataType == DataType.Texture);
			var isMaterial = (dataType == DataType.Material);
			var hasSourceTexture = false;

			var hasChanged = false;
			if (isMaterial == true) {

				hasChanged = (this.source.material != to);

			} else if (isTexture == true) {

				var sourceImage = this.image.GetRawImageSource();
				hasChanged = (sourceImage.texture != to);

			} else if (isSprite == true) {

				var sourceImage = this.image.GetImageSource();
				hasChanged = (sourceImage.sprite != to);

			}

			if (hasChanged == false) {

				if (callback != null) callback.Invoke();
				return;

			}

			var copy = this.MakeCopy<T>(this.source.rectTransform);

			if (isMaterial == true) {

				copy.material = to as Material;
				hasSourceTexture = (this.source.material != null && this.source.material != this.source.defaultMaterial);

			} else if (isTexture == true) {

				var sourceImage = this.image.GetRawImageSource();
				var copyImage = (copy as RawImage);
				hasSourceTexture = (sourceImage.texture != null);

				copyImage.material = (this.source.defaultMaterial == this.source.material ? null : this.source.material);
				copyImage.texture = to as Texture;
				copyImage.uvRect = sourceImage.uvRect;

			} else if (isSprite == true) {

				var sourceImage = this.image.GetImageSource();
				var copyImage = (copy as Image);
				hasSourceTexture = (sourceImage.sprite != null);

				copyImage.material = (this.source.defaultMaterial == this.source.material ? null : this.source.material);
				copyImage.sprite = to as Sprite;
				copyImage.preserveAspect = sourceImage.preserveAspect;
				copyImage.fillAmount = sourceImage.fillAmount;
				copyImage.fillCenter = sourceImage.fillCenter;
				copyImage.fillClockwise = sourceImage.fillClockwise;
				copyImage.fillMethod = sourceImage.fillMethod;
				copyImage.fillOrigin = sourceImage.fillOrigin;

			}

			TweenerGlobal.instance.removeTweens(source, immediately: true);
			TweenerGlobal.instance.removeTweens(copy, immediately: true);

			var sourceColor = this.targetColor;

			if (hasSourceTexture == true || fadeIfWasNull == true) {

				copy.color = this.startColor;//new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0f);

				if (hasSourceTexture == false) {

					source.color = this.startColor;//new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0f);

				}

				if (this.fadeOutSource == true) TweenerGlobal.instance.addTweenAlpha(source, duration, 0f).tag(source).ease(ME.Ease.GetByType(this.easeType));
				TweenerGlobal.instance.addTweenAlpha(copy, this.duration, 1f).tag(copy).ease(ME.Ease.GetByType(this.easeType)).onComplete(() => {

					if (source != null) source.color = sourceColor;
					this.Finalize(isSprite, isTexture, isMaterial, source, copy);

					if (callback != null) callback.Invoke();

				}).onCancel(() => {

					if (callback != null) callback.Invoke();

				});

			} else {

				if (copy != null) copy.color = sourceColor;
				if (source != null) source.color = sourceColor;
				this.Finalize(isSprite, isTexture, isMaterial, source, copy);

				if (callback != null) callback.Invoke();

			}

		}

		private void Finalize(bool isSprite, bool isTexture, bool isMaterial, Graphic source, Graphic copy) {

			if (isMaterial == true) {

				var sourceImage = source;
				if (sourceImage != null) {

					sourceImage.material = (copy.material == sourceImage.defaultMaterial ? null : copy.material);
					sourceImage.SetMaterialDirty();

				}

			} else if (isSprite == true) {

				var sourceImage = source as Image;
				if (sourceImage != null) {

					sourceImage.sprite = (copy as Image).sprite;

				}

			} else if (isTexture == true) {

				var sourceImage = source as RawImage;
				if (sourceImage != null) {

					var texture = (copy as RawImage).texture;
					if (texture.GetID() != sourceImage.texture.GetID()) {

						MovieSystem.Stop(this.image, sourceImage.texture.GetID());

					}

					sourceImage.texture = texture;

				}

			}

			if (copy != null) {

				GameObject.Destroy(copy.gameObject);

			}

		}

		protected override T MakeCopy<T>(RectTransform transform) {

			var component = base.MakeCopy<T>(transform);

			this.CopyFlippable(this.image.IsHorizontalFlip(), this.image.IsVerticalFlip(), component.gameObject.AddComponent<UIFlippable>());
			this.CopyPreserveAspect(this.image.IsPreserveAspect(), component, component.gameObject.AddComponent<UIPreserveAspect>());

			return component;

		}

		private void CopyFlippable(bool isHorizontal, bool isVertical, UIFlippable copy) {

			copy.horizontal = isHorizontal;
			copy.vertical = isVertical;

		}

		private void CopyPreserveAspect(bool isPreserveAspect, Graphic graphic, UIPreserveAspect copy) {

			copy.preserveAspect = isPreserveAspect;
			copy.rawImage = graphic as RawImage;

		}

	}

}