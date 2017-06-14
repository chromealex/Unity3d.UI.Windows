using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEditor.UI.Windows.Plugins.Flow.Editors {
	
	public class FlowHierarchyWindow : EditorWindowExt {
		
		public FlowSystemEditorWindow rootWindow;
		private Rect popupRect;
		private Vector2 popupSize;
		private bool hided;
		
		public const float MIN_WIDTH = 200f;
		public const float MAX_WIDTH = 300f;
		
		public float GetWidth() {
			
			return this.popupSize.x;
			
		}
		
		new public void Focus() {
			
			base.Focus();
			
		}
		
		private GameObject lastSelected;
		private GameObject currentSelected;

		private bool isLocked;
		private bool pinned = true;

		public void OnSelected(GameObject go) {
			
			this.Repaint();
			
		}
		
		public void ShowView() {
			
			this.isLocked = false;//!FlowSystem.GetData().editorWindowAsPopup;
			if (this.isLocked == true) {
				
				this.ShowPopup();
				
			} else {
				
				this.ShowUtility();
				
			}

			this.hided = false;
			
			this.minSize = new Vector2(this.minSize.x, 1f);
			this.autoRepaintOnSceneChange = true;
			
			this.UpdateSize(1f);

		}
		
		public void HideView() {
			
			this.hided = true;
			
		}
		
		public void UpdateSize(float progress) {
			
			var popupOffset = FlowSceneItem.POPUP_OFFSET;
			var margin = FlowSceneItem.POPUP_MARGIN;
			this.popupSize = new Vector2((this.rootWindow.position.width - popupOffset * 2f) * 0.3f, (this.rootWindow.position.height - popupOffset * 2f) * progress);
			this.popupSize.x = Mathf.Clamp(this.popupSize.x, MIN_WIDTH + margin, MAX_WIDTH);
			this.popupRect = new Rect(this.rootWindow.position.x + this.rootWindow.position.width - popupOffset - this.popupSize.x + margin, this.rootWindow.position.y + popupOffset, this.popupSize.x - margin, this.popupSize.y);
			
			this.position = this.popupRect;

		}

		new public void Update() {

			base.Update();
			
			if (this.rootWindow == null) return;

			this.currentSelected = Selection.activeGameObject;
			if (this.currentSelected != this.lastSelected) {
				
				this.OnSelected(this.currentSelected);
				this.lastSelected = this.currentSelected;
				
			}
			
			if (this.isLocked == true) {

				if (this.hided == true) {
					
					this.minSize = Vector2.zero;
					this.popupRect = new Rect(0f, 0f, 1f, 1f);

				} else {
					
					this.UpdateSize(this.progress);

				}
				
				this.position = this.popupRect;

			} else {

				if (this.pinned == true) {

					this.UpdateSize(this.progress);

				}

				var pinnedDistance = 10f;

				// snap to SceneView
				if (Vector2.Distance(new Vector2(this.rootWindow.position.x + this.rootWindow.position.width, this.rootWindow.position.y), new Vector2(this.position.x, this.position.y)) < pinnedDistance) {

					this.pinned = true;

				} else {

					this.pinned = false;

				}

			}

			if (FlowSceneView.recompileChecker == null) {
				
				this.Close();
				
			}
			
		}
		
		private Vector2 scrollPosition;
		public void OnGUI() {

			this.position = this.popupRect;

			var color = GUI.color;
			color.a = this.progress;
			GUI.color = color;

			var depthOffset = 15f;

			var screen = (FlowSceneView.GetItem() != null) ? FlowSceneView.GetItem().GetScreenInstance() : null;
			UnityEngine.UI.Windows.WindowLayout layout = (FlowSceneView.GetItem() != null) ? FlowSceneView.GetItem().GetLayoutInstance() : null;

			if (layout != null || screen != null) {
				
				this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, false, false);

				var elementStyle = new GUIStyle(EditorStyles.toolbarTextField);
				elementStyle.stretchHeight = false;
				elementStyle.fixedHeight = 0f;
				elementStyle.margin = new RectOffset(0, 0, 0, 2);
				elementStyle.padding = new RectOffset(10, 10, 6, 6);

				GUILayout.BeginHorizontal(elementStyle);
				if (screen != null) {
					
					var oldBackColor = GUI.backgroundColor;
					var backColor = new Color(0.4f, 0.4f, 0.4f, 1f);
					GUI.backgroundColor = backColor;

					GUILayout.BeginVertical();
					{
						GUILayout.Label(screen.name, EditorStyles.whiteLargeLabel);
						GUILayout.Label("Screen", EditorStyles.miniLabel);
					}
					GUILayout.EndVertical();

					var rect = GUILayoutUtility.GetLastRect();
					if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) == true) {
						
						FlowSceneView.GetItem().SetTab(2);
						
					}

					GUI.backgroundColor = oldBackColor;

				}
				GUILayout.EndHorizontal();

				if (layout != null) {

					var root = layout.root;
					this.Traverse(root.transform, (element, depth) => {
						
						var title = string.Empty;
						var descr = string.Empty;
						var headerStyle = EditorStyles.whiteLargeLabel;
						color = Color.white;

						var layoutRoot = element.GetComponent<WindowLayoutRoot>();
						if (layoutRoot != null) {

							headerStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
							headerStyle.fontStyle = FontStyle.Bold;
							title = "ROOT";
							descr = "Layout Root";

							color = new Color(0.8f, 0.8f, 0.6f, 1f);

						}

						var layoutElement = element.GetComponent<WindowLayoutElement>();
						if (layoutElement != null) {
							
							title = layoutElement.comment;
							descr = layoutElement.tag.ToString() + " (" + layoutElement.name + ")";

						}

						var maxDepth = 10f;
						var depthValue = depth / maxDepth;

						var oldColor = GUI.color;
						var oldBackColor = GUI.backgroundColor;
						var backColor = Selection.activeTransform == element ? new Color(0.7f, 1f, 0.7f, 1f) : color;
						backColor.a = 1f - depthValue;
						GUI.backgroundColor = backColor;

						GUILayout.BeginHorizontal(elementStyle);
						{
							GUILayout.BeginVertical();
							{
								GUILayout.BeginHorizontal();
								{
									GUILayout.Label(string.Empty, GUILayout.Width(depthOffset * depth));
									GUILayout.Label(title, headerStyle);
								}
								GUILayout.EndVertical();
								GUILayout.BeginHorizontal();
								{
									GUILayout.Label(string.Empty, GUILayout.Width(depthOffset * depth));
									GUILayout.Label(descr, EditorStyles.miniLabel);
								}
								GUILayout.EndVertical();
							}
							GUILayout.EndVertical();
						}
						GUILayout.EndHorizontal();
						
						GUI.color = oldColor;
						GUI.backgroundColor = oldBackColor;

						var rect = GUILayoutUtility.GetLastRect();
						var contains = rect.Contains(Event.current.mousePosition);

						if (layoutElement != null) {

							var size = 20f;
							var addButtonRect = rect;
							addButtonRect.x += addButtonRect.width - size - elementStyle.padding.right;
							addButtonRect.y -= elementStyle.margin.bottom;
							addButtonRect.width = size;
							addButtonRect.height = size;

							if (GUI.Button(addButtonRect, "+", EditorStyles.toolbarButton) == true) {

								// Add New Layout Element before current
								this.CreateLayoutElement(element);
								return;

							}

							var removeButtonRect = addButtonRect;
							removeButtonRect.x -= removeButtonRect.width;

							if (GUI.Button(removeButtonRect, "-", EditorStyles.toolbarButton) == true) {

								this.RemoveLayoutElement(element);
								return;
								
							}

							contains = contains && !addButtonRect.Contains(Event.current.mousePosition);

						}

						if (Event.current.type == EventType.MouseDown && contains == true) {

							Selection.activeTransform = element;

						}

					});

				}
				
				EditorGUILayout.EndScrollView();

			} else {
				
				var style = new GUIStyle(EditorStyles.whiteLargeLabel);
				style.alignment = TextAnchor.MiddleCenter;
				style.normal.textColor = Color.gray;
				
				var rect = this.position;
				rect.x = 0f;
				rect.y = 0f;
				GUI.Label(rect, "No Scene and Layout Selected", style);
				
			}

		}

		public void RemoveLayoutElement(Transform element) {
			
			EditorApplication.delayCall += () => {
				
				//GameObject.DestroyImmediate(element.gameObject);
				//FlowSceneView.GetItem().SetLayoutDirty();

				FlowDatabase.RemoveLayoutComponent(element);

			};

		}

		public void CreateLayoutElement(Transform element) {
			
			EditorApplication.delayCall += () => {
				
				FlowDatabase.AddLayoutElementComponent("LayoutElement", element.parent, element.GetSiblingIndex());

			};

		}
		
		public override void OnClose() {
			
			FlowSceneView.Reset(this.rootWindow.OnItemProgress, onDestroy: true);
			
		}

		public void Traverse(Transform root, System.Action<Transform, int> handler) {

			this.IterateGameObject(root, handler, true);

		}

		public void IterateGameObject(Transform root, System.Action<Transform, int> handler, bool recursive) {

			this.DoIterate(root, handler, recursive, 0);

		}
		
		public void DoIterate(Transform root, System.Action<Transform, int> handler, bool recursive, int depthLevel) {

			handler(root, depthLevel);
			
			if (root == null) return;

			foreach (var child in root) {

				if (recursive) {

					this.DoIterate(child as Transform, handler, recursive, depthLevel + 1);

				}

			}
		}

	}

}