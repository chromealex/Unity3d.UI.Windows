using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Resources;

namespace UnityEngine.UI.Windows {
	
	[System.Serializable]
	public class TextureResource : ResourceBase {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		[BundleIgnore]
		public Texture tempTexture;
		
		public override void ResetToDefault() {
			
			base.ResetToDefault();
			
			this.tempTexture = null;
			
		}
		
		public override void Validate(Object item) {
			
			if (item == null) {
				
				if (this.tempTexture != null) {
					
					item = this.tempTexture;
					
				}
				
				if (item == null) return;
				
			}
			
			this.tempTexture = item as Texture;
			
			base.Validate(item);
			
		}
		#endif
		
	};

}