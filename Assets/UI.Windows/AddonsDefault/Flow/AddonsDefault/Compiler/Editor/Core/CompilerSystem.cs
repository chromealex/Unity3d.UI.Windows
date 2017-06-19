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

	public static class CompilerSystem {
		
		public static string currentNamespace;
		private static string currentProject;
		private static string currentProjectDirectory;

		#if UNITY_EDITOR
		private static void GenerateWindow(string newPath, FD.FlowWindow window, bool recompile, bool minimalScriptsSize) {

			if (window.compiled == true && recompile == false) return;

			var oldPath = window.compiledDirectory;
			
			var newInfo = new Tpl.Info(Tpl.GetNamespace(window), Tpl.GetDerivedClassName(window), Tpl.GetBaseClassName(window), Tpl.GetContainerClassName(window), window.directory);
			var oldInfo = new Tpl.Info(window);

			if (string.IsNullOrEmpty(oldPath) == true) {

				oldPath = newPath;

			}

			var path = oldPath;

			if (window.compiled == true && (oldPath != newPath)) {

				// If window is moving and compiled - just rename

				// Replace in files
				IO.ReplaceInFiles(CompilerSystem.currentProjectDirectory, (file) => {
					
					var text = file.text;
					return text.Contains(oldInfo.baseNamespace);
					
				}, (text) => {

					return Tpl.ReplaceText(text, oldInfo, newInfo);

				});

				// Rename base class name
				IO.RenameFile(oldPath + oldInfo.baseClassnameFile, oldPath + newInfo.baseClassnameFile);

				// Rename derived class name
				IO.RenameFile(oldPath + oldInfo.classnameFile, oldPath + newInfo.classnameFile);

				// Rename main folder
				IO.RenameDirectory(oldPath, newPath);

				path = newPath;

			}

			// Rebuild without rename
			//Debug.Log(window.title + " :: REBUILD BASE :: " + path);

			string baseClassTemplate = null;

			if (window.IsContainer() == true) {

				baseClassTemplate = TemplateGenerator.GenerateWindowLayoutContainerBaseClass(newInfo.baseClassname, newInfo.baseNamespace, newInfo.containerClassName);

			} else {

				baseClassTemplate = TemplateGenerator.GenerateWindowLayoutBaseClass(newInfo.baseClassname, newInfo.baseNamespace, newInfo.containerClassName, Tpl.GenerateTransitionMethods(window));

			}

			var derivedClassTemplate = TemplateGenerator.GenerateWindowLayoutDerivedClass(newInfo.classname, newInfo.baseClassname, newInfo.baseNamespace);

			//Debug.Log(newPath + " :: " + newInfo.containerClassName + " :: " + baseClassTemplate);
			//return;

			if (minimalScriptsSize == true) {

				baseClassTemplate = CompilerSystem.Compress(baseClassTemplate);

			}

            if (window.IsContainer() == false) {

				IO.CreateFile(path, ".uiwspackage", string.Empty, import: false);
                IO.CreateDirectory(path, string.Empty);
                IO.CreateDirectory(path, FlowDatabase.COMPONENTS_FOLDER);
                IO.CreateDirectory(path, FlowDatabase.LAYOUT_FOLDER);
                IO.CreateDirectory(path, FlowDatabase.SCREENS_FOLDER);

            } else {
                
				IO.CreateFile(path, ".uiwscontainer", string.Empty, import: false);
                
            }

            if (baseClassTemplate != null && derivedClassTemplate != null) {

                IO.CreateFile (path, newInfo.baseClassnameFile, baseClassTemplate, rewrite: true);
                IO.CreateFile (path, newInfo.classnameFile, derivedClassTemplate, rewrite: false);

            }

			window.compiledNamespace = newInfo.baseNamespace;
			window.compiledScreenName = newInfo.screenName;
			window.compiledBaseClassName = newInfo.baseClassname;
			window.compiledDerivedClassName = newInfo.classname;
			
			window.compiledDirectory = path;
			window.compiled = true;

		}

		private static string Compress(string data) {

			var symbols = new string[] { ",", "{", "}", @"\(", @"\)", "==", ">=", "<=", "=>", "-", @"\+", "--", @"\+\+", ">", "<", "=", ":", @"\?" };

			data = data.Replace("\t", string.Empty);

			var blockComments = @"/\*(.*?)\*/";
			var lineComments = @"//(.*?)\r?\n";
			var strings = @"""((\\[^\n]|[^""\n])*)""";
			var verbatimStrings = @"@(""[^""]*"")+";

			data = Regex.Replace(data, string.Format("{0}|{1}|{2}|{3}", blockComments, lineComments, strings, verbatimStrings), me => {
				if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
					return me.Value.StartsWith("//") ? Environment.NewLine : "";
				// Keep the literal strings
				return me.Value;
			},
			RegexOptions.Singleline);

			data = data.Replace("\t", string.Empty);
			data = data.Replace("\r\n", string.Empty);
			data = data.Replace("\r", string.Empty);
			data = data.Replace("\n", string.Empty);

			foreach (var symbol in symbols) {

				data = Regex.Replace(data, @"(\s?" + symbol + @"\s?)", symbol.Replace(@"\", string.Empty));

			}

			return data;

		}
		
		private static void Generate(string pathToData, bool recompile, bool minimalScriptsSize, System.Func<FD.FlowWindow, bool> predicate) {

			var filename = Path.GetFileName(pathToData);
			if (string.IsNullOrEmpty(pathToData) == true) {

				throw new Exception(string.Format("`pathToData` is wrong: {0}. Filename: {1}", pathToData, filename));

			}

			var directory = pathToData.Replace(filename, string.Empty);

			CompilerSystem.currentProject = Path.GetFileNameWithoutExtension(pathToData);
			CompilerSystem.currentProjectDirectory = directory;

			var basePath = directory + CompilerSystem.currentProject;
			IO.CreateDirectory(basePath, string.Empty);
			IO.CreateDirectory(basePath, FlowDatabase.OTHER_NAME);

			predicate = predicate ?? delegate { return true; };

			AssetDatabase.StartAssetEditing();
			{

				try {

					var windows = FlowSystem.GetContainersAndWindows().Where(w => w.CanCompiled() && predicate(w));
					//var windows = FlowSystem.GetContainers().Where(w => w.CanCompiled() && predicate(w));
					foreach (var each in windows) {

						var relativePath = IO.GetRelativePath(each, "/");
						CompilerSystem.GenerateWindow(string.Format("{0}{1}/", basePath, relativePath), each, recompile, minimalScriptsSize);

					}

					// Generate Base Files
					var newInfo = new Tpl.Info(CompilerSystem.currentNamespace, "Container", "ContainerBase", "LayoutWindowType", basePath);
					var baseClassTemplate = TemplateGenerator.GenerateWindowLayoutContainerBaseClass(newInfo.baseClassname, newInfo.baseNamespace, newInfo.containerClassName);
					var derivedClassTemplate = TemplateGenerator.GenerateWindowLayoutDerivedClass(newInfo.classname, newInfo.baseClassname, newInfo.baseNamespace);

					IO.CreateFile(basePath, newInfo.baseClassnameFile, baseClassTemplate, rewrite: true);
					IO.CreateFile(basePath, newInfo.classnameFile, derivedClassTemplate, rewrite: false);

				} catch (Exception e) {
					
					Debug.LogException(e);

				}

			}
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			FlowSystem.SetDirty();
			FlowSystem.Save();

		}

		#region VARIANTS
		public static void GenerateByTag(string pathToData, int tag, bool recompile = false, bool minimalScriptsSize = false) {

			CompilerSystem.Generate(pathToData, recompile, minimalScriptsSize, flowWindow => flowWindow.tags.Contains(tag));

		}

		public static void GenerateByTags(string pathToData, int[] tags, bool recompile = false, bool minimalScriptsSize = false) {

			CompilerSystem.Generate(pathToData, recompile, minimalScriptsSize, flowWindow => {

				foreach (var tag in flowWindow.tags) {

					if (tags.Contains(tag) == true) return true;

				}

				return false;

			});

		}
		
		public static void GenerateByWindow(string pathToData, bool recompile = false, bool minimalScriptsSize = false, FD.FlowWindow window = null) {
			
			CompilerSystem.Generate(pathToData, recompile, minimalScriptsSize, flowWindow => flowWindow == window);
			
		}
		
		public static void Generate(string pathToData, bool recompile = false, bool minimalScriptsSize = false) {
			
			CompilerSystem.Generate(pathToData, recompile, minimalScriptsSize, null);
			
		}
		#endregion
		#endif

	}

}