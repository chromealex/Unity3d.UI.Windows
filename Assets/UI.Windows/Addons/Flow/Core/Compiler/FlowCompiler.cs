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

			var path = Path.Combine( root, folder );

			if ( Directory.Exists( path ) == false ) {

				Directory.CreateDirectory( path );
				AssetDatabase.Refresh();
			}
		}

		private static string GetNamespace() {

			return FlowSystem.GetData().namespaceName;
		}

		private static string GetBaseClassName( FlowWindow flowWindow ) {

			return flowWindow.directory.UppercaseFirst() + "ScreenBase";
		}

		private static string GetDerivedClassName( FlowWindow flowWindow ) {

			return flowWindow.directory.UppercaseFirst() + "Screen";
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

		private static bool CompiledInfoIsInvalid( FlowWindow flowWindow ) {

			return GetBaseClassName( flowWindow ) != flowWindow.compiledBaseClassName;
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
				var classNameWithNamespace = GetNamespace( each ) + "." + GetBaseClassName( each );

				result = result + FlowTemplateGenerator.GenerateWindowLayoutTransitionMethod( className, classNameWithNamespace );
			}

			return result;
		}

		private static void GenerateUIWindow( string fullpath, FlowWindow window, bool recompile = false ) {

			var isCompiledInfoInvalid = window.compiled && CompiledInfoIsInvalid( window );

			if ( window.compiled == false || recompile == true || isCompiledInfoInvalid ) {

				var baseClassName = GetBaseClassName( window );
				var derivedClassName = GetDerivedClassName( window );
				var classNamespace = GetNamespace( window );

				var baseClassTemplate = FlowTemplateGenerator.GenerateWindowLayoutBaseClass( baseClassName, classNamespace, GenerateTransitionMethods( window ) );
				var derivedClassTemplate = FlowTemplateGenerator.GenerateWindowLayoutDerivedClass( derivedClassName, baseClassName, classNamespace );

				var baseClassPath = ( fullpath + "/" + baseClassName + ".cs" ).Replace( "//", "/" );
				var derivedClassPath = ( fullpath + "/" + derivedClassName + ".cs" ).Replace( "//", "/" );

				if ( baseClassTemplate != null && derivedClassTemplate != null ) {

					CreateDirectory( fullpath, string.Empty );
					CreateDirectory( fullpath, COMPONENTS_FOLDER );
					CreateDirectory( fullpath, LAYOUT_FOLDER );
					CreateDirectory( fullpath, SCREENS_FOLDER );

					if ( Directory.Exists( window.compiledDirectory ) && fullpath != window.compiledDirectory ) {

						foreach ( var each in Directory.GetFiles( window.compiledDirectory ) ) {

							if ( Path.GetFileNameWithoutExtension( each ) != window.compiledDerivedClassName ) {

								File.Delete( each );
							} else {

								if ( !File.Exists( derivedClassPath ) ) {

									File.Move( each, derivedClassPath );

									AssetDatabase.ImportAsset( derivedClassName );
								}
							}
						}

						Directory.Delete( window.compiledDirectory, recursive: true );
					}

#if !WEBPLAYER

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

				if ( isCompiledInfoInvalid ) {

					AssetDatabase.Refresh( ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate );

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
			CreateDirectory( basePath, OTHER_NAME );

			AssetDatabase.StartAssetEditing();

			predicate = predicate ?? delegate { return true; };

			try {

				foreach ( var each in FlowSystem.GetWindows().Where( predicate ) ) {

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

		public static void GenerateUIByTag( string pathToData, bool recompile = false, int tag = 0 ) {

			GenerateUI( pathToData, recompile, flowWindow => flowWindow.tags.Contains( tag ) );
		}

		public static void GenerateWindow( string pathToData, bool recompile = false, FlowWindow window = null ) {

			GenerateUI( pathToData, recompile, flowWindow => flowWindow == window );
		}

#endif

	}

}