using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows.Animations;
using System.Linq;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;

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
					
					layoutScreen.GetCurrentLayout().layout = layoutPrefab;
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

		public static void CreateScreen(Object activeObject, string namespaceName, string localPath = "", System.Action callback = null) {
			
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
					var layoutPrefab = FlowDatabase.GenerateScreen(path + "/" + name + ".prefab", className, namespaceName, element);

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

		public static void CreateTransition<T>(FD.FlowWindow windowWithScreen, FD.FlowWindow flowWindow, FD.FlowWindow toWindow, int attachIndex, string localPath, string namePrefix, System.Action<T> callback = null) where T : TransitionInputTemplateParameters {

			if (windowWithScreen.GetScreen() == null) return;

			var screenPath = AssetDatabase.GetAssetPath(windowWithScreen.GetScreen().Load<WindowBase>());
			screenPath = System.IO.Path.GetDirectoryName(screenPath);
			var splitted = screenPath.Split(new string[] {"/"}, System.StringSplitOptions.RemoveEmptyEntries);
			var packagePath = string.Join("/", splitted, 0, splitted.Length - 1);
			var path = packagePath + localPath;

			FlowChooserFilterWindow.Show<T>(
				root: null,
			    onSelect: (element) => {
				
				// Clean up previous transitions if exists
				var attachItem = flowWindow.GetAttachItem(toWindow.id, (x) => x.index == attachIndex);
				if (attachItem != null) {

					if (FlowSystem.GetData().modeLayer == ModeLayer.Flow) {
						
						if (attachItem.transitionParameters != null) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(attachItem.transitionParameters.gameObject));
						
						attachItem.transition = null;
						attachItem.transitionParameters = null;

					} else if (FlowSystem.GetData().modeLayer == ModeLayer.Audio) {

						if (attachItem.audioTransitionParameters != null) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(attachItem.audioTransitionParameters.gameObject));
						
						attachItem.audioTransition = null;
						attachItem.audioTransitionParameters = null;

					}

				}

				if (System.IO.Directory.Exists(path) == false) {

					System.IO.Directory.CreateDirectory(path);

				}

				if (element == null) return;

				var prefix = string.Empty;
				if (FlowSystem.GetData().modeLayer == ModeLayer.Flow) {
					
					prefix = "Transition-" + namePrefix;

				} else if (FlowSystem.GetData().modeLayer == ModeLayer.Audio) {
					
					prefix = "AudioTransition-" + namePrefix;

				}

				var elementPath = AssetDatabase.GetAssetPath(element.gameObject);
				var targetName = prefix + "-" + element.gameObject.name + "-" + (toWindow.IsFunction() == true ? FlowSystem.GetWindow(toWindow.functionId).directory : toWindow.directory);
				var targetPath = path + "/" + targetName + ".prefab";

				if (AssetDatabase.CopyAsset(elementPath, targetPath) == true) {

					AssetDatabase.ImportAsset(targetPath);

					var newInstance = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
					var instance = newInstance.GetComponent<T>();
					instance.useDefault = false;
					instance.useAsTemplate = false;
					EditorUtility.SetDirty(instance);

					if (FlowSystem.GetData().modeLayer == ModeLayer.Flow) {
						
						attachItem.transition = instance.transition;
						attachItem.transitionParameters = instance;

					} else if (FlowSystem.GetData().modeLayer == ModeLayer.Audio) {
						
						attachItem.audioTransition = instance.transition;
						attachItem.audioTransitionParameters = instance;

					}

					if (callback != null) callback(instance);

				}

			},
			onEveryGUI: (element) => {
				
				// on gui
				
				var style = new GUIStyle(GUI.skin.label);
				style.wordWrap = true;
				
				if (element != null) {

					GUILayout.Label(element.name, style);

				} else {

					GUILayout.Label("None", style);

				}

			},
			predicate: (element) => {

				var elementPath = AssetDatabase.GetAssetPath(element.gameObject);
				var isInPackage = FlowEditorUtilities.IsValidPackage(elementPath + "/../");

				if (element.transition != null && (isInPackage == true || element.useAsTemplate == true)) {

					var name = element.GetType().FullName;
					var baseName = name.Substring(0, name.IndexOf("Parameters"));

					var type = System.Type.GetType(baseName + ", " + element.GetType().Assembly.FullName, throwOnError: true, ignoreCase: true);
					if (type != null) {

						var attribute = type.GetCustomAttributes(inherit: true).OfType<TransitionCameraAttribute>().FirstOrDefault();
						if (attribute != null) {

							return true;

						} else {

							Debug.Log("No Attribute: " + baseName, element);

						}

					} else {

						Debug.Log("No type: " + baseName);

					}

				}

				return false;

			},
			strongType: false,
			directory: null,
			useCache: false,
			drawNoneOption: true,
			updateRedraw: true);
			
		}

	}

}