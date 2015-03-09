using UnityEditor.UI.Windows.Plugins.DevicePreview;
using UnityEditor;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview {

	public class DevicePreview : IWindowAddon {

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

	}

}