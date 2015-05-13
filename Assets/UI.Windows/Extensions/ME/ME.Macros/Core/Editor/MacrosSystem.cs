using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace ME.Macros {
	
	public class MacrosSystem {
		
		public static bool IsFileContainsMacros(string path) {

			var script = AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)) as MonoScript;
			if (script == null) return false;

			return script.text.ContainsMacros();

		}

		public static void Process(string path) {

			var script = AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)) as MonoScript;
			if (script == null) return;

			//Debug.Log("[MACROS] Processing " + path);

			var updated = false;
			string oldText = script.text;

			var names = script.text.GetMacrosNames();
			foreach (var macrosName in names) {

				string text;
				if (MacrosSystem.cacheMacros.TryGetValue(macrosName, out text) == false) {

					Debug.LogWarningFormat("[MACROS] Macros `{0}` doesn't exists in file `{1}`. Skipped.", macrosName, path);

				} else {

					var newText = oldText.RefreshMacros(macrosName, text);
					if (newText != oldText) {

						updated = true;
						oldText = newText;

					}

				}

			}

			if (updated == true) {

				File.WriteAllText(path, oldText);
				// Updated
				//Debug.Log("[MACROS] Updated: " + path);

			}

		}

		private static Dictionary<string, string> cacheMacros = new Dictionary<string, string>();
		public static bool IsFileMacrosDefinition(string path, bool textFilesOnly = false) {

			string text;

			if (textFilesOnly == true) {

				if (Path.GetExtension(path) != ".macros") return false;

				var macros = AssetDatabase.LoadAssetAtPath(path, typeof(Object)) as Object;
				if (macros == null) return false;

				text = File.ReadAllText(path);

			} else {

				var script = AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)) as MonoScript;
				if (script == null) return false;

				text = script.text;

			}

			return text.ContainsMacrosDefinition();

		}

		public static void ProcessMacrosDefinition(string path) {

			var text = File.ReadAllText(path);

			//Debug.Log("[MACROS] Processing definition: " + path);

			var dic = text.GetMacrosDefinitions();

			foreach (var keyValue in dic) {

				var macrosName = keyValue.Key;
				var macrosText = keyValue.Value;

				//Debug.Log("Macros: " + macrosName + " :: " + macrosText);

				if (MacrosSystem.cacheMacros.ContainsKey(macrosName) == true) {

					Debug.LogWarningFormat("[MACROS] Duplicate macros with name `{0}` ({1}). Skipped.", macrosName, path);

				} else {

					MacrosSystem.cacheMacros.Add(macrosName, macrosText);

				}

			}

		}

		public static void Clear() {

			MacrosSystem.cacheMacros.Clear();

		}

	}

}