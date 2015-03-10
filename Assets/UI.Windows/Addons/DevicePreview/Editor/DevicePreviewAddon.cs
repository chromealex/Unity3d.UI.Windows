using UnityEditor.UI.Windows.Plugins.DevicePreview;
using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview {

	public class DevicePreview : IWindowFlowAddon {

		[MenuItem("Window/UI.Windows: Device Preview")]
		public static void ShowEditor() {
			
			DevicePreview.ShowEditor(null);
			
		}

		public static void ShowEditor(System.Action onClose) {

			DevicePreviewWindow.ShowEditor(onClose);

		}

		public void Show(System.Action onClose) {
			
			DevicePreview.ShowEditor(onClose);

		}

		public void OnFlowSettingsGUI() {
			
			GUILayout.Label("Device Preview", EditorStyles.boldLabel);
			GUILayout.Label("Module Installed");

		}

		public void OnFlowWindowGUI(FlowWindow window) {
		}

		public void OnFlowToolbarGUI(GUIStyle button) {
		}

	}

}