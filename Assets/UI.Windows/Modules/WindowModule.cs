using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {
	
	[ExecuteInEditMode()]
	public class WindowModule : WindowLayoutBase {
		
		#if UNITY_EDITOR
		public string comment;
		#endif

		public int defaultSortingOrder;
		public bool defaultBackgroundLayer;

	}

}