using UnityEngine;
using System.Collections;
using UnityEditor.UI.Windows.Plugins.Flow;

namespace ME {

	public partial class GUILayoutExt {
		
		public static bool LargeButton(string caption, float height, float maxWidth) {
			
			var content = new GUIContent(caption);
			return GUILayout.Button(content, FlowSystemEditorWindow.defaultSkin.button, GUILayout.Height(height), GUILayout.MaxWidth(maxWidth));
			
		}
		
		public static bool LargeButton(string caption, params GUILayoutOption[] layouts) {
			
			var content = new GUIContent(caption);
			return GUILayout.Button(content, FlowSystemEditorWindow.defaultSkin.button, layouts);
			
		}

	}

}