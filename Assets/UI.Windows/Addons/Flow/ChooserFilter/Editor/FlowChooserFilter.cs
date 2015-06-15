using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowChooserFilter {

		public static bool IsDirectoryOrLayoutValidate(Object activeObject) {
			
			var obj = activeObject;
			if (obj != null) {
				
				var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
				if (path.Length > 0 && Directory.Exists(path) == true) {
					
					var splitted = path.Split('/');
					if (splitted[splitted.Length - 1] == "Layouts") {
						
						return true;
						
					}
					
				} else if (Selection.activeGameObject != null) {
					
					var screen = Selection.activeGameObject.GetComponent<WindowBase>();
					if (screen != null) {
						
						var splitted = path.Split('/');
						path = string.Join("/", splitted, 0, splitted.Length - 1);
						
						var dir = path + "/../Layouts";
						if (Directory.Exists(dir) == true) {
							
							return true;
							
						}
						
					}
					
				}
				
			}
			
			return false;
			
		}

		public static void CreateLayout(Object activeObject, GameObject activeGameObject, System.Action callback = null) {
			
			LayoutWindowType layoutScreen = null;
			
			var name = string.Empty;
			
			var obj = activeObject;
			var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
			if (path.Length > 0 && Directory.Exists(path) == true) {
				
				
				
			} else if (activeGameObject != null) {
				
				var screen = activeGameObject.GetComponent<WindowBase>();
				if (screen != null) {
					
					var splitted = path.Split('/');
					path = string.Join("/", splitted, 0, splitted.Length - 2).Trim('/');
					
					path = path + "/Layouts";
					if (Directory.Exists(path) == true) {
						
						layoutScreen = screen as LayoutWindowType;
						name = layoutScreen.name.Replace("Screen", "Layout");
						
					}
					
				}
				
			}
			
			FlowChooserFilterWindow.Show<FlowWindowLayoutTemplate>(null, (element) => {
				
				// on select
				
				if (string.IsNullOrEmpty(name) == true) name = element.name + "Layout";
				
				// Create an instance
				var layoutPrefab = FlowDatabase.GenerateLayout(path + "/" + name + ".prefab", element);
				
				if (layoutScreen != null) {
					
					layoutScreen.layout.layout = layoutPrefab;
					layoutScreen.OnValidate();
					EditorUtility.SetDirty(layoutScreen);
					
				}
				
				Selection.activeObject = layoutPrefab;
				
				if (callback != null) callback();

			}, (element) => {
				
				// on gui
				
				var style = new GUIStyle(GUI.skin.label);
				style.wordWrap = true;
				
				GUILayout.Label(element.comment, style);
				
			}, strongType: true);

		}
		
		public static bool IsScreenValidate(Object activeObject) {
			
			var obj = activeObject;
			if (obj != null) {
				
				var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
				if (path.Length > 0 && Directory.Exists(path) == true) {
					
					var splitted = path.Split('/');
					if (splitted[splitted.Length - 1] == "Screens") {
						
						var _path = string.Join("/", splitted, 0, splitted.Length - 1).Trim('/');
						
						var className = string.Empty;
						var files = AssetDatabase.FindAssets("t:MonoScript", new string[] { _path });
						
						foreach (var file in files) {
							
							var data = AssetDatabase.GUIDToAssetPath(file);
							var sp = data.Split('/');
							var last = sp[sp.Length - 1];
							if (last.EndsWith("Base.cs") == true) className = last.Replace("Base.cs", string.Empty);
							
						}
						
						if (string.IsNullOrEmpty(className) == false) {
							
							return true;
							
						}
						
					}
					
				}
				
			}
			
			return false;
			
		}

		public static void CreateScreen(Object activeObject, string localPath = "", System.Action callback = null) {
			
			var obj = activeObject;
			var path = AssetDatabase.GetAssetPath(obj.GetInstanceID()) + localPath;
			
			FlowChooserFilterWindow.Show<FlowLayoutWindowTypeTemplate>(null, (element) => {
				
				// on select
				var splitted = path.Split('/');
				var _path = string.Join("/", splitted, 0, splitted.Length - 1).Trim('/');
				
				var className = string.Empty;
				var files = AssetDatabase.FindAssets("t:MonoScript", new string[] { _path });
				
				foreach (var file in files) {
					
					var data = AssetDatabase.GUIDToAssetPath(file);
					var sp = data.Split('/');
					var last = sp[sp.Length - 1];
					if (last.EndsWith("Base.cs") == true) className = last.Replace("Base.cs", string.Empty);
					
				}
				
				if (string.IsNullOrEmpty(className) == false) {
					
					var name = className;
					// Create an instance
					var layoutPrefab = FlowDatabase.GenerateScreen(path + "/" + name + ".prefab", className, element);

					Selection.activeObject = layoutPrefab;
					
				}
				
				if (callback != null) callback();

			}, (element) => {
				
				// on gui
				
				var style = new GUIStyle(GUI.skin.label);
				style.wordWrap = true;
				
				GUILayout.Label(element.comment, style);
				
			}, strongType: true);
			
		}

	}

}