using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine;
using ME;

namespace UnityEditor.UI.Windows.Plugins.Flow {
	
	public interface IWindowFlowAddon : IWindowAddon {

		void OnFlowWindowGUI(FlowWindow window);
		void OnFlowSettingsGUI();
		void OnFlowToolbarGUI(GUIStyle toolbarButton);
		
	}

	public class FlowAddon : IWindowFlowAddon {

		public string name;
		
		public virtual void Show(System.Action onClose) {}
		public virtual void OnFlowSettingsGUI() {}
		public virtual void OnFlowWindowGUI(FlowWindow window) {}
		public virtual void OnFlowToolbarGUI(GUIStyle buttonStyle) {}

	}

	public class Flow : IWindowAddon {

		public static void DrawModuleSettingsGUI(string caption, System.Action onGUI) {
			
			GUILayout.Label(caption.ToSentenceCase().UppercaseWords(), EditorStyles.boldLabel);
			
			GUILayout.BeginVertical(FlowSystemEditorWindow.defaultSkin.box);//GUI.skin.box);
			{
				onGUI();
			}
			GUILayout.EndVertical();

		}

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

			return FlowSystemEditorWindow.ShowEditor(onClose);

		}

		public void Show(System.Action onClose) {

			Flow.ShowEditor(onClose);

		}
		
		public static void OnDrawWindowGUI(FlowWindow window) {
			
			var flowAddons = WindowUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.OnFlowWindowGUI(window);
				
			}
			
		}
		
		public static void OnDrawSettingsGUI() {
			
			var flowAddons = WindowUtilities.GetAddons<FlowAddon>((name, item) => item.name = name);
			if (flowAddons.Count == 0) {

				GUILayout.Label("No Modules Have Been Installed.");

			} else {

				foreach (var addon in flowAddons) {

					Flow.DrawModuleSettingsGUI(addon.name, () => { addon.OnFlowSettingsGUI(); });

				}
			
			}

			CustomGUI.Splitter();

			GUILayout.BeginHorizontal();
			{
				
				GUILayout.FlexibleSpace();

				var content = new GUIContent("Install Modules...");
				if (GUILayout.Button(content, FlowSystemEditorWindow.defaultSkin.button, GUILayout.Height(40f), GUILayout.MaxWidth(200f)) == true) {

					Application.OpenURL(VersionInfo.downloadLink);

				}

				GUILayout.FlexibleSpace();

			}
			GUILayout.EndHorizontal();

		}
		
		public static void OnDrawToolbarGUI(GUIStyle buttonStyle) {
			
			var flowAddons = WindowUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.OnFlowToolbarGUI(buttonStyle);
				
			}
			
		}

	}

}