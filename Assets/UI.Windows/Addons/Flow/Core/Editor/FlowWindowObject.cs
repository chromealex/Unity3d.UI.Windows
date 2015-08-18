using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	[InitializeOnLoad]
	public static class FlowProjectWindowObject {
		
		public static bool IsValidPackage(string path) {
			
			if (System.IO.Directory.Exists(path) == true) {
				
				var isScreens = System.IO.Directory.Exists(path + "/Screens");
				var isLayouts = System.IO.Directory.Exists(path + "/Layouts");
				var isComponents = System.IO.Directory.Exists(path + "/Components");
				
				if (isScreens && isLayouts && isComponents) {
					
					return true;
					
				}
				
			}
			
			return false;
			
		}

		public class Drawer {

			private Texture2D packageIcon;
			private Texture2D packageIconLarge;

			public void DrawFlowFolderProjectGUI(string path, Rect rect) {

				if (this.packageIcon == null) this.packageIcon = Resources.Load<Texture2D>("UI.Windows/Icons/PackageIcon");
				if (this.packageIconLarge == null) this.packageIconLarge = Resources.Load<Texture2D>("UI.Windows/Icons/PackageIconLarge");

				this.DrawIcon(rect, this.packageIcon, this.packageIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}
			
			private Texture2D transitionsIcon;
			private Texture2D transitionsIconLarge;
			
			public void DrawFlowTransitionsProjectGUI(string path, Rect rect) {
				
				if (this.transitionsIcon == null) this.transitionsIcon = Resources.Load<Texture2D>("UI.Windows/Icons/Transitions");
				if (this.transitionsIconLarge == null) this.transitionsIconLarge = Resources.Load<Texture2D>("UI.Windows/Icons/TransitionsIconLarge");
				
				this.DrawIcon(rect, this.transitionsIcon, this.transitionsIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));
				
			}

			private Texture2D componentsIcon;
			private Texture2D componentsIconLarge;

			public void DrawFlowComponentsProjectGUI(string path, Rect rect) {

				if (this.componentsIcon == null) this.componentsIcon = Resources.Load<Texture2D>("UI.Windows/Icons/Components");
				if (this.componentsIconLarge == null) this.componentsIconLarge = Resources.Load<Texture2D>("UI.Windows/Icons/ComponentsIconLarge");

				this.DrawIcon(rect, this.componentsIcon, this.componentsIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}

			private Texture2D layoutsIcon;
			private Texture2D layoutsIconLarge;

			public void DrawFlowLayoutsProjectGUI(string path, Rect rect) {

				if (this.layoutsIcon == null) this.layoutsIcon = Resources.Load<Texture2D>("UI.Windows/Icons/Layouts");
				if (this.layoutsIconLarge == null) this.layoutsIconLarge = Resources.Load<Texture2D>("UI.Windows/Icons/LayoutsIconLarge");

				this.DrawIcon(rect, this.layoutsIcon, this.layoutsIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}

			private Texture2D screensIcon;
			private Texture2D screensIconLarge;

			public void DrawFlowScreensProjectGUI(string path, Rect rect) {

				if (this.screensIcon == null) this.screensIcon = Resources.Load<Texture2D>("UI.Windows/Icons/Screens");
				if (this.screensIconLarge == null) this.screensIconLarge = Resources.Load<Texture2D>("UI.Windows/Icons/ScreensIconLarge");

				this.DrawIcon(rect, this.screensIcon, this.screensIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}

			private Texture2D baseScriptIcon;
			private Texture2D baseScriptLarge;

			private GUIStyle unitySysStyle;

			public void DrawFlowBaseScriptProjectGUI(string path, Rect rect) {

				if (this.baseScriptIcon == null) this.baseScriptIcon = Resources.Load<Texture2D>("UI.Windows/Icons/UnityDocumentIcon");
				if (this.baseScriptLarge == null) this.baseScriptLarge = Resources.Load<Texture2D>("UI.Windows/Icons/UnityDocumentIconLarge");

				if (this.unitySysStyle == null) {

					this.unitySysStyle = new GUIStyle(EditorStyles.miniLabel);
					this.unitySysStyle.alignment = TextAnchor.MiddleRight;
					this.unitySysStyle.normal.textColor = Color.gray;

				}

				var sys = new GUIContent("System");
				GUI.Label(new Rect(rect.xMin, rect.yMin, rect.width, rect.height), sys, this.unitySysStyle);
				this.DrawIcon(rect, this.baseScriptIcon, this.baseScriptLarge, new Vector2(0f, 0f), new Vector2(0f, 0f), -10f);

			}

			private MethodInfo isDirtyMethod;
			private bool IsDirty(int instanceId) {
				
				if (this.isDirtyMethod == null) {
					
					System.Type type = typeof(EditorUtility);
					this.isDirtyMethod = type.GetMethod("IsDirty", BindingFlags.Static | BindingFlags.NonPublic);
					
				}

				return (bool)this.isDirtyMethod.Invoke(this, new object[1] { instanceId });

			}

			private GUIStyle unityMarkBackStyle;

			public void OnProjectItemGUI(string guid, Rect rect) {
				
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

				if (this.unityMarkBackStyle == null) {
					
					this.unityMarkBackStyle = new GUIStyle("flow shader in 5");
					
				}

				if (obj != null) {

					var dirty = false;
					var go = obj as GameObject;
					if (go != null) {

						dirty = this.IsDirty(go.GetInstanceID());
						if (dirty == false) {

							var comps = go.GetComponents<MonoBehaviour>();
							foreach (var comp in comps) {

								dirty = this.IsDirty(comp.GetInstanceID());
								if (dirty == true) break;

							}

						}

					} else {

						dirty = this.IsDirty(obj.GetInstanceID());

					}

					if (dirty == true) {

						var r = new Rect(rect);
						r.width += r.x;
						r.x = 0f;
						
						GUI.Box(r, "test", this.unityMarkBackStyle);

					}

				}

				if (FlowProjectWindowObject.IsValidPackage(path) == true) {

					this.DrawFlowFolderProjectGUI(path, rect);
					return;

				}

				var packageDir = path;

				if (System.IO.File.Exists(packageDir) == true) {

					var splitted = path.Split('/');
					packageDir = string.Join("/", splitted, 0, splitted.Length - 1);

				}

				if (FlowProjectWindowObject.IsValidPackage(packageDir) == true || FlowProjectWindowObject.IsValidPackage(System.IO.Path.Combine(packageDir, "..")) == true) {

					var last = path.Split('/');
					if (last.Length > 0) {
						
						var folder = last[last.Length - 1];
						if (folder == "Screens") {

							this.DrawFlowScreensProjectGUI(path, rect);

						} else if (folder == "Transitions") {
							
							this.DrawFlowTransitionsProjectGUI(path, rect);
							
						} else if (folder == "Components") {
							
							this.DrawFlowComponentsProjectGUI(path, rect);
							
						} else if (folder == "Layouts") {

							this.DrawFlowLayoutsProjectGUI(path, rect);

						} else if (path.EndsWith("Base.cs") == true) {

							this.DrawFlowBaseScriptProjectGUI(path, rect);

						}

					}

				}

			}

			private void DrawIcon(Rect rect, Texture2D small, Texture2D large, Vector2 smallOffset, Vector2 largeOffset, float heightOffset = 0f) {

				if (rect.height > 16f) {

					if (large != null) {

						rect.height += heightOffset;

						var offset = rect.height * largeOffset.x;

						rect.x -= offset;
						rect.width = rect.height;
						rect.width += largeOffset.y;
						GUI.DrawTexture(rect, large);

					}

				} else {

					if (small != null) {
						
						rect.x -= smallOffset.x;
						rect.width = rect.height;
						rect.width += smallOffset.y;
						GUI.DrawTexture(rect, small);

					}

				}

			}

		}

		public static Drawer drawer;

		static FlowProjectWindowObject() {

			FlowProjectWindowObject.drawer = new Drawer();
			EditorApplication.projectWindowItemOnGUI += FlowProjectWindowObject.drawer.OnProjectItemGUI;

		}

	}

	[CanEditMultipleObjects]
	[CustomEditor(typeof(UnityEngine.Object), true)]
	public class FlowWindowObject : Editor {

		/*public override void OnInspectorGUI() {

			//UnityEditor.ProjectWindowCallback.EndNameEditAction.
			//var pBrowser = EditorWindow.GetWindow<ProjectBrowser>();
			var _target = this.target;

			Debug.Log(_target);

			base.OnInspectorGUI();

		}*/

		private Texture2D packageIcon;

		public virtual void OnEnable() {
			
		}

		public virtual void OnDisable() {
			
		}

		public override void OnInspectorGUI() {

			if (Selection.activeObject == null) return;

			var path = AssetDatabase.GetAssetPath(Selection.activeObject);

			if (AssetDatabase.IsValidFolder(path) == true) {

				var isScreens = System.IO.Directory.Exists(path + "/Screens");
				var isLayouts = System.IO.Directory.Exists(path + "/Layouts");
				var isComponents = System.IO.Directory.Exists(path + "/Components");

				if (isScreens && isLayouts && isComponents) {

					this.DrawFlowFolderInspector(path);

				}

			} else {

				path = path.Substring(path.LastIndexOf("/") + 1);
				var ext = path.Substring(path.LastIndexOf(".") + 1);

				switch (ext) {
					
					default:
						this.DrawDefaultInspector();
						break;

				}

			}

		}

		public override void DrawPreview(Rect previewArea) {

			//base.DrawPreview(previewArea);

		}

		public void DrawFlowFolderInspector(string path) {

			var oldState = GUI.enabled;
			GUI.enabled = true;

			GUILayout.Label("Flow Window Object");

			GUI.enabled = oldState;

		}

	}

}
