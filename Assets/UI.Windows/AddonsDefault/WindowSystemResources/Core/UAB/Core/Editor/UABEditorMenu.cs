using ME.UAB.Tests;

namespace ME.UAB.Editor {

	public static class UABEditorMenu {

		#region Tools Menu
		[UnityEditor.MenuItem("Tools/ME.UAB/Rebuild All...", false, 8)]
		public static void RebuildAll() {

			var dir = UnityEditor.EditorUtility.OpenFolderPanel("Choose Directory", "Directory to Save UAB", "UAB");
			if (string.IsNullOrEmpty(dir) == false) {
				
				Builder.RebuildAll(dir);
				UnityEditor.EditorPrefs.SetString("UAB.LastBuildDirectory", dir);

			}

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Rebuild All (Last Dir)", true)]
		public static bool RebuildLastValidator() {

			var dir = UnityEditor.EditorPrefs.GetString("UAB.LastBuildDirectory", string.Empty);
			return string.IsNullOrEmpty(dir) == false;

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Rebuild All (Last Dir)", false, 9)]
		public static void RebuildLast() {

			var dir = UnityEditor.EditorPrefs.GetString("UAB.LastBuildDirectory", string.Empty);
			Builder.RebuildAll(dir);

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Build All...", false, 10)]
		public static void BuildAll() {

			var dir = UnityEditor.EditorUtility.OpenFolderPanel("Choose Directory", "Directory to Save UAB", "UAB");
			if (string.IsNullOrEmpty(dir) == false) {

				Builder.BuildAll(dir);
				UnityEditor.EditorPrefs.SetString("UAB.LastBuildDirectory", dir);

			}

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Build All (Last Dir)", true)]
		public static bool BuildLastValidator() {

			var dir = UnityEditor.EditorPrefs.GetString("UAB.LastBuildDirectory", string.Empty);
			return string.IsNullOrEmpty(dir) == false;

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Build All (Last Dir)", false, 11)]
		public static void BuildLast() {

			var dir = UnityEditor.EditorPrefs.GetString("UAB.LastBuildDirectory", string.Empty);
			Builder.BuildAll(dir);


		}
		#endregion

	}

}