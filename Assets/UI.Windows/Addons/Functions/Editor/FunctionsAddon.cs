using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Plugins.FlowCompiler;

namespace UnityEditor.UI.Windows.Plugins.Functions {
	
	public static class FlowFunctionsTemplateGenerator {
		
		public static string GenerateReturnMethod(FlowSystemEditorWindow flowEditor, FlowWindow exitWindow) {
			
			var file = Resources.Load("UI.Windows/Functions/Templates/TemplateReturnMethod") as TextAsset;
			if (file == null) {
				
				Debug.LogError("Functions Template Loading Error: Could not load template 'TemplateReturnMethod'");
				
				return string.Empty;
				
			}
			
			var data = FlowSystem.GetData();
			if (data == null) return string.Empty;
			
			var result = string.Empty;
			var part = file.text;

			var functionContainer = exitWindow.GetFunctionContainer();

			var functionName = functionContainer.title;
			var functionCallName = functionContainer.directory;
			var classNameWithNamespace = Tpl.GetNamespace(exitWindow) + "." + Tpl.GetDerivedClassName(exitWindow);
			
			result +=
				part.Replace("{FUNCTION_NAME}", functionName)
					.Replace("{FUNCTION_CALL_NAME}", functionCallName)
					.Replace("{CLASS_NAME_WITH_NAMESPACE}", classNameWithNamespace);
			
			return result;
			
		}
		
		public static string GenerateTransitionMethod(FlowSystemEditorWindow flowEditor, FlowWindow windowFrom, FlowWindow windowTo) {
			
			var file = Resources.Load("UI.Windows/Functions/Templates/TemplateTransitionMethod") as TextAsset;
			if (file == null) {
				
				Debug.LogError("Functions Template Loading Error: Could not load template 'TemplateTransitionMethod'");
				
				return string.Empty;
				
			}
			
			var data = FlowSystem.GetData();
			if (data == null) return string.Empty;
			
			var result = string.Empty;
			var part = file.text;
			
			// Function link
			var functionId = windowTo.GetFunctionId();
			
			// Find function container
			var functionContainer = data.GetWindow(functionId);
			if (functionContainer == null) {
				
				// Function not found
				return string.Empty;
				
			}
			
			// Get function root window
			var root = data.GetWindow(functionContainer.functionRootId);
			//var exit = data.GetWindow(functionContainer.functionExitId);
			
			var functionName = functionContainer.title;
			var functionCallName = functionContainer.directory;
			var classNameWithNamespace = Tpl.GetNamespace(root) + "." + Tpl.GetDerivedClassName(root);
			var transitionMethods = Tpl.GenerateTransitionMethods(windowTo);
			transitionMethods = transitionMethods.Replace("\r\n", "\r\n\t")
												 .Replace("\n", "\n\t");

			result +=
				part.Replace("{TRANSITION_METHODS}", transitionMethods)
					.Replace("{FUNCTION_NAME}", functionName)
					.Replace("{FUNCTION_CALL_NAME}", functionCallName)
					.Replace("{FLOW_FROM_ID}", windowFrom.id.ToString())
					.Replace("{FLOW_TO_ID}", windowTo.id.ToString())
					.Replace("{CLASS_NAME_WITH_NAMESPACE}", classNameWithNamespace);
			
			return result;
			
		}

	}

	public class Functions : FlowAddon {

		private Editor editor;
		
		private List<FlowWindow> functions = new List<FlowWindow>();

		public override void OnFlowSettingsGUI() {
			
			GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);
			
			GUILayout.Label("Functions", EditorStyles.boldLabel);
			
			var data = FlowSystem.GetData();
			if (data == null) return;
			
			this.functions.Clear();
			foreach (var window in data.windows) {
				
				if (window.IsFunction() == true &&
				    window.IsContainer() == true) {
					
					this.functions.Add(window);
					
				}
				
			}

			if (this.functions.Count == 0) {

				var style = new GUIStyle(GUI.skin.label);
				style.wordWrap = true;
				GUILayout.Label("No functions ware found in the current project. Add new one by clicking on `Create->Functions` menu.", style);

			} else {

				foreach (var function in this.functions) {
					
					if (GUILayoutExt.LargeButton(function.title, 20f, FlowSystemEditorWindow.SETTINGS_WIDTH) == true) {

						this.flowEditor.SetCenterTo(function);

					}
					
				}

			}

		}
		
