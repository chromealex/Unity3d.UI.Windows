using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Components {

	public class ImageComponent : WindowComponent, IImageComponent {

		[Header("Properties")]
		public bool preserveAspect;

		//[HideInInspector][SerializeField]
		public Image image;
		
		//[HideInInspector][SerializeField]
		public RawImage rawImage;

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
		
		public Color GetColor() {
			
			Color color = Color.white;
			if (this.image != null) {
				
				color = this.image.color;
				
			} else if (this.rawImage != null) {
				
				color = this.rawImage.color;
				
			}

			return color;

		}

		public void SetColor(Color color) {

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

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();
			
			if (this.gameObject.activeSelf == false) return;

			if (this.image == null) this.image = this.GetComponent<Image>();
			if (this.rawImage == null) this.rawImage = this.GetComponent<RawImage>();
			
		}
		#endif

	}

}