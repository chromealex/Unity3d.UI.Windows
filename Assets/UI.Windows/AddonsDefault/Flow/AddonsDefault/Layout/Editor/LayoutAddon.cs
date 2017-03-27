using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEditorInternal;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEditor.UI.Windows.Plugins.Flow.Layout;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using UnityEditor.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Plugins.Layout {

	public class Layout : FlowAddon {

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

			public Styles() {

				this.skin = UnityEngine.Resources.Load(string.Format("UI.Windows/Flow/Styles/Skin{0}", (EditorGUIUtility.isProSkin == true ? "Dark" : "Light"))) as GUISkin;
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
					this.dropShadow = this.skin.FindStyle("DropShadow");

				}

			}

		}

		public static Styles styles = null;
		public const float MARGIN = 40f;
		public const float OFFSET = 4f;

		//private FD.FlowWindow window;
		private LayoutWindowType screen;
		private bool opened = false;
		private IPreviewEditor editor;
		private WindowLayoutElement element;
		private Vector2 listScrollPosition;
		private Vector2 settingsScrollPosition;

		public override void OnFlowSettingsGUI() {
			
			GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

		}
		
		private UnityEngine.UI.Windows.Types.Layout.Component component;
		private UnityEngine.UI.Windows.Types.Layout.Component hovered;
		private float allListHeight;
		private List<SerializedProperty> props = new List<SerializedProperty>();
		private List<WindowLayoutElement> highlighted = new List<WindowLayoutElement>();
		public override void OnGUI() {

			if (Layout.styles == null) Layout.styles = new Styles();

			if (this.opened == true) {

				const float settingsWidth = 320f;
				const float listHeight = 200f;
				const float padding = 5f;
				const float closeSize = 30f;
				const float scrollWidth = 16f;

				var rect = new Rect(0f, -OFFSET, Screen.width, Screen.height - OFFSET).PixelPerfect();
				var rectContent = new Rect(rect.x + MARGIN + settingsWidth + padding, rect.y + MARGIN, rect.width - MARGIN * 2f - padding - settingsWidth, rect.height - MARGIN * 2f - FlowSystemEditorWindow.TOOLBAR_HEIGHT).PixelPerfect();
				var rectList = new Rect(MARGIN, rect.y + MARGIN, settingsWidth, listHeight - padding).PixelPerfect();
				var rectSettings = new Rect(MARGIN, rect.y + MARGIN + listHeight, settingsWidth, rect.height - MARGIN * 2f - FlowSystemEditorWindow.TOOLBAR_HEIGHT - listHeight).PixelPerfect();
				var rectCloseButton = new Rect(rectContent.x + rectContent.width, rectContent.y - closeSize * 0.5f, closeSize, closeSize).PixelPerfect();
				
				GUI.Box(new Rect(rect.x + MARGIN, rect.y + MARGIN, rect.width - MARGIN * 2f, rect.height - MARGIN * 2f - FlowSystemEditorWindow.TOOLBAR_HEIGHT).PixelPerfect(), string.Empty, Layout.styles.layoutBack);
				GUI.Box(rect, string.Empty, Layout.styles.backLock);
				GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height).PixelPerfect(), string.Empty, Layout.styles.dropShadow);
				GUI.Box(rectList, string.Empty, Layout.styles.content);
				GUI.Box(rectSettings, string.Empty, Layout.styles.content);
				GUI.Box(rectContent, string.Empty, Layout.styles.contentScreen);
				
				GUI.BeginGroup(rectSettings);
				{
					if (this.component != null) {

						const float offsetTop = 50f;
						
						var viewRect = new Rect(0f, 0f, rectSettings.width, 0f).PixelPerfect();
						var scrollView = new Rect(0f, 0f + offsetTop, rectSettings.width, rectSettings.height - offsetTop).PixelPerfect();
						
						System.Action<WindowComponent> onChange = (WindowComponent component) => {

							//Debug.Log(component + "!=" + this.component.component);
							if (component != this.component.component) {

								this.component.componentParametersEditor = null;
								this.component.componentParameters = this.component.OnComponentChanged(this.screen, component);

								if (this.component.componentParameters != null) {
									
									var e = Editor.CreateEditor(this.component.componentParameters) as IParametersEditor;
									this.component.componentParametersEditor = e;

								}

								UnityEditor.EditorUtility.SetDirty(this.screen);

							}

						};

						var c = EditorGUI.ObjectField(new Rect(5f, 5f, viewRect.width - 40f - 5f * 2f, 16f).PixelPerfect(), this.component.component, typeof(WindowComponent), allowSceneObjects: false) as WindowComponent;
						if (c != this.component.component) {

							onChange(c);

						}

						var nRect = new Rect(viewRect.width - 40f, 5f, 40f - 5f, 16f).PixelPerfect();
						GUILayoutExt.DrawComponentChooser(nRect, this.screen.gameObject, this.component.component, (component) => {

							onChange(component);

						});
						
						if (this.component.component != null) {
							
							var editor = this.component.componentParametersEditor;

							nRect.x = 5f;
							nRect.width = viewRect.width - 5f * 2f;
							nRect.y += nRect.height + 5f;
							this.component.sortingOrder = EditorGUI.IntField(nRect, new GUIContent("Sorting Order"), this.component.sortingOrder);

							if (editor != null) {

								var h = Mathf.Max(scrollView.height, (editor == null) ? 0f : editor.GetHeight());
								viewRect = new Rect(scrollView.x, scrollView.y, viewRect.width - scrollWidth, h).PixelPerfect();

								var oldSkin = GUI.skin;
								GUI.skin = FlowSystemEditorWindow.defaultSkin;
								this.settingsScrollPosition = GUI.BeginScrollView(scrollView, this.settingsScrollPosition, viewRect, false, true);
								GUI.skin = oldSkin;
								{
									if (editor != null) {
										
										EditorGUIUtility.labelWidth = 100f;
										//++EditorGUI.indentLevel;
										editor.OnParametersGUI(viewRect);
										//--EditorGUI.indentLevel;
										EditorGUIUtilityExt.LookLikeControls();
										
									}
								}
								GUI.EndScrollView();

							} else {

								GUI.Label(new Rect(0f, 0f, rectSettings.width - scrollWidth, rectSettings.height).PixelPerfect(), "Selected component have no parameters", EditorStyles.centeredGreyMiniLabel);

							}

						}

					} else {
						
						GUI.Label(new Rect(0f, 0f, rectSettings.width - scrollWidth, rectSettings.height).PixelPerfect(), "Select an Element", EditorStyles.centeredGreyMiniLabel);
						
					}
				}
				GUI.EndGroup();

				GUI.BeginGroup(rectList);
				{

					const float itemHeight = 30f;

					this.highlighted.Clear();

					var viewRect = new Rect(0f, 0f, rectList.width - scrollWidth, 0f).PixelPerfect();
					this.allListHeight = 0f;
					for (int i = 0; i < this.props.Count; ++i) {
						
						var root = this.screen.GetCurrentLayout().layout.GetRootByTag(this.screen.GetCurrentLayout().components[i].tag);
						if (root.showInComponentsList == false) continue;

						if (this.screen.GetCurrentLayout().components[i].component == null) {

							this.highlighted.Add(root);

						}

						this.allListHeight += itemHeight;
						
					}

					viewRect.height = Mathf.Max(rectList.height, this.allListHeight);

					var oldSkin = GUI.skin;
					GUI.skin = FlowSystemEditorWindow.defaultSkin;
					this.listScrollPosition = GUI.BeginScrollView(new Rect(0f, 0f, rectList.width, rectList.height).PixelPerfect(), this.listScrollPosition, viewRect, false, true);
					GUI.skin = oldSkin;
					{
						GUI.BeginGroup(viewRect);
						{
							var h = 0f;
							this.hovered = null;
							for (int i = 0; i < this.props.Count; ++i) {

								var root = this.screen.GetCurrentLayout().layout.GetRootByTag(this.screen.GetCurrentLayout().components[i].tag);
								if (root.showInComponentsList == false) continue;

								var r = new Rect(0f, h, viewRect.width, itemHeight).PixelPerfect();
								h += r.height;

								var isSelected = (this.element == root);
								if (isSelected == true) {

									GUI.Label(r, this.screen.GetCurrentLayout().components[i].description, Layout.styles.listButtonSelected);

								} else {
									
									//r.width -= scrollWidth;
									if (GUI.Button(r, this.screen.GetCurrentLayout().components[i].description, Layout.styles.listButton) == true) {

										this.component = this.screen.GetCurrentLayout().components.FirstOrDefault(c => c.tag == root.tag);
										this.element = root;

									}

									var inRect = rectList.Contains(Event.current.mousePosition - this.listScrollPosition + Vector2.up * 40f);
									if (GUI.enabled == true) EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
									if (r.Contains(Event.current.mousePosition) == true && inRect == true) {
										
										this.hovered = this.screen.GetCurrentLayout().components[i];
										
									}
									//r.width += scrollWidth;

								}

								//r.width -= scrollWidth;
								GUI.Label(r, this.screen.GetCurrentLayout().components[i].tag.ToString(), Layout.styles.listTag);

							}
						}
						GUI.EndGroup();
					}
					GUI.EndScrollView();

				}
				GUI.EndGroup();

				var selected = (this.hovered != null) ? this.screen.GetCurrentLayout().layout.GetRootByTag(this.hovered.tag) : this.element;
				this.editor.OnPreviewGUI(Color.white, rectContent, Layout.styles.content, selected: selected, onSelection: (element) => {

					this.component = this.screen.GetCurrentLayout().components.FirstOrDefault(c => c.tag == element.tag);
					this.element = element;

				}, highlighted: this.highlighted);

				if (GUI.Button(rectCloseButton, string.Empty, Layout.styles.closeButton) == true) {
					
					this.flowEditor.SetEnabled();
					this.opened = false;
					
				}

			}

		}

		public override void OnFlowWindowScreenMenuGUI(FD.FlowWindow windowSource, GenericMenu menu) {

			menu.AddItem(new GUIContent("Components Editor..."), on: false, func: (object win) => {

				var window = win as FD.FlowWindow;
				var screen = window.GetScreen().Load<LayoutWindowType>();
				if (screen != null) {

					this.flowEditor.SetDisabled();
					//this.window = window;
					this.screen = screen;
					this.editor = Editor.CreateEditor(window.GetScreen().Load<WindowBase>()) as IPreviewEditor;
					
					this.component = null;
					this.hovered = null;
					this.element = null;
					this.listScrollPosition = Vector2.zero;
					var serializedObject = new SerializedObject(this.screen);
					var layout = serializedObject.FindProperty("layout");
					var components = layout.FindPropertyRelative("components");
					this.props.Clear();
					for (int i = 0; i < components.arraySize; ++i) {
						
						var component = components.GetArrayElementAtIndex(i);
						this.props.Add(component);
						
						var componentParametersEditor = this.screen.GetCurrentLayout().components[i].OnComponentChanged(this.screen, this.screen.GetCurrentLayout().components[i].component);
						if (componentParametersEditor != null) {
							
							var e = Editor.CreateEditor(componentParametersEditor) as IParametersEditor;
							this.screen.GetCurrentLayout().components[i].componentParametersEditor = e;
							
						}

					}
					
					this.settingsScrollPosition = Vector2.zero;
					
					this.opened = true;

				}

			}, userData: windowSource);

		}

		//private GUIStyle layoutBoxStyle;
		/*public override void OnFlowWindowGUI(FD.FlowWindow window) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			if (data.modeLayer == ModeLayer.Flow) {

				if (window.IsContainer() == true ||
				    window.IsSmall() == true ||
				    window.IsShowDefault() == true)
					return;

				var screen = window.GetScreen() as LayoutWindowType;
				if (screen != null) {

					GUILayout.BeginVertical();
					{
						if (this.layoutBoxStyle == null) {
							
							this.layoutBoxStyle = FlowSystemEditorWindow.defaultSkin.FindStyle("LayoutBox");
							
						}

						GUILayout.FlexibleSpace();

						GUILayout.BeginHorizontal();
						{
							GUILayout.FlexibleSpace();

							if (GUILayoutExt.LargeButton("Open Editor", 50f, 160f) == true) {

							}

							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();

						GUILayout.FlexibleSpace();

					}
					GUILayout.EndVertical();

				}

			}

		}*/

		public override bool InstallationNeeded() {

			return false;

		}

	}

}