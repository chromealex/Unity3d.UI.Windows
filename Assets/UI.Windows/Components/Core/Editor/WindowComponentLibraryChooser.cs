using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows {
	
	public class WindowComponentLibraryChooser : EditorWindow {

		private System.Action<WindowComponentLibraryLinker.Item> onSelect;

		private List<WindowComponentLibraryLinker> libraries = new List<WindowComponentLibraryLinker>();
		
		private static WindowComponentLibraryChooser GetInstance() {
			
			var editor = WindowComponentLibraryChooser.CreateInstance<WindowComponentLibraryChooser>();
			editor.title = "UI.Windows Components Filter";
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
			
			//var scrollWidth = 30f;
			
			var width = 160f;
			var height = 160f;
			
			var itemBox = new GUIStyle(GUI.skin.box);
			itemBox.fixedWidth = 0f;
			itemBox.stretchWidth = false;
			
			/*var box = new GUIStyle(GUI.skin.box);
			box.fixedWidth = 0f;
			box.stretchWidth = true;
			box.margin = new RectOffset();*/
			
			var label = new GUIStyle(GUI.skin.label);
			label.wordWrap = true;

			var path = new GUIStyle(EditorStyles.miniLabel);
			path.wordWrap = true;

			var style = new GUIStyle(EditorStyles.toolbarButton);
			
			var rect = this.position;
			rect.x = 0f;
			rect.y = 0f;
			GUI.Box(rect, string.Empty);
			
			this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, false, false);
			{

				GUILayout.BeginHorizontal();
				{

					GUILayout.BeginVertical(GUILayout.Width(180f));
					{

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
							
							var xCount = Mathf.FloorToInt((this.position.width - 180f - width * 0.5f) / width);
							if (xCount > 0) {
								
								style.fixedWidth = width;
								style.fixedHeight = height;
								style.stretchWidth = false;
								style.stretchHeight = false;
								
								this.DrawLibrary(this.selectedLibrary, xCount, width, height, itemBox, style, label, path);

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
					
					{

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
