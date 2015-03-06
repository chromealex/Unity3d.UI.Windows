

using System.IO;
using System.Linq;
using ME;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI.Windows.Plugins.Flow;

public static class FlowCompiler {


#if UNITY_EDITOR
	private static void CreateDirectory( string root, string folder, bool suppressWarnings = false ) {

		var path = root + "/" + folder;

		if ( Directory.Exists( path ) == false ) {

			Directory.CreateDirectory( path );
			AssetDatabase.Refresh();

		} else {

			if ( !suppressWarnings ) {

				Debug.LogWarning( "Folder Already Exists: " + path );
			}

		}

	}

	private static string LoadScriptTemplate( string templateName, string projectName, string containerNamespace, string screenName, out string className ) {

		className = null;

		var file = Resources.Load( "UI.Windows/Templates/" + templateName ) as TextAsset;
		if ( file == null ) return null;

		projectName = projectName.UppercaseFirst() + ".UI" + ( string.IsNullOrEmpty( containerNamespace ) == false ? ( "." + containerNamespace ) : string.Empty );
		className = screenName.UppercaseFirst() + "Screen";

		var text = file.text;
		text = text.Replace( "{NAMESPACE_NAME}", projectName );
		text = text.Replace( "{CLASS_NAME}", className );

		return text;

	}

	private static void GenerateUIWindow( string fullpath, string containerNamespace, FlowWindow window, bool recompile = false ) {

		CreateDirectory( fullpath, window.directory, suppressWarnings: true );
		CreateDirectory( fullpath, window.directory + "/" + COMPONENTS_FOLDER, suppressWarnings: true );
		CreateDirectory( fullpath, window.directory + "/" + LAYOUT_FOLDER, suppressWarnings: true );
		CreateDirectory( fullpath, window.directory + "/" + SCREENS_FOLDER, suppressWarnings: true );

		if ( window.compiled == false || recompile == true ) {

			var screenName = window.directory;
			var tplName = "TemplateScreen";
			var className = string.Empty;
			var tplData = LoadScriptTemplate( tplName, currentProject, containerNamespace, screenName, out className );
			if ( tplData != null ) {

				var filepath = ( fullpath + "/" + window.directory + "/" + className + ".cs" ).Replace( "//", "/" );
#if !WEBPLAYER
				File.WriteAllText( filepath, tplData );

				AssetDatabase.ImportAsset( filepath );
#endif

			} else {

				Debug.LogError( "Template Loading Error: " + currentProject + " -> " + tplName );

			}

			window.compiledClassName = className;
			window.compiledScreenName = screenName;
			window.compiledNamespace = currentProject.UppercaseFirst() + ".UI" + ( string.IsNullOrEmpty( containerNamespace ) == false ? ( "." + containerNamespace ) : string.Empty ); ;

			window.compiledDirectory = fullpath + "/" + window.directory;
			window.compiledDirectory = window.compiledDirectory.Replace( "//", "/" );

			window.compiled = true;
		}

	}

	public const string COMPONENTS_FOLDER = "Components";
	public const string LAYOUT_FOLDER = "Layouts";
	public const string SCREENS_FOLDER = "Screens";
	public const string OTHER_NAME = "Other";

	private static string currentProject;
	public static void GenerateUI( string pathToData, bool recompile = false ) {

		var dir = pathToData.Split( '/' );
		var filename = dir[dir.Length - 1];
		var directory = string.Join( "/", dir ).Replace( filename, "" );

		currentProject = filename.Split( '.' )[0];
		var fullpath = directory + "/" + currentProject;

		CreateDirectory( fullpath, string.Empty, suppressWarnings: true );
		CreateDirectory( fullpath, OTHER_NAME, suppressWarnings: true );

		AssetDatabase.StartAssetEditing();

		var containers = FlowSystem.GetContainers();
		foreach ( var container in containers ) {

			if ( containers.Any( ( c ) => c.HasContainer( container ) ) == false ) {

				FlowSystem.ForEachContainer( container.id, ( child, accumulate ) => {

					var local = ( string.IsNullOrEmpty( accumulate ) == false ? accumulate + "/" : string.Empty ) + child.directory;
					if ( string.IsNullOrEmpty( child.directory ) == false ) {

						CreateDirectory( fullpath, local, suppressWarnings: true );

					}

					var localNamespace = local.Replace( "/", "." );
					foreach ( var attachId in child.attaches ) {

						var window = FlowSystem.GetWindow( attachId );
						if ( window.isContainer == false && window.isDefaultLink == false ) {

							GenerateUIWindow( fullpath + "/" + local + "/", localNamespace, window, recompile );

						}

					}

					return local;

				}, string.Empty );

			}

		}

		var windows = FlowSystem.GetWindows();
		foreach ( var window in windows ) {

			if ( window.isDefaultLink == false && window.HasContainer() == false ) {

				// Put into other
				GenerateUIWindow( fullpath + "/" + OTHER_NAME + "/", OTHER_NAME, window, recompile );

			}

		}

		AssetDatabase.StopAssetEditing();
	}
#endif

}