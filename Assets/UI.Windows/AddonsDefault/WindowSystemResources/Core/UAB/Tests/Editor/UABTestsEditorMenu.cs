using ME.UAB.Tests;

namespace ME.UAB.Editor {

	public static class UABTestsEditorMenu {

		#region Tests Menu
		[UnityEditor.MenuItem("Tools/ME.UAB/Tests/One Selection->Pack->Unpack", false, 20)]
		public static void OneSelectionPackUnpack() {

			UABTests.OneSelectionPackUnpack();

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Tests/One Selection->Pack", false, 21)]
		public static void OneSelectionPack() {

			UABTests.OneSelectionPack();

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Tests/One Selection->Pack into File", false, 22)]
		public static void OneSelectionPackFile() {

			UABTests.OneSelectionPackToFile(filename: "Assets/UAB/Test.bytes");

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Tests/One Selection->Unpack", false, 23)]
		public static void OneSelectionUnpack() {

			UABTests.OneSelectionUnpack();

		}

		[UnityEditor.MenuItem("Tools/ME.UAB/Tests/Build Unity AssetBundles", false, 24)]
		public static void BuildUnityAssetBundles() {

			var dir = UnityEditor.EditorUtility.OpenFolderPanel("Choose Directory", "Directory to Save Unity AssetBundles", "UAB");
			if (string.IsNullOrEmpty(dir) == false) {

				UABTests.BuildUnityAssetBundles(dir);

			}

		}
		#endregion

	}

}