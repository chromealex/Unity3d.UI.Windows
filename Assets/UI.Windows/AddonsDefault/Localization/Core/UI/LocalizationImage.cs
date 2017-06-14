using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Plugins.Localization.UI {

	public class LocalizationImage : UnityEngine.UI.Image {

		public LocalizationKey localizationKey;

		public override void GraphicUpdateComplete() {

			base.GraphicUpdateComplete();

			this.OnLocalizationChanged();

		}

		new public Sprite sprite {
			
			get {
				
				return base.sprite;

			}

			set {
				
				base.sprite = LocalizationSystem.GetSprite(this.localizationKey);

			}

		}

		public void OnLocalizationChanged() {

			this.sprite = base.sprite;

		}

	}

}