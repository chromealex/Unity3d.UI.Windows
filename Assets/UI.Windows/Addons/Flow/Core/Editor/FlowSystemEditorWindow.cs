using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using UnityEditorInternal;
using ME;
using System.Reflection;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowSystemEditor {
		
		public static Rect GetCenterRect(EditorWindow editorWindow, float width, float height) {
			
			var size = editorWindow.position;
			
			return new Rect(size.width * 0.5f - width * 0.5f, size.height * 0.5f - height * 0.5f, width, height);
			
		}
		
		public static Rect GetCenterRect(Rect rect, float width, float height) {
			
			var size = rect;
			
			return new Rect(size.width * 0.5f - width * 0.5f, size.height * 0.5f - height * 0.5f, width, height);
			
		}

		public static Rect Scale(Rect realRect, Rect minMaxRect, Rect toPosition, Vector2 offset) {

			var width = minMaxRect.width;
			var height = minMaxRect.height;

			return new Rect(realRect.x / width * toPosition.width + offset.x, realRect.y / height * toPosition.height + offset.y, realRect.width / width * toPosition.width, realRect.height / height * toPosition.height);

		}

	}

	public class FlowSystemEditorWindow : EditorWindowExt {
		
		public static GUISkin defaultSkin;

		public static FlowSystemEditorWindow ShowEditor(System.Action onClose) {

			var editor = FlowSystemEditorWindow.GetWindow<FlowSystemEditorWindow>(typeof(SceneView));
			editor.title = "UI.Windows: Flow";
			editor.autoRepaintOnSceneChange = true;
			editor.onClose = onClose;

			var fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
			var isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
			if ((bool)isDockedMethod.Invoke(editor, null) == false) {

				var width = 800f;
				var height = 600f;

				var rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
				if (rect.x < 120f) rect.x = 120f;
				if (rect.y < 120f) rect.y = 120f;

				editor.position = rect;
				editor.minSize = new Vector2(width, height);

			}

			editor.ChangeFlowData();

			return editor;

		}

		private System.Action onClose;

		private Rect scrollRect;
		private Rect contentRect;
		
		private const float scrollSize = 18f;

		public override void OnActive() {

			FlowSceneView.Show();

		}
		
		public override void OnInactive() {

			FlowSceneView.Hide();

		}

		public override void OnClose() {

			FlowSceneView.Reset(this.OnItemProgress);

			if (this.onClose != null) this.onClose();

		}

		private int focusedGUIWindow {
			
			get;
			set;

		}

		private const float SETTINGS_WIDTH = 250f;
		private const float TOOLBAR_HEIGHT = 18f;

		private Texture2D _background;
		private List<int> tempAttaches = new List<int>();
		private void OnGUI() {
			
			WindowUtilities.LoadAddons();

			//var draw = !FlowSceneView.IsActive();

			//if (draw == true) {
			
			if (FlowSystemEditorWindow.defaultSkin == null) FlowSystemEditorWindow.defaultSkin = Resources.Load("UI.Windows/Flow/Styles/Skin" + (EditorGUIUtility.isProSkin == true ? "Dark" : "Light")) as GUISkin;

			this.contentRect = this.position;
			this.contentRect.x = 0f;
			this.contentRect.y = 0f;
			this.contentRect.width = 10000f;
			this.contentRect.height = 10000f;
			this.contentRect.height -= scrollSize;

			GUI.enabled = true;//!FlowSceneView.IsActive();
			
			var hasData = FlowSystem.HasData();

			if (hasData == false) {

				this.BeginWindows();
				
				this.DrawDataSelection();
				
				this.EndWindows();

			} else {

				var oldEnabled = GUI.enabled;
				GUI.enabled = FlowSystem.HasData() && GUI.enabled;
				this.DrawToolbar();
				GUI.enabled = oldEnabled;
				
				this.DrawSettings(TOOLBAR_HEIGHT);

				IEnumerable<FlowWindow> windows = null;
				IEnumerable<FlowWindow> containers = null;
				if (hasData == true) {
					
					windows = FlowSystem.GetWindows();
					containers = FlowSystem.GetContainers();

				}

				this.scrollRect = this.position;
				this.scrollRect.x = SETTINGS_WIDTH;
				this.scrollRect.y = TOOLBAR_HEIGHT;
				this.scrollRect.width -= SETTINGS_WIDTH;
				this.scrollRect.height -= TOOLBAR_HEIGHT;

				var scrollPos = FlowSystem.GetScrollPosition();
				if (scrollPos == -Vector2.one) scrollPos = new Vector2(this.contentRect.width * 0.5f - this.scrollRect.width * 0.5f, this.contentRect.height * 0.5f - this.scrollRect.height * 0.5f);
				FlowSystem.SetScrollPosition(GUI.BeginScrollView(this.scrollRect, scrollPos, this.contentRect));

				this.DrawBackground();

				if (hasData == true && windows != null) {
					
					this.tempAttaches.Clear();
					foreach (var window in windows) {
						
						var attaches = window.attaches;
						foreach (var attachId in attaches) {
							
							if (FlowSystem.GetWindow(attachId).isContainer == true) continue;
							
							var doubleSided = FlowSystem.AlreadyAttached(attachId, window.id);
							if (this.tempAttaches.Contains(attachId) == true && doubleSided == true) continue;
							
							this.DrawNodeCurve(window, FlowSystem.GetWindow(attachId), doubleSided);
							
						}
						
						this.tempAttaches.Add(window.id);
						
					}

					var oldColor = GUI.backgroundColor;

					this.bringFront.Clear();
					
					var selectionMain = -1;
					var selected = FlowSystem.GetSelected();

					this.BeginWindows();

					var containerPadding = new Vector4(50f, 100f, 50f, 50f);
					foreach (var container in containers) {
						
						var backColor = container.randomColor;
						backColor.a = 0.3f;
						GUI.backgroundColor = backColor;

						var rootContainer = container.GetContainer();
						if (rootContainer != null) {

							// If container has other container

						}

						var attaches = container.attaches;
						if (attaches.Count == 0) {

							container.rect.width = 200f;
							container.rect.height = 200f;

						} else {
							
							var minX = float.MaxValue;
							var minY = float.MaxValue;
							var maxX = float.MinValue;
							var maxY = float.MinValue;
							foreach (var attachId in attaches) {
								
								var window = FlowSystem.GetWindow(attachId);

								minX = Mathf.Min(minX, window.rect.xMin);
								minY = Mathf.Min(minY, window.rect.yMin);
								maxX = Mathf.Max(maxX, window.rect.xMax);
								maxY = Mathf.Max(maxY, window.rect.yMax);

							}
							
							container.rect.xMin = minX - containerPadding.x;
							container.rect.yMin = minY - containerPadding.y;
							container.rect.xMax = maxX + containerPadding.z;
							container.rect.yMax = maxY + containerPadding.w;

						}

						var style = container.GetEditorStyle(false);

						var rect = GUI.Window(container.id, container.rect, this.DrawNodeContainer, container.title, style);
						this.BringBackOrFront(container, containers);

						if (selectionMain == -1 || selectionMain == container.id) {

							var isMoving = (rect != container.rect);
							var newRect = FlowSystem.Grid(rect);
							if (newRect != container.rect && isMoving == true) {

								if (selected.Count > 0 && selected.Contains(container.id) == false) {

									// nothing to do

								} else {

									var delta = new Vector2(newRect.x - container.rect.x, newRect.y - container.rect.y);
									if (delta != Vector2.zero) {

										selected.Clear();
										if (selected.Contains(container.id) == false) selected.Add(container.id);
										
										container.rect = newRect;

										FlowSystem.MoveContainerOrWindow(container.id, delta);

										selectionMain = container.id;

									}

								}

							}

						}

					}
					GUI.backgroundColor = oldColor;

					foreach (var window in windows) {

						var title = string.Empty;
						if (window.isDefaultLink == true) {

							window.rect.width = 150f;
							window.rect.height = 30f;

							title = "Default Link";

						} else {

							window.rect.width = 250f;
							window.rect.height = 80f;

						}

						var isSelected = selected.Contains(window.id) || (selected.Count == 0 && this.focusedGUIWindow == window.id);
						var style = window.GetEditorStyle(isSelected);

						var rect = GUI.Window(window.id, window.rect, this.DrawNodeWindow, title, style/*, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)*/);
						GUI.BringWindowToFront(window.id);

						var isMoving = (rect != window.rect);

						if (selectionMain == -1 || selectionMain == window.id) {

							var newRect = FlowSystem.Grid(rect);
							if (newRect != window.rect && isMoving == true) {

								// If selected contains
								if (selected.Count > 0 && selected.Contains(window.id) == false) {

									// nothing to do

								} else {
									
									var delta = new Vector2(newRect.x - window.rect.x, newRect.y - window.rect.y);
									if (delta != Vector2.zero) {

										window.rect = newRect;

										// Move all selected windows
										foreach (var selectedId in selected) {

											if (selectedId != window.id) {

												FlowSystem.GetWindow(selectedId).Move(delta);

											}

										}

										selectionMain = window.id;

									}

								}

							}

						}

					}

					this.EndWindows();

					var defaultColor = GUI.color;
					//var selectedColor = new Color(0.8f, 0.8f, 1f, 1f);

					if (selectionMain >= 0 && FlowSystem.GetWindow(selectionMain).isContainer == true) FlowSystem.ResetSelection();

					GUI.color = defaultColor;

					FlowSystem.Save();

				}

				if (this.scrollingMouseAnimation != null && this.scrollingMouseAnimation.isAnimating == true || this.scrollingMouse == true) this.DrawMinimap();

				GUI.EndScrollView();
				
				this.DrawTagsPopup();

				this.DragBackground(TOOLBAR_HEIGHT);

				GUI.enabled = true;

			}

		}
		
		public void OnLostFocus() {

			//FlowSceneView.Reset();
			
		}
		
		private void OnFocus() {

			//FlowSceneView.Reset();
			
		}

		private Dictionary<int, List<FlowWindow>> bringFront = new Dictionary<int, List<FlowWindow>>();

		private void BringBackOrFront(FlowWindow current, IEnumerable<FlowWindow> windows) {

			// Is any of other window has bigger size and collide current
			foreach (var window in windows) {

				if (window != current) {

					var p1 = window.rect.width * window.rect.height;
					var p2 = current.rect.width * current.rect.height;

					if (p1 > p2 && window.rect.Overlaps(current.rect) == true) {

						// Bring window to front
						GUI.BringWindowToFront(current.id);

						if (this.bringFront.ContainsKey(current.id) == true) {

							foreach (var item in this.bringFront[current.id]) {

								GUI.BringWindowToFront(item.id);

							}

						}

						if (this.bringFront.ContainsKey(window.id) == true) {

							this.bringFront[window.id].Add(current);

						} else {

							this.bringFront.Add(window.id, new List<FlowWindow>() { current });

						}

					}

				}

			}

		}

		private Vector2 settingsWindowScroll;
		private ReorderableList defaultWindows;
		private ReorderableList tagsList;
		private void DrawSettings(float offsetY) {

			//var scrollPos = FlowSystem.GetScrollPosition();

			//var wRect = new Rect(10f + scrollPos.x, 20f + scrollPos.y, 200f, 200f);
			//GUI.Window(-1, wRect, (id) => {
				
			if (FlowSystem.HasData() == false) return;

			var boxStyle = new GUIStyle("AnimationCurveEditorBackground");
			boxStyle.padding = new RectOffset(10, 10, 10, 10);

			GUILayout.BeginArea(new Rect(0f, offsetY, SETTINGS_WIDTH, this.position.height - offsetY), boxStyle);
			{
				
				//var buttonStyle = new GUIStyle(EditorStyles.miniButton);
				//this.DrawToolbar(buttonStyle);

				this.settingsWindowScroll = GUILayout.BeginScrollView(this.settingsWindowScroll, false, false);
				
				CustomGUI.Splitter();
				GUILayout.Label("Base Modules:", EditorStyles.whiteLargeLabel);
				CustomGUI.Splitter();

				#region ROOT WINDOW
				GUILayout.Label("Root Window", EditorStyles.boldLabel);

				var rootWindow = FlowSystem.GetWindow(FlowSystem.GetRootWindow());
				if (rootWindow != null) {

					if (GUILayout.Button(rootWindow.title) == true) {

						this.focusedGUIWindow = rootWindow.id;
						FlowSystem.ResetSelection();

					}

				} else {

					GUILayout.Label("No root window selected.");

				}

				#endregion
				
				#region DEFAULT WINDOWS
				GUILayout.Label("Default Windows", EditorStyles.boldLabel);
				if (this.defaultWindows == null) {
					
					var label = "Default Windows";
					
					this.defaultWindows = new ReorderableList(FlowSystem.GetDefaultWindows(), typeof(int), true, true, false, true);

					this.defaultWindows.drawHeaderCallback += rect => GUI.Label(rect, label);
					this.defaultWindows.drawElementCallback += (rect, index, active, focused) => {
						
						GUI.Label(rect, FlowSystem.GetWindow(FlowSystem.GetDefaultWindows()[index]).title);
						
					};
					
				}
				
				if (this.defaultWindows != null) this.defaultWindows.DoLayoutList();
				#endregion
				
				#region TAGS
				GUILayout.Label("Tags", EditorStyles.boldLabel);
				if (this.tagsList == null) {
					
					var label = "Tags";

					var styles = new GUIStyle[7] {
						
						new GUIStyle("sv_label_1"),
						new GUIStyle("sv_label_2"),
						new GUIStyle("sv_label_3"),
						new GUIStyle("sv_label_4"),
						new GUIStyle("sv_label_5"),
						new GUIStyle("sv_label_6"),
						new GUIStyle("sv_label_7")

					};

					var selected = new GUIStyle("U2D.pivotDotActive");

					var buttonStyle = new GUIStyle(EditorStyles.miniButton);
					buttonStyle.normal.background = buttonStyle.active.background;
					buttonStyle.active.background = null;
					buttonStyle.focused.background = null;
					buttonStyle.wordWrap = false;

					var tagsSource = FlowSystem.GetTags();
					if (tagsSource != null) {

						this.tagsList = new ReorderableList(tagsSource, typeof(FlowTag), true, true, false, true);

						this.tagsList.drawHeaderCallback += rect => GUI.Label(rect, label);
						this.tagsList.drawElementCallback += (rect, index, active, focused) => {

							var tags = FlowSystem.GetTags();
							if (index < 0 || index >= tags.Count) return;

							var tag = tags[index];

							var itemRect = new Rect(rect);
							itemRect.x += itemRect.width;
							itemRect.width = 14f;

							var i = 0;
							foreach (var style in styles) {

								itemRect.x -= itemRect.width;
								if (GUI.Button(itemRect, " ", style) == true) {

									tag.color = i;

								}

								if (tag.color == i) GUI.Label(new Rect(itemRect.x - 2f, itemRect.y - 2f, itemRect.width, itemRect.height), string.Empty, selected);

								++i;

							}

							rect.width = itemRect.x - rect.x - 5f;
							rect.height -= 2f;
							var title = GUI.TextField(rect, tag.title, buttonStyle);
							if (title != tag.title) {
								
								tag.title = title;
								FlowSystem.SetDirty();
								
							}

						};
						
					}

				}
				
				if (this.tagsList != null) this.tagsList.DoLayoutList();
				#endregion

				#region WINDOW EDITOR
				
				GUILayout.Label("Window Editor", EditorStyles.boldLabel);
				/*
				var editorWindowAsPopup = GUILayout.Toggle(FlowSystem.GetData().editorWindowAsPopup, "Show as popup");
				if (editorWindowAsPopup != FlowSystem.GetData().editorWindowAsPopup) {

					FlowSystem.GetData().editorWindowAsPopup = editorWindowAsPopup;
					FlowSystem.SetDirty();

				}
				*/
				#endregion

				CustomGUI.Splitter();
				GUILayout.Label("Installed Modules:", EditorStyles.whiteLargeLabel);
				CustomGUI.Splitter();

				Flow.OnDrawSettingsGUI();

				GUILayout.EndScrollView();

			}
			GUILayout.EndArea();

			//}, "Settings");

			//GUI.BringWindowToFront(-1);

		}

		private bool scrollingMouse = false;
		private bool selectionRectWait;
		private Rect selectionRect;
		private void DragBackground(float offset) {

			var button = Event.current.button;
			var position = Event.current.mousePosition + FlowSystem.GetScrollPosition() + new Vector2(-SETTINGS_WIDTH, -offset);
			
			if (Event.current.type == EventType.MouseDown && button == 2) {

				this.scrollingMouse = true;
				
				this.scrollingMouseAnimation = new UnityEditor.AnimatedValues.AnimFloat(0f, () => {
					
					this.Repaint();
					
				});
				this.scrollingMouseAnimation.speed = 2f;
				this.scrollingMouseAnimation.target = 1f;
				
			}
			
			if (Event.current.type == EventType.MouseDrag && this.scrollingMouse == true) {

				this.Repaint();
				
			}
			
			if (Event.current.type == EventType.MouseUp && this.scrollingMouse == true) {

				this.scrollingMouse = false;
				
				this.scrollingMouseAnimation.value = 1f;
				this.scrollingMouseAnimation.speed = 2f;
				this.scrollingMouseAnimation.target = 0f;
				
				this.Repaint();
				
			}

			if (Event.current.type == EventType.MouseDown && button == 0) {
				
				this.selectionRect = new Rect(position.x, position.y, 0f, 0f);
				this.selectionRectWait = true;

				this.selectionRectAnimation = new UnityEditor.AnimatedValues.AnimFloat(0f, () => {
					
					this.Repaint();

				});
				this.selectionRectAnimation.speed = 2f;
				this.selectionRectAnimation.target = 1f;

			}

			if (Event.current.type == EventType.MouseDrag && this.selectionRectWait == true) {

				var p1x = this.selectionRect.x;
				var p1y = this.selectionRect.y;
				var p2x = position.x;
				var p2y = position.y;

				this.selectionRect.width = p2x - p1x;
				this.selectionRect.height = p2y - p1y;

				FlowSystem.SelectWindowsInRect(this.selectionRect);

				this.Repaint();

			}

			if (Event.current.type == EventType.MouseUp && this.selectionRectWait == true) {
				
				// Select in rect
				FlowSystem.SelectWindowsInRect(this.selectionRect);

				this.selectionRectWait = false;
				
				this.selectionRectAnimation.value = 1f;
				this.selectionRectAnimation.speed = 2f;
				this.selectionRectAnimation.target = 0f;

				this.Repaint();

			}

			if (Event.current.type == EventType.MouseDrag && Event.current.button == 2) {

				var scrollPos = FlowSystem.GetScrollPosition();
				scrollPos -= Event.current.delta;
				FlowSystem.SetScrollPosition(scrollPos);

				Event.current.Use();

			}

		}

		private AnimatedValues.AnimFloat selectionRectAnimation;
		private void DrawBackground() {
			
			if (this._background == null) this._background = Resources.Load("UI.Windows/Flow/Background") as Texture2D;
			
			FlowSystem.grid = new Vector2(this._background.width / 20f, this._background.height / 20f);

			var oldColor = GUI.color;

			var color = new Color(1f, 1f, 1f, 0.2f);
			GUI.color = color;

			var size = new Vector2(this._background.width, this._background.height);
			var drawSize = new Vector2(this.contentRect.width, this.contentRect.height);

			GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, drawSize.x, drawSize.y), this._background, new Rect(0f, 0f, drawSize.x / size.x, drawSize.y / size.y), true);

			if (this.selectionRect.size != Vector2.zero && (this.selectionRectAnimation.isAnimating == true || this.selectionRectWait == true)) {

				var normalRect = this.selectionRect;

				if (normalRect.width < 0f) {
					
					normalRect.x += normalRect.width;
					normalRect.width = -normalRect.width;
					
				}

				if (normalRect.height < 0f) {
					
					normalRect.y += normalRect.height;
					normalRect.height = -normalRect.height;
					
				}

				var selectionBoxStyle = new GUIStyle("SelectionRect");
				selectionBoxStyle.margin = new RectOffset();
				selectionBoxStyle.padding = new RectOffset();
				selectionBoxStyle.contentOffset = Vector2.zero;

				color = new Color(1f, 1f, 1f, this.selectionRectAnimation.value);

				GUI.color = color;
				GUI.Box(normalRect, string.Empty, selectionBoxStyle);
				
			}

			GUI.color = oldColor;

		}
		
		private AnimatedValues.AnimFloat scrollingMouseAnimation;
		private void DrawMinimap() {
			
			var oldColor = GUI.color;

			var elementStyle = new GUIStyle(GUI.skin.FindStyle("button"));

			var minMax = Rect.MinMaxRect(10000f, 10000f, -10000f, -10000f);
			var backOffset = 300f;

			var factor = this.scrollingMouseAnimation.value;

			var backAlpha = 0.3f * factor;
			var cameraAlpha = 0.5f * factor;
			var elementAlpha = 0.6f * factor;

			var windows = FlowSystem.GetWindows();
			var containers = FlowSystem.GetContainers();
			
			if (containers != null) {
				
				foreach (var container in containers) {
					
					minMax.xMin = Mathf.Min(container.rect.xMin, minMax.xMin);
					minMax.yMin = Mathf.Min(container.rect.yMin, minMax.yMin);
					minMax.xMax = Mathf.Max(container.rect.xMax, minMax.xMax);
					minMax.yMax = Mathf.Max(container.rect.yMax, minMax.yMax);
					
				}
				
			}
			
			if (windows != null) {
				
				foreach (var window in windows) {
					
					minMax.xMin = Mathf.Min(window.rect.xMin, minMax.xMin);
					minMax.yMin = Mathf.Min(window.rect.yMin, minMax.yMin);
					minMax.xMax = Mathf.Max(window.rect.xMax, minMax.xMax);
					minMax.yMax = Mathf.Max(window.rect.yMax, minMax.yMax);
					
				}
				
			}
			
			minMax.xMin -= backOffset;
			minMax.yMin -= backOffset;
			minMax.xMax += backOffset;
			minMax.yMax += backOffset;

			var color = Color.black;
			color.a = backAlpha;
			GUI.color = color;
			GUI.Box(FlowSystemEditor.Scale(minMax, new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, FlowSystem.GetScrollPosition()), string.Empty, elementStyle);

			if (containers != null) {
				
				foreach (var container in containers) {
					
					color = container.randomColor;
					color.a = elementAlpha;
					GUI.color = color;
					GUI.Box(FlowSystemEditor.Scale(container.rect, new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, FlowSystem.GetScrollPosition()), string.Empty, elementStyle);
					
				}
				
			}
			
			color = Color.white;
			color.a = elementAlpha;
			GUI.color = color;

			if (windows != null) {
				
				foreach (var window in windows) {

					var rect = window.rect;
					if (rect.height < 60f) rect.height = 60f;
					GUI.Box(FlowSystemEditor.Scale(rect, new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, FlowSystem.GetScrollPosition()), string.Empty, elementStyle);
					
				}
				
			}
			
			color = Color.white;
			color.a = cameraAlpha;
			GUI.color = color;

			var scrollPos = FlowSystem.GetScrollPosition();
			GUI.Box(FlowSystemEditor.Scale(new Rect(scrollPos.x, scrollPos.y, this.scrollRect.width, this.scrollRect.height), new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, FlowSystem.GetScrollPosition()), string.Empty, elementStyle);
			
			GUI.color = oldColor;

		}

		private Vector2 dataSelectionScroll;
		private Texture splash;
		private FlowData cachedData;
		private FlowData[] scannedData;
		private void DrawDataSelection() {

			this.DrawBackground();

			if (this.splash == null) this.splash = Resources.Load("UI.Windows/Flow/Splash") as Texture;
			
			var darkLabel = FlowSystemEditorWindow.defaultSkin.FindStyle("DarkLabel");

			var rect = FlowSystemEditor.GetCenterRect(this.position, this.splash.width, this.splash.height);

			var boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.margin = new RectOffset(0, 0, 0, 0);
			boxStyle.padding = new RectOffset(0, 0, 0, 0);
			boxStyle.normal.background = null;
			GUI.Box(rect, this.splash, boxStyle);

			var width = 730f;
			var height = 456f;
			var rectOffset = FlowSystemEditor.GetCenterRect(this.position, width, height);

			var marginLeft = 240f;
			var margin = 20f;

			var padding = 20f;

			GUILayout.BeginArea(rectOffset);
			{

				var borderWidth = width - marginLeft - margin;
				var borderHeight = height - margin * 2f;

				GUILayout.BeginArea(new Rect(marginLeft, margin, borderWidth, borderHeight), new GUIStyle("sv_iconselector_labelselection"));
				{

					GUILayout.BeginArea(new Rect(padding, padding, borderWidth - padding * 2f, borderHeight - padding * 2f));
					{

						var headerStyle = new GUIStyle("LODLevelNotifyText");
						headerStyle.fontSize = 18;
						headerStyle.alignment = TextAnchor.MiddleCenter;

						GUILayoutExt.LabelWithShadow("UI.Windows Flow Extension v" + VersionInfo.bundleVersion, headerStyle);
						
						GUILayout.Space(10f);

						GUILayout.Label("Open one of your projects:", darkLabel);

						var backStyle = new GUIStyle("sv_iconselector_labelselection");

						var skin = GUI.skin;
						GUI.skin = FlowSystemEditorWindow.defaultSkin;
						this.dataSelectionScroll = GUILayout.BeginScrollView(this.dataSelectionScroll, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, backStyle);
						{

							GUI.skin = skin;

							if (this.scannedData == null) {

								this.scannedData = EditorUtilities.GetAssetsDataOfType<FlowData>("*.asset", true);

							} else {

								if (this.scannedData.Length == 0) {

									var center = new GUIStyle(FlowSystemEditorWindow.defaultSkin.label);
									center.stretchWidth = true;
									center.stretchHeight = true;
									center.alignment = TextAnchor.MiddleCenter;

									GUILayout.Label("No projects was found. Create a new one by Right-Click on any folder in Project View and choose Create->UI.Windows->Flow->Graph option.", center);

								} else {

									var buttonStyle = new GUIStyle("U2D.createRect");
									buttonStyle.padding = new RectOffset(15, 15, 15, 15);
									buttonStyle.margin = new RectOffset(2, 2, 2, 2);
									buttonStyle.fixedWidth = 0f;
									buttonStyle.fixedHeight = 0f;
									buttonStyle.stretchWidth = true;
									buttonStyle.stretchHeight = false;
									buttonStyle.normal.textColor = Color.black;
									buttonStyle.fontSize = 12;
									buttonStyle.richText = true;

									var buttonStyleSelected = new GUIStyle(buttonStyle);

									buttonStyle.normal.background = null;
									
									this.scannedData = this.scannedData.OrderByDescending((data) => data.lastModified).ToArray();

									foreach (var data in this.scannedData) {

										var title = data.name + "\n<color=#777><size=10>Modified: " + data.lastModified + "</size></color>";

										if (GUILayout.Button(title, this.cachedData == data ? buttonStyleSelected : buttonStyle) == true) {

											this.cachedData = data;

										}

									}

								}

							}

							GUILayout.FlexibleSpace();

						}
						GUILayout.EndScrollView();
						
						GUILayout.Space(10f);

						GUILayout.Label("Or select the project file:", darkLabel);

						this.cachedData = GUILayoutExt.ObjectField<FlowData>(this.cachedData, false, FlowSystemEditorWindow.defaultSkin.FindStyle("ObjectField"));

						CustomGUI.Splitter();

						GUILayout.BeginHorizontal();
						{
							
							GUILayout.FlexibleSpace();

							if (GUILayout.Button("Open", FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(40f)) == true) {
								
								FlowSystem.SetData(this.cachedData);
								
							}
							
						}
						GUILayout.EndHorizontal();

					}
					GUILayout.EndArea();

				}
				GUILayout.EndArea();

			}
			GUILayout.EndArea();

		}

		public void OpenFlowData(FlowData flowData) {

			this.cachedData = flowData;
			FlowSystem.SetData(flowData);

		}

		public void ChangeFlowData() {

			FlowSystem.SetData(null);
			this.defaultWindows = null;
			this.tagsList = null;

		}

		private void CreateNewItem(System.Func<FlowWindow> predicate) {
			
			var scrollPos = FlowSystem.GetScrollPosition();
			
			var window = predicate();
			window.rect.x = scrollPos.x + this.scrollRect.width * 0.5f - window.rect.width * 0.5f;
			window.rect.y = scrollPos.y + this.scrollRect.height * 0.5f - window.rect.height * 0.5f;

		}
		
		private void DrawToolbar() {

			var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.stretchWidth = false;
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

			this.DrawToolbar(buttonStyle);
			
			GUILayout.EndHorizontal();

		}

		private void DrawToolbar(GUIStyle buttonStyle) {

			if (GUILayout.Button("New Window", buttonStyle) == true) {

				this.CreateNewItem(() => FlowSystem.CreateWindow());

			}
			
			if (GUILayout.Button("New Container", buttonStyle) == true) {
				
				this.CreateNewItem(() => FlowSystem.CreateContainer());

			}
			
			if (GUILayout.Button("New Default Link", buttonStyle) == true) {
				
				this.CreateNewItem(() => FlowSystem.CreateDefaultLink());

			}
			   
			if (GUILayout.Button("Center Screen", buttonStyle)) {
				
				FlowSystem.SetScrollPosition(Vector2.one * -1f);
				
			}

			Flow.OnDrawToolbarGUI(buttonStyle);

			GUILayout.FlexibleSpace();

			GUILayout.Label("Current Data: " + AssetDatabase.GetAssetPath(this.cachedData), buttonStyle);

			if (GUILayout.Button("Change", buttonStyle) == true) {
				
				this.ChangeFlowData();

			}

		}
		
		private void DrawNodeContainer(int id) {

			EditorGUIUtility.labelWidth = 65f;

			GUI.enabled = true;//!FlowSceneView.IsActive();

			var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.stretchWidth = false;
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			
			var window = FlowSystem.GetWindow(id);

			if (this.waitForAttach == true) {
				
				if (id != this.currentAttachId) {
					
					var currentAttach = FlowSystem.GetWindow(this.currentAttachId);
					var attachTo = FlowSystem.GetWindow(id);
					var hasContainer = currentAttach.HasContainer();
					var container = currentAttach.GetContainer();

					if ((attachTo.isContainer == true && currentAttach.isContainer == true && attachTo == container) || (hasContainer == true && container.id != id)) {
						
						
						
					} else {

						if (attachTo.isContainer == true && currentAttach.isContainer == true) {

							if (FlowSystem.AlreadyAttached(id, this.currentAttachId) == true) {
								
								if (GUILayout.Button("Detach Here", buttonStyle) == true) {
									
									FlowSystem.Detach(this.currentAttachId, id, oneWay: false);
									if (!Event.current.shift) this.WaitForAttach(-1);
									
								}
								
							} else {
								
								if (GUILayout.Button("Attach Here", buttonStyle) == true) {

									FlowSystem.Attach(id, this.currentAttachId, oneWay: true);
									if (!Event.current.shift) this.WaitForAttach(-1);
									
								}
								
							}

						} else {

							if (FlowSystem.AlreadyAttached(this.currentAttachId, id) == true) {
								
								if (GUILayout.Button("Detach Here", buttonStyle) == true) {

									FlowSystem.Detach(this.currentAttachId, id, oneWay: false);
									if (!Event.current.shift) this.WaitForAttach(-1);
									
								}
								
							} else {
								
								if (GUILayout.Button("Attach Here", buttonStyle) == true) {

									FlowSystem.Attach(this.currentAttachId, id, oneWay: false);
									if (!Event.current.shift) this.WaitForAttach(-1);
									
								}
								
							}

						}

					}

				} else {
					
					if (GUILayout.Button("Cancel", buttonStyle) == true) {
						
						this.WaitForAttach(-1);
						
					}
					
				}
				
			} else {
				
				if (GUILayout.Button("Attach/Detach", buttonStyle) == true) {
					
					this.ShowNotification(new GUIContent("Use Attach/Detach buttons to connect/disconnect the container"));
					this.WaitForAttach(id);
					
				}

				if (GUILayout.Button("Destroy", buttonStyle) == true) {
					
					if (EditorUtility.DisplayDialog("Are you sure?", "Current container will be destroyed with all links (All windows will be saved)", "Yes, destroy", "No") == true) {
						
						this.ShowNotification(new GUIContent("The container '" + window.title + "' was successfully destroyed"));
						FlowSystem.DestroyWindow(id);
						return;

					}
					
				}

			}
			
			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayoutExt.LabelWithShadow("Title:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
			window.title = GUILayout.TextField(window.title);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayoutExt.LabelWithShadow("Directory:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
			window.directory = GUILayout.TextField(window.directory);
			GUILayout.EndHorizontal();

			this.DrawTags(window);

			var attaches = window.attaches.Count;
			if (attaches == 0) {

				this.DragWindow(headerOnly: false);

			} else {
				
				this.DragWindow(headerOnly: true);

			}
			
			GUI.enabled = true;

			EditorGUIUtility.LookLikeControls();

		}

		private void DragWindow(bool headerOnly) {
			
			GUI.enabled = true;//!FlowSceneView.IsActive();
			if (GUI.enabled == false) return;

			if (Event.current.button != 2) {

				if (headerOnly == false) {

					GUI.DragWindow();

				} else {

					var dragRect = new Rect(0f, 0f, 5000f, 20f);
					GUI.DragWindow(dragRect);

				}

			}

			GUI.enabled = true;

		}

		private void UpdateFocus(int id) {

			if ((Event.current.button == 0) && (Event.current.type == EventType.MouseDown)) {

				this.focusedGUIWindow = id;

			}

		}

		private void DrawTagsPopup() {

			if (this.showTagsPopup == false) return;

			if (string.IsNullOrEmpty(this.tagCaption) == true) return;

			var oldColor = GUI.color;

			this.Repaint();
			
			var tagStyles = new GUIStyle[7] {
				
				new GUIStyle("sv_label_1"),
				new GUIStyle("sv_label_2"),
				new GUIStyle("sv_label_3"),
				new GUIStyle("sv_label_4"),
				new GUIStyle("sv_label_5"),
				new GUIStyle("sv_label_6"),
				new GUIStyle("sv_label_7")
				
			};

			var tagStyle = tagStyles[0];
			tagStyle.stretchWidth = false;
			tagStyle.margin = new RectOffset(0, 0, 2, 2);

			var shadow = new GUIStyle("ObjectPickerPreviewBackground");

			var allTags = FlowSystem.GetTags();
			if (allTags != null) {
				
				var window = FlowSystem.GetWindow(this.showTagsPopupId);

				var topOffset = 15f;
				var backTopOffset = 12f;
				var offset = 5f;

				var count = 0;
				foreach (var tag in allTags) {
					
					if (tag.title.ToLower().Contains(this.tagCaption.ToLower()) == true && window != null && window.tags.Contains(tag.id) == false) {

						++count;

					}

				}

				if (count > 0) {

					var r = this.showTagsPopupRect;
					var elementHeight = r.height + tagStyle.margin.top + tagStyle.margin.bottom;
					
					r.y += this.showTagsPopupRect.height * 2f + offset;
					r.height = elementHeight * count + topOffset;

					var scrollPos = FlowSystem.GetScrollPosition();

					var drawRect = new Rect(r.x - scrollPos.x, r.y - scrollPos.y - topOffset, r.width, r.height);

					tagStyle.fixedWidth = drawRect.width;

					GUILayout.BeginArea(drawRect);
					{
						GUI.color = Color.black;
						GUI.Label(new Rect(10f, backTopOffset, drawRect.width - 10f * 2f, drawRect.height - backTopOffset * 2f), string.Empty, shadow);

						GUILayout.Space(topOffset);

						GUI.color = oldColor;
						foreach (var tag in allTags) {
							
							if (tag.title.ToLower().Contains(this.tagCaption.ToLower()) == true && window != null && window.tags.Contains(tag.id) == false) {

								var style = tagStyles[tag.color];
								style.margin = tagStyle.margin;
								style.fixedWidth = tagStyle.fixedWidth;

								if (GUILayout.Button(tag.title, style) == true) {

									this.tagCaption = string.Empty;
									this.showTagsPopupId = -1;

									window.AddTag(tag);
									
								}
								
							}
							
						}
					}
					GUILayout.EndArea();

				}

			}

			GUI.color = oldColor;

		}

		private bool showTagsPopup = false;
		private Rect showTagsPopupRect;
		private int showTagsPopupId;
		private string tagCaption = string.Empty;
		private void DrawTags(FlowWindow window) {
			
			var tagStyles = new GUIStyle[7] {
				
				new GUIStyle("sv_label_1"),
				new GUIStyle("sv_label_2"),
				new GUIStyle("sv_label_3"),
				new GUIStyle("sv_label_4"),
				new GUIStyle("sv_label_5"),
				new GUIStyle("sv_label_6"),
				new GUIStyle("sv_label_7")
				
			};

			var tagCaptionStyle = new GUIStyle("sv_label_0");
			tagCaptionStyle.fixedWidth = 60f;
			tagCaptionStyle.stretchWidth = false;
			
			var tagStyleAdd = new GUIStyle("sv_label_3");
			tagStyleAdd.stretchWidth = false;
			
			var changed = false;

			GUILayout.BeginHorizontal();
			{
				GUILayoutExt.LabelWithShadow("Tags:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));

				GUILayout.BeginHorizontal();
				{
					var tagCaption = string.Empty;
					if (this.showTagsPopupId == window.id) tagCaption = this.tagCaption;

					var isEnter = (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return);

					var newTagCaption = string.Empty;
					var rect = new Rect();
					GUILayout.BeginHorizontal();
					{

						var oldEnabled = GUI.enabled;
						GUI.enabled = !string.IsNullOrEmpty(this.tagCaption) && (this.showTagsPopupId == window.id);
						if ((GUILayout.Button(new GUIContent("+"), tagStyleAdd) == true || isEnter == true) && GUI.enabled == true) {
							
							FlowSystem.AddTag(window, new FlowTag(FlowSystem.GetData().GetNextTagId(), this.tagCaption));
							this.tagCaption = string.Empty;
							
						}
						GUI.enabled = oldEnabled;

						newTagCaption = GUILayout.TextField(tagCaption, tagCaptionStyle);
						rect = GUILayoutUtility.GetLastRect();

					}
					GUILayout.EndHorizontal();

					if (tagCaption != newTagCaption) {
						
						this.showTagsPopupId = window.id;
						this.tagCaption = newTagCaption;
						
						this.showTagsPopup = false;
						changed = true;

					}

					if (this.showTagsPopupId == window.id && newTagCaption.Length > 0) {

						// Show Tags Popup
						var allTags = FlowSystem.GetTags();
						if (allTags != null) {

							this.showTagsPopup = true;
							if (Event.current.type == EventType.Repaint) {

								this.showTagsPopupRect = new Rect(window.rect.x + rect.x + SETTINGS_WIDTH, window.rect.y + rect.y, rect.width, rect.height);

							}

							if (changed == true) this.Repaint();

						}

					}

					GUILayout.FlexibleSpace();

					foreach (var tag in window.tags) {

						var tagInfo = FlowSystem.GetData().GetTag(tag);
						if (tagInfo == null) {

							window.tags.Remove(tag);
							break;

						}

						if (GUILayout.Button(tagInfo.title, tagStyles[tagInfo.color]) == true) {

							FlowSystem.RemoveTag(window, tagInfo);
							break;

						}

					}

				}
				GUILayout.EndHorizontal();

			}
			GUILayout.EndHorizontal();

			if (changed == true) {

				this.Repaint();

			}

		}

		private void DrawStates(CompletedState[] states, FlowWindow window) {

			if (states == null) return;

			var oldColor = GUI.color;
			var style = new GUIStyle("Grad Down Swatch");

			var elemWidth = style.fixedWidth - 3f;
			var width = window.rect.width - 6f;

			var posY = -9f;

			var color = Color.black;
			color.a = 0.6f;
			var posX = width - elemWidth;

			var shadowOffset = 1f;
			for (int i = states.Length - 1; i >= 0; --i) {

				GUI.color = color;
				GUI.Label(new Rect(posX + shadowOffset, posY + shadowOffset, elemWidth, style.fixedHeight), string.Empty, style);
				posX -= elemWidth;

			}
			
			posX = width - elemWidth;
			for (int i = states.Length - 1; i >= 0; --i) {

				var state = states[i];

				if (state == CompletedState.NotReady) {
					
					color = new Color(1f, 0.3f, 0.3f, 1f);
					
				} else if (state == CompletedState.Ready) {
					
					color = new Color(0.3f, 1f, 0.3f, 1f);
					
				} else if (state == CompletedState.ReadyButWarnings) {
					
					color = new Color(1f, 1f, 0.3f, 1f);
					
				}

				GUI.color = color;
				GUI.Label(new Rect(posX, posY, elemWidth, style.fixedHeight), string.Empty, style);
				posX -= elemWidth;

			}

			GUI.color = oldColor;

		}

		private void DrawWindowToolbar(FlowWindow window) {

			var id = window.id;

			var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.stretchWidth = false;

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			if (this.waitForAttach == true) {
				
				if (id != this.currentAttachId) {
					
					var currentAttach = FlowSystem.GetWindow(this.currentAttachId);
					//var attachTo = FlowSystem.GetWindow(id);
					//var hasContainer = currentAttach.HasContainer();
					
					if (currentAttach.isContainer == true) {
						
						
						
					} else {
						
						if (FlowSystem.AlreadyAttached(this.currentAttachId, id) == true) {
							
							if (GUILayout.Button("Detach Here" + (Event.current.alt ? " (Double Direction)" : ""), buttonStyle) == true) {
								
								FlowSystem.Detach(this.currentAttachId, id, oneWay: !Event.current.shift);
								if (!Event.current.shift) this.WaitForAttach(-1);
								
							}
							
						} else {
							
							if (GUILayout.Button("Attach Here" + (Event.current.alt ? " (Double Direction)" : ""), buttonStyle) == true) {
								
								FlowSystem.Attach(this.currentAttachId, id, oneWay: !Event.current.shift);
								if (!Event.current.shift) this.WaitForAttach(-1);
								
							}
							
						}
						
					}
					
				} else {
					
					if (GUILayout.Button("Cancel", buttonStyle) == true) {
						
						this.WaitForAttach(-1);
						
					}
					
				}
				
			} else {
				
				if (window.isDefaultLink == false) {

					if (GUILayout.Button("Attach/Detach", buttonStyle) == true) {
						
						this.ShowNotification(new GUIContent("Use Attach/Detach buttons to connect/disconnect the window"));
						this.WaitForAttach(id);
						
					}

				}
				
				if (GUILayout.Button("Destroy", buttonStyle) == true) {
					
					if (EditorUtility.DisplayDialog("Are you sure?", "Current window will be destroyed with all links", "Yes, destroy", "No") == true) {
						
						this.ShowNotification(new GUIContent("The window '" + window.title + "' was successfully destroyed"));
						FlowSystem.DestroyWindow(id);
						return;
						
					}
					
				}

			}

			if (window.isDefaultLink == false) {

				var isRoot = (FlowSystem.GetRootWindow() == id);
				if (GUILayout.Toggle(isRoot, new GUIContent("R", "Set as root"), buttonStyle) != isRoot) {
					
					if (isRoot == true) {
						
						// Was as root
						FlowSystem.SetRootWindow(-1);
						
					} else {
						
						// Was not as root
						FlowSystem.SetRootWindow(id);
						
					}
					
					FlowSystem.SetDirty();
					
				}
				
				var isDefault = FlowSystem.GetDefaultWindows().Contains(id);
				if (GUILayout.Toggle(isDefault, new GUIContent("D", "Set as default"), buttonStyle) != isDefault) {
					
					if (isDefault == true) {
						
						// Was as default
						FlowSystem.GetDefaultWindows().Remove(id);
						
					} else {
						
						// Was not as default
						FlowSystem.GetDefaultWindows().Add(id);
						
					}
					
					FlowSystem.SetDirty();
					
				}

			}

			GUILayout.FlexibleSpace();
			
			var edit = false;
			if (window.isDefaultLink == false && FlowSceneView.IsActive() == false) {

				if (GUILayout.Button("Edit", buttonStyle) == true) {
					
					if (window.compiled == false) {
						
						this.ShowNotification(new GUIContent("You need to compile this window to use 'Edit' command"));
						
					} else {
						
						edit = true;
						
					}
					
				}

			}
			GUILayout.EndHorizontal();
			
			if (edit == true) {
				
				FlowSceneView.SetControl(this, window, this.OnItemProgress);
				
			}

		}

		private void DrawNodeWindow(int id) {

			EditorGUIUtility.labelWidth = 65f;

			this.UpdateFocus(id);
			
			var window = FlowSystem.GetWindow(id);

			if (window.isDefaultLink == true) {
				
				this.DrawWindowToolbar(window);
				this.DragWindow(headerOnly: false);

			} else {

				this.DrawStates(window.states, window);

				GUI.enabled = true;//!FlowSceneView.IsActive();

				this.DrawWindowToolbar(window);

				GUILayout.BeginHorizontal();
				GUILayoutExt.LabelWithShadow("Title:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
				window.title = GUILayout.TextField(window.title);
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayoutExt.LabelWithShadow("Directory:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
				if (string.IsNullOrEmpty(window.directory) == true) window.directory = string.Empty;
				window.directory = GUILayout.TextField(window.directory);
				GUILayout.EndHorizontal();
				
				this.DrawTags(window);

				Flow.OnDrawWindowGUI(window);

				this.DragWindow(headerOnly: false);

				if (GUI.changed == true) this.cachedData.isDirty = true;

				GUI.enabled = true;

			}

			EditorGUIUtility.LookLikeControls();

		}
		
		//private float itemProgress;
		public void OnItemProgress(float value) {

			//this.itemProgress = value;
			this.Repaint();

		}

		private int currentAttachId = -1;
		private bool waitForAttach = false;
		private void WaitForAttach(int id) {

			this.currentAttachId = id;
			this.waitForAttach = id >= 0;

		}

		private void DrawNodeCurve(FlowWindow from, FlowWindow to, bool doubleSide) {

			Rect start = from.rect;
			Rect end = to.rect;

			var color1 = Color.white;
			var color2 = Color.white;

			if (from.GetContainer() != to.GetContainer()) {
				
				color1 = Color.gray;
				color2 = Color.gray;

				if (to.GetContainer() != null) color1 = to.GetContainer().randomColor;
				if (from.GetContainer() != null) color2 = from.GetContainer().randomColor;

			}

			var zOffset = -10f;

			if (doubleSide == true) {

				var size = 6f;
				
				var rot = Quaternion.AngleAxis(90f, Vector3.back);
				var ray = new Ray(Vector3.zero, (rot * (end.center - start.center)).normalized);

				var offset = ray.GetPoint(size);
				var startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, zOffset);
				var endPos = new Vector3(end.center.x + offset.x, end.center.y + offset.y, zOffset);
				
				this.DrawNodeCurve(startPos, endPos, color1);
				
				offset = ray.GetPoint(-size);
				startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, zOffset);
				endPos = new Vector3(end.center.x + offset.x, end.center.y + offset.y, zOffset);
				
				this.DrawNodeCurve(endPos, startPos, color2);

			} else {

				var offset = Vector2.zero;
				var startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, zOffset);
				var endPos = new Vector3(end.center.x + offset.x, end.center.y + offset.y, zOffset);

				this.DrawNodeCurve(startPos, endPos, color1);

			}

		}

		private void DrawNodeCurve(Vector3 startPos, Vector3 endPos, Color color) {

			var shadowColor = new Color(0f, 0f, 0f, 0.5f);
			var lineColor = color;//new Color(1f, 1f, 1f, 1f);

			var shadowOffset = Vector3.one * 2f;
			shadowOffset.z = 0f;

			Handles.BeginGUI();

			Handles.color = shadowColor;
			Handles.DrawAAPolyLine(4f, new Vector3[] { startPos, endPos });
			
			Handles.color = lineColor;
			Handles.DrawAAPolyLine(4f, new Vector3[] { startPos, endPos });
			
			var ray = new Ray(startPos, (endPos - startPos).normalized);
			var rot = Quaternion.LookRotation(endPos - startPos);

			var every = 300f;
			var fullDistance = Vector3.Distance(endPos, startPos);
			if (fullDistance < every * 2f) {

				var pos = ray.GetPoint(fullDistance * 0.5f);

				Handles.color = shadowColor;
				Handles.ConeCap(-1, pos + shadowOffset, rot, 15f);
				Handles.color = lineColor;
				Handles.ConeCap(-1, pos, rot, 15f);

				//var arrow = (new GUIStyle("StaticDropdown")).normal.background;
				//GUIExt.DrawTextureRotated(pos, arrow, Vector3.Angle(startPos, endPos));

			} else {

				for (float distance = every; distance < fullDistance; distance += every) {

					var pos = ray.GetPoint(distance);

					Handles.color = shadowColor;
					Handles.ConeCap(-1, pos + shadowOffset, rot, 15f);
					Handles.color = lineColor;
					Handles.ConeCap(-1, pos, rot, 15f);

					//var arrow = (new GUIStyle("StaticDropdown")).normal.background;
					//GUIExt.DrawTextureRotated(pos, arrow, rot);

				}

			}

			Handles.EndGUI();

		}

	}

}
