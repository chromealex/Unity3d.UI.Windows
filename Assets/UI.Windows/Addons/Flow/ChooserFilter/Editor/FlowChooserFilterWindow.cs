using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Linq;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowChooserFilterWindow : EditorWindow {
		
		private EditorWindow root;
		private bool isLocked;
		private bool updateRedraw;

		private System.Action<Component> onSelect;
		private System.Action<Component> onEveryGUI;
		private List<Component> items = new List<Component>();
		
		private System.Action<ScriptableObject> onSelectAsset;
		private System.Func<ScriptableObject, bool> onEveryGUIAsset;
		private List<ScriptableObject> itemsAsset = new List<ScriptableObject>();

		private static FlowChooserFilterWindow GetInstance(EditorWindow root, bool updateRedraw = false) {
			
			var editor = FlowChooserFilterWindow.CreateInstance<FlowChooserFilterWindow>();
			var title = "UI.Windows Flow Filter";
			#if !UNITY_4
			editor.titleContent = new GUIContent(title);
			#else
			editor.title = title;
			#endif
			editor.position = new Rect(0f, 0f, 1f, 1f);

			editor.updateRedraw = updateRedraw;

			editor.isLocked = false;
			if (editor.isLocked == true) {
				
				editor.ShowPopup();
				
			} else {
				
				editor.ShowUtility();
				
			}
			
			editor.root = root;

			editor.UpdateSize();
			//editor.Focus();

			return editor;

		}

		public void OnDisable() {

			if (this.editors != null) {

				for (int i = 0; i < this.editors.Length; ++i) {

					if (this.editors[i] != null) this.editors[i].OnDisable();

				}

			}

		}

		public static void Show<T>(EditorWindow root, System.Action<T> onSelect, System.Action<T> onEveryGUI = null, System.Func<T, bool> predicate = null, bool strongType = false, string directory = null, bool useCache = true, bool drawNoneOption = false, bool updateRedraw = false) where T : Component {

			var editor = FlowChooserFilterWindow.GetInstance(root, updateRedraw);
			
			editor.onSelect = (c) => {
				
				if (onSelect != null) onSelect(c as T);
				editor.Close();
				
			};
			
			editor.onEveryGUI = (c) => {
				
				if (onEveryGUI != null) onEveryGUI(c as T);
				
			};

			editor.Scan<T>(strongType, predicate, directory, useCache, drawNoneOption);

		}
		
		public static void ShowAssets<T>(EditorWindow root, System.Action<T> onSelect, System.Func<T, bool> onEveryGUI = null, System.Func<T, bool> predicate = null, string directory = null) where T : ScriptableObject {
			
			var editor = FlowChooserFilterWindow.GetInstance(root);
			
			editor.onSelectAsset = (c) => {
				
				if (onSelect != null) onSelect(c as T);
				editor.Close();
				
			};
			
			editor.onEveryGUIAsset = (c) => {
				
				if (onEveryGUI != null) return onEveryGUI(c as T);

				return false;
				
			};

			editor.ScanAssets<T>(directory, predicate);

		}

		public void OnLostFocus() {

			if (this.root != null) this.Close();

		}
		
		public void Scan<T>(bool strongType, System.Func<T, bool> predicate = null, string directory = null, bool useCache = true, bool drawNoneOption = false) where T : Component {
			
			var items = ME.EditorUtilities.GetPrefabsOfTypeRaw<T>(strongType, directory, useCache, predicate: predicate).ToList();

			if (drawNoneOption == true) {

				items.Insert(0, null);

			}

			this.items = items;

		}
		
		public void ScanAssets<T>(string directory = null, System.Func<T, bool> predicate = null) where T : ScriptableObject {
			
			this.itemsAsset = ME.EditorUtilities.GetAssetsOfTypeRaw<T>(directory, (p) => { if (predicate != null && p is T) { return predicate(p as T); } else { return true; } }).ToList();
			
		}

		public void UpdateSize() {

			if (this.root != null) {

				var offset = FlowSceneItem.POPUP_OFFSET + 50f;
				this.position = new Rect(this.root.position.x + offset, this.root.position.y + offset, this.root.position.width - offset * 2f, this.root.position.height - offset * 2f);

			} else {

				var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
				var w = 800f;
				var h = 600f;

				this.position = new Rect(center.x - w * 0.5f, center.y - h * 0.5f, w, h);

			}

		}

		public void Update() {
			
			if (this.updateRedraw == true) {
				
				this.Repaint();
				
			}

			if (this.isLocked == true) {

				this.UpdateSize();

			}

			if (this.root != null && FlowSceneView.recompileChecker == null) this.Close();

		}

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

			var style = new GUIStyle(EditorStyles.toolbarButton);

			var rect = this.position;
			rect.x = 0f;
			rect.y = 0f;
			GUI.Box(rect, string.Empty);

			this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, false, false);
			{

				GUILayout.BeginVertical();
				{

					var xCount = Mathf.FloorToInt((this.position.width - width * 0.5f) / width);
					if (xCount > 0) {

						style.fixedWidth = width;
						style.fixedHeight = height;
						style.stretchWidth = false;
						style.stretchHeight = false;

						if (this.onEveryGUI != null) {
							
							this.DrawComponents(xCount, width, height, itemBox, style);

						} else {
							
							this.DrawAssets(xCount, width, height, itemBox, style);

						}

					}

				}
				GUILayout.EndVertical();

			}
			GUILayout.EndScrollView();

		}
		
		private void DrawAssets(int xCount, float width, float height, GUIStyle itemBox, GUIStyle style) {
			
			var i = 0;
			foreach (var item in this.itemsAsset) {
				
				if (item == null) continue;
				
				if (i % xCount == 0) {
					
					if (i != 0) GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					
				}
				
				GUILayout.BeginVertical(itemBox, GUILayout.Width(width), GUILayout.Height(height));
				{
					
					if (this.onEveryGUIAsset(item) == true) {
						
						this.onSelectAsset(item);
						
					}
					
					var lastRect = GUILayoutUtility.GetLastRect();
					lastRect.width = width;
					lastRect.height = height;

				}
				GUILayout.EndVertical();
				
				++i;
				
			}
			
			if (i % xCount != 0 && xCount > 0) {
				
				GUILayout.EndHorizontal();
				
			}
			
		}

		private IPreviewEditor[] editors;
		private void DrawComponents(int xCount, float width, float height, GUIStyle itemBox, GUIStyle style) {

			if (this.editors == null) this.editors = new IPreviewEditor[0];
			if (this.items.Count != this.editors.Length) this.editors = new IPreviewEditor[this.items.Count];

			var i = 0;
			foreach (var item in this.items) {
				
				//if (item == null) continue;

				if (i % xCount == 0) {
					
					if (i != 0) GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					
				}
				
				GUILayout.BeginVertical(itemBox, GUILayout.Width(width), GUILayout.Height(height));
				{
					
					if (GUILayout.Button(string.Empty, style) == true) {
						
						this.onSelect(item);
						
					}

					if (item != null) {

						var lastRect = GUILayoutUtility.GetLastRect();
						lastRect.width = width;
						lastRect.height = height;
						
						{
							
							var hovered = lastRect.Contains(Event.current.mousePosition);
							var editor = (this.editors[i] != null) ? this.editors[i] : this.editors[i] = Editor.CreateEditor(item) as IPreviewEditor;
							if (editor != null && editor.HasPreviewGUI() == true) {
								
								Color color = Color.white;
								editor.OnPreviewGUI(color, lastRect, style, false, false, hovered);
								
							} else {

								EditorGUI.HelpBox(lastRect, "No Editor was found. Override OnPreviewGUI method to use rect.", MessageType.Warning);

							}

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

}