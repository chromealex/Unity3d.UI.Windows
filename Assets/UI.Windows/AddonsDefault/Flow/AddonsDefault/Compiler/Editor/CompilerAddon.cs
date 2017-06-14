using UnityEditor;
using UnityEngine.UI.Windows;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEngine.UI.Windows.Plugins.FlowCompiler;

namespace UnityEditor.UI.Windows.Plugins.Compiler {

	public class Compiler : FlowAddon {
		
		private GUISkin skin;

		public static void ShowEditor(System.Action onClose, System.Action onComplete) {

			CompilerWizard.ShowEditor(onClose, onComplete);

		}

		public override void Show(System.Action onClose) {
			
			Compiler.ShowEditor(onClose, null);

		}

		public override void OnFlowToolsMenuGUI(string prefix, GenericMenu menu) {
			
			menu.AddSeparator(prefix);
			
			#if WEBPLAYER
			menu.AddDisabledItem(new GUIContent("Compile UI..."));
			#else
			menu.AddItem(new GUIContent(prefix + "Compile UI Wizard..."), on: false, func: () => {
				
				Compiler.ShowEditor(null, null);
				
			});
			menu.AddItem(new GUIContent(prefix + "Compile UI"), on: false, func: () => {

				CompilerSystem.currentNamespace = FlowSystem.GetData().namespaceName;
				CompilerSystem.Generate(AssetDatabase.GetAssetPath(FlowSystem.GetData()), recompile: true, minimalScriptsSize: false);

			});
			#endif

		}
		
		public override void OnFlowToolbarGUI(GUIStyle buttonStyle) {
			
			base.OnFlowToolbarGUI(buttonStyle);
			
			if (FlowSystem.IsCompileDirty() == true) {
				
				var color = GUI.color;
				
				GUI.color = Color.red;
				if (GUILayout.Button("Recompile", buttonStyle) == true) {

					CompilerSystem.currentNamespace = FlowSystem.GetData().namespaceName;
					CompilerSystem.Generate(AssetDatabase.GetAssetPath(FlowSystem.GetData()), recompile: true, minimalScriptsSize: false);

				}
				
				GUI.color = color;
				
			}
			
		}

		public override void OnFlowSettingsGUI() {

			if (this.skin == null) this.skin = FlowSystemEditorWindow.defaultSkin;

			GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			{
				
				#region NAMESPACE
				GUILayout.Label("Namespace:");
				GUILayout.BeginVertical(GUILayout.Height(30f));
				{

					var namespaceName = EditorGUILayout.TextField(FlowSystem.GetData().namespaceName, this.skin.textField);
					if (namespaceName != FlowSystem.GetData().namespaceName) {
						
						FlowSystem.GetData().namespaceName = namespaceName;
						FlowSystem.SetDirty();

					}

				}
				GUILayout.EndHorizontal();

				var forceRecompile = EditorGUILayout.ToggleLeft("Force Recompile", FlowSystem.GetData().forceRecompile);
				if (forceRecompile != FlowSystem.GetData().forceRecompile) {
					
					FlowSystem.GetData().forceRecompile = forceRecompile;
					FlowSystem.SetDirty();
					
				}
				
				var minimalScriptsSize = EditorGUILayout.ToggleLeft("Minimal Scripts Size", FlowSystem.GetData().minimalScriptsSize);
				if (minimalScriptsSize != FlowSystem.GetData().minimalScriptsSize) {
					
					FlowSystem.GetData().minimalScriptsSize = minimalScriptsSize;
					FlowSystem.SetDirty();
					
				}
				#endregion

			}
			EditorGUILayout.EndVertical();

		}

		public override void OnFlowWindowGUI(FD.FlowWindow window) {

			if (window.CanCompiled() == false) return;

			if (string.IsNullOrEmpty(window.compiledDirectory) == false) {
				
				window.compiled = System.IO.File.Exists(window.compiledDirectory + "/" + window.compiledBaseClassName + ".cs");
				
			}
			
			var oldColor = GUI.color;
			var style = new GUIStyle("U2D.dragDotDimmed");
			var styleCompiled = new GUIStyle("U2D.dragDot");
			
			var elemWidth = style.fixedWidth - 3f;
			
			var posY = -1f;
			var posX = -1f;
			
			GUI.color = window.compiled ? Color.white : Color.red;
			GUI.Label(new Rect(posX, posY, elemWidth, style.fixedHeight), new GUIContent(string.Empty, window.compiled ? "Compiled" : "Not compiled"), window.compiled ? styleCompiled : style);
			
			GUI.color = oldColor;
			
		}

	}

}