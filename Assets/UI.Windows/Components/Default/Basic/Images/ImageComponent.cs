using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Components.Modules;

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

				this.tempImagePlayOnShow = false;

				if (this.imageLocalizationKey.IsNone() == false) {

					this.SetImage(this.imageLocalizationKey);

				} else {
					
					WindowSystemResources.LoadAuto(this, onDataLoaded: () => {

						this.tempImagePlayOnShow = true;

						if (this.IsVisible() == true) {

							if (this.playOnShow == true) {

								this.Play();

							}

						}

					}, onComplete: null, onShowHide: false);

				}

				this.imageCrossFadeModule.Init(this);

			}
			#endregion

		}
		
		public override void OnDeinit() {
			
			base.OnDeinit();
			
			#region source macros UI.Windows.OnDeinit.ImageComponent
			{

				this.Stop();
				
				WindowSystemResources.UnloadAuto(this, onShowHide: false);

			}
			#endregion
			
		}

		public override void OnShowBegin() {

			base.OnShowBegin();

			#region source macros UI.Windows.OnShowBegin.ImageComponent
			{
				
				WindowSystemResources.LoadAuto(this, onDataLoaded: () => {

					if (this.playOnShow == true) {

						this.Play();

					}

				}, onComplete: null, onShowHide: true);

				if (this.tempImagePlayOnShow == true) {

					if (this.playOnShow == true) {

						this.Play();

					}

				}

			}
			#endregion

		}

		public override void OnHideEnd() {
			
			base.OnHideEnd();
			
			#region source macros UI.Windows.OnHideEnd.ImageComponent
			{
				
				this.Stop();

				WindowSystemResources.UnloadAuto(this, onShowHide: true);

			}
			#endregion
			
		}

		#region source macros UI.Windows.ImageComponent
		[Header("Image Component")]
		[SerializeField]
		private Image image;
		[BeginGroup("image")][SerializeField]
		private RawImage rawImage;

		[SerializeField]
		private bool preserveAspect;

		public UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey imageLocalizationKey;

		[ReadOnly("rawImage", null)]
		[SerializeField][UnityEngine.Serialization.FormerlySerializedAs("playOnStart")]
		private bool playOnShow;
		[ReadOnly("rawImage", null)]
		[SerializeField]
		private bool loop;

		[SerializeField]
		private AutoResourceItem imageResource = new AutoResourceItem();

		[EndGroup][SerializeField]
		private ImageCrossFadeModule imageCrossFadeModule = new ImageCrossFadeModule();

		private bool tempImagePlayOnShow = false;

		public AutoResourceItem GetResource() {

			return this.imageResource;

		}

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
			
			return MovieSystem.IsMovie(image.mainTexture);
			
		}

		public IImageComponent Unload(ResourceBase resource) {

			WindowSystemResources.Unload(this, resource);

			return this;

		}

		public IImageComponent SetMovieTexture(AutoResourceItem resource, System.Action onDataLoaded, System.Action onComplete = null) {
			
			this.Stop();
			this.SetImage(resource, onDataLoaded, onComplete);

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
		
		public IImageComponent PlayAndPause() {

			MovieSystem.PlayAndPause(this, this.loop);
			return this;
			
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

		public Texture GetTexture() {

			if (this.image != null) {

				return (this.image.sprite != null ? this.image.sprite.texture : null);

			}

			if (this.rawImage != null) {

				return this.rawImage.texture;

			}

			return null;

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

		public Graphic GetGraphicSource() {

			return this.image;

		}

		public Image GetImageSource() {
			
			return this.image;
			
		}
		
		public RawImage GetRawImageSource() {
			
			return this.rawImage;
			
		}
		
		public IImageComponent SetImage(ImageComponent source) {
			
			if (source.GetImageSource() != null) this.SetImage(source.GetImageSource().sprite);
			if (source.GetRawImageSource() != null) this.SetImage(source.GetRawImageSource().texture);
			
			return this;
			
		}

		public IImageComponent SetImage(AutoResourceItem resource, System.Action onDataLoaded = null, System.Action onComplete = null) {

			var oldResource = this.imageResource;
			var newResource = resource;
			this.imageResource = resource;

			//Debug.Log("Loading resource: " + newResource.GetId());
			WindowSystemResources.Load(this, onDataLoaded: onDataLoaded, onComplete: () => {

				//Debug.Log("Resource loaded: " + newResource.GetId());
				if (newResource.GetId() != oldResource.GetId()) {

					//Debug.Log("Unloading: " + newResource.GetId() + " != " + oldResource.GetId());
					WindowSystemResources.Unload(this, oldResource, resetController: false);

				}

				if (onComplete != null) onComplete.Invoke();

			});

			return this;
			
		}

		private bool lastImageLocalization = false;
		private Plugins.Localization.LocalizationKey lastImageLocalizationKey;
		private object[] lastImageLocalizationParameters;
		public IImageComponent SetImage(UnityEngine.UI.Windows.Plugins.Localization.LocalizationKey key, params object[] parameters) {

			this.lastImageLocalization = true;
			this.lastImageLocalizationKey = key;
			this.lastImageLocalizationParameters = parameters;

			//this.SetImage(UnityEngine.UI.Windows.Plugins.Localization.LocalizationSystem.GetSprite(key, parameters));
			WindowSystemResources.Unload(this, this.GetResource());
			WindowSystemResources.Load(this, onDataLoaded: null, onComplete: null, customResourcePath: UnityEngine.UI.Windows.Plugins.Localization.LocalizationSystem.GetSpritePath(key, parameters));

			return this;

		}

		public IImageComponent SetImage(Sprite sprite, System.Action onComplete = null) {

			this.SetImage(sprite, this.preserveAspect, withPivotsAndSize: false, onComplete: onComplete);

			return this;

		}

		public IImageComponent SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize, System.Action onComplete = null) {
			
			if (this.image != null) {
				
				this.image.preserveAspect = preserveAspect;

				if (withPivotsAndSize == true && sprite != null) {

					var rect = (this.transform as RectTransform);
					rect.sizeDelta = sprite.rect.size;
					rect.pivot = sprite.GetPivot();
					rect.anchoredPosition = Vector2.zero;

				}

				if (this.imageCrossFadeModule.IsValid() == true) {

					this.imageCrossFadeModule.FadeTo(sprite, onComplete);

				} else {

					this.image.sprite = sprite;

					if (onComplete != null) onComplete.Invoke();

				}

			} else {

				if (onComplete != null) onComplete.Invoke();

			}

			return this;

		}

		public IImageComponent SetImage(Texture texture, System.Action onComplete = null) {

			this.SetImage(texture, this.preserveAspect, onComplete);
			
			return this;

		}

		public IImageComponent SetImage(Texture texture, bool preserveAspect, System.Action onComplete = null) {
			
			if (this.rawImage != null) {
				
				if (this.preserveAspect == true) ME.Utilities.PreserveAspect(this.rawImage);

				if (this.imageCrossFadeModule.IsValid() == true) {

					this.imageCrossFadeModule.FadeTo(texture, onComplete);

				} else {

					this.rawImage.texture = texture;

					if (onComplete != null) onComplete.Invoke();

				}

			} else {

				if (onComplete != null) onComplete.Invoke();

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
				
			}

			if (this.rawImage != null) {
				
				this.rawImage.color = color;
				
			}

		}

		public IAlphaComponent SetAlpha(float value) {

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
			{
				
				//if (this.image == null) this.image = this.GetComponent<Image>();
				//if (this.rawImage == null) this.rawImage = this.GetComponent<RawImage>();

				WindowSystemResources.Validate(this);

				this.imageCrossFadeModule.Init(this);
				this.imageCrossFadeModule.OnValidateEditor();

			}
			#endregion

		}
		#endif

	}

}