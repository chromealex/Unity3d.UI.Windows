using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows.Plugins.Flow.Editors {

	public class FlowGameViewRenderWindow : EditorWindowExt {

		public static FlowGameViewRenderWindow Show(WindowBase previewScreen) {

			var window = FlowGameViewRenderWindow.CreateInstance<FlowGameViewRenderWindow>();
			window.previewScreen = previewScreen;
			window.ShowPopup();

			window.SetRenderSize(Screen.width, Screen.height, Screen.dpi);

			return window;

		}

		private WindowBase previewScreen;
		private RenderTexture tempRenderTexture;
		private int currentWidth;
		private int currentHeight;
		private float ppi;
		public void SetRenderSize(int width, int height, float ppi) {

			this.ppi = ppi;
			this.currentWidth = width;
			this.currentHeight = height;

			var layoutWindow = this.previewScreen as LayoutWindowType;
			if (layoutWindow != null) {
				
				var layout = layoutWindow.layout.GetLayoutInstance();
				
				this.tempRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				this.previewScreen.workCamera.targetTexture = this.tempRenderTexture;

				layout.canvasScaler.enabled = true;
				//layout.canvasScaler.drawAsWorld = true;
				//layout.canvasScaler.manualUpdate = true;

				layoutWindow.transition.StartRenderToTexture();

				var size = new Vector2(this.position.width, this.position.height);
				var imageSize = new Vector2(this.currentWidth, this.currentHeight);
				var factor = this.GetFactor(imageSize, size);
				var k = Screen.dpi / this.ppi;

				if (factor / k < 1f) {

					this.ShowNotification(new GUIContent("Drawing screen (" + width.ToString() + "x" + height.ToString() + ") doesn't fit into your window size (" + size.x.ToString() + "x" + size.y.ToString() + ")."));

				} else {

					this.RemoveNotification();

				}

				this.Repaint();

			}

		}

		private float GetFactor(Vector2 inner, Vector2 boundingBox) {     
			
			float widthScale = 0, heightScale = 0;
			if (inner.x != 0)
				widthScale = boundingBox.x / inner.x;
			if (inner.y != 0)
				heightScale = boundingBox.y / inner.y;                
			
			return Mathf.Min(widthScale, heightScale);
			
		}

		public override void Update() {

			base.Update();

			var layoutWindow = this.previewScreen as LayoutWindowType;
			if (layoutWindow != null) {

				//layoutWindow.layout.GetLayoutInstance().canvasScaler.UpdateWithMode(new Vector2(this.currentWidth, this.currentHeight));
				this.Repaint();

			}

		}

		public void OnGUI() {
			
			var rectStyle = new GUIStyle("flow node 0");

			var size = new Vector2(this.position.width, this.position.height);

			GUI.Box(new Rect(0f, 0f, size.x, size.y), string.Empty, rectStyle);

			var imageSize = new Vector2(this.currentWidth, this.currentHeight);

			var factor = this.GetFactor(imageSize, size);
			var k = Screen.dpi / this.ppi;
			
			var newImageSize = Vector2.zero;
			if (factor / k < 1f) {

				newImageSize = imageSize * factor;

			} else {

				var deviceSize = imageSize * k;
				newImageSize = deviceSize;

			}

			GUI.DrawTexture(new Rect((size.x - newImageSize.x) * 0.5f, (size.y - newImageSize.y) * 0.5f, newImageSize.x, newImageSize.y), this.tempRenderTexture);

		}

		public override void OnClose() {

			Object.DestroyImmediate(this.tempRenderTexture);
			this.tempRenderTexture = null;

		}

	}

	public class FlowGameViewWindow : EditorWindowExt {

		public FlowSystemEditorWindow rootWindow;
		private Rect popupRect;
		private Vector2 popupSize;
		private bool hided;

		private const float PANEL_WIDTH = 200f;

		private FlowGameViewRenderWindow gameView;
		private System.Action onClose;
		/*
		private PropertyInfo gameViewSizeProperty;
		private object gameViewSize;
		private System.Type gameViewSizeType;*/
		private WindowBase previewScreen;

		public void ShowView(WindowBase previewScreen, System.Action onClose) {

			this.previewScreen = previewScreen;
			this.onClose = onClose;

			this.ShowPopup();
			this.hided = false;
			
			this.minSize = new Vector2(this.minSize.x, 1f);
			this.progress = 1f;

			/*this.gameView = this.GetGameView();
			this.gameView.ShowPopup();
			this.gameView.Focus();*/

			this.gameView = FlowGameViewRenderWindow.Show(this.previewScreen);

			this.Focus();

			//this.selectedId = -1;
			//this.directoriesFoldOut = new List<bool>();

			this.gameView.ShowNotification(new GUIContent("Double-Click on any item in left list to toggle between Portrait/Landscape orientation"));

		}

		private void SetGameViewSize(int width, int height, float dpi) {
			
			//var x = orientation == ScreenOrientation.Landscape ? width : height;
			//var y = orientation == ScreenOrientation.Landscape ? height : width;

			/*this.gameViewSizeType.GetProperty("width", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(this.gameViewSize, x, new object[] {});
			this.gameViewSizeType.GetProperty("height", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(this.gameViewSize, y, new object[] {});
			*/

			this.gameView.SetRenderSize(width, height, dpi);

			/*var layoutWindow = (this.previewScreen as LayoutWindowType);
			if (layoutWindow != null) {

				var layout = layoutWindow.layout.GetLayoutInstance();
				if (layout != null) {

					// Set new render size

					//layout.SetScale(x, y, true);

				}

			}*/

		}

		/*private EditorWindow GetGameView() {

			var type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			var gameView = EditorWindow.CreateInstance(type) as EditorWindow;
			
			this.gameViewSizeProperty = gameView.GetType().GetProperty("currentGameViewSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			this.gameViewSize = this.gameViewSizeProperty.GetValue(gameView, new object[0]{});
			this.gameViewSizeType = this.gameViewSize.GetType();

			return gameView;

		}*/
		
		public void HideView() {
			
			this.hided = true;
			
		}

		public void OnFocus() {

			if (this.gameView != null) this.gameView.Focus();

		}

		public void OnLostFocus() {

			this.Close();

		}
		
		new public void Update() {
			
			base.Update();

			if (this.rootWindow == null) return;

			this.rootWindow.Update();
			
			if (this.hided == true) {
				
				this.minSize = Vector2.zero;
				this.popupRect = new Rect(0f, 0f, 1f, 1f);
				
			} else {
				
				var progress = this.progress;
				
				var popupOffset = FlowSceneItem.POPUP_OFFSET + 50f;
				this.popupSize = new Vector2(this.rootWindow.position.width - popupOffset * 2f, (this.rootWindow.position.height - popupOffset * 2f) * progress);
				this.popupRect = new Rect(this.rootWindow.position.x + popupOffset, this.rootWindow.position.y + popupOffset, PANEL_WIDTH, this.popupSize.y);
				
			}
			
			this.position = this.popupRect;
			if (this.gameView != null) {

				var rect = this.popupRect;
				rect.x += PANEL_WIDTH + FlowSceneItem.POPUP_MARGIN;
				rect.width = this.popupSize.x - PANEL_WIDTH - FlowSceneItem.POPUP_MARGIN;
				this.gameView.position = rect;

			}
			
			if (FlowSceneView.recompileChecker == null) this.Close();
			
		}
		
		//private List<bool> directoriesFoldOut = new List<bool>();

		//private int selectedId = -1;
		private Vector2 scrollPosition;
		//private double lastClickTime = 0d;

		public void OnGUI() {
			
			var scrollWidth = 20f;
			var offset = 10;

			var rectStyle = new GUIStyle("flow node 0");
			
			var elementStyle = new GUIStyle("flow node hex 0");
			elementStyle.contentOffset = Vector2.zero;
			elementStyle.fixedWidth = PANEL_WIDTH - scrollWidth - offset;
			elementStyle.margin = new RectOffset(offset, 0, 0, 0);
			elementStyle.padding = new RectOffset(20, 20, 10, 20);
			elementStyle.alignment = TextAnchor.MiddleLeft;
			
			var elementStyleSelected = new GUIStyle("flow node hex 1 on");
			elementStyleSelected.contentOffset = Vector2.zero;
			elementStyleSelected.fixedWidth = PANEL_WIDTH - scrollWidth - offset;
			elementStyleSelected.margin = new RectOffset(offset, 0, 0, 0);
			elementStyleSelected.padding = new RectOffset(20, 20, 10, 20);
			elementStyleSelected.alignment = TextAnchor.MiddleLeft;
			
			var elementStylePortrait = new GUIStyle("flow node hex 2");
			elementStylePortrait.contentOffset = Vector2.zero;
			elementStylePortrait.fixedWidth = PANEL_WIDTH - scrollWidth - offset;
			elementStylePortrait.margin = new RectOffset(offset, 0, 0, 0);
			elementStylePortrait.padding = new RectOffset(20, 20, 10, 20);
			elementStylePortrait.alignment = TextAnchor.MiddleLeft;
			
			var elementStyleSelectedPortrait = new GUIStyle("flow node hex 2 on");
			elementStyleSelectedPortrait.contentOffset = Vector2.zero;
			elementStyleSelectedPortrait.fixedWidth = PANEL_WIDTH - scrollWidth - offset;
			elementStyleSelectedPortrait.margin = new RectOffset(offset, 0, 0, 0);
			elementStyleSelectedPortrait.padding = new RectOffset(20, 20, 10, 20);
			elementStyleSelectedPortrait.alignment = TextAnchor.MiddleLeft;


			var headerLabel = new GUIStyle(EditorStyles.whiteLargeLabel);
			headerLabel.padding = new RectOffset(0, 0, 0, 0);
			headerLabel.margin = new RectOffset(0, 0, 0, 0);
			
			var miniLabel = new GUIStyle(EditorStyles.miniLabel);
			miniLabel.padding = new RectOffset(0, 0, 0, 0);
			miniLabel.margin = new RectOffset(0, 0, 0, 0);

			
			var foldOutStyle = new GUIStyle("flow node hex 4");
			foldOutStyle.contentOffset = Vector2.zero;
			foldOutStyle.fixedWidth = PANEL_WIDTH - scrollWidth;
			foldOutStyle.font = headerLabel.font;
			foldOutStyle.fontSize = headerLabel.fontSize;
			foldOutStyle.padding = new RectOffset(20, 20, 15, 15);
			foldOutStyle.alignment = TextAnchor.MiddleLeft;
			
			var foldOutStyleSelected = new GUIStyle("flow node hex 4 on");
			foldOutStyleSelected.contentOffset = Vector2.zero;
			foldOutStyleSelected.fixedWidth = PANEL_WIDTH - scrollWidth;
			foldOutStyleSelected.font = headerLabel.font;
			foldOutStyleSelected.fontSize = headerLabel.fontSize;
			foldOutStyleSelected.padding = new RectOffset(20, 20, 15, 15);
			foldOutStyleSelected.alignment = TextAnchor.MiddleLeft;


			var boxRect = this.position;
			boxRect.x = 0f;
			boxRect.y = 0f;
			GUI.Box(boxRect, string.Empty, rectStyle);

			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, false, false);
			
			/*var i = 0;
			var directories = Parser.manufacturerToDevices;
			if (this.directoriesFoldOut.Count != directories.Count) {

				this.directoriesFoldOut = new List<bool>();

				for (i = 0; i < directories.Count; ++i) {

					this.directoriesFoldOut.Add(false);

				}

			}

			i = 0;
			var f = 0;
			foreach (var directory in directories) {

				if (GUILayout.Button(directory.Key, this.directoriesFoldOut[f] ? foldOutStyleSelected : foldOutStyle) == true) {

					this.directoriesFoldOut[f] = !this.directoriesFoldOut[f];

				}

				if (this.directoriesFoldOut[f] == true) {
					
					GUILayout.BeginVertical();

					var list = directory.Value;
					if (list != null && list.Count > 0) {

						foreach (var item in list) {

							var style = this.selectedId == i ? (item.orientation == ScreenOrientation.Landscape ? elementStyleSelected : elementStyleSelectedPortrait) : (item.orientation == ScreenOrientation.Landscape ? elementStyle : elementStylePortrait);
							
							if (item.current == true) {
								
								item.width = Mathf.RoundToInt(this.gameView.position.width);
								item.height = Mathf.RoundToInt(this.gameView.position.height);
								item.orientation = item.orientation;
									
							}

							if (GUILayout.Button(item.model, style) == true) {
								
								// Apply Device Info
								this.selectedId = i;

								if (EditorApplication.timeSinceStartup - this.lastClickTime < 0.3d) {

									item.orientation = (item.orientation == ScreenOrientation.Landscape) ? ScreenOrientation.Portrait : ScreenOrientation.Landscape;

								}

								this.lastClickTime = EditorApplication.timeSinceStartup;

								this.SetGameViewSize(item.width, item.height, item.ppi);

							}

							var rect = GUILayoutUtility.GetLastRect();
							rect.x += style.padding.left;
							rect.y += rect.height - style.padding.bottom;
							GUI.Label(rect, "Resolution: " + item.width.ToString() + "x" + item.height.ToString() + " (" + item.ppi.ToString() + ")", miniLabel);

							++i;

						}

					}

					GUILayout.EndVertical();

				} else {

					i += directory.Value.Count;

				}

				++f;

			}*/

			EditorGUILayout.EndScrollView();

		}

		public override void OnClose() {

			if (this.gameView != null) this.gameView.Close();
			if (this.onClose != null) this.onClose();

		}
		
	}
	
}