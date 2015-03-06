using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview {

	public class DevicePreviewWindow : EditorWindow {

		public class GameView {

			public float width = 600f;
			public float height = 400f;

			private bool isActive = false;

			public void Resize(float width, float height) {

				this.width = width;
				this.height = height;

			}

			private DevicePreviewCamera previewCamera;

			private RenderTexture tempRenderTexture;
			private int currentWidth;
			private int currentHeight;
			private float ppi;
			public bool SetResolution(int width, int height, float ppi) {

				var result = false;

				this.ppi = ppi;
				this.currentWidth = width;
				this.currentHeight = height;

				this.tempRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

				if (this.previewCamera == null) {

					var go = new GameObject("__TempCamera");
					go.AddComponent<Camera>();
					this.previewCamera = go.AddComponent<DevicePreviewCamera>();

				}

				this.previewCamera.Initialize(this.tempRenderTexture);

				var size = new Vector2(this.width, this.height);
				var imageSize = new Vector2(this.currentWidth, this.currentHeight);
				var factor = this.GetFactor(imageSize, size);
				var k = Screen.dpi / this.ppi;

				if (factor / k < 1f) {

					result = false;

				}

				this.isActive = true;

				this.Update();

				return result;

			}

			public void CleanUp() {

				if (this.previewCamera != null) this.previewCamera.CleanUp();

			}

			public void Update() {

				if (this.isActive == false) return;

			}
			
			public void OnGUI() {
				
				if (this.isActive == false) {

					GUILayout.Label("No Selection");

					return;

				}

				var rectStyle = new GUIStyle("flow node 0");
				
				var size = new Vector2(this.width, this.height);
				
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
			
			private float GetFactor(Vector2 inner, Vector2 boundingBox) {     
				
				float widthScale = 0, heightScale = 0;
				if (inner.x != 0)
					widthScale = boundingBox.x / inner.x;
				if (inner.y != 0)
					heightScale = boundingBox.y / inner.y;                
				
				return Mathf.Min(widthScale, heightScale);
				
			}

		}
		
		private const float PANEL_WIDTH = 200f;
		private const float MARGIN = 5f;

		[MenuItem("Window/UI.Windows: Device Preview")]
		public static void ShowEditor() {
			
			var editor = DevicePreviewWindow.CreateInstance<DevicePreviewWindow>();
			editor.title = "UI.Windows: Device Preview";
			editor.position = new Rect(0f, 0f, 1f, 1f);

			editor.gameView = new GameView();

			editor.Show();
			
			editor.Focus();

		}

		public void OnDisable() {
			
			this.Validate();

			this.gameView.CleanUp();

		}

		public void Validate() {

			if (this.gameView == null) this.gameView = new GameView();

		}

		public void Update() {
			
			this.Validate();

			this.gameView.Update();

		}

		private void OnGUI() {

			this.Validate();
			
			var rectStyle = new GUIStyle("flow node 0");

			var rect = new Rect(0f, 0f, PANEL_WIDTH, this.position.height);

			GUI.Box(rect, string.Empty, rectStyle);

			GUILayout.BeginArea(rect);
			{

				this.DrawDevicesList();

			}
			GUILayout.EndArea();

			rect = new Rect(PANEL_WIDTH + MARGIN, 0f, this.position.width - PANEL_WIDTH - MARGIN, this.position.height);

			this.gameView.Resize(rect.width, rect.height);

			GUI.Box(rect, string.Empty, rectStyle);

			GUILayout.BeginArea(rect);
			{

				this.gameView.OnGUI();

			}
			GUILayout.EndArea();

		}

		public GameView gameView;

		private List<bool> directoriesFoldOut = new List<bool>();
		
		private int selectedId = -1;
		private Vector2 scrollPosition;
		private double lastClickTime = 0d;

		private void DrawDevicesList() {

			var scrollWidth = 20f;
			var offset = 10;

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

			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, false, false);
			
			var i = 0;
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
								
								item.width = Mathf.RoundToInt(this.gameView.width);
								item.height = Mathf.RoundToInt(this.gameView.height);
								item.orientation = item.orientation;
								
							}
							
							if (GUILayout.Button(item.model, style) == true) {
								
								// Apply Device Info
								if (EditorApplication.timeSinceStartup - this.lastClickTime < 0.3d && this.selectedId == i) {
									
									item.orientation = (item.orientation == ScreenOrientation.Landscape) ? ScreenOrientation.Portrait : ScreenOrientation.Landscape;
									
								}
								
								this.selectedId = i;

								this.lastClickTime = EditorApplication.timeSinceStartup;
								
								if (this.gameView.SetResolution(item.width, item.height, item.ppi) == true) {

									this.ShowNotification(new GUIContent("Drawing screen (" + item.width.ToString() + "x" + item.height.ToString() + ") doesn't fit into your window size (" + this.gameView.width.ToString() + "x" + this.gameView.height.ToString() + ")."));

								} else {
									
									this.RemoveNotification();
									
								}

								this.Repaint();
								
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
				
			}
			
			EditorGUILayout.EndScrollView();

		}

	}

}