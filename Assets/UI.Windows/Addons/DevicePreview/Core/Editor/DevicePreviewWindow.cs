using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using System;
using UnityEditor.UI.Windows.Plugins.DevicePreview.Output;
using UnityEngine.UI.Windows.Plugins.DevicePreview;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview {

	public class DevicePreviewWindow : EditorWindowExt {

		public class Styles {
			
			public GUIStyle leftPanel;
			public GUIStyle contentPanel;
			
			public GUIStyle listFoldout;
			public GUIStyle listFoldoutHovered;

			public GUIStyle listButton;
			public GUIStyle listButtonHovered;
			public GUIStyle listButtonPortrait;
			public GUIStyle listButtonPortraitHovered;

			public GUIStyle warning;
			public GUIStyle screen;

			public GUISkin skin;

			public Styles() {

				this.skin = UnityEngine.Resources.Load<GUISkin>("UI.Windows/DevicePreview/Styles/Skin" + (EditorGUIUtility.isProSkin == true ? "Dark" : "Light"));
				if (this.skin != null) {

					this.leftPanel = this.skin.FindStyle("LeftPanel");
					this.contentPanel = this.skin.FindStyle("ContentPanel");
					
					this.listFoldout = this.skin.FindStyle("ListFoldout");
					this.listFoldoutHovered = this.skin.FindStyle("ListFoldoutHovered");

					this.listButton = this.skin.FindStyle("ListButton");
					this.listButtonHovered = this.skin.FindStyle("ListButtonHovered");
					this.listButtonPortrait = this.skin.FindStyle("ListButtonPortrait");
					this.listButtonPortraitHovered = this.skin.FindStyle("ListButtonPortraitHovered");

					this.warning = this.skin.FindStyle("Warning");
					this.screen = this.skin.FindStyle("Screen");

				}

			}

			public bool IsValid() {

				return this.skin != null && this.leftPanel != null;

			}

		}

		public static Styles styles = null;

		public class GameView {

			public DevicePreviewWindow root;

			public float width = 600f;
			public float height = 400f;
			
			private IDeviceOutput deviceOutput;

			private bool isActive = false;

			public GameView(DevicePreviewWindow root) {

				this.root = root;
				
				EditorApplication.update += this.OnUpdate;

			}

			~GameView() {
				
				EditorApplication.update -= this.OnUpdate;

			}

			public void Resize(float width, float height) {

				this.width = width;
				this.height = height;

			}

			//private DevicePreviewCamera previewCamera;
			private RenderTexture tempRenderTexture;
			private Texture2D tempScreenshot;

			private int currentWidth;
			private int currentHeight;
			private float ppi;
			private ScreenOrientation orientation;
			public bool SetResolution(int width, int height, float ppi, ScreenOrientation orientation, IDeviceOutput deviceOutput) {

				var result = false;

				this.orientation = orientation;
				this.deviceOutput = deviceOutput;

				this.ppi = ppi;
				this.currentWidth = width;
				this.currentHeight = height;
				
				this.tempScreenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

				this.tempRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				/*
				if (this.previewCamera == null) {

					var go = new GameObject("__TempCamera");
					this.previewCamera = go.AddComponent<DevicePreviewCamera>();

				}

				this.previewCamera.Initialize(this.tempRenderTexture, () => {

					this.root.Repaint();
					GameObject.DestroyImmediate(this.previewCamera.gameObject);

				});*/

				var size = new Vector2(this.width, this.height);
				var imageSize = new Vector2(this.currentWidth, this.currentHeight);
				var factor = this.GetFactor(imageSize, size);
				var k = Screen.dpi / this.ppi;

				if (factor / k < 1f) {

					result = true;

				}

				this.isActive = true;

				this.Update();

				return result;

			}

			public void CleanUp() {

				//if (this.previewCamera != null) this.previewCamera.CleanUp();

			}

			public void Update() {

				if (this.isActive == false) return;

				this.OnUpdate();

			}

			public void OnGUI(Rect rect) {
				
				if (this.isActive == false) {

					var warningOffset = 15f;

					var style = DevicePreviewWindow.styles.warning;//new GUIStyle("flow node 6");
					if (style == null) return;
					style.alignment = TextAnchor.MiddleCenter;
					style.fontStyle = FontStyle.Bold;
					style.margin = new RectOffset();
					style.padding = new RectOffset();
					style.contentOffset = Vector2.zero;

					var content = new GUIContent("YOU HAVE NO DEVICE SELECTED");
					var contentRect = GUILayoutUtility.GetRect(content, style);
					
					var width = contentRect.width + warningOffset;
					var height = contentRect.height + warningOffset;

					var centerRect = new Rect(DevicePreviewWindow.PANEL_WIDTH + DevicePreviewWindow.MARGIN + this.width * 0.5f - width * 0.5f, this.height * 0.5f - height * 0.5f, width, height);
					GUI.Label(centerRect, content, style);

					this.root.selectedId = -1;

					return;

				}

				var rectStyle = DevicePreviewWindow.styles.screen;//new GUIStyle("flow node 0");
				
				var size = new Vector2(this.width, this.height);
				
				GUI.Box(new Rect(rect.x, rect.y, size.x, size.y), string.Empty, rectStyle);
				
				var drawRect = this.GetRectFromSize(this.currentWidth, this.currentHeight);
				var drawRectLandscape = this.GetRectFromSize(this.orientation == ScreenOrientation.Landscape ? this.currentWidth : this.currentHeight, this.orientation == ScreenOrientation.Landscape ? this.currentHeight : this.currentWidth);

				var deviceOutput = this.deviceOutput as IDeviceOutput;
				if (deviceOutput == null) {
					
					var offset = 20f;
					var backRect = new Rect(DevicePreviewWindow.PANEL_WIDTH + DevicePreviewWindow.MARGIN + drawRect.x - offset, DevicePreviewWindow.MARGIN + drawRect.y - offset, drawRect.width + offset * 2f, drawRect.height + offset * 2f);
					drawRect.x += DevicePreviewWindow.PANEL_WIDTH + DevicePreviewWindow.MARGIN;
					drawRect.y += DevicePreviewWindow.MARGIN;
					var oldColor = GUI.color;
					GUI.color = Color.black;
					GUI.Box(backRect, string.Empty, rectStyle);
					GUI.color = oldColor;

				}

				if (deviceOutput != null) {

					deviceOutput.SetRect(rect, new Rect(0f, 0f, size.x, size.y), drawRect, drawRectLandscape, this.orientation);

					deviceOutput.DoPreGUI();

				}

				if (this.tempRenderTexture != null) GUI.DrawTexture(drawRect, this.tempRenderTexture);
				
				if (deviceOutput != null) {

					deviceOutput.DoPostGUI();
					
				}
				
				EventType type = Event.current.type;
				EditorGUIUtility.AddCursorRect(drawRect, MouseCursor.CustomCursor);
				if (type == EventType.Repaint) {

					//EditorGUIUtility.RenderGameViewCameras(cameraRect: new Rect(0f, 0f, 300f, 300f), targetDisplay: 0, gizmos: false, gui: true);

				} if (type != EventType.Layout && type != EventType.Used) {

					bool flag = drawRect.Contains(Event.current.mousePosition);
					if (Event.current.rawType == EventType.MouseDown && !flag)
					{
						return;
					}
					Event.current.mousePosition = new Vector2(Event.current.mousePosition.x - drawRect.x, Event.current.mousePosition.y - drawRect.y);
					EditorGUIUtility.QueueGameViewInputEvent(Event.current);
					bool flag2 = true;
					if (Event.current.rawType == EventType.MouseUp && !flag)
					{
						flag2 = false;
					}
					if (type == EventType.ExecuteCommand || type == EventType.ValidateCommand)
					{
						flag2 = false;
					}
					if (flag2)
					{
						Event.current.Use();
					}
					else
					{
						Event.current.mousePosition = new Vector2(Event.current.mousePosition.x + drawRect.x, Event.current.mousePosition.y + drawRect.y);
					}

				}

			}

			public void OnUpdate() {

				if (this.isActive == false) return;

				// Clear screen
				Graphics.Blit(Texture2D.blackTexture, this.tempRenderTexture); 
				
				var allCameras = Camera.allCameras.OrderBy((c) => c.depth);
				foreach (var camera in allCameras) {

					Graphics.Blit(this.TakeScreenshot(camera), this.tempRenderTexture); 

				}

			}
			
			public Texture2D TakeScreenshot(Camera camera) {

				if (this.tempRenderTexture == null) {

					return null;

				}

				var width = this.tempRenderTexture.width;
				var height = this.tempRenderTexture.height;

				camera.targetTexture = this.tempRenderTexture;
				camera.Render();
				
				RenderTexture.active = this.tempRenderTexture;
				
				this.tempScreenshot.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
				this.tempScreenshot.Apply(false);
				
				camera.targetTexture = null;
				
				RenderTexture.active = null;
				
				return this.tempScreenshot;
				
			}

			private float GetFactor(Vector2 inner, Vector2 boundingBox) {     
				
				float widthScale = 0, heightScale = 0;
				if (inner.x != 0) widthScale = boundingBox.x / inner.x;
				if (inner.y != 0) heightScale = boundingBox.y / inner.y;                
				
				return Mathf.Min(widthScale, heightScale);
				
			}
			
			private Rect GetRectFromSize(float currentWidth, float currentHeight) {
				
				var size = new Vector2(this.width, this.height);
				var imageSize = new Vector2(currentWidth, currentHeight);
				
				var factor = this.GetFactor(imageSize, size);
				var k = Screen.dpi / this.ppi;
				
				var newImageSize = Vector2.zero;
				if (factor / k < 1f) {
					
					newImageSize = imageSize * factor;
					
				} else {
					
					var deviceSize = imageSize * k;
					newImageSize = deviceSize;
					
				}
				
				return new Rect((size.x - newImageSize.x) * 0.5f, (size.y - newImageSize.y) * 0.5f, newImageSize.x, newImageSize.y);
				
			}

		}

		public const float PANEL_WIDTH = 250f;
		public const float MARGIN = 100f;
		public const float TOP_OFFSET = 21f;

		public static DevicePreviewWindow ShowEditor(System.Action onClose) {
			
			var editor = DevicePreviewWindow.CreateInstance<DevicePreviewWindow>();
			var title = "UIW Preview";
			#if !UNITY_4
			editor.titleContent = new GUIContent(title, UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/FlowIcon"));
			#else
			editor.title = title;
			#endif
			editor.position = new Rect(0f, 0f, 1f, 1f);

			editor.autoRepaintOnSceneChange = true;
			editor.wantsMouseMove = true;
			editor.gameView = new GameView(editor);

			editor.onClose = onClose;

			editor.Show();
			editor.Focus();

			Parser.Collect(forced: false);

			return editor;

		}
		
		public void OnFocus() {

			UnityEditorInternal.InternalEditorUtility.OnGameViewFocus(true);
			
		}
		
		public void OnLostFocus() {

			UnityEditorInternal.InternalEditorUtility.OnGameViewFocus(false);

		}

		public override void OnClose() {
			
			if (this.gameView != null) this.gameView.CleanUp();

			if (this.onClose != null) this.onClose();

		}

		public void Validate() {

			if (DevicePreviewWindow.styles == null || DevicePreviewWindow.styles.IsValid() == false) DevicePreviewWindow.styles = new Styles();

			if (Parser.manufacturerToDevices == null || Parser.manufacturerToDevices.Count == 0) {

				Parser.Collect(forced: false);

			}

			if (this.gameView == null) this.gameView = new GameView(this);

		}

		public override void Update() {
			
			this.gameView.Update();
			
			this.Repaint();

		}

		private void OnGUI() {

			this.Validate();
			if (DevicePreviewWindow.styles.IsValid() == false) return;
			
			var rectStyle = DevicePreviewWindow.styles.leftPanel;//new GUIStyle("flow node 0");
			if (rectStyle == null) return;

			Rect rect;
			
			rect = new Rect(0f, 0f, PANEL_WIDTH, this.position.height);
			GUI.Box(rect, string.Empty, rectStyle);
			GUILayout.BeginArea(rect);
			{
				
				this.DrawDevicesList();
				
			}
			GUILayout.EndArea();

			rect = new Rect(PANEL_WIDTH + MARGIN, MARGIN, this.position.width - PANEL_WIDTH - MARGIN * 2f, this.position.height - MARGIN * 2f);
			this.gameView.Resize(rect.width, rect.height);
			GUI.Box(rect, string.Empty, DevicePreviewWindow.styles.contentPanel);
			//GUI.BeginClip(rect);
			//GUILayout.BeginArea(rect);
			{
				
				this.gameView.OnGUI(rect);
				
			}
			//GUILayout.EndArea();
			//GUI.EndClip();

		}

		private System.Action onClose;

		public GameView gameView;

		private List<bool> directoriesFoldOut = new List<bool>();
		
		private int selectedId = -1;
		private Vector2 scrollPosition;
		private double lastClickTime = 0d;

		private void DrawDevicesList() {

			if (DevicePreviewWindow.styles.IsValid() == false) return;

			var scrollWidth = 10f;
			var offset = 10;

			var elementStyle = DevicePreviewWindow.styles.listButton;//new GUIStyle("flow node hex 0");
			if (elementStyle == null) return;
			elementStyle.contentOffset = Vector2.zero;
			elementStyle.fixedWidth = PANEL_WIDTH - scrollWidth - offset;
			elementStyle.margin = new RectOffset(offset, 0, 0, 0);
			elementStyle.padding = new RectOffset(20, 20, 10, 20);
			elementStyle.alignment = TextAnchor.MiddleLeft;
			
			var elementStyleSelected = DevicePreviewWindow.styles.listButtonHovered;//new GUIStyle("flow node hex 1 on");
			elementStyleSelected.contentOffset = Vector2.zero;
			elementStyleSelected.fixedWidth = PANEL_WIDTH - scrollWidth - offset;
			elementStyleSelected.margin = new RectOffset(offset, 0, 0, 0);
			elementStyleSelected.padding = new RectOffset(20, 20, 10, 20);
			elementStyleSelected.alignment = TextAnchor.MiddleLeft;
			
			var elementStylePortrait = DevicePreviewWindow.styles.listButtonPortrait;//new GUIStyle("flow node hex 2");
			elementStylePortrait.contentOffset = Vector2.zero;
			elementStylePortrait.fixedWidth = PANEL_WIDTH - scrollWidth - offset;
			elementStylePortrait.margin = new RectOffset(offset, 0, 0, 0);
			elementStylePortrait.padding = new RectOffset(20, 20, 10, 20);
			elementStylePortrait.alignment = TextAnchor.MiddleLeft;
			
			var elementStyleSelectedPortrait = DevicePreviewWindow.styles.listButtonPortraitHovered;//new GUIStyle("flow node hex 2 on");
			elementStyleSelectedPortrait.contentOffset = Vector2.zero;
			elementStyleSelectedPortrait.fixedWidth = PANEL_WIDTH - scrollWidth - offset;
			elementStyleSelectedPortrait.margin = new RectOffset(offset, 0, 0, 0);
			elementStyleSelectedPortrait.padding = new RectOffset(20, 20, 10, 20);
			elementStyleSelectedPortrait.alignment = TextAnchor.MiddleLeft;

			var headerLabel = new GUIStyle(EditorStyles.largeLabel);
			headerLabel.padding = new RectOffset(0, 0, 0, 0);
			headerLabel.margin = new RectOffset(0, 0, 0, 0);
			
			var miniLabel = new GUIStyle(EditorStyles.miniLabel);
			miniLabel.padding = new RectOffset(0, 0, 0, 0);
			miniLabel.margin = new RectOffset(0, 0, 0, 0);

			var foldOutStyle = DevicePreviewWindow.styles.listFoldout;//new GUIStyle("flow node hex 4");
			foldOutStyle.contentOffset = Vector2.zero;
			foldOutStyle.fixedWidth = PANEL_WIDTH - scrollWidth;
			foldOutStyle.font = headerLabel.font;
			foldOutStyle.fontSize = headerLabel.fontSize;
			foldOutStyle.padding = new RectOffset(20, 20, 15, 15);
			foldOutStyle.alignment = TextAnchor.MiddleLeft;
			
			var foldOutStyleSelected = DevicePreviewWindow.styles.listFoldoutHovered;//new GUIStyle("flow node hex 4 on");
			foldOutStyleSelected.contentOffset = Vector2.zero;
			foldOutStyleSelected.fixedWidth = PANEL_WIDTH - scrollWidth;
			foldOutStyleSelected.font = headerLabel.font;
			foldOutStyleSelected.fontSize = headerLabel.fontSize;
			foldOutStyleSelected.padding = new RectOffset(20, 20, 15, 15);
			foldOutStyleSelected.alignment = TextAnchor.MiddleLeft;
			
			var oldSkin = GUI.skin;
			GUI.skin = DevicePreviewWindow.styles.skin;
			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, false, false);
			GUI.skin = oldSkin;

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

								IDeviceOutput specialInstance = null;
								if (string.IsNullOrEmpty(item.deviceOutput) == false) {

									var outputNamespace = "UnityEditor.UI.Windows.Plugins.DevicePreview.Output";
									var type = outputNamespace + "." + item.deviceOutput;

									var specialOutput = System.Type.GetType(type);
									if (specialOutput != null) {

										specialInstance = Activator.CreateInstance(specialOutput) as IDeviceOutput;

									} else {

										Debug.LogWarning("Class not found: " + type);

									}

								}

								if (this.gameView.SetResolution(item.width, item.height, item.ppi, item.orientation, specialInstance) == true) {

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