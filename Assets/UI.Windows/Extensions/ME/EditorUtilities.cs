#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ME {

	public static class EditorUtilities {

		public static void ClearLayoutElement(UnityEngine.UI.Windows.WindowLayoutElement layoutElement) {
			
			if (layoutElement.GetComponent<UnityEngine.UI.Image>() == null) return;

			var root = layoutElement.gameObject.transform.root.gameObject;

			if (EditorUtilities.IsPrefab(root) == false) {

				Component.DestroyImmediate(layoutElement.GetComponent<UnityEngine.UI.Image>());
				Component.DestroyImmediate(layoutElement.GetComponent<CanvasRenderer>());
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

		}

		public static bool IsPrefab(GameObject go) {

			if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab) {

				return true;

			}

			return false;

		}

		public static T[] GetPrefabsOfType<T>(string fileExtension = ".prefab", bool silent = false) where T : Component {
			
			if (silent == false) UnityEditor.EditorUtility.DisplayProgressBar("Searching for files", "*" + fileExtension, 0f);
			
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
				
				//Debug.Log(tempFilePath + "\n" + Application.dataPath);
				
				tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(GameObject)) as GameObject;
				if (tempGO == null) continue;
				var tempComp = tempGO.GetComponent<T>();
				if (tempComp == null) continue;

				tempObjects.Add(tempComp);
				
				if (silent == false) UnityEditor.EditorUtility.DisplayProgressBar("Searching for files", "*" + fileExtension, i / (float)goFileInfoLength);
				
			}
			
			if (silent == false) UnityEditor.EditorUtility.ClearProgressBar();
			
			return tempObjects.ToArray();
			
		}

		public static T[] GetAssetsOfType<T>(string fileExtension = ".asset", bool silent = false) where T : ScriptableObject {

			if (silent == false) UnityEditor.EditorUtility.DisplayProgressBar("Searching for files", "*" + fileExtension, 0f);

			List<T> tempObjects = new List<T>();
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
			FileInfo[] goFileInfo = directory.GetFiles("*" + fileExtension, SearchOption.AllDirectories);
			
			int i = 0; int goFileInfoLength = goFileInfo.Length;
			FileInfo tempGoFileInfo; string tempFilePath;
			T tempGO;
			for (; i < goFileInfoLength; i++) {

				tempGoFileInfo = goFileInfo[i];
				if (tempGoFileInfo == null)
					continue;
				
				tempFilePath = tempGoFileInfo.FullName;
				tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");
				
				//Debug.Log(tempFilePath + "\n" + Application.dataPath);
				
				tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(T)) as T;
				if (tempGO == null) continue;

				tempObjects.Add(tempGO);
				
				if (silent == false) UnityEditor.EditorUtility.DisplayProgressBar("Searching for files", "*" + fileExtension, i / (float)goFileInfoLength);

			}
			
			if (silent == false) UnityEditor.EditorUtility.ClearProgressBar();

			return tempObjects.ToArray();

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

	}

}
#endif