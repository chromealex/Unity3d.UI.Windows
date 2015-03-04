using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowChooserFilterWindow : EditorWindow {
		
		private EditorWindow root;
		
		private System.Action<Component> onSelect;
		private System.Action<Component> onEveryGUI;
		private List<Component> items = new List<Component>();

		private static Dictionary<string, List<Component>> cache = new Dictionary<string, List<Component>>();

		public static void Show<T>(EditorWindow root, System.Action<T> onSelect, System.Action<T> onEveryGUI = null, bool strongType = false) where T : Component {

			var editor = FlowChooserFilterWindow.CreateInstance<FlowChooserFilterWindow>();
			editor.title = "UI.Windows Flow Filter";
			editor.ShowPopup();
			editor.position = new Rect(0f, 0f, 1f, 1f);

			editor.root = root;

			editor.onSelect = (c) => {

				if (onSelect != null) onSelect(c as T);
				editor.Close();

			};

			editor.onEveryGUI = (c) => {

				if (onEveryGUI != null) onEveryGUI(c as T);

			};

			editor.Scan<T>(strongType, "CACHE" + strongType.ToString());

			editor.Focus();

		}

		public void OnLostFocus() {

			this.Close();

		}

		public void Scan<T>(bool strongType, string cacheKey) where T : Component {

			List<Component> list;
			if (FlowChooserFilterWindow.cache.TryGetValue(cacheKey, out list) == true) {

				this.items = list;
				return;

			}

			var items = ME.EditorUtilities.GetAssetsOfType<T>(".prefab", strongType);

			this.items = new List<Component>();
			foreach (var item in items) {

				this.items.Add(item);

			}

			FlowChooserFilterWindow.cache.Add(cacheKey, this.items);

		}

		public void Update() {

			var offset = FlowSceneItem.POPUP_OFFSET + 50f;
			this.position = new Rect(this.root.position.x + offset, this.root.position.y + offset, this.root.position.width - offset * 2f, this.root.position.height - offset * 2f);

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
				GUILayout.EndVertical();

			}
			GUILayout.EndScrollView();

		}

	}

}