using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

namespace ME.UAB {
	
	public static class Builder {
		
		#if UNITY_EDITOR
		public static UABConfig GetDefaultConfig(bool required) {

			var guids = UnityEditor.AssetDatabase.FindAssets("t:UABConfig");
			foreach (var guid in guids) {

				var config = UnityEditor.AssetDatabase.LoadAssetAtPath<UABConfig>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
				if (config != null) {

					return config;

				}

			}

			if (required == true) {

				throw new UnityException("UABConfig was not found in the project. Create one.");

			}

			return null;

		}

		public static void RebuildAll(string path, UABConfig config = null, int version = 0) {

			if (config == null) config = Builder.GetDefaultConfig(required: true);

			// Clean up all resources
			var resourcesPath = string.Format("Assets/{0}", config.UAB_RESOURCES_PATH);
			if (Directory.Exists(resourcesPath) == false) {

				throw new UnityException("Resources path `" + resourcesPath + "` was not found in the project.");

			}

			var files = System.IO.Directory.GetFiles(resourcesPath);
			foreach (var file in files) {

				if (file.Contains("/.") == true) continue;

				UnityEditor.AssetDatabase.DeleteAsset(file);

			}

			// Build
			Builder.BuildAll(path, config, version);

		}

		public static void BuildAll(string path, UABConfig config = null, int version = 0) {

			if (config == null) config = Builder.GetDefaultConfig(required: true);

			if (System.IO.Directory.Exists(path) == false) {

				System.IO.Directory.CreateDirectory(path);

			}

			var buildingAssets = new Dictionary<string, List<GameObject>>();
			var bundles = UnityEditor.AssetDatabase.GetAllAssetBundleNames();
			for (int i = 0; i < bundles.Length; ++i) {

				buildingAssets.Clear();
				var bundle = bundles[i];
				var assetsPath = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
				for (int j = 0; j < assetsPath.Length; ++j) {

					var assetPath = assetsPath[j];
					var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
					if (asset != null && ME.UAB.Extensions.EditorUtilities.IsPrefab(asset) == true) {

						List<GameObject> list;
						if (buildingAssets.TryGetValue(bundle, out list) == true) {

							buildingAssets[bundle].Add(asset);

						} else {

							buildingAssets.Add(bundle, new List<GameObject>() { asset });

						}

					}

				}

				var assetBuildPath = string.Format("{0}/{1}.{2}", path, bundle, config.UAB_EXT);
				foreach (var asset in buildingAssets) {

					var assets = asset.Value.ToArray();

					var dir = System.IO.Path.GetDirectoryName(assetBuildPath);
					if (System.IO.Directory.Exists(dir) == false) {

						System.IO.Directory.CreateDirectory(dir);

					}

					var zipped = Builder.PackToBytes(assets, config);
					System.IO.File.WriteAllBytes(assetBuildPath, zipped);
					Debug.Log("Built to `" + assetBuildPath + "`, size: " + zipped.Length + " bytes.");

					if (version > 0) {

						var name = System.IO.Path.GetFileName(assetBuildPath);
						var builtinPath = string.Format(config.UAB_CACHE_PATH, Application.streamingAssetsPath, version, name);
						var builtinDir = System.IO.Path.GetDirectoryName(builtinPath);
						if (System.IO.Directory.Exists(builtinDir) == false) {

							System.IO.Directory.CreateDirectory(builtinDir);

						}

						System.IO.File.WriteAllBytes(builtinPath, zipped);

					}

				}

			}

		}
		#endif

		public static List<ISerializer> GetAllSerializers(UABConfig config = null) {

			#if UNITY_EDITOR
			if (config == null) config = Builder.GetDefaultConfig(required: true);
			#endif

			var output = new List<ISerializer>();

			var @namespace = config.UAB_SERIALIZERS_NAMESPACE;
			var query = from t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
					where t.IsClass == true && t.IsAbstract == false && t.IsNested == false && t.Namespace == @namespace
				select t.Name.ToLower();

			foreach (var element in query) {

				var instance = System.Activator.CreateInstance(null,
					               string.Format ("{0}.{1}", @namespace, element.ToLower()),
					               true,
					               System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.CreateInstance,
					               null, null, null, null, null);

				if (instance != null) {

					var module = instance.Unwrap() as ISerializer;
					if (module != null) {

						output.Add(module);

					}

				}

			}

			return output;

		}

		#region Unpack
		public static GameObject[] Unpack(byte[] bytes, List<ISerializer> serializers = null) {
			
			var unzipped = Zipper.UnzipString(bytes);
			var dataDeserialized = UABSerializer.DeserializeValueType<UABPackage>(unzipped);
			return Builder.Unpack(dataDeserialized, serializers);

		}

		public static GameObject[] Unpack(UABPackage package, List<ISerializer> serializers = null) {

			return new UABUnpacker().Run(package, serializers);

		}
		#endregion

		#region Pack
		#if UNITY_EDITOR
		public static byte[] PackToBytes(GameObject[] objects, UABConfig config = null, List<ISerializer> serializers = null) {

			var package = Builder.Pack(objects, config, serializers);
			var dataSerialized = UABSerializer.SerializeValueType(package);
			var zipped = Zipper.ZipString(dataSerialized);
			return zipped;

		}

		public static UABPackage Pack(GameObject[] objects, UABConfig config = null, List<ISerializer> serializers = null) {

			return new UABPacker().Run(objects, config, serializers);

		}
		#endif
		#endregion

		public static string GetTypeString(System.Type type) {

			return string.Format("{0}, {1}", type.FullName, type.Assembly.FullName);

		}

		public static Transform GetRoot(object obj) {

			if (obj is Component) {

				var comp = obj as Component;
				if (comp != null) return comp.transform.root;

			}

			if (obj is GameObject) {

				var comp = obj as GameObject;
				if (comp != null) return comp.transform.root;

			}

			return null;

		}

		public static int GetInstanceID(object obj) {

			if (obj is Object) {

				var _obj = obj as Object;
				if (_obj != null) return _obj.GetInstanceID();
				
			}

			return 0;

		}

		public static bool IsBinary(string bundleName, Object obj) {

			var dataInBundle = false;

			#if UNITY_EDITOR
			var assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
			var importer = UnityEditor.AssetImporter.GetAtPath(assetPath);
			if (importer != null && string.IsNullOrEmpty(importer.assetBundleName) == false) {

				//Debug.LogWarning(assetPath + " :: " + importer.assetBundleName + " == " + this.currentBundleName);
				dataInBundle = (importer.assetBundleName == bundleName);

			}
			//Debug.Log(assetPath + " :: " + (obj is Mesh) + " :: " + (obj is Texture) + " :: " + obj + " :: " + dataInBundle);
			#endif

			return
				dataInBundle == true &&
				(obj is Mesh || obj is Texture || obj is Sprite);

		}

	}

}