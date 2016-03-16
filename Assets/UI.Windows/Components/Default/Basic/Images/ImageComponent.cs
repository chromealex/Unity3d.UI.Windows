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
		
		public override void OnDeinit() {
			
			base.OnDeinit();
			
			#region source macros UI.Windows.OnDeinit.ImageComponent
			{
				
				this.Stop();
				
			}
			#endregion
			
		}

		public override void OnShowBegin() {

			base.OnShowBegin();
			
			#region source macros UI.Windows.OnShowBegin.ImageComponent
			{

				if (this.playOnShow == true) {
					
					this.Play();
					
				}
				
			}
			#endregion

		}

		public override void OnHideEnd() {
			
			base.OnHideEnd();
			
			#region source macros UI.Windows.OnHideEnd.ImageComponent
			{
				
				this.Stop();
				
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
		[SerializeField][UnityEngine.Serialization.FormerlySerializedAs("playOnStart")]
		private bool playOnShow;
		[ReadOnly("rawImage", null)]
		[SerializeField]
		private bool loop;

		public IImageComponent SetPreserveAspectState(bool state) {

			this.preserveAspect = state;

			return this;

		}
		
		public IImageComponent SetLoop(bool state) {
			
			this.loop = state;
			
			return this;

		}

		public bool IsMovie() {
			
			var image = this.GetRawImageSource();
			if (image == null) return false;
			
			return image.mainTexture is MovieTexture;
			
		}

		public IImageComponent SetMovieTexture(Texture texture) {

			this.Stop();
			this.SetImage(texture);
			
			return this;

		}

		public bool GetPlayOnShow() {
			
			return this.playOnShow;
			
		}
		
		public IImageComponent SetPlayOnShow(bool state) {
			
			this.playOnShow = state;
			
			return this;

		}
		
		public bool IsPlaying() {

			return MovieSystem.IsPlaying(this);

		}

		public IImageComponent Play() {

			return this.Play(this.loop);

		}

		public IImageComponent Play(bool loop) {

			MovieSystem.Play(this, loop);

			return this;

		}

		public IImageComponent Stop() {

			MovieSystem.Stop(this);

			return this;

		}

		public IImageComponent Pause() {

			MovieSystem.Pause(this);

			return this;

		}

		public IImageComponent ResetImage() {
			
			if (this.image != null) {
				
				this.image.sprite = null;
				
			}
			
			if (this.rawImage != null) {

				this.Stop();
				this.rawImage.texture = null;
				
			}
			
			return this;

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
		public IImageComponent SetImage(UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey key, params object[] parameters) {

			this.lastImageLocalization = true;
			this.lastImageLocalizationKey = key;
			this.lastImageLocalizationParameters = parameters;

			this.SetImage(UnityEngine.UI.Windows.Plugins.Localization.LocalizationSystem.GetSprite(key, parameters));
			
			return this;

		}

		public IImageComponent SetImage(ImageComponent source) {
			
			if (source.GetImageSource() != null) this.SetImage(source.GetImageSource().sprite);
			if (source.GetRawImageSource() != null) this.SetImage(source.GetRawImageSource().texture);
			
			return this;

		}

		public IImageComponent SetImage(Sprite sprite, bool withPivotsAndSize = false) {

			this.SetImage(sprite, this.preserveAspect, withPivotsAndSize);
			
			return this;

		}

		public IImageComponent SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize = false) {
			
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
			
			return this;

		}

		public IImageComponent SetImage(Texture texture) {

			this.SetImage(texture, this.preserveAspect);
			
			return this;

		}

		public IImageComponent SetImage(Texture texture, bool preserveAspect) {
			
			if (this.rawImage != null) {

				this.rawImage.texture = texture;
				if (this.preserveAspect == true) ME.Utilities.PreserveAspect(this.rawImage);

			}
			
			return this;

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

		public IImageComponent SetAlpha(float value) {

			var color = this.GetColor();
			color.a = value;
			this.SetColor(color);
			
			return this;

		}

		public IImageComponent SetMaterial(Material material) {

			if (this.image != null) {

				this.image.material = material;
				this.image.SetMaterialDirty();

			} else if (this.rawImage != null) {

				this.rawImage.material = material;
				this.rawImage.SetMaterialDirty();

			}
			
			return this;

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