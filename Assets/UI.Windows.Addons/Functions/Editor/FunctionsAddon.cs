using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Plugins.FlowCompiler;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEditor.UI.Windows.Plugins.Functions {
	
	public static class FlowFunctionsTemplateGenerator {
		
		public static string GenerateReturnMethod(FlowSystemEditorWindow flowEditor, FD.FlowWindow exitWindow) {
			
			var file = UnityEngine.Resources.Load("UI.Windows/Functions/Templates/TemplateReturnMethod") as TextAsset;
			if (file == null) {
				
				Debug.LogError("Functions Template Loading Error: Could not load template 'TemplateReturnMethod'");
				
				return string.Empty;
				
			}
			
			var data = FlowSystem.GetData();
			if (data == null) return string.Empty;
			
			var result = string.Empty;
			var part = file.text;

			var functionContainer = exitWindow.GetFunctionContainer();
			if (functionContainer == null) {
				
				// Function not found
				return string.Empty;
				
			}

			var exit = data.GetWindow(functionContainer.functionExitId);

			var functionName = functionContainer.title;
			var functionCallName = functionContainer.directory;
			var classNameWithNamespace = Tpl.GetClassNameWithNamespace(exit);
			
			result +=
				part.Replace("{FUNCTION_NAME}", functionName)
					.Replace("{FUNCTION_CALL_NAME}", functionCallName)
					.Replace("{CLASS_NAME_WITH_NAMESPACE}", classNameWithNamespace);
			
			return result;
			
		}
		
		public static string GenerateTransitionMethod(FlowSystemEditorWindow flowEditor, FD.FlowWindow windowFrom, FD.FlowWindow windowTo) {
			
			var file = UnityEngine.Resources.Load("UI.Windows/Functions/Templates/TemplateTransitionMethod") as TextAsset;
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

			if (root == null) {

				Debug.LogWarning(string.Format("[ GenerateTransitionMethod ] Function Container `{0}` doesn't have root with id `{1}`", functionContainer.name, functionContainer.functionRootId));
				return string.Empty;

			}
			
			var functionName = functionContainer.title;
			var functionCallName = functionContainer.directory;
			var classNameWithNamespace = Tpl.GetClassNameWithNamespace(root);
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
		
		public static string GenerateTransitionTypedMethod(FlowSystemEditorWindow flowEditor, FD.FlowWindow windowFrom, FD.FlowWindow windowTo, System.Type[] parameters, string[] parameterNames) {
			
			var file = UnityEngine.Resources.Load("UI.Windows/Functions/Templates/TemplateTransitionTypedMethod") as TextAsset;
			if (file == null) {
				
				Debug.LogError("Functions Template Loading Error: Could not load template 'TemplateTransitionTypedMethod'");
				
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

			var definition = parameters.Select((x, i) => ME.Utilities.FormatParameter(x) + " " + parameterNames[i]).ToArray();
			var call = parameterNames;
			var description = parameters.Select((x, i) => "/// <param name=\"" + parameterNames[i] + "\">" + parameterNames[i] + " to OnParametersPass</param>").ToArray();

			result +=
				part.Replace("{TRANSITION_METHODS}", transitionMethods)
					.Replace("{FUNCTION_NAME}", functionName)
					.Replace("{FUNCTION_CALL_NAME}", functionCallName)
					.Replace("{FLOW_FROM_ID}", windowFrom.id.ToString())
					.Replace("{FLOW_TO_ID}", windowTo.id.ToString())
					.Replace("{CLASS_NAME_WITH_NAMESPACE}", classNameWithNamespace)
					.Replace("{PARAMETERS_DEFINITION}", string.Join(", ", definition))
					.Replace("{PARAMETERS_CALL}", string.Join(", ", call))
					.Replace("{PARAMETERS_DESCRIPTION}", string.Join(System.Environment.NewLine, description));
			
			return result;
			
		}

	}

	public class Functions : FlowAddon {

		private Editor editor;
		
		private List<FD.FlowWindow> functions = new List<FD.FlowWindow>();

		public override void OnFlowSettingsGUI() {
			
			GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);
			
			GUILayout.Label("Functions", EditorStyles.boldLabel);
			
			var data = FlowSystem.GetData();
			if (data == null) return;
			
			this.functions.Clear();
			foreach (var window in data.windowAssets) {
				
				if (window.IsFunction() == true &&
				    window.IsContainer() == true) {
					
					this.functions.Add(window);
					
				}
				
			}

			if (this.functions.Count == 0) {

				var style = new GUIStyle(GUI.skin.label);
				style.wordWrap = true;
				GUILayout.Label("No functions were found in the current project. Add new one by clicking on `Create->Functions` menu.", style);

			} else {

				foreach (var function in this.functions) {
					
					if (GUILayout.Button(function.title, FlowSystemEditorWindow.defaultSkin.button) == true) {

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
						
						var window = FlowSystem.CreateWindow(flags: FD.FlowWindow.Flags.IsContainer | FD.FlowWindow.Flags.IsFunction);
						window.title = "Function Definition";

						return window;
						
					});

				});

				menu.AddItem(new GUIContent(prefix + "Functions/Call"), on: false, func: () => {
					
					this.flowEditor.CreateNewItem(() => {
						
						var window = FlowSystem.CreateWindow(flags: FD.FlowWindow.Flags.IsFunction | FD.FlowWindow.Flags.IsSmall | FD.FlowWindow.Flags.CantCompiled);
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

		public override bool IsCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo) {

			return windowTo.IsFunction() == true && 
					windowTo.IsSmall() == true &&
					windowTo.IsContainer() == false &&
					windowTo.GetFunctionId() > 0;

		}

		public override string OnCompilerTransitionGeneration(FD.FlowWindow window) {
			
			var functionContainer = window.GetFunctionContainer();
			if (functionContainer != null) {
				
				var exit = FlowSystem.GetWindow(functionContainer.functionExitId);
				if (exit != null && exit.id == window.id) {
					
					return FlowFunctionsTemplateGenerator.GenerateReturnMethod(this.flowEditor, exit);
					
				}
				
			}

			return base.OnCompilerTransitionGeneration(window);

		}
		
		public override string OnCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName) {
			
			if (windowTo.IsFunction() == true && 
			    windowTo.IsSmall() == true &&
			    windowTo.IsContainer() == false &&
			    windowTo.GetFunctionId() > 0) {
				
				return FlowFunctionsTemplateGenerator.GenerateTransitionMethod(this.flowEditor, windowFrom, windowTo);

			}
			
			return base.OnCompilerTransitionAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName);
			
		} 
		
		public override string OnCompilerTransitionTypedAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName, System.Type[] types, string[] names) {
			
			if (windowTo.IsFunction() == true && 
			    windowTo.IsSmall() == true &&
			    windowTo.IsContainer() == false &&
			    windowTo.GetFunctionId() > 0) {

				return FlowFunctionsTemplateGenerator.GenerateTransitionTypedMethod(this.flowEditor, windowFrom, windowTo, types, names);

			}
			
			return base.OnCompilerTransitionTypedAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName, types, names);
			
		} 

		public override void OnFlowWindowGUI(FD.FlowWindow window) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			var flag = (window.IsFunction() == true && 
					window.IsSmall() == true &&
					window.IsContainer() == false);

			if (flag == true) {
				
				var alreadyConnectedFunctionIds = new List<int>();

				// Find caller window
				var windowFrom = data.windowAssets.FirstOrDefault((item) => item.HasAttach(window.id));
				if (windowFrom != null) {
					
					var attaches = windowFrom.GetAttachedWindows();
					foreach (var attachWindow in attaches) {
						
						if (attachWindow.IsFunction() == true) {
							
							alreadyConnectedFunctionIds.Add(attachWindow.GetFunctionId());
							
						}
						
					}
					
				}
				
				foreach (var win in data.windowAssets) {
					
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
				var result = GUILayout.Button(functionContainer != null ? functionContainer.title : "None", FlowSystemEditorWindow.defaultSkin.button, GUILayout.ExpandHeight(true));
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
					foreach (var win in data.windowAssets) {
						
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