using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {
	
	[System.Serializable]
	public class TextureResource : ResourceBase {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		public Texture tempTexture;
		
		public override void Reset() {
			
			base.Reset();
			
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