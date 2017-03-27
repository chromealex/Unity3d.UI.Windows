using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using UnityEngine.UI.Windows.Plugins.Flow;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows.Plugins.Analytics;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Plugins.Heatmap {

	public class Heatmap : FlowAddon {
		
		public class Styles {
			
			public GUISkin skin;
			public GUIStyle backLock;
			public GUIStyle content;
			public GUIStyle contentScreen;
			public GUIStyle closeButton;
			public GUIStyle listButton;
			public GUIStyle listButtonSelected;
			public GUIStyle listTag;
			public GUIStyle objectField;
			public GUIStyle layoutBack;
			public GUIStyle dropShadow;
			
			public GUIStyle tabButtonLeft;
			public GUIStyle tabButtonMid;
			public GUIStyle tabButtonRight;
			
			public Styles() {
				
				this.skin = UnityEngine.Resources.Load("UI.Windows/Flow/Styles/Skin" + (EditorGUIUtility.isProSkin == true ? "Dark" : "Light")) as GUISkin;
				if (this.skin != null) {
					
					this.backLock = this.skin.FindStyle("LayoutBackLock");
					this.content = this.skin.FindStyle("LayoutContent");
					this.contentScreen = this.skin.FindStyle("LayoutContentScreen");
					this.closeButton = new GUIStyle("TL SelectionBarCloseButton");
					this.listButton = this.skin.FindStyle("ListButton");
					this.listButtonSelected = this.skin.FindStyle("ListButtonSelected");
					
					this.listTag = new GUIStyle(this.skin.FindStyle("ListButton"));
					this.listTag.alignment = TextAnchor.MiddleRight;
					this.objectField = this.skin.FindStyle("ObjectField");
					this.layoutBack = this.skin.FindStyle("LayoutBack");
					this.dropShadow = this.skin.FindStyle("DropShadowOuter");
					
					this.tabButtonLeft = new GUIStyle("ButtonLeft");
					this.tabButtonLeft.margin = new RectOffset();
					this.tabButtonMid = new GUIStyle("ButtonMid");
					this.tabButtonMid.margin = new RectOffset();
					this.tabButtonRight = new GUIStyle("ButtonRight");
					this.tabButtonRight.margin = new RectOffset();
					
				}
				
			}
			
		}

		private static HeatmapSettings settings;

		private HeatmapSettingsEditor editor;
		private Texture noDataTexture;
		
		private Dictionary<string, ScreenResult> resultsCache = new Dictionary<string, ScreenResult>();
		private Dictionary<string, int> resultsTransitionCache = new Dictionary<string, int>();
		
		public override string GetName() {
			
			return "Analytics (pre-alpha)";
			
		}
		
		public void DrawBubble(Vector2 position, string text, string styleName, Vector2 centerOffset) {
			
			var style = ME.Utilities.CacheStyle("UI.Windows.Plugins.Heatmap", styleName, (name) => {
				
				return FlowSystemEditorWindow.defaultSkin.FindStyle(name);
				
			});
			
			if (style == null) return;
			
			var content = text;
			var size = style.CalcSize(new GUIContent(content));
			
			float offset = Mathf.Max(size.x, size.y) * 0.5f + 5f;
			
			EditorGUI.LabelField(new Rect(position - size * 0.5f + centerOffset * offset, size), content, style);

		}

		public void DrawBubble(Vector2 position, ScreenResult result, Vector2 centerOffset, string styleName = "LabelYellow", string format = "<b>All:</b> {0}\n<b>Unique:</b> {1}") {

			var content = string.Format(format, result.count, result.uniqueCount);

			this.DrawBubble(position, content, styleName, centerOffset);

		}

		public void DrawBubble(Rect rect, int index, int fromScreenId, int toScreenId, Vector2 offset, string styleName = "LabelYellow", string format = "<b>All:</b> {0}\n<b>Unique:</b> {1}") {
			
			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();
			if (this.editor == null) return;

			var settings = Heatmap.settings;
			foreach (var item in settings.items) {

				if (item.show == true && item.enabled == true && item.processing == false) {

					foreach (var serviceBase in this.editor.services) {

						var service = serviceBase as IAnalyticsService;
						if (service.GetServiceName() == item.serviceName) {
							
							var key = string.Format("{0}_{1}_{2}_{3}", item.serviceName, index, fromScreenId, toScreenId);
							var keyTransition = string.Format("{0}_{1}", item.serviceName, fromScreenId);

							ScreenResult result;
							if (this.resultsCache.TryGetValue(key, out result) == true) {

								if (result != null) {

									this.DrawBubble(rect.center, result, offset, styleName, format);

								} else {

									// still loading
									this.DrawBubble(rect.center, "Loading...", "LabelYellow", Vector2.zero);

								}

							} else {

								this.resultsCache.Add(key, null);
								if (this.resultsTransitionCache.ContainsKey(keyTransition) == false) this.resultsTransitionCache.Add(keyTransition, 0);

								var filter = item.userFilter;

								if (toScreenId == -1) {

/*
									Debug.Log("Screen Request");
*/
									service.GetScreen(fromScreenId, filter, (_result) => {
										
										this.resultsCache[key] = _result;
										this.resultsTransitionCache[keyTransition] += _result.uniqueCount;
										
									});

								} else {
									
/*
									Debug.Log("Screen Transition Request");
*/
									service.GetScreenTransition(index, fromScreenId, toScreenId, filter, (_result) => {

										this.resultsCache[key] = _result;
										this.resultsTransitionCache[keyTransition] -= _result.uniqueCount;

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

			this.heatmapResultsCache.Clear();
			this.heatmapTexturesCache.Clear();

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
			if (window.IsContainer() == true) return;
			if (window.IsSmall() == true && window.IsFunction() == true) return;
			if (window.IsShowDefault() == true) return;

			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();
			
			var settings = Heatmap.settings;
			if (settings != null) {

				var result = new ScreenResult();

				foreach (var item in settings.items) {
					
					if (item.show == true && item.enabled == true) {
						
						foreach (var serviceBase in this.editor.services) {

							var service = serviceBase as IAnalyticsService;
							if (service.GetServiceName() == item.serviceName) {

								var rect = window.rect;
								this.DrawBubble(new Rect(new Vector2(rect.x + rect.width * 0.5f, rect.y), Vector2.zero), 0, window.id, -1, Vector2.zero, "LabelGreen");

								int value;
								var keyTransition = string.Format("{0}_{1}", item.serviceName, window.id);
								if (this.resultsTransitionCache.TryGetValue(keyTransition, out value) == true) {

									result.uniqueCount = value;

								}

								if (result.uniqueCount > 0 && result.popup == false) {

									// Draw exit bubble
									this.DrawBubble(new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height), result, Vector2.zero, "LabelRed", "{1}");

								}

							}

						}

					}

				}

			}

		}

		public override void OnFlowSettingsGUI() {
			
			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();

			if (this.noDataTexture == null) this.noDataTexture = UnityEngine.Resources.Load<Texture>("UI.Windows/Heatmap/NoData");

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
		
		public static Styles styles = null;
		public const float MARGIN = 5f;

		private bool openFullScreen;
		//private HeatmapResult fullScreenData;
		private Texture2D fullScreenTexture;
		private int fullScreenWindowId;
		private IPreviewEditor fullScreenEditor;

		public override void OnGUI() {

			if (Heatmap.styles == null) Heatmap.styles = new Styles();

			if (this.openFullScreen == true) {
				
				var data = FlowSystem.GetData();
				if (data == null) return;

				const float closeSize = 50f;

				LayoutWindowType screen;
				var layout = HeatmapSystem.GetLayout(this.fullScreenWindowId, out screen);
				if (layout == null) return;

				var window = FlowSystem.GetWindow(this.fullScreenWindowId);
				if (this.fullScreenEditor == null) this.fullScreenEditor = Editor.CreateEditor(window.GetScreen().Load<WindowBase>()) as IPreviewEditor;

				var rect = new Rect(0f, 0f, 780f, 580f);
				var scaleFactor = HeatmapSystem.GetFactor(new Vector2(layout.root.editorRectLocal.width, layout.root.editorRectLocal.height), rect.size);

				var r = layout.root.editorRectLocal;
				r.x *= scaleFactor;
				r.y *= scaleFactor;
				r.x += rect.x + rect.width * 0.5f;
				r.y += rect.y + rect.height * 0.5f;
				r.width *= scaleFactor;
				r.height *= scaleFactor;

				var screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
				var settingsSize = rect.size;
				var settingsRect = new Rect(screenRect.width * 0.5f - settingsSize.x * 0.5f, screenRect.height * 0.5f - settingsSize.y * 0.5f, settingsSize.x, settingsSize.y).PixelPerfect();
				var settingsBackRect = new Rect(settingsRect.x - MARGIN, settingsRect.y - MARGIN, settingsRect.width + MARGIN * 2f, settingsRect.height + MARGIN * 2f).PixelPerfect();
				var rectCloseButton = new Rect(settingsRect.x + settingsRect.width, settingsRect.y - closeSize * 0.5f, closeSize, closeSize).PixelPerfect();
				
				GUI.Box(screenRect, string.Empty, Heatmap.styles.backLock);
				GUI.Box(settingsBackRect, string.Empty, Heatmap.styles.dropShadow);
				GUI.Box(settingsBackRect, string.Empty, Heatmap.styles.contentScreen);

				var outRect = new Rect(r.x + settingsRect.x, r.y + settingsRect.y, r.width, r.height);
				GUI.Box(outRect, string.Empty, Heatmap.styles.layoutBack);
				this.fullScreenEditor.OnPreviewGUI(Color.white, new Rect(outRect.x, outRect.y, outRect.width, outRect.height), GUIStyle.none, selected: null, onSelection: null, highlighted: null);
				GUI.DrawTexture(outRect, this.fullScreenTexture, ScaleMode.StretchToFill, alphaBlend: true);
				
				if (GUI.Button(rectCloseButton, string.Empty, Heatmap.styles.closeButton) == true) {
					
					this.flowEditor.SetEnabled();
					this.openFullScreen = false;
					
				}

			}

		}

		public override void OnFlowWindowScreenMenuGUI(FD.FlowWindow window, GenericMenu menu) {
			
			if (window.isVisibleState == false) return;
			if (window.IsContainer() == true) return;
			if (window.IsSmall() == true && window.IsFunction() == true) return;
			if (window.IsShowDefault() == true) return;
			
			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();
			
			var settings = Heatmap.settings;
			if (settings != null) {
				
				var data = settings.data.Get(window);
				if (data == null) return;

				foreach (var item in settings.items) {
					
					if (item.show == true && item.enabled == true) {
						
						foreach (var serviceBase in this.editor.services) {

							var service = serviceBase as IAnalyticsService;
							if (service.GetServiceName() == item.serviceName) {
								
								var key = string.Format("{0}_{1}", item.serviceName, window.id);
								var windowId = window.id;
								menu.AddItem(new GUIContent("Open Heatmap..."), false, () => {

									//this.fullScreenData = this.heatmapResultsCache[key];
									this.fullScreenTexture = this.heatmapTexturesCache[key];
									this.fullScreenWindowId = windowId;
									this.fullScreenEditor = null;

									this.openFullScreen = true;
									this.flowEditor.SetDisabled();

								});

							}

						}

					}

				}

			}

		}

		private Dictionary<string, HeatmapResult> heatmapResultsCache = new Dictionary<string, HeatmapResult>();
		private Dictionary<string, Texture2D> heatmapTexturesCache = new Dictionary<string, Texture2D>();
		public override void OnFlowWindowLayoutGUI(Rect rect, FD.FlowWindow window) {
			
			if (window.isVisibleState == false) return;
			if (window.IsContainer() == true) return;
			if (window.IsSmall() == true && window.IsFunction() == true) return;
			if (window.IsShowDefault() == true) return;

			if (Heatmap.settings == null) Heatmap.settings = Heatmap.GetSettingsFile();

			var settings = Heatmap.settings;
			if (settings != null) {
				
				var data = settings.data.Get(window);
				if (data == null) return;
				
				LayoutWindowType screen;
				var layout = HeatmapSystem.GetLayout(window.id, out screen);
				if (layout == null) return;

				var targetScreenSize = new Vector2(layout.root.editorRect.width, layout.root.editorRect.height);

				foreach (var item in settings.items) {
					
					if (item.show == true && item.enabled == true) {

						foreach (var serviceBase in this.editor.services) {
							
							var service = serviceBase as IAnalyticsService;
							if (service.GetServiceName() == item.serviceName) {

								var key = string.Format("{0}_{1}", item.serviceName, window.id);
								HeatmapResult result;
								if (this.heatmapResultsCache.TryGetValue(key, out result) == true) {

									if (result != null) {

										var texture = this.heatmapTexturesCache[key];
										if (texture != null) {
											
											var scaleFactor = HeatmapSystem.GetFactor(targetScreenSize, rect.size);
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
											GUI.color = c;
											GUI.DrawTexture(r, texture, ScaleMode.StretchToFill, alphaBlend: true);
											GUI.color = Color.white;

										} else {
											
											if (this.noDataTexture != null) GUI.DrawTexture(rect, this.noDataTexture, ScaleMode.ScaleToFit, alphaBlend: true);
											
										}

									} else {

										// still loading...

									}

								} else {

									if (Event.current.type == EventType.Repaint) {

										var rectSize = targetScreenSize;//rect.size;
										var rootRect = layout.root.editorRect;

										this.heatmapResultsCache.Add(key, null);
										this.heatmapTexturesCache.Add(key, null);
										service.GetHeatmapData(window.id, (int)targetScreenSize.x, (int)targetScreenSize.y, item.userFilter, (_result) => {

											var heatmapResult = _result as HeatmapResult;

											// Convert normalized points to real points
											for (int i = 0; i < heatmapResult.points.Length; ++i) {

												var root = layout.GetRootByTag((LayoutTag)heatmapResult.points[i].tag);
												if (root != null) {

													var xn = heatmapResult.points[i].x;
													var yn = heatmapResult.points[i].y;

													var sourceRect = root.editorRect;
													var radius = (float)HeatmapVisualizer.GetRadius();
													sourceRect.x += radius;
													sourceRect.y += radius;
													sourceRect.width -= radius * 2f;
													sourceRect.height -= radius * 2f;

													var scaleFactor = HeatmapSystem.GetFactor(targetScreenSize, rectSize);
													var r = sourceRect;
													r.x *= scaleFactor;
													r.y *= scaleFactor;
													r.x += rootRect.width * 0.5f;
													r.y = rootRect.height * 0.5f - r.y;
													r.width *= scaleFactor;
													r.height *= scaleFactor;

													heatmapResult.points[i].realPoint = new Vector2(r.x + xn * r.width, r.y - yn * r.height);

												}

											}

											this.heatmapResultsCache[key] = heatmapResult;
											HeatmapSystem.GenerateTextureFromData((int)targetScreenSize.x, (int)targetScreenSize.y, this.heatmapResultsCache[key], (texture) => { this.heatmapTexturesCache[key] = texture; });
											
										});

									}

								}

							}

						}

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

			ME.EditorUtilities.ResetCache<HeatmapSettings>(modulesPath);

			AssetDatabase.Refresh();
			
			return false;
			
		}

	}

}