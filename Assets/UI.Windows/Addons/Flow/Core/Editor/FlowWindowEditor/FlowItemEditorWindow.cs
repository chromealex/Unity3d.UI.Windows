using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using UnityEditor.UI.Windows.Plugins.Flow.Editors;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowSceneItem {
		
		public const float POPUP_OFFSET = 50f;
		public const float POPUP_MARGIN = 5f;

		private FlowSystemEditorWindow rootWindow;
		private FlowWindow window;
		private string currentScene;
		private SceneView currentView;
		private List<WindowLayout> layouts;
		private List<WindowBase> screens;
		private bool autoloadedLayout = false;
		private bool autoloadedScreen = false;
		
		private bool _isLayoutDirty = false;
		private bool isLayoutDirty {
			
			set {
				
				this._isLayoutDirty = value;
				if (value == true) this.ValidateTabs();
				
			}
			
			get {
				
				return this._isLayoutDirty;
				
			}
			
		}
		private bool _isScreenDirty = false;
		private bool isScreenDirty {
			
			set {
				
				this._isScreenDirty = value;
				if (value == true) this.ValidateTabs();
				
			}
			
			get {
				
				return this._isScreenDirty;
				
			}
			
		}

		internal FlowSceneViewWindow view;
		internal FlowInspectorWindow inspector;
		internal FlowHierarchyWindow hierarchy;
		internal FlowGameViewWindow gameView;

		public bool isLocked = false;

		public FlowSceneItem(FlowSystemEditorWindow rootWindow, FlowWindow window, AnimatedValues.AnimFloat progressValue) {

			this.isLocked = false;

			this.rootWindow = rootWindow;
			this.window = window;
			this.currentScene = EditorApplication.currentScene;
			this.layouts = new List<WindowLayout>();
			this.layoutPrefab = null;

			this.screens = new List<WindowBase>();
			this.screenPrefab = null;

			#if UNITY_5_0
			EditorApplication.NewEmptyScene();
			#else
			EditorApplication.NewScene();
			#endif

			var popupOffset = 100f;
			var popupSize = new Vector2(rootWindow.position.width - popupOffset * 2f, rootWindow.position.height - popupOffset * 2f);
			var popupRect = new Rect(rootWindow.position.x + popupOffset, rootWindow.position.y + popupOffset, popupSize.x, popupSize.y);

			this.view = FlowSceneViewWindow.CreateInstance<FlowSceneViewWindow>();
			FlowSceneView.recompileChecker = this.view;

			this.view.title = "UI.Windows Flow Screen Editor ('" + this.window.title + "')";
			this.view.position = popupRect;
			this.view.rootWindow = rootWindow;
			
			/*this.inspector = FlowInspectorWindow.CreateInstance<FlowInspectorWindow>();
			this.inspector.position = popupRect;
			this.inspector.rootWindow = rootWindow;
			this.inspector.Repaint();
			this.inspector.Focus();
			
			this.hierarchy = FlowHierarchyWindow.CreateInstance<FlowHierarchyWindow>();
			this.hierarchy.position = popupRect;
			this.hierarchy.rootWindow = rootWindow;
			this.hierarchy.Repaint();
			this.hierarchy.Focus();*/

			this.Show();

			//this.inspector.Repaint();
			//this.inspector.Focus();
			//this.hierarchy.Repaint();
			//this.hierarchy.Focus();
			this.view.Repaint();
			this.view.Focus();

			this.autoloadedScreen = false;
			this.ReloadScreens();
			this.autoloadedLayout = false;
			this.ReloadLayouts();

			this.defaultRects = false;

			progressValue.valueChanged.AddListener(() => {
				
				this.view.DrawProgress(progressValue.value);
				//this.inspector.DrawProgress(progressValue.value);
				//this.hierarchy.DrawProgress(progressValue.value);

				if (progressValue.value == progressValue.target) {
					
					this.view.Repaint();
					this.view.Focus();

				}

			});

		}

		public void OnLostFocus() {

		}

		public void Show() {
			
			SceneView.onSceneGUIDelegate -= this.OnGUI;
			SceneView.onSceneGUIDelegate += this.OnGUI;

			if (this.inspector != null) this.inspector.ShowView();
			if (this.hierarchy != null) this.hierarchy.ShowView();
			if (this.view != null) this.view.ShowView();

		}

		public void Hide() {
			
			SceneView.onSceneGUIDelegate -= this.OnGUI;

			if (this.view != null) this.view.HideView();
			if (this.inspector != null) this.inspector.HideView();
			if (this.hierarchy != null) this.hierarchy.HideView();

		}

		public void Dispose(bool onDestroy = false, AnimatedValues.AnimFloat progressValue = null) {

			System.Action close = () => {
				
				if (this.view != null && onDestroy == false) this.view.Close();
				if (this.inspector != null && onDestroy == false) this.inspector.Close();
				if (this.hierarchy != null && onDestroy == false) this.hierarchy.Close();
				this.view = null;
				this.inspector = null;
				this.hierarchy = null;

			};

			if (progressValue != null) {

				progressValue.valueChanged.AddListener(() => {
					
					this.view.DrawProgress(progressValue.value);
					//this.inspector.DrawProgress(progressValue.value);
					//this.hierarchy.DrawProgress(progressValue.value);

					if (progressValue.value == progressValue.target) {

						close();

					}

				});

			} else {

				close();

			}

			if (string.IsNullOrEmpty(this.currentScene) == true) {

				EditorApplication.NewScene();

			} else {
				
				EditorApplication.OpenScene(this.currentScene);

			}

			SceneView.onSceneGUIDelegate -= this.OnGUI;

		}

		private Rect mainRect;
		private Rect itemRect;
		private bool defaultRects = false;

		public void OnGUI(SceneView sceneView) {

			sceneView.in2DMode = true;
			
			if (this.view != null) {

				GUILayout.BeginArea(new Rect(0f, SceneView.kToolbarHeight, this.view.position.width, this.view.position.height));

				var headerStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
				headerStyle.fontSize = 30;

				var oldColor = GUI.color;
				var color = Color.white;
				color.a = 0.6f;
				GUI.color = color;
				GUI.Label(new Rect(30f, SceneView.kToolbarHeight, this.view.position.width, this.view.position.height), this.window.title, headerStyle);
				GUI.color = oldColor;

				GUILayout.EndArea();

			}

			if (this.defaultRects == false) {

				var rect = sceneView.position;
				var size = new Vector2(250f, 100f);
				this.mainRect = new Rect(rect.width - size.x, 30f, size.x, size.y);
				
				var itemSize = new Vector2(250f, 300f);
				this.itemRect = this.mainRect;
				this.itemRect.height = itemSize.y;

				this.defaultRects = true;

			}

			this.currentView = sceneView;

			if (this.view != null) {

				if (this.mainRect.x + this.mainRect.width > this.view.position.width) this.mainRect.x = this.view.position.width - this.mainRect.width;
				if (this.mainRect.y + this.mainRect.height > this.view.position.height) this.mainRect.y = this.view.position.height - this.mainRect.height;
				if (this.mainRect.x < 0f) this.mainRect.x = 0f;
				if (this.mainRect.y < 0f) this.mainRect.y = 0f;

			}

			this.mainRect = GUILayout.Window(1, this.mainRect, (id) => {

				this.DrawGUI();
				GUI.DragWindow();
				
			}, "Window Settings", GUILayout.ExpandHeight(true));
			
			if (this.isScreenDirty == true && this.screenInstance != null) {
				
				FlowDatabase.SaveScreen(this.screenInstance);
				this.isScreenDirty = false;

			}
			
			if (this.isLayoutDirty == true && this.layoutInstance != null) {
				
				FlowDatabase.SaveLayout(this.layoutInstance);
				this.isLayoutDirty = false;
				
			}
			
			if (this.view != null) {

				var height = 80f;
				var tabsRect = this.view.position;
				tabsRect.x = 10f;
				tabsRect.y = tabsRect.height - height - 10f;
				tabsRect.width -= 10f * 2f;
				tabsRect.height = height;
				GUILayout.BeginArea(tabsRect);
				this.DrawTabs(height);
				GUILayout.EndArea();

			}

		}
		
		private UnityEngine.UI.Windows.WindowLayout layoutPrefab;
		private UnityEngine.UI.Windows.WindowLayout layoutInstance;
		
		private WindowBase screenPrefab;
		private WindowBase screenInstance;
		
		public WindowBase GetScreenInstance() {

			return this.screenInstance;

		}
		
		public WindowLayout GetLayoutInstance() {

			return this.layoutInstance;

		}
		
		public void SetScreenDirty() {
			
			EditorApplication.delayCall += () => {

				this.isScreenDirty = true;

			};

		}
		
		public void SetLayoutDirty() {

			EditorApplication.delayCall += () => {

				this.isLayoutDirty = true;

			};

		}

		public void SetTab(int index) {

			this.tab = index;

		}

		private float scaleFactor = 0f;
		private int tab = 0;
		
		private Vector2 scrollPosition;
		private Rect lastRect;

		private void DrawGUI() {

			if (this.window.compiled == false) {

				EditorGUILayout.HelpBox("To start use this function you need to generate ui layouts first.", MessageType.Warning);

			} else {

				var header = EditorStyles.whiteLargeLabel;
				
				var width = 250f - 40f;

				this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, false, true, GUILayout.Height(this.itemRect.height));
				
				GUILayout.Label(this.GetHeader(this.tab), header);
				
				this.lastRect = GUILayoutUtility.GetLastRect();

				switch (this.tab) {
					
				case 0:
					this.DrawScreenChooser(width);
					break;

				case 1:
					this.DrawLayoutChooser(width);
					break;

				case 2:
					this.DrawPreferences(width);
					break;
					
				case 3:
					this.DrawModules(width);
					break;

				case 4:
					this.DrawComponents(width);
					break;
					
				case 5:
					this.DrawPreview(width);
					break;

				}
				
				GUILayout.EndScrollView();

				if (this.layoutInstance != null && this.screenInstance != null) {

					this.layoutInstance.canvas.worldCamera = this.screenInstance.workCamera;

				}

			}

		}

		private void ValidateTabs() {

			var count = 6;
			for (int i = 0; i < count; ++i) {
				
				CompletedState state = CompletedState.NotReady;

				var errors = false;
				var warnings = false;
				var hasState = this.IsValidTab(i, out errors, out warnings);

				if (hasState == true) {

					state = (errors == false) ? (warnings == true ? CompletedState.ReadyButWarnings : CompletedState.Ready) : CompletedState.NotReady;
					
					this.window.SetCompletedState(i, state);
					
				}

			}

		}

		private void DrawTabs(float height) {

			var scaleWidth = this.view.position.width * 0.4f;
			var scaleHeight = 20f;
			var newScaleFactor = GUI.HorizontalSlider(new Rect(this.view.position.width * 0.5f - scaleWidth * 0.5f, 0f, scaleWidth, scaleHeight), this.scaleFactor, 1f, 20f);
			if (newScaleFactor != this.scaleFactor) {

				this.scaleFactor = newScaleFactor;
				
				if (this.layoutInstance != null) {
					
					var scale = this.scaleFactor;
					var size = this.screenInstance.workCamera.pixelWidth / this.layoutInstance.canvas.pixelRect.width * scale;
					this.currentView.LookAtDirect(Selection.activeTransform == null ? this.layoutInstance.transform.position : Selection.activeTransform.position, Quaternion.Euler(Vector3.up), size);
					
				}

			}

			GUILayout.Space(scaleHeight);

			var notPassedColor = Color.white;
			var activeColor = new Color(0.9f, 0.9f, 1f, 1f);
			var passedColor = Color.white;//new Color(0.9f, 1f, 0.9f, 1f);

			var left = new GUIStyle(EditorStyles.miniButtonLeft);
	        var mid = new GUIStyle(EditorStyles.miniButtonMid);
	       	var right = new GUIStyle(EditorStyles.miniButtonRight);
			var leftActive = new GUIStyle(EditorStyles.miniButtonLeft);
			leftActive.normal = leftActive.active;
			var midActive = new GUIStyle(EditorStyles.miniButtonMid);
			midActive.normal = midActive.active;
			var rightActive = new GUIStyle(EditorStyles.miniButtonRight);
			rightActive.normal = rightActive.active;
			var leftInactive = new GUIStyle(EditorStyles.miniButtonLeft);
			leftInactive.normal.textColor = Color.gray;
			var midInactive = new GUIStyle(EditorStyles.miniButtonMid);
			midInactive.normal.textColor = Color.gray;
			var rightInactive = new GUIStyle(EditorStyles.miniButtonRight);
			rightInactive.normal.textColor = Color.gray;

			var oldColor = GUI.color;

			var iconIncorrect = (new GUIStyle("WinBtnCloseMac")).normal.background;
			var iconCorrect = (new GUIStyle("WinBtnMaxMac")).normal.background;
			var iconWarning = (new GUIStyle("WinBtnMinMac")).normal.background;

			EditorGUILayout.BeginHorizontal();
			var count = 6;
			for (int i = 0; i < count; ++i) {

				var step = i + 1;
				
				var errors = false;
				var warnings = false;
				var hasState = this.IsValidTab(i, out errors, out warnings);

				var stepState = string.Empty;
				Texture icon = null;

				if (hasState == true) {

					stepState = (errors == false) ? (warnings == true ? "Completed with warnings" : "Completed") : "Has errors";
					icon = (errors == false) ? (warnings == true ? iconWarning : iconCorrect) : iconIncorrect;

				}

				var style = mid;
				var color = (this.tab == i) ? activeColor : (this.tab > i ? passedColor : notPassedColor);
				
				if (i == 0) {
					
					style = (this.tab == i) ? leftActive : (this.tab > i ? leftInactive : left);
					
				} else if (i == count - 1) {
					
					style = (this.tab == i) ? rightActive : (this.tab > i ? rightInactive : right);
					
				} else {
					
					style = (this.tab == i) ? midActive : (this.tab > i ? midInactive : mid);
					
				}

				style.fontSize = 11;
				style.fontStyle = FontStyle.Bold;

				GUI.color = color;
				if (GUILayout.Button(new GUIContent(step.ToString() + ". " + this.GetHeader(i), icon, "Step " + step.ToString() + (string.IsNullOrEmpty(stepState) ? string.Empty : (": " + stepState))), style, GUILayout.Height(height - scaleHeight)) == true) this.tab = i;
				
			}
			EditorGUILayout.EndHorizontal();

			GUI.color = oldColor;

		}

		private string GetHeader(int tabIndex) {

			var result = string.Empty;

			switch (tabIndex) {
				
			case 0:
				result = "Screen";
				break;
				
			case 1:
				result = "Layout";
				break;
				
			case 2:
				result = "Preferences";
				break;
				
			case 3:
				result = "Modules";
				break;
				
			case 4:
				result = "Components";
				break;
				
			case 5:
				result = "Summary & Preview";
				break;
				
			}

			return result;

		}

		private bool IsValidTab(int tabIndex, out bool errors, out bool warnings) {
			
			errors = false;
			warnings = false;
			var hasState = false;

			switch (tabIndex) {
				
			case 0:
				hasState = this.IsValidScreenChooser(out errors, out warnings);
				break;
				
			case 1:
				hasState = this.IsValidLayoutChooser(out errors, out warnings);
				break;
				
			case 2:
				hasState = this.IsValidPreferences(out errors, out warnings);
				break;
				
			case 3:
				hasState = this.IsValidModules(out errors, out warnings);
				break;
				
			case 4:
				hasState = this.IsValidComponents(out errors, out warnings);
				break;
				
			case 5:
				hasState = this.IsValidPreview(out errors, out warnings);
				break;
				
			}

			return hasState;

		}
		
		private bool IsValidScreenChooser(out bool errors, out bool warnings) {
			
			warnings = false;
			errors = this.screenInstance == null;

			return true;
			
		}
		
		private bool IsValidLayoutChooser(out bool errors, out bool warnings) {
			
			warnings = false;
			errors = this.screenInstance == null || this.layoutInstance == null;

			return true;
			
		}
		
		private bool IsValidPreferences(out bool errors, out bool warnings) {
			
			warnings = false;
			errors = this.screenInstance == null || this.layoutInstance == null;

			return true;
			
		}
		
		private bool IsValidModules(out bool errors, out bool warnings) {
			
			warnings = false;
			errors = this.screenInstance == null || this.layoutInstance == null;

			if (errors == true) return true;

			var modules = this.screenInstance.modules.GetModulesInfo();
			var query = from x in modules group x.moduleSource by x.moduleSource into g let count = g.Count() orderby count descending select new { Value = g.Key, Count = count };
			
			var warningsList = new List<string>();
			foreach (var element in query) {
				
				if (element.Count > 1) {
					
					warningsList.Add(element.Value.name);
					
				}
				
			}
			
			if (warningsList.Count > 0) {

				warnings = true;

			}

			return true;
			
		}
		
		private bool IsValidComponents(out bool errors, out bool warnings) {

			warnings = false;
			errors = false;

			var screen = this.screenInstance as LayoutWindowType;
			if (screen == null || screen.layout.layout == null) {

				errors = true;
				return true;

			}

			foreach (var component in screen.layout.components) {

				if (component.component == null) {

					warnings = true;
					break;

				}

			}

			return true;
			
		}
		
		private bool IsValidPreview(out bool errors, out bool warnings) {
			
			warnings = false;
			errors = false;

			return false;
			
		}

		private WindowBase previewScreen;
		private void DrawPreview(float width) {

			if (this.screenInstance == null || this.layoutInstance == null) return;

			GUILayout.Label("Screen: " + this.screenInstance.name);
			GUILayout.Label("Layout: " + this.layoutInstance.name);

			if (WindowGUIUtilities.ButtonAddon("DevicePreview", "Build Preview", "Install it to get the preview of your screen per device.") == true) {
				
				if (this.previewScreen != null) GameObject.DestroyImmediate(this.previewScreen.gameObject);
				
				this.previewScreen = WindowSystem.Show(this.screenInstance);
				this.ShowPreview();

			}

		}

		private void ShowPreview() {

			var addon = WindowUtilities.GetAddon("DevicePreview");
			if (addon != null) {

				addon.Show(() => {
					
					if (this.previewScreen != null) GameObject.DestroyImmediate(this.previewScreen.gameObject);

				});

			} else {

				Debug.LogWarning("No Addon Found: DevicePreview");

			}

			/*
			this.gameView = FlowGameViewWindow.CreateInstance<FlowGameViewWindow>();
			this.gameView.rootWindow = this.rootWindow;
			this.gameView.Focus();
			this.gameView.Repaint();
			this.gameView.ShowView(this.previewScreen, () => {
				
				if (this.previewScreen != null) GameObject.DestroyImmediate(this.previewScreen.gameObject);

			});*/

		}

		private bool showScreenWindow = false;
		private void DrawScreenChooser(float width) {
			
			var screens = new string[this.screens.Count + 1];
			screens[0] = "None";
			for (int i = 1; i < screens.Length; ++i) screens[i] = this.screens[i - 1].name.Replace("Screen", string.Empty);
			
			GUILayout.Label("Use existing screen:");
			
			var index = this.screenPrefab == null ? 0 : (System.Array.IndexOf(screens, this.screenPrefab.name.Replace("Screen", string.Empty)));
			index = EditorGUILayout.Popup(index, screens);
			if (index > 0) {
				
				this.screenPrefab = this.screens[index - 1];
				
			}
			
			this.screenPrefab = EditorGUILayout.ObjectField(this.screenPrefab, typeof(WindowBase), false) as WindowBase;
			if (GUILayout.Button("Load") == true) {
				
				this.LoadScreen(this.screenPrefab);
				
			}
			
			GUILayout.Label("Or create the new one:");
			
			if (GUILayout.Button("Create Screen...", GUILayout.Height(30f)) == true) {
				
				this.showScreenWindow = true;

			}
			
			if (Event.current.type == EventType.Repaint && this.showScreenWindow == true) {

				this.showScreenWindow = false;

				var commentStyle = new GUIStyle(EditorStyles.miniLabel);
				commentStyle.wordWrap = true;

				var rect = GUILayoutUtility.GetLastRect();
				rect.x += this.mainRect.x + this.currentView.position.x - this.scrollPosition.x + this.lastRect.x;
				rect.y += this.mainRect.y + this.currentView.position.y + rect.height - this.scrollPosition.y + this.lastRect.y + 15f;
				FlowDropDownFilterWindow.Show<FlowLayoutWindowTypeTemplate>(rect, (screen) => {

					this.screenPrefab = FlowDatabase.GenerateScreen(this.window, screen);
					this.ReloadScreens();
					this.LoadScreen(this.screenPrefab);

					this.isScreenDirty = true;
					
				}, (screen) => {
					
					GUILayout.Label(screen.comment, commentStyle);
					
				}, strongType: true);

				Event.current.Use();

			}

		}
		
		private void DrawPreferences(float width) {
			
			if (this.screenInstance != null) {

				var screen = this.screenInstance;
				if (screen != null) {
					
					EditorGUILayout.LabelField("Screen", EditorStyles.boldLabel);

					var prefs = screen.preferences;

					EditorGUIUtility.labelWidth = 50f;

					EditorGUILayout.HelpBox("By default all windows sorted by Z-Order and Camera's Depth. But you may need to show for example level loader at the top or put some window at the background (For example animated background).", MessageType.Info);
					var depth = (Preferences.Depth)EditorGUILayout.EnumPopup("Depth:", prefs.depth, GUILayout.Width(width));
					if (depth != prefs.depth) {

						this.isScreenDirty = true;
						prefs.depth = depth;

					}

					EditorGUIUtility.labelWidth = 100f;

					EditorGUILayout.HelpBox("You may need to destroy windows automatic on new scene loaded. Set this toggle off to do it. By default its on.", MessageType.Info);
					var dontDestroyOnLoad = EditorGUILayout.Toggle("Dont Destroy:", prefs.dontDestroyOnLoad, GUILayout.Width(width));
					if (dontDestroyOnLoad != prefs.dontDestroyOnLoad) {
						
						this.isScreenDirty = true;
						prefs.dontDestroyOnLoad = dontDestroyOnLoad;
						
					}

					EditorGUIUtility.LookLikeControls();

					var layoutScreen = screen as LayoutWindowType;
					if (this.layoutInstance != null && layoutScreen != null) {
						
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Layout Fit", EditorStyles.boldLabel);

						var scaleMode = (WindowLayout.ScaleMode)EditorGUILayout.EnumPopup("", layoutScreen.layout.scaleMode);
						if (scaleMode != layoutScreen.layout.scaleMode) {

							layoutScreen.layout.scaleMode = scaleMode;

							this.isScreenDirty = true;
							this.isLayoutDirty = true;

							this.layoutInstance.SetScale(scaleMode);

						}

						if (layoutScreen.layout.scaleMode == UnityEngine.UI.Windows.WindowLayout.ScaleMode.Custom) {

							EditorGUIUtility.labelWidth = 70f;
							
							if (this.layoutInstance.ValidateCanvasScaler() == true) this.isLayoutDirty = true;
							
							var scalerEditor = Editor.CreateEditor(this.layoutInstance.canvasScaler);
							scalerEditor.OnInspectorGUI();
							
							if (GUI.changed == true) this.isLayoutDirty = true;
							
							EditorGUIUtility.LookLikeControls();

						}

					}

				}

			}

		}

		private bool foldoutModules = false;
		private bool showModuleWindow;
		private void DrawModules(float width) {

			if (this.screenInstance != null) {
				
				var screen = this.screenInstance;
				if (screen != null) {

					var commentStyle = new GUIStyle(EditorStyles.miniLabel);
					commentStyle.wordWrap = true;

					var modules = screen.modules.GetModulesInfo();

					var query = from x in modules group x.moduleSource by x.moduleSource into g let count = g.Count() orderby count descending select new { Value = g.Key, Count = count };

					var warnings = new List<string>();
					foreach (var element in query) {

						if (element.Count > 1) {

							warnings.Add(element.Value.name);

						}

					}

					if (warnings.Count > 0) {

						EditorGUILayout.HelpBox("Warning! You have duplicate elements in modules list. It may cause problems in the View. Modules: " + (string.Join(", ", warnings.ToArray())), MessageType.Warning);

					}

					foreach (var moduleInfo in modules) {

						var oldColor = GUI.color;

						GUILayout.BeginVertical(GUI.skin.box);
						{

							GUILayout.BeginHorizontal();
							{
								GUILayout.Label(moduleInfo.moduleSource.name, EditorStyles.miniButtonLeft);
								GUI.color = Color.red;
								if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(30f)) == true) {

									screen.modules.RemoveModule(moduleInfo);
									this.isScreenDirty = true;
									break;

								}
								GUI.color = oldColor;
							}
							GUILayout.EndHorizontal();
								
							GUILayout.BeginHorizontal();
							{
								GUILayout.Label(moduleInfo.moduleSource.comment, commentStyle);
							}
							GUILayout.EndHorizontal();
								
							GUILayout.BeginVertical();
							{
								this.foldoutModules = EditorGUILayout.Foldout(this.foldoutModules, "Properties", EditorStyles.foldout);
								if (this.foldoutModules == true) {

									EditorGUIUtility.labelWidth = 100f;
									
									EditorGUILayout.HelpBox("Sorting Order - it's SiblibngIndex() of transform. Sorting order always must have more than zero values.", MessageType.Info);
									var sortingOrder = EditorGUILayout.IntField("Sorting Order:", moduleInfo.sortingOrder, GUILayout.Width(width));
									if (sortingOrder < 0) sortingOrder = 0;
									if (sortingOrder != moduleInfo.sortingOrder) {
										
										this.isScreenDirty = true;
										moduleInfo.sortingOrder = sortingOrder;
										
									}
									
									EditorGUIUtility.labelWidth = 140f;
									
									EditorGUILayout.HelpBox("To set SiblingIndex() lower than zero use this option.", MessageType.Info);
									var backgroundLayer = EditorGUILayout.Toggle("Is Background Layer:", moduleInfo.backgroundLayer, GUILayout.Width(width));
									if (backgroundLayer != moduleInfo.backgroundLayer) {
										
										this.isScreenDirty = true;
										moduleInfo.backgroundLayer = backgroundLayer;
										
									}

								}
							}
							GUILayout.EndHorizontal();

						}
						GUILayout.EndVertical();

					}

					if (GUILayout.Button("Add Module...", GUILayout.Height(30f)) == true) {

						this.showModuleWindow = true;

					}

					if (Event.current.type == EventType.Repaint && this.showModuleWindow == true) {

						this.showModuleWindow = false;

						var rect = GUILayoutUtility.GetLastRect();
						rect.x += this.mainRect.x + this.currentView.position.x - this.scrollPosition.x + this.lastRect.x;
						rect.y += this.mainRect.y + this.currentView.position.y + rect.height - this.scrollPosition.y + this.lastRect.y + 15f;
						FlowDropDownFilterWindow.Show<WindowModule>(rect, (module) => {
							
							screen.modules.AddModuleInfo(module, module.defaultSortingOrder, module.defaultBackgroundLayer);
							this.isScreenDirty = true;
							
						}, (module) => {

							GUILayout.Label(module.comment, commentStyle);

						}, strongType: false);
						
						Event.current.Use();

					}

				}

			}

		}

		private void DrawComponents(float width) {

			// Drawing selected screen
			if (this.screenInstance != null) {

				var screen = this.screenInstance as LayoutWindowType;
				if (screen != null) {

					var layout = this.layoutInstance;

					foreach (var component in screen.layout.components) {

						this.DrawLayoutItem(screen, layout, component, width);

					}

				}

			}

		}

		private void DrawLayoutItem(WindowBase screen, WindowLayout layout, Layout.Component component, float width) {

			if (layout == null) return;

			var tempComponent = layout.elements.FirstOrDefault((e) => e != null && e.tag == component.tag);
			var selected = (Selection.activeGameObject == tempComponent.gameObject);
			var oldColor = GUI.color;

			var boxStyle = new GUIStyle(EditorStyles.toolbar);
			boxStyle.fixedHeight = 0f;
			boxStyle.stretchHeight = true;
			boxStyle.padding.right = -20;
			boxStyle.margin.right = -20;

			var titleStyle = EditorStyles.whiteMiniLabel;

			EditorGUILayout.Separator();

			GUI.color = selected == true ? new Color(0.7f, 1f, 0.7f, 1f) : Color.white;
			EditorGUILayout.BeginVertical(boxStyle, GUILayout.Width(width));

			if (GUILayout.Button(component.tag.ToString() + " (" + component.description + ")", titleStyle, GUILayout.Width(width)) == true) {

				Selection.activeGameObject = tempComponent.gameObject;

			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Order:", GUILayout.Width(50f));
			var newOrder = EditorGUILayout.IntField(component.sortingOrder, EditorStyles.miniTextField, GUILayout.Width(50f));
			if (newOrder != component.sortingOrder) {

				component.sortingOrder = newOrder;
				this.isScreenDirty = true;

			}

			if (component.sortingOrder == 0) {

				GUILayout.Label("(Auto)", EditorStyles.miniLabel);

			} else {

				if (GUILayout.Button("Set Auto", EditorStyles.miniButton) == true) {

					component.sortingOrder = 0;
					this.isScreenDirty = true;

				}

			}

			EditorGUILayout.EndHorizontal();

			var newComponent = EditorGUILayout.ObjectField(component.component, typeof(WindowComponent), false, GUILayout.Width(width)) as WindowComponent;
			if (newComponent != component.component) {
				
				component.component = newComponent;
				this.isScreenDirty = true;
				
			}

			tempComponent.tempEditorComponent = component.component;

			EditorGUILayout.EndVertical();
			var rect = GUILayoutUtility.GetLastRect();
			if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition) == true) {

				Selection.activeGameObject = tempComponent.gameObject;

			}

			GUI.color = oldColor;

		}

		private bool showLayoutWindow = false;
		private void DrawLayoutChooser(float width) {

			if (this.screenInstance == null) return;

			var layouts = new string[this.layouts.Count + 1];
			layouts[0] = "None";
			for (int i = 1; i < layouts.Length; ++i) layouts[i] = this.layouts[i - 1].name.Replace("Layout", string.Empty);

			GUILayout.Label("Use existing layout:");

			var index = this.layoutPrefab == null ? 0 : (System.Array.IndexOf(layouts, this.layoutPrefab.name.Replace("Layout", string.Empty)));
			index = EditorGUILayout.Popup(index, layouts);
			if (index > 0) {

				this.layoutPrefab = this.layouts[index - 1];

			}

			this.layoutPrefab = EditorGUILayout.ObjectField(this.layoutPrefab, typeof(WindowLayout), false) as WindowLayout;
			if (GUILayout.Button("Load") == true) {

				this.LoadLayout(this.layoutPrefab);

			}

			GUILayout.Label("Or create the new one:");

			if (GUILayout.Button("Create Layout...", GUILayout.Height(30f)) == true) {
				
				this.showLayoutWindow = true;
				
			}

			if (Event.current.type == EventType.Repaint && this.showLayoutWindow == true) {
				
				this.showLayoutWindow = false;
				
				var commentStyle = new GUIStyle(EditorStyles.miniLabel);
				commentStyle.wordWrap = true;

				FlowChooserFilterWindow.Show<FlowWindowLayoutTemplate>(this.rootWindow, (layout) => {

					this.layoutPrefab = FlowDatabase.GenerateLayout(this.window, layout);
					this.ReloadLayouts();
					this.LoadLayout(this.layoutPrefab);

					this.isScreenDirty = true;
					
				}, (layout) => {
					
					GUILayout.Label(layout.comment, commentStyle);
					
				});
				
			}

		}
		
		private void LoadLayout(WindowLayout layoutPrefab) {
			
			if (layoutPrefab != null) {
				
				if (this.layoutInstance != null) GameObject.DestroyImmediate(this.layoutInstance.gameObject);
				
				// Loading layout
				this.layoutInstance = FlowDatabase.LoadLayout(layoutPrefab);
				this.layoutInstance.transform.position = Vector3.zero;
				this.layoutInstance.transform.rotation = Quaternion.identity;
				this.layoutInstance.transform.localScale = Vector3.zero;
				
				if ((this.screenInstance as LayoutWindowType) != null) {
					
					var layoutScreen = this.screenInstance as LayoutWindowType;
					layoutScreen.layout.layout = layoutPrefab;
					this.isScreenDirty = true;

				}

			}
			
		}
		
		private void ReloadLayouts() {

			this.layouts.Clear();
			
			var guids = AssetDatabase.FindAssets("t:GameObject", new string[] { window.compiledDirectory.Trim('/') + "/" + FlowDatabase.LAYOUT_FOLDER });
			foreach (var guid in guids) {
				
				var layout = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(WindowLayout)) as WindowLayout;
				if (layout != null) this.layouts.Add(layout);
				
			}

			if (this.layouts.Count == 1 && this.autoloadedLayout == false) {
				
				this.layoutPrefab = this.layouts[0];
				this.LoadLayout(this.layoutPrefab);
				this.autoloadedLayout = true;
				
			}
			
		}
		
		private void LoadScreen(WindowBase screenPrefab) {
			
			if (screenPrefab != null) {
				
				if (this.screenInstance != null) GameObject.DestroyImmediate(this.screenInstance.gameObject);
				
				// Loading screen
				this.screenInstance = FlowDatabase.LoadScreen(screenPrefab);
				this.screenInstance.transform.position = Vector3.zero;
				this.screenInstance.transform.rotation = Quaternion.identity;
				this.screenInstance.transform.localScale = Vector3.zero;

				this.window.SetScreen(screenPrefab);

			}
			
		}
		
		private void ReloadScreens() {
			
			this.screens.Clear();

			var guids = AssetDatabase.FindAssets("t:GameObject", new string[] { window.compiledDirectory.Trim('/') + "/" + FlowDatabase.SCREENS_FOLDER });
			foreach (var guid in guids) {

				var screen = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(WindowBase)) as WindowBase;
				if (screen != null) this.screens.Add(screen);

			}

			if (this.screens.Count == 1 && this.autoloadedScreen == false) {
				
				this.screenPrefab = this.screens[0];
				this.LoadScreen(this.screenPrefab);
				this.autoloadedScreen = true;
				
			}
			
		}

	}

	public class FlowSceneView {
		
		public static FlowSceneViewWindow recompileChecker;

		private static FlowSceneItem currentItem;
		private static bool isActive = false;
		
		public static FlowSceneItem GetItem() {
			
			return FlowSceneView.currentItem;
			
		}

		public static FlowSceneViewWindow GetView() {
			
			return FlowSceneView.currentItem != null ? FlowSceneView.currentItem.view : null;
			
		}
		
		public static FlowInspectorWindow GetInspector() {
			
			return FlowSceneView.currentItem != null ? FlowSceneView.currentItem.inspector : null;
			
		}
		
		public static FlowHierarchyWindow GetHierarchy() {
			
			return FlowSceneView.currentItem != null ? FlowSceneView.currentItem.hierarchy : null;
			
		}

		public static bool IsActive() {

			return FlowSceneView.isActive;

		}
		
		private static AnimatedValues.AnimFloat editAnimation;
		public static void SetControl(FlowSystemEditorWindow rootWindow, FlowWindow window, System.Action<float> onProgress) {

			if (EditorApplication.SaveCurrentSceneIfUserWantsTo() == true) {
				
				FlowSceneView.editAnimation = new UnityEditor.AnimatedValues.AnimFloat(0f, () => {

					onProgress(FlowSceneView.editAnimation.value);

				});
				FlowSceneView.editAnimation.value = 0f;
				FlowSceneView.editAnimation.speed = 2f;
				FlowSceneView.editAnimation.target = 1f;

				FlowSceneView.currentItem = new FlowSceneItem(rootWindow, window, FlowSceneView.editAnimation);
				FlowSceneView.isActive = true;

			}

		}

		public static void Reset(System.Action<float> onProgress, bool onDestroy = false) {
			
			FlowSceneView.editAnimation = new UnityEditor.AnimatedValues.AnimFloat(1f, () => {
				
				onProgress(FlowSceneView.editAnimation.value);
				
			});
			FlowSceneView.editAnimation.value = 1f;
			FlowSceneView.editAnimation.speed = 2f;
			FlowSceneView.editAnimation.target = 0f;

			if (FlowSceneView.currentItem != null) FlowSceneView.currentItem.Dispose(onDestroy, FlowSceneView.editAnimation);
			FlowSceneView.currentItem = null;
			
			FlowSceneView.isActive = false;
			
		}
		
		public static void Show() {
			
			if (FlowSceneView.currentItem != null) FlowSceneView.currentItem.Show();
			
		}
		
		
		public static void Hide() {
			
			if (FlowSceneView.currentItem != null) FlowSceneView.currentItem.Hide();
			
		}

	}

}