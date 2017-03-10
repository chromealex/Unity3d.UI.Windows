using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ME;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI.Windows.Plugins.Flow;
using System.Text.RegularExpressions;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEngine.UI.Windows.Plugins.FlowCompiler {

	public static class Tpl {

		public class Info {

			public string baseNamespace;
			public string classname;
			public string baseClassname;
			public string screenName;
			
			public string classnameFile {
				
				get {
					
					return this.classname + ".cs";
					
				}
				
			}
			
			public string baseClassnameFile {
				
				get {
					
					return this.baseClassname + ".cs";
					
				}
				
			}
			
			public string classnameWithNamespace {
				
				get {
					
					return this.baseNamespace + "." + this.screenName;
					
				}
				
			}

			public Info(FD.FlowWindow window) {
				
				this.baseNamespace = window.compiledNamespace;
				this.classname = window.compiledDerivedClassName;
				this.baseClassname = window.compiledBaseClassName;
				this.screenName = window.directory;

			}

			public Info(string baseNamespace, string classname, string baseClassname, string screenName) {

				this.baseNamespace = baseNamespace;
				this.classname = classname;
				this.baseClassname = baseClassname;
				this.screenName = screenName;

			}

			public string Replace(string replace, System.Func<string, string> predicate = null) {

				replace = replace.Replace("{CLASS_NAME}", this.classname);
				replace = replace.Replace("{BASE_CLASS_NAME}", this.baseClassname);
				replace = replace.Replace("{NAMESPACE_NAME}", this.baseNamespace);
				replace = replace.Replace("{CLASS_NAME_WITH_NAMESPACE}", this.classnameWithNamespace);

				if (predicate != null) replace = predicate(replace);

				return replace;

			}

		}

		public static string ReplaceText(string text, Info oldInfo, Info newInfo) {

			return TemplateGenerator.ReplaceText(text, oldInfo, newInfo);

		}

		public static string GetNamespace() {

			return CompilerSystem.currentNamespace;

		}
		
		public static string GetBaseClassName(FD.FlowWindow flowWindow) {
			
			return flowWindow.directory.UppercaseFirst() + "ScreenBase";
			
		}
		
		public static string GetDerivedClassName(FD.FlowWindow flowWindow) {
			
			return flowWindow.directory.UppercaseFirst() + "Screen";
			
		}
		
		public static string GetNamespace(FD.FlowWindow window) {
			
			return Tpl.GetNamespace() + IO.GetRelativePath(window, ".");
			
		}
		
		public static string GetClassNameWithNamespace(FD.FlowWindow window) {
			
			return Tpl.GetNamespace(window) + "." + Tpl.GetDerivedClassName(window);
			
		}

		public static string GenerateTransitionMethods(FD.FlowWindow window) {

			var flowData = FlowSystem.GetData();
			
			var transitions = flowData.windowAssets.Where(w => window.attachItems.Any((item) => item.targetId == w.id) && !w.IsContainer());

			var result = string.Empty;
			foreach (var each in transitions) {
				
				var className = each.directory;
				var classNameWithNamespace = Tpl.GetNamespace(each) + "." + Tpl.GetDerivedClassName(each);

				if (each.IsShowDefault() == true) {

					// Collect all default windows
					foreach (var defaultWindowId in FlowSystem.GetDefaultWindows()) {

						var defaultWindow = FlowSystem.GetWindow(defaultWindowId);

						className = defaultWindow.directory;
						classNameWithNamespace = Tpl.GetNamespace(defaultWindow) + "." + Tpl.GetDerivedClassName(defaultWindow);

						result += TemplateGenerator.GenerateWindowLayoutTransitionMethod(window, each, className, classNameWithNamespace);
						WindowSystem.CollectCallVariations(each.GetScreen().Load<WindowBase>(), (listTypes, listNames) => {

							result += TemplateGenerator.GenerateWindowLayoutTransitionTypedMethod(window, each, className, classNameWithNamespace, listTypes, listNames);

						});

					}

					continue;

				} else {

					if (each.CanCompiled() == false) continue;

				}

				result += TemplateGenerator.GenerateWindowLayoutTransitionMethod(window, each, className, classNameWithNamespace);
				WindowSystem.CollectCallVariations(each.GetScreen().Load<WindowBase>(), (listTypes, listNames) => {
					
					result += TemplateGenerator.GenerateWindowLayoutTransitionTypedMethod(window, each, className, classNameWithNamespace, listTypes, listNames);

				});

			}

			var c = 0;
			var everyPlatformHasUniqueName = false;
			foreach (var attachItem in window.attachItems) {

				var attachId = attachItem.targetId;

				var attachedWindow = FlowSystem.GetWindow(attachId);
				var tmp = UnityEditor.UI.Windows.Plugins.Flow.Flow.IsCompilerTransitionAttachedGeneration(window, attachedWindow);
				if (tmp == true) ++c;

			}

			everyPlatformHasUniqueName = c > 1;

			foreach (var attachItem in window.attachItems) {
				
				var attachId = attachItem.targetId;

				var attachedWindow = FlowSystem.GetWindow(attachId);
				if (attachedWindow.IsShowDefault() == true) {

					// Collect all default windows
					foreach (var defaultWindowId in FlowSystem.GetDefaultWindows()) {

						var defaultWindow = FlowSystem.GetWindow(defaultWindowId);

						result += UnityEditor.UI.Windows.Plugins.Flow.Flow.OnCompilerTransitionAttachedGeneration(window, defaultWindow, everyPlatformHasUniqueName);
						WindowSystem.CollectCallVariations(attachedWindow.GetScreen().Load<WindowBase>(), (listTypes, listNames) => {

							result += UnityEditor.UI.Windows.Plugins.Flow.Flow.OnCompilerTransitionTypedAttachedGeneration(window, defaultWindow, everyPlatformHasUniqueName, listTypes, listNames);

						});

					}

					result += TemplateGenerator.GenerateWindowLayoutTransitionMethodDefault();

				}

				/*if (withFunctionRoot == true) {

					var functionId = attachedWindow.GetFunctionId();
					var functionContainer = flowData.GetWindow(functionId);
					if (functionContainer != null) {

						var root = flowData.GetWindow(functionContainer.functionRootId);
						if (root != null) {

							attachedWindow = root;

						}

					}

				}*/

				result += UnityEditor.UI.Windows.Plugins.Flow.Flow.OnCompilerTransitionAttachedGeneration(window, attachedWindow, everyPlatformHasUniqueName);
				WindowSystem.CollectCallVariations(attachedWindow.GetScreen().Load<WindowBase>(), (listTypes, listNames) => {
					
					result += UnityEditor.UI.Windows.Plugins.Flow.Flow.OnCompilerTransitionTypedAttachedGeneration(window, attachedWindow, everyPlatformHasUniqueName, listTypes, listNames);
					
				});

			}

			// Run addons transition logic
			result += UnityEditor.UI.Windows.Plugins.Flow.Flow.OnCompilerTransitionGeneration(window);

			return result;

		}

	}

}
