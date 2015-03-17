using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components.Events;

namespace UnityEngine.UI.Windows {

	public class WindowSystemInput : MonoBehaviour {
		
		public static ComponentEvent onPointerUp = new ComponentEvent();
		public static ComponentEvent onPointerDown = new ComponentEvent();
		public static ComponentEvent onPointerClick = new ComponentEvent();
		
		public void LateUpdate() {
			
			#if UNITY_STANDALONE || UNITY_WEBPLAYER
			if (Input.GetMouseButtonDown(0) == true) WindowSystemInput.onPointerDown.Invoke();
			if (Input.GetMouseButtonUp(0) == true) WindowSystemInput.onPointerUp.Invoke();
			if (Input.GetMouseButtonDown(1) == true) WindowSystemInput.onPointerDown.Invoke();
			if (Input.GetMouseButtonUp(1) == true) WindowSystemInput.onPointerUp.Invoke();
			if (Input.GetMouseButtonDown(2) == true) WindowSystemInput.onPointerDown.Invoke();
			if (Input.GetMouseButtonUp(2) == true) WindowSystemInput.onPointerUp.Invoke();
			#endif
			
		}


	}

}