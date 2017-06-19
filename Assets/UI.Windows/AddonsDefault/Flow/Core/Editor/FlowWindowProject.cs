using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	[InitializeOnLoad]
	public static class FlowWindowProject {

		public class Drawer {

			public class Item {

				public int instanceId;
				public string path;
				
				public bool isProject;
				public bool isWindow;

			}

			public Dictionary<string, Item> cache = new Dictionary<string, Item>();

			public void DrawWithCache(string guid, Rect rect) {

				Item item;
				if (this.cache.TryGetValue(guid, out item) == false) {

					item = new Item();

					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (System.IO.Path.GetExtension(path) != ".unity") {

						var objs = AssetDatabase.LoadAllAssetsAtPath(path);

						item.path = path;

						foreach (var obj in objs) {

							if (obj != null) {

								//Debug.Log(obj + " :: " + guid + " :: " + obj.GetInstanceID(), obj);
								item.instanceId = obj.GetInstanceID();
								item.isWindow = (obj is FD.FlowWindow);
								item.isProject = (obj is FlowData);
								if (item.isProject == true) {

									break;

								}
								
							}

						}

					}

					this.cache.Add(guid, item);

				}

				this.OnGUIItem(item.path, rect, item);

			}
			
			private Texture2D projectIcon;
			private Texture2D projectIconLarge;
			public void DrawFlowProjectGUI(string path, Rect rect) {
				
				if (this.projectIcon == null) this.projectIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/ProjectIcon");
				if (this.projectIconLarge == null) this.projectIconLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/ProjectIconLarge");
				
				this.DrawIcon(rect, this.projectIcon, this.projectIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));
				
			}
			
			private Texture2D windowIcon;
			private Texture2D windowIconLarge;
			public void DrawFlowWindowGUI(string path, Rect rect) {
				
				if (this.windowIcon == null) this.windowIcon = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/WindowIcon");
				if (this.windowIconLarge == null) this.windowIconLarge = UnityEngine.Resources.Load<Texture2D>("UI.Windows/Icons/WindowIconLarge");
				
				this.DrawIcon(rect, this.windowIcon, this.windowIconLarge, new Vector2(0f, 0f), new Vector2(0.1f, 3f));
				
			}

			public void OnGUIItem(string path, Rect rect, Item item) {
				
				if (item.isProject == true) {
					
					this.DrawFlowProjectGUI(path, rect);
					
				} /*else if (item.isWindow == true) {
					
					this.DrawFlowWindowGUI(path, rect);
					
				}*/

			}

			public void OnProjectItemGUI(string guid, Rect rect) {

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

		static FlowWindowProject() {

			FlowWindowProject.drawer = new Drawer();
			EditorApplication.projectWindowItemOnGUI += FlowWindowProject.drawer.OnProjectItemGUI;

		}

	}

}