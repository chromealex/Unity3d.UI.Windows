using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Plugins.Localization.UI {

	public class LocalizationText : UnityEngine.UI.Text {

		public LocalizationKey localizationKey;

		public override void GraphicUpdateComplete() {

			base.GraphicUpdateComplete();

			this.OnLocalizationChanged();

		}

		protected override void OnEnable() {

			base.OnEnable();

			this.OnLocalizationChanged();

		}

		/*public override string text {
			
			get {
				
				return base.text;

			}

			set {
				
				base.text = LocalizationSystem.Get(this.localizationKey);

			}

		}*/

		public void OnLocalizationChanged() {

			if (this.localizationKey.IsNone() == false) {

				//this.text = base.text;
				this.text = LocalizationSystem.Get(this.localizationKey);

			}

		}

	}

}