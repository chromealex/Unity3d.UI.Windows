#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ME {

	public partial class EditorUtilities {

		/*public static void ClearLayoutElement(UnityEngine.UI.Windows.WindowLayoutElement layoutElement) {
			
			if (layoutElement.editorLabel == null) return;

			var root = layoutElement.gameObject.transform.root.gameObject;

			if (EditorUtilities.IsPrefab(root) == false) {

				GameObject.DestroyImmediate(layoutElement.editorLabel);
				return;

			}

			var tag = layoutElement.tag;

			// Create prefab instance
			var instance = GameObject.Instantiate(root) as GameObject;
			var layout = instance.GetComponent<UnityEngine.UI.Windows.WindowLayout>();
			layout.hideFlags = HideFlags.HideAndDontSave;
			if (layout != null) {

				layoutElement = layout.GetRootByTag(tag);
				if (layoutElement != null) EditorUtilities.ClearLayoutElement(layoutElement);

				// Apply prefab
				PrefabUtility.ReplacePrefab(instance, root, ReplacePrefabOptions.ReplaceNameBased);

				// Clean up
				GameObject.DestroyImmediate(instance);

			}

		}*/
		/*
		public static void DestroyImmediateHidden<T>(this MonoBehaviour root) where T : Component {

			var components = root.GetComponents<T>();
			foreach (var component in components) {

				if (component.hideFlags == HideFlags.HideAndDontSave) {

					Component.DestroyImmediate(component);

				}

			}

		}*/

		public static bool IsPrefab(GameObject go) {

			if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab) {

				return true;

			}

			return false;

		}

		public static T CreateAsset<T>() where T : ScriptableObject {

			var asset = ScriptableObject.CreateInstance<T>();

			string path = AssetDatabase.GetAssetPath( Selection.activeObject );
			if ( path == "" ) {
				path = "Assets";
			} else if ( Path.GetExtension( path ) != "" ) {
				path = path.Replace( Path.GetFileName( AssetDatabase.GetAssetPath( Selection.activeObject ) ), "" );
			}

			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( path + "/New " + typeof( T ).ToString() + ".asset" );

			AssetDatabase.CreateAsset( asset, assetPathAndName );

			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;

			return asset;

		}

		public static T CreateAsset<T>(string filepath) where T : ScriptableObject {

			var asset = ScriptableObject.CreateInstance<T>();

			//string path = filepath;
			string assetPathAndName = filepath + ".asset";

			AssetDatabase.CreateAsset( asset, assetPathAndName );

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			
			return AssetDatabase.LoadAssetAtPath(assetPathAndName, typeof(T)) as T;
			
		}

		public static void CreateAsset( System.Type type ) {

			var asset = ScriptableObject.CreateInstance( type ) as Object;

			string path = AssetDatabase.GetAssetPath( Selection.activeObject );
			if ( path == "" ) {
				path = "Assets";
			} else if ( Path.GetExtension( path ) != "" ) {
				path = path.Replace( Path.GetFileName( AssetDatabase.GetAssetPath( Selection.activeObject ) ), "" );
			}

			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( path + "/New " + type.ToString() + ".asset" );

			AssetDatabase.CreateAsset( asset, assetPathAndName );

			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;

		}

		/// TODO: REPLACE WITH MY VERSION!!!
		public static T[] GetAssetsOfType<T>(string fileExtension = ".*", bool strongType = false) where T : Component {

			List<T> tempObjects = new List<T>();
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
			FileInfo[] goFileInfo = directory.GetFiles("*" + fileExtension, SearchOption.AllDirectories);
			
			int i = 0; int goFileInfoLength = goFileInfo.Length;
			FileInfo tempGoFileInfo; string tempFilePath;
			GameObject tempGO;
			for (; i < goFileInfoLength; i++) {

				tempGoFileInfo = goFileInfo[i];
				if (tempGoFileInfo == null)
					continue;
				
				tempFilePath = tempGoFileInfo.FullName;
				tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");

				tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(Object)) as GameObject;
				if (tempGO == null) {
					//Debug.LogWarning("Skipping Null");
					continue;
				} else if (tempGO.GetComponent<T>() == null) {
					//Debug.LogWarning("Skipping " + tempGO.GetType().ToString());
					continue;
				}

				if (strongType == true) {

					if (tempGO.GetComponent<T>().GetType().Name != typeof(T).Name) continue;

				}

				tempObjects.Add(tempGO.GetComponent<T>());

			}
			
			return tempObjects.ToArray();
		}

	}

}
#endif