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

namespace UnityEngine.UI.Windows.Plugins.FlowCompiler {

	public class Tpl {

		public class Info {

			public string baseNamespace;
			public string classname;
			public string baseClassname;
			public string screenName;
			
			public string classnameFile {
				
				get {
					
					return this.classname + ".cs";
					
				}
				
			}
			
			public string baseClassnameFile {
				
				get {
					
					return this.baseClassname + ".cs";
					
				}
				
			}
			
			public string classnameWithNamespace {
				
				get {
					
					return this.baseNamespace + "." + this.screenName;
					
				}
				
			}

			public Info(FlowWindow window) {
				
				this.baseNamespace = window.compiledNamespace;
				this.classname = window.compiledDerivedClassName;
				this.baseClassname = window.compiledBaseClassName;
				this.screenName = window.directory;

			}

			public Info(string baseNamespace, string classname, string baseClassname, string screenName) {

				this.baseNamespace = baseNamespace;
				this.classname = classname;
				this.baseClassname = baseClassname;
				this.screenName = screenName;

			}

			public string Replace(string replace, System.Func<string, string> predicate = null) {

				replace = replace.Replace("{CLASS_NAME}", this.classname);
				replace = replace.Replace("{BASE_CLASS_NAME}", this.baseClassname);
				replace = replace.Replace("{NAMESPACE_NAME}", this.baseNamespace);
				replace = replace.Replace("{CLASS_NAME_WITH_NAMESPACE}", this.classnameWithNamespace);

				if (predicate != null) replace = predicate(replace);

				return replace;

			}

		}

		public static string ReplaceText(string text, Info oldInfo, Info newInfo) {

			return FlowTemplateGenerator.ReplaceText(text, oldInfo, newInfo);

		}

		public static string GetNamespace() {

			return FlowCompilerSystem.currentNamespace;

		}
		
		public static string GetBaseClassName(FlowWindow flowWindow) {
			
			return flowWindow.directory.UppercaseFirst() + "ScreenBase";
			
		}
		
		public static string GetDerivedClassName(FlowWindow flowWindow) {
			
			return flowWindow.directory.UppercaseFirst() + "Screen";
			
		}
		
		public static string GetNamespace(FlowWindow window) {
			
			return Tpl.GetNamespace() + IO.GetRelativePath(window, ".");

		}
		
		public static string GenerateTransitionMethods(FlowWindow window) {

			var flowData = FlowSystem.GetData();
			
			var transitions = flowData.windows.Where(w => window.attachItems.Any((item) => item.targetId == w.id) && w.CanCompiled() && !w.IsContainer());

			var result = string.Empty;
			foreach (var each in transitions) {
				
				var className = each.directory;
				var classNameWithNamespace = Tpl.GetNamespace(each) + "." + Tpl.GetDerivedClassName(each);
				
				result += FlowTemplateGenerator.GenerateWindowLayoutTransitionMethod(window, each, className, classNameWithNamespace);

			}

			// Make FlowDefault() method if exists
			var c = 0;
			var everyPlatformHasUniqueName = false;
			foreach (var attachItem in window.attachItems) {

				var attachId = attachItem.targetId;

				var attachedWindow = FlowSystem.GetWindow(attachId);
				var tmp = UnityEditor.UI.Windows.Plugins.Flow.Flow.IsCompilerTransitionAttachedGeneration(window, attachedWindow);
				if (tmp == true) ++c;

			}

			everyPlatformHasUniqueName = c > 1;

			foreach (var attachItem in window.attachItems) {
				
				var attachId = attachItem.targetId;

				var attachedWindow = FlowSystem.GetWindow(attachId);
				if (attachedWindow.IsShowDefault() == true) {

					result += FlowTemplateGenerator.GenerateWindowLayoutTransitionMethodDefault();

				}

				result += UnityEditor.UI.Windows.Plugins.Flow.Flow.OnCompilerTransitionAttachedGeneration(window, attachedWindow, everyPlatformHasUniqueName);

			}

			// Run addons transition logic
			result += UnityEditor.UI.Windows.Plugins.Flow.Flow.OnCompilerTransitionGeneration(window);

			return result;

		}

	}

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

