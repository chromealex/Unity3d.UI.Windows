using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows {

	public class WindowComponentLibraryMenu {

		[MenuItem("Assets/Create Component...", validate = true)]
		private static bool IsDirectoryValidate() {
			
			var obj = Selection.activeObject;
			
			if (obj != null) {
				
				var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
				if (path.Length > 0) {
					
					if (Directory.Exists(path)) {

						var splitted = path.Split('/');
						if (splitted[splitted.Length - 1] == "Components") {
								
							return true;

						}

					}

				}
				
			}
			
			return false;
			
		}
		
		[MenuItem("Assets/Create Component...", isValidateFunction: false, priority: 21)]
		public static void CreateComponent() {

			var obj = Selection.activeObject;
			var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

			WindowComponentLibraryChooser.Show((element) => {
				
				element.CopyTo(path);

			});
			
		}

	}

}