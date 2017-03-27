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
		#endregion

	}

}