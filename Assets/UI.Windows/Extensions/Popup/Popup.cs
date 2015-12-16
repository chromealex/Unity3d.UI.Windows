#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;

namespace UnityEditor.UI.Windows.Extensions {

	public class Popup {
		/// <summary> Окно, которое связано с попапом </summary>
		internal PopupWindowAnim window;
		/// <summary> Прямоугольник, в котором будет отображен попап </summary>
		public Rect screenRect { get { return window.screenRect; } set { window.screenRect = value; } }
		
		/// <summary> Указывает, что является разделителем в пути </summary>
		public char separator { get { return window.separator; } set { window.separator = value; } }
		
		/// <summary> Позволяет использовать/убирать поиск </summary>
		public bool useSearch { get { return window.useSearch; } set { window.useSearch = value; } }

		/// <summary> Название рута </summary>
		public string title { get { return window.title; } set { window.title = value; } }

		/// <summary> Название рута </summary>
		public string searchText { get { return window.searchText; } set { window.searchText = value; } }

		/// <summary> Автоматически установить размер по высоте, узнав максимальное количество видимых элементов </summary>
		public bool autoHeight { get { return window.autoHeight; } set { window.autoHeight = value; } }
		
		/// <summary> Создание окна </summary>
		public Popup(Rect screenRect, bool useSearch = true, string title = "Menu", char separator = '/') {
			window = PopupWindowAnim.Create(screenRect, useSearch);
			this.title = title;
			this.separator = separator;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(Vector2 size, bool useSearch = true, string title = "Menu", char separator = '/') {
			window = PopupWindowAnim.CreateBySize(size, useSearch);
			this.title = title;
			this.separator = separator;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(float width, bool useSearch = true, string title = "Menu", char separator = '/', bool autoHeight = true) {
			window = PopupWindowAnim.Create(width, useSearch);
			this.title = title;
			this.separator = separator;
			this.autoHeight = autoHeight;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(bool useSearch = true, string title = "Menu", char separator = '/', bool autoHeight = true) {
			window = PopupWindowAnim.Create(useSearch);
			this.title = title;
			this.separator = separator;
			this.autoHeight = autoHeight;
		}
		
		public void BeginFolder(string folderName) {
			window.BeginRoot(folderName);
		}
		
		public void EndFolder() {
			window.EndRoot();
		}
		
		public void EndFolderAll() {
			window.EndRootAll();
		}
		
		public void Item(string name) {
			window.Item(name);
		}
		
		public void Item(string name, Action action) {
			window.Item(name, action);
		}
		
		public void Item(string name, Texture2D image, Action action) {
			window.Item(name, image, action);
		}
		
		public void ItemByPath(string path) {
			window.ItemByPath(path);
		}
		
		public void ItemByPath(string path, Action action) {
			window.ItemByPath(path, action);
		}
		
		public void ItemByPath(string path, Texture2D image, Action action) {
			window.ItemByPath(path, image, action);
		}
		
		public void Show() {
			window.Show();
		}
		
		public static void DrawInt(GUIContent label, string selected, System.Action<int> onResult, GUIContent[] options, int[] keys) {
			
			DrawInt_INTERNAL(new Rect(), selected, label, onResult, options, keys, true);
			
		}

		public static void DrawInt(Rect rect, string selected, GUIContent label, System.Action<int> onResult, GUIContent[] options, int[] keys) {

			DrawInt_INTERNAL(rect, selected, label, onResult, options, keys, false);

		}

		private static void DrawInt_INTERNAL(Rect rect, string selected, GUIContent label, System.Action<int> onResult, GUIContent[] options, int[] keys, bool layout) {

			var state = false;
			if (layout == true) {

				GUILayout.BeginHorizontal();
				if (label != null) GUILayout.Label(label);
				if (GUILayout.Button(selected, EditorStyles.popup) == true) {
					
					state = true;
					
				}
				GUILayout.EndHorizontal();

			} else {
				
				if (label != null) rect = EditorGUI.PrefixLabel(rect, label);
				if (GUI.Button(rect, selected, EditorStyles.popup) == true) {
					
					state = true;
					
				}
				
			}
			
			if (state == true) {

				Popup popup = null;
				if (layout == true) {

					Debug.Log(rect);
					popup = new Popup() { title = (label == null ? string.Empty : label.text), screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
					
				} else {
					
					Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
					rect.x = vector.x;
					rect.y = vector.y;
					
					popup = new Popup() { title = (label == null ? string.Empty : label.text), screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
					
				}
				
				for (int i = 0; i < options.Length; ++i) {
					
					var option = options[i];
					var result = keys[i];
					popup.ItemByPath(option.text, () => {
						
						onResult(result);
						
					});
					
				}
				
				popup.Show();

			}

		}

	}

}
#endif