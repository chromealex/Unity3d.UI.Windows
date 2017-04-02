using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	[InitializeOnLoad]
	public static class FlowProjectWindowObject {

		public class Drawer {

			public class Item {

				public int instanceId;
				public string path;

				public bool isValidPackage;
				public bool withSystemLabel;
				public bool isDirty;
				
				public bool isPackage;
				public bool isPackageContainer;
				public bool isBaseClass;
				public bool isComponentsFolder;
				public bool isLayoutsFolder;
				public bool isScreensFolder;
				public bool isTransitionsFolder;

			}

			public Dictionary<string, Item> cache = new Dictionary<string, Item>();

			public void DrawWithCache(string guid, Rect rect) {

				Item item;
				if (this.cache.TryGetValue(guid, out item) == false) {

					item = new Item();
					
					item.path = AssetDatabase.GUIDToAssetPath(guid);
					var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.path);

					if (obj != null) {

						var dirty = false;
						var go = obj as GameObject;
						if (go != null) {
							
							dirty = this.IsDirty(go.GetInstanceID());
							if (dirty == false) {
								
								var comps = go.GetComponents<MonoBehaviour>();
								foreach (var comp in comps) {

									if (comp == null) continue;

									dirty = this.IsDirty(comp.GetInstanceID());
									if (dirty == true) break;
									
								}
								
							}
							
						} else {
							
							dirty = this.IsDirty(obj.GetInstanceID());
							
						}
						
						item.instanceId = obj.GetInstanceID();
						item.isDirty = dirty;

					}
					
					if (FlowEditorUtilities.IsValidPackage(item.path) == true) {
						
						item.isValidPackage = true;
						item.isPackage = true;

					}

					if (item.isPackage == false && FlowEditorUtilities.IsValidPackageContainer(item.path) == true) {

						item.isPackageContainer = true;

					}

					var packageDir = item.path;
					
					if (System.IO.File.Exists(packageDir) == true) {
						
						var splitted = item.path.Split('/');
						packageDir = string.Join("/", splitted, 0, splitted.Length - 1);
						
					}

					if (FlowEditorUtilities.IsValidPackage(packageDir) == true ||
						FlowEditorUtilities.IsValidPackage(System.IO.Path.Combine(packageDir, "..")) == true ||
						FlowEditorUtilities.IsValidPackageContainer(packageDir) == true ||
						FlowEditorUtilities.IsValidPackageContainer(System.IO.Path.Combine(packageDir, "..")) == true
					) {
						
						var last = item.path.Split('/');
						if (last.Length > 0) {

							item.isValidPackage = true;

							var folder = last[last.Length - 1];
							if (folder == "Screens") {
								
								item.isScreensFolder = true;
								
							} else if (folder == "Transitions") {
								
								item.isTransitionsFolder = true;
								
							} else if (folder == "Components") {
								
								item.isComponentsFolder = true;
								
							} else if (folder == "Layouts") {
								
								item.isLayoutsFolder = true;
								
							} else if (item.path.EndsWith("Base.cs") == true) {
								
								item.withSystemLabel = true;
								item.isBaseClass = true;
								
							}

						}

					}

					this.cache.Add(guid, item);

				}

				this.OnGUIItem(item.path, rect, item);

			}

			private Texture2D packageContainerIcon;
			private Texture2D packageContainerIconLarge;

			public void DrawFlowFolderContainerProjectGUI(string path, Rect rect) {

				if (this.packageContainerIcon == null) this.packageContainerIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/PackageContainerIcon");
				if (this.packageContainerIconLarge == null) this.packageContainerIconLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/PackageContainerIconLarge");

				this.DrawIcon(rect, this.packageContainerIcon, this.packageContainerIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}

			private Texture2D packageIcon;
			private Texture2D packageIconLarge;

			public void DrawFlowFolderProjectGUI(string path, Rect rect) {

				if (this.packageIcon == null) this.packageIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/PackageIcon");
				if (this.packageIconLarge == null) this.packageIconLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/PackageIconLarge");

				this.DrawIcon(rect, this.packageIcon, this.packageIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}
			
			private Texture2D transitionsIcon;
			private Texture2D transitionsIconLarge;
			
			public void DrawFlowTransitionsProjectGUI(string path, Rect rect) {
				
				if (this.transitionsIcon == null) this.transitionsIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/Transitions");
				if (this.transitionsIconLarge == null) this.transitionsIconLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/TransitionsIconLarge");
				
				this.DrawIcon(rect, this.transitionsIcon, this.transitionsIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));
				
			}

			private Texture2D componentsIcon;
			private Texture2D componentsIconLarge;

			public void DrawFlowComponentsProjectGUI(string path, Rect rect) {

				if (this.componentsIcon == null) this.componentsIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/Components");
				if (this.componentsIconLarge == null) this.componentsIconLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/ComponentsIconLarge");

				this.DrawIcon(rect, this.componentsIcon, this.componentsIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}

			private Texture2D layoutsIcon;
			private Texture2D layoutsIconLarge;

			public void DrawFlowLayoutsProjectGUI(string path, Rect rect) {

				if (this.layoutsIcon == null) this.layoutsIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/Layouts");
				if (this.layoutsIconLarge == null) this.layoutsIconLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/LayoutsIconLarge");

				this.DrawIcon(rect, this.layoutsIcon, this.layoutsIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}

			private Texture2D screensIcon;
			private Texture2D screensIconLarge;

			public void DrawFlowScreensProjectGUI(string path, Rect rect) {

				if (this.screensIcon == null) this.screensIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/Screens");
				if (this.screensIconLarge == null) this.screensIconLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/ScreensIconLarge");

				this.DrawIcon(rect, this.screensIcon, this.screensIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));

			}

			private Texture2D baseScriptIcon;
			private Texture2D baseScriptLarge;

			private GUIStyle unitySysStyle;

			public void DrawFlowBaseScriptProjectGUI(string path, Rect rect) {

				if (this.baseScriptIcon == null) this.baseScriptIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/UnityDocumentIcon");
				if (this.baseScriptLarge == null) this.baseScriptLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/UnityDocumentIconLarge");

				if (this.unitySysStyle == null) {

					this.unitySysStyle = new GUIStyle(EditorStyles.miniLabel);
					this.unitySysStyle.alignment = TextAnchor.MiddleRight;
					this.unitySysStyle.normal.textColor = Color.gray;

				}

				var sys = new GUIContent("System");
				GUI.Label(new Rect(rect.xMin, rect.yMin, rect.width, rect.height), sys, this.unitySysStyle);
				this.DrawIcon(rect, this.baseScriptIcon, this.baseScriptLarge, new Vector2(0f, 0f), new Vector2(0f, 0f), -10f);

			}

			public void OnGUIItem(string path, Rect rect, Item item) {

				if (item.isDirty == true) {
					
					var r = new Rect(rect);
					r.width += r.x;
					r.x = 0f;
					
					GUI.Box(r, string.Empty, this.unityMarkBackStyle);
					
				}

				if (item.isPackageContainer == true) {

					this.DrawFlowFolderContainerProjectGUI(path, rect);
					return;

				}

				if (item.isPackage == true) {

					this.DrawFlowFolderProjectGUI(path, rect);
					return;

				}

				if (item.isScreensFolder == true) {
					
					this.DrawFlowScreensProjectGUI(path, rect);
					
				} else if (item.isTransitionsFolder == true) {
					
					this.DrawFlowTransitionsProjectGUI(path, rect);
					
				} else if (item.isComponentsFolder == true) {
					
					this.DrawFlowComponentsProjectGUI(path, rect);
					
				} else if (item.isLayoutsFolder == true) {
					
					this.DrawFlowLayoutsProjectGUI(path, rect);
					
				} else if (item.isBaseClass == true) {
					
					this.DrawFlowBaseScriptProjectGUI(path, rect);
					
				}

			}

			private MethodInfo isDirtyMethod;
			private bool IsDirty(int instanceId) {
				
				if (this.isDirtyMethod == null) {
					
					System.Type type = typeof(EditorUtility);
					this.isDirtyMethod = type.GetMethod("IsDirty", BindingFlags.Static | BindingFlags.NonPublic);
					
				}

				if (this.isDirtyMethod == null) return false;

				return (bool)this.isDirtyMethod.Invoke(this, new object[1] { instanceId });

			}

			private GUIStyle unityMarkBackStyle;

			public void OnProjectItemGUI(string guid, Rect rect) {
				
				if (this.unityMarkBackStyle == null) {
					
					this.unityMarkBackStyle = new GUIStyle("flow shader in 5");
					
				}

				this.DrawWithCache(guid, rect);

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
		
		private Texture2D packageIcon;

		public virtual void OnEnable() {
			
		}

		public virtual void OnDisable() {
			
		}

		public override void OnInspectorGUI() {

			if (Selection.activeObject == null) return;

			var path = AssetDatabase.GetAssetPath(Selection.activeObject);

			if (AssetDatabase.IsValidFolder(path) == true) {

				if (FlowEditorUtilities.IsValidPackage(path) == true) {

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

			GUILayout.Label("Flow Package");

			GUI.enabled = oldState;

		}

	}

}