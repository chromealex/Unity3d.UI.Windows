using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview.Output {
	
	public interface IDeviceOutput {
		
		void SetRect(Rect screenRect, Rect rect, Rect landscapeRect, ScreenOrientation orientation);
		void OnPreGUI();
		void OnPostGUI();
		void DoPreGUI();
		void DoPostGUI();

	}

}