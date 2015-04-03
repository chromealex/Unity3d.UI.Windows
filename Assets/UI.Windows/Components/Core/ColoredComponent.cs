using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {

	public class ColoredComponent : WindowComponent {

		[Header("Colored Sprites")]
		public Image[] images;

		private Color color;
		
		public virtual void SetColor(Color color) {
			
			this.color = color;
			for (int i = 0; i < this.images.Length; ++i) this.images[i].color = color;
			
		}
		
		public virtual Color GetColor() {
			
			return this.color;
			
		}

	}

}