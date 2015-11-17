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

		#region OLD
		/*
		private static bool CompiledInfoIsInvalid( FD.FlowWindow flowWindow ) {

			return GetBaseClassName( flowWindow ) != flowWindow.compiledBaseClassName
				|| GetNamespace( flowWindow ) != flowWindow.compiledNamespace;
		}

		private static void UpdateInheritedClasses( string oldBaseClassName, string newBaseClassName, string oldDerivedClassName, string newDerivedClassName, string oldNamespace, string newNamespace ) {

			if ( string.IsNullOrEmpty( oldBaseClassName ) || string.IsNullOrEmpty( newBaseClassName ) ) {

				return;
			}

			var oldFullClassPath = oldNamespace + oldBaseClassName;
			var newFullClassPath = newNamespace + newBaseClassName;

			AssetDatabase.StartAssetEditing();

			try {

				var scripts =
					AssetDatabase.FindAssets( "t:MonoScript" )
						.Select( _ => AssetDatabase.GUIDToAssetPath( _ ) )
						.Select( _ => AssetDatabase.LoadAssetAtPath( _, typeof( MonoScript ) ) )
						.OfType<MonoScript>()
						.Where( _ => _.text.Contains( oldBaseClassName ) || _.text.Contains( oldDerivedClassName ) || _.text.Contains( oldNamespace ) )
						.Where( _ => _.name != newBaseClassName );

				foreach ( var each in scripts ) {

					var path = AssetDatabase.GetAssetPath( each );

					var lines = File.ReadAllLines( path );

					var writer = new StreamWriter( path );

					foreach ( var line in lines ) {

						writer.WriteLine( line.Replace( oldFullClassPath, newFullClassPath )
											  .Replace( oldNamespace, newNamespace )
											  .Replace( oldBaseClassName, newBaseClassName )
											  .Replace( oldDerivedClassName, newDerivedClassName ) );
					}

					writer.Dispose();
				}
			} catch ( Exception e ) { Debug.LogException( e ); }

			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );

		}
		
		private static void GenerateUIWindow( string fullpath, FD.FlowWindow window, bool recompile = false ) {

			var isCompiledInfoInvalid = window.compiled && CompiledInfoIsInvalid( window );

			if ( window.compiled == false || recompile == true || isCompiledInfoInvalid ) {

				var baseClassName = GetBaseClassName( window );
				var derivedClassName = GetDerivedClassName( window );
				var classNamespace = GetNamespace( window );

				var baseClassTemplate = FlowTemplateGenerator.GenerateWindowLayoutBaseClass( baseClassName, classNamespace, GenerateTransitionMethods( window ) );
				var derivedClassTemplate = FlowTemplateGenerator.GenerateWindowLayoutDerivedClass( derivedClassName, baseClassName, classNamespace );
				
				#if !UNITY_WEBPLAYER
				var baseClassPath = ( fullpath + "/" + baseClassName + ".cs" ).Replace( "//", "/" );
				var derivedClassPath = ( fullpath + "/" + derivedClassName + ".cs" ).Replace( "//", "/" );
				#endif

				if ( baseClassTemplate != null && derivedClassTemplate != null ) {

					IO.CreateDirectory( fullpath, string.Empty );
					IO.CreateDirectory( fullpath, FlowDatabase.COMPONENTS_FOLDER );
					IO.CreateDirectory( fullpath, FlowDatabase.LAYOUT_FOLDER );
					IO.CreateDirectory( fullpath, FlowDatabase.SCREENS_FOLDER );
					
					#if !UNITY_WEBPLAYER

					Directory.CreateDirectory( fullpath );

					File.WriteAllText( baseClassPath, baseClassTemplate );

					if ( !File.Exists( derivedClassPath ) ) {

						File.WriteAllText( derivedClassPath, derivedClassTemplate );

						AssetDatabase.ImportAsset( derivedClassName );
					}

					AssetDatabase.ImportAsset( baseClassPath );

					#endif

				} else {

					return;
				}

				var oldBaseClassName = window.compiledBaseClassName;
				var newBaseClassName = baseClassName;
				var oldDerivedClassName = window.compiledDerivedClassName;
				var newDerivedClassName = derivedClassName;

				var oldNamespace = window.compiledNamespace;

				window.compiledBaseClassName = baseClassName;
				window.compiledDerivedClassName = derivedClassName;
				window.compiledNamespace = classNamespace;

				var newNamespace = window.compiledNamespace;

				window.compiledDirectory = fullpath;

				window.compiled = true;
				
				AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );

				if ( isCompiledInfoInvalid ) {

					EditorApplication.delayCall += () => UpdateInheritedClasses( oldBaseClassName, newBaseClassName, oldDerivedClassName, newDerivedClassName, oldNamespace, newNamespace );
				}
			}

		}
		
		public static void GenerateUI( string pathToData, bool recompile = false, Func<FD.FlowWindow, bool> predicate = null ) {

			var filename = Path.GetFileName( pathToData );
			var directory = pathToData.Replace( filename, "" );

			currentProject = Path.GetFileNameWithoutExtension( pathToData );
			var basePath = directory + currentProject;

			CreateDirectory( basePath, string.Empty );
			CreateDirectory( basePath, FlowDatabase.OTHER_NAME );

			AssetDatabase.StartAssetEditing();

			predicate = predicate ?? delegate { return true; };

			try {

				foreach ( var each in FlowSystem.GetWindows().Where( _ => !_.isDefaultLink && predicate( _ ) ) ) {

					var relativePath = GetRelativePath( each, "/" );

					if ( !string.IsNullOrEmpty( each.directory ) ) {

						CreateDirectory( basePath, relativePath );
					}

					GenerateUIWindow( basePath + relativePath + "/", each, recompile );
				}
			} catch ( Exception e ) {

				Debug.LogException( e );
			}

			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );

		}*/
		#endregion

		private static void GenerateWindow(string newPath, FD.FlowWindow window, bool recompile, bool minimalScriptsSize) {

			if (window.compiled == true && recompile == false) return;

			var oldPath = window.compiledDirectory;
			
			var newInfo = new Tpl.Info(Tpl.GetNamespace(window), Tpl.GetDerivedClassName(window), Tpl.GetBaseClassName(window), window.directory);
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

			IO.CreateDirectory(path, string.Empty);
			IO.CreateDirectory(path, FlowDatabase.COMPONENTS_FOLDER);
			IO.CreateDirectory(path, FlowDatabase.LAYOUT_FOLDER);
			IO.CreateDirectory(path, FlowDatabase.SCREENS_FOLDER);

			var baseClassTemplate = TemplateGenerator.GenerateWindowLayoutBaseClass(newInfo.baseClassname, newInfo.baseNamespace, Tpl.GenerateTransitionMethods(window));
			var derivedClassTemplate = TemplateGenerator.GenerateWindowLayoutDerivedClass(newInfo.classname, newInfo.baseClassname, newInfo.baseNamespace);

			if (minimalScriptsSize == true) {

				baseClassTemplate = CompilerSystem.Compress(baseClassTemplate);

			}

			if (baseClassTemplate != null && derivedClassTemplate != null) {

				IO.CreateFile(path, newInfo.baseClassnameFile, baseClassTemplate, rewrite: true);
				IO.CreateFile(path, newInfo.classnameFile, derivedClassTemplate, rewrite: false);
				
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

			data = Regex.Replace(data, blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings, me => {
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

				throw new Exception("`pathToData` is wrong: " + pathToData + ". Filename: " + filename);

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

					var windows = FlowSystem.GetWindows().Where(w => w.CanCompiled() && predicate(w));

					foreach (var each in windows) {

						var relativePath = IO.GetRelativePath(each, "/");
						CompilerSystem.GenerateWindow(basePath + relativePath + "/", each, recompile, minimalScriptsSize);

					}

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