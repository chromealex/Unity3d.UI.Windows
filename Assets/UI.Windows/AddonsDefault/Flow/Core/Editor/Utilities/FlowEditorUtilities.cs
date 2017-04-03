using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowEditorUtilities {

		public static bool IsValidPackageContainer(string path) {

			try {

				var files = System.IO.Directory.GetFiles(path);
				foreach (var file in files) {

					var filename = Path.GetFileName(file);
					if (filename.EndsWith("Base.cs") == true) {

						var original = filename.Replace("Base.cs", ".cs");
						var exists = System.IO.File.Exists(Path.Combine(path, original));
						if (exists == true) {

							return true;

						}

					}

				}

			} catch (Exception) {
			}

			return false;

		}

		public static bool IsValidPackage(string path) {

			return ME.Utilities.CacheByFrame<bool>("FlowEditorUtilities.IsValidPackage." + path, () => {

				if (System.IO.Directory.Exists(path) == true) {
					
					var isScreens = System.IO.Directory.Exists(Path.Combine(path, "Screens"));
					var isLayouts = System.IO.Directory.Exists(Path.Combine(path, "Layouts"));
					var isComponents = System.IO.Directory.Exists(Path.Combine(path, "Components"));
					
					if (isScreens == true && isLayouts == true && isComponents == true) {
						
						return true;
						
					}

				}

				return false;

			});

		}

		public static string NormalizePath(string path) {

			var newPath = new List<string>();
			var splitted = path.Split(Path.DirectorySeparatorChar);
			for (int i = 0; i < splitted.Length; ++i) {

				if (splitted[i] == "..") {

					if (newPath.Count > 0) newPath.RemoveAt(newPath.Count - 1);
					continue;

				}

				newPath.Add(splitted[i]);

			}

			return string.Join(Path.DirectorySeparatorChar.ToString(), newPath.ToArray());

		}

		public static UnityEngine.Object GetPackage(GameObject source) {

			if (ME.EditorUtilities.IsPrefab(source) == true) {
				
				var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(source));
				var newPath = path;

				var iterations = path.Split(Path.DirectorySeparatorChar).Length;
				for (int i = 0; i < iterations; ++i) {

					newPath = Path.Combine(newPath, "..");
					if (FlowEditorUtilities.IsValidPackage(newPath) == true) {

						return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(FlowEditorUtilities.NormalizePath(newPath));

					}

				}

			}

			return null;

		}

	}

}
