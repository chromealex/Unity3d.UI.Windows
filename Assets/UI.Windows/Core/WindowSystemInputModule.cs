﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows {

	public class WindowSystemInputModule : StandaloneInputModule {
		
		public PointerEventData GetEventData() {
			
			return base.GetBaseEventData() as PointerEventData;
			
		}
		
		public RaycastResult GetCurrentRaycast(int id = PointerInputModule.kMouseLeftId) {
			
			var data = this.GetLastPointerEventData(id);
			if (data == null) return new RaycastResult();
			
			return data.pointerCurrentRaycast;
			
		}

	}

}