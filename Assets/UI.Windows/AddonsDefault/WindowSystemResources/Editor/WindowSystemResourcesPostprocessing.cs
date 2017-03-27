using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows {

	/*public class WindowSystemResourcesPostprocessing : AssetPostprocessor {

		public static void OnPostprocessAllAssets(string[] importedAssets,
		                                          string[] deletedAssets,
		                                          string[] movedAssets,
		                                          string[] movedFromAssetPaths) {

			var mapInstance = WindowSystemResourcesMap.FindFirst();
			if (mapInstance == null) return;
			
			foreach (var item in importedAssets) {
				
				var texture = AssetDatabase.LoadAssetAtPath<Texture>(item);
				if (texture != null) mapInstance.Validate(texture);
				
			}
			
			foreach (var item in movedAssets) {
				
				var texture = AssetDatabase.LoadAssetAtPath<Texture>(item);
				if (texture != null) mapInstance.Validate(texture);
				
			}

		}

	}*/

}