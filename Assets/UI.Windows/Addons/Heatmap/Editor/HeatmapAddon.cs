using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEditor.UI.Windows.Plugins.Heatmap {

	public class Heatmap : FlowAddon {
		
		private static HeatmapSettings settings;
		
		private Editor editor;
		private Texture noDataTexture;

		public override void OnFlowSettingsGUI() {
			
			if (Heatmap.settings == null) {

				Heatmap.settings = this.GetSettingsFile();
				//if (Heatmap.settings == null) Heatmap.settings = ME.EditorUtilities.GetAssetsOfType<HeatmapSettings>(useCache: false).FirstOrDefault();

			}

			if (this.noDataTexture == null) this.noDataTexture = Resources.Load("UI.Windows/Heatmap/NoData") as Texture;

			var settings = Heatmap.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (HeatmapSettings)."), MessageType.Error);
				
			} else {
				
				GUILayout.Label(FlowAddon.MODULE_INSTALLED);
				
				if (this.editor == null) this.editor = Editor.CreateEditor(settings);
				if (this.editor != null) {
					
					this.editor.OnInspectorGUI();
					
				}
				
			}

		}

		public override void OnFlowWindowLayoutGUI(Rect rect, UnityEngine.UI.Windows.Plugins.Flow.FlowWindow window) {
			
			if (Heatmap.settings == null) Heatmap.settings = this.GetSettingsFile();

			var settings = Heatmap.settings;
			if (settings != null) {

				if (settings.show == true) {

					var data = settings.data.Get(window);
					data.UpdateMap();

					if (data != null && data.texture != null && data.status == HeatmapSettings.WindowsData.Window.Status.Loaded) {

						GUI.DrawTexture(rect, data.texture, ScaleMode.StretchToFill, alphaBlend: true);

					} else {
						
						GUI.DrawTexture(rect, this.noDataTexture, ScaleMode.StretchToFill, alphaBlend: true);

					}

				}

			}

		}
		
		private HeatmapSettings GetSettingsFile() {
			
			var data = FlowSystem.GetData();
			if (data == null) return null;

			var dataPath = AssetDatabase.GetAssetPath(data);
			var directory = Path.GetDirectoryName(dataPath);
			var projectName = data.name;
			var modulesPath = Path.Combine(directory, projectName + ".Modules");
			
			var settings = ME.EditorUtilities.GetAssetsOfType<HeatmapSettings>(modulesPath, useCache: true);
			
			if (settings != null && settings.Length > 0) {
				
				return settings[0];
				
			}
			
			return null;
			
		}
		
		public override bool InstallationNeeded() {
			
			//var moduleName = "Heatmap";

			var data = FlowSystem.GetData();
			if (data == null) return false;
			
			// Check directories
			var dataPath = AssetDatabase.GetAssetPath(data);
			var directory = Path.GetDirectoryName(dataPath);
			var projectName = data.name;
			
			var modulesPath = Path.Combine(directory, projectName + ".Modules");

			var settings = ME.EditorUtilities.GetAssetsOfType<HeatmapSettings>(modulesPath, useCache: false).FirstOrDefault();
			
			return settings == null;
			
		}
		
		public override void Install() {
			
			this.Install_INTERNAL();
			
		}
		
		public override void Reinstall() {
			
			this.Install_INTERNAL();
			
		}
		
		private bool Install_INTERNAL() {
			
			var moduleName = "Heatmap";
			var settings = new[] {
				new { type = typeof(HeatmapSettings), name = "HeatmapSettings", directory = "" }
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
					
					var instance = ME.EditorUtilities.CreateAsset(file.type, path, file.name) as HeatmapSettings;

					if (instance != null) EditorUtility.SetDirty(instance);

				}
				
			}
			
			AssetDatabase.Refresh();
			
			return false;
			
		}

	}

}