using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {
	
	[System.Serializable]
	public class AutoResourceItem : ResourceBase {

		public AutoResourceItem() {}

		public AutoResourceItem(ResourceBase source) : base(source) {
		}

		#if UNITY_EDITOR
		//[HideInInspector]
		public Object tempObject;
		#endif
		
		#if UNITY_EDITOR
		public override void Reset() {
			
			base.Reset();

			this.tempObject = null;
			
		}

		public override void Validate() {

			base.Validate();

			if (this.tempObject == null) return;

			this.Validate(this.tempObject);

		}

		public override void Validate(Object item) {
			
			base.Validate(item);

			if (item == null) {
				
				if (this.tempObject != null) {
					
					item = this.tempObject;
					
				}

				if (item == null) return;
				
			}
			
			var tempSprite = item as Sprite;
			
			if (tempSprite != null) {
				
				var imp = UnityEditor.TextureImporter.GetAtPath(this.assetPath) as UnityEditor.TextureImporter;
				this.multiObjects = false;
				if (imp != null && imp.spriteImportMode == UnityEditor.SpriteImportMode.Multiple) {

					var allObjects = Resources.LoadAll(this.resourcesPath);
					this.multiObjects = true;
					this.objectIndex = System.Array.IndexOf(allObjects, item);
					
				}
				
			}

			this.tempObject = item;

		}
		#endif
		
	}

}