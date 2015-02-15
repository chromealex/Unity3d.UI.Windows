using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {
	
	[ExecuteInEditMode()]
	public class WindowModule : WindowLayoutBase {

#if UNITY_EDITOR
		protected override void Update() {

			base.Update();

		}
#endif

	}

}