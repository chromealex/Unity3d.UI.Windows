using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine;
using ME;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using System.IO;

namespace UnityEditor.UI.Windows.Plugins.Flow {
	
	public interface IWindowFlowAddon : IWindowAddon {
		
		FlowSystemEditorWindow flowEditor { get; set; }
		
		void OnFlowWindow(FD.FlowWindow window);
		void OnFlowWindowGUI(FD.FlowWindow window);
		void OnFlowWindowLayoutGUI(Rect rect, FD.FlowWindow window);
		void OnFlowSettingsGUI();
		void OnFlowToolbarGUI(GUIStyle toolbarButton);
		void OnFlowCreateMenuGUI(string prefix, GenericMenu menu);
		void OnFlowToolsMenuGUI(string prefix, GenericMenu menu);
		void OnFlowWindowScreenMenuGUI(FD.FlowWindow window, GenericMenu menu);
		
		string OnCompilerTransitionGeneration(FD.FlowWindow window);
		string OnCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName);
		string OnCompilerTransitionTypedAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName, System.Type[] types, string[] names);
		bool IsCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo);

		Vector2 OnFlowDrawNodeCurveOffset(UnityEngine.UI.Windows.AttachItem attachItem, FD.FlowWindow fromWindow, FD.FlowWindow toWindow, bool doubleSide);

		void OnFlowWindowTransition(int index, FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool doubleSided, Vector2 centerOffset);

		void OnGUI();

		void Reset();

		void Install();
		bool InstallationNeeded();

	}

	public abstract class FlowAddon : IWindowFlowAddon {
		
		public const string MODULE_INSTALLED = "The module is installed properly";
		public const string MODULE_HAS_ERRORS = "{0} Please, try to re-open Unity and/or reinstall the module.";

		public string name;

		public virtual string GetName() {

			return this.name;

		}

		public FlowSystemEditorWindow flowEditor { get; set; }
		
		public virtual GenericMenu GetSettingsMenu(GenericMenu menu) { return menu; }

		public virtual void Show(System.Action onClose) {}
		public virtual void OnFlowSettingsGUI() {}
		public virtual void OnFlowWindow(FD.FlowWindow window) {}
		public virtual void OnFlowWindowGUI(FD.FlowWindow window) {}
		public virtual void OnFlowWindowLayoutGUI(Rect rect, FD.FlowWindow window) {}
		public virtual void OnFlowToolbarGUI(GUIStyle buttonStyle) {}
		public virtual void OnFlowCreateMenuGUI(string prefix, GenericMenu menu) {}
		public virtual void OnFlowToolsMenuGUI(string prefix, GenericMenu menu) {}
		public virtual void OnFlowWindowScreenMenuGUI(FD.FlowWindow window, GenericMenu menu) {}
		
		public virtual string OnCompilerTransitionGeneration(FD.FlowWindow window) { return string.Empty; }
		public virtual string OnCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName) { return string.Empty; }
		public virtual string OnCompilerTransitionTypedAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName, System.Type[] types, string[] names) { return string.Empty; }
		public virtual bool IsCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo) { return false; }
		
		public virtual Vector2 OnFlowDrawNodeCurveOffset(UnityEngine.UI.Windows.AttachItem attachItem, FD.FlowWindow fromWindow, FD.FlowWindow toWindow, bool doubleSide) { return Vector2.zero; }
		
		public virtual void OnFlowWindowTransition(int index, FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool doubleSided, Vector2 centerOffset) {}

		public virtual void OnGUI() {}

		public virtual void Reset() {}

		public virtual void Install() {}
		public virtual void Reinstall() {}
		public virtual bool InstallationNeeded() { return false; }

		public bool InstallModule<T>(string name, string settingsName) where T : UnityEngine.UI.Windows.Plugins.Services.ServiceSettings {

			var moduleName = name;
			var settings = new[] {
				new { type = typeof(T), name = settingsName, directory = "" }
			};

			var data = FlowSystem.GetData();
			if (data == null) return false;

			// Check directories
			var dataPath = AssetDatabase.GetAssetPath(data);
			var directory = Path.GetDirectoryName(dataPath);
			var projectName = data.name;

			var modulesPath = Path.Combine(directory, projectName + ".Modules");
			var modulePath = Path.Combine(modulesPath, moduleName);

			if (Directory.Exists(modulesPath) == false) Directory.CreateDirectory(modulesPath);
			if (Directory.Exists(modulePath) == false) Directory.CreateDirectory(modulePath);

			foreach (var file in settings) {

				var path = Path.Combine(modulePath, file.directory);
				if (Directory.Exists(path) == false) Directory.CreateDirectory(path);

				if (File.Exists(path + "/" + file.name + ".asset") == false) {

					var instance = ME.EditorUtilities.CreateAsset(file.type, path, file.name) as T;

					if (instance != null) EditorUtility.SetDirty(instance);

				}

			}

			ME.EditorUtilities.ResetCache<T>(modulesPath);

			AssetDatabase.Refresh();

			return true;

		}

	}

	public class Flow : IWindowAddon {

		public static void DrawModuleSettingsGUI(IWindowFlowAddon addon, string caption, GenericMenu settingsMenu, System.Action onGUI) {
			
			//CustomGUI.Splitter(new Color(0.7f, 0.7f, 0.7f, 0.2f));

			var key = "UI.Windows.Addons." + caption + ":foldout";
			var show = EditorPrefs.GetBool(key, true);

			GUILayout.BeginHorizontal();
			{

				var style = ME.Utilities.CacheStyle("UI.Windows.Settings.AddonToggle", "Toggle", (name) => {
					
					return FlowSystemEditorWindow.defaultSkin.FindStyle("SettingsAddonToggle");
					
				});
				var styleSelected = ME.Utilities.CacheStyle("UI.Windows.Settings.AddonToggle", "ToggleSelected", (name) => {
					
					return FlowSystemEditorWindow.defaultSkin.FindStyle("SettingsAddonToggleSelected");
					
				});

				var newShow = GUILayout.Toggle(show, caption.ToSentenceCase().UppercaseWords(), show == true ? styleSelected : style);
				var rect = GUILayoutUtility.GetLastRect();
				if (GUI.enabled == true) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
				if (rect.Contains(Event.current.mousePosition) == true) FlowSystemEditorWindow.GetWindow<FlowSystemEditorWindow>().Repaint();
				if (newShow != show) {
					
					show = newShow;
					EditorPrefs.SetBool(key, show);

				}

				if (settingsMenu != null) {

					var settingsStyle = new GUIStyle("PaneOptions");
					if (GUILayout.Button(string.Empty, settingsStyle) == true) {

						settingsMenu.ShowAsContext();

					}

				}

			}
			GUILayout.EndHorizontal();
			
			//CustomGUI.Splitter(new Color(0.7f, 0.7f, 0.7f, 0.2f));

			if (show == true) {

				GUILayout.BeginVertical(FlowSystemEditorWindow.defaultSkin.box);
				{

					if (addon != null && addon.InstallationNeeded() == true) {

						GUILayout.BeginHorizontal();
						{
							GUILayout.FlexibleSpace();
							if (GUILayoutExt.LargeButton("Install", 40f, 200f) == true) {

								addon.Install();
								
							}
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();

					} else {

						onGUI();

					}

				}
				GUILayout.EndVertical();

			}

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

		public static FlowData FindProjectForPath(string root, string path) {

			var topPath = path.Split('/')[0];
			var datas = ME.EditorUtilities.GetAssetsOfType<FlowData>(root + "/" + topPath, useCache: false);

			if (datas.Length > 0) return datas[0];

			return null;

		}
		
		public static Vector2 OnDrawNodeCurveOffset(FlowSystemEditorWindow flowEditor, UnityEngine.UI.Windows.AttachItem attachItem, FD.FlowWindow fromWindow, FD.FlowWindow toWindow, bool doubleSide) {

			var offset = Vector2.zero;

			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				offset += addon.OnFlowDrawNodeCurveOffset(attachItem, fromWindow, toWindow, doubleSide);
				
			}

			return offset;
			
		}
		
		public static void OnFlowWindowTransition(FlowSystemEditorWindow flowEditor, int index, FD.FlowWindow fromWindow, FD.FlowWindow toWindow, bool doubleSided, Vector2 centerOffset) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowWindowTransition(index, fromWindow, toWindow, doubleSided, centerOffset);
				
			}
			
		}
		
		public static void OnDrawWindow(FlowSystemEditorWindow flowEditor, FD.FlowWindow window) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowWindow(window);
				
			}
			
		}

		public static void OnDrawWindowGUI(FlowSystemEditorWindow flowEditor, FD.FlowWindow window) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowWindowGUI(window);
				
			}
			
		}
		
		public static void OnDrawWindowLayoutGUI(Rect rect, FD.FlowWindow window) {
			
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

					#if DEBUGBUILD
					Profiler.BeginSample("[ GUI ] Addon " + addon.name);
					#endif

					addon.flowEditor = flowEditor;
					Flow.DrawModuleSettingsGUI(addon, addon.GetName(), addon.GetSettingsMenu(null), () => { addon.OnFlowSettingsGUI(); });

					#if DEBUGBUILD
					Profiler.EndSample();
					#endif

				}
			
			}

			CustomGUI.Splitter();

			GUILayout.BeginHorizontal();
			{
				
				GUILayout.FlexibleSpace();

				if (GUILayoutExt.LargeButton("Install Modules...", 40f, 200f) == true) {

					Application.OpenURL(VersionInfo.DOWNLOAD_LINK);

				}

				GUILayout.FlexibleSpace();

			}
			GUILayout.EndHorizontal();

		}
		
		public static void OnReset(FlowSystemEditorWindow flowEditor) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.Reset();
				
			}
			
		}

		public static void OnDrawGUI(FlowSystemEditorWindow flowEditor) {

			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnGUI();
				
			}

		}

		public static void OnDrawToolbarGUI(FlowSystemEditorWindow flowEditor, GUIStyle buttonStyle) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowToolbarGUI(buttonStyle);
				
			}
			
		}
		
		public static void OnFlowWindowScreenMenuGUI(FlowSystemEditorWindow flowEditor, FD.FlowWindow window, GenericMenu menu) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowWindowScreenMenuGUI(window, menu);
				
			}

		}

		public static void OnDrawCreateMenuGUI(FlowSystemEditorWindow flowEditor, string prefix, GenericMenu menu) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowCreateMenuGUI(prefix, menu);
				
			}
			
		}
		
		public static void OnDrawToolsMenuGUI(FlowSystemEditorWindow flowEditor, string prefix, GenericMenu menu) {
			
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				addon.flowEditor = flowEditor;
				addon.OnFlowToolsMenuGUI(prefix, menu);
				
			}
			
		}

		public static string OnCompilerTransitionGeneration(FD.FlowWindow window) {
			
			var result = string.Empty;
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				result += addon.OnCompilerTransitionGeneration(window);
				
			}
			
			return result;
			
		}
		
		public static bool IsCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo) {

			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {

				if (addon.IsCompilerTransitionAttachedGeneration(windowFrom, windowTo) == true) return true;

			}

			return false;
			
		}
		
		public static string OnCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName) {
			
			var result = string.Empty;
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				result += addon.OnCompilerTransitionAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName);
				
			}
			
			return result;
			
		}
		
		public static string OnCompilerTransitionTypedAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName, System.Type[] types, string[] names) {
			
			var result = string.Empty;
			var flowAddons = CoreUtilities.GetAddons<IWindowFlowAddon>();
			foreach (var addon in flowAddons) {
				
				result += addon.OnCompilerTransitionTypedAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName, types, names);
				
			}
			
			return result;
			
		}

	}

}