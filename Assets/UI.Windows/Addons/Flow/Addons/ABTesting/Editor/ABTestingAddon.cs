using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Plugins.FlowCompiler;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEditor.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Plugins.ABTesting;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Plugins.ABTesting {
	
	public static class FlowABTestingTemplateGenerator {

		public static string GenerateTransitionMethod(FlowSystemEditorWindow flowEditor, FD.FlowWindow windowFrom, FD.FlowWindow windowTo) {
			
			var file = UnityEngine.Resources.Load("UI.Windows/ABTesting/Templates/TemplateTransitionMethod") as TextAsset;
			if (file == null) {
				
				Debug.LogError("ABTesting Template Loading Error: Could not load template 'TemplateTransitionMethod'");
				
				return string.Empty;
				
			}
			
			var data = FlowSystem.GetData();
			if (data == null) return string.Empty;
			
			if (windowTo.IsABTest() == false) {
				
				return string.Empty;
				
			}

			var result = string.Empty;
			var part = file.text;

			var methodPatternDefault = "(item, h, wh) => WindowSystemFlow.DoFlow<{0}>(this, item, h, wh, false, null)";
			var methods = string.Empty;
			var methodList = new List<string>();

			foreach (var item in windowTo.abTests.items) {

				var window = FlowSystem.GetWindow(item.attachItem.targetId);
				
				if (window.IsFunction() == true) {
					
					var winFunc = FlowSystem.GetWindow(window.functionId);
					if (winFunc != null) {

						window = FlowSystem.GetWindow(winFunc.functionRootId);

					} else {

						window = null;

					}
					
				}

				if (window == null) {
					
					methodList.Add("null");

				} else {

					var classNameWithNamespace = Tpl.GetClassNameWithNamespace(window);
					methodList.Add(string.Format(methodPatternDefault, classNameWithNamespace));

				}

			}

			methods = string.Join(", ", methodList.ToArray());

			result +=
				part.Replace("{METHOD_NAMES}", methods)
					.Replace("{FLOW_FROM_ID}", windowFrom.id.ToString())
					.Replace("{FLOW_TO_ID}", windowTo.id.ToString());
			
			return result;
			
		}

	}

	public class ABTesting : FlowAddon {
		
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
		
		private static ABTestingSettings settings;
		private ABTestingSettingsEditor editor;

		public TabsContent tabs = new TabsContent();
		public static Styles styles = null;
		public const float MARGIN = 5f;

		private ABTestingItem selectedItem;
		private bool opened = false;

		public override string GetName() {

			return "A/B Testing";

		}
		
		public override void OnGUI() {

			if (ABTesting.styles == null) ABTesting.styles = new Styles();

			if (this.opened == true) {
				
				var data = FlowSystem.GetData();
				if (data == null) return;

				const float closeSize = 50f;

				var screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
				var settingsSize = new Vector2(350f, 500f);
				var settingsRect = new Rect(screenRect.width * 0.5f - settingsSize.x * 0.5f, screenRect.height * 0.5f - settingsSize.y * 0.5f, settingsSize.x, settingsSize.y).PixelPerfect();
				var settingsBackRect = new Rect(settingsRect.x - MARGIN, settingsRect.y - MARGIN, settingsRect.width + MARGIN * 2f, settingsRect.height + MARGIN * 2f).PixelPerfect();
				var rectCloseButton = new Rect(settingsRect.x + settingsRect.width, settingsRect.y - closeSize * 0.5f, closeSize, closeSize).PixelPerfect();
				
				GUI.Box(screenRect, string.Empty, ABTesting.styles.backLock);
				GUI.Box(settingsBackRect, string.Empty, ABTesting.styles.dropShadow);
				GUI.Box(settingsBackRect, string.Empty, ABTesting.styles.contentScreen);
				GUI.Box(settingsRect, string.Empty, ABTesting.styles.layoutBack);

				GUILayout.BeginArea(settingsRect.PixelPerfect());
				this.tabs.selectedItem = this.selectedItem;
				this.tabs.OnGUI();
				GUILayout.EndArea();

				if (GUI.Button(rectCloseButton, string.Empty, ABTesting.styles.closeButton) == true) {
					
					this.flowEditor.SetEnabled();
					this.opened = false;
					
				}

			}
			
		}
		
		public override string OnCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName) {
			
			if (windowTo.IsABTest() == true && 
			    windowTo.IsSmall() == true) {
				
				return FlowABTestingTemplateGenerator.GenerateTransitionMethod(this.flowEditor, windowFrom, windowTo);
				
			}
			
			return base.OnCompilerTransitionAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName);
			
		}

		public override void OnFlowCreateMenuGUI(string prefix, GenericMenu menu) {

			if (this.InstallationNeeded() == false) {

				menu.AddSeparator(prefix);

				menu.AddItem(new GUIContent(prefix + "AB Testing/Condition"), on: false, func: () => {
					
					this.flowEditor.CreateNewItem(() => {
						
						var window = FlowSystem.CreateWindow(flags: FD.FlowWindow.Flags.IsSmall | FD.FlowWindow.Flags.CantCompiled | FD.FlowWindow.Flags.IsABTest);
						window.smallStyleDefault = "flow node 5";
						window.smallStyleSelected = "flow node 5 on";
						window.title = "A/B Test Condition";
						
						window.rect.width = 150f;
						window.rect.height = 100f;

						return window;
						
					});

				});

			}

		}

		public override bool IsCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo) {

			return windowTo.IsABTest() == true;

		}

		private void Validate(FD.FlowWindow window) {
			
			if (window.abTests == null) {

				window.abTests = new UnityEngine.UI.Windows.Plugins.ABTesting.ABTestingItems();
				EditorUtility.SetDirty(window);
				FlowSystem.SetDirty();
				FlowSystem.Save();

			}

			window.abTests.Validate(window);

			var sourceWin = FlowSystem.GetData().windowAssets.FirstOrDefault(x => x.AlreadyAttached(window.id));
			if (sourceWin != null) window.abTests.sourceWindowId = sourceWin.id;

		}

		public override Vector2 OnFlowDrawNodeCurveOffset(UnityEngine.UI.Windows.AttachItem attachItem, FD.FlowWindow window, FD.FlowWindow toWindow, bool doubleSide) {
			
			var offset = Vector2.zero;

			var data = FlowSystem.GetData();
			if (data == null) return offset;

			Vector2 dotOffset = new Vector2(5f, 0f);

			var flag = window.IsABTest();
			if (flag == true) {

				this.Validate(window);

				for (int i = 0; i < window.abTests.Count(); ++i) {

					var abTest = window.abTests.items[i];
					if (abTest.attachItem.targetId == attachItem.targetId && abTest.attachItem.index == attachItem.index) {

						var editorRect = abTest.editorRect;
						var delta = editorRect.center - window.rect.size * 0.5f + dotOffset;

						offset = delta;

					}

				}

			}

			return offset;

		}

		public override void OnFlowWindowGUI(FD.FlowWindow window) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			var flag = window.IsABTest();
			if (flag == true) {
				
				this.Validate(window);

				var windowSize = window.rect;
				var repaint = false;
				
				var connectorActive = ME.Utilities.CacheStyle("UI.Windows.ABTesting", "flow shader out 2");
				var connectorInactive = ME.Utilities.CacheStyle("UI.Windows.ABTesting", "flow shader out 5");
				var connectorOuterActive = ME.Utilities.CacheStyle("UI.Windows.ABTesting", "flow shader in 2");
				var connectorOuterInactive = ME.Utilities.CacheStyle("UI.Windows.ABTesting", "flow shader in 5");
				var editButtonStyle = ME.Utilities.CacheStyle("UI.Windows.ABTesting.EditButton", "LargeButtonRight", (name) => {
					
					var _style = new GUIStyle(name);
					_style.fixedWidth = 0f;
					_style.stretchWidth = true;
					
					return _style;
					
				});
				var addButtonStyle = ME.Utilities.CacheStyle("UI.Windows.ABTesting.AddButtonStyle", "MiniButton", (name) => {
					
					var _style = new GUIStyle(name);
					_style.fixedWidth = 0f;
					_style.stretchWidth = false;
					
					return _style;
					
				});
				var removeButtonStyle = ME.Utilities.CacheStyle("UI.Windows.ABTesting.RemoveButtonStyle", "LargeButtonLeft", (name) => {
					
					var _style = new GUIStyle(name);
					_style.fixedWidth = 0f;
					_style.stretchWidth = false;
					
					return _style;
					
				});

				var buttonHeight = editButtonStyle.fixedHeight;

				var height = 0f;

				const float topOffset = 5f;
				const float margin = 2f;
				const float marginHorizontalLeft = 2f;
				const float bottomOffset = 2f;

				Color oldColor;
				Color c;

				GUILayout.Space(topOffset);

				System.Action<int, ABTestingItem> onItem = (i, item) => {
					
					var hasAttach = (data.AlreadyAttached(window.id, item.attachItem.index, item.attachItem.targetId) == true);
					var connectorStyle = hasAttach ? connectorActive : connectorInactive;
					var connectorOuterStyle = hasAttach ? connectorOuterActive : connectorOuterInactive;
					
					var size = Mathf.Max(connectorStyle.fixedHeight, buttonHeight);
					var connectorSize = new Vector2(size, size);
					
					GUILayout.BeginHorizontal();
					{
						
						GUILayout.Space(marginHorizontalLeft);

						var canEdit = (i > 0);
						var canRemove = canEdit && (window.abTests.Count() > 2);
						
						oldColor = GUI.color;
						c = (canRemove == true) ? Color.red : Color.grey;
						GUI.color = c;
						EditorGUI.BeginDisabledGroup(!canRemove);
						{
							
							if (GUILayout.Button("X", removeButtonStyle) == true) {
								
								window.abTests.RemoveAt(i);
								EditorUtility.SetDirty(window);
								repaint = true;
								
							}
							
						}
						EditorGUI.EndDisabledGroup();
						GUI.color = oldColor;
						
						EditorGUI.BeginDisabledGroup(!canEdit);
						{
							
							if (GUILayout.Button(canEdit == true ? "Edit Condition" : "On Any Other", editButtonStyle) == true) {
								
								this.flowEditor.SetDisabled();
								this.selectedItem = window.abTests.items[i];
								this.opened = true;
								
							}
							
						}
						EditorGUI.EndDisabledGroup();
						
						GUILayout.Space(connectorSize.x);
						
					}
					GUILayout.EndHorizontal();
					
					var rect = GUILayoutUtility.GetLastRect();
					
					rect.x = window.rect.width - connectorSize.x;
					rect.y += 3f;
					rect.size = connectorSize;
					rect = rect.PixelPerfect();
					
					if (Event.current.type == EventType.Repaint) {
						
						window.abTests.items[i].editorRect = rect;
						
					}
					
					if (GUI.Button(rect, string.Empty, connectorStyle) == true) {
						
						var index = i;
						this.flowEditor.WaitForAttach(window.id, index: i + 1, onAttach: (withId, attachIndex, isAttach) => {
							
							window.abTests.Attach(index, withId, attachIndex);
							
						});
						
					}
					
					rect.x += connectorSize.x - 3f;
					rect = rect.PixelPerfect();
					if (GUI.Button(rect, string.Empty, connectorOuterStyle) == true) {
						
						var index = i;
						this.flowEditor.WaitForAttach(window.id, index: i + 1, onAttach: (withId, attachIndex, isAttach) => {
							
							window.abTests.Attach(index, withId, attachIndex);
							
						});
						
					}
					
					var comment = item.comment;
					if (string.IsNullOrEmpty(comment) == false) GUILayout.Label(comment.Trim(), EditorStyles.helpBox);
					
					GUILayout.Space(margin);

				};

				for (int i = 1; i < window.abTests.Count(); ++i) {

					onItem(i, window.abTests.items[i]);

				}

				oldColor = GUI.color;
				c = Color.green;
				GUI.color = c;
				GUILayout.BeginHorizontal();
				{

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+ Add", addButtonStyle) == true) {

						window.abTests.AddNew();
						EditorUtility.SetDirty(window);
						repaint = true;

					}
					
					GUILayout.FlexibleSpace();

				}
				GUILayout.EndHorizontal();
				GUI.color = oldColor;
				
				onItem(0, window.abTests.items[0]);

				if (Event.current.type == EventType.Repaint) {
					
					var lastRect = GUILayoutUtility.GetLastRect();
					height += lastRect.y;
					height += lastRect.height;
					height += bottomOffset;

					windowSize.height = height;

					if (windowSize != window.rect) {

						window.rect = windowSize.PixelPerfect();
						EditorUtility.SetDirty(window);

					}

				}

				if (repaint == true) {
					
					this.flowEditor.Repaint();

				}

			}

		}

		public override void OnFlowSettingsGUI() {

			if (ABTesting.settings == null) ABTesting.settings = ABTesting.GetSettingsFile();
			
			var settings = ABTesting.settings;
			if (settings == null) {
				
				EditorGUILayout.HelpBox(string.Format(FlowAddon.MODULE_HAS_ERRORS, "Settings file not found (ABTestingSettings)."), MessageType.Error);
				
			} else {
				
				GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);
				
				if (this.editor == null) {
					
					this.editor = Editor.CreateEditor(settings) as ABTestingSettingsEditor;
					
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
		
		public static ABTestingSettings GetSettingsFile() {
			
			var data = FlowSystem.GetData();
			if (data == null) return null;
			
			var modulesPath = data.GetModulesPath();
			
			var settings = ME.EditorUtilities.GetAssetsOfType<ABTestingSettings>(modulesPath, useCache: true);
			if (settings != null && settings.Length > 0) {
				
				return settings[0];
				
			}
			
			return null;
			
		}
		
		public override bool InstallationNeeded() {
			
			return ABTesting.GetSettingsFile() == null;
			
		}
		
		public override void Install() {
			
			this.Install_INTERNAL();
			
		}
		
		public override void Reinstall() {
			
			this.Install_INTERNAL();
			
		}
		
		private bool Install_INTERNAL() {

			var moduleName = "ABTesting";
			var settingsName = "ABTestingSettings";
			return this.InstallModule<ABTestingSettings>(moduleName, settingsName);

		}

	}

}