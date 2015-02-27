using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowSystemEditor {

		public static Rect GetCenterRect(EditorWindow editorWindow, float width, float height) {

			var size = editorWindow.position;

			return new Rect(size.width * 0.5f - width * 0.5f, size.height * 0.5f - height * 0.5f, width, height);

		}

		public static Rect Scale(Rect realRect, Rect minMaxRect, Rect toPosition, Vector2 offset) {

			var width = minMaxRect.width;
			var height = minMaxRect.height;

			return new Rect(realRect.x / width * toPosition.width + offset.x, realRect.y / height * toPosition.height + offset.y, realRect.width / width * toPosition.width, realRect.height / height * toPosition.height);

		}

	}

	public class FlowSystemEditorWindow : EditorWindow {

		[MenuItem("Window/UI.Windows Flow")]
		static void ShowEditor() {

			var editor = EditorWindow.GetWindow<FlowSystemEditorWindow>();
			editor.title = "Windows Flow";

		}

		private Rect scrollRect;
		private Rect contentRect;
		
		private const float scrollSize = 18f;

		private Texture2D _background;
		private List<int> tempAttaches = new List<int>();
		private void OnGUI() {

			var windowStyle = new GUIStyle(GUI.skin.FindStyle("window"));
			windowStyle.fontStyle = FontStyle.Bold;
			windowStyle.margin = new RectOffset(0, 0, 0, 0);
			windowStyle.padding = new RectOffset(1, 2, 16, 4);
			windowStyle.alignment = TextAnchor.UpperLeft;
			windowStyle.contentOffset = new Vector2(5f, -15f);

			var containerStyle = new GUIStyle(GUI.skin.FindStyle("box"));
			containerStyle.padding = new RectOffset(1, 1, 16, 1);
			containerStyle.contentOffset = new Vector2(0f, -15f);
			containerStyle.fontStyle = FontStyle.Bold;
			containerStyle.normal.textColor = Color.white;

			if (this._background == null) this._background = Resources.Load("UI.Windows/Editor/Background") as Texture2D;

			FlowSystem.grid = new Vector2(this._background.width / 20f, this._background.height / 20f);

			var hasData = FlowSystem.HasData();

			GUI.enabled = hasData;
			var toolbarHeight = this.DrawToolbar();
			GUI.enabled = true;
			
			IEnumerable<FlowWindow> windows = null;
			IEnumerable<FlowWindow> containers = null;
			if (hasData == true) {
				
				windows = FlowSystem.GetWindows();
				containers = FlowSystem.GetContainers();

			}

			this.scrollRect = this.position;
			this.scrollRect.x = 0f;
			this.scrollRect.y = toolbarHeight;
			this.scrollRect.height -= toolbarHeight;

			this.contentRect = this.position;
			this.contentRect.x = 0f;
			this.contentRect.y = 0f;
			this.contentRect.width = 10000f;
			this.contentRect.height = 10000f;
			this.contentRect.height -= scrollSize;

			var scrollPos = FlowSystem.GetScrollPosition();
			if (scrollPos == -Vector2.one) scrollPos = new Vector2(this.contentRect.width * 0.5f - this.position.width * 0.5f, this.contentRect.height * 0.5f - this.position.height * 0.5f);
			FlowSystem.SetScrollPosition(GUI.BeginScrollView(this.scrollRect, scrollPos, this.contentRect));

			this.DrawBackground();

			this.BeginWindows();

			if (hasData == false) {

				this.DrawDataSelection();

			} else {

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

				var containerPadding = new Vector4(50f, 100f, 50f, 50f);
				foreach (var container in containers) {
					
					GUI.backgroundColor = container.randomColor;
					
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

					var rect = GUI.Window(container.id, container.rect, this.DrawNodeContainer, container.title, containerStyle);
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

				var defaultColor = GUI.color;
				var selectedColor = new Color(0.8f, 0.8f, 1f, 1f);

				foreach (var window in windows) {

					window.rect.width = 200f;
					window.rect.height = 30f;
					GUI.color = selected.Contains(window.id) ? selectedColor : defaultColor;
					var rect = GUILayout.Window(window.id, window.rect, this.DrawNodeWindow, window.title, windowStyle, GUILayout.ExpandHeight(true));
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

				if (selectionMain >= 0 && FlowSystem.GetWindow(selectionMain).isContainer == true) FlowSystem.ResetSelection();

				GUI.color = defaultColor;

				FlowSystem.Save();

			}

			this.EndWindows();
			
			if (this.scrollingMouseAnimation.isAnimating == true || this.scrollingMouse == true) this.DrawMinimap();

			GUI.EndScrollView();

			this.DragBackground(toolbarHeight);

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

		private bool scrollingMouse = false;
		private bool selectionRectWait;
		private Rect selectionRect;
		private void DragBackground(float offset) {

			var button = Event.current.button;
			var position = Event.current.mousePosition + FlowSystem.GetScrollPosition() + new Vector2(0f, -offset);
			
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

			var oldColor = GUI.color;

			var color = new Color(1f, 1f, 1f, 0.2f);
			GUI.color = color;

			var size = new Vector2(this._background.width, this._background.height);
			var drawSize = new Vector2(this.contentRect.width, this.contentRect.height);

			GUI.DrawTextureWithTexCoords(new Rect(0f, 0f, drawSize.x, drawSize.y), this._background, new Rect(0f, 0f, drawSize.x / size.x, drawSize.y / size.y), true);

			if (this.selectionRect.size != Vector2.zero && (this.selectionRectAnimation.isAnimating == true || this.selectionRectWait == true)) {
				
				var selectionBoxStyle = new GUIStyle(GUI.skin.FindStyle("box"));
				selectionBoxStyle.margin = new RectOffset();
				selectionBoxStyle.padding = new RectOffset();
				selectionBoxStyle.contentOffset = Vector2.zero;

				color = new Color(1f, 1f, 1f, this.selectionRectAnimation.value);

				GUI.color = color;
				GUI.Box(this.selectionRect, string.Empty, selectionBoxStyle);
				
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
			GUI.Box(FlowSystemEditor.Scale(minMax, new Rect(0f, 0f, 10000f, 10000f), this.position, FlowSystem.GetScrollPosition()), string.Empty, elementStyle);

			if (containers != null) {
				
				foreach (var container in containers) {
					
					color = container.randomColor;
					color.a = elementAlpha;
					GUI.color = color;
					GUI.Box(FlowSystemEditor.Scale(container.rect, new Rect(0f, 0f, 10000f, 10000f), this.position, FlowSystem.GetScrollPosition()), string.Empty, elementStyle);
					
				}
				
			}
			
			color = Color.white;
			color.a = elementAlpha;
			GUI.color = color;

			if (windows != null) {
				
				foreach (var window in windows) {

					var rect = window.rect;
					if (rect.height < 60f) rect.height = 60f;
					GUI.Box(FlowSystemEditor.Scale(rect, new Rect(0f, 0f, 10000f, 10000f), this.position, FlowSystem.GetScrollPosition()), string.Empty, elementStyle);
					
				}
				
			}
			
			color = Color.white;
			color.a = cameraAlpha;
			GUI.color = color;

			var scrollPos = FlowSystem.GetScrollPosition();
			GUI.Box(FlowSystemEditor.Scale(new Rect(scrollPos.x, scrollPos.y, this.position.width, this.position.height), new Rect(0f, 0f, 10000f, 10000f), this.position, FlowSystem.GetScrollPosition()), string.Empty, elementStyle);
			
			GUI.color = oldColor;

		}

		private FlowData cachedData;
		private void DrawDataSelection() {

			var rect = FlowSystemEditor.GetCenterRect(this, 300f, 100f);

			rect.height = 0f;
			GUILayout.Window(0, rect, (id) => {

				this.cachedData = EditorGUILayout.ObjectField(this.cachedData, typeof(FlowData), false) as FlowData;

				GUILayout.BeginHorizontal();

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Load", GUILayout.Width(100f), GUILayout.Height(40f)) == true) {

					FlowSystem.SetData(this.cachedData);

				}

				GUILayout.EndHorizontal();

			}, "Select Data File", GUILayout.ExpandHeight(true));

		}

		private float DrawToolbar() {

			var toolbarHeight = 0f;
			var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.stretchWidth = false;

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			
			if (GUILayout.Button("Create Window", buttonStyle)) {
				
				var scrollPos = FlowSystem.GetScrollPosition();
				
				var window = FlowSystem.CreateWindow();
				window.rect.x = scrollPos.x + this.position.width * 0.5f - window.rect.width * 0.5f;
				window.rect.y = scrollPos.y + this.position.height * 0.5f - window.rect.height * 0.5f;
				
			}
			
			if (GUILayout.Button("Create Container", buttonStyle)) {
				
				var scrollPos = FlowSystem.GetScrollPosition();
				
				var container = FlowSystem.CreateContainer();
				container.rect.x = scrollPos.x + this.position.width * 0.5f - container.rect.width * 0.5f;
				container.rect.y = scrollPos.y + this.position.height * 0.5f - container.rect.height * 0.5f;
				
			}
			
			if (GUILayout.Button("Center Screen", buttonStyle)) {
				
				FlowSystem.SetScrollPosition(Vector2.one * -1f);

			}

			GUILayout.FlexibleSpace();

			GUILayout.Label("Current Data: " + AssetDatabase.GetAssetPath(this.cachedData), buttonStyle);

			GUILayout.EndHorizontal();

			toolbarHeight = GUILayoutUtility.GetLastRect().height;

			return toolbarHeight;

		}
		
		private void DrawNodeContainer(int id) {
			
			var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.stretchWidth = false;
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

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
					
					this.WaitForAttach(id);
					
				}

				if (GUILayout.Button("Destroy", buttonStyle) == true) {
					
					if (EditorUtility.DisplayDialog("Are you sure?", "Current container will be destroyed with all links (All windows will be saved)", "Yes, destroy", "No") == true) {
						
						FlowSystem.DestroyWindow(id);
						return;

					}
					
				}

			}
			
			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();
			
			var window = FlowSystem.GetWindow(id);
			window.title = GUILayout.TextField(window.title);

			var attaches = window.attaches.Count;
			if (attaches == 0) {

				this.DragWindow(headerOnly: false);

			} else {
				
				this.DragWindow(headerOnly: true);

			}

		}

		private void DragWindow(bool headerOnly) {

			if (Event.current.button != 2) {

				if (headerOnly == false) {

					GUI.DragWindow();

				} else {

					var dragRect = new Rect(0f, 0f, 5000f, 20f);
					GUI.DragWindow(dragRect);

				}

			}

		}

		private void DrawNodeWindow(int id) {
			
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

				if (GUILayout.Button("Attach/Detach", buttonStyle) == true) {

					this.WaitForAttach(id);

				}

				if (GUILayout.Button("Destroy", buttonStyle) == true) {

					if (EditorUtility.DisplayDialog("Are you sure?", "Current window will be destroyed with all links", "Yes, destroy", "No") == true) {

						FlowSystem.DestroyWindow(id);
						return;

					}

				}

			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("View", buttonStyle) == true) {



			}

			GUILayout.EndHorizontal();

			var window = FlowSystem.GetWindow(id);

			window.title = GUILayout.TextField(window.title);

			this.DragWindow(headerOnly: true);

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

			if (doubleSide == true) {

				var size = 6f;
				
				var rot = Quaternion.AngleAxis(90f, Vector3.back);
				var ray = new Ray(Vector3.zero, (rot * (end.center - start.center)).normalized);

				var offset = ray.GetPoint(size);
				var startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, -10f);
				var endPos = new Vector3(end.center.x + offset.x, end.center.y + offset.y, -10f);
				
				this.DrawNodeCurve(startPos, endPos, color1);
				
				offset = ray.GetPoint(-size);
				startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, -10f);
				endPos = new Vector3(end.center.x + offset.x, end.center.y + offset.y, -10f);
				
				this.DrawNodeCurve(endPos, startPos, color2);

			} else {

				var offset = Vector2.zero;
				var startPos = new Vector3(start.center.x + offset.x, start.center.y + offset.y, -10f);
				var endPos = new Vector3(end.center.x + offset.x, end.center.y + offset.y, -10f);

				this.DrawNodeCurve(startPos, endPos, color1);

			}

		}

		private void DrawNodeCurve(Vector3 startPos, Vector3 endPos, Color color) {
			
			startPos.z = -50f;
			endPos.z = -50f;

			var shadowColor = new Color(0f, 0f, 0f, 0.5f);
			var lineColor = color;//new Color(1f, 1f, 1f, 1f);

			var shadowOffset = Vector3.one * 2f;
			shadowOffset.z = 0f;

			Handles.BeginGUI();

			Handles.color = shadowColor;
			Handles.DrawAAPolyLine(4f, new Vector3[] { startPos, endPos });
			Handles.ConeCap(-1, (endPos + startPos) * 0.5f + shadowOffset, Quaternion.LookRotation(endPos - startPos), 15f);
			
			Handles.color = lineColor;
			Handles.DrawAAPolyLine(4f, new Vector3[] { startPos, endPos });
			Handles.ConeCap(-1, (endPos + startPos) * 0.5f, Quaternion.LookRotation(endPos - startPos), 15f);

			Handles.EndGUI();

		}

	}

}