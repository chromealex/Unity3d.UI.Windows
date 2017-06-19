using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowDropDownFilterWindow : EditorWindow {

		public static void Show<T>(Rect buttonRect, System.Action<T> onSelect, System.Action<T> onEveryGUI = null, bool strongType = true) where T : Component {
			
			var size = new Vector2(buttonRect.width, 250f);

			FlowDropDownFilterWindow.Show(buttonRect, size, onSelect, onEveryGUI, strongType);
			
		}

		public static void Show<T>(Rect buttonRect, Vector2 windowSize, System.Action<T> onSelect, System.Action<T> onEveryGUI = null, bool strongType = true) where T : Component {

			var editor = FlowDropDownFilterWindow.CreateInstance<FlowDropDownFilterWindow>();
			var title = "UI.Windows: Flow Filter";
			#if !UNITY_4
			editor.titleContent = new GUIContent(title);
			#else
			editor.title = title;
			#endif
			editor.ShowAsDropDown(new Rect(buttonRect.x, buttonRect.y, buttonRect.width, 1), windowSize);

			editor.onSelect = (c) => {

				if (onSelect != null) onSelect(c as T);
				editor.Close();

			};

			editor.onEveryGUI = (c) => {

				if (onEveryGUI != null) onEveryGUI(c as T);

			};

			editor.Scan<T>(strongType);

		}
		
		private System.Action<Component> onSelect;
		private System.Action<Component> onEveryGUI;
		private List<Component> items = new List<Component>();

		public void Scan<T>(bool strongType) where T : Component {

			this.items = ME.EditorUtilities.GetPrefabsOfTypeRaw<T>(strongType).ToList();

		}

		public void OnFocusLost() {

			this.Close();

		}

		private Vector2 scrollPos;
		private void OnGUI() {

			if (this.items.Count == 0) return;

			var scrollWidth = 30f;

			var origBox = new GUIStyle(GUI.skin.box);
			origBox.fixedWidth = this.position.width - scrollWidth;

			var box = new GUIStyle(GUI.skin.box);
			box.margin = new RectOffset();
			var style = new GUIStyle(EditorStyles.toolbarButton);
			style.fixedHeight = 30f;

			var rect = this.position;
			rect.x = 0f;
			rect.y = 0f;
			GUI.Box(rect, string.Empty, box);

			this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, false, true);
			{

				GUILayout.BeginVertical(box);
				{

					foreach (var item in this.items) {

						if (item == null) continue;

						GUILayout.BeginVertical(origBox);
						{

							if (GUILayout.Button(item.name, style) == true) {

								this.onSelect(item);

							}

							this.onEveryGUI(item);

						}
						GUILayout.EndVertical();

					}

				}
				GUILayout.EndVertical();

			}
			GUILayout.EndScrollView();

		}

	}

}