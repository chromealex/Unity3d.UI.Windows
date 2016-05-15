using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows.Components.Modules {
	
	[System.Serializable]
	public class ImageCrossFadeModule : ComponentModuleBase {

		public enum DataType : byte {

			Sprite,
			Texture,

		};

		[HideInInspector][SerializeField] private IImageComponent image;
		[HideInInspector][SerializeField] private Image sourceImage;
		[HideInInspector][SerializeField] private Image copyImage;
		[HideInInspector][SerializeField] private RawImage sourceRawImage;
		[HideInInspector][SerializeField] private RawImage copyRawImage;

		[ReadOnly("enabled", state: false)]
		public float duration = 0.5f;
		[ReadOnly("enabled", state: false)]
		public bool fadeIfWasNull = false;

		public void Init(IImageComponent image) {

			this.image = image;

		}

		public void FadeTo(Sprite to, System.Action callback = null) {

			this.FadeTo<Image>(to, this.duration, this.fadeIfWasNull, callback, DataType.Sprite);

		}

		public void FadeTo(Texture to, System.Action callback = null) {

			this.FadeTo<RawImage>(to, this.duration, this.fadeIfWasNull, callback, DataType.Texture);

		}

		public void FadeTo(Sprite to, float duration, System.Action callback = null) {

			this.FadeTo<Image>(to, duration, this.fadeIfWasNull, callback, DataType.Sprite);

		}

		public void FadeTo(Texture to, float duration, System.Action callback = null) {

			this.FadeTo<RawImage>(to, duration, this.fadeIfWasNull, callback, DataType.Texture);

		}

		public void FadeTo<T>(Object to, float duration, bool fadeIfWasNull, System.Action callback, DataType dataType) where T : Graphic {

			var isSprite = (dataType == DataType.Sprite);
			var isTexture = (dataType == DataType.Texture);

			Graphic copy = null;
			Graphic source = null;
			var hasSourceTexture = false;

			//Debug.Log("FadeTO: " + to + " :: " + dataType, this.image as MonoBehaviour);

			if (isSprite == true) {

				var image = this.copyImage;
				var sourceImage = this.sourceImage;
				image.preserveAspect = sourceImage.preserveAspect;
				image.sprite = to as Sprite;

				hasSourceTexture = (sourceImage.sprite != null);

				this.CopyRect(sourceImage.rectTransform, image.rectTransform);

				copy = image;
				source = sourceImage;

			} else if (isTexture == true) {

				var image = this.copyRawImage;
				var sourceImage = this.sourceRawImage;
				image.texture = to as Texture;

				hasSourceTexture = (sourceImage.texture != null);

				this.CopyRect(sourceImage.rectTransform, image.rectTransform);

				copy = image;
				source = sourceImage;

			}

			if (copy == null) return;

			var sourceColor = source.color;
			copy.material = source.material;
			copy.enabled = true;

			copy.rectTransform.SetAsFirstSibling();

			if (hasSourceTexture == true || fadeIfWasNull == true) {

				copy.color = new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0f);

				if (hasSourceTexture == false) {

					source.color = new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0f);

				}

				TweenerGlobal.instance.removeTweens(copy);
				TweenerGlobal.instance.addTweenAlpha(copy, duration, 0f, 1f).tag(copy).onComplete(() => {

					this.Finalize(isSprite, isTexture, copy);
					source.color = sourceColor;

					if (callback != null) callback.Invoke();

				});

			} else {

				copy.color = sourceColor;
				this.Finalize(isSprite, isTexture, copy);

				if (callback != null) callback.Invoke();

			}

		}

		private void Finalize(bool isSprite, bool isTexture, Graphic copy) {

			if (isSprite == true) {

				this.sourceImage.sprite = (copy as Image).sprite;

			} else if (isTexture == true) {

				this.sourceRawImage.texture = (copy as RawImage).texture;

			}

			copy.enabled = false;

			if (isSprite == true) {

				(copy as Image).sprite = null;
				copy.material = null;

			} else if (isTexture == true) {

				(copy as RawImage).texture = null;
				copy.material = null;

			}

		}

		private void CopyRect(RectTransform source, RectTransform copy) {

			var rect = copy;
			rect.sizeDelta = source.rect.size;
			rect.pivot = Vector2.one * 0.5f;
			rect.anchoredPosition = Vector2.zero;
			rect.localScale = Vector3.one;
			rect.localRotation = Quaternion.identity;
			rect.anchorMin = Vector2.one * 0.5f;
			rect.anchorMax = Vector2.one * 0.5f;

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {
			
			base.OnValidateEditor();

			this.ValidateCopy();

		}

		public void ValidateCopy() {

			if (this.image != null && this.IsValid() == true) {

				this.CreateCopy(ref this.copyImage, ref this.sourceImage, this.image.GetImageSource());
				this.CreateCopy(ref this.copyRawImage, ref this.sourceRawImage, this.image.GetRawImageSource());

			} else {

				this.ClearCopy(this.copyImage, this.sourceImage, () => { this.copyImage = null; this.sourceImage = null; });
				this.ClearCopy(this.copyRawImage, this.sourceRawImage, () => { this.copyRawImage = null; this.sourceRawImage = null; });

			}

		}

		private void ClearCopy(Graphic copy, Graphic source, System.Action onComplete) {
			
			UnityEditor.EditorApplication.delayCall += () => {
			
				if (copy != null) GameObject.DestroyImmediate(copy.gameObject);

				onComplete.Invoke();

			};

		}

		private void CreateCopy<T>(ref T copy, ref T source, T containerSource) where T : Graphic {

			if (copy == null && source == null && containerSource != null) {
				
				source = containerSource;

				var sourceType = source.GetType();
				var graphicCopy = new GameObject(string.Format("{0}_copy", source.name), typeof(RectTransform), sourceType);
				graphicCopy.layer = source.gameObject.layer;
				graphicCopy.transform.SetParent(source.transform);
				graphicCopy.transform.SetTransformAs();

				copy = graphicCopy.GetComponent(sourceType) as T;
				copy.enabled = false;

			}

		}
		#endif

	}

}