using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using UnityEditorInternal;
using ME;
using System.Reflection;
using UnityEngine.UI.Windows.Types;
using UnityEditor.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Plugins.Flow {
	
	public class FlowSystemEditorWindow : EditorWindowExt {
		
		public static GUISkin defaultSkin;
		private static bool loaded = false;
		private static bool loading = false;

		private GUIDrawer guiDrawer;
		public EditorZoomArea zoomDrawer;

		public static FlowSystemEditorWindow ShowEditor(System.Action onClose) {
			
			var editor = FlowSystemEditorWindow.GetWindow<FlowSystemEditorWindow>(typeof(SceneView));
			editor.autoRepaintOnSceneChange = true;
			editor.onClose = onClose;
			editor.wantsMouseMove = true;
			
			var title = "UIW Flow";
			#if !UNITY_4
			editor.titleContent = new GUIContent(title, Resources.Load<Texture2D>("UI.Windows/Icons/FlowIcon"));
			#else
			editor.title = title;
			#endif
			
			var width = 800f;
			var height = 600f;

			var fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
			var isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
			if ((bool)isDockedMethod.Invoke(editor, null) == false) {
				
				var rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
				if (rect.x < 120f) rect.x = 120f;
				if (rect.y < 120f) rect.y = 120f;
				
				editor.position = rect;
				
				editor.Focus();
				
			}

			editor.minSize = new Vector2(width, height);
			editor.ChangeFlowData();

			FlowSystemEditorWindow.loaded = false;
			FlowSystemEditorWindow.loading = false;
			
			return editor;
			
		}
		
		private System.Action onClose;
		
		public Rect scrollRect;
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
		
		public int focusedGUIWindow {
			
			get;
			private set;
			
		}
		
		public override void Update() {
			
			if (FlowSystemEditorWindow.loading == true) {
				
				return;
				
			}
			
			if (FlowSystemEditorWindow.loaded == false) {
				
				FlowSystemEditorWindow.loading = true;
				
				EditorApplication.delayCall += () => {
					
					if (this.guiDrawer == null) this.guiDrawer = new GUIDrawer(this);

					// Cache
					ME.EditorUtilities.GetAssetsOfType<FlowData>();
					ME.EditorUtilities.GetPrefabsOfType<FlowWindowLayoutTemplate>();
					ME.EditorUtilities.GetPrefabsOfType<FlowLayoutWindowTypeTemplate>();
					ME.EditorUtilities.GetPrefabsOfType<WindowModule>(strongType: false);

					FlowSystemEditorWindow.loading = false;
					FlowSystemEditorWindow.loaded = true;
					
				};
				
				return;
				
			} else {
				
				CoreUtilities.LoadAddons();
				
			}
			
		}
		
		public const float SETTINGS_WIDTH = 280f;
		public const float TOOLBAR_HEIGHT = 18f;
		
		private Texture2D _background;
		private List<int> tempAttaches = new List<int>();
		private void OnGUI() {
			
			if (FlowSystemEditorWindow.defaultSkin == null) FlowSystemEditorWindow.defaultSkin = Resources.Load("UI.Windows/Flow/Styles/Skin" + (EditorGUIUtility.isProSkin == true ? "Dark" : "Light")) as GUISkin;
			
			if (FlowSystemEditorWindow.loaded == false) {
				
				this.DrawLoader();
				return;
				
			}
			
			if (this.guiDrawer == null) this.guiDrawer = new GUIDrawer(this);
			if (this.zoomDrawer == null) this.zoomDrawer = new EditorZoomArea();

			//var draw = !FlowSceneView.IsActive();
			
			//if (draw == true) {
			
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
				if (scrollPos.x > 0f || scrollPos.y > 0f) scrollPos = -new Vector2(this.contentRect.width * 0.5f - this.scrollRect.width * 0.5f, this.contentRect.height * 0.5f - this.scrollRect.height * 0.5f);
				//FlowSystem.SetScrollPosition(GUI.BeginScrollView(this.scrollRect, scrollPos, this.contentRect));
				{

					//this.zoomDrawer.SetZoom(FlowSystem.GetZoom());
					//FlowSystem.SetScrollPosition(this.zoomDrawer.SetRect(this.scrollRect, scrollPos));
					//FlowSystem.SetZoom(this.zoomDrawer.GetZoom());
					//FlowSystem.SetScrollPosition(this.zoomDrawer.GetOrigin());

					this.zoomDrawer.SetZoom(FlowSystem.GetZoom());
					FlowSystem.SetScrollPosition(this.zoomDrawer.Begin(this.scrollRect, scrollPos, this.contentRect));
					FlowSystem.SetZoom(this.zoomDrawer.GetZoom());
					{

						this.DrawBackground();

						if (hasData == true && windows != null) {
							
							this.tempAttaches.Clear();
							foreach (var window in windows) {

								var attaches = window.attachItems;
								foreach (var attachItem in attaches) {

									var attachId = attachItem.targetId;

									var curWindow = FlowSystem.GetWindow(attachId);
									if (curWindow.IsContainer() == true &&
									    curWindow.IsFunction() == false) continue;
									
									//if (this.IsVisible(window) == false) continue;

									if (curWindow.IsFunction() == true &&
									    curWindow.IsContainer() == true) {
										
										if (curWindow.functionRootId == window.id) {
											
											// Find entrance window
											var rootWindow = FlowSystem.GetWindow(curWindow.functionRootId);
											if (rootWindow != null) {
												
												// Draw entrance point
												this.guiDrawer.DrawNodeCurve(new Vector3(curWindow.rect.x + 6f, curWindow.rect.y + curWindow.rect.height * 0.5f + 25f, -10f),
												                   			 new Vector3(rootWindow.rect.x + rootWindow.rect.width * 0.5f, rootWindow.rect.y + rootWindow.rect.height * 0.5f, -10f),
												                   			 Color.yellow);
												
											}
											
										}
										
										if (curWindow.functionExitId == window.id) {
											
											// Draw exit point
											var exitWindow = FlowSystem.GetWindow(curWindow.functionExitId);
											if (exitWindow != null) {
												
												// Draw entrance point
												this.guiDrawer.DrawNodeCurve(new Vector3(exitWindow.rect.x + exitWindow.rect.width * 0.5f, exitWindow.rect.y + exitWindow.rect.height * 0.5f, -10f),
												                   			 new Vector3(curWindow.rect.x + curWindow.rect.width - 6f, curWindow.rect.y + curWindow.rect.height * 0.5f + 25f, -10f),
												                  			 Color.green);
												
											}
											
										}
										
									} else {
										
										var doubleSided = FlowSystem.AlreadyAttached(attachId, window.id);
										if (this.tempAttaches.Contains(attachId) == true && doubleSided == true) continue;
										
										this.guiDrawer.DrawNodeCurve(window, curWindow, doubleSided);

										// Draw Transition Chooser
										this.DrawTransitionChooser(attachItem, window, curWindow, doubleSided);

									}
									
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
								
								if (this.IsVisible(container) == false) continue;

								var backColor = container.randomColor;
								backColor.a = 0.3f;
								GUI.backgroundColor = backColor;
								
								var rootContainer = container.GetContainer();
								if (rootContainer != null) {
									
									// If container has other container
									
								}
								
								var attaches = container.attachItems;
								if (attaches.Count == 0) {
									
									container.rect.width = 200f;
									container.rect.height = 200f;
									
								} else {
									
									var minX = float.MaxValue;
									var minY = float.MaxValue;
									var maxX = float.MinValue;
									var maxY = float.MinValue;
									foreach (var attachItem in attaches) {

										var attachId = attachItem.targetId;
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

								if (this.IsVisible(window) == false) continue;

								var title = string.Empty;
								if (window.IsSmall() == true) {
									
									title = window.title;
									
								} else {
									
									var size = this.GetWindowSize(window);
									
									window.rect.width = size.x;
									window.rect.height = size.y;
									
								}
								
								var isSelected = selected.Contains(window.id) || (selected.Count == 0 && this.focusedGUIWindow == window.id);
								var style = window.GetEditorStyle(isSelected);
								
								var rect = GUI.Window(window.id, window.rect, this.DrawNodeWindow, title, style);
								GUI.BringWindowToFront(window.id);

								GUI.Window(-window.id, new Rect(rect.x, rect.y + rect.height, rect.width, rect.height), (id) => {

									this.DrawTags(FlowSystem.GetWindow(-id), true);

								}, string.Empty, GUIStyle.none);
								GUI.BringWindowToFront(-window.id);

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
							
							if (selectionMain >= 0 && FlowSystem.GetWindow(selectionMain).IsContainer() == true) FlowSystem.ResetSelection();
							
							GUI.color = defaultColor;
							
							FlowSystem.Save();
							
						}
						
						if (FlowSystem.GetZoom() >= 0.3f) {

							if (FlowSystem.GetData() != null && FlowSystem.GetData().flowWindowWithLayout == true) {
								
								foreach (var window in windows) {

									if (this.IsVisible(window) == false) continue;

									var components = window.attachedComponents;
									for (int i = 0; i < components.Count; ++i) {
										
										var component = components[i];
										this.guiDrawer.DrawComponentCurve(window, ref component, FlowSystem.GetWindow (component.targetWindowId));
										components[i] = component;
										
									}
									
								}
								
							}

						}

					}
					this.zoomDrawer.End();

				}
				//GUI.EndScrollView();

				this.DrawWaitForConnection();
				this.DrawTagsPopup();
				this.HandleEvents(TOOLBAR_HEIGHT);
				
				GUI.enabled = true;
				
				if (this.scrollingMouseAnimation != null && this.scrollingMouseAnimation.isAnimating == true || this.scrollingMouse == true) this.DrawMinimap();
				
			}
			
		}

		public bool IsVisible(FlowWindow window) {

			/*var scrollPos = FlowSystem.GetScrollPosition();
			var rect = new Rect(scrollPos.x - this.scrollRect.width * 0.5f + this.scrollRect.x,
			                    scrollPos.y + this.scrollRect.y,
			                    this.scrollRect.width,
			                    this.scrollRect.height);

			var newState = true;//rect.ScaleSizeBy(this.zoomDrawer.GetZoom()).Overlaps(window.rect.ScaleSizeBy(this.zoomDrawer.GetZoom()));

			if (newState == true &&
				window.isVisibleState == false) {

				window.isVisibleState = true;
				this.Repaint();
				return false;

			}

			return newState;*/

			return true;

		}
		
		public Vector2 GetWindowSize(FlowWindow window) {
			
			var flowWindowWithLayout = FlowSystem.GetData().flowWindowWithLayout;
			var flowWindowWithLayoutScaleFactor = FlowSystem.GetData().flowWindowWithLayoutScaleFactor;
			if (flowWindowWithLayout == true) {

				return new Vector2(250f, 250f) * (1f + flowWindowWithLayoutScaleFactor);

			}
			
			return new Vector2(250f, 80f + (Mathf.CeilToInt(window.tags.Count / 3f)) * 15f);
			
		}
		
		public void OnLostFocus() {
			
			//FlowSceneView.Reset();
			
		}
		
		private void OnFocus() {
			
			//FlowSceneView.Reset();
			
		}
		
		public void DrawTransitionChooser(UnityEngine.UI.Windows.Plugins.Flow.FlowWindow.AttachItem attach, FlowWindow fromWindow, FlowWindow toWindow, bool doubleSided) {
			
			if (toWindow.IsEnabled() == false) return;

			if (toWindow.IsContainer() == true) return;

			if (toWindow.IsSmall() == true) {

				if (toWindow.IsFunction() == false) return;

			}

			const float size = 32f;
			const float offset = size * 0.5f + 5f;

			if (doubleSided == true) {

				var q = Quaternion.LookRotation(toWindow.rect.center - fromWindow.rect.center, Vector3.back);

				this.DrawTransitionChooser(attach, fromWindow, toWindow, q * Vector2.left * offset, size);
				this.DrawTransitionChooser(attach, fromWindow, toWindow, q * Vector2.right * offset, size);

			} else {

				this.DrawTransitionChooser(attach, fromWindow, toWindow, Vector2.zero, size);

			}

		}

		public void DrawTransitionChooser(UnityEngine.UI.Windows.Plugins.Flow.FlowWindow.AttachItem attach, FlowWindow fromWindow, FlowWindow toWindow, Vector2 offset, float size) {

			var _size = Vector2.one * size;
			var rect = new Rect(Vector2.Lerp(fromWindow.rect.center, toWindow.rect.center, 0.5f) + offset - _size * 0.5f, _size);

			var transitionStyle = ME.Utilities.CacheStyle("UI.Windows.Styles.DefaultSkin", "TransitionIcon", (name) => FlowSystemEditorWindow.defaultSkin.FindStyle("TransitionIcon"));
			var transitionStyleBorder = ME.Utilities.CacheStyle("UI.Windows.Styles.DefaultSkin", "TransitionIconBorder", (name) => FlowSystemEditorWindow.defaultSkin.FindStyle("TransitionIconBorder"));
			if (transitionStyle != null && transitionStyleBorder != null) {

				if (fromWindow.GetScreen() != null) {

					System.Action onClick = () => {
						
						FlowChooserFilter.CreateTransition(fromWindow, toWindow, "/Transitions", (element) => {
							
							FlowSystem.Save();
							
						});

					};

					// Has transition or not?
					var hasTransition = attach.transition != null && attach.transitionParameters != null;
					if (hasTransition == true) {

						var hovered = rect.Contains(Event.current.mousePosition);
						if (attach.editor == null) {

							attach.editor = Editor.CreateEditor(attach.transitionParameters) as IPreviewEditor;
							hovered = true;

						}
						if (attach.editor.HasPreviewGUI() == true) {

							if (hovered == false) {

								attach.editor.OnDisable();

							} else {

								attach.editor.OnEnable();
								
							}

							var style = new GUIStyle(EditorStyles.toolbarButton);
							attach.editor.OnPreviewGUI(Color.white, rect, style, false, false, hovered);

						}

						if (GUI.Button(rect, string.Empty, transitionStyleBorder) == true) {

							onClick();

						}

					} else {
						
						GUI.Box(rect, string.Empty, transitionStyle);
						if (GUI.Button(rect, string.Empty, transitionStyleBorder) == true) {
							
							onClick();

						}

					}

				}

			}

		}

		private GUIStyle layoutBoxStyle;
		public void DrawWindowLayout(FlowWindow window) {
			
			var flowWindowWithLayout = FlowSystem.GetData().flowWindowWithLayout;
			if (flowWindowWithLayout == true) {
				
				if (this.layoutBoxStyle == null) this.layoutBoxStyle = FlowSystemEditorWindow.defaultSkin.FindStyle("LayoutBox");
				
				GUILayout.Box(string.Empty, this.layoutBoxStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				var rect = GUILayoutUtility.GetLastRect();
				
				if (window.OnPreviewGUI(rect,
				                        FlowSystemEditorWindow.defaultSkin.button,
				                        this.layoutBoxStyle,
				                        drawInfo: true,
				                        selectable: true,
				                        onCreateScreen: () => {
					
					this.SelectWindow(window);
					FlowChooserFilter.CreateScreen(Selection.activeObject, "/Screens", () => {
						
						this.SelectWindow(window);
						
					});
					
				}, onCreateLayout: () => {
					
					this.SelectWindow(window);
					Selection.activeObject = window.GetScreen();
					FlowChooserFilter.CreateLayout(Selection.activeObject, Selection.activeGameObject, () => {
						
						this.SelectWindow(window);
						
					});
					
				}) == true) {
					
					// Set for waiting connection
					var element = WindowLayoutElement.waitForComponentConnectionElementTemp;
					
					this.WaitForAttach(window.id, element);
					
					WindowLayoutElement.waitForComponentConnectionTemp = false;
					
				}
				
				UnityEditor.UI.Windows.Plugins.Flow.Flow.OnDrawWindowLayoutGUI(rect, window);
				
			}
			
		}
		
		private void SelectWindow(FlowWindow window) {
			
			for (int i = 0; i < window.states.Length; ++i) window.SetCompletedState(i, CompletedState.NotReady);
			
			if (window.compiled == false) {
				
				this.ShowNotification(new GUIContent("You need to compile this window to use `Select` command"));
				
			} else {
				
				Selection.activeObject = AssetDatabase.LoadAssetAtPath(window.compiledDirectory.Trim('/'), typeof(Object));
				EditorGUIUtility.PingObject(Selection.activeObject);
				
				//if (window.screen == null) {
				
				window.SetCompletedState(0, CompletedState.NotReady);
				
				var files = AssetDatabase.FindAssets("t:GameObject", new string[] { window.compiledDirectory.Trim('/') + "/Screens" });
				foreach (var file in files) {
					
					var path = AssetDatabase.GUIDToAssetPath(file);
					
					var go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
					if (go != null) {
						
						var screen = go.GetComponent<WindowBase>();
						if (screen != null) {
							
							window.SetScreen(screen);
							window.SetCompletedState(0, CompletedState.Ready);
							
							var lWin = screen as LayoutWindowType;
							if (lWin != null) {
								
								if (lWin.layout.layout != null) {
									
									window.SetCompletedState(1, CompletedState.Ready);
									window.SetCompletedState(2, (lWin.layout.components.Any((c) => c.component == null) == true) ? CompletedState.ReadyButWarnings : CompletedState.Ready);
									
								} else {
									
									window.SetCompletedState(0, CompletedState.NotReady);
									window.SetCompletedState(1, CompletedState.NotReady);
									window.SetCompletedState(2, CompletedState.NotReady);
									
								}
								
							} else {
								
								window.SetCompletedState(1, CompletedState.Ready);
								
							}
							
							break;
							
						} else {
							
							window.SetCompletedState(0, CompletedState.ReadyButWarnings);
							
						}
						
					}
					
				}
				
				//}
				
			}
			
		}
		
		public void DrawWaitForConnection() {
			
			if (this.waitForAttach == true && this.currentAttachId >= 0) {
				
				GUIStyle style = null;
				string label = string.Empty;
				
				var window = FlowSystem.GetWindow(this.currentAttachId);
				
				if (this.currentAttachComponent != null) {
					
					var element = this.currentAttachComponent;
					
					const int maxDepth = 6;
					var styles = ME.Utilities.Cache<GUIStyle[]>("FlowEditor.Styles", () => {

						var _styles = new GUIStyle[maxDepth] {
							
							new GUIStyle("flow node 0"),
							new GUIStyle("flow node 1"),
							new GUIStyle("flow node 2"),
							new GUIStyle("flow node 3"),
							new GUIStyle("flow node 4"),
							new GUIStyle("flow node 5")
							
						};

						return _styles;

					});

					style = styles[Mathf.Clamp(element.editorDrawDepth, 0, maxDepth - 1)];
					label = window.title + ": " + element.comment;
					
				} else {
					
					style = ME.Utilities.CacheStyle("FlowEditor.Styles", "flow node 6");
					label = window.title;
					
				}
				
				var offset = 10f;
				var width = 60f;
				var height = 60f;
				
				var boxStyle = ME.Utilities.CacheStyle("FlowEditor.Styles", "flow node 6 on", (styleName) => {

					var _boxStyle = new GUIStyle(styleName);
					_boxStyle.padding = new RectOffset(20 + (int)width + (int)offset, 20, 20, 20);
					_boxStyle.wordWrap = true;
					_boxStyle.alignment = TextAnchor.MiddleCenter;
					_boxStyle.contentOffset = Vector2.zero;
					_boxStyle.stretchHeight = true;

					return _boxStyle;

				});

				var rect = this.scrollRect;
				rect.x += offset;
				rect.y = rect.height - height - offset;
				rect.width = width;
				rect.height = height;
				
				var boxRect = rect;
				boxRect.height -= 20f;
				boxRect.y += 10f;
				boxRect.width += width * 3f;
				boxRect.x += width - offset - width;
				var backColor = GUI.backgroundColor;
				var color = Color.black;
				color.a = 0.6f;
				GUI.backgroundColor = color;
				GUI.Box(boxRect, label, boxStyle);
				GUI.backgroundColor = backColor;
				
				if (GUI.Button(rect, string.Empty, style) == true) {
					
					// Cancel
					this.WaitForAttach(-1);
					
				}
				
			}
			
		}
		
		private Dictionary<int, List<FlowWindow>> bringFront = new Dictionary<int, List<FlowWindow>>();
		
		private void BringBackOrFront(FlowWindow current, IEnumerable<FlowWindow> windows) {
			
			// Is any of other window has bigger size and collide current
			foreach (var window in windows) {
				
				if (window.id != current.id) {
					
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
			
			var boxStyle = ME.Utilities.CacheStyle("FlowEditor.Settings.Styles", "miniButton", (styleName) => {

				var _boxStyle = new GUIStyle(FlowSystemEditorWindow.defaultSkin.button);
				_boxStyle.margin = new RectOffset(10, 10, 10, 10);

				return _boxStyle;

			});

			GUILayout.BeginArea(new Rect(0f, offsetY, SETTINGS_WIDTH, this.position.height - offsetY), boxStyle);
			{
				
				//var buttonStyle = new GUIStyle(EditorStyles.miniButton);
				//this.DrawToolbar(buttonStyle);

				var oldSkin = GUI.skin;
				GUI.skin = FlowSystemEditorWindow.defaultSkin;
				this.settingsWindowScroll = GUILayout.BeginScrollView(this.settingsWindowScroll, false, false);
				GUI.skin = oldSkin;

				CustomGUI.Splitter();
				GUILayout.Label("Base Modules:", EditorStyles.largeLabel);
				CustomGUI.Splitter();
				
				#region ROOT WINDOW
				Flow.DrawModuleSettingsGUI(null, "Root Window", null, () => {
					
					var rootWindow = FlowSystem.GetWindow(FlowSystem.GetRootWindow());
					if (rootWindow != null) {
						
						if (GUILayout.Button(rootWindow.title, FlowSystemEditorWindow.defaultSkin.button) == true) {

							this.SetCenterTo(rootWindow);

							this.focusedGUIWindow = rootWindow.id;
							FlowSystem.ResetSelection();
							
						}
						
					} else {
						
						GUILayout.Label("No root window selected.");
						
					}
					
				});
				#endregion
				
				#region DEFAULT WINDOWS
				Flow.DrawModuleSettingsGUI(null, "Default Windows", null, () => {
					
					if (this.defaultWindows == null) {
						
						var label = "Default Windows";
						
						this.defaultWindows = new ReorderableList(FlowSystem.GetDefaultWindows(), typeof(int), true, true, false, true);
						
						this.defaultWindows.drawHeaderCallback += rect => GUI.Label(rect, label);
						this.defaultWindows.drawElementCallback += (rect, index, active, focused) => {
							
							GUI.Label(rect, FlowSystem.GetWindow(FlowSystem.GetDefaultWindows()[index]).title);
							
						};
						this.defaultWindows.onSelectCallback += (list) => {

							var index = list.index;
							this.SetCenterTo(FlowSystem.GetWindow(FlowSystem.GetDefaultWindows()[index]));

						};
						
					}
					
					if (this.defaultWindows != null) this.defaultWindows.DoLayoutList();
					
				});
				#endregion
				
				#region TAGS
				Flow.DrawModuleSettingsGUI(null, "Tags", null, () => {
					
					if (this.tagsList == null) {
						
						var label = "Tags";

						var styles = FlowSystemEditor.GetTagStyles();

						var selected = ME.Utilities.CacheStyle("FlowEditor.Settings.Styles", "U2D.pivotDotActive");
						
						var buttonStyle = ME.Utilities.CacheStyle("FlowEditor.Settings.Styles", "miniButton", (styleName) => {

							var _buttonStyle = new GUIStyle(EditorStyles.miniButton);
							_buttonStyle.normal.background = _buttonStyle.active.background;
							_buttonStyle.active.background = null;
							_buttonStyle.focused.background = null;
							_buttonStyle.wordWrap = false;

							return _buttonStyle;

						});

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
								
								var toggleWidth = rect.height;
								
								rect.width = itemRect.x - rect.x - 5f - toggleWidth;
								rect.height -= 2f;
								rect.x += toggleWidth;
								var title = GUI.TextField(rect, tag.title, buttonStyle);
								if (title != tag.title) {
									
									tag.title = title;
									FlowSystem.SetDirty();
									
								}
								
								rect.x -= toggleWidth;
								rect.width = toggleWidth;
								
								var enabled = GUI.Toggle(rect, tag.enabled, string.Empty);
								if (enabled != tag.enabled) {
									
									tag.enabled = enabled;
									FlowSystem.SetDirty();
									
								}
								
							};
							
						}
						
					}
					
					if (this.tagsList != null) this.tagsList.DoLayoutList();
					
				});
				#endregion
				
				#region FLOW
				Flow.DrawModuleSettingsGUI(null, "Flow Settings", null, () => {
					
					var flowWindowWithLayout = GUILayout.Toggle(FlowSystem.GetData().flowWindowWithLayout, "Window With Layout");
					if (flowWindowWithLayout != FlowSystem.GetData().flowWindowWithLayout) {
						
						FlowSystem.GetData().flowWindowWithLayout = flowWindowWithLayout;
						FlowSystem.SetDirty();
						
						this.Repaint();
						
					}
					
					if (flowWindowWithLayout == true) {
						
						EditorGUIUtility.labelWidth = 50f;
						
						var flowWindowWithLayoutScaleFactor = EditorGUILayout.Slider("Scale", FlowSystem.GetData().flowWindowWithLayoutScaleFactor, 0f, 1f);
						if (flowWindowWithLayoutScaleFactor != FlowSystem.GetData().flowWindowWithLayoutScaleFactor) {
							
							FlowSystem.GetData().flowWindowWithLayoutScaleFactor = flowWindowWithLayoutScaleFactor;
							FlowSystem.SetDirty();
							
							this.Repaint();
							
						}
						
						EditorGUIUtility.LookLikeControls();
						
					}
					
				});
				#endregion
				
				#region WINDOW EDITOR
				
				//GUILayout.Label("Window Editor", EditorStyles.boldLabel);
				/*
				var editorWindowAsPopup = GUILayout.Toggle(FlowSystem.GetData().editorWindowAsPopup, "Show as popup");
				if (editorWindowAsPopup != FlowSystem.GetData().editorWindowAsPopup) {

					FlowSystem.GetData().editorWindowAsPopup = editorWindowAsPopup;
					FlowSystem.SetDirty();

				}
				*/
				#endregion
				
				CustomGUI.Splitter();
				GUILayout.Label("Installed Modules:", EditorStyles.largeLabel);
				CustomGUI.Splitter();
				
				Flow.OnDrawSettingsGUI(this);

				var copyright = ME.Utilities.CacheStyle("FlowEditor.Styles", "copyright", (styleName) => {
					
					var copyrightLabel = new GUIStyle(EditorStyles.miniLabel);
					copyrightLabel.alignment = TextAnchor.MiddleCenter;
					copyrightLabel.wordWrap = true;

					return copyrightLabel;

				});

				GUILayout.Label(string.Format("Version {0}. MIT license Alex Feer <chrome.alex@gmail.com>", VersionInfo.BUNDLE_VERSION), copyright);
				
				GUILayout.EndScrollView();
				
			}
			GUILayout.EndArea();
			
			//}, "Settings");
			
			//GUI.BringWindowToFront(-1);
			
		}
		
		private bool scrollingMouse = false;
		private bool selectionRectWait;
		private Rect selectionRect;
		private void HandleEvents(float offset) {
			
			var button = Event.current.button;
			var position =
				this.zoomDrawer.ConvertScreenCoordsToZoomCoords(Event.current.mousePosition, topLeft: true) + 
				-FlowSystem.GetScrollPosition() * 2f;
			
			if (Event.current.type == EventType.MouseDown && button == 1) {
				
				this.DrawContextMenu();
				
			}
			
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
				
				this.showTagsPopup = false;
				this.showTagsPopupId = -1;
				
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
				
				this.showTagsPopup = false;
				this.showTagsPopupId = -1;
				
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
			
			/*if (Event.current.type == EventType.MouseDrag &&
			    (Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
			    Event.current.button == 2) {
				
				var scrollPos = FlowSystem.GetScrollPosition();
				scrollPos -= Event.current.delta / this.zoomDrawer.GetZoom();
				FlowSystem.SetScrollPosition(scrollPos);
				
				Event.current.Use();
				
			}*/
			
		}
		
		private AnimatedValues.AnimFloat selectionRectAnimation;
		private void DrawBackground() {
			
			if (this._background == null) this._background = Resources.Load<Texture2D>("UI.Windows/Flow/Background");
			
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

				var selectionBoxStyle = ME.Utilities.CacheStyle("FlowEditor.Styles", "SelectionRect", (styleName) => {
					
					var _selectionBoxStyle = new GUIStyle(styleName);
					_selectionBoxStyle.margin = new RectOffset();
					_selectionBoxStyle.padding = new RectOffset();
					_selectionBoxStyle.contentOffset = Vector2.zero;

					return _selectionBoxStyle;

				});

				color = new Color(1f, 1f, 1f, this.selectionRectAnimation.value);
				
				GUI.color = color;
				GUI.Box(normalRect, string.Empty, selectionBoxStyle);
				
			}
			
			GUI.color = oldColor;
			
		}
		
		private AnimatedValues.AnimFloat scrollingMouseAnimation;
		private void DrawMinimap() {

			var oldColor = GUI.color;
			
			var elementStyle = ME.Utilities.CacheStyle("FlowEditor.Minimap.Styles", "button", (styleName) => new GUIStyle(GUI.skin.FindStyle(styleName)));
			
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
			
			var nullOffset = FlowSystemEditor.Scale(minMax, new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, Vector2.zero);
			var offset = new Vector2(Screen.width * 0.5f - SETTINGS_WIDTH * 0.5f - nullOffset.width * 0.5f, 0f);
			
			var backRect = FlowSystemEditor.Scale(minMax, new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, offset);
			
			var color = Color.black;
			color.a = backAlpha;
			GUI.color = color;
			GUI.Box(backRect, string.Empty, elementStyle);
			
			if (containers != null) {
				
				foreach (var container in containers) {
					
					color = container.randomColor;
					color.a = elementAlpha;
					GUI.color = color;
					GUI.Box(FlowSystemEditor.Scale(container.rect, new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, offset), string.Empty, elementStyle);
					
				}
				
			}
			
			color = Color.white;
			color.a = elementAlpha;
			GUI.color = color;
			
			if (windows != null) {
				
				foreach (var window in windows) {
					
					var rect = window.rect;
					if (rect.height < 60f) rect.height = 60f;
					GUI.Box(FlowSystemEditor.Scale(rect, new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, offset), string.Empty, elementStyle);
					
				}
				
			}
			
			color = Color.white;
			color.a = cameraAlpha;
			GUI.color = color;

			var zoom = this.zoomDrawer.GetZoom();
			var scrollPos = -FlowSystem.GetScrollPosition();
			var r = FlowSystemEditor.Scale(new Rect(scrollPos.x, scrollPos.y, this.scrollRect.width / zoom, this.scrollRect.height / zoom), new Rect(0f, 0f, 10000f, 10000f), this.scrollRect, offset);
			GUI.Box(r, string.Empty, elementStyle);
			
			GUI.color = oldColor;
			
		}
		
		private void DrawLoader() {
			
			this.DrawBackground();
			
			if (this.splash == null) this.splash = Resources.Load<Texture>("UI.Windows/Flow/Splash");

			var darkLabel = ME.Utilities.CacheStyle("FlowEditor.Minimap.Styles", "DarkLabel", (styleName) => {

			    var _darkLabel = FlowSystemEditorWindow.defaultSkin.FindStyle(styleName);
				_darkLabel.alignment = TextAnchor.MiddleCenter;
				_darkLabel.stretchWidth = true;
				_darkLabel.stretchHeight = true;
				_darkLabel.fixedWidth = 0f;
				_darkLabel.fixedHeight = 0f;

				return _darkLabel;
				
			});

			var rect = FlowSystemEditor.GetCenterRect(this.position, this.splash.width, this.splash.height);
			
			var boxStyle = ME.Utilities.CacheStyle("FlowEditor.Minimap.Styles", "boxStyle", (styleName) => {

				var _boxStyle = new GUIStyle(GUI.skin.box);
				_boxStyle.margin = new RectOffset(0, 0, 0, 0);
				_boxStyle.padding = new RectOffset(0, 0, 0, 0);
				_boxStyle.normal.background = null;

				return _boxStyle;

			});

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

				var labelStyle = ME.Utilities.CacheStyle("FlowEditor.Minimap.Styles", "sv_iconselector_labelselection");

				GUILayout.BeginArea(new Rect(marginLeft, margin, borderWidth, borderHeight), labelStyle);
				{
					
					GUILayout.BeginArea(new Rect(padding, padding, borderWidth - padding * 2f, borderHeight - padding * 2f));
					{
						
						GUILayout.Label("Loading...", darkLabel);
						
					}
					GUILayout.EndArea();
					
				}
				GUILayout.EndArea();
				
			}
			GUILayout.EndArea();
			
		}
		
		private Vector2 dataSelectionScroll;
		private Texture splash;
		private FlowData cachedData;
		private FlowData[] scannedData;
		private void DrawDataSelection() {
			
			this.DrawBackground();
			
			if (this.splash == null) this.splash = Resources.Load<Texture>("UI.Windows/Flow/Splash");
			
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
				
				var labelStyle = ME.Utilities.CacheStyle("FlowEditor.DataSelection.Styles", "sv_iconselector_labelselection");

				GUILayout.BeginArea(new Rect(marginLeft, margin, borderWidth, borderHeight), labelStyle);
				{
					
					GUILayout.BeginArea(new Rect(padding, padding, borderWidth - padding * 2f, borderHeight - padding * 2f));
					{
						
						var headerStyle = new GUIStyle("LODLevelNotifyText");
						headerStyle.fontSize = 18;
						headerStyle.alignment = TextAnchor.MiddleCenter;
						
						GUILayoutExt.LabelWithShadow("UI.Windows Flow Extension v" + VersionInfo.BUNDLE_VERSION, headerStyle);
						
						GUILayout.Space(10f);
						
						GUILayout.Label("Open one of your projects:", darkLabel);
						
						var backStyle = new GUIStyle("sv_iconselector_labelselection");
						
						var skin = GUI.skin;
						GUI.skin = FlowSystemEditorWindow.defaultSkin;
						this.dataSelectionScroll = GUILayout.BeginScrollView(this.dataSelectionScroll, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, backStyle);
						{
							
							GUI.skin = skin;
							
							this.scannedData = EditorUtilities.GetAssetsOfType<FlowData>();
							
							if (this.scannedData.Length == 0) {
								
								var center = new GUIStyle(darkLabel);
								center.fixedWidth = 0f;
								center.fixedHeight = 0f;
								center.stretchWidth = true;
								center.stretchHeight = true;
								center.alignment = TextAnchor.MiddleCenter;
								center.wordWrap = true;
								
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
								
								this.scannedData = this.scannedData.OrderByDescending((data) => (data != null ? data.lastModified : string.Empty)).ToArray();
								
								foreach (var data in this.scannedData) {
									
									if (data == null) continue;
									
									var title = data.name + "\n";
									title += "<color=#777><size=10>Modified: " + data.lastModified + "</size></color>\n";
									title += "<color=#888><size=10>Version: " + data.version + "</size></color>";
									
									if (GUILayout.Button(title, this.cachedData == data ? buttonStyleSelected : buttonStyle) == true) {
										
										this.cachedData = data;
										
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
							
							var oldState = GUI.enabled;
							GUI.enabled = oldState && this.cachedData != null;

							if (this.cachedData != null && this.cachedData.version < VersionInfo.BUNDLE_VERSION) {

								// Need to upgrade
								
								if (GUILayout.Button("Upgrade to " + VersionInfo.BUNDLE_VERSION, FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(150f), GUILayout.Height(40f)) == true) {
									
									UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", this.cachedData.version, VersionInfo.BUNDLE_VERSION), 0f);
									var type = this.cachedData.GetType();

									while (this.cachedData.version < VersionInfo.BUNDLE_VERSION) {

										// Try to find upgrade method
										var methodName = "UpgradeTo" + this.cachedData.version.ToSmallWithoutTypeString();
										var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
										if (methodInfo != null) {

											methodInfo.Invoke(this.cachedData, null);

											//this.cachedData.version = VersionInfo.BUNDLE_VERSION;

										} else {

											//Debug.Log("Method `" + methodName + "` was not found: version " + this.cachedData.version + " skipped");

										}

										var nextVersion = this.cachedData.version + 1;
										
										UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", this.cachedData.version, nextVersion), 0.5f);

										this.cachedData.version = nextVersion;
										UnityEditor.EditorUtility.SetDirty(this.cachedData);

									}
									
									UnityEditor.EditorUtility.DisplayProgressBar("Upgrading", string.Format("Migrating from {0} to {1}", this.cachedData.version, VersionInfo.BUNDLE_VERSION), 1f);
									UnityEditor.EditorUtility.ClearProgressBar();

								}

							} else if (this.cachedData != null && this.cachedData.version > VersionInfo.BUNDLE_VERSION) {

								EditorGUILayout.BeginHorizontal();
								{

									EditorGUILayout.HelpBox(string.Format("Selected Project has {0} version while UI.Windows System has {1} version number. Please, download a new version.", this.cachedData.version, VersionInfo.BUNDLE_VERSION), MessageType.Warning);
									if (GUILayout.Button("Download", FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(40f)) == true) {
										
										Application.OpenURL(VersionInfo.DOWNLOAD_LINK);
										
									}

								}
								EditorGUILayout.EndHorizontal();

							} else {

								if (GUILayout.Button("Open", FlowSystemEditorWindow.defaultSkin.button, GUILayout.Width(100f), GUILayout.Height(40f)) == true) {
									
									FlowSystem.SetData(this.cachedData);
									
								}

							}

							GUI.enabled = oldState;
							
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
		
		public void CreateNewItem(System.Func<FlowWindow> predicate) {
			
			var scrollPos = FlowSystem.GetScrollPosition();
			
			var window = predicate();
			window.rect.x = -(scrollPos.x - this.scrollRect.width * 0.5f + window.rect.width * 0.5f);
			window.rect.y = -(scrollPos.y - this.scrollRect.height * 0.5f + window.rect.height * 0.5f);
			
		}
		
		private void DrawToolbar() {
			
			var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.stretchWidth = false;
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			
			this.DrawToolbar(buttonStyle);
			
			GUILayout.EndHorizontal();
			
		}
		
		private void DrawContextMenu() {
			
			var menu = new GenericMenu();
			
			this.SetupCreateMenu("Create/", menu);
			this.SetupToolsMenu(string.Empty, menu);
			
			menu.ShowAsContext();
			
		}
		
		private void SetupCreateMenu(string prefix, GenericMenu menu) {
			
			menu.AddItem(new GUIContent(string.Format("{0}Window", prefix)), on: false, func: () => {
				
				this.CreateNewItem(() => FlowSystem.CreateWindow());
				
			});
			menu.AddItem(new GUIContent(string.Format("{0}Container", prefix)), on: false, func: () => {
				
				this.CreateNewItem(() => FlowSystem.CreateContainer());
				
			});
			menu.AddItem(new GUIContent(string.Format("{0}Default Link", prefix)), on: false, func: () => {
				
				this.CreateNewItem(() => FlowSystem.CreateDefaultLink());
				
			});
			
			Flow.OnDrawCreateMenuGUI(this, prefix, menu);
			
		}
		
		private void SetupToolsMenu(string prefix, GenericMenu menu) {
			
			menu.AddItem(new GUIContent(string.Format("{0}Center Screen", prefix)), on: false, func: () => {
				
				FlowSystem.SetScrollPosition(Vector2.one * -1f);
				
			});
			
			Flow.OnDrawToolsMenuGUI(this, prefix, menu);
			
		}

		public void SetCenterTo(FlowWindow window) {

			FlowSystem.SetZoom(1f);
			FlowSystem.SetScrollPosition(-new Vector2(window.rect.x - this.scrollRect.width * 0.5f, window.rect.y - this.scrollRect.height * 0.5f));

		}
		
		private void DrawToolbar(GUIStyle buttonStyle) {
			
			var result = GUILayout.Button("Create", buttonStyle);
			var rect = GUILayoutUtility.GetLastRect();
			
			if (result == true) {
				
				var menu = new GenericMenu();
				this.SetupCreateMenu(string.Empty, menu);
				
				menu.DropDown(new Rect(rect.x, rect.y + TOOLBAR_HEIGHT, rect.width, rect.height));
				
			}
			
			result = GUILayout.Button("Tools", buttonStyle);
			rect = GUILayoutUtility.GetLastRect();
			
			if (result == true) {
				
				var menu = new GenericMenu();
				this.SetupToolsMenu(string.Empty, menu);
				
				menu.DropDown(new Rect(rect.x, rect.y + TOOLBAR_HEIGHT, rect.width, rect.height));
				
			}
			
			Flow.OnDrawToolbarGUI(this, buttonStyle);
			
			GUILayout.FlexibleSpace();
			
			var oldColor = GUI.color;
			GUI.color = Color.gray;
			GUILayout.Label(string.Format("Current Data: {0}", AssetDatabase.GetAssetPath(this.cachedData)), buttonStyle);
			GUI.color = oldColor;
			
			if (GUILayout.Button("Change", buttonStyle) == true) {
				
				this.ChangeFlowData();
				
			}
			
		}
		
		private void DrawNodeContainer(int id) {
			
			EditorGUIUtility.labelWidth = 65f;
			
			GUI.enabled = true;//!FlowSceneView.IsActive();
			
			var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.stretchWidth = false;
			
			var window = FlowSystem.GetWindow(id);
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			{
				
				if (this.waitForAttach == true) {
					
					if (this.currentAttachComponent == null) {
						
						if (id != this.currentAttachId) {
							
							var currentAttach = FlowSystem.GetWindow(this.currentAttachId);
							if (currentAttach != null) {
								
								var attachTo = FlowSystem.GetWindow(id);
								var hasContainer = currentAttach.HasContainer();
								var container = currentAttach.GetContainer();
								
								if ((attachTo.IsContainer() == true && currentAttach.IsContainer() == true && attachTo == container) || (hasContainer == true && container.id != id)) {
									
									
									
								} else {
									
									if (attachTo.IsContainer() == true && currentAttach.IsContainer() == true) {
										
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
								
							}
							
						} else {
							
							if (GUILayout.Button("Cancel", buttonStyle) == true) {
								
								this.WaitForAttach(-1);
								
							}
							
						}
						
					}
					
				} else {
					
					if (GUILayout.Button("Attach/Detach", buttonStyle) == true) {
						
						this.ShowNotification(new GUIContent("Use Attach/Detach buttons to connect/disconnect the container"));
						this.WaitForAttach(id);
						
					}
					
					if (GUILayout.Button("Destroy", buttonStyle) == true) {
						
						if (EditorUtility.DisplayDialog("Are you sure?", "Current container will be destroyed with all links (All windows will be saved)", "Yes, destroy", "No") == true) {
							
							this.ShowNotification(new GUIContent(string.Format("The container `{0}` was successfully destroyed", window.title)));
							FlowSystem.DestroyWindow(id);
							return;
							
						}
						
					}
					
				}
				
				GUILayout.FlexibleSpace();
				
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				
				GUILayoutExt.LabelWithShadow("Title:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
				window.title = GUILayout.TextField(window.title);
				
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				
				GUILayoutExt.LabelWithShadow("Directory:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
				window.directory = GUILayout.TextField(window.directory);
				
			}
			GUILayout.EndHorizontal();
			
			this.DrawTags(window);
			
			if (window.IsFunction() == true) {
				
				// Draw entrance point and an exit point
				//window.rect.x;
				
				var rootStyle = ME.Utilities.CacheStyle("FlowEditor.DrawNodeContainer.Styles", "flow node hex 4");
				var rect = new Rect(-rootStyle.normal.background.width * 0.5f, window.rect.height * 0.5f, rootStyle.normal.background.width, rootStyle.normal.background.height);
				
				if (GUI.Button(rect, string.Empty, rootStyle) == true) {
					
					// Setup for wait for attach
					
				}
				
				var exitStyle = ME.Utilities.CacheStyle("FlowEditor.DrawNodeContainer.Styles", this.waitForAttach == true ? "flow node hex 6 on" : "flow node hex 3");
				rect = new Rect(-exitStyle.normal.background.width * 0.5f + window.rect.width, window.rect.height * 0.5f, exitStyle.normal.background.width, exitStyle.normal.background.height);
				
				if (GUI.Button(rect, string.Empty, exitStyle) == true) {
					
					if (this.waitForAttach == true) {
						
						// Attach/Detach window from this function
						window.functionExitId = this.currentAttachId;
						
						this.currentAttachId = 0;
						this.waitForAttach = false;
						
					}
					
				}
				
			}
			
			var attaches = window.attachItems.Count;
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
			
			var tagStyles = FlowSystemEditor.GetTagStylesEdited();

			var tagStyle = tagStyles[0];
			tagStyle.stretchWidth = false;
			tagStyle.margin = new RectOffset(0, 0, 2, 2);
			
			var shadow = ME.Utilities.CacheStyle("FlowEditor.DrawTagsPopup.Styles", "ObjectPickerPreviewBackground");
			
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
					
					var scrollPos = -FlowSystem.GetScrollPosition();
					var drawRect = new Rect(r.x - scrollPos.x, r.y - scrollPos.y - topOffset, r.width, r.height);
					
					tagStyle.fixedWidth = drawRect.width;
					
					GUI.BeginGroup(drawRect);
					{
						GUI.color = Color.black;
						GUI.Label(new Rect(10f, backTopOffset, drawRect.width - 10f * 2f, drawRect.height - backTopOffset * 2f), string.Empty, shadow);

						var buttonRect = new Rect(0f, topOffset, drawRect.width, elementHeight);

						GUI.color = oldColor;
						foreach (var tag in allTags) {
							
							if (tag.title.ToLower().Contains(this.tagCaption.ToLower()) == true && window != null && window.tags.Contains(tag.id) == false) {
								
								var style = tagStyles[tag.color];
								if (GUI.Button(buttonRect, tag.title, style) == true) {
									
									this.tagCaption = string.Empty;
									this.showTagsPopupId = -1;
									
									window.AddTag(tag);
									
								}

								buttonRect.y += elementHeight;
								
							}
							
						}
					}
					GUI.EndGroup();
					
				}
				
			}
			
			GUI.color = oldColor;
			
			this.Repaint();
			
		}
		
		private bool showTagsPopup = false;
		private Rect showTagsPopupRect;
		private int showTagsPopupId;
		private string tagCaption = string.Empty;
		private void DrawTags(FlowWindow window, bool defaultWindow = false) {

			EditorGUIUtility.labelWidth = 35f;
			
			var tagStyles = FlowSystemEditor.GetTagStyles();

			var tagCaptionStyleText = ME.Utilities.CacheStyle("FlowEditor.DrawTags.Styles", "sv_label_0");

			var tagCaptionStyle = ME.Utilities.CacheStyle("FlowEditor.DrawTags.Styles", "tagCaptionStyle", (styleName) => {

				var _tagCaptionStyle = new GUIStyle(GUI.skin.textField);
				_tagCaptionStyle.alignment = TextAnchor.MiddleCenter;
				_tagCaptionStyle.fixedWidth = 90f;
				_tagCaptionStyle.fixedHeight = tagCaptionStyleText.fixedHeight;
				_tagCaptionStyle.stretchWidth = false;
				_tagCaptionStyle.font = tagCaptionStyleText.font;
				_tagCaptionStyle.fontStyle = tagCaptionStyleText.fontStyle;
				_tagCaptionStyle.fontSize = tagCaptionStyleText.fontSize;
				_tagCaptionStyle.normal = tagCaptionStyleText.normal;
				_tagCaptionStyle.focused = tagCaptionStyleText.normal;
				_tagCaptionStyle.active = tagCaptionStyleText.normal;
				_tagCaptionStyle.hover = tagCaptionStyleText.normal;
				_tagCaptionStyle.border = tagCaptionStyleText.border;
				//_tagCaptionStyle.padding = tagCaptionStyleText.padding;
				//_tagCaptionStyle.margin = tagCaptionStyleText.margin;
				_tagCaptionStyle.margin = new RectOffset();

				return _tagCaptionStyle;

			});
			
			var tagStyleAdd = ME.Utilities.CacheStyle("FlowEditor.DrawTags.Styles", "sv_label_3", (styleName) => {
				
				var _tagStyleAdd = new GUIStyle(styleName);
				_tagStyleAdd.margin = new RectOffset(0, 0, 0, 0);
				_tagStyleAdd.padding = new RectOffset(3, 5, 0, 2);
				_tagStyleAdd.alignment = TextAnchor.MiddleCenter;
				_tagStyleAdd.stretchWidth = false;

				return _tagStyleAdd;

			});

			var tagsLabel = ME.Utilities.CacheStyle("FlowEditor.DrawTags.Styles", "defaultLabel", (styleName) => {
				
				var _tagsLabel = new GUIStyle(FlowSystemEditorWindow.defaultSkin.label);
				_tagsLabel.padding = new RectOffset(_tagsLabel.padding.left, _tagsLabel.padding.right, _tagsLabel.padding.top, _tagsLabel.padding.bottom + 4);

				return _tagsLabel;

			});

			var changed = false;
			
			GUILayout.BeginHorizontal();
			{
				GUILayoutExt.LabelWithShadow("Tags:", tagsLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
				
				GUILayout.BeginVertical();
				{
					GUILayout.Space(4f);
					
					var tagCaption = string.Empty;
					if (this.showTagsPopupId == window.id) tagCaption = this.tagCaption;
					
					var isEnter = (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return);
					
					GUILayout.BeginVertical();
					{
						
						GUILayout.BeginHorizontal();
						{
							
							var columns = 3;
							var i = 0;
							foreach (var tag in window.tags) {
								
								if (i % columns == 0) {
									
									GUILayout.EndHorizontal();
									GUILayout.BeginHorizontal();
									
								}
								
								var tagInfo = FlowSystem.GetData().GetTag(tag);
								if (tagInfo == null) {
									
									window.tags.Remove(tag);
									break;
									
								}
								
								if (GUILayout.Button(tagInfo.title, tagStyles[tagInfo.color]) == true) {
									
									FlowSystem.RemoveTag(window, tagInfo);
									break;
									
								}
								
								++i;
								
							}
							
							if (i % columns != 0) GUILayout.FlexibleSpace();
							
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						{
							
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
										
										this.showTagsPopupRect = new Rect(window.rect.x + rect.x + SETTINGS_WIDTH, window.rect.y + rect.y + (defaultWindow == true ? window.rect.height : 0f), rect.width, rect.height);
										
									}
									
									if (changed == true) this.Repaint();
									
								}
								
							}
							
						}
						GUILayout.EndHorizontal();
						
					}
					GUILayout.EndVertical();
					
				}
				GUILayout.EndVertical();
				
			}
			GUILayout.EndHorizontal();
			
			if (changed == true) {
				
				this.Repaint();
				
			}
			
			EditorGUIUtility.LookLikeControls();
			
		}
		
		private void DrawStates(CompletedState[] states, FlowWindow window) {
			
			if (states == null) return;
			
			var oldColor = GUI.color;
			var style = ME.Utilities.CacheStyle("FlowEditor.DrawStates.Styles", "Grad Down Swatch");
			
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
			
			//var edit = false;
			var id = window.id;

			var buttonStyle = ME.Utilities.CacheStyle("FlowEditor.DrawWindowToolbar.Styles", "toolbarButton", (name) => {

				var _buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
				_buttonStyle.stretchWidth = false;

				return _buttonStyle;

			});

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			if (this.waitForAttach == false || this.currentAttachComponent == null) {
				
				if (this.waitForAttach == true) {
					
					if (id != this.currentAttachId) {
						
						var currentAttach = FlowSystem.GetWindow(this.currentAttachId);
						if (currentAttach != null) {
							
							//var attachTo = FlowSystem.GetWindow(id);
							//var hasContainer = currentAttach.HasContainer();
							
							if (currentAttach.IsContainer() == false) {
								
								if (FlowSystem.AlreadyAttached(this.currentAttachId, id) == true) {
									
									if (GUILayout.Button(string.Format("Detach Here{0}", (Event.current.alt == true ? " (Double Direction)" : string.Empty)), buttonStyle) == true) {
										
										FlowSystem.Detach(this.currentAttachId, id, oneWay: Event.current.alt == false);
										if (Event.current.shift == false) this.WaitForAttach(-1);
										
									}
									
								} else {
									
									if (GUILayout.Button(string.Format("Attach Here{0}", (Event.current.alt == true ? " (Double Direction)" : string.Empty)), buttonStyle) == true) {
										
										FlowSystem.Attach(this.currentAttachId, id, oneWay: Event.current.alt == false);
										if (Event.current.shift == false) this.WaitForAttach(-1);
										
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
					
					if (window.IsSmall() == false ||
					    window.IsFunction() == true) {
						
						if (GUILayout.Button("Attach/Detach", buttonStyle) == true) {
							
							this.ShowNotification(new GUIContent("Use Attach/Detach buttons to Connect/Disconnect a window"));
							this.WaitForAttach(id);
							
						}
						
					}
					
					if (GUILayout.Button("Destroy", buttonStyle) == true) {
						
						if (EditorUtility.DisplayDialog("Are you sure?", "Current window will be destroyed with all links", "Yes, destroy", "No") == true) {
							
							this.ShowNotification(new GUIContent(string.Format("The window `{0}` was successfully destroyed", window.title)));
							FlowSystem.DestroyWindow(id);
							return;
							
						}
						
					}
					
				}
				
				if (window.IsSmall() == false) {
					
					//var isExit = false;
					
					var functionWindow = window.GetFunctionContainer();
					if (functionWindow != null) {
						
						if (functionWindow.functionRootId == 0) functionWindow.functionRootId = id;
						if (functionWindow.functionExitId == 0) functionWindow.functionExitId = id;
						
						//isExit = (functionWindow.functionExitId == id);
						
					}
					
					var isRoot = (FlowSystem.GetRootWindow() == id || (functionWindow != null && functionWindow.functionRootId == id));
					if (GUILayout.Toggle(isRoot, new GUIContent("R", "Set as root"), buttonStyle) != isRoot) {
						
						if (functionWindow != null) {
							
							if (isRoot == true) {
								
								// Was root
								// Setup root for the first window in function
								functionWindow.functionRootId = window.id;
								
							} else {
								
								// Was not root
								// Setup as root but inside this function only
								functionWindow.functionRootId = window.id;
								
							}
							
						} else {
							
							if (isRoot == true) {
								
								// Was root
								FlowSystem.SetRootWindow(-1);
								
							} else {
								
								// Was not root
								FlowSystem.SetRootWindow(id);
								
							}
							
						}
						
						FlowSystem.SetDirty();
						
					}
					/*
					if (functionWindow != null) {

						if (GUILayout.Toggle(isExit, new GUIContent("E", "Set as exit point"), buttonStyle) != isExit) {

							if (isExit == true) {
								
								// Was exit
								// Setup exit for the first window in function
								functionWindow.functionExitId = window.id;
								
							} else {
								
								// Was not exit
								// Setup as exit but inside this function only
								functionWindow.functionExitId = window.id;
								
							}

							FlowSystem.SetDirty();
							
						}

					}*/
					
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
				
				if (window.IsSmall() == false && FlowSceneView.IsActive() == false && window.storeType == FlowWindow.StoreType.NewScreen) {
					
					if (GUILayout.Button("Select", buttonStyle) == true) {
						
						this.SelectWindow(window);
						
					}
					
					/*
					if (GUILayout.Button("Edit", buttonStyle) == true) {
						
						if (window.compiled == false) {
							
							this.ShowNotification(new GUIContent("You need to compile this window to use 'Edit' command"));
							
						} else {
							
							edit = true;
							
						}
						
					}*/
					
				}
				
			} else {
				
				// Draw Attach/Detach component link
				
				if (this.currentAttachId == id) {
					
					// Cancel
					if (GUILayout.Button("Cancel", buttonStyle) == true) {
						
						this.WaitForAttach(-1);
						
					}
					
				} else {
					
					// If it's other window
					if (window.IsSmall() == false ||
					    window.IsFunction() == true) {
						
						if (FlowSystem.AlreadyAttached(this.currentAttachId, id, this.currentAttachComponent) == true) {
							
							if (GUILayout.Button("Detach Here", buttonStyle) == true) {
								
								FlowSystem.Detach(this.currentAttachId, id, oneWay: true, component: this.currentAttachComponent);
								if (Event.current.shift == false) this.WaitForAttach(-1);
								
							}
							
						} else {
							
							if (GUILayout.Button("Attach Here", buttonStyle) == true) {
								
								FlowSystem.Attach(this.currentAttachId, id, oneWay: true, component: this.currentAttachComponent);
								if (Event.current.shift == false) this.WaitForAttach(-1);
								
							}
							
						}
						
					}
					
				}
				
				GUILayout.FlexibleSpace();
				
			}
			GUILayout.EndHorizontal();
			
			/*if (edit == true) {
				
				FlowSceneView.SetControl(this, window, this.OnItemProgress);

			}*/
			
		}
		
		private void DrawNodeWindow(int id) {
			
			var window = FlowSystem.GetWindow(id);

			EditorGUIUtility.labelWidth = 65f;
			
			this.UpdateFocus(id);

			var zoom = this.zoomDrawer.GetZoom();
			if (zoom <= 0.3f) {

				var style = ME.Utilities.CacheStyle("FlowEditor.NodeWindow.Styles", "NodeWindow", (name) => {

					var _style = new GUIStyle(FlowSystemEditorWindow.defaultSkin.button);
					_style.fontSize = 40;

					return _style;

				});

				GUI.Label(new Rect(0f, 0f, window.rect.width, window.rect.height), window.title, style);

			} else {

				if (window.IsSmall() == true) {
					
					this.DrawWindowToolbar(window);
					Flow.OnDrawWindowGUI(this, window);
					
					this.DragWindow(headerOnly: false);
					
				} else {

					if (window.storeType == FlowWindow.StoreType.NewScreen) {
						
						this.DrawStates(window.states, window);
						
					} else {
						
						var win = FlowSystem.GetWindow(window.screenWindowId);
						if (win != null) this.DrawStates(win.states, win);
						
					}
					
					GUI.enabled = true;//!FlowSceneView.IsActive();
					
					this.DrawWindowToolbar(window);
					
					GUILayout.BeginHorizontal();
					{
						
						GUILayoutExt.LabelWithShadow("Title:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
						window.title = GUILayout.TextField(window.title);
						
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					{
						
						var typeId = (int)window.storeType;
						typeId = GUILayoutExt.Popup(typeId, new string[1] { "Directory:"/*, "Re-use Screen:"*/ }, FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
						if (typeId == 0) {
							
							// Directory choosed
							
							window.flags &= (~FlowWindow.Flags.CantCompiled);
							window.storeType = FlowWindow.StoreType.NewScreen;
							
							if (string.IsNullOrEmpty(window.directory) == true)
								window.directory = string.Empty;
							window.directory = GUILayout.TextField(window.directory);
							
						} else if (typeId == 1) {
							
								// Re-use screen choosed
							
								window.flags |= FlowWindow.Flags.CantCompiled;
								window.storeType = FlowWindow.StoreType.ReUseScreen;
							
								var linkIndex = -1;
								var index = 0;
								var values = new List<string>();
								var list = new List<FlowWindow>();
								var windows = FlowSystem.GetWindows();
								foreach (var win in windows) {
								
									if (win.storeType == FlowWindow.StoreType.NewScreen &&
										win.IsSmall() == false &&
										win.IsContainer() == false) {
									
										values.Add(win.title.Replace("/", " "));
										list.Add(win);
									
										if (window.screenWindowId == win.id)
											linkIndex = index;
									
										++index;
									
									}
								
								}
							
								linkIndex = EditorGUILayout.Popup(linkIndex, values.ToArray()/*, new GUIStyle("ExposablePopupMenu")*/);
								if (linkIndex >= 0 && linkIndex < list.Count) {
								
									window.screenWindowId = list[linkIndex].id;
								
								}
							
								//window.screen = GUILayoutExt.ScreenField(window.screen, false, FlowSystemEditorWindow.defaultSkin.FindStyle("ScreenField"));
								//window.directory = GUILayout.TextField(window.directory);
							
							}
						//GUILayoutExt.LabelWithShadow("Directory:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
						
					}
					GUILayout.EndHorizontal();

					this.DrawWindowLayout(window);
					Flow.OnDrawWindowGUI(this, window);

				}

			}
			
			this.DragWindow(headerOnly: false);
			
			if (GUI.changed == true) this.cachedData.isDirty = true;
			
			GUI.enabled = true;

			EditorGUIUtility.LookLikeControls();
			
		}
		
		//private float itemProgress;
		public void OnItemProgress(float value) {
			
			//this.itemProgress = value;
			this.Repaint();
			
		}
		
		private int currentAttachId = -1;
		private bool waitForAttach = false;
		private WindowLayoutElement currentAttachComponent;
		private void WaitForAttach(int id, WindowLayoutElement currentAttachComponent = null) {
			
			this.currentAttachId = id;
			this.currentAttachComponent = currentAttachComponent;
			this.waitForAttach = id >= 0;
			
			if (this.waitForAttach == false) {
				
				WindowLayoutElement.waitForComponentConnectionElementTemp = null;
				
			}
			
		}

	}

}