using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ME;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using ADB = UnityEditor.AssetDatabase;
#endif

namespace UnityEngine.UI.Windows.Plugins.Flow {

	public class FlowSystem {

		public FlowData data;

		private static FlowSystem _instance;
		private static FlowSystem instance {

			get {

				if (FlowSystem._instance == null) FlowSystem._instance = new FlowSystem();
				return FlowSystem._instance;

			}

		}
		
		public static Vector2 grid;

		public static Rect Grid(Rect rect) {
			
			rect.x = Mathf.Round(rect.x / FlowSystem.grid.x) * FlowSystem.grid.x;
			rect.y = Mathf.Round(rect.y / FlowSystem.grid.y) * FlowSystem.grid.y;
			
			return rect;
			
		}

		public static void Save() {

			FlowSystem.instance.data.Save();

		}

		public static FlowData GetData() {
			
			return FlowSystem.instance.data;
			
		}

		public static void SetData(FlowData data) {

			FlowSystem.instance.data = data;

		}

		public static bool HasData() {

			return FlowSystem.instance.data != null;

		}
		
		public static IEnumerable<FlowWindow> GetWindows() {
			
			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetWindows();
			
		}
		
		public static IEnumerable<FlowWindow> GetContainers() {

			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetContainers();
			
		}

		public static FlowWindow GetWindow(int id) {

			return FlowSystem.instance.data.GetWindow(id);

		}
		
		public static FlowWindow CreateContainer() {
			
			return FlowSystem.instance.data.CreateContainer();
			
		}

		public static FlowWindow CreateWindow() {

			return FlowSystem.instance.data.CreateWindow();

		}

		public static void DestroyWindow(int id) {

			FlowSystem.instance.data.DestroyWindow(id);

		}
		
		public static void Attach(int source, int other, bool oneWay) {

			FlowSystem.instance.data.Attach(source, other, oneWay);

		}
		
		public static void Detach(int source, int other, bool oneWay) {
			
			FlowSystem.instance.data.Detach(source, other, oneWay);

		}
		
		public static bool AlreadyAttached(int source, int other) {
			
			return FlowSystem.instance.data.AlreadyAttached(source, other);

		}
		
		public static void SetScrollPosition(Vector2 pos) {
			
			if (FlowSystem.HasData() == false) return;

			FlowSystem.instance.data.SetScrollPosition(pos);
			
		}
		
		public static Vector2 GetScrollPosition() {

			if (FlowSystem.HasData() == false) return Vector2.zero;

			return FlowSystem.instance.data.GetScrollPosition();
			
		}
		
		public static void MoveContainerOrWindow(int id, Vector2 delta) {
			
			var window = FlowSystem.GetWindow(id);
			if (window.isContainer == true) {
				
				var childs = window.attaches;
				foreach (var child in childs) {
					
					FlowSystem.MoveContainerOrWindow(child, delta);
					
				}
				
			} else {
				
				window.Move(delta);
				
			}
			
		}
		
		public static void ForEachContainer(int startId, System.Func<FlowWindow, string, string> each, string accumulate = "") {
			
			var window = FlowSystem.GetWindow(startId);
			if (window.isContainer == true) {

				accumulate += each(window, accumulate);

				var childs = window.attaches;
				foreach (var child in childs) {
					
					FlowSystem.ForEachContainer(child, each, accumulate);
					
				}
				
			}
			
		}

		public static void SelectWindowsInRect(Rect rect, System.Func<FlowWindow, bool> predicate = null) {

			if (FlowSystem.HasData() == false) return;

			FlowSystem.instance.data.SelectWindowsInRect(rect, predicate);

		}

		public static List<int> GetSelected() {

			return FlowSystem.instance.data.GetSelected();

		}

		public static void ResetSelection() {

			FlowSystem.instance.data.ResetSelection();

		}

#if UNITY_EDITOR
		private static void CreateDirectory(string root, string folder, bool suppressWarnings = false) {
		
			var path = root	+ "/" + folder;

			if (System.IO.Directory.Exists(path) == false) {
				
				System.IO.Directory.CreateDirectory(path);
				ADB.Refresh();
				
			} else {

				if ( !suppressWarnings ) {
					
					Debug.LogWarning("Folder Already Exists: " + path);
				}

			}

		}

