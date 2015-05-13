using UnityEngine;
using System.Collections;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Social;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Social.Core;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.IO;
using System.Collections.Generic;

namespace UnityEditor.UI.Windows.Components.Social {

	public class Social : FlowAddon {

		private static SocialSettings settings;

		private Editor editor;

		public override void OnFlowSettingsGUI() {

			if (Social.settings == null) Social.settings = ME.EditorUtilities.GetAssetsOfType<SocialSettings>(useCache: false).FirstOrDefault();

			var settings = Social.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (SocialSettings)."), MessageType.Error);

			} else {

				GUILayout.Label(FlowAddon.MODULE_INSTALLED);

				if (this.editor == null) this.editor = Editor.CreateEditor(settings);
				if (this.editor != null) {

					this.editor.OnInspectorGUI();

				}

			}

		}

		public override bool InstallationNeeded() {
			
			var moduleName = "Social";
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

			var settings = ME.EditorUtilities.GetAssetsOfType<SocialSettings>(modulesPath, useCache: false);

			return settings == null;

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