using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ME;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.UI.Windows.Plugins.Flow {

	public static class FlowCompiler {

		public const string COMPONENTS_FOLDER = "Components";
		public const string LAYOUT_FOLDER = "Layouts";
		public const string SCREENS_FOLDER = "Screens";
		public const string OTHER_NAME = "Other";

		private static string currentProject;

#if UNITY_EDITOR

		private static void CreateDirectory( string root, string folder ) {

			var path = root + "/" + folder;

			if ( Directory.Exists( path ) == false ) {

				Directory.CreateDirectory( path );
				AssetDatabase.Refresh();

			}

		}

		private static string GenerateTransitionMethods( FlowWindow window ) {

			if ( window.isContainer ) {

				return string.Empty;
			}

			var flowData = FlowSystem.GetData();

			var transitions = flowData.windows.Where( _ => window.attaches.Contains( _.id ) ).Where( _ => !_.isDefaultLink && !_.isContainer );

			var result = string.Empty;
			foreach ( var each in transitions ) {

				var className = each.directory;
				var classNameWithNamespace = GetNamespace( each ) + "." + GetClassName( each );

				result = result + FlowTemplateGenerator.GenerateWindowLayoutTransitionMethod( className, classNameWithNamespace );
			}

			return result;
		}

		private static void GenerateUIWindow( string fullpath, FlowWindow window, bool recompile = false ) {

			var isCompiledInfoInvalid = window.compiled && CompiledInfoIsInvalid( window );

			if ( window.compiled == false || recompile == true || isCompiledInfoInvalid ) {

				var directory = window.directory;

				var className = GetClassName( window );
				var classNamespace = GetNamespace( window );

				var templateData = FlowTemplateGenerator.GenerateWindowLayoutBaseClass( className, classNamespace, GenerateTransitionMethods( window ) );//LoadScriptTemplate( tplName, currentProject, containerNamespace, screenName, out className );
				if ( templateData != null ) {

					if ( Directory.Exists( window.compiledDirectory ) ) {

						Directory.Delete( window.compiledDirectory, recursive: true );
					}

					CreateDirectory( fullpath, directory );
					CreateDirectory( fullpath, directory + "/" + COMPONENTS_FOLDER );
					CreateDirectory( fullpath, directory + "/" + LAYOUT_FOLDER );
					CreateDirectory( fullpath, directory + "/" + SCREENS_FOLDER );

					var filepath = ( fullpath + "/" + directory + "/" + className + ".cs" ).Replace( "//", "/" );
#if !WEBPLAYER

					File.WriteAllText( filepath, templateData );

					AssetDatabase.ImportAsset( filepath );
#endif

				} else {
					return;
				}

				var oldClassName = window.compiledClassName;
				var newClassName = className;
				var oldNamespace = window.compiledNamespace;

				window.compiledClassName = className;
				window.compiledNamespace = classNamespace;

				var newNamespace = window.compiledNamespace;

				window.compiledDirectory = fullpath + "/" + directory;
				window.compiledDirectory = window.compiledDirectory.Replace( "//", "/" );

				window.compiled = true;

				if ( isCompiledInfoInvalid ) {

					UpdateInheritedClasses( oldClassName, newClassName, oldNamespace, newNamespace );
				}
			}

		}

		private static string GetNamespace() {

			return FlowSystem.GetData().namespaceName;
		}

		private static string GetClassName( FlowWindow flowWindow ) {

			return flowWindow.directory.UppercaseFirst() + "ScreenBase";
		}

		private static bool CompiledInfoIsInvalid( FlowWindow flowWindow ) {

			return GetClassName( flowWindow ) != flowWindow.compiledClassName;
		}

		private static void UpdateInheritedClasses( string oldClassName, string newClassName, string oldNamespace, string newNamespace ) {

			if ( string.IsNullOrEmpty( oldClassName ) || string.IsNullOrEmpty( newClassName ) ) {

				return;
			}

			var scripts =
				AssetDatabase.FindAssets( "t:MonoScript" )
					.Select( _ => AssetDatabase.GUIDToAssetPath( _ ) )
					.Select( _ => AssetDatabase.LoadAssetAtPath( _, typeof( MonoScript ) ) )
					.OfType<MonoScript>();

			foreach ( var each in scripts ) {

				if ( !each.text.Contains( oldClassName ) ) {

					continue;
				}

				var path = AssetDatabase.GetAssetPath( each );

				var lines = File.ReadAllLines( path );

				var writer = new StreamWriter( path );

				foreach ( var line in lines ) {

					if ( line.Contains( "using" ) ) {

						writer.WriteLine( line.Replace( oldNamespace, newNamespace ) );
					} else {

						writer.WriteLine( line.Replace( oldClassName, newClassName ) );
					}
				}

				writer.Dispose();
			}

			AssetDatabase.Refresh();
		}

		private static IEnumerable<FlowWindow> GetParentContainers( FlowWindow window, IEnumerable<FlowWindow> containers ) {

			var parent = containers.FirstOrDefault( where => where.attaches.Contains( window.id ) );

			while ( parent != null ) {

				yield return parent;

				parent = containers.FirstOrDefault( where => where.attaches.Contains( parent.id ) );
			}
		}

		private static string GetRelativePath( FlowWindow window, string token ) {

			return GetParentContainers( window, FlowSystem.GetContainers() )
					.Select( _ => _.directory )
					.Aggregate( string.Empty, ( total, _ ) => total + token + _ )
					   + token + window.directory;
		}

		private static string GetNamespace( FlowWindow window ) {

			return GetNamespace() + GetRelativePath( window, "." );
		}

		public static void GenerateUI( string pathToData, bool recompile = false ) {

			var dir = pathToData.Split( '/' );
			var filename = dir[dir.Length - 1];
			var directory = string.Join( "/", dir ).Replace( filename, "" );

			currentProject = filename.Split( '.' )[0];
			var basePath = directory + currentProject;

			CreateDirectory( basePath, string.Empty );
			CreateDirectory( basePath, OTHER_NAME );

			AssetDatabase.StartAssetEditing();

			try {

				foreach ( var each in FlowSystem.GetWindows() ) {

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
		}

#endif

	}

}