using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {

	public class ColoredComponent : HoveredComponent {

		[Header("Colored Sprites")]
		public Image[] images;

		private Color color = new Color(1f, 1f, 1f, 1f);
		
		public virtual void SetColors(Color color) {
			
			this.color = color;
			for (int i = 0; i < this.images.Length; ++i) {

				var image = this.images[i];
				if (image != null && image.IsDestroyed() == false) image.color = color;

			}
			
		}

		public virtual void SetColor(Color color) {
			
			this.SetColors(color);
			
		}
		
		public virtual Color GetColor() {
			
			return this.color;
			
		}

	}

}