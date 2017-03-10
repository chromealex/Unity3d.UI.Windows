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

		[HideInInspector]
		[SerializeField]
		private WindowComponent image;
		[HideInInspector]
		[SerializeField]
		private Image sourceImage;
		[HideInInspector]
		[SerializeField]
		private Image copyImage;
		[HideInInspector]
		[SerializeField]
		private UIFlippable copyFlippableImage;
		[HideInInspector]
		[SerializeField]
		private RawImage sourceRawImage;
		[HideInInspector]
		[SerializeField]
		private RawImage copyRawImage;
		[HideInInspector]
		[SerializeField]
		private UIFlippable copyFlippableRawImage;
		[HideInInspector]
		[SerializeField]
		private UIPreserveAspect copyPreserveAspectRawImage;

		[ReadOnly("enabled", state: false)]
		public float duration = 0.5f;
		[ReadOnly("enabled", state: false)]
		public bool fadeIfWasNull = false;
		[ReadOnly("enabled", state: false)]
		public bool fadeOutSource = false;
		[ReadOnly("enabled", state: false)]
		public ME.Ease.Type easeType;

		[ReadOnly("enabled", state: false)]
		public Color startColor = new Color(1f, 1f, 1f, 0f);
		[ReadOnly("enabled", state: false)]
		public Color targetColor = new Color(1f, 1f, 1f, 1f);

		public void Init(IImageComponent image) {

			this.image = image as WindowComponent;

		}

		public override void ValidateTexture(Texture texture) {

			base.ValidateTexture(texture);

			if (this.copyRawImage != null && this.copyRawImage.enabled == false && this.sourceRawImage != null) this.sourceRawImage.texture = texture;
			if (this.copyRawImage != null && this.copyRawImage.enabled == true) this.copyRawImage.texture = texture;

		}

		public override void ValidateMaterial(Material material) {

			base.ValidateMaterial(material);

			if (this.copyRawImage != null && this.copyRawImage.material != null) this.copyRawImage.texture = this.copyRawImage.material.mainTexture;

		}

		public void Prepare(IImageComponent source) {

			var sourceRawImage = source.GetRawImageSource();
			var sourceImage = source.GetImageSource();

			if (sourceRawImage != null) {

				if (sourceRawImage.texture == null) {

					sourceRawImage.color = this.startColor;

				}

				if (this.copyRawImage != null) {

					this.copyRawImage.material = sourceRawImage.material;
					this.copyRawImage.enabled = false;

				}

			}

			if (sourceImage != null) {

				if (sourceImage.sprite == null) {

					sourceImage.color = this.startColor;

				}

				if (this.copyImage != null) {

					this.copyImage.material = sourceImage.material;
					this.copyImage.enabled = false;

				}

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

		public void FadeTo<T>(IImageComponent imageSource, Object to, float duration, bool fadeIfWasNull, System.Action callback, DataType dataType) where T : Graphic {

			var isSprite = (dataType == DataType.Sprite);
			var isTexture = (dataType == DataType.Texture);
			var isMaterial = (dataType == DataType.Material);

			Graphic copy = null;
			Graphic source = null;
			var hasSourceTexture = false;

			//Debug.Log("FadeTO: " + to + " :: " + dataType, this.image as MonoBehaviour);

			if (isMaterial == true) {

				Graphic image = (Graphic)this.copyImage ?? (Graphic)this.copyRawImage;
				Graphic sourceImage = (Graphic)this.sourceImage ?? (Graphic)this.sourceRawImage;
				image.material = to as Material;

				hasSourceTexture = (sourceImage.material != null && sourceImage.material != sourceImage.defaultMaterial);

				this.CopyRect(sourceImage.rectTransform, image.rectTransform);
				this.CopyFlip(imageSource, this.copyFlippableImage ?? this.copyFlippableRawImage);
				this.CopyPreserveAspect(imageSource, this.copyPreserveAspectRawImage);

				//Debug.Log(sourceImage.color + " :: " + hasSourceTexture, sourceImage);

				copy = image;
				source = sourceImage;

			} else if (isSprite == true) {

				var image = this.copyImage;
				var sourceImage = this.sourceImage;
				image.preserveAspect = sourceImage.preserveAspect;
				image.sprite = to as Sprite;

				hasSourceTexture = (sourceImage.sprite != null);

				this.CopyRect(sourceImage.rectTransform, image.rectTransform);
				this.CopyFlip(imageSource, this.copyFlippableImage);

				copy = image;
				source = sourceImage;

				//copy.material = (source.material == sourceImage.defaultMaterial ? null : source.material);

			} else if (isTexture == true) {

				var image = this.copyRawImage;
				var sourceImage = this.sourceRawImage;
				image.texture = to as Texture;

				hasSourceTexture = (sourceImage.texture != null);

				this.CopyRect(sourceImage.rectTransform, image.rectTransform);
				this.CopyFlip(imageSource, this.copyFlippableRawImage);
				this.CopyPreserveAspect(imageSource, this.copyPreserveAspectRawImage);

				copy = image;
				source = sourceImage;

				//copy.material = (source.material == sourceImage.defaultMaterial ? null : source.material);

			}

			if (copy == null) return;

			TweenerGlobal.instance.removeTweens(source, immediately: true);
			TweenerGlobal.instance.removeTweens(copy, immediately: true);

			var sourceColor = this.targetColor;//source.color;
			copy.enabled = true;

			copy.rectTransform.SetAsFirstSibling();

			if (hasSourceTexture == true || fadeIfWasNull == true) {
				
				copy.color = this.startColor;//new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0f);

				if (hasSourceTexture == false) {

					source.color = this.startColor;//new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0f);

				}

				if (this.fadeOutSource == true) TweenerGlobal.instance.addTweenAlpha(source, duration, 0f).tag(source).ease(ME.Ease.GetByType(this.easeType));
				TweenerGlobal.instance.addTweenAlpha(copy, duration, 1f).tag(copy).ease(ME.Ease.GetByType(this.easeType)).onComplete(() => {

					if (source != null) source.color = sourceColor;
					this.Finalize(isSprite, isTexture, isMaterial, copy);

					if (callback != null) callback.Invoke();

				}).onCancel(() => {

                    if (callback != null) callback.Invoke();

                });

			} else {

				if (copy != null) copy.color = sourceColor;
				if (source != null) source.color = sourceColor;
				this.Finalize(isSprite, isTexture, isMaterial, copy);

				if (callback != null) callback.Invoke();

			}

		}

		private void Finalize(bool isSprite, bool isTexture, bool isMaterial, Graphic copy) {
			
			if (isMaterial == true) {

				if (this.sourceImage != null) {

					this.sourceImage.material = (copy.material == this.sourceImage.defaultMaterial ? null : copy.material);
					this.sourceImage.SetMaterialDirty();

				}

				if (this.sourceRawImage != null) {

					this.sourceRawImage.material = (copy.material == this.sourceRawImage.defaultMaterial ? null : copy.material);
					this.sourceRawImage.SetMaterialDirty();

				}

			} else if (isSprite == true) {

				if (this.sourceImage != null) {
				
					this.sourceImage.sprite = (copy as Image).sprite;

				}

			} else if (isTexture == true) {

				if (this.sourceRawImage != null) {

					var texture = (copy as RawImage).texture;
					if (texture.GetID() != this.sourceRawImage.texture.GetID()) {
						
						MovieSystem.Stop(this.image as IImageComponent, this.sourceRawImage.texture.GetID());

					}

					this.sourceRawImage.texture = texture;

				}

			}

			if (copy != null) {

				copy.enabled = false;

				if (isMaterial == true) {

					copy.material = null;

				} else if (isSprite == true) {

					(copy as Image).sprite = null;
					copy.material = null;

				} else if (isTexture == true) {

					(copy as RawImage).texture = null;
					copy.material = null;

				}

				copy.SetMaterialDirty();

			}

		}

		private void CopyFlip(IImageComponent imageSource, UIFlippable flippable) {

			if (flippable == null) {

				if (imageSource.IsHorizontalFlip() == true || imageSource.IsVerticalFlip() == true) {

					Debug.LogWarning("CopyFlip failed on component: " + (imageSource as MonoBehaviour).name, imageSource as MonoBehaviour);

				}
				return;

			}

			if (imageSource.IsHorizontalFlip() == true) {

				flippable.horizontal = true;

			}

			if (imageSource.IsVerticalFlip() == true) {

				flippable.vertical = true;

			}

		}

		private void CopyPreserveAspect(IImageComponent imageSource, UIPreserveAspect component) {

			if (component == null) {

				if (imageSource.IsPreserveAspect() == true) {

					Debug.LogWarning("CopyPreserveAspect failed on component: " + (imageSource as MonoBehaviour).name, imageSource as MonoBehaviour);

				}
				return;

			}

			if (imageSource.IsPreserveAspect() == true) {

				component.preserveAspect = true;

			}

		}

		private void CopyRect(RectTransform source, RectTransform copy) {

			var rect = copy;
			rect.sizeDelta = Vector2.zero;//source.rect.size;
			rect.pivot = source.pivot;
			rect.anchoredPosition = Vector2.zero;
			rect.localScale = Vector3.one;
			rect.localRotation = Quaternion.identity;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			/*rect.pivot = Vector2.one * 0.5f;
			rect.anchoredPosition = Vector2.zero;
			rect.localScale = Vector3.one;
			rect.localRotation = Quaternion.identity;
			rect.anchorMin = Vector2.one * 0.5f;
			rect.anchorMax = Vector2.one * 0.5f;
*/
		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {
			
			base.OnValidateEditor();

			this.ValidateCopy();

		}

		public void ValidateCopy() {

			if (this.image != null && this.IsValid() == true) {

				this.copyFlippableImage = this.CreateCopy(ref this.copyImage, ref this.sourceImage, (this.image as IImageComponent).GetImageSource());
				this.copyFlippableRawImage = this.CreateCopy(ref this.copyRawImage, ref this.sourceRawImage, (this.image as IImageComponent).GetRawImageSource());

			} else {

				this.ClearCopy(this.copyImage, this.sourceImage, () => { this.copyImage = null; this.sourceImage = null; });
				this.ClearCopy(this.copyRawImage, this.sourceRawImage, () => { this.copyRawImage = null; this.sourceRawImage = null; });

			}

			if (this.copyRawImage != null && this.copyPreserveAspectRawImage == null) {

				this.copyPreserveAspectRawImage = this.copyRawImage.GetComponent<UIPreserveAspect>();
				if (this.copyPreserveAspectRawImage == null) this.copyPreserveAspectRawImage = this.copyRawImage.gameObject.AddComponent<UIPreserveAspect>();
				this.copyPreserveAspectRawImage.rawImage = this.copyRawImage;
				this.CopyPreserveAspect(this.image as IImageComponent, this.copyPreserveAspectRawImage);

			} else {

				//if (this.copyPreserveAspectRawImage != null) ME.EditorUtilities.Destroy(this.copyPreserveAspectRawImage, () => { this.copyPreserveAspectRawImage = null; });

			}

			if (this.copyRawImage != null && this.copyFlippableRawImage == null) {

				this.copyFlippableRawImage = this.copyRawImage.GetComponent<UIFlippable>();
				if (this.copyFlippableRawImage == null) this.copyFlippableRawImage = this.copyRawImage.gameObject.AddComponent<UIFlippable>();
				this.CopyFlip(this.image as IImageComponent, this.copyFlippableRawImage);

			} else {

				//if (this.copyFlippableRawImage != null) ME.EditorUtilities.Destroy(this.copyFlippableRawImage, () => { this.copyFlippableRawImage = null; });

			}

			if (this.copyImage != null && this.copyFlippableImage == null) {

				this.copyFlippableImage = this.copyImage.GetComponent<UIFlippable>();
				if (this.copyFlippableImage == null) this.copyFlippableImage = this.copyImage.gameObject.AddComponent<UIFlippable>();
				this.CopyFlip(this.image as IImageComponent, this.copyFlippableImage);

			} else {

				//if (this.copyFlippableImage != null) ME.EditorUtilities.Destroy(this.copyFlippableImage, () => { this.copyFlippableImage = null; });

			}

			if (this.copyPreserveAspectRawImage != null) {

				var cps = this.copyRawImage.GetComponents<IMeshModifier>();
				if (System.Array.IndexOf(cps, this.copyPreserveAspectRawImage) > System.Array.IndexOf(cps, this.copyFlippableRawImage)) {

					UnityEditorInternal.ComponentUtility.MoveComponentUp(this.copyPreserveAspectRawImage);

				}

			}

		}

		private void ClearCopy(Graphic copy, Graphic source, System.Action onComplete) {
			
			UnityEditor.EditorApplication.delayCall += () => {
				
				if (copy != null) GameObject.DestroyImmediate(copy.gameObject, allowDestroyingAssets: true);

				onComplete.Invoke();

			};

		}

		private UIFlippable CreateCopy<T>(ref T copy, ref T source, T containerSource) where T : Graphic {

			if (copy == null && source == null && containerSource != null) {
				
				source = containerSource;

				var sourceType = source.GetType();
				var graphicCopy = new GameObject(string.Format("{0}_copy", source.name), typeof(RectTransform), sourceType, typeof(UIFlippable));
				graphicCopy.layer = source.gameObject.layer;
				graphicCopy.transform.SetParent(source.transform);
				graphicCopy.transform.SetTransformAs();

				copy = graphicCopy.GetComponent(sourceType) as T;
				copy.enabled = false;

				return graphicCopy.GetComponent<UIFlippable>();

			}

			if (copy != null) {

				return copy.GetComponent<UIFlippable>();

			}

			return null;

		}
		#endif

	}

}