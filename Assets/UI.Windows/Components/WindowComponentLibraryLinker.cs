using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#endif

namespace UnityEngine.UI.Windows {

	public class WindowComponentLibraryLinker : ScriptableObject {

		[System.Serializable]
		public class Item {

			public string title;
			public string localDirectory;
			public WindowComponent mainComponent;
			public WindowComponent[] components;
			
			#if UNITY_EDITOR
			public Item(WindowComponent component) {

				this.mainComponent = component;
				this.components = new WindowComponent[1] { component };

			}

			public void CopyTo(string path) {

				if (this.components.Length > 1) {
					
					var from = AssetDatabase.GetAssetPath(this.mainComponent);
					var splitted = from.Split('/');
					from = string.Join("/", splitted, 0, splitted.Length - 1).Trim('/');
					var to = path + "/" + splitted[splitted.Length - 2];

					to = AssetDatabase.GenerateUniqueAssetPath(to);

					AssetDatabase.CopyAsset(from, to);
					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				} else {

					var from = AssetDatabase.GetAssetPath(this.mainComponent);
					var to = path + "/" + this.mainComponent.name + ".asset";
					
					to = AssetDatabase.GenerateUniqueAssetPath(to);

					AssetDatabase.CopyAsset(from, to);
					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				}

			}

			public void Fill(string mainFolder) {

				var component = this.mainComponent;

				var path = AssetDatabase.GetAssetPath(component);
				var splitted = path.Split('/');
				var folder = string.Join("/", splitted, 0, splitted.Length - 1);

				this.title = component.name;
				this.localDirectory = folder.Replace(mainFolder, string.Empty).Trim('/');

			}

			private IPreviewEditor editor;
			public void OnPreviewGUI(Color color, Rect rect, GUIStyle background) {
				
				if (this.editor == null) this.editor = Editor.CreateEditor(this.mainComponent) as IPreviewEditor;
				if (this.editor != null && this.editor.HasPreviewGUI() == true) {

					this.editor.OnPreviewGUI(color, rect, background, false, false);
					
				}

			}
			#endif

		}

		public bool child;
		public Item[] items;
		
		#if UNITY_EDITOR
		[ContextMenu("Setup")]
		public void Setup() {

			var items = new List<Item>();

			var path = AssetDatabase.GetAssetPath(this);
			var splitted = path.Split('/');
			var folder = string.Join("/", splitted, 0, splitted.Length - 1);

			var linkers = ME.EditorUtilities.GetAssetsOfType<WindowComponentLibraryLinker>(directory: folder, useCache: false);
			var prefabs = ME.EditorUtilities.GetPrefabsOfType<WindowComponent>(strongType: false, directory: folder, useCache: false).ToList();

			foreach (var linker in linkers) {

				if (linker == this) continue;

				// Remove prefabs
				foreach (var item in linker.items) {

					prefabs.RemoveAll((c) => {

						return item.components.Contains(c);

					});

				}

				items.AddRange(linker.items.ToList());

			}
			
			this.items = new Item[0];

			foreach (var prefab in prefabs) {

				items.Add(new Item(prefab));

			}

			foreach (var item in items) {

				item.Fill(folder);

			}

			this.items = items.ToArray();

		}

		[UnityEditor.MenuItem("Assets/Create/UI Windows/Components/Library Linker")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<WindowComponentLibraryLinker>();
			
		}
		#endif

	}

}