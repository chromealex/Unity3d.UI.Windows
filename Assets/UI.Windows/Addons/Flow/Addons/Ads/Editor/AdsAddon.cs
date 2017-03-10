using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Flow;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows.Plugins.Ads;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Plugins;

namespace UnityEditor.UI.Windows.Plugins.Ads {

	public class Ads : FlowAddon {
		
		public class Styles {
			
			public GUISkin skin;
			
			public Styles() {
				
				this.skin = UnityEngine.Resources.Load("UI.Windows/Flow/Styles/Skin" + (EditorGUIUtility.isProSkin == true ? "Dark" : "Light")) as GUISkin;
				if (this.skin != null) {
					
				}
				
			}
			
		}

		private static AdsSettings settings;

		private AdsSettingsEditor editor;

		public override string GetName() {
			
			return "Ads";
			
		}

		public override void OnFlowSettingsGUI() {
			
			if (Ads.settings == null) Ads.settings = Ads.GetSettingsFile();

			var settings = Ads.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (HeatmapSettings)."), MessageType.Error);
				
			} else {
				
				GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

				if (this.editor == null) {

					this.editor = Editor.CreateEditor(settings) as AdsSettingsEditor;

				}

				if (this.editor != null) {
					
					this.editor.OnInspectorGUI();
					
				}

			}

		}

		public override GenericMenu GetSettingsMenu(GenericMenu menu) {
			
			if (menu == null) menu = new GenericMenu();
			menu.AddItem(new GUIContent("Reinstall"), false, () => { this.Reinstall(); });
			
			return menu;
			
		}

		public static AdsSettings GetSettingsFile() {
			
			var data = FlowSystem.GetData();
			if (data == null) return null;

			var modulesPath = data.GetModulesPath();

			var settings = ME.EditorUtilities.GetAssetsOfType<AdsSettings>(modulesPath, useCache: true);
			if (settings != null && settings.Length > 0) {
				
				return settings[0];
				
			}
			
			return null;
			
		}

		public override bool InstallationNeeded() {

			return Ads.GetSettingsFile() == null;
			
		}
		
		public override void Install() {
			
			this.Install_INTERNAL();
			
		}
		
		public override void Reinstall() {
			
			this.Install_INTERNAL();
			
		}
		
		private bool Install_INTERNAL() {
			
			var moduleName = "Ads";
			var settings = new[] {
				new { type = typeof(AdsSettings), name = "AdsSettings", directory = "" }
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
					
					var instance = ME.EditorUtilities.CreateAsset(file.type, path, file.name) as AdsSettings;

					if (instance != null) EditorUtility.SetDirty(instance);

				}
				
			}

			ME.EditorUtilities.ResetCache<AdsSettings>(modulesPath);

			AssetDatabase.Refresh();
			
			return false;
			
		}

	}

}