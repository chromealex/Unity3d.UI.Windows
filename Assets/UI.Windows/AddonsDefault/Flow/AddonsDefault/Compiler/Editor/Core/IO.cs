using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ME;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Text.RegularExpressions;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEngine.UI.Windows.Plugins.FlowCompiler {

	public static class IO {
		
		public static void RenameDirectory(string oldPath, string newPath) {
			
			oldPath = oldPath.Replace("//", "/");
			newPath = newPath.Replace("//", "/");
			
			oldPath = oldPath.Trim('/');
			newPath = newPath.Trim('/');
			
			//Debug.Log("MoveAsset: " + oldPath + " => " + newPath);
			AssetDatabase.MoveAsset(oldPath, newPath);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			
		}

		public static void RenameFile(string oldFile, string newFile) {
			
			oldFile = oldFile.Replace("//", "/");
			newFile = newFile.Replace("//", "/");
			
			//Debug.Log("RenameAsset: " + oldFile + " => " + newFile);
			//Debug.Log(AssetDatabase.RenameAsset(oldFile, newFile));
			System.IO.File.Move(oldFile, newFile);
			AssetDatabase.ImportAsset(newFile);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			
		}

		public static void ReplaceInFiles(string path, System.Func<MonoScript, bool> predicate, System.Func<string, string> replace) {
			
			#if !UNITY_WEBPLAYER
			try {
				
				path = path.Trim('/');
				
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				
				var scripts = AssetDatabase.FindAssets("t:MonoScript", new string[] { path })
					.Select(file => AssetDatabase.GUIDToAssetPath(file))
					.Select(file => AssetDatabase.LoadAssetAtPath(file, typeof(MonoScript)))
					.OfType<MonoScript>()
					.Where(predicate);
				
				foreach (var each in scripts) {
					
					var filepath = AssetDatabase.GetAssetPath(each);
					/*var lines = File.ReadAllLines(filepath);
					var writer = new StreamWriter(filepath);

					foreach (var line in lines) {

						writer.WriteLine(replace(line));

					}
					
					writer.Dispose();*/
					
					File.WriteAllText(filepath, replace(File.ReadAllText(filepath)));
					
				}
				
			} catch (Exception e) {
				
				Debug.LogException(e);
				
			}
			#endif
			
		}

		public static void CreateFile(string path, string filename, string content, bool rewrite = true) {
			
			#if !UNITY_WEBPLAYER
			IO.CreateDirectory(path, string.Empty);
			
			path = path.Trim('/');
			filename = filename.Trim('/');
			
			var filepath = Path.Combine(path, filename);
			filepath = filepath.Replace("//", "/");
			
			if (File.Exists(filepath) == false || rewrite == true) {
				
				File.WriteAllText(filepath, content);
				AssetDatabase.ImportAsset(filepath);
				
			}
			#endif
			
		}

		public static void CreateDirectory(string root, string folder) {
			
			#if !UNITY_WEBPLAYER
			folder = folder.Trim('/');
			root = root.Trim('/');
			
			var path = Path.Combine(root, folder);
			path = path.Replace("//", "/");

			var dummyFile = string.Format("{0}/Dummy.txt", path);

			var filesCount = Directory.GetFiles(path).Count(x => Path.GetFileName(x) != "Dummy.txt" && x.EndsWith(".meta") == false);
			if (filesCount > 0) {
				
				File.Delete(dummyFile);

			} else {

				File.WriteAllText(dummyFile, string.Empty);

			}

			if (Directory.Exists(path) == false) {

				File.WriteAllText(dummyFile, string.Empty);

				Directory.CreateDirectory(path);
				AssetDatabase.Refresh();
				
			}
			#endif
			
		}

		private static IEnumerable<FD.FlowWindow> GetParentContainers(FD.FlowWindow window, IEnumerable<FD.FlowWindow> containers) {
			
			var parent = containers.FirstOrDefault(where => where.attachItems.Any((item) => item.targetId == window.id));
			
			while (parent != null) {
				
				yield return parent;
				parent = containers.FirstOrDefault(where => where.attachItems.Any((item) => item.targetId == parent.id));
				
			}
			
		}

		public static string GetRelativePath(FD.FlowWindow window, string token) {
			
			var result = GetParentContainers(window, FlowSystem.GetContainers())
				.Reverse()
				.Select(w => w.directory)
				.Aggregate(string.Empty, (total, path) => total + token + path);
			
			if (string.IsNullOrEmpty(result) == true && window.IsContainer() == false) {
				
				result = token + FlowDatabase.OTHER_NAME;
				
			}
			
			result += token + window.directory;
			
			return result;
			
		}
		
	}

}
