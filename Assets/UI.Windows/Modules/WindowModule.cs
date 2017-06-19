using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {
	
	[ExecuteInEditMode()]
	public class WindowModule : WindowLayoutBase {

		[Header("Module")]
		public int defaultSortingOrder;
		public bool defaultBackgroundLayer;

		public virtual bool IsInstantiate() {

			return true;

		}

		public virtual bool IsSupported() {

			return true;

		}

		#if UNITY_EDITOR
		[Header("Module Editor")]
		[TextArea]
		public string comment;
		#endif

	}

}