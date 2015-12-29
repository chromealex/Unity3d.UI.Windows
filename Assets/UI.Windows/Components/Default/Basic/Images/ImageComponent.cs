using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Components {

	public class ImageComponent : WindowComponent, IImageComponent {
		
		public override void Setup(IComponentParameters parameters) {
			
			base.Setup(parameters);
			
			var inputParameters = parameters as ImageComponentParameters;
			#region source macros UI.Windows.Initialization.ImageComponent
			{
				if (inputParameters != null) inputParameters.Setup(this as IImageComponent);
				
				if (this.playOnStart == true) {
					
					this.Play(this.loop);
					
				}

			}
			#endregion
			
		}

		public override void OnLocalizationChanged() {

			base.OnLocalizationChanged();

			#region source macros UI.Windows.OnLocalizationChanged.ImageComponent
			{
				
				if (this.lastImageLocalization == true) {

					this.SetImage(this.lastImageLocalizationKey, this.lastImageLocalizationParameters);

				} else {

					if (this.image is UnityEngine.UI.Windows.Plugins.Localization.UI.LocalizationImage) {

						(this.image as UnityEngine.UI.Windows.Plugins.Localization.UI.LocalizationImage).OnLocalizationChanged();

					}

				}

			}
			#endregion

		}

		public override void OnInit() {

			base.OnInit();

			#region source macros UI.Windows.OnInit.ImageComponent
			{

				if (this.imageLocalizationKey.IsNone() == false) {

					this.SetImage(this.imageLocalizationKey);

				}

			}
			#endregion

		}

		#region source macros UI.Windows.ImageComponent
		[Header("Image Component")]
		[SerializeField]
		private bool preserveAspect;

		[SerializeField]
		private Image image;
		
		[SerializeField]
		private RawImage rawImage;

		public UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey imageLocalizationKey;

		[ReadOnly("rawImage", null)]
		[SerializeField]
		private  bool playOnStart;
		[ReadOnly("rawImage", null)]
		[SerializeField]
		private  bool loop;

		public void SetPlayOnStart(bool state) {

			this.playOnStart = state;

		}

		public void SetLoop(bool state) {

			this.loop = state;

		}

		public void SetPreserveAspectState(bool state) {

			this.preserveAspect = state;

		}
		
		public void Play() {

			this.Play(this.loop);

		}

		public void Play(bool loop) {

			var image = this.GetRawImageSource();
			if (image == null) return;

			var movie = image.mainTexture as MovieTexture;
			if (movie != null) {
				
				movie.loop = loop;
				movie.Play();

			}

		}

		public void Stop() {
			
			var image = this.GetRawImageSource();
			if (image == null) return;
			
			var movie = image.mainTexture as MovieTexture;
			if (movie != null) movie.Stop();

		}

		public void Pause() {
			
			var image = this.GetRawImageSource();
			if (image == null) return;
			
			var movie = image.mainTexture as MovieTexture;
			if (movie != null) movie.Pause();

		}

		public void ResetImage() {
			
			if (this.image != null) {
				
				this.image.sprite = null;
				
			}
			
			if (this.rawImage != null) {
				
				this.rawImage.texture = null;
				
			}
			
		}
		
		public Image GetImageSource() {
			
			return this.image;
			
		}
		
		public RawImage GetRawImageSource() {
			
			return this.rawImage;
			
		}

		private bool lastImageLocalization = false;
		private Plugins.Localization.LocalizationKey lastImageLocalizationKey;
		private object[] lastImageLocalizationParameters;
		public void SetImage(UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey key, params object[] parameters) {

			this.lastImageLocalization = true;
			this.lastImageLocalizationKey = key;
			this.lastImageLocalizationParameters = parameters;

			this.SetImage(UnityEngine.UI.Windows.Plugins.Localization.LocalizationSystem.GetSprite(key, parameters));

		}

		public void SetImage(ImageComponent source) {
			
			if (source.GetImageSource() != null) this.SetImage(source.GetImageSource().sprite);
			if (source.GetRawImageSource() != null) this.SetImage(source.GetRawImageSource().texture);
			
		}

		public void SetImage(Sprite sprite, bool withPivotsAndSize = false) {

			this.SetImage(sprite, this.preserveAspect, withPivotsAndSize);

		}

		public void SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize = false) {
			
			if (this.image != null) {

				this.image.sprite = sprite;
				this.image.preserveAspect = preserveAspect;

				if (withPivotsAndSize == true && sprite != null) {

					var rect = (this.transform as RectTransform);

					rect.sizeDelta = sprite.rect.size;
					rect.pivot = sprite.GetPivot();
					rect.anchoredPosition = Vector2.zero;

				}

			}
			
		}

		public void SetImage(Texture texture) {

			this.SetImage(texture, this.preserveAspect);

		}

		public void SetImage(Texture texture, bool preserveAspect) {
			
			if (this.rawImage != null) {

				this.rawImage.texture = texture;
				if (this.preserveAspect == true) ME.Utilities.PreserveAspect(this.rawImage);

			}
			
		}
		
		public /*{overrideColor}*/ Color GetColor() {
			
			Color color = Color.white;
			if (this.image != null) {
				
				color = this.image.color;
				
			} else if (this.rawImage != null) {
				
				color = this.rawImage.color;
				
			}

			return color;

		}

		public /*{overrideColor}*/ void SetColor(Color color) {

			if (this.image != null) {
				
				this.image.color = color;
				
			} else if (this.rawImage != null) {
				
				this.rawImage.color = color;
				
			}

		}

		public void SetAlpha(float value) {

			var color = this.GetColor();
			color.a = value;
			this.SetColor(color);

		}

		public void SetMaterial(Material material) {

			if (this.image != null) {

				this.image.material = material;
				this.image.SetMaterialDirty();

			} else if (this.rawImage != null) {

				this.rawImage.material = material;
				this.rawImage.SetMaterialDirty();

			}

		}
		#endregion

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();
			
			if (this.gameObject.activeSelf == false) return;

			#region source macros UI.Windows.Editor.ImageComponent
			if (this.image == null) this.image = this.GetComponent<Image>();
			if (this.rawImage == null) this.rawImage = this.GetComponent<RawImage>();
			#endregion
			
		}
		#endif

	}

}