		public override void OnFlowCreateMenuGUI(string prefix, GenericMenu menu) {

			if (this.InstallationNeeded() == false) {

				menu.AddSeparator(prefix);

				menu.AddItem(new GUIContent(prefix + "Functions/Definition"), on: false, func: () => {
					
					this.flowEditor.CreateNewItem(() => {
						
						var window = FlowSystem.CreateWindow(flags: FlowWindow.Flags.IsContainer | FlowWindow.Flags.IsFunction);
						window.title = "Function Definition";

						return window;
						
					});

				});

				menu.AddItem(new GUIContent(prefix + "Functions/Call"), on: false, func: () => {
					
					this.flowEditor.CreateNewItem(() => {
						
						var window = FlowSystem.CreateWindow(flags: FlowWindow.Flags.IsFunction | FlowWindow.Flags.IsSmall | FlowWindow.Flags.CantCompiled);
						window.smallStyleDefault = "flow node 3";
						window.smallStyleSelected = "flow node 3 on";
						window.title = "Function Call";

						window.rect.width = 150f;
						window.rect.height = 100f;

						return window;
						
					});

				});

			}

		}

		public override bool IsCompilerTransitionAttachedGeneration(FlowWindow windowFrom, FlowWindow windowTo) {

			return windowTo.IsFunction() == true && 
					windowTo.IsSmall() == true &&
					windowTo.IsContainer() == false &&
					windowTo.GetFunctionId() > 0;

		}

		public override string OnCompilerTransitionGeneration(FlowWindow window) {
			
			var functionContainer = window.GetFunctionContainer();
			if (functionContainer != null) {
				
				var exit = FlowSystem.GetWindow(functionContainer.functionExitId);
				if (exit != null && exit.id == window.id) {
					
					return FlowFunctionsTemplateGenerator.GenerateReturnMethod(this.flowEditor, exit);
					
				}
				
			}

			return base.OnCompilerTransitionGeneration(window);

		}

		public override string OnCompilerTransitionAttachedGeneration(FlowWindow windowFrom, FlowWindow windowTo, bool everyPlatformHasUniqueName) {

			if (windowTo.IsFunction() == true && 
			    windowTo.IsSmall() == true &&
			    windowTo.IsContainer() == false &&
			    windowTo.GetFunctionId() > 0) {
				
				return FlowFunctionsTemplateGenerator.GenerateTransitionMethod(this.flowEditor, windowFrom, windowTo);
				
			}

			return base.OnCompilerTransitionAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName);
			
		} 

		public override void OnFlowWindowGUI(FlowWindow window) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			var flag = (window.IsFunction() == true && 
					window.IsSmall() == true &&
					window.IsContainer() == false);

			if (flag == true) {
				
				var alreadyConnectedFunctionIds = new List<int>();

				// Find caller window
				var windowFrom = data.windows.FirstOrDefault((item) => item.HasAttach(window.id));
				if (windowFrom != null) {
					
					var attaches = windowFrom.GetAttachedWindows();
					foreach (var attachWindow in attaches) {
						
						if (attachWindow.IsFunction() == true) {
							
							alreadyConnectedFunctionIds.Add(attachWindow.GetFunctionId());
							
						}
						
					}
					
				}
				
				foreach (var win in data.windows) {
					
					if (win.IsFunction() == true &&
					    win.IsContainer() == true) {
						
						var count = alreadyConnectedFunctionIds.Count((e) => e == win.id);
						if ((window.GetFunctionId() == win.id && count == 1) || count == 0) {
							
						} else {
							
							if (win.id == window.functionId) window.functionId = 0;
							alreadyConnectedFunctionIds.Remove(win.id);
							
						}
						
					}
					
				}

				var functionId = window.GetFunctionId();
				var functionContainer = functionId == 0 ? null : data.GetWindow(functionId);
				var isActiveSelected = true;

				var oldColor = GUI.color;
				GUI.color = isActiveSelected ? Color.white : Color.grey;
				var result = GUILayoutExt.LargeButton(functionContainer != null ? functionContainer.title : "None", GUILayout.MaxHeight(60f), GUILayout.MaxWidth(150f));
				GUI.color = oldColor;
				var rect = GUILayoutUtility.GetLastRect();
				rect.y += rect.height;

				if (result == true) {

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("None"), window.functionId == 0, () => {

						window.functionId = 0;

					});

					if (windowFrom != null) {

						alreadyConnectedFunctionIds.Clear();
						var attaches = windowFrom.GetAttachedWindows();
						foreach (var attachWindow in attaches) {
							
							if (attachWindow.IsFunction() == true) {
								
								alreadyConnectedFunctionIds.Add(attachWindow.GetFunctionId());
								
							}
							
						}
						
					}
					foreach (var win in data.windows) {
						
						if (win.IsFunction() == true &&
						    win.IsContainer() == true) {

							var count = alreadyConnectedFunctionIds.Count((e) => e == win.id);
							if ((window.GetFunctionId() == win.id && count == 1) || count == 0) {

								var id = win.id;
								menu.AddItem(new GUIContent(win.title), win.id == window.functionId, () => {

									window.functionId = id;

								});

							} else {

								if (win.id == window.functionId) window.functionId = 0;

								alreadyConnectedFunctionIds.Remove(win.id);
								menu.AddDisabledItem(new GUIContent(win.title));

							}

						}
						
					}

					menu.DropDown(rect);

				}

			}

		}

		public override bool InstallationNeeded() {

			return false;

		}

	}

}