using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {
	
	[System.Serializable]
	public class SpriteResource : ResourceBase {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		public Sprite tempSprite;
		
		public override void Reset() {
			
			base.Reset();
			
			this.tempSprite = null;
			
		}
		
		public override void Validate(Object item) {

			base.Validate(item);

			if (item == null) {
				
				if (this.tempSprite != null) {
					
					item = this.tempSprite;
					
				}
				
				if (item == null) return;
				
			}
			
			this.tempSprite = item as Sprite;
			
			if (this.tempSprite != null) {

				var imp = UnityEditor.TextureImporter.GetAtPath(this.assetPath) as UnityEditor.TextureImporter;
				this.multiObjects = false;
				if (imp.spriteImportMode == UnityEditor.SpriteImportMode.Multiple) {

					var allObjects = Resources.LoadAll(this.resourcesPath);
					this.multiObjects = true;
					this.objectIndex = System.Array.IndexOf(allObjects, item);

				}

			}

		}
		#endif
		
	};

}