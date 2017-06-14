using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Modules.ImageEffects;

namespace UnityEngine.UI.Windows.Modules {

	public class BackgroundBlur : BackgroundBase {

		[Header("Optional")]
		public Camera blurCamera;
		public BlurOptimized blur;

		public override void OnShowBegin() {

			base.OnShowBegin();

			if (this.blurCamera != null) {

				var workCamera = this.GetWindow().workCamera;
				this.blurCamera.depth = workCamera.depth - WindowSystem.GetDepthStep() * 0.1f;
				this.blur.enabled = true;

			}

		}

		public override void OnHideEnd() {

			base.OnHideEnd();

			if (this.blurCamera != null) {
				
				this.blur.enabled = true;

			}

		}

	}

}