#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ME {

	public partial class EditorUtilities {

		public static void Destroy(Object component, System.Action callback = null) {

			if (component != null) {

				UnityEditor.EditorApplication.CallbackFunction action = null;
				action = () => {

					EditorApplication.delayCall -= action;
					if (component != null) Object.DestroyImmediate(component, allowDestroyingAssets: true);
					if (callback != null) callback.Invoke();

				};
				EditorApplication.delayCall += action;

			}

		}

		private static bool changeCheckValue = false;

		public static void BeginChangeCheck() {

			EditorUtilities.changeCheckValue = false;

		}

		public static void SetChangeDirty() {

			EditorUtilities.changeCheckValue = true;

		}

		public static bool EndChangeCheck() {

			var oldValue = EditorUtilities.changeCheckValue;
			EditorUtilities.changeCheckValue = false;
			return oldValue;

		}

		public static bool SetValueIfDirty<T>(ref T structRef, T result) where T : struct {

			if (structRef.Equals(result) == false) {

				structRef = result;
				EditorUtilities.SetChangeDirty();
				return true;

			}

			return false;

		}

		public static bool SetObjectIfDirty(ref object structRef, object result) {

			if (structRef.Equals(result) == false) {

				structRef = result;
				EditorUtilities.SetChangeDirty();
				return true;

			}

			return false;

		}

		public static bool SetObjectIfDirty(ref Object structRef, Object result) {

			if (structRef != result) {

				structRef = result;
				EditorUtilities.SetChangeDirty();
				return true;

			}

			return false;

		}

		public static bool SetValueIfDirty(ref string structRef, string result) {

			if (structRef != result) {

				structRef = result;
				EditorUtilities.SetChangeDirty();
				return true;

			}

			return false;

		}

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

		public static T[] GetPrefabsOfType<T>(bool strongType = true, string directory = null, bool useCache = true, System.Func<T, bool> predicate = null) where T : Component {
			
			return ME.EditorUtilities.GetPrefabsOfTypeRaw<T>(strongType, directory, useCache, (p) => { if (predicate != null && p is T) { return predicate(p as T); } else { return true; } }).Cast<T>().ToArray();
			
		}

		public static Component[] GetPrefabsOfTypeRaw<T>(bool strongType, string directory = null, bool useCache = true, System.Func<T, bool> predicate = null) where T : Component {

			if (directory != null && System.IO.Directory.Exists(directory) == false) return new Component[0] {};

			System.Func<T[]> action = () => {
				
				var folder = (directory == null) ? null : new string[] { directory };
				
				var objects = AssetDatabase.FindAssets("t:GameObject", folder);
				
				var output = new List<T>();
				foreach (var obj in objects) {
					
					if (obj == null) continue;
					
					var path = AssetDatabase.GUIDToAssetPath(obj);
					if (path == null) continue;
					
					var file = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
					if (file == null) continue;

					var comps = file.GetComponents<T>();
					foreach (var comp in comps) {

						if (comp == null) continue;

						if (predicate != null && predicate(comp) == false) continue;

						if (strongType == true && comp.GetType().Name != typeof(T).Name) continue;
						
						output.Add(comp);

					}

				}
				
				return output.ToArray();

			};

			if (useCache == true) {

				return ME.Utilities.CacheComponentsArray<T>(action, strongType, directory);

			}

			return action();

		}
		
		public static T[] GetAssetsOfType<T>(string directory = null, System.Func<T, bool> predicate = null, bool useCache = true) where T : ScriptableObject {

			return ME.EditorUtilities.GetAssetsOfTypeRaw<T>(directory, (p) => { if (predicate != null && p is T) { return predicate(p as T); } else { return true; } }, useCache).Cast<T>().ToArray();

		}

		public static ScriptableObject[] GetAssetsOfTypeRaw<T>(string directory = null, System.Func<ScriptableObject, bool> predicate = null, bool useCache = true) where T : ScriptableObject {

			if (directory != null && System.IO.Directory.Exists(directory) == false) return new ScriptableObject[0] {};

			System.Func<T[]> action = () => {

				var folder = (directory == null) ? null : new string[] { directory };

				//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				var objects = AssetDatabase.FindAssets("t:ScriptableObject", folder);

				var output = new List<T>();
				foreach (var obj in objects) {
					
					if (obj == null) continue;

					var path = AssetDatabase.GUIDToAssetPath(obj);
					if (path == null) continue;
					
					var files = AssetDatabase.LoadAllAssetsAtPath(path);
					if (files == null) continue;

					foreach (var file in files) {

						var comp = file as T;
						if (comp == null) continue;

						if (predicate != null && predicate(comp) == false) continue;

						if (output.Contains(comp) == false) {

							output.Add(comp);

						}

					}

				}
				
				return output.ToArray();
				
			};

			if (useCache == true) {

				return ME.Utilities.CacheAssetsArray<T>(action, directory);

			}

			return action();

		}
		
		public static T[] GetObjectsOfType<T>(string filepath = null, string inFolder = null, System.Func<T, bool> predicate = null, bool useCache = true) where T : Object {
			
			System.Func<T[]> action = () => {
				
				var output = new List<T>();

				System.Func<string, bool> item = (string guid) => {
					
					if (guid == null) return false;
					
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (path == null) return false;
					
					var files = AssetDatabase.LoadAllAssetsAtPath(path);
					if (files == null) return false;

					var result = false;
					for (int i = 0; i < files.Length; ++i) {

						var file = files[i];

						var comp = file as T;
						if (comp == null) continue;

						if (predicate != null && predicate(comp) == false) continue;

						if (output.Contains(comp) == false) {

							output.Add(comp);
							result = true;

						}

					}

					return result;

				};

				//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				var objects = AssetDatabase.FindAssets("t:Object", null);

				if (string.IsNullOrEmpty(inFolder) == false) {

					// Find directories
					var dirs = new List<string>();
					foreach (var obj in objects) {
						
						if (obj == null) continue;
						
						var path = AssetDatabase.GUIDToAssetPath(obj);
						if (path == null) continue;

						var isDir = System.IO.Directory.Exists(path);
						if (isDir == true) {

							var locs = Path.GetDirectoryName(path).Split('/');
							if (locs.Length > 0) {
								
								var loc = locs[locs.Length - 1];
								if (loc == inFolder) {
									
									dirs.Add(path);
									
								}
								
							}

						}

					}

					foreach (var dir in dirs) {

						var objs = AssetDatabase.FindAssets("t:Object", new string[] { dir });
						foreach (var obj in objs) {

							if (obj == null) continue;

							var path = AssetDatabase.GUIDToAssetPath(obj);
							if (path == null) continue;

							if (path.Contains(filepath) == true) {

								item(obj);

							}

						}

					}

				} else {

					foreach (var obj in objects) {

						item(obj);

					}

				}

				return output.ToArray();
				
			};
			
			if (useCache == true) {
				
				return ME.Utilities.CacheObjectsArray<T>(action, filepath).Cast<T>().ToArray();
				
			}
			
			return action();
			
		}

		public static void ResetCache<T>(string directory = null) where T : ScriptableObject {

			ME.Utilities.ResetAssetsArray<T>(directory);

		}

		private static int _controlId;
		public static void BeginDraw() {

			EditorUtilities._controlId = 0;

		}

		public static void EndDraw() {

			EditorUtilities._controlId = 0;

		}

		public static void DrawArrow(Color color, Vector3 point, Vector3 target, float size) {

			var offset = 0f;

			++EditorUtilities._controlId;

			var oldColor = UnityEditor.Handles.color;
			UnityEditor.Handles.color = color;

			UnityEditor.Handles.DrawLine(point, target);
			UnityEditor.Handles.ArrowHandleCap(EditorUtilities._controlId, target - (target - point).normalized * (size + offset), Quaternion.LookRotation(target - point), size, EventType.Repaint);

			UnityEditor.Handles.color = oldColor;

		}

		public static void DrawRectangle(Vector3[] points, Color fill, Color border, Color borderLines, float borderWidthPercent = 0.01f, bool horizontalDraw = true, bool verticalDraw = true) {
			
			var margin = Vector3.Distance(points[0], points[2]) * borderWidthPercent;
			var noColor = new Color(0f, 0f, 0f, 0f);

			if (horizontalDraw == true) {

				// Left margin
				UnityEditor.Handles.DrawSolidRectangleWithOutline(new Vector3[] {
					points[0],
					points[1],
					points[1] + Vector3.right * margin,
					points[0] + Vector3.right * margin
				}, border, noColor);

				// Right margin
				UnityEditor.Handles.DrawSolidRectangleWithOutline(new Vector3[] {
					points[3] + Vector3.left * margin,
					points[2] + Vector3.left * margin,
					points[2],
					points[3]
				}, border, noColor);

			}

			if (verticalDraw == true) {

				// Top margin
				UnityEditor.Handles.DrawSolidRectangleWithOutline(new Vector3[] {
					points[1] + Vector3.down * margin + (horizontalDraw == true ? Vector3.right * margin : Vector3.zero),
					points[1] + (horizontalDraw == true ? Vector3.right * margin : Vector3.zero),
					points[2] + (horizontalDraw == true ? Vector3.left * margin : Vector3.zero),
					points[2] + Vector3.down * margin + (horizontalDraw == true ? Vector3.left * margin : Vector3.zero)
				}, border, noColor);

				// Bottom margin
				UnityEditor.Handles.DrawSolidRectangleWithOutline(new Vector3[] {
					points[0] + (horizontalDraw == true ? Vector3.right * margin : Vector3.zero),
					points[0] + Vector3.up * margin + (horizontalDraw == true ? Vector3.right * margin : Vector3.zero),
					points[3] + Vector3.up * margin + (horizontalDraw == true ? Vector3.left * margin : Vector3.zero),
					points[3] + (horizontalDraw == true ? Vector3.left * margin : Vector3.zero)
				}, border, noColor);

			}

			{

				UnityEditor.Handles.DrawSolidRectangleWithOutline(new Vector3[] {
					points[0] + (horizontalDraw == true ? Vector3.right * margin : Vector3.zero) + (verticalDraw == true ? Vector3.up * margin : Vector3.zero),
					points[1] + (horizontalDraw == true ? Vector3.right * margin : Vector3.zero) + (verticalDraw == true ? Vector3.down * margin : Vector3.zero),
					points[2] + (horizontalDraw == true ? Vector3.left * margin : Vector3.zero) + (verticalDraw == true ? Vector3.down * margin : Vector3.zero),
					points[3] + (horizontalDraw == true ? Vector3.left * margin : Vector3.zero) + (verticalDraw == true ? Vector3.up * margin : Vector3.zero)
				}, fill, borderLines);
				UnityEditor.Handles.DrawSolidRectangleWithOutline(points, noColor, borderLines);

			}

		}

	}

}
#endif