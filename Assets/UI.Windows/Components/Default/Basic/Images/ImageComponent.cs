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

					if ((this.imageResource.controlType & ResourceBase.ControlType.Init) != 0) {

						this.SetImage(this.imageLocalizationKey);

					}

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

				if (this.imageLocalizationKey.IsNone() == false) {

					if ((this.imageResource.controlType & ResourceBase.ControlType.Show) != 0) {

						this.SetImage(this.imageLocalizationKey);

					}

				} else {
					
					WindowSystemResources.LoadAuto(this, onDataLoaded: () => {

						if (this.playOnShow == true) {

							this.Play();

						}

					}, onComplete: null, onShowHide: true);

				}

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
				
				//this.Stop();

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
		private bool flipHorizontal;
		[SerializeField]
		private bool flipVertical;

		private bool flipHorizontalInternal;
		private bool flipVerticalInternal;

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

		public bool GetLoop() {
			
			return this.loop;

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

		public IImageComponent SetMovieTexture(AutoResourceItem resource, System.Action onDataLoaded, System.Action onComplete = null, System.Action onFailed = null) {

			this.flipVerticalInternal = MovieSystem.IsVerticalFlipped();

			this.Stop();
			this.SetImage(resource,
				onDataLoaded: () => {
					
					if (onDataLoaded != null) onDataLoaded.Invoke();

				},
				onComplete: () => {

					//Debug.Log("SetMovieTexture: " + this.name);
					MovieSystem.UnregisterOnUpdateTexture(this.ValidateTexture);
					MovieSystem.RegisterOnUpdateTexture(this.ValidateTexture);
					if (onComplete != null) onComplete.Invoke();

				},
				onFailed: onFailed);

			return this;
			
		}

		private void ValidateTexture(IImageComponent component, Texture texture) {

			//Debug.Log("ValidateTexture: " + (component as MonoBehaviour) + ", tex: " + texture, component as MonoBehaviour);
			if (this == component) {
				
				if (this.rawImage != null) {

					if (this.imageCrossFadeModule.IsValid() == true) {

						this.imageCrossFadeModule.ValidateTexture(texture);

					} else {

						this.rawImage.texture = texture;

					}

				}

			}

		}

		public bool GetPlayOnShow() {
			
			return this.playOnShow;
			
		}
		
		public IImageComponent SetPlayOnShow(bool state) {
			
			this.playOnShow = state;
			
			return this;

		}
		
		public virtual bool IsPlaying() {

			return MovieSystem.IsPlaying(this);

		}
		
		public virtual IImageComponent PlayAndPause() {

			MovieSystem.PlayAndPause(this, this.loop);

			return this;
			
		}

		public virtual IImageComponent Rewind(bool pause = true) {

			MovieSystem.Rewind(this, pause);

			return this;

		}

		public virtual IImageComponent Play() {

			return this.Play(this.loop);

		}

		public virtual IImageComponent Play(bool loop) {

			MovieSystem.Play(this, loop);

			return this;

		}

		public virtual IImageComponent Play(bool loop, System.Action onComplete) {

			MovieSystem.Play(this, loop, onComplete);

			return this;

		}

		public virtual IImageComponent Stop() {

			MovieSystem.UnregisterOnUpdateMaterial(this.ValidateMaterial);
			MovieSystem.UnregisterOnUpdateTexture(this.ValidateTexture);
			MovieSystem.Stop(this);

			return this;

		}

		public virtual IImageComponent Pause() {

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

			this.flipHorizontalInternal = false;
			this.flipVerticalInternal = false;

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

		public bool IsHorizontalFlip() {

			return this.flipHorizontal == true || this.flipHorizontalInternal == true;

		}

		public bool IsVerticalFlip() {

			return this.flipVertical == true || this.flipVerticalInternal == true;

		}

		public IImageComponent SetImage(ImageComponent source) {
			
			if (source.GetImageSource() != null) this.SetImage(source.GetImageSource().sprite);
			if (source.GetRawImageSource() != null) this.SetImage(source.GetRawImageSource().texture);
			
			return this;
			
		}

		public IImageComponent SetImage(AutoResourceItem resource, System.Action onDataLoaded = null, System.Action onComplete = null, System.Action onFailed = null) {

			var oldResource = this.imageResource;
			this.imageResource = resource;

			//Debug.Log("Loading resource: " + this.imageResource.GetId());
			WindowSystemResources.Load(this,
				onDataLoaded: onDataLoaded,
				onComplete: () => {

					//Debug.Log("Resource loaded: " + newResource.GetId() + " :: " + this.name, this);
					if (this.imageResource.GetId() != oldResource.GetId()) {

						//Debug.Log("Unloading: " + this.imageResource.GetId() + " != " + oldResource.GetId() + " :: " + this.name, this);
						WindowSystemResources.Unload(this, oldResource, resetController: false);

					}

					if (onComplete != null) onComplete.Invoke();

				},
				onFailed: () => {

					//Debug.Log("Resource loading failed: " + newResource.GetId() + " :: " + this.name, this);
					if (this.imageResource.GetId() != oldResource.GetId()) {

						//Debug.Log("Failed, Unloading: " + this.imageResource.GetId() + " != " + oldResource.GetId() + " :: " + this.name, this);
						WindowSystemResources.Unload(this, oldResource, resetController: false);

					}

					if (onFailed != null) onFailed.Invoke();

				}
			);

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

		public IImageComponent SetImage(Sprite sprite) {

			return this.SetImage(sprite, this.preserveAspect, false, null, false);

		}

		public IImageComponent SetImage(Sprite sprite, bool immediately) {

			return this.SetImage(sprite, this.preserveAspect, false, null, immediately);

		}

		public IImageComponent SetImage(Sprite sprite, System.Action onComplete) {

			return this.SetImage(sprite, this.preserveAspect, false, onComplete, false);

		}

		public IImageComponent SetImage(Sprite sprite, System.Action onComplete, bool immediately) {

			return this.SetImage(sprite, this.preserveAspect, false, onComplete, immediately);

		}

		public IImageComponent SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize, System.Action onComplete) {

			return this.SetImage(sprite, preserveAspect, withPivotsAndSize, onComplete, false);

		}

		public IImageComponent SetImage(Sprite sprite, bool preserveAspect, bool withPivotsAndSize, System.Action onComplete = null, bool immediately = false) {
			
			if (this.image != null) {
				
				this.image.preserveAspect = preserveAspect;

				if (withPivotsAndSize == true && sprite != null) {

					var rect = (this.transform as RectTransform);
					rect.sizeDelta = sprite.rect.size;
					rect.pivot = sprite.GetPivot();
					rect.anchoredPosition = Vector2.zero;

				}

				if (immediately == false && this.imageCrossFadeModule.IsValid() == true) {

					this.imageCrossFadeModule.FadeTo(this, sprite, onComplete);

				} else {

					this.image.sprite = sprite;

					if (onComplete != null) onComplete.Invoke();

				}

			} else {

				if (onComplete != null) onComplete.Invoke();

			}

			return this;

		}

		public IImageComponent SetImage(Texture texture) {

			return this.SetImage(texture, this.preserveAspect, null, false);

		}

		public IImageComponent SetImage(Texture texture, bool immediately) {

			return this.SetImage(texture, this.preserveAspect, null, immediately);

		}

		public IImageComponent SetImage(Texture texture, System.Action onComplete) {

			return this.SetImage(texture, this.preserveAspect, onComplete, false);

		}

		public IImageComponent SetImage(Texture texture, System.Action onComplete, bool immediately) {

			return this.SetImage(texture, this.preserveAspect, onComplete, immediately);

		}

		public IImageComponent SetImage(Texture texture, bool preserveAspect, System.Action onComplete) {

			return this.SetImage(texture, preserveAspect, onComplete, false);

		}

		public IImageComponent SetImage(Texture texture, bool preserveAspect, System.Action onComplete, bool immediately) {

			//MovieSystem.UnregisterOnUpdateTexture(this.ValidateTexture);
			//MovieSystem.Stop(this, this.rawImage.texture.GetInstanceID());

			if (this.rawImage != null) {
				
				if (this.preserveAspect == true) ME.Utilities.PreserveAspect(this.rawImage);

				if (immediately == false && this.imageCrossFadeModule.IsValid() == true) {

					this.imageCrossFadeModule.FadeTo(this, texture, onComplete);

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

		public IImageComponent SetMaterial(Material material, bool setMainTexture = false, System.Action onComplete = null) {

			MovieSystem.UnregisterOnUpdateMaterial(this.ValidateMaterial);

			if (material == null) {

				if (this.image != null) {

					this.image.material = null;
					this.image.SetMaterialDirty();

				} else if (this.rawImage != null) {

					this.rawImage.material = null;
					this.rawImage.SetMaterialDirty();

				}


				if (onComplete != null) onComplete.Invoke();
				return this;

			}

			MovieSystem.RegisterOnUpdateMaterial(this.ValidateMaterial);

			if (this.image != null) {

				if (this.image.enabled == false) this.image.enabled = true;

				var tex = material.mainTexture;
				if (this.imageCrossFadeModule.IsValid() == true) {

					this.imageCrossFadeModule.FadeTo<Image>(this, material, () => {

						if (setMainTexture == true) {

							var sprite = Sprite.Create(tex as Texture2D, new Rect(0f, 0f, tex.width, tex.height), Vector2.one * 0.5f);
							this.image.sprite = sprite;

						}

						this.image.SetMaterialDirty();

						if (onComplete != null) onComplete.Invoke();

					}, ImageCrossFadeModule.DataType.Material);

				} else {

					if (setMainTexture == true) {

						var sprite = Sprite.Create(tex as Texture2D, new Rect(0f, 0f, tex.width, tex.height), Vector2.one * 0.5f);
						this.image.sprite = sprite;

					}

					this.image.material = material;
					this.image.SetMaterialDirty();

					if (onComplete != null) onComplete.Invoke();

				}

			} else if (this.rawImage != null) {

				if (this.rawImage.enabled == false) this.rawImage.enabled = true;

				var tex = material.mainTexture;
				if (this.imageCrossFadeModule.IsValid() == true) {

					this.imageCrossFadeModule.FadeTo<RawImage>(this, material, () => {

						if (setMainTexture == true) {
							
							this.rawImage.texture = tex;

						}
						this.rawImage.SetMaterialDirty();

						if (onComplete != null) onComplete.Invoke();

					}, ImageCrossFadeModule.DataType.Material);

				} else {

					if (setMainTexture == true) {
					
						this.rawImage.texture = tex;

					}

					this.rawImage.material = material;
					this.rawImage.SetMaterialDirty();

					if (onComplete != null) onComplete.Invoke();

				}

			}

			return this;

		}

		public void ValidateMaterial(Material material) {
			
			if (this.rawImage != null) {

				this.rawImage.texture = this.rawImage.material.mainTexture;
				if (this.imageCrossFadeModule.IsValid() == true) {

					this.imageCrossFadeModule.ValidateMaterial(material);

				}
				//Debug.Log("MATERIAL DIRTY: " + this.rawImage.texture, this);

			}

		}

		public void ModifyMesh(Mesh mesh) {}

		private System.Collections.Generic.List<UIVertex> modifyVertsTemp = new System.Collections.Generic.List<UIVertex>();
		public void ModifyMesh(VertexHelper helper) {

			if (this.flipHorizontal == false &&
			    this.flipVertical == false &&
			    this.flipHorizontalInternal == false &&
			    this.flipVerticalInternal == false) {

				return;

			}

			this.modifyVertsTemp.Clear();
			helper.GetUIVertexStream(this.modifyVertsTemp);

			this.ModifyVertices(this.modifyVertsTemp);

			helper.AddUIVertexTriangleStream(this.modifyVertsTemp);

		}

		public void ModifyVertices(System.Collections.Generic.List<UIVertex> verts) {

			var rt = this.GetRectTransform();
			var rectCenter = rt.rect.center;
			for (var i = 0; i < verts.Count; ++i) {
				
				var v = verts[i];
				v.position = new Vector3(
					((this.flipHorizontal == true || this.flipHorizontalInternal == true) ? (v.position.x + (rectCenter.x - v.position.x) * 2f) : v.position.x),
					((this.flipVertical == true || this.flipVerticalInternal == true) ? (v.position.y + (rectCenter.y - v.position.y) * 2f) : v.position.y),
					v.position.z
				);
				verts[i] = v;

			}

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