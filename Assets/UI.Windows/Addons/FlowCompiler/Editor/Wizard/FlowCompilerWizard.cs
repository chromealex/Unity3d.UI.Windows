using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.FlowCompiler {

	public class FlowCompilerWizard : EditorWindowExt {

		public static FlowCompilerWizard ShowEditor(System.Action onClose) {

			var rootWindow = EditorWindow.focusedWindow;
			
			var rootX = rootWindow.position.x;
			var rootY = rootWindow.position.y;
			var rootWidth = rootWindow.position.width;
			var rootHeight = rootWindow.position.height;

			var width = 400f;
			var height = 300f;

			FlowCompilerWizard editor = null;

			FlowCompilerWizard.FocusWindowIfItsOpen<FlowCompilerWizard>();
			editor = EditorWindow.focusedWindow as FlowCompilerWizard;

			if (editor == null) {

				editor = FlowCompilerWizard.CreateInstance<FlowCompilerWizard>();
				editor.title = "Flow Compiler Wizard";
				editor.ShowUtility();

			}

			editor.position = new Rect(rootX + rootWidth * 0.5f - width * 0.5f, rootY + rootHeight * 0.5f - height * 0.5f, width, height);

			editor.Focus();

			return editor;

		}



	}

}