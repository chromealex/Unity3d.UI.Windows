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
		
		public override void OnFlowToolsMenuGUI(string prefix, GenericMenu menu) {
			
			menu.AddSeparator(string.Empty);

			menu.AddItem(new GUIContent(prefix + "Open Device Preview"), on: false, func: () => {
				
				DevicePreview.ShowEditor();

			});
			
		}

		public override void OnFlowSettingsGUI() {
			
			GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			{

				if (GUILayout.Button("Open Preview", FlowSystemEditorWindow.defaultSkin.button) == true) {

					DevicePreview.ShowEditor();

				}

			}
			EditorGUILayout.EndVertical();

		}

	}

}