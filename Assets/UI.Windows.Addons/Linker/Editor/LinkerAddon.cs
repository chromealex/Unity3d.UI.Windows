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
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Plugins.Linker {
	/*
	public static class FlowFunctionsTemplateGenerator {
		
		public static string GenerateReturnMethod(FlowSystemEditorWindow flowEditor, FD.FlowWindow exitWindow) {
			
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
			
			var file = Resources.Load("UI.Windows/Functions/Templates/TemplateTransitionTypedMethod") as TextAsset;
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

	}*/

	public class Linker : FlowAddon {

		private Editor editor;

		public override void OnFlowSettingsGUI() {
			
			GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);
			
			GUILayout.Label("Linker", EditorStyles.boldLabel);

			var style = new GUIStyle(GUI.skin.label);
			style.wordWrap = true;
			GUILayout.Label("Use by clicking on `Create->Linker` menu.", style);

		}
		
		public override void OnFlowCreateMenuGUI(string prefix, GenericMenu menu) {

			if (this.InstallationNeeded() == false) {

				menu.AddSeparator(prefix);

				menu.AddItem(new GUIContent(string.Format("{0}Linker", prefix)), on: false, func: () => {

					this.flowEditor.CreateNewItem(() => {
						
						var window = FlowSystem.CreateWindow(flags:
							UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.IsSmall |
							UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.CantCompiled |
							UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.IsLinker
						);

						window.title = "Linker";
						window.name = window.ToString();
						window.rect.width = 150f;
						window.rect.height = 90f;

						window.smallStyleDefault = "flow node 2";
						window.smallStyleSelected = "flow node 2 on";

						return window;

					});

				});

			}

		}

		public override bool IsCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo) {

			return 
				windowTo.IsLinker() == true &&
				windowTo.GetLinkerId() > 0;

		}

		/*public override string OnCompilerTransitionGeneration(FD.FlowWindow window) {
			
			return Tpl.GenerateTransitionMethods(window);

		}
		
		public override string OnCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName) {
			
			if (windowTo.IsFunction() == true && 
			    windowTo.IsSmall() == true &&
			    windowTo.IsContainer() == false &&
			    windowTo.GetFunctionId() > 0) {
				
				return FlowFunctionsTemplateGenerator.GenerateTransitionMethod(this.flowEditor, windowFrom, windowTo);

			}
			
			return base.OnCompilerTransitionAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName);
			
		}*/

		/*public override string OnCompilerTransitionAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName) {
			
			if (windowTo.IsLinker() == true &&
				windowTo.GetLinkerId() > 0) {

				var result = string.Empty;

				var linkerWindow = FlowSystem.GetWindow(windowTo.GetLinkerId());

				var className = linkerWindow.directory;
				var classNameWithNamespace = Tpl.GetNamespace(linkerWindow) + "." + Tpl.GetDerivedClassName(linkerWindow);

				result += TemplateGenerator.GenerateWindowLayoutTransitionMethod(windowFrom, linkerWindow, className, classNameWithNamespace);

				WindowSystem.CollectCallVariations(linkerWindow.GetScreen(), (listTypes, listNames) => {

					result += TemplateGenerator.GenerateWindowLayoutTransitionTypedMethod(windowFrom, linkerWindow, className, classNameWithNamespace, listTypes, listNames);

				});

				return result;

			}

			return base.OnCompilerTransitionAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName);

		}*/

		/*public override string OnCompilerTransitionTypedAttachedGeneration(FD.FlowWindow windowFrom, FD.FlowWindow windowTo, bool everyPlatformHasUniqueName, System.Type[] types, string[] names) {
			
			if (windowTo.IsLinker() == true &&
				windowTo.GetLinkerId() > 0) {

				var result = string.Empty;

				var window = FlowSystem.GetWindow(windowTo.GetLinkerId());

				var className = window.directory;
				var classNameWithNamespace = Tpl.GetNamespace(window) + "." + Tpl.GetDerivedClassName(window);

				result += TemplateGenerator.GenerateWindowLayoutTransitionMethod(windowFrom, window, className, classNameWithNamespace);

				WindowSystem.CollectCallVariations(windowTo.GetScreen(), (listTypes, listNames) => {

					result += TemplateGenerator.GenerateWindowLayoutTransitionTypedMethod(windowFrom, window, className, classNameWithNamespace, listTypes, listNames);

				});

				Debug.Log(className + " :: " + classNameWithNamespace + " == " + result);

				return result;

			}
			
			return base.OnCompilerTransitionTypedAttachedGeneration(windowFrom, windowTo, everyPlatformHasUniqueName, types, names);
			
		}*/

		public override void OnFlowWindowGUI(FD.FlowWindow window) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			var flag =
				(window.IsLinker() == true && 
				 window.IsSmall() == true &&
				 window.IsContainer() == false);

			if (flag == true) {
				
				var alreadyConnectedFunctionIds = new List<int>();

				// Find caller window
				var windowFrom = data.windowAssets.FirstOrDefault((item) => item.HasAttach(window.id));
				if (windowFrom != null) {
					
					var attaches = windowFrom.GetAttachedWindows();
					foreach (var attachWindow in attaches) {
						
						if (attachWindow.IsLinker() == true) {
							
							alreadyConnectedFunctionIds.Add(attachWindow.GetLinkerId());
							
						}
						
					}
					
				}
				
				foreach (var win in data.windowAssets) {

					if (win.CanDirectCall() == true) {

						var count = alreadyConnectedFunctionIds.Count((e) => e == win.id);
						if ((window.GetLinkerId() == win.id && count == 1) || count == 0) {
							
						} else {
							
							if (win.id == window.GetLinkerId()) window.linkerId = 0;
							alreadyConnectedFunctionIds.Remove(win.id);
							
						}

					}

				}

				var linkerId = window.GetLinkerId();
				var linker = linkerId == 0 ? null : data.GetWindow(linkerId);
				var isActiveSelected = true;

				var oldColor = GUI.color;
				GUI.color = isActiveSelected ? Color.white : Color.grey;
				var result = GUILayout.Button(linker != null ? string.Format("{0} ({1})", linker.title, linker.directory) : "None", FlowSystemEditorWindow.defaultSkin.button, GUILayout.ExpandHeight(true));
				GUI.color = oldColor;
				var rect = GUILayoutUtility.GetLastRect();
				rect.y += rect.height;

				if (result == true) {

					System.Action<int> onApply = (int id) => {

						var linkerSources = new List<FD.FlowWindow>();
						foreach (var w in data.windowAssets) {

							if (w.AlreadyAttached(window.id) == true) {

								linkerSources.Add(w);

							}

						}

						if (window.linkerId != 0) {

							foreach (var w in linkerSources) {

								data.Detach(w.id, window.linkerId, oneWay: true);

							}

						}

						window.linkerId = id;

						if (window.linkerId != 0) {

							foreach (var w in linkerSources) {

								data.Attach(w.id, window.linkerId, oneWay: true);

							}

						}

					};

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("None"), window.linkerId == 0, () => {

						onApply(0);

					});

					if (windowFrom != null) {

						alreadyConnectedFunctionIds.Clear();
						var attaches = windowFrom.GetAttachedWindows();
						foreach (var attachWindow in attaches) {
							
							if (attachWindow.IsLinker() == true) {
								
								alreadyConnectedFunctionIds.Add(attachWindow.GetLinkerId());
								
							}
							
						}
						
					}
					foreach (var win in data.windowAssets) {
						
						if (win.CanDirectCall() == true) {

							var caption = new GUIContent(string.Format("{0} ({1})", win.title, win.directory));

							var count = alreadyConnectedFunctionIds.Count((e) => e == win.id);
							if (((window.GetLinkerId() == win.id && count == 1) || count == 0) && window.compiled == true) {

								var id = win.id;
								menu.AddItem(caption, win.id == window.GetLinkerId(), () => {

									onApply(id);

								});

							} else {
								
								if (window.compiled == false) caption = new GUIContent(string.Format("{0} (Compilation needed)", caption.text));
								if (win.id == window.GetLinkerId()) window.linkerId = 0;

								alreadyConnectedFunctionIds.Remove(win.id);
								menu.AddDisabledItem(caption);

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