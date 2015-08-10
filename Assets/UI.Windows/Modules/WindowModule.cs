using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {
	
	[ExecuteInEditMode()]
	public class WindowModule : WindowLayoutBase {

		[Header("Module")]
		public int defaultSortingOrder;
		public bool defaultBackgroundLayer;

		#if UNITY_EDITOR
		[Header("Module Editor")]
		public string comment;
		#endif

	}

}