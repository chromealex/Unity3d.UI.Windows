using UnityEditor.UI.Windows.Plugins.DevicePreview;
using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview {

	public class DevicePreview : FlowAddon {

		[MenuItem("Window/UI.Windows: Device Preview")]
		public static void ShowEditor() {
			
			DevicePreview.ShowEditor(null);
			
		}

		public static void ShowEditor(System.Action onClose) {

			DevicePreviewWindow.ShowEditor(onClose);

		}

		public override void Show(System.Action onClose) {
			
			DevicePreview.ShowEditor(onClose);

		}

		public override void OnFlowSettingsGUI() {

			GUILayout.Label("Module Installed");

			if (GUILayout.Button("Open Preview") == true) {

				DevicePreview.ShowEditor();

			}

		}

	}

}