			if (Directory.Exists(path) == false) {
				
				Directory.CreateDirectory(path);
				AssetDatabase.Refresh();
				
			}
			#endif
			
		}
		
		private static IEnumerable<FlowWindow> GetParentContainers(FlowWindow window, IEnumerable<FlowWindow> containers) {
			
			var parent = containers.FirstOrDefault(where => where.attachItems.Any((item) => item.targetId == window.id));
			
			while (parent != null) {
				
				yield return parent;
				parent = containers.FirstOrDefault(where => where.attachItems.Any((item) => item.targetId == parent.id));

			}

		}
		
		public static string GetRelativePath(FlowWindow window, string token) {
			
			var result = GetParentContainers(window, FlowSystem.GetContainers())
					.Reverse()
					.Select(w => w.directory)
					.Aggregate(string.Empty, (total, path) => total + token + path);
			
			if (string.IsNullOrEmpty(result) == true) {
				
				result = token + FlowDatabase.OTHER_NAME;

			}
			
			result += token + window.directory;
			
			return result;

		}

	}

}

namespace UnityEngine.UI.Windows.Plugins.FlowCompiler {

	public static class FlowCompilerSystem {
		
		public static string currentNamespace;
		private static string currentProject;
		private static string currentProjectDirectory;

#if UNITY_EDITOR

		#region OLD
		/*
		private static bool CompiledInfoIsInvalid( FlowWindow flowWindow ) {

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
		
		private static void GenerateUIWindow( string fullpath, FlowWindow window, bool recompile = false ) {

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
		
		public static void GenerateUI( string pathToData, bool recompile = false, Func<FlowWindow, bool> predicate = null ) {

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

		private static void GenerateWindow(string newPath, FlowWindow window, bool recompile) {

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
				IO.ReplaceInFiles(FlowCompilerSystem.currentProjectDirectory, (file) => {
					
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

			var baseClassTemplate = FlowTemplateGenerator.GenerateWindowLayoutBaseClass(newInfo.baseClassname, newInfo.baseNamespace, Tpl.GenerateTransitionMethods(window));
			var derivedClassTemplate = FlowTemplateGenerator.GenerateWindowLayoutDerivedClass(newInfo.classname, newInfo.baseClassname, newInfo.baseNamespace);
			
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
		
		private static void Generate(string pathToData, bool recompile, System.Func<FlowWindow, bool> predicate) {

			var filename = Path.GetFileName(pathToData);
			var directory = pathToData.Replace(filename, string.Empty);
			
			FlowCompilerSystem.currentProject = Path.GetFileNameWithoutExtension(pathToData);
			FlowCompilerSystem.currentProjectDirectory = directory;

			var basePath = directory + FlowCompilerSystem.currentProject;
			IO.CreateDirectory(basePath, string.Empty);
			IO.CreateDirectory(basePath, FlowDatabase.OTHER_NAME);

			predicate = predicate ?? delegate { return true; };

			AssetDatabase.StartAssetEditing();
			{

				try {

					var windows = FlowSystem.GetWindows().Where(w => w.CanCompiled() && predicate(w));

					foreach (var each in windows) {

						var relativePath = IO.GetRelativePath(each, "/");
						FlowCompilerSystem.GenerateWindow(basePath + relativePath + "/", each, recompile);

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
		public static void GenerateByTag(string pathToData, int tag, bool recompile = false) {

			FlowCompilerSystem.Generate(pathToData, recompile, flowWindow => flowWindow.tags.Contains(tag));

		}

		public static void GenerateByTags(string pathToData, int[] tags, bool recompile = false) {

			FlowCompilerSystem.Generate(pathToData, recompile, flowWindow => {

				foreach (var tag in flowWindow.tags) {

					if (tags.Contains(tag) == true) return true;

				}

				return false;

			});

		}
		
		public static void GenerateByWindow(string pathToData, bool recompile = false, FlowWindow window = null) {
			
			FlowCompilerSystem.Generate(pathToData, recompile, flowWindow => flowWindow == window);
			
		}
		
		public static void Generate(string pathToData, bool recompile = false) {
			
			FlowCompilerSystem.Generate(pathToData, recompile, null);
			
		}
		#endregion

#endif

	}

}