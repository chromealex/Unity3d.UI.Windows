using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview.Output {

	public class DeviceOutputBase : IDeviceOutput {

		private Rect screenRect;
		protected Rect rect;
		protected Rect deviceRect;

		protected ScreenOrientation orientation;

		protected Sprite sprite;
		
		public void SetRect(Rect screenRect, Rect rect, Rect landscapeRect, ScreenOrientation orientation) {

			this.screenRect = screenRect;
			this.rect = rect;
			this.orientation = orientation;
			
			this.sprite = Resources.Load<Sprite>("UI.Windows/DevicePreview/Images/" + this.GetMainImage());

			if (this.sprite != null) {
				/*
				if (orientation == ScreenOrientation.Landscape) {
					
					var textureWidth = this.sprite.texture.width;
					var textureHeight = this.sprite.texture.height;
					var border = this.sprite.border;
					var offsetRect = new Vector4(border.x / textureWidth * this.rect.width, border.y / textureHeight * this.rect.height, border.z / textureWidth * this.rect.width, border.w / textureHeight * this.rect.height);

					this.landscapeRect = new Rect(this.rect.x - offsetRect.x + offsetRect.z * 0.5f,
										          this.rect.y - offsetRect.y + offsetRect.w * 0.5f,
										          this.rect.width + offsetRect.z * 0.5f + offsetRect.z,
										          this.rect.height + offsetRect.w * 0.5f + offsetRect.w);

				} else {

					this.landscapeRect = landscapeRect;

				}*/

				if (orientation == ScreenOrientation.Landscape) {

					var offsetRect = this.sprite.border;
					this.deviceRect = new Rect(this.rect.x - offsetRect.x + offsetRect.z * 0.5f,
					                              this.rect.y - offsetRect.y + offsetRect.w * 0.5f,
					                              this.rect.width + offsetRect.z * 0.5f + offsetRect.z,
					                              this.rect.height + offsetRect.w * 0.5f + offsetRect.w);

				} else {

					this.rect = landscapeRect;
					var offsetRect = this.sprite.border;
					this.deviceRect = new Rect(this.rect.x - offsetRect.x + offsetRect.z * 0.5f,
					                           this.rect.y - offsetRect.y + offsetRect.w * 0.5f,
					                           this.rect.width + offsetRect.z * 0.5f + offsetRect.z,
					                           this.rect.height + offsetRect.w * 0.5f + offsetRect.w
					                           );

				}

			}

		}
		
		public virtual float GetLandscapeOrientationAngle() {
			
			return 0f;
			
		}
		
		public virtual float GetPortraitOrientationAngle() {
			
			return 270f;
			
		}

		public virtual string GetMainImage() {

			return string.Empty;

		}
		
		public virtual void DoPreGUI() {

			this.ApplyOrientation();
			this.OnPreGUI();
			this.RestoreOrientation();

		}
		
		public virtual void DoPostGUI() {
			
			this.ApplyOrientation();
			this.OnPostGUI();
			this.RestoreOrientation();

		}

		public virtual void OnPreGUI() {}
		public virtual void OnPostGUI() {}

		private Matrix4x4 guiMatrix;
		private void ApplyOrientation() {

			this.guiMatrix = GUI.matrix;

			var pivot = this.screenRect.center;
			GUIUtility.RotateAroundPivot((this.orientation == ScreenOrientation.Landscape ? this.GetLandscapeOrientationAngle() : this.GetPortraitOrientationAngle()), pivot);
			GUILayout.BeginArea(this.screenRect);

		}

		private void RestoreOrientation() {
			
			GUILayout.EndArea();
			GUI.matrix = this.guiMatrix;

		}

	}

}
