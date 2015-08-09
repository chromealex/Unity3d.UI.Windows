using UnityEngine;
using System.Collections;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Social;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Social.Core;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.IO;
using System.Collections.Generic;
using ME;

namespace UnityEditor.UI.Windows.Plugins.Social {
	
	public static class FlowSocialTemplateGenerator {
		
		public static string GenerateTransitionMethod(ModuleSettings settings, bool everyPlatformHasUniqueName) {
			
			var platformName = settings.GetPlatformName();
			var className = settings.GetPlatformClassName();

			var file = Resources.Load("UI.Windows/Social/Templates/TemplateTransitionMethod") as TextAsset;
			if (file == null) {
				
				Debug.LogError("Social Template Loading Error: Could not load template 'TemplateTransitionMethod'");
				
				return string.Empty;

			}
			
			var result = string.Empty;
			var multiModules = string.Empty;
			if (everyPlatformHasUniqueName == true) multiModules = className;

			var part = file.text;

			var moduleName = string.Format("UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.{0}.{0}Module", className);

			result +=
				part.Replace("{MODULE_NAME}", moduleName)
					.Replace("{CLASS_NAME}", className)
					.Replace("{MULTI_MODULES_CLASS_NAME}", multiModules)
					.Replace("{PLATFORM_NAME}", platformName);

			return result;

		}

	}

	public class Social : FlowAddon {

		private static SocialSettings settings;

		private Editor editor;

		public override string GetName() {

			return "Social (pre-alpha)";

		}

		public override void OnFlowSettingsGUI() {
			
			if (Social.settings == null) {
				
				Social.settings = this.GetSettingsFile();
				if (Social.settings == null) Social.settings = ME.EditorUtilities.GetAssetsOfType<SocialSettings>(useCache: false).FirstOrDefault();
				
			}

			var settings = Social.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (SocialSettings)."), MessageType.Error);

			} else {
				
				GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

				if (this.editor == null) this.editor = Editor.CreateEditor(settings);
				if (this.editor != null) {

					this.editor.OnInspectorGUI();

				}

			}

		}
		
		public override void OnFlowCreateMenuGUI(string prefix, GenericMenu menu) {

			if (this.InstallationNeeded() == false) {

				menu.AddSeparator(prefix);

				menu.AddItem(new GUIContent(prefix + "Social"), on: false, func: () => {

					this.flowEditor.CreateNewItem(() => {

						var window = FlowSystem.CreateWindow(FlowWindow.Flags.IsSmall | FlowWindow.Flags.CantCompiled | Social.settings.uniqueTag);
						window.smallStyleDefault = "flow node 1";
						window.smallStyleSelected = "flow node 1 on";
						window.title = "Social";
						
						window.rect.width = 150f;
						window.rect.height = 100f;

						return window;

					});

				});

			}

		}

		public override bool IsCompilerTransitionAttachedGeneration(FlowWindow windowFrom, FlowWindow windowTo) {

			var settings = Social.settings;
			if (settings != null) {
				
				var data = settings.data.Get(windowTo);
				if (data != null && data.settings != null && settings.IsPlatformActive(data.settings) == true) {

					return true;

				}

			}

			return false;

		}

