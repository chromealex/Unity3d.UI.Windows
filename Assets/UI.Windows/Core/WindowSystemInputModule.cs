using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows {

	public class WindowSystemInputModule : StandaloneInputModule {
		
		public PointerEventData GetEventData() {
			
			return base.GetBaseEventData() as PointerEventData;
			
		}
		
		public RaycastResult GetCurrentRaycast() {
			
			var data = this.GetLastPointerEventData(PointerInputModule.kMouseLeftId);
			return data.pointerCurrentRaycast;
			
		}

	}

}