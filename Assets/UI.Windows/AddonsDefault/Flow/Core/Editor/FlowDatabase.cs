using UnityEngine;
using System.Collections;
using ME;

using System.Reflection;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI.Windows;
using ADB = UnityEditor.AssetDatabase;
#endif
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEngine.UI.Windows.Plugins.Flow {

	public class FlowDatabase {
		
		public const string COMPONENTS_FOLDER = "Components";
		public const string LAYOUT_FOLDER = "Layouts";
		public const string SCREENS_FOLDER = "Screens";
		public const string OTHER_NAME = "Other";

		public static void RemoveLayoutComponent(Transform element) {

			var root = element.root;

			GameObject.DestroyImmediate(element.gameObject);

			FlowDatabase.SaveLayout(root.GetComponent<WindowLayout>());

		}

		public static WindowLayoutElement AddLayoutElementComponent(string name, Transform root, int siblingIndex) {
			
			var go = new GameObject(name);
			go.transform.SetParent(root);
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			
			go.AddComponent<RectTransform>();
			
			go.transform.SetSiblingIndex(siblingIndex);
			
			var layoutElement = go.AddComponent<WindowLayoutElement>();
			layoutElement.comment = "NEW LAYOUT ELEMENT";
			
			/*var canvas = go.AddComponent<CanvasGroup>();
			canvas.alpha = 1f;
			canvas.blocksRaycasts = true;
			canvas.interactable = true;
			canvas.ignoreParentGroups = false;*/
			//layoutElement.canvas = canvas;
			
			//FlowSceneView.GetItem().SetLayoutDirty();
			FlowDatabase.SaveLayout(layoutElement.transform.root.GetComponent<WindowLayout>());

			return layoutElement;

		}
		
		public static TWith ReplaceComponents<TReplace, TWith>(TReplace source, System.Type withType) where TReplace : Component where TWith : Component {

			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var cachedFields = source.GetType().GetFields(flags);

			var fieldValues = new Dictionary<string, object>();
			foreach (var cache in cachedFields) {
				/*
				var cache = cachedFields.FirstOrDefault((f) => f.Name == field.Name);
				if (cache != null) {*/

				fieldValues.Add(cache.Name, cache.GetValue(source));
				//field.SetValue(newInstance, cache.GetValue(source));

				//}

			}

			var go = source.gameObject;

			// Destroy the old one
			Object.DestroyImmediate(source, true);
			
			// Add a new component
			var newInstance = go.AddComponent(withType);
			var newFields = newInstance.GetType().GetFields(flags).Concat(newInstance.GetType().BaseType.GetFields(flags));

			foreach (var field in newFields) {

				object value;
				if (fieldValues.TryGetValue(field.Name, out value) == true) {

					field.SetValue(newInstance, value);

				}

			}

			return newInstance as TWith;

		}
		
		#if UNITY_EDITOR
		private static T LoadPrefabTemplate<T>(string directory, string templateName) where T : Component {
			
			var go = UnityEngine.Resources.Load("UI.Windows/Templates/" + directory + "/" + templateName) as GameObject;
			if (go == null) return null;

			return go.GetComponent<T>();
			
		}
		
		public static WindowLayout LoadLayout(WindowLayout prefab) {
			
			return UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as WindowLayout;
			
		}
		
		public static void SaveLayout(WindowLayout instance) {

			if (instance == null) return;

			UnityEditor.PrefabUtility.ReplacePrefab(instance.gameObject, UnityEditor.PrefabUtility.GetPrefabParent(instance.gameObject), UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
			ADB.Refresh();

		}
		
		public static WindowLayout GenerateLayout(FD.FlowWindow window, FlowWindowLayoutTemplate layout) {
			
			WindowLayout instance = null;
			
			if (window.compiled == false) return instance;
			
			var tplName = layout.name;//"3Buttons";
			var tplData = layout;//FlowSystem.LoadPrefabTemplate<WindowLayout>(FlowSystem.LAYOUT_FOLDER, tplName);
			if (tplData != null) {
				
				var filepath = window.compiledDirectory + "/" + FlowDatabase.LAYOUT_FOLDER + "/" + tplName + "Layout.prefab";

				instance = FlowDatabase.GenerateLayout(filepath, tplData);

			} else {
				
				Debug.LogError("Template Loading Error: " + tplName);
				
			}
			
			return instance;
			
		}

		public static WindowLayout GenerateLayout(string filepath, FlowWindowLayoutTemplate layout) {

			filepath = filepath.Replace("//", "/");

			WindowLayout instance = null;

			var sourcepath = ADB.GetAssetPath(layout);

			filepath = AssetDatabase.GenerateUniqueAssetPath(filepath);

			System.IO.File.Copy(sourcepath, filepath, true);
			ADB.Refresh();

			var source = ADB.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject;
			var prefab = source.GetComponent<WindowLayout>();
			instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as WindowLayout;
			
			instance = FlowDatabase.ReplaceComponents<FlowWindowLayoutTemplate, WindowLayout>(instance as FlowWindowLayoutTemplate, typeof(WindowLayout));
			
			FlowDatabase.SaveLayout(instance);
			
			GameObject.DestroyImmediate(instance.gameObject);
			instance = (ADB.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject).GetComponent<WindowLayout>();

			return instance;

		}
		
		public static WindowBase LoadScreen(WindowBase prefab) {
			
			return UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as WindowBase;
			
		}
		
		public static void SaveScreen(WindowBase instance) {
			
			UnityEditor.PrefabUtility.ReplacePrefab(instance.gameObject, UnityEditor.PrefabUtility.GetPrefabParent(instance.gameObject), UnityEditor.ReplacePrefabOptions.ReplaceNameBased);
			ADB.Refresh();

		}
		
		public static WindowBase GenerateScreen(FD.FlowWindow window, FlowLayoutWindowTypeTemplate template) {
			
			WindowBase instance = null;
			
			if (window.compiled == false) return instance;
			
			var tplName = template.name;//"Layout";
			var tplData = template;//FlowSystem.LoadPrefabTemplate<WindowBase>(FlowSystem.SCREENS_FOLDER, tplName);
			if (tplData != null) {
				
				var filepath = window.compiledDirectory + "/" + FlowDatabase.SCREENS_FOLDER + "/" + tplName + "Screen.prefab";

				instance = FlowDatabase.GenerateScreen(filepath, window.compiledDerivedClassName, string.Empty, tplData);

			} else {
				
				Debug.LogError("Template Loading Error: " + tplName);
				
			}
			
			return instance;
			
		}
		
		public static WindowBase GenerateScreen(string filepath, string className, string namespaceName, FlowLayoutWindowTypeTemplate template) {

			WindowBase instance = null;

			filepath = filepath.Replace("//", "/");
			
			var sourcepath = ADB.GetAssetPath(template);

			filepath = AssetDatabase.GenerateUniqueAssetPath(filepath);

			System.IO.File.Copy(sourcepath, filepath, true);
			ADB.Refresh();
			
			var source = ADB.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject;
			var prefab = source.GetComponent<WindowBase>();
			instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as WindowBase;
			
			var type = CoreUtilities.GetTypeFromAllAssemblies(className, namespaceName);
			instance = FlowDatabase.ReplaceComponents<FlowLayoutWindowTypeTemplate, WindowBase>(instance as FlowLayoutWindowTypeTemplate, type);
			
			FlowDatabase.SaveScreen(instance);
			
			GameObject.DestroyImmediate(instance.gameObject);
			instance = (ADB.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject).GetComponent<WindowBase>();

			return instance;

		}
		#endif

	}

}