using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public class WindowObject : MonoBehaviour {

		[HideInInspector]
		private WindowBase window;
		
		internal void Setup(WindowBase window) {
			
			this.window = window;
			
		}
		
		public WindowBase GetWindow() {
			
			return this.window;
			
		}
		
		public void HideCurrentWindow() {
			
			this.GetWindow().Hide();
			
		}

	}

}