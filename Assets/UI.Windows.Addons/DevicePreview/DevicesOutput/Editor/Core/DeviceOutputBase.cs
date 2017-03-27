using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview.Output {

	public class DeviceOutputBase : IDeviceOutput {
		
		private Rect groupRect;
		private Rect screenRect;
		protected Rect rect;
		protected Rect deviceRect;

		protected ScreenOrientation orientation;

		protected Sprite sprite;
		
		public void SetRect(Rect groupRect, Rect screenRect, Rect rect, Rect landscapeRect, ScreenOrientation orientation) {

			this.groupRect = groupRect;
			this.screenRect = screenRect;
			this.rect = rect;
			this.orientation = orientation;

			if (this.sprite == null) {
				
				var filepath = "UI.Windows/DevicePreview/Images/" + this.GetMainImage();
				this.sprite = UnityEditor.UI.Windows.CoreUtilities.Load<Sprite>(filepath);

			}

			if (this.sprite != null) {

				var imageSize = this.sprite.rect.size;
				var offsetRect = this.sprite.border;
				var offset = this.GetOffset();

				var cx = (imageSize.x - offsetRect.x - offsetRect.z) * 0.5f + offsetRect.x + offset.x;
				var cy = (imageSize.y - offsetRect.y - offsetRect.w) * 0.5f + offsetRect.y + offset.y;
				
				offset = new Vector2((cx - imageSize.x * 0.5f) * 0.5f, (imageSize.y * 0.5f - cy) * 0.5f);
				this.rect.center -= offset;

				if (orientation == ScreenOrientation.Portrait) {

					var imageContentRect = new Rect(offsetRect.x, offsetRect.y, imageSize.x - offsetRect.z - offsetRect.x, imageSize.y - offsetRect.w - offsetRect.y);
					var k = this.rect.height / imageContentRect.height;
					this.deviceRect = new Rect(this.groupRect.x + this.rect.center.x - imageSize.x * 0.5f * k, this.groupRect.y + this.rect.center.y - imageSize.y * 0.5f * k, imageSize.x * k, imageSize.y * k);

				} else {

					var imageContentRect = new Rect(offsetRect.y, offsetRect.x, imageSize.y - offsetRect.w - offsetRect.y, imageSize.x - offsetRect.z - offsetRect.x);
					var k = this.rect.width / imageContentRect.width;
					this.deviceRect = new Rect(this.groupRect.y + this.rect.center.x - imageSize.x * 0.5f * k, -this.groupRect.x + this.rect.center.y - imageSize.y * 0.5f * k, imageSize.x * k, imageSize.y * k);

				}

			}

		}

		public virtual Vector2 GetOffset() {

			return Vector2.zero;

		}

		public virtual float GetLandscapeOrientationAngle() {
			
			return 90f;
			
		}
		
		public virtual float GetPortraitOrientationAngle() {
			
			return 0f;
			
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

		public virtual void OnPreGUI() {

			if (this.sprite != null) GUI.DrawTexture(this.deviceRect, this.sprite.texture, ScaleMode.StretchToFill);

		}

		public virtual void OnPostGUI() {}

		private Matrix4x4 guiMatrix;
		private void ApplyOrientation() {
			
			GUI.EndGroup();

			this.guiMatrix = GUI.matrix;

			var pivot = this.screenRect.center;
			GUIUtility.RotateAroundPivot((this.orientation == ScreenOrientation.Landscape ? this.GetLandscapeOrientationAngle() : this.GetPortraitOrientationAngle()), pivot);

			//GUILayout.BeginArea(this.screenRect);

		}

		private void RestoreOrientation() {

			//GUILayout.EndArea();
			GUI.matrix = this.guiMatrix;
			
			GUI.BeginGroup(this.groupRect);//new Rect(this.screenRect.x, this.screenRect.y, Screen.width, Screen.height));

		}

	}

}
