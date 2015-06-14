using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows {
	
	public class WindowComponentLibraryChooser : EditorWindow {

		private class Styles {

			public GUISkin skin;
			public GUIStyle box1;
			public GUIStyle box2;
			public GUIStyle itemButton;

			public Styles() {

				this.skin = Resources.Load<GUISkin>("UI.Windows/Core/Styles/" + (EditorGUIUtility.isProSkin == true ? "SkinDark" : "SkinLight"));
				this.box1 = this.skin.FindStyle("StyledBox1");
				this.box2 = this.skin.FindStyle("StyledBox2");
				this.itemButton = new GUIStyle(this.skin.FindStyle("LayoutBox"));

			}

		}

		private Styles styles;

		private System.Action<WindowComponentLibraryLinker.Item> onSelect;

		private List<WindowComponentLibraryLinker> libraries = new List<WindowComponentLibraryLinker>();
		
		private static WindowComponentLibraryChooser GetInstance() {
			
			var editor = WindowComponentLibraryChooser.CreateInstance<WindowComponentLibraryChooser>();
			var title = "UI.Windows Components Filter";
			#if !UNITY_4
			editor.titleContent = new GUIContent(title);
			#else
			editor.title = title;
			#endif
			editor.position = new Rect(0f, 0f, 1f, 1f);

			editor.ShowUtility();

			editor.UpdateSize();
			
			return editor;
			
		}
		
		public static void Show(System.Action<WindowComponentLibraryLinker.Item> onSelect) {
			
			var editor = WindowComponentLibraryChooser.GetInstance();
			
			editor.onSelect = (c) => {
				
				if (onSelect != null) onSelect(c);
				editor.Close();
				
			};

			editor.Scan();
			
		}

		public void Scan() {
			
			this.libraries = ME.EditorUtilities.GetAssetsOfType<WindowComponentLibraryLinker>(null, (p) => {

				return !p.child;
			
			}).ToList();
			
		}
		
		public void UpdateSize() {

			var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
			var w = 800f;
			var h = 600f;
			
			this.position = new Rect(center.x - w * 0.5f, center.y - h * 0.5f, w, h);

		}

		//private string[] currentDirectory = new string[0];
		private WindowComponentLibraryLinker selectedLibrary;
		private Vector2 scrollPos;
		private void OnGUI() {

			if (this.styles == null) this.styles = new Styles();

			GUI.skin = this.styles.skin;

			//var scrollWidth = 30f;
			
			var width = 160f;
			var height = 100f;

			var panelWidth = 180f;
			
			var itemBox = new GUIStyle(this.styles.box2);
			itemBox.fixedWidth = 0f;
			itemBox.stretchWidth = false;

			var label = new GUIStyle(GUI.skin.label);
			label.wordWrap = true;

			var path = new GUIStyle(EditorStyles.miniLabel);
			path.wordWrap = true;

			var style = this.styles.itemButton;
			
			var rect = this.position;
			rect.x = 0f;
			rect.y = 0f;
			GUI.Box(rect, string.Empty, this.styles.box1);
			
			this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, false, false);
			{

				GUILayout.BeginHorizontal();
				{

					GUILayout.BeginVertical(this.styles.box2, GUILayout.Width(panelWidth));
					{

						GUILayout.Label("Libraries:");

						foreach (var library in this.libraries) {

							if (GUILayout.Button(library.name) == true) {

								this.selectedLibrary = library;

							}

						}
						
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					{

						if (this.selectedLibrary != null) {

							var sizeWidth = this.position.width - panelWidth - width * 0.5f;
							var xCount = Mathf.FloorToInt(sizeWidth / width);
							var widthOffset = (sizeWidth - xCount * width) / xCount;
							if (xCount > 0) {
								
								style.fixedWidth = width + widthOffset;
								style.fixedHeight = height + widthOffset;
								style.stretchWidth = false;
								style.stretchHeight = false;
								
								this.DrawLibrary(this.selectedLibrary, xCount, width + widthOffset, height + widthOffset, itemBox, style, label, path);

							}

						}

					}
					GUILayout.EndVertical();

				}
				GUILayout.EndHorizontal();

			}
			GUILayout.EndScrollView();
			
		}

		private void DrawLibrary(WindowComponentLibraryLinker lib, int xCount, float width, float height, GUIStyle itemBox, GUIStyle style, GUIStyle label, GUIStyle path) {
			
			const float offset = 5f;

			var drawing = false;

			var i = 0;
			foreach (var item in lib.items) {

				if (i % xCount == 0) {
					
					if (i != 0) GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					
				}

				GUILayout.BeginVertical(itemBox, GUILayout.Width(width), GUILayout.Height(height));
				{

					if (GUILayout.Button(item.title, style) == true) {

						if (this.onSelect != null) this.onSelect(item);

					}

					var lastRect = GUILayoutUtility.GetLastRect();
					lastRect.width = width;
					lastRect.height = height;

					lastRect.x += offset;
					lastRect.y += offset;
					lastRect.width -= offset * 2f;
					lastRect.height -= offset * 2f;

					drawing = new Rect(this.scrollPos.x, this.scrollPos.y, Screen.width, Screen.height).Overlaps(lastRect);
					if (drawing == true) {

						item.OnPreviewGUI(Color.white, lastRect, style);

					}

					GUILayout.BeginVertical();
					{

						GUILayout.Label(item.title, label);
						GUILayout.Label(item.localDirectory, path);

					}
					GUILayout.EndVertical();

				}
				GUILayout.EndVertical();
				
				++i;

			}
			
			if (i % xCount != 0 && xCount > 0) {
				
				GUILayout.EndHorizontal();
				
			}

		}

		/*
		private List<string> dirs = new List<string>();
		private List<string[]> dirsSplitted = new List<string[]>();
		private void DrawLevel(string rootDir, int level) {

			var lib = this.selectedLibrary;
			this.dirs.Clear();
			this.dirsSplitted.Clear();

			foreach (var item in lib.items) {

				if (item.localDirectory.Contains(rootDir) == false &&
				    rootDir.Contains(item.localDirectory) == false) continue;

				var splitted = item.localDirectory.Split('/');
				var dir = splitted[level];

				if (this.dirs.Contains(dir) == false) {
					
					this.dirs.Add(dir);
					this.dirsSplitted.Add(splitted);

				}

			}

			GUILayout.BeginVertical(GUILayout.Width(180f));
			{

				foreach (var splitted in this.dirsSplitted) {

					var dir = splitted[level];
					if (GUILayout.Button(dir) == true) {

						this.currentDirectory = string.Join("/", splitted, 0, level + 1).Split('/');
						Debug.Log(string.Join("/", this.currentDirectory));

					}

				}

			}
			GUILayout.EndVertical();

		}
		*/

	}

}
