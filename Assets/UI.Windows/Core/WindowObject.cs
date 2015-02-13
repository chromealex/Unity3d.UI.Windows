using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public class WindowObject : MonoBehaviour {
		
		private WindowBase window;
		
		internal void Setup(WindowBase window) {
			
			this.window = window;
			
		}
		
		public WindowBase GetWindow() {
			
			return this.window;
			
		}

	}

}