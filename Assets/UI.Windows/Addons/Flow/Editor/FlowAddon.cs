using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine;
using ME;

namespace UnityEditor.UI.Windows.Plugins.Flow {
	
	public interface IWindowFlowAddon : IWindowAddon {
		
		FlowSystemEditorWindow flowEditor { get; set; }
		
		void OnFlowWindowGUI(FlowWindow window);
		void OnFlowWindowLayoutGUI(Rect rect, FlowWindow window);
		void OnFlowSettingsGUI();
		void OnFlowToolbarGUI(GUIStyle toolbarButton);
		void OnFlowCreateMenuGUI(GenericMenu menu);
		void OnFlowToolsMenuGUI(GenericMenu menu);
		
		string OnCompilerTransitionGeneration(FlowWindow window);
		string OnCompilerTransitionAttachedGeneration(FlowWindow window, bool everyPlatformHasUniqueName);
		bool IsCompilerTransitionAttachedGeneration(FlowWindow window);

		void Install();
		bool InstallationNeeded();

	}

	public class FlowAddon : IWindowFlowAddon {
		
		public const string MODULE_INSTALLED = "The module is installed properly";
		public const string MODULE_HAS_ERRORS = "{0} Please, try to re-open Unity and/or reinstall the module.";

		public string name;

		public FlowSystemEditorWindow flowEditor { get; set; }

		public virtual void Show(System.Action onClose) {}
		public virtual void OnFlowSettingsGUI() {}
		public virtual void OnFlowWindowGUI(FlowWindow window) {}
		public virtual void OnFlowWindowLayoutGUI(Rect rect, FlowWindow window) {}
		public virtual void OnFlowToolbarGUI(GUIStyle buttonStyle) {}
		public virtual void OnFlowCreateMenuGUI(GenericMenu menu) {}
		public virtual void OnFlowToolsMenuGUI(GenericMenu menu) {}
		
		public virtual string OnCompilerTransitionGeneration(FlowWindow window) { return string.Empty; }
		public virtual string OnCompilerTransitionAttachedGeneration(FlowWindow window, bool everyPlatformHasUniqueName) { return string.Empty; }
		public virtual bool IsCompilerTransitionAttachedGeneration(FlowWindow window) { return false; }

		public virtual void Install() {}
		public virtual void Reinstall() {}
		public virtual bool InstallationNeeded() { return false; }

	}

	public class Flow : IWindowAddon {

		public static void DrawModuleSettingsGUI(IWindowFlowAddon addon, string caption, System.Action onGUI) {
			
			GUILayout.Label(caption.ToSentenceCase().UppercaseWords(), EditorStyles.boldLabel);

			GUILayout.BeginVertical(FlowSystemEditorWindow.defaultSkin.box);//GUI.skin.box);
			{
				if (addon != null && addon.InstallationNeeded() == true) {
					
					if (GUILayoutExt.LargeButton("Install", 40f, 200f) == true) {

						addon.Install();
						
					}

				} else {

					onGUI();

				}
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
		
		public static void OnDrawWindowGUI(FlowSystemEditorWindow flowEditor, FlowWindow window) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowWindowGUI(window);
				
			}
			
		}
		
		public static void OnDrawWindowLayoutGUI(Rect rect, FlowWindow window) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {

				addon.OnFlowWindowLayoutGUI(rect, window);
				
			}
			
		}

		public static void OnDrawSettingsGUI(FlowSystemEditorWindow flowEditor) {
			
			var flowAddons = CoreUtilities.GetAddons<FlowAddon>((name, item) => item.name = name);
			if (flowAddons.Count == 0) {

				GUILayout.Label("No Modules Have Been Installed.");

			} else {

				foreach (var addon in flowAddons) {
					
					addon.flowEditor = flowEditor;
					Flow.DrawModuleSettingsGUI(addon, addon.name, () => { addon.OnFlowSettingsGUI(); });

				}
			
			}

			CustomGUI.Splitter();

			GUILayout.BeginHorizontal();
			{
				
				GUILayout.FlexibleSpace();

				if (GUILayoutExt.LargeButton("Install Modules...", 40f, 200f) == true) {

					Application.OpenURL(VersionInfo.downloadLink);

				}

				GUILayout.FlexibleSpace();

			}
			GUILayout.EndHorizontal();

		}

		public static void OnDrawToolbarGUI(FlowSystemEditorWindow flowEditor, GUIStyle buttonStyle) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowToolbarGUI(buttonStyle);
				
			}
			
		}
		
		public static void OnDrawCreateMenuGUI(FlowSystemEditorWindow flowEditor, GenericMenu menu) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowCreateMenuGUI(menu);
				
			}
			
		}
		
		public static void OnDrawToolsMenuGUI(FlowSystemEditorWindow flowEditor, GenericMenu menu) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowToolsMenuGUI(menu);
				
			}
			
		}

		public static string OnCompilerTransitionGeneration(FlowWindow window) {
			
			var result = string.Empty;
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				result += addon.OnCompilerTransitionGeneration(window);
				
			}
			
			return result;
			
		}
		
		public static bool IsCompilerTransitionAttachedGeneration(FlowWindow window) {

			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {

				if (addon.IsCompilerTransitionAttachedGeneration(window) == true) return true;

			}

			return false;
			
		}

		public static string OnCompilerTransitionAttachedGeneration(FlowWindow window, bool everyPlatformHasUniqueName) {
			
			var result = string.Empty;
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				result += addon.OnCompilerTransitionAttachedGeneration(window, everyPlatformHasUniqueName);
				
			}
			
			return result;
			
		}

	}

}