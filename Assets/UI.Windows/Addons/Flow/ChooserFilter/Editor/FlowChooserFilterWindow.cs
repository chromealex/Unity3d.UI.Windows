using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowChooserFilterWindow : EditorWindow {
		
		private EditorWindow root;
		private bool isLocked;

		private System.Action<Component> onSelect;
		private System.Action<Component> onEveryGUI;
		private List<Component> items = new List<Component>();

		public static void Show<T>(EditorWindow root, System.Action<T> onSelect, System.Action<T> onEveryGUI = null, bool strongType = false) where T : Component {

			var editor = FlowChooserFilterWindow.CreateInstance<FlowChooserFilterWindow>();
			editor.title = "UI.Windows Flow Filter";
			editor.position = new Rect(0f, 0f, 1f, 1f);

			editor.isLocked = false;
			if (editor.isLocked == true) {

				editor.ShowPopup();

			} else {

				editor.ShowUtility();

			}

			editor.root = root;

			editor.onSelect = (c) => {

				if (onSelect != null) onSelect(c as T);
				editor.Close();

			};

			editor.onEveryGUI = (c) => {

				if (onEveryGUI != null) onEveryGUI(c as T);

			};

			editor.Scan<T>(strongType, "CACHE" + strongType.ToString());

			editor.UpdateSize();
			//editor.Focus();

		}

		public void OnLostFocus() {

			this.Close();

		}

		public void Scan<T>(bool strongType, string cacheKey) where T : Component {

			this.items = ME.EditorUtilities.GetPrefabsOfTypeRaw<T>().ToList();

		}

		public void UpdateSize() {
			
			var offset = FlowSceneItem.POPUP_OFFSET + 50f;
			this.position = new Rect(this.root.position.x + offset, this.root.position.y + offset, this.root.position.width - offset * 2f, this.root.position.height - offset * 2f);

		}

		public void Update() {

			if (this.isLocked == true) {

				this.UpdateSize();

			}

			if (FlowSceneView.recompileChecker == null) this.Close();

		}

		private Vector2 scrollPos;
		private void OnGUI() {

			//var scrollWidth = 30f;
			
			var width = 160f;
			var height = 160f;

			var itemBox = new GUIStyle(GUI.skin.box);
			itemBox.fixedWidth = 0f;
			itemBox.stretchWidth = false;

			var box = new GUIStyle(GUI.skin.box);
			box.fixedWidth = 0f;
			box.stretchWidth = true;

			box.margin = new RectOffset();
			var style = new GUIStyle(EditorStyles.toolbarButton);

			var rect = this.position;
			rect.x = 0f;
			rect.y = 0f;
			GUI.Box(rect, string.Empty);

			this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, false, false);
			{

				GUILayout.BeginVertical();
				{

					var xCount = Mathf.FloorToInt(this.position.width / width);
					if (xCount > 0) {

						style.fixedWidth = width;
						style.fixedHeight = height;
						style.stretchWidth = false;
						style.stretchHeight = false;

						var i = 0;
						foreach (var item in this.items) {

							if (item == null) continue;

							if (i % xCount == 0) {

								if (i != 0) GUILayout.EndHorizontal();
								GUILayout.BeginHorizontal();

							}

							GUILayout.BeginVertical(itemBox, GUILayout.Width(width), GUILayout.Height(height));
							{

								if (GUILayout.Button(string.Empty, style) == true) {
									
									this.onSelect(item);
									
								}

								var lastRect = GUILayoutUtility.GetLastRect();
								lastRect.width = width;
								lastRect.height = height;

								{

									var editor = Editor.CreateEditor(item) as IPreviewEditor;
									if (editor.HasPreviewGUI() == true) {

										Color color = Color.white;
										editor.OnPreviewGUI(color, lastRect, style, false);

									}

								}

								this.onEveryGUI(item);

							}
							GUILayout.EndVertical();

							++i;

						}
						
						if (i % xCount != 0 && xCount > 0) {
							
							GUILayout.EndHorizontal();
							
						}

					}

				}
				GUILayout.EndVertical();

			}
			GUILayout.EndScrollView();

		}

	}

}