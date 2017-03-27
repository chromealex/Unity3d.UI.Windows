#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ME.UAB.Extensions {

	public class EditorUtilities {
		
		public static bool IsPrefab(GameObject go) {

			if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab) {

				return true;

			}

			return false;

		}

		public static int GetLocalIdentfierInFile(Object comp) {
			
			var inspectorModeInfo = typeof(UnityEditor.SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
			UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(comp);
			inspectorModeInfo.SetValue(serializedObject, UnityEditor.InspectorMode.Debug, null);
			UnityEditor.SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");

			return localIdProp.intValue;

		}

		public static T CreatePrefab<T>() where T : MonoBehaviour {

			var go = new GameObject();
			var asset = go.AddComponent<T>();

			string path = AssetDatabase.GetAssetPath( Selection.activeObject );
			if ( path == "" ) {
				path = "Assets";
			} else if ( Path.GetExtension( path ) != "" ) {
				path = path.Replace( Path.GetFileName( AssetDatabase.GetAssetPath( Selection.activeObject ) ), "" );
			}
			
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( path + "/New " + typeof( T ).ToString() + ".prefab" );

			PrefabUtility.CreatePrefab(assetPathAndName, go, ReplacePrefabOptions.ConnectToPrefab);

			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
			//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;

			GameObject.DestroyImmediate(go);

			return asset;
			
		}

		public static T CreateAsset<T>(string name = null, Object pathWithObject = null) where T : ScriptableObject {

			var asset = ScriptableObject.CreateInstance<T>();

			var selectedObject = pathWithObject ?? Selection.activeObject;

			string path = AssetDatabase.GetAssetPath( selectedObject );
			if ( path == "" ) {
				path = "Assets";
			} else if ( Path.GetExtension( path ) != "" ) {
				path = path.Replace( Path.GetFileName( AssetDatabase.GetAssetPath( selectedObject ) ), "" );
			}

			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}", path, (name != null ? name : "New " + typeof(T).ToString() + ".asset")));

			AssetDatabase.CreateAsset(asset, assetPathAndName);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
			//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			//AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			//Selection.activeObject = asset;

			return asset;

		}

		public static T CreateAsset<T>(string name, string filepath, bool reimport = true) where T : ScriptableObject {

			var asset = ScriptableObject.CreateInstance<T>();

			if (string.IsNullOrEmpty(filepath) == true) {

				filepath = AssetDatabase.GetAssetPath(Selection.activeObject);
				if (filepath == "") {

					filepath = "Assets";

				} else if (Path.GetExtension(filepath) != "") {

					filepath = filepath.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

				}

			}

			string path = filepath;
			if ( path == "" ) {
				path = "Assets";
			} else if ( Path.GetExtension( path ) != "" ) {
				path = path.Replace( Path.GetFileName( filepath ), "" );
			}

			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}", path, (name != null ? name : "New " + typeof(T).ToString() + ".asset")));

			AssetDatabase.CreateAsset(asset, assetPathAndName);
			if (reimport == true) AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
			//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			//AssetDatabase.SaveAssets();
			//EditorUtility.FocusProjectWindow();
			//Selection.activeObject = asset;

			return asset;

		}

		public static T CreateAssetWithModules<T>(string name = null, string filepath = null) where T : ScriptableObject {

			var path = filepath;
			if (string.IsNullOrEmpty(filepath) == true) {

				path = AssetDatabase.GetAssetPath(Selection.activeObject);
				if (path == "") {
					
					path = "Assets";

				} else if (Path.GetExtension(path) != "") {
					
					path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

				}

			}

			var dirName = (name != null ? name : "New " + typeof(T));
			path = path.Trim('/');
			var guid = AssetDatabase.CreateFolder(path, dirName);
			path = AssetDatabase.GUIDToAssetPath(guid);
			//path = path + "/" + dirName;

			var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", path, (name != null ? name : "New " + typeof(T).ToString())));

			var asset = ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(asset, assetPathAndName);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
			//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			if (string.IsNullOrEmpty(filepath) == true) {
				
				EditorUtility.FocusProjectWindow();
				Selection.activeObject = asset;

			}

			return asset;

		}

		public static T CreateAssetFromFilepath<T>(string filepath) where T : ScriptableObject {

			var asset = ScriptableObject.CreateInstance<T>();

			string assetPathAndName = filepath + ".asset";

			AssetDatabase.CreateAsset(asset, assetPathAndName);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
			//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			return AssetDatabase.LoadAssetAtPath(assetPathAndName, typeof(T)) as T;
			
		}
		
		public static Object CreateAsset( System.Type type ) {
			
			var asset = ScriptableObject.CreateInstance( type ) as Object;
			
			string path = AssetDatabase.GetAssetPath( Selection.activeObject );
			if ( path == "" ) {
				path = "Assets";
			} else if ( Path.GetExtension( path ) != "" ) {
				path = path.Replace( Path.GetFileName( AssetDatabase.GetAssetPath( Selection.activeObject ) ), "" );
			}
			
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( path + "/New " + type.ToString() + ".asset" );

			AssetDatabase.CreateAsset(asset, assetPathAndName);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
			//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
			
			return asset;
			
		}
		
		public static Object CreateAsset(System.Type type, string path, string filename) {
			
			var asset = ScriptableObject.CreateInstance(type) as Object;

			if ( path == "" ) {
				path = "Assets";
			} else if ( Path.GetExtension( path ) != "" ) {
				path = path.Replace( Path.GetFileName( AssetDatabase.GetAssetPath( Selection.activeObject ) ), "" );
			}
			
			string assetPathAndName = path + "/" + filename + ".asset";

			AssetDatabase.CreateAsset(asset, assetPathAndName);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
			//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			return asset;
			
		}

	}

}
#endif