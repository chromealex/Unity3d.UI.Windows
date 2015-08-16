using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class FlowChooserFilterMenu {

		[MenuItem("Assets/Create Layout...", validate = true)]
		private static bool IsDirectoryOrLayoutValidate() {
			
			return FlowChooserFilter.IsDirectoryOrLayoutValidate(Selection.activeObject);
			
		}
		
		[MenuItem("Assets/Create Layout...", isValidateFunction: false, priority: 21)]
		public static void CreateLayout() {

			FlowChooserFilter.CreateLayout(Selection.activeObject, Selection.activeGameObject);

		}
		
		[MenuItem("Assets/Create Screen...", validate = true)]
		private static bool IsScreenValidate() {
			
			return FlowChooserFilter.IsScreenValidate(Selection.activeObject);

		}

		[MenuItem("Assets/Create Screen...", isValidateFunction: false, priority: 21)]
		public static void CreateScreen() {

			FlowChooserFilter.CreateScreen(Selection.activeObject, string.Empty);
			
		}

	}

}