		private static string LoadScriptTemplate(string templateName, string projectName, string containerNamespace, string screenName, out string className) {

			className = null;

			var file = Resources.Load("UI.Windows/Templates/" + templateName) as TextAsset;
			if (file == null) return null;
			
			projectName = projectName.UppercaseFirst() + ".UI" + (string.IsNullOrEmpty(containerNamespace) == false ? ("." + containerNamespace) : string.Empty);
			className = screenName.UppercaseFirst() + "Screen";

			var text = file.text;
			text = text.Replace("{NAMESPACE_NAME}", projectName);
			text = text.Replace("{CLASS_NAME}", className);

			return text;

		}

		private static void GenerateUIWindow(string fullpath, string containerNamespace, FlowWindow window, bool recompile = false) {

			FlowSystem.CreateDirectory(fullpath, window.directory, suppressWarnings: true);
			FlowSystem.CreateDirectory(fullpath, window.directory + "/" + COMPONENTS_FOLDER, suppressWarnings: true);
			FlowSystem.CreateDirectory(fullpath, window.directory + "/" + LAYOUT_FOLDER, suppressWarnings: true);
			FlowSystem.CreateDirectory(fullpath, window.directory + "/" + SCREENS_FOLDER, suppressWarnings: true);

			if (window.compiled == false || recompile == true) {

				var screenName = window.directory;
				var tplName = "TemplateScreen";
				var className = string.Empty;
				var tplData = FlowSystem.LoadScriptTemplate( tplName, FlowSystem.currentProject, containerNamespace, screenName, out className );
				if (tplData != null) {

					var filepath = ( fullpath + "/" + window.directory + "/" + className + ".cs" ).Replace( "//", "/" );
#if !WEBPLAYER
					File.WriteAllText(filepath, tplData);

					ADB.ImportAsset( filepath );
#endif

				} else {

					Debug.LogError("Template Loading Error: " + FlowSystem.currentProject + " -> " + tplName);

				}
				
				window.compiledClassName = className;
				window.compiledScreenName = screenName;
				window.compiledNamespace = FlowSystem.currentProject.UppercaseFirst() + ".UI" + (string.IsNullOrEmpty(containerNamespace) == false ? ("." + containerNamespace) : string.Empty);;
				
				window.compiledDirectory = fullpath + "/" + window.directory;
				window.compiledDirectory = window.compiledDirectory.Replace("//", "/");

				window.compiled = true;
			}

		}

		public const string COMPONENTS_FOLDER = "Components";
		public const string LAYOUT_FOLDER = "Layouts";
		public const string SCREENS_FOLDER = "Screens";
		public const string OTHER_NAME = "Other";

		private static string currentProject;
		public static void GenerateUI(string pathToData, bool recompile = false) {

			var dir = pathToData.Split( '/' );
			var filename = dir[dir.Length - 1];
			var directory = string.Join( "/", dir ).Replace( filename, "" );

			FlowSystem.currentProject = filename.Split( '.' )[0];
			var fullpath = directory + "/" + FlowSystem.currentProject;

			FlowSystem.CreateDirectory(fullpath, string.Empty, suppressWarnings: true);
			FlowSystem.CreateDirectory(fullpath, OTHER_NAME, suppressWarnings: true);

			ADB.StartAssetEditing();

			var containers = FlowSystem.GetContainers();
			foreach (var container in containers) {

				if (containers.Any((c) => c.HasContainer(container)) == false) {

					FlowSystem.ForEachContainer(container.id, (child, accumulate) => {
						
						var local = (string.IsNullOrEmpty(accumulate) == false ? accumulate + "/" : string.Empty) + child.directory;
						if (string.IsNullOrEmpty(child.directory) == false) {
							
							FlowSystem.CreateDirectory(fullpath, local, suppressWarnings: true);
							
						}
						
						var localNamespace = local.Replace("/", ".");
						foreach (var attachId in child.attaches) {

							var window = FlowSystem.GetWindow(attachId);
							if (window.isContainer == false) {

								FlowSystem.GenerateUIWindow(fullpath + "/" + local + "/", localNamespace, window, recompile);

							}

						}

						return local;

					}, string.Empty);

				}

			}

			var windows = FlowSystem.GetWindows();
			foreach (var window in windows) {

				if (window.HasContainer() == false) {

					// Put into other
					FlowSystem.GenerateUIWindow(fullpath + "/" + OTHER_NAME + "/", OTHER_NAME, window, recompile);

				}

			}

			ADB.StopAssetEditing();
		}
#endif

	}

}