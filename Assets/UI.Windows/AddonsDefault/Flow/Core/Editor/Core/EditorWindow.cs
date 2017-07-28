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
using System.IO;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEngine.UI.Windows.Audio;
using UnityEditor.UI.Windows.Audio;
using UnityEngine.UI.Windows.Animations;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowSystemEditorWindow : EditorWindowExt {
		
		public class Styles {
			
			public GUISkin skin;
			public GUIStyle layoutBoxStyle;
			
			public Styles() {
				
				this.skin = UnityEngine.Resources.Load(string.Format("UI.Windows/Flow/Styles/Skin{0}", (EditorGUIUtility.isProSkin == true ? "Dark" : "Light"))) as GUISkin;
				if (this.skin != null) {

					this.layoutBoxStyle = this.skin.FindStyle("LayoutBox");

				}

			}

			public bool IsValid() {

				return this.skin != null && this.layoutBoxStyle != null;

			}

		}

		public static Styles styles = null;

		public static GUISkin defaultSkin {

			get {

				if (FlowSystemEditorWindow.styles == null) FlowSystemEditorWindow.styles = new Styles();
				return FlowSystemEditorWindow.styles.skin;

			}

		}

		public static bool loaded = false;
		public static bool loading = false;

		private GUIDrawer guiDrawer;
		private FlowSplash guiSplash;
		public EditorZoomArea zoomDrawer;

		public static FlowSystemEditorWindow ShowEditor(System.Action onClose) {
			
			var editor = FlowSystemEditorWindow.GetWindow<FlowSystemEditorWindow>(typeof(SceneView));
			editor.autoRepaintOnSceneChange = true;
			editor.onClose = onClose;
			editor.wantsMouseMove = true;
			//editor.antiAlias = 0;

			var title = "UIW Flow";
			#if !UNITY_4
			editor.titleContent = new GUIContent(title, UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/FlowIcon"));
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

			if (FlowSystemEditorWindow.loaded == false) {
				
				if (FlowSystemEditorWindow.loading == true) {
					
					return;
					
				}

				FlowSystemEditorWindow.loading = true;
				
				EditorApplication.delayCall += () => {

					EditorApplication.delayCall = null;

					// Cache
					ME.EditorUtilities.GetAssetsOfType<FlowData>(useCache: false);
					ME.EditorUtilities.GetPrefabsOfType<FlowWindowLayoutTemplate>(useCache: false);
					ME.EditorUtilities.GetPrefabsOfType<FlowLayoutWindowTypeTemplate>(useCache: false);
					ME.EditorUtilities.GetPrefabsOfType<WindowModule>(strongType: false, useCache: false);
					
					CoreUtilities.LoadAddons(forced: true);

					FlowSystemEditorWindow.settingsWidth = EditorPrefs.GetFloat("UI.Windows.Editor.Settings.Width", 280f);

					FlowSystemEditorWindow.loading = false;
					FlowSystemEditorWindow.loaded = true;

				};

			}
			
		}

		public const float TOOLBAR_HEIGHT = 18f;

		private static float settingsWidth = 280f;

		public static float GetSettingsWidth() {

			return FlowSystemEditorWindow.settingsWidth;

		}

		private Texture2D _background;
		private List<int> tempAttaches = new List<int>();
		private void OnGUI() {
			
			if (this.guiDrawer == null) this.guiDrawer = new GUIDrawer(this);
			if (this.guiSplash == null) this.guiSplash = new FlowSplash(this);
			if (this.zoomDrawer == null) this.zoomDrawer = new EditorZoomArea(this);
			if (FlowSystemEditorWindow.styles == null || FlowSystemEditorWindow.styles.IsValid() == false) FlowSystemEditorWindow.styles = new Styles();

			GUI.enabled = this.IsEnabled();

			if (this.guiSplash.Draw() == false) {

				return;

			}

			//var draw = !FlowSceneView.IsActive();
			
			//if (draw == true) {
			
			this.contentRect = this.position;
			this.contentRect.x = 0f;
			this.contentRect.y = 0f;
			this.contentRect.width = 10000f;
			this.contentRect.height = 10000f;
			this.contentRect.height -= scrollSize;

			var hasData = FlowSystem.HasData();
			if (hasData == true) {

				Handles.BeginGUI();

				var oldEnabled = GUI.enabled;
				GUI.enabled = FlowSystem.HasData() && GUI.enabled;
				this.DrawToolbar();
				GUI.enabled = oldEnabled;

				this.DrawSettings(TOOLBAR_HEIGHT);
				
				IEnumerable<FD.FlowWindow> windows = null;
				IEnumerable<FD.FlowWindow> containers = null;
				if (hasData == true) {
					
					windows = FlowSystem.GetWindows();
					containers = FlowSystem.GetContainers();
					
				}
				
				this.scrollRect = this.position;
				this.scrollRect.x = FlowSystemEditorWindow.GetSettingsWidth();
				this.scrollRect.y = TOOLBAR_HEIGHT;
				this.scrollRect.width -= FlowSystemEditorWindow.GetSettingsWidth();
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

							foreach (var window in windows) {

								window.isMovingState = false;

							}

							this.tempAttaches.Clear();
							foreach (var window in windows) {

								//if (this.IsVisible(window) == false) continue;

								var attaches = window.attachItems;
								foreach (var attachItem in attaches) {

									var attachId = attachItem.targetId;

									var curWindow = FlowSystem.GetWindow(attachId);
									//if (this.IsVisible(curWindow) == false) continue;

									if (curWindow.IsContainer() == true &&
									    curWindow.IsFunction() == false) continue;

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
										
										this.guiDrawer.DrawNodeCurve(attachItem, window, curWindow, doubleSided);

										// Draw Transition Chooser
										this.DrawTransitionChooser(attachItem, window, curWindow, doubleSided);

									}
									
								}
								
								this.tempAttaches.Add(window.id);
								
							}

							this.bringFront.Clear();
							
							var selectionMain = -1;
							var selected = FlowSystem.GetSelected();
							
							this.BeginWindows();
							
							var containerPadding = new Vector4(40f, 100f, 40f, 40f);
							foreach (var container in containers) {
								
								if (this.IsVisible(container) == false) continue;

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
									var count = 0;
									foreach (var attachItem in attaches) {

										var attachId = attachItem.targetId;
										if (attachId == container.id) continue;

										var window = FlowSystem.GetWindow(attachId);

										minX = Mathf.Min(minX, window.rect.xMin);
										minY = Mathf.Min(minY, window.rect.yMin);
										maxX = Mathf.Max(maxX, window.rect.xMax);
										maxY = Mathf.Max(maxY, window.rect.yMax);

										++count;

									}

									if (count > 0) {

										var r = new Rect();
										r.min = new Vector2(minX - containerPadding.x, minY - containerPadding.y);
										r.max = new Vector2(maxX + containerPadding.z, maxY + containerPadding.w);

										container.rect = r;

									}

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

							foreach (var window in windows) {

								if (this.IsVisible(window) == false) continue;

								var title = string.Empty;
								if (window.IsSmall() == true) {
									
									title = window.title;
									
								} else {

									if (FlowSystem.GetData().modeLayer != ModeLayer.Flow) {

										title = window.title;

									}

									var size = this.GetWindowSize(window);
									
									window.rect.width = size.x;
									window.rect.height = size.y;
									
								}
								
								var isSelected = selected.Contains(window.id) || (selected.Count == 0 && this.focusedGUIWindow == window.id);
								var style = window.GetEditorStyle(isSelected);

								var rect = window.rect;
								rect = GUI.Window(window.id, rect, this.DrawNodeWindow, title, style);
								GUI.BringWindowToFront(window.id);

								if (FlowSystem.GetData().flowDrawTags == true) {

									GUI.Window(-window.id, new Rect(rect.x, rect.y + rect.height, rect.width, this.GetTagsHeight(window)), (id) => {

										this.DrawTags(FlowSystem.GetWindow(-id), true);

									}, string.Empty, GUIStyle.none);
									GUI.BringWindowToFront(-window.id);

								}

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

												if (window.IsContainer() == false) {

													window.rect = newRect.PixelPerfect();

												}

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

							if (FlowSystem.GetData() != null && FlowSystem.GetData().HasView(FlowView.Layout) == true) {
								
								foreach (var window in windows) {

									if (this.IsVisible(window) == false) continue;

									var components = window.attachedComponents;
									for (int i = 0; i < components.Count; ++i) {
										
										var component = components[i];
										this.guiDrawer.DrawComponentCurve(window, ref component, FlowSystem.GetWindow(component.targetWindowId));
										components[i] = component;
										
									}
									
								}
								
							}

						}

						this.tempAttaches.Clear();
						if (hasData == true && windows != null) {

							foreach (var container in containers) {

								if (this.IsVisible(container) == false) continue;

								var attaches = container.attachItems;
								foreach (var attachItem in attaches) {

									var curWindow = FlowSystem.GetWindow(attachItem.targetId);
									curWindow.parentId = container.id;

								}

							}

							foreach (var window in windows) {

								if (this.IsVisible(window) == false) continue;

								var attaches = window.attachItems;
								foreach (var attachItem in attaches) {
									
									var attachId = attachItem.targetId;
									
									var curWindow = FlowSystem.GetWindow(attachId);
									if (this.IsVisible(curWindow) == false) continue;

									if (curWindow.IsContainer() == true &&
									    curWindow.IsFunction() == false) continue;
									
									if (curWindow.IsFunction() == true &&
									    curWindow.IsContainer() == true) {

									} else {
										
										var doubleSided = FlowSystem.AlreadyAttached(attachId, window.id);
										if (this.tempAttaches.Contains(attachId) == true && doubleSided == true) continue;

										Vector2 centerOffset = Flow.OnDrawNodeCurveOffset(this, attachItem, window, curWindow, doubleSided);

										Flow.OnFlowWindowTransition(this, attachItem.index, window, curWindow, doubleSided, centerOffset);

									}
									
								}

								Flow.OnDrawWindow(this, window);

								this.tempAttaches.Add(window.id);
								
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

				Flow.OnDrawGUI(this);

				if (this.scrollingMouseAnimation != null && this.scrollingMouseAnimation.isAnimating == true || this.scrollingMouse == true) this.DrawMinimap();

				Handles.EndGUI();

			}

		}

		public bool ContainsRect(Rect sourceRect) {
			
			if (this.zoomDrawer.GetZoom() < 1f) {
				
				return true;
				
			}
			
			var scrollPos = -FlowSystem.GetScrollPosition();
			var rect = new Rect(scrollPos.x - this.scrollRect.width * 0.5f + this.scrollRect.x,
			                    scrollPos.y + this.scrollRect.y,
			                    this.scrollRect.width + FlowSystemEditorWindow.GetSettingsWidth(),
			                    this.scrollRect.height);
			
			var scaledRect = rect.ScaleSizeBy(this.zoomDrawer.GetZoom());
			var scaledWin = sourceRect.ScaleSizeBy(this.zoomDrawer.GetZoom());

			return scaledRect.Overlaps(scaledWin);

		}

		public bool IsVisible(FD.FlowWindow window) {

			if (window.isMovingState == true) return true;

			if (this.zoomDrawer.GetZoom() < 1f) {

				return true;

			}

			window.isVisibleState = this.ContainsRect(window.rect);
			return window.isVisibleState;

		}
		
		public Vector2 GetWindowSize(FD.FlowWindow window) {
			
			var flowWindowWithLayout = FlowSystem.GetData().HasView(FlowView.Layout);
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
		
		public void DrawTransitionChooser(AttachItem attach, FD.FlowWindow fromWindow, FD.FlowWindow toWindow, bool doubleSided) {
			
			if (this.drawWindowContent == false) return;

			if (toWindow.IsEnabled() == false) return;
			if (toWindow.IsContainer() == true) return;

			var factor = 0.5f;
			var transitionsContainer = fromWindow;
			var namePrefix = string.Empty;

			if (fromWindow.IsSmall() == true &&
				fromWindow.IsABTest() == true) {

				// is ABTest
				//Debug.Log(fromWindow.id + " => " + toWindow.id + " :: " + attach.index + " :: " + doubleSided);
				transitionsContainer = FlowSystem.GetWindow(fromWindow.abTests.sourceWindowId);
				if (transitionsContainer == null) return;

				namePrefix = string.Format("Variant{0}", attach.index.ToString());
				factor = 0.2f;

			} else {

				if (toWindow.IsSmall() == true) {

					if (toWindow.IsFunction() == false) return;

				}

			}

			if (FlowSystem.GetData().modeLayer == ModeLayer.Audio) {

				if (FlowSystem.GetData().HasView(FlowView.AudioTransitions) == false) return;

			} else {
				
				if (FlowSystem.GetData().HasView(FlowView.VideoTransitions) == false) return;

			}

			const float size = 32f;
			const float offset = size * 0.5f + 5f;

			Vector2 centerOffset = Flow.OnDrawNodeCurveOffset(this, attach, fromWindow, toWindow, doubleSided);

			if (doubleSided == true) {

				var q = Quaternion.LookRotation(toWindow.rect.center - fromWindow.rect.center, Vector3.back);
				var attachRevert = FlowSystem.GetAttachItem(toWindow.id, fromWindow.id);
				
				this.DrawTransitionChooser(attachRevert, toWindow, toWindow, fromWindow, centerOffset, q * Vector2.left * offset, size, factor, namePrefix);
				this.DrawTransitionChooser(attach, fromWindow, fromWindow, toWindow, centerOffset, q * Vector2.right * offset, size, factor, namePrefix);

			} else {

				this.DrawTransitionChooser(attach, transitionsContainer, fromWindow, toWindow, centerOffset, Vector2.zero, size, factor, namePrefix);

			}

		}

		public void DrawTransitionChooser(AttachItem attach, FD.FlowWindow transitionsContainer, FD.FlowWindow fromWindow, FD.FlowWindow toWindow, Vector2 centerOffset, Vector2 offset, float size, float factor = 0.5f, string namePrefix = "") {

			var _size = Vector2.one * size;
			var rect = new Rect(Vector2.Lerp(fromWindow.rect.center + centerOffset, toWindow.rect.center, factor) + offset - _size * 0.5f, _size);

			var icon = "TransitionIcon";
			if (FlowSystem.GetData().modeLayer == ModeLayer.Audio) {

				icon = "TransitionIconAudio";

			}

			var transitionStyle = ME.Utilities.CacheStyle("UI.Windows.Styles.DefaultSkin", icon, (name) => FlowSystemEditorWindow.defaultSkin.FindStyle(name));
			var transitionStyleBorder = ME.Utilities.CacheStyle("UI.Windows.Styles.DefaultSkin", "TransitionIconBorder", (name) => FlowSystemEditorWindow.defaultSkin.FindStyle(name));
			if (transitionStyle != null && transitionStyleBorder != null) {

				//if (fromWindow.GetScreen() != null) {

				System.Action onClick = () => {

					if (FlowSystem.GetData().modeLayer == ModeLayer.Flow) {

						FlowChooserFilter.CreateTransition<TransitionVideoInputTemplateParameters>(transitionsContainer, fromWindow, toWindow, attach.index, "/Transitions", namePrefix, (element) => {
							
							FlowSystem.Save();
							
						});

					} else if (FlowSystem.GetData().modeLayer == ModeLayer.Audio) {
						
						FlowChooserFilter.CreateTransition<TransitionAudioInputTemplateParameters>(transitionsContainer, fromWindow, toWindow, attach.index, "/Transitions", namePrefix, (element) => {
							
							FlowSystem.Save();
							
						});

					}

				};

				// Has transition or not?
				TransitionBase transition = null;
				TransitionInputParameters transitionParameters  = null;
				IPreviewEditor editor = null;
				if (FlowSystem.GetData().modeLayer == ModeLayer.Flow) {

					transition = attach.transition;
					transitionParameters = attach.transitionParameters;
					editor = attach.editor;

				} else if (FlowSystem.GetData().modeLayer == ModeLayer.Audio) {
					
					transition = attach.audioTransition;
					transitionParameters = attach.audioTransitionParameters;
					editor = attach.editorAudio;

				}
				
				var hasTransition = transition != null && transitionParameters != null;
				if (hasTransition == true) {

					GUI.DrawTexture(rect, Texture2D.blackTexture, ScaleMode.ScaleAndCrop, false);

					var hovered = GUI.enabled && rect.Contains(Event.current.mousePosition);
					if (editor == null) {

						editor = Editor.CreateEditor(transitionParameters) as IPreviewEditor;
						if (FlowSystem.GetData().modeLayer == ModeLayer.Flow) {
							
							attach.editor = editor;
							
						} else if (FlowSystem.GetData().modeLayer == ModeLayer.Audio) {
							
							attach.editorAudio = editor;
							
						}

						hovered = true;

					}

					if (editor.HasPreviewGUI() == true) {

						if (hovered == false) {

							editor.OnDisable();

						} else {

							editor.OnEnable();
							
						}

						var style = new GUIStyle(EditorStyles.toolbarButton);
						editor.OnPreviewGUI(Color.white, rect, style, false, false, hovered);

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

				//}

			}

		}

		public void DrawWindowLayout(FD.FlowWindow window) {
			
			var flowWindowWithLayout = FlowSystem.GetData().HasView(FlowView.Layout);
			if (flowWindowWithLayout == true) {

				GUILayout.Box(string.Empty, FlowSystemEditorWindow.styles.layoutBoxStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				var rect = GUILayoutUtility.GetLastRect();
				
				if (window.OnPreviewGUI(rect,
				                        FlowSystemEditorWindow.defaultSkin.button,
				                        FlowSystemEditorWindow.styles.layoutBoxStyle,
				                        drawInfo: true,
				                        selectable: true,
				                        onCreateScreen: () => {
					
					this.SelectWindow(window);
					FlowChooserFilter.CreateScreen(Selection.activeObject, window.compiledNamespace, "/Screens", () => {
						
						this.SelectWindow(window);
						
					});
					
				}, onCreateLayout: () => {
					
					this.SelectWindow(window);
					Selection.activeObject = window.GetScreen().Load<WindowBase>();
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

		private void CreateOnScene(FD.FlowWindow window) {

			if (window.compiled == false) {
				
				this.ShowNotification(new GUIContent("You need to compile this window to use `Create on Scene` command"));
				
			} else {

				var screen = window.GetScreen().Load<WindowBase>();
				if (screen != null) {

					screen.CreateOnScene(callEvents: false);

				}

			}

		}

		private void SelectWindow(FD.FlowWindow window) {

			if (window.compiled == false) {
				
				this.ShowNotification(new GUIContent("You need to compile this window to use `Select` command"));
					
			} else {

				window.Select();

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
		
		private Dictionary<int, List<FD.FlowWindow>> bringFront = new Dictionary<int, List<FD.FlowWindow>>();
		
		private void BringBackOrFront(FD.FlowWindow current, IEnumerable<FD.FlowWindow> windows) {
			
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
							
							this.bringFront.Add(window.id, new List<FD.FlowWindow>() { current });
							
						}
						
					}
					
				}
				
			}
			
		}

		private static bool enabled = true;
		public void SetDisabled() {

			FlowSystemEditorWindow.enabled = false;

		}

		public void SetEnabled() {

			FlowSystemEditorWindow.enabled = true;

		}

		public bool IsEnabled() {

			return FlowSystemEditorWindow.enabled == true && EditorApplication.isCompiling == false;

		}

		private Vector2 settingsWindowScroll;
		private ReorderableList defaultWindows;
		private ReorderableList tagsList;
		private void DrawSettings(float offsetY) {

			//var scrollPos = FlowSystem.GetScrollPosition();
			
			//var wRect = new Rect(10f + scrollPos.x, 20f + scrollPos.y, 200f, 200f);
			//GUI.Window(-1, wRect, (id) => {
			
			if (FlowSystem.HasData() == false) return;
			
			EditorGUI.BeginDisabledGroup(this.scrollingMouse);
			
			var boxStyle = ME.Utilities.CacheStyle("FlowEditor.Settings.Styles", "miniButton", (styleName) => {

				var _boxStyle = new GUIStyle(FlowSystemEditorWindow.defaultSkin.button);
				_boxStyle.margin = new RectOffset(10, 10, 10, 10);

				return _boxStyle;

			});

			GUILayout.BeginArea(new Rect(0f, offsetY, FlowSystemEditorWindow.GetSettingsWidth(), this.position.height - offsetY).PixelPerfect(), boxStyle);
			{

				GUILayout.BeginHorizontal();
				{
					var data = FlowSystem.GetData();

					var oldState = GUI.enabled;

					var layers = System.Enum.GetNames(typeof(ModeLayer));
					for (int i = 0; i < layers.Length; ++i) {
						
						var layerName = layers[i];
						var style = EditorStyles.miniButtonMid;
						if (i == 0) style = EditorStyles.miniButtonLeft;
						if (i == layers.Length - 1) style = EditorStyles.miniButtonRight;
						var selected = ((int)data.modeLayer == i);

						style = new GUIStyle(style);
						style.fontSize = 12;
						style.fontStyle = FontStyle.Bold;

						GUI.enabled = !selected && oldState;
						if (GUILayout.Button(layerName, style, GUILayout.Height(30f)) == true) {
							
							FlowSystem.GetData().modeLayer = (ModeLayer)i;
							
						}
						
					}

					GUI.enabled = oldState;

				}
				GUILayout.EndHorizontal();

				//var buttonStyle = new GUIStyle(EditorStyles.miniButton);
				//this.DrawToolbar(buttonStyle);

				var oldSkin = GUI.skin;
				GUI.skin = FlowSystemEditorWindow.defaultSkin;
				this.settingsWindowScroll = GUILayout.BeginScrollView(this.settingsWindowScroll.PixelPerfect(), false, false);
				GUI.skin = oldSkin;

				CustomGUI.Splitter();
				GUILayout.Label("Base Modules:", EditorStyles.largeLabel);
				CustomGUI.Splitter();
				
				#region AUTH
				Flow.DrawModuleSettingsGUI(null, "UIW Services Authorization", null, () => {
					
					var data = FlowSystem.GetData();
					var isFull = true;
					var isInvalid = false;

					UnityEditor.EditorGUILayout.LabelField("Key:");
					var newKey = UnityEditor.EditorGUILayout.TextArea(data.authKey, FlowSystemEditorWindow.defaultSkin.textArea, GUILayout.ExpandWidth(true));

					GUILayout.BeginHorizontal();
					{

						if (data.IsValidAuthKey() == true) {

							foreach (var permission in System.Enum.GetValues(typeof(AuthKeyPermissions))) {

								var perm = (AuthKeyPermissions)permission;
								if (perm == AuthKeyPermissions.None) continue;

								if (data.IsValidAuthKey(perm) == true) {

									GUILayout.Label(perm.ToString(), (GUIStyle)"sv_label_3", GUILayout.ExpandWidth(false));

								} else {

									isFull = false;

								}

							}

						} else {

							GUILayout.Label("Invalid Key", (GUIStyle)"sv_label_6", GUILayout.ExpandWidth(false));
							isFull = false;
							isInvalid = true;

						}
						
					}
					GUILayout.EndHorizontal();

					if (newKey != data.authKey) {
						
						data.authKey = newKey;
						data.ValidateAuthKey((result) => {
							
							if (result == true) {
								
								// key ok
								
							} else {
								
								// key failed
								
							}
							
							FlowSystem.SetDirty();
							
						});
						FlowSystem.SetDirty();
						
					}

					if (isInvalid == true) {
						
						FlowSystem.DrawEditorGetKeyButton(FlowSystemEditorWindow.defaultSkin);

					} else if (isFull == false) {

						FlowSystem.DrawEditorGetKeyButton(FlowSystemEditorWindow.defaultSkin, "Expand Key");
						
					}

				});
				#endregion

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

					GUILayout.Label("Filters:", EditorStyles.boldLabel);

					var filter = FlowSystem.GetData().flowView;
					var names = System.Enum.GetValues(typeof(FlowView));
					foreach (var flag in names) {

						var value = (FlowView)flag;
						if (value == FlowView.None) continue;

						var isSelected = (filter & value) == value;

						var newValue = GUILayout.Toggle(isSelected, System.Enum.GetName(typeof(FlowView), value));
						if (newValue != isSelected) {

							if (newValue == true) {

								filter |= value;

							} else {

								filter = (filter ^ value);

							}

						}

					}

					if (filter != FlowSystem.GetData().flowView) {
						
						FlowSystem.GetData().flowView = filter;
						FlowSystem.SetDirty();
						
						this.Repaint();
						
					}
					
					GUILayout.Label("Settings:", EditorStyles.boldLabel);
					var anySettings = false;

					{
						
						anySettings = true;
						var flowDrawTags = EditorGUILayout.Toggle("Draw Tags", FlowSystem.GetData().flowDrawTags);
						if (flowDrawTags != FlowSystem.GetData().flowDrawTags) {

							FlowSystem.GetData().flowDrawTags = flowDrawTags;
							FlowSystem.SetDirty();

							this.Repaint();

						}

					}

					if (FlowSystem.GetData().HasView(FlowView.Layout) == true) {
						
						EditorGUIUtility.labelWidth = 50f;
						
						var flowWindowWithLayoutScaleFactor = EditorGUILayout.Slider("Scale", FlowSystem.GetData().flowWindowWithLayoutScaleFactor, 0f, 1f);
						if (flowWindowWithLayoutScaleFactor != FlowSystem.GetData().flowWindowWithLayoutScaleFactor) {
							
							FlowSystem.GetData().flowWindowWithLayoutScaleFactor = flowWindowWithLayoutScaleFactor;
							FlowSystem.SetDirty();
							
							this.Repaint();
							
						}
						
						EditorGUIUtilityExt.LookLikeControls();

						anySettings = true;

					}

					if (anySettings == false) {

						GUILayout.Label("There are no settings", EditorStyles.centeredGreyMiniLabel);

					}

				});
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

				GUILayout.Label(string.Format(VersionInfo.DESCRIPTION, VersionInfo.BUNDLE_VERSION), copyright);
				
				GUILayout.EndScrollView();
				
			}
			GUILayout.EndArea();

			EditorGUI.EndDisabledGroup();

			//}, "Settings");
			
			//GUI.BringWindowToFront(-1);
			
		}

		private bool drawWindowContent = true;
		private bool scrollingMouse = false;
		private bool selectionRectWait;
		private Rect selectionRect;
		private bool settingsDragStarted;
		private Vector2 settingsDragLastPos;
		private void HandleEvents(float offset) {
			
			var button = Event.current.button;
			var position =
				this.zoomDrawer.ConvertScreenCoordsToZoomCoords(Event.current.mousePosition, topLeft: true) + 
				-FlowSystem.GetScrollPosition() * 2f;
			
			if (Event.current.type == EventType.MouseDown && button == 1) {
				
				this.DrawContextMenu();
				
			}

			#region Settings
			var settingsHandleWidth = 10f;
			var settingsHandleRect = new Rect(FlowSystemEditorWindow.settingsWidth - settingsHandleWidth * 0.5f, TOOLBAR_HEIGHT, settingsHandleWidth, Screen.height - TOOLBAR_HEIGHT);
			if (GUI.enabled == true) EditorGUIUtility.AddCursorRect(settingsHandleRect, MouseCursor.ResizeHorizontal);
			if (Event.current.type == EventType.MouseDown && settingsHandleRect.Contains(Event.current.mousePosition) == true) {

				// Start settings drag
				this.settingsDragStarted = true;
				this.settingsDragLastPos = Event.current.mousePosition;

			}

			if (this.settingsDragStarted == true) {

				if (Event.current.type == EventType.MouseDrag) {

					var delta = Event.current.mousePosition - this.settingsDragLastPos;
					this.settingsDragLastPos = Event.current.mousePosition;
					FlowSystemEditorWindow.settingsWidth += delta.x;
					if (FlowSystemEditorWindow.settingsWidth < 150f) {
						
						FlowSystemEditorWindow.settingsWidth = 150f;
						
					}
					if (FlowSystemEditorWindow.settingsWidth > 300f) {
						
						FlowSystemEditorWindow.settingsWidth = 300f;
						
					}
					EditorPrefs.SetFloat("UI.Windows.Editor.Settings.Width", FlowSystemEditorWindow.settingsWidth);

					this.Repaint();

				}

				if (Event.current.type == EventType.MouseUp) {

					this.settingsDragStarted = false;

				}

			}
			#endregion

			if (this.settingsDragStarted == false) {

				if (Event.current.type == EventType.MouseDown && button == 2) {
					
					this.drawWindowContent = true;

					this.scrollingMouse = true;
					
					this.scrollingMouseAnimation = new UnityEditor.AnimatedValues.AnimFloat(0f, () => {
						
						this.Repaint();
						
					});
					this.scrollingMouseAnimation.speed = 2f;
					this.scrollingMouseAnimation.target = 1f;
					
				}
				
				if (Event.current.type == EventType.MouseDrag && this.scrollingMouse == true) {
					
					this.drawWindowContent = false;

					this.Repaint();
					
					this.showTagsPopup = false;
					this.showTagsPopupId = -1;

				}
				
				if (Event.current.type == EventType.MouseUp && this.scrollingMouse == true) {
					
					this.scrollingMouse = false;
					
					this.scrollingMouseAnimation.value = 1f;
					this.scrollingMouseAnimation.speed = 2f;
					this.scrollingMouseAnimation.target = 0f;
					
					this.drawWindowContent = true;

					this.Repaint();
					
				}
				
				if (Event.current.type == EventType.MouseDown && Event.current.alt == false && button == 0) {
					
					this.selectionRect = new Rect(position.x, position.y, 0f, 0f);
					this.selectionRectWait = true;
					
					this.selectionRectAnimation = new UnityEditor.AnimatedValues.AnimFloat(0f, () => {
						
						this.Repaint();
						
					});
					this.selectionRectAnimation.speed = 2f;
					this.selectionRectAnimation.target = 1f;
					
					this.drawWindowContent = true;

				}
				
				if (Event.current.type == EventType.MouseDrag && this.selectionRectWait == true) {
					
					var p1x = this.selectionRect.x;
					var p1y = this.selectionRect.y;
					var p2x = position.x;
					var p2y = position.y;
					
					this.selectionRect.width = p2x - p1x;
					this.selectionRect.height = p2y - p1y;
					
					FlowSystem.SelectWindowsInRect(this.selectionRect);
					
					this.drawWindowContent = false;

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
					
					this.drawWindowContent = true;

					this.Repaint();
					
				}

			}

		}
		
		private AnimatedValues.AnimFloat selectionRectAnimation;
		public void DrawBackground() {
			
			if (this._background == null) this._background = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Flow/Background");
			
			FlowSystem.grid = new Vector2(this._background.width / 20f, this._background.height / 20f).PixelPerfect();
			
			var oldColor = GUI.color;
			
			var color = new Color(1f, 1f, 1f, 0.2f);
			GUI.color = color;
			
			var size = new Vector2(this._background.width, this._background.height);
			var drawSize = new Vector2(this.contentRect.width, this.contentRect.height);
			
			GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, drawSize.x, drawSize.y).PixelPerfect(), this._background, new Rect(0f, 0f, drawSize.x / size.x, drawSize.y / size.y).PixelPerfect(), true);
			
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
			var offset = new Vector2(Screen.width * 0.5f - FlowSystemEditorWindow.GetSettingsWidth() * 0.5f - nullOffset.width * 0.5f, 0f);
			
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
		
		public void OpenFlowData(FlowData flowData) {
			
			if (this.guiSplash == null) {

				this.ChangeFlowData();
				return;

			}

			FlowSystem.SetData(flowData);
			
		}

		public void ChangeFlowData() {

			this.guiSplash = null;
			FlowSystem.SetData(null);
			this.defaultWindows = null;
			this.tagsList = null;
			Flow.OnReset(this);

		}
		
		public void CreateNewItem(System.Func<FD.FlowWindow> predicate) {
			
			var scrollPos = FlowSystem.GetScrollPosition();
			
			var window = predicate();
			window.rect.x = -(scrollPos.x - this.scrollRect.width * 0.5f + window.rect.width * 0.5f);
			window.rect.y = -(scrollPos.y - this.scrollRect.height * 0.5f + window.rect.height * 0.5f);

			FlowSystem.SetCompileDirty();

		}
		
		private void DrawToolbar() {

			var buttonStyle = ME.Utilities.CacheStyle("FlowEditor.DrawWindowToolbar.Styles", "toolbarButton", (name) => {
				
				var _buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
				_buttonStyle.stretchWidth = false;
				
				return _buttonStyle;
				
			});
			
			var buttonDropdownStyle = ME.Utilities.CacheStyle("FlowEditor.DrawWindowToolbar.Styles", "toolbarDropDown", (name) => {
				
				var _buttonStyle = new GUIStyle(EditorStyles.toolbarDropDown);
				_buttonStyle.stretchWidth = false;
				
				return _buttonStyle;
				
			});

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			
			this.DrawToolbar(buttonStyle, buttonDropdownStyle);
			
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
				
				FlowSystem.SetScrollPosition(Vector2.one);
				
			});
			
			Flow.OnDrawToolsMenuGUI(this, prefix, menu);
			
		}

		public void SetCenterTo(FD.FlowWindow window) {

			FlowSystem.SetZoom(1f);
			FlowSystem.SetScrollPosition(-new Vector2(window.rect.x - this.scrollRect.width * 0.5f, window.rect.y - this.scrollRect.height * 0.5f));

		}
		
		private Rect layoutStateToolbarCreateButtonRect;
		private Rect layoutStateToolbarToolsButtonRect;
		private void DrawToolbar(GUIStyle buttonStyle, GUIStyle buttonDropdownStyle) {
			
			var result = GUILayout.Button("Create", buttonDropdownStyle);
			if (Event.current.type == EventType.Repaint) {
				
				this.layoutStateToolbarCreateButtonRect = GUILayoutUtility.GetLastRect();
				
			}

			if (result == true) {
				
				var menu = new GenericMenu();
				this.SetupCreateMenu(string.Empty, menu);
				
				menu.DropDown(this.layoutStateToolbarCreateButtonRect);
				
			}
			
			result = GUILayout.Button("Tools", buttonDropdownStyle);
			if (Event.current.type == EventType.Repaint) {
				
				this.layoutStateToolbarToolsButtonRect = GUILayoutUtility.GetLastRect();
				
			}

			if (result == true) {
				
				var menu = new GenericMenu();
				this.SetupToolsMenu(string.Empty, menu);
				
				menu.DropDown(this.layoutStateToolbarToolsButtonRect);
				
			}

			Flow.OnDrawToolbarGUI(this, buttonStyle);
			
			GUILayout.FlexibleSpace();
			
			var oldColor = GUI.color;
			var c = Color.cyan;
			c.a = 0.3f;
			GUI.color = c;
			GUILayout.Label(string.Format("Version: {1}. Current Data: {0}", AssetDatabase.GetAssetPath(FlowSystem.GetData()), VersionInfo.BUNDLE_VERSION), buttonStyle);
			GUI.color = oldColor;
			
			if (GUILayout.Button("Change", buttonStyle) == true) {
				
				this.ChangeFlowData();
				
			}
			
		}
		
		private void DrawNodeContainer(int id) {
			
			EditorGUIUtility.labelWidth = 65f;
			
			var oldState = GUI.enabled;
			GUI.enabled = this.IsEnabled();

			var buttonStyle = ME.Utilities.CacheStyle("FlowEditor.DrawWindowToolbar.Styles", "toolbarButton", (name) => {
				
				var _buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
				_buttonStyle.stretchWidth = false;
				
				return _buttonStyle;
				
			});

			var buttonWarningStyle = ME.Utilities.CacheStyle("FlowEditor.DrawWindowToolbar.Styles", "buttonWarningStyle", (name) => {
				
				var _buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
				_buttonStyle.stretchWidth = false;
				_buttonStyle.fontStyle = FontStyle.Bold;
				
				return _buttonStyle;
				
			});

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
								
								if ((attachTo.IsContainer() == true && currentAttach.IsContainer() == true && attachTo == container) || (hasContainer == true && container.id != id) || this.currentAttachIndex > 0) {
									
									
									
								} else {
									
									if (attachTo.IsContainer() == true && currentAttach.IsContainer() == true) {
										
										if (FlowSystem.AlreadyAttached(id, this.currentAttachId, this.currentAttachIndex) == true) {
											
											if (GUILayout.Button("Detach Here", buttonStyle) == true) {
												
												FlowSystem.Detach(this.currentAttachId, this.currentAttachIndex, id, oneWay: false);
												if (this.onAttach != null) this.onAttach(id, this.currentAttachIndex, false);
												if (!Event.current.shift) this.WaitForAttach(-1);
												
											}
											
										} else {
											
											if (GUILayout.Button("Attach Here", buttonStyle) == true) {
												
												FlowSystem.Attach(id, this.currentAttachIndex, this.currentAttachId, oneWay: true);
												if (this.onAttach != null) this.onAttach(id, this.currentAttachIndex, true);
												if (!Event.current.shift) this.WaitForAttach(-1);
												
											}
											
										}
										
									} else {
										
										if (FlowSystem.AlreadyAttached(this.currentAttachId, this.currentAttachIndex, id) == true) {
											
											if (GUILayout.Button("Detach Here", buttonStyle) == true) {
												
												FlowSystem.Detach(this.currentAttachId, this.currentAttachIndex, id, oneWay: false);
												if (this.onAttach != null) this.onAttach(id, this.currentAttachIndex, false);
												if (!Event.current.shift) this.WaitForAttach(-1);
												
											}
											
										} else {
											
											if (GUILayout.Button("Attach Here", buttonStyle) == true) {
												
												FlowSystem.Attach(this.currentAttachId, this.currentAttachIndex, id, oneWay: false);
												if (this.onAttach != null) this.onAttach(id, this.currentAttachIndex, true);
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

					if (GUILayout.Button("Destroy", buttonWarningStyle) == true) {
						
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
				var newTitle = GUILayout.TextField(window.title);
				if (newTitle != window.title) {

					window.title = newTitle;

					FlowSystem.SetCompileDirty();
					FlowSystem.SetDirty();

				}
				
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				
				GUILayoutExt.LabelWithShadow("Directory:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
				var newDirectory = GUILayout.TextField(window.directory);
				if (newDirectory != window.directory) {
					
					window.directory = newDirectory;
					
					FlowSystem.SetCompileDirty();
					FlowSystem.SetDirty();
					
				}

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
			
			GUI.enabled = oldState;
			
			EditorGUIUtilityExt.LookLikeControls();
			
		}
		
		private void DragWindow(bool headerOnly) {

			var oldState = GUI.enabled;
			GUI.enabled = this.IsEnabled();
			if (GUI.enabled == false) return;
			
			if (Event.current.button != 2) {
				
				if (headerOnly == false) {
					
					GUI.DragWindow();
					
				} else {
					
					var dragRect = new Rect(0f, 0f, 5000f, 20f);
					GUI.DragWindow(dragRect);
					
				}
				
			}
			
			GUI.enabled = oldState;
			
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
					
					GUI.BeginGroup(drawRect.PixelPerfect());
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

		public float GetTagsHeight(FD.FlowWindow window) {
			
			var columns = 3;
			var height = 16f;

			return Mathf.CeilToInt(window.tags.Count / (float)columns) * height + height + 2f;

		}

		private bool showTagsPopup = false;
		private Rect showTagsPopupRect;
		private int showTagsPopupId;
		private string tagCaption = string.Empty;
		private void DrawTags(FD.FlowWindow window, bool defaultWindow = false) {

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
										
										this.showTagsPopupRect = new Rect(window.rect.x + rect.x + FlowSystemEditorWindow.GetSettingsWidth(), window.rect.y + rect.y + (defaultWindow == true ? window.rect.height : 0f), rect.width, rect.height);
										
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
			
			EditorGUIUtilityExt.LookLikeControls();
			
		}
		
		private void DrawStates(FD.CompletedState[] states, FD.FlowWindow window) {
			
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
				
				if (state == FD.CompletedState.NotReady) {
					
					color = new Color(1f, 0.3f, 0.3f, 1f);
					
				} else if (state == FD.CompletedState.Ready) {
					
					color = new Color(0.3f, 1f, 0.3f, 1f);
					
				} else if (state == FD.CompletedState.ReadyButWarnings) {
					
					color = new Color(1f, 1f, 0.3f, 1f);
					
				}
				
				GUI.color = color;
				GUI.Label(new Rect(posX, posY, elemWidth, style.fixedHeight), string.Empty, style);
				posX -= elemWidth;
				
			}
			
			GUI.color = oldColor;
			
		}

		private Rect layoutStateSelectButtonRect;
		private void DrawWindowToolbar(FD.FlowWindow window) {

			/*if (FlowSystem.GetData().modeLayer != ModeLayer.Flow) {

				return;

			}*/

			//var edit = false;
			var id = window.id;
			
			var buttonStyle = ME.Utilities.CacheStyle("FlowEditor.DrawWindowToolbar.Styles", "toolbarButton", (name) => {
				
				var _buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
				_buttonStyle.stretchWidth = false;
				
				return _buttonStyle;
				
			});
			
			var buttonDropdownStyle = ME.Utilities.CacheStyle("FlowEditor.DrawWindowToolbar.Styles", "toolbarDropDown", (name) => {
				
				var _buttonStyle = new GUIStyle(EditorStyles.toolbarDropDown);
				_buttonStyle.stretchWidth = false;
				
				return _buttonStyle;
				
			});

			var buttonWarningStyle = ME.Utilities.CacheStyle("FlowEditor.DrawWindowToolbar.Styles", "buttonWarningStyle", (name) => {
				
				var _buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
				_buttonStyle.stretchWidth = false;
				_buttonStyle.fontStyle = FontStyle.Bold;
				
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
								
								if (FlowSystem.AlreadyAttached(this.currentAttachId, this.currentAttachIndex, id) == true) {
									
									if (GUILayout.Button(string.Format("Detach Here{0}", (Event.current.alt == true ? " (Double Direction)" : string.Empty)), buttonStyle) == true) {
										
										FlowSystem.Detach(this.currentAttachId, this.currentAttachIndex, id, oneWay: Event.current.alt == false);
										if (this.onAttach != null) this.onAttach(id, this.currentAttachIndex, false);
										if (Event.current.shift == false) this.WaitForAttach(-1);
										
									}
									
								} else {

									var abTests = (window.abTests != null && window.abTests.sourceWindowId >= 0/* || currentAttach.attachItems.Any(x => FlowSystem.GetWindow(x.targetId).IsABTest() == true) == true*/);

									if (this.currentAttachIndex == 0 && 
									    (currentAttach.IsABTest() == true ||
									    abTests == true)) {
										/*
										if (abTests == true) {

											if (GUILayout.Button("Attach Here", buttonStyle) == true) {

												this.ShowNotification(new GUIContent("You can't connect using this method. Use `Attach` function on `A/B Test Condition`"));

											}

										}*/

									} else {

										if (GUILayout.Button(string.Format("Attach Here{0}", (Event.current.alt == true ? " (Double Direction)" : string.Empty)), buttonStyle) == true) {
											
											FlowSystem.Attach(this.currentAttachId, this.currentAttachIndex, id, oneWay: Event.current.alt == false);
											if (this.onAttach != null) this.onAttach(id, this.currentAttachIndex, true);
											if (Event.current.shift == false) this.WaitForAttach(-1);
											
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
					
				} else {
					
					if (window.IsSmall() == false ||
					    window.IsFunction() == true ||
					    window.IsABTest() == true) {
						
						if (GUILayout.Button("Attach/Detach", buttonStyle) == true) {
							
							this.ShowNotification(new GUIContent("Use Attach/Detach buttons to Connect/Disconnect a window"));
							this.WaitForAttach(id);
							
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

						FlowSystem.SetCompileDirty();
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
						
						FlowSystem.SetCompileDirty();
						FlowSystem.SetDirty();
						
					}
					
				}
				
				GUILayout.FlexibleSpace();
				
				if (window.IsSmall() == false && FlowSceneView.IsActive() == false && window.storeType == FD.FlowWindow.StoreType.NewScreen) {

					var state = GUILayout.Button("Screen", buttonDropdownStyle);
					if (Event.current.type == EventType.Repaint) {

						this.layoutStateSelectButtonRect = GUILayoutUtility.GetLastRect();

					}

					if (state == true) {

						var screen = window.GetScreen().Load<WindowBase>();

						var menu = new GenericMenu();
						menu.AddItem(new GUIContent("Select Package"), on: false, func: () => { this.SelectWindow(window); });
						
						if (window.compiled == true) {

							menu.AddItem(new GUIContent("Edit..."), on: false, func: () => {

								var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(window.GetScreen().Load<WindowBase>()));
								var filename = window.compiledDerivedClassName + ".cs";
								EditorUtility.OpenWithDefaultApp(string.Format("{0}/../{1}", path, filename));

							});

							var k = 0;
							var screens = window.GetScreens();
							for (int i = 0; i < screens.Length; ++i) {

								if (screens[i] == null) continue;

								var index = i;
								menu.AddItem(new GUIContent(string.Format("Targets/Target {0} ({1})", ++k, screens[i].name)), on: (window.selectedScreenIndex == index), func: () => {

									window.selectedScreenIndex = index;
									EditorUtility.SetDirty(window);

								});

							}

							//var screen = screens[window.selectedScreenIndex];
							//var wt = screen.Load<LayoutWindowType>();
							var wt = screen as LayoutWindowType;
							if (wt != null) {

								if (wt.layouts == null || wt.layouts.layouts == null || wt.layouts.layouts.Length == 0) {

									wt.OnValidateEditor();

								}

								for (int i = 0; i < wt.layouts.layouts.Length; ++i) {

									var index = i;
									menu.AddItem(new GUIContent(string.Format("Views/{0}", wt.layouts.types[i])), on: (window.selectedViewIndex == index), func: () => {

										window.selectedViewIndex = index;
										wt.ApplyOrientationIndexDirect(index);
										EditorUtility.SetDirty(window);

									});

								}

							}

						}

						menu.AddItem(new GUIContent("Create on Scene"), on: false, func: () => { this.CreateOnScene(window); });

						var methodsCount = 0;
						FlowEditorUtilities.CollectCallVariations(screen, (types, names) => {

							++methodsCount;

						});

						var displaysCount = 8;
						for (int i = 0; i < displaysCount; ++i) {

							var displayNumber = i + 1;

							if (/*i >= Display.displays.Length ||*/ screen == null) {

								menu.AddDisabledItem(new GUIContent(string.Format("Display/Display {0}", displayNumber)));

							} else {

								menu.AddItem(new GUIContent(string.Format("Display/Display {0}", displayNumber)), screen.workCamera.targetDisplay == i, (displayIndex) => {

									screen.workCamera.targetDisplay = (int)displayIndex;
									EditorUtility.SetDirty(screen);

								}, i);

							}

						}

						menu.AddDisabledItem(new GUIContent(string.Format("Calls/Methods: {0}", methodsCount.ToString())));
						menu.AddSeparator("Calls/");

						if (window.compiled == true &&
						    screen != null) {

							methodsCount = 0;
							FlowEditorUtilities.CollectCallVariations(screen, (types, names) => {

								var parameters = new List<string>();
								for (int i = 0; i < types.Length; ++i) {

									parameters.Add(ME.Utilities.FormatParameter(types[i]) + " " + names[i]);

								}

								var paramsStr = parameters.Count > 0 ? string.Format("({0})", string.Join(", ", parameters.ToArray())) : string.Empty;
								menu.AddItem(new GUIContent(string.Format("Calls/OnParametersPass{0}", paramsStr)), on: false, func: () => {

									Selection.activeObject = screen;

								});

								++methodsCount;

							});

							if (methodsCount == 0) {
								
								menu.AddDisabledItem(new GUIContent("Calls/No `OnParametersPass` Methods Found"));

							}

						} else {
							
							menu.AddDisabledItem(new GUIContent("Calls/You need to compile window"));

						}

						Flow.OnFlowWindowScreenMenuGUI(this, window, menu);

						menu.DropDown(this.layoutStateSelectButtonRect);

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
				
				if (GUILayout.Button("X", buttonWarningStyle) == true) {
					
					if (EditorUtility.DisplayDialog("Are you sure?", "Current window will be destroyed with all links.", "Yes, destroy", "No") == true) {
						
						this.ShowNotification(new GUIContent(string.Format("The window `{0}` was successfully destroyed", window.title)));
						FlowSystem.DestroyWindow(id);
						return;
						
					}
					
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
						
						if (FlowSystem.AlreadyAttached(this.currentAttachId, this.currentAttachIndex, id, this.currentAttachComponent) == true) {
							
							if (GUILayout.Button("Detach Here", buttonStyle) == true) {
								
								FlowSystem.Detach(this.currentAttachId, this.currentAttachIndex, id, oneWay: true, component: this.currentAttachComponent);
								if (this.onAttach != null) this.onAttach(id, this.currentAttachIndex, false);
								if (Event.current.shift == false) this.WaitForAttach(-1);

							}
							
						} else {
							
							if (GUILayout.Button("Attach Here", buttonStyle) == true) {
								
								FlowSystem.Attach(this.currentAttachId, this.currentAttachIndex, id, oneWay: true, component: this.currentAttachComponent);
								if (this.onAttach != null) this.onAttach(id, this.currentAttachIndex, true);
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

			if (this.drawWindowContent == false) return;

			var oldState = GUI.enabled;
			GUI.enabled = this.IsEnabled();

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

				var textField = ME.Utilities.CacheStyle("UI.Windows.Styles.TextFieldSmall", "TextFieldSmall", (name) => FlowSystemEditorWindow.defaultSkin.FindStyle(name));

				if (window.IsSmall() == true) {
					
					this.DrawWindowToolbar(window);
					Flow.OnDrawWindowGUI(this, window);
					
					this.DragWindow(headerOnly: false);
					
				} else {

					if (window.storeType == FD.FlowWindow.StoreType.NewScreen) {
						
						this.DrawStates(window.states, window);
						
					} else {
						
						var win = FlowSystem.GetWindow(window.screenWindowId);
						if (win != null) this.DrawStates(win.states, win);
						
					}

					this.DrawWindowToolbar(window);

					if (FlowSystem.GetData().modeLayer == ModeLayer.Flow) {

						GUILayout.BeginHorizontal();
						{
							
							GUILayoutExt.LabelWithShadow("Title:", FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
							var newTitle = GUILayout.TextField(window.title, textField);
							if (newTitle != window.title) {
								
								window.title = newTitle;
								
								FlowSystem.SetCompileDirty();
								FlowSystem.SetDirty();
								
							}
							
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						{
							
							var typeId = (int)window.storeType;
							typeId = GUILayoutExt.Popup(typeId, new string[1] { "Directory:"/*, "Re-use Screen:"*/ }, FlowSystemEditorWindow.defaultSkin.label, GUILayout.Width(EditorGUIUtility.labelWidth));
							if (typeId == 0) {
								
								// Directory choosed
								
								window.flags &= (~FD.FlowWindow.Flags.CantCompiled);
								window.storeType = FD.FlowWindow.StoreType.NewScreen;
								
								if (string.IsNullOrEmpty(window.directory) == true) window.directory = string.Empty;
								var newDirectory = GUILayout.TextField(window.directory, textField);
								if (newDirectory != window.directory) {
									
									window.directory = newDirectory;
									
									FlowSystem.SetCompileDirty();
									FlowSystem.SetDirty();
									
								}

							} else if (typeId == 1) {
								
								// Re-use screen choosed
								
								window.flags |= FD.FlowWindow.Flags.CantCompiled;
								window.storeType = FD.FlowWindow.StoreType.ReUseScreen;
								
								var linkIndex = -1;
								var index = 0;
								var values = new List<string>();
								var list = new List<FD.FlowWindow>();
								var windows = FlowSystem.GetWindows();
								foreach (var win in windows) {
									
									if (win.storeType == FD.FlowWindow.StoreType.NewScreen &&
										win.IsSmall() == false &&
										win.IsContainer() == false) {
										
										values.Add(win.title.Replace("/", " "));
										list.Add(win);
										
										if (window.screenWindowId == win.id) linkIndex = index;
										
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

					}

					Flow.OnDrawWindowGUI(this, window);

				}

			}

			this.DragWindow(headerOnly: false);
			
			if (GUI.changed == true) {

				FlowSystem.SetDirty();

			}
			
			GUI.enabled = oldState;

			EditorGUIUtilityExt.LookLikeControls();
			
		}
		
		//private float itemProgress;
		public void OnItemProgress(float value) {
			
			//this.itemProgress = value;
			this.Repaint();
			
		}
		
		private int currentAttachId = -1;
		private int currentAttachIndex = 0;
		private bool waitForAttach = false;
		private System.Action<int, int, bool> onAttach = null;
		private WindowLayoutElement currentAttachComponent;
		public void WaitForAttach(int id, WindowLayoutElement currentAttachComponent = null, int index = 0, System.Action<int, int, bool> onAttach = null) {

			this.onAttach = onAttach;
			this.currentAttachId = id;
			this.currentAttachIndex = index;
			this.currentAttachComponent = currentAttachComponent;
			this.waitForAttach = id >= 0;
			
			if (this.waitForAttach == false) {
				
				WindowLayoutElement.waitForComponentConnectionElementTemp = null;
				
			}
			
		}

	}

}