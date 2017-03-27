using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using ME.UAB;

namespace UnityEngine.UI.Windows {

	[System.Serializable]
	public class ResourceMono : ResourceBase, IResourceValidationObject {

		public Object GetValidationObject() {

#if UNITY_EDITOR
			return this.tempResource;
#else
			return null;
#endif

		}

		#if UNITY_EDITOR
		//[HideInInspector]
		[BundleIgnore]
		public MonoBehaviour tempResource;
		
		public override void ResetToDefault() {
			
			base.ResetToDefault();
			
			this.tempResource = null;
			
		}

		public override void Validate() {

			this.Validate(this.tempResource);

		}

		public override void Validate(Object item) {
			
			var r = this.tempResource as Object;
			ME.EditorUtilities.SetObjectIfDirty(ref r, item);
			this.tempResource = r as MonoBehaviour;

			base.Validate(item);
			
		}
		#endif
		
	};

}