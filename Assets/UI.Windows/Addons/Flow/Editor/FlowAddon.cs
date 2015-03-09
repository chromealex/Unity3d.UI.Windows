#if UNITY_EDITOR
using UnityEditor.UI.Windows.Plugins.DevicePreview;
using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Plugins.Flow;


#endif

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class Flow : IWindowAddon {
		
		#if UNITY_EDITOR
		[MenuItem("Window/UI.Windows: Flow")]
		public static void ShowEditor() {
			
			Flow.ShowEditor(null);
			
		}
		
		[MenuItem("Assets/Open In Flow Editor", validate = true)]
		private static bool ContextMenuValidate() {
			
			return Selection.activeObject is FlowData;
		}
		
		[MenuItem("Assets/Open In Flow Editor")]
		[MenuItem("Window/UI.Windows: Flow")]
		public static void ShowEditorFromContextMenu() {
			
			var editor = Flow.ShowEditor(null);
			
			var selectedObject = Selection.activeObject as FlowData;
			if (selectedObject != null) {
				
				editor.OpenFlowData(selectedObject);

			}
			
		}

		public static FlowSystemEditorWindow ShowEditor(System.Action onClose) {
			
			var editor = EditorWindow.GetWindow<FlowSystemEditorWindow>();
			editor.title = "UI.Windows: Flow";
			editor.autoRepaintOnSceneChange = true;

			return editor;

		}
		#endif

		public void Show(System.Action onClose) {

			Flow.ShowEditor(onClose);

		}

	}

}