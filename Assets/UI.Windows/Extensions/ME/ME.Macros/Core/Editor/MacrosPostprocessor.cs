#define SEARCH_SOURCES_IN_TEXT

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace ME.Macros {

	public static class MacrosPostprocessor/* : AssetPostprocessor*/ {
		
		/*public static void OnPostprocessAllAssets(
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths) {*/

		[MenuItem("Tools/ME.Macros/Process Selected")]
		public static void ProcessSelectedObjects() {

			var objects = Selection.objects;
			if (objects == null) return;

			var paths = new List<string>();
			foreach (var obj in objects) {
				
				var path = AssetDatabase.GetAssetPath(obj);
				paths.Add(path);

			}

			MacrosPostprocessor.ProcessFiles(paths.ToArray());

		}

		public static void ProcessFiles(string[] importedAssets) {

			MacrosSystem.Clear();

			//var macrosRefreshed = false;
			
			var output = string.Empty;
			var defCount = 0;

			foreach (var file in importedAssets) {

				if (MacrosSystem.IsFileMacrosDefinition(file) == true) {

					MacrosSystem.ProcessMacrosDefinition(file);
					output += "Processing definition: " + file + "\n";
					++defCount;

				}

				//if (importedAssets.Contains(file) == true) macrosRefreshed = true;
				
				#if SEARCH_SOURCES_IN_TEXT
				if (MacrosSystem.IsFileMacrosDefinition(file, true) == true) {
					
					MacrosSystem.ProcessMacrosDefinition(file);
					output += "Processing definition: " + file + "\n";
					++defCount;
					
				}
				
				//if (importedAssets.Contains(file) == true) macrosRefreshed = true;
				#endif

			}

			if (defCount > 0) Debug.Log("[MACROS] Definitions processed: " + defCount.ToString() + "\n" + output);

			output = string.Empty;
			var pCount = 0;

			foreach (var file in importedAssets) {
				
				if (MacrosSystem.IsFileContainsMacros(file) == true) {

					MacrosSystem.Process(file);
					output += "Processing: " + file + "\n";
					++pCount;

				}

			}

			if (pCount > 0) Debug.Log("[MACROS] Processed: " + pCount.ToString() + "\n" + output);

		}

	}

}