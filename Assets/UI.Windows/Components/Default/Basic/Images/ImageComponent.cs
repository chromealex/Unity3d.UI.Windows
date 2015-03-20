using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI;

namespace UnityEngine.UI.Windows.Components {

	public class ImageComponent : WindowComponent {
		
		[HideInInspector][SerializeField]
		public Image image;
		
		[HideInInspector][SerializeField]
		public RawImage rawImage;
		
		public void SetImage(Sprite sprite) {
			
			if (this.image != null) this.image.sprite = sprite;
			
		}
		
		public void SetImage(Texture2D texture) {
			
			if (this.rawImage != null) this.rawImage.texture = texture;
			
		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();
			
			if (this.gameObject.activeSelf == false) return;

			this.image = this.GetComponent<Image>();
			this.rawImage = this.GetComponent<RawImage>();
			
		}
		#endif

	}

}