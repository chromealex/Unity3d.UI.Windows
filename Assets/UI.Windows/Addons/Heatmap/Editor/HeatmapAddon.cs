using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using UnityEngine.UI.Windows.Plugins.Flow;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows.Plugins.Heatmap {

	public class Heatmap : FlowAddon {
		
		private static HeatmapSettings settings;
		
		private Editor editor;
		private Texture noDataTexture;
		
		public override string GetName() {
			
			return "Heatmap (pre-alpha)";
			
		}

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
				
				GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

				if (this.editor == null) this.editor = Editor.CreateEditor(settings);
				if (this.editor != null) {
					
					this.editor.OnInspectorGUI();
					
				}

				if (GUILayout.Button("Refresh") == true) {

					//var settings = Heatmap.settings;
					//TODO:..
					settings.data.UpdateMap();

				}

			}

		}

		public override void OnFlowWindowLayoutGUI(Rect rect, FD.FlowWindow window) {
			
			if (Heatmap.settings == null) Heatmap.settings = this.GetSettingsFile();

			var settings = Heatmap.settings;
			if (settings != null) {

				if (settings.show == true) {

					var data = settings.data.Get(window);
					//data.UpdateMap();

					if (data != null && data.texture != null && data.status == HeatmapSettings.WindowsData.Window.Status.Loaded) {

						LayoutWindowType screen;
						var layout = HeatmapSystem.GetLayout(window.id, out screen);
						if (layout == null) return;

						var scaleFactor = HeatmapSystem.GetFactor(new Vector2(layout.root.editorRect.width, layout.root.editorRect.height), rect.size);
						//var scaleFactorCanvas = layout.editorScale > 0f ? 1f / layout.editorScale : 1f;
						//scaleFactor *= scaleFactorCanvas;

						var r = layout.root.editorRect;
						r.x *= scaleFactor;
						r.y *= scaleFactor;
						r.x += rect.x + rect.width * 0.5f;
						r.y += rect.y + rect.height * 0.5f;
						r.width *= scaleFactor;
						r.height *= scaleFactor;

						var c = Color.white;
						c.a = 0.5f;
						GUI.color = c;
						GUI.DrawTexture(r, data.texture, ScaleMode.StretchToFill, alphaBlend: false);
						GUI.color = Color.white;

					} else {
						
						if (this.noDataTexture != null) GUI.DrawTexture(rect, this.noDataTexture, ScaleMode.ScaleToFit, alphaBlend: true);

					}

				}

			}

		}
		
		public override GenericMenu GetSettingsMenu(GenericMenu menu) {
			
			if (menu == null) menu = new GenericMenu();
			menu.AddItem(new GUIContent("Reinstall"), false, () => { this.Reinstall(); });
			
			return menu;
			
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

			/*var data = FlowSystem.GetData();
			if (data == null) return false;
			
			// Check directories
			var dataPath = AssetDatabase.GetAssetPath(data);
			var directory = Path.GetDirectoryName(dataPath);
			var projectName = data.name;
			
			var modulesPath = Path.Combine(directory, projectName + ".Modules");

			var settings = ME.EditorUtilities.GetAssetsOfType<HeatmapSettings>(modulesPath, useCache: false).FirstOrDefault();
			
			return settings == null;*/

			return this.GetSettingsFile() == null;
			
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