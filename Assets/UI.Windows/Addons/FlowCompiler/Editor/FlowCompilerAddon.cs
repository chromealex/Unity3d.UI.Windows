using UnityEditor.UI.Windows.Plugins.DevicePreview;
using UnityEditor;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Plugins.FlowCompiler {

	public class FlowCompiler : IWindowAddon {

		public static void ShowEditor(System.Action onClose) {

			//FlowCompilerWizard.ShowEditor(onClose);

		}

		public void Show(System.Action onClose) {
			
			FlowCompiler.ShowEditor(onClose);

		}

	}

}