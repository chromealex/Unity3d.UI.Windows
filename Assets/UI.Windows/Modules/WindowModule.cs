using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {
	
	[ExecuteInEditMode()]
	public class WindowModule : WindowLayoutBase {

		[Header("Module")]
		public int defaultSortingOrder;
		public bool defaultBackgroundLayer;
		
		[Header("Module Editor")]
		#if UNITY_EDITOR
		public string comment;
		#endif

	}

}