using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	[System.Serializable]
	public class MonoResource : ResourceBase {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		public MonoBehaviour tempResource;
		
		public override void Reset() {
			
			base.Reset();
			
			this.tempResource = null;
			
		}
		
		public override void Validate(Object item) {
			
			if (item == null) {
				
				if (this.tempResource != null) {
					
					item = this.tempResource;
					
				}
				
				if (item == null) return;
				
			}
			
			this.tempResource = item as MonoBehaviour;
			
			base.Validate(item);
			
		}
		#endif
		
	};

}