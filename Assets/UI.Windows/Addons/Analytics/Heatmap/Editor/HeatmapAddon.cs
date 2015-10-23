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
using UnityEngine.UI.Windows.Plugins.Analytics;

namespace UnityEditor.UI.Windows.Plugins.Heatmap {

	public class Heatmap : FlowAddon {
		
		private static HeatmapSettings settings;

		private HeatmapSettingsEditor editor;
		private Texture noDataTexture;
		
		private Dictionary<string, ScreenResult> resultsCache = new Dictionary<string, ScreenResult>();
		private Dictionary<string, int> resultsTransitionCache = new Dictionary<string, int>();
		
		public override string GetName() {
			
			return "Analytics (pre-alpha)";
			
		}

		public void DrawBubble(Vector2 position, ScreenResult result, Vector2 centerOffset, string styleName = "flow node hex 4", string format = "<b>All:</b> {0}\n<b>Unique:</b> {1}", int overflowY = 0) {

			var topOffset = 22;

			var style = ME.Utilities.CacheStyle("UI.Windows.Plugins.Heatmap", styleName, (name) => {

				var _style = new GUIStyle(name);
				_style.alignment = TextAnchor.MiddleLeft;
				_style.padding = new RectOffset(20, 20, 0, 0);
				_style.margin = new RectOffset();
				_style.richText = true;
				return _style;

			});

			if (style == null) return;
			style.overflow = new RectOffset(0, 0, topOffset + overflowY, 0);

			var content = string.Format(format, result.count, result.uniqueCount);
			var size = style.CalcSize(new GUIContent(content));
			
			float offset = Mathf.Max(size.x, size.y) * 0.5f + 5f;

			EditorGUI.LabelField(new Rect(position - size * 0.5f + centerOffset * offset + Vector2.up * topOffset * 0.5f, size), content, style);

		}

		public void DrawBubble(Rect rect, int index, int fromScreenId, int toScreenId, Vector2 offset, string styleName = "flow node hex 4", string format = "<b>All:</b> {0}\n<b>Unique:</b> {1}", int overflowY = 0) {
			
			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();
			if (this.editor == null) return;

			var settings = Heatmap.settings;
			foreach (var item in settings.keys) {

				if (item.show == true && item.enabled == true) {

					foreach (var service in this.editor.services) {

						if (service.GetPlatformName() == item.platformName) {
							
							var key = string.Format("{0}_{1}_{2}_{3}", item.platformName, index, fromScreenId, toScreenId);
							var keyTransition = string.Format("{0}_{1}", item.platformName, fromScreenId);

							ScreenResult result;
							if (this.resultsCache.TryGetValue(key, out result) == true) {

								if (result != null) {

									this.DrawBubble(rect.center, result, offset, styleName, format, overflowY);

								} else {

									// still loading

								}

							} else {

								this.resultsCache.Add(key, null);
								if (this.resultsTransitionCache.ContainsKey(keyTransition) == false) this.resultsTransitionCache.Add(keyTransition, 0);

								if (toScreenId == -1) {
									
									service.GetScreen(item.authKey, fromScreenId, (_result) => {
										
										this.resultsCache[key] = _result as ScreenResult;
										this.resultsTransitionCache[keyTransition] += (_result as ScreenResult).uniqueCount;
										
									});

								} else {

									service.GetScreenTransition(item.authKey, index, fromScreenId, toScreenId, (_result) => {

										this.resultsCache[key] = _result as ScreenResult;
										this.resultsTransitionCache[keyTransition] -= (_result as ScreenResult).uniqueCount;

									});

								}

							}

						}
						
					}

				}

			}

		}

		public void ResetCache() {

			this.resultsCache.Clear();
			this.resultsTransitionCache.Clear();

		}

		public override void OnFlowWindowTransition(int index, FD.FlowWindow fromWindow, FD.FlowWindow toWindow, bool doubleSided, Vector2 centerOffset) {
			
			var factor = 0.5f;

			if (fromWindow.IsSmall() == true &&
				fromWindow.IsABTest() == true) {

				factor = 0.2f;

			}

			var point = Vector2.Lerp(fromWindow.rect.center, toWindow.rect.center, factor) + centerOffset;
			var rect = new Rect(point, Vector2.zero);

			if (this.flowEditor.ContainsRect(rect) == false) return;

			if (doubleSided == true) {

				var q = Quaternion.LookRotation(toWindow.rect.center - fromWindow.rect.center, Vector3.back);

				this.DrawBubble(rect, index, fromWindow.id, toWindow.id, q * Vector2.left);

				this.DrawBubble(rect, index, toWindow.id, fromWindow.id, q * Vector2.right);

			} else {

				this.DrawBubble(rect, index, fromWindow.id, toWindow.id, Vector2.zero);

			}

		}

		public override void OnFlowWindow(FD.FlowWindow window) {

			if (window.isVisibleState == false) return;

			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();
			
			var settings = Heatmap.settings;
			if (settings != null) {

				var result = new ScreenResult();

				foreach (var item in settings.keys) {
					
					if (item.show == true && item.enabled == true) {
						
						foreach (var service in this.editor.services) {
							
							if (service.GetPlatformName() == item.platformName) {

								var rect = window.rect;
								this.DrawBubble(new Rect(new Vector2(rect.x + rect.width * 0.5f, rect.y), Vector2.zero), 0, window.id, -1, Vector2.zero, "flow node hex 3");

								int value;
								var keyTransition = string.Format("{0}_{1}", item.platformName, window.id);
								if (this.resultsTransitionCache.TryGetValue(keyTransition, out value) == true) {

									result.uniqueCount = value;

								}

								if (result.uniqueCount > 0) {

									// Draw exit bubble
									this.DrawBubble(new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height), result, Vector2.zero, "flow node hex 6", "{1}", 5);

								}

							}

						}

					}

				}

			}

		}

		public override void OnFlowSettingsGUI() {
			
			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();

			if (this.noDataTexture == null) this.noDataTexture = Resources.Load<Texture>("UI.Windows/Heatmap/NoData");

			var settings = Heatmap.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (HeatmapSettings)."), MessageType.Error);
				
			} else {
				
				GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

				if (this.editor == null) {

					this.editor = Editor.CreateEditor(settings) as HeatmapSettingsEditor;
					this.editor.SetResetCallback(this.ResetCache);

				}

				if (this.editor != null) {
					
					this.editor.OnInspectorGUI();
					
				}

			}

		}

		public override void OnFlowWindowLayoutGUI(Rect rect, FD.FlowWindow window) {
			
			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();

			var settings = Heatmap.settings;
			if (settings != null) {

				if (settings.heatmapShow == true) {

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

		public static HeatmapSettings GetSettingsFile() {
			
			var data = FlowSystem.GetData();
			if (data == null) return null;

			var modulesPath = data.GetModulesPath();

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

			return Heatmap.GetSettingsFile() == null;
			
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

			ME.EditorUtilities.ResetCache<HeatmapSettings>();

			AssetDatabase.Refresh();
			
			return false;
			
		}

	}

}