		public override string OnCompilerTransitionAttachedGeneration(FlowWindow windowFrom, FlowWindow windowTo, bool everyPlatformHasUniqueName) {

			var settings = Social.settings;
			if (settings != null) {

				var data = settings.data.Get(windowTo);
				if (data != null && data.settings != null && settings.IsPlatformActive(data.settings) == true) {

					return FlowSocialTemplateGenerator.GenerateTransitionMethod(data.settings, everyPlatformHasUniqueName);
				
				}

			}

			return base.OnCompilerTransitionAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName);
			
		} 

		public override void OnFlowWindowGUI(FlowWindow window) {

			var socialFlag = (window.flags & Social.settings.uniqueTag) == Social.settings.uniqueTag;
			if (socialFlag == true) {

				var settings = Social.settings;
				if (settings == null) return;
				
				var data = settings.data.Get(window);
				var isActiveSelected = settings.IsPlatformActive(data.settings);

				var oldColor = GUI.color;
				GUI.color = isActiveSelected ? Color.white : Color.grey;
				var result = GUILayoutExt.LargeButton(data.settings ? data.settings.GetPlatformName() : "None", 60f, 150f);
				GUI.color = oldColor;
				var rect = GUILayoutUtility.GetLastRect();
				rect.y += rect.height;

				if (result == true) {

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("None"), data.settings == null, () => {

						data.settings = null;

					});

					foreach (var platform in settings.activePlatforms) {

						if (platform.active == true) {

							var item = platform.settings;
							menu.AddItem(new GUIContent(platform.GetPlatformName()), data.settings == platform.settings, () => {

								data.settings = item;

							});
							
						} else {

							menu.AddDisabledItem(new GUIContent(platform.GetPlatformName()));

						}
						
					}

					menu.DropDown(rect);

				}

			}

		}

		private SocialSettings GetSettingsFile() {
			
			var data = FlowSystem.GetData();
			if (data == null) return null;
			
			var dataPath = AssetDatabase.GetAssetPath(data);
			var directory = Path.GetDirectoryName(dataPath);
			var projectName = data.name;
			var modulesPath = Path.Combine(directory, projectName + ".Modules");
			
			var settings = ME.EditorUtilities.GetAssetsOfType<SocialSettings>(modulesPath, useCache: true);
			
			if (settings != null && settings.Length > 0) {
				
				return settings[0];

			}

			return null;

		}

		public override bool InstallationNeeded() {
			
			/*var moduleName = "Social";
			var directories = new List<string>() {
				"VK",
				"VK/Queries",
				"FB",
				"FB/Queries",
			};
			
			var data = FlowSystem.GetData();
			if (data == null) return false;

			// Check directories
			var dataPath = AssetDatabase.GetAssetPath(data);
			var directory = Path.GetDirectoryName(dataPath);
			var projectName = data.name;
			
			var modulesPath = Path.Combine(directory, projectName + ".Modules");
			var modulePath = Path.Combine(modulesPath, moduleName);

			foreach (var dir in directories) {
				
				var path = Path.Combine(modulePath, dir);
				if (Directory.Exists(path) == false) {

					return true;

				}

			}

			var settings = ME.EditorUtilities.GetAssetsOfType<SocialSettings>(modulesPath, useCache: false).FirstOrDefault();;

			return settings == null;*/

			return this.GetSettingsFile() == null;

		}

		public override GenericMenu GetSettingsMenu(GenericMenu menu) {

			if (menu == null) menu = new GenericMenu();
			menu.AddItem(new GUIContent("Reinstall"), false, () => { this.Reinstall(); });

			return menu;

		}

		public override void Install() {

			this.Install_INTERNAL();

		}

		public override void Reinstall() {
			
			this.Install_INTERNAL();

		}

		private bool Install_INTERNAL() {
			
			var moduleName = "Social";
			var directories = new List<string>() {
				"VK",
				"VK/Queries",
				"FB",
				"FB/Queries",
			};
			var settings = new[] {
				new { type = typeof(SocialSettings), name = "SocialSettings", directory = "" }
			};
			var subModules = new[] {
				new { type = typeof(UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.VK.VKSettings), name = "VKSettings", directory = "VK" },
				new { type = typeof(UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.FB.FBSettings), name = "FBSettings", directory = "FB" }
			};

			var platforms = new List<Platform>();

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

			foreach (var dir in directories) {

				var path = Path.Combine(modulePath, dir);
				if (Directory.Exists(path) == false) Directory.CreateDirectory(path);

			}
			
			foreach (var file in subModules) {
				
				var path = Path.Combine(modulePath, file.directory);
				if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
				
				if (File.Exists(path + "/" + file.name + ".asset") == false) {
					
					var instance = ME.EditorUtilities.CreateAsset(file.type, path, file.name) as ModuleSettings;
					
					platforms.Add(new Platform() { active = true, settings = instance });
					
				}
				
			}

			foreach (var file in settings) {
				
				var path = Path.Combine(modulePath, file.directory);
				if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
				
				if (File.Exists(path + "/" + file.name + ".asset") == false) {
					
					var instance = ME.EditorUtilities.CreateAsset(file.type, path, file.name) as SocialSettings;
					
					instance.activePlatforms = new Platform[platforms.Count];
					for (int i = 0; i < platforms.Count; ++i) {

						instance.activePlatforms[i] = platforms[i];

					}

					EditorUtility.SetDirty(instance);

				}
				
			}

			AssetDatabase.Refresh();

			return false;

		}

	}

}