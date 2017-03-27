using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI.Windows.Plugins.Services;
using ME;

namespace UnityEngine.UI.Windows.Plugins.Console {
	
	public class ConsoleManager : ServiceManager<ConsoleManager> {

		public const int TOUCH_COUNT_TO_OPEN = 7;
		public const int TOUCH_COUNT_TO_CLOSE = 3;

		public override string GetServiceName() {

			return "Console";

		}

		public override UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions GetAuthPermission() {

			return UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions.None;

		}

		public static readonly Version VERSION = new Version(1, 1, 0, Version.Type.Beta);

		private ConsoleScreen screen;

		private bool autoComplete = false;
		
		private Dictionary<string, SubModuleBase> modules = new Dictionary<string, SubModuleBase>();
		private Dictionary<string, SubModuleBase> modulesUnique = new Dictionary<string, SubModuleBase>();
		
		private AccessGroup accessMode = AccessGroup.Public;
		
		private bool state = false;
		private List<SubModuleBase> loadedModules = new List<SubModuleBase>();
		
		public Console.SubModules.Bind bind = null;

		public override void OnInitialized() {

			WindowSystem.RegisterWindow(this.GetSettings<ConsoleSettings>().screen);

			this.screen = WindowSystem.Show<ConsoleScreen>();
			this.screen.Register(ConsoleManager.OnCommand, this.OnCommandCheck);

			this.Hide();
			this.StartAll();

		}

		public override void OnInitializedLate() {
			
			base.OnInitializedLate();

			this.StartAllLate();

		}

		public void OnDestroy() {

			if (this.screen != null) {

				this.screen.Hide();

			}

		}

		public void Update() {

			if (this.screen != null) {

				#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8
				if (UnityEngine.Input.touchCount > 0) {

					var failed = false;
					for (int i = 0; i < UnityEngine.Input.touchCount; ++i) {

						var touch = UnityEngine.Input.GetTouch(i);
						if (touch.phase != TouchPhase.Moved &&
							touch.phase != TouchPhase.Stationary) {

							failed = true;
							break;

						}

					}

					if (failed == false) {

						if (UnityEngine.Input.touchCount == ConsoleManager.TOUCH_COUNT_TO_OPEN) {

							if (this.state == false) {

								this.state = true;
								this.Show();

							}

						}

						if (UnityEngine.Input.touchCount == ConsoleManager.TOUCH_COUNT_TO_CLOSE) {

							if (this.state == true) {

								this.state = false;
								this.Hide();

							}

						}

					}

				}
				#endif

				if (UnityEngine.Input.GetKeyDown(KeyCode.BackQuote) == true) {
					
					this.Toggle();
					
				}

				if (this.state == true) {

					if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) == true) {
						
						this.screen.MovePrev();

					}
					
					if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) == true) {
						
						this.screen.MoveNext();

					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.Tab) == true) {

						this.AutoComplete();

					}

				}

			}

		}

		public void Toggle() {

			this.state = !this.state;

			if (this.state == true) {

				this.Show();

			} else {
			
				this.Hide();

			}

		}

		public void Show() {

			this.screen.ShowConsole();

		}

		public void Hide() {

			this.screen.HideConsole();

		}

		public void EnablePrivate() {

			this.accessMode = AccessGroup.Private;

			this.StartAll();

		}

		public void DisablePrivate() {

			this.accessMode = AccessGroup.Private;

			this.StartAll();

		}

		public void ImportModules(string @namespace) {
			
			var query = from t in Assembly.GetExecutingAssembly().GetTypes()
						where t.IsClass && t.IsNested == false && t.Namespace == @namespace
						select t.Name.ToLower();

			foreach (var element in query) {

				if (this.modules.ContainsKey(element) == true) continue;
				
				var instance = System.Activator.CreateInstance(null,
				                                               string.Format("{0}.{1}", @namespace, element.ToLower()),
				                                               true,
				                                               BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
				                                               null, null, null, null, null);

				if (instance != null) {
					
					var module = (SubModuleBase)instance.Unwrap();
					if (module != null) {

						var accessGroups = module.GetType().GetCustomAttributes(typeof(AccessLevelAttribute), inherit: true);
						if (accessGroups != null && accessGroups.Length > 0) {

							var accessGroup = accessGroups[0] as AccessLevelAttribute;
							if (accessGroup.group == AccessGroup.Private && this.accessMode != AccessGroup.Private) continue;

						}

						if (this.loadedModules.Contains(module) == true) {

							Debug.LogWarningFormat("[ Console ] Module import warning: Module `{0}` can't be loaded because it already in list.", element);
							continue;

						}

						var name = module.GetName();
						var aliases = module.GetAliases();

						this.modules.Add(name, module);
						this.modulesUnique.Add(name, module);

						foreach (var alias in aliases) {

							if (this.modules.ContainsKey(alias) == false) {
								
								this.modules.Add(alias, module);
								
							} else {
								
								Debug.LogWarningFormat("[ Console ] Alias warning: Module already contains alias with name `{0}`.", alias);
								
							}

						}

						this.screen.AddLine(string.Format("[ Console ] Module imported: {0}", module.GetType().Name));

						module.OnStart(this.screen);
						this.loadedModules.Add(module);
						
					}
					
				}
				
			}

		}

		private void StartAll() {
			
			string @namespace = "UnityEngine.UI.Windows.Plugins.Console.SubModules";
			this.ImportModules(@namespace);

		}

		private void StartAllLate() {

		    for (var i = 0; i < this.loadedModules.Count; ++i) {

                this.loadedModules[i].OnStartLate();

            }

		}

		public void AutoComplete() {

			var cmd = this.screen.GetInputText();

			var first = cmd;
			string[] subModuleArgs;
			var args = ConsoleManager.GetCmdArgs(ref cmd, out first, out subModuleArgs, System.StringSplitOptions.None);
			
			var variants = new List<string>();
			var variantsConcat = new List<string>();

			//Debug.Log(args.Length);

			if (args.Length == 1) {

				// module auto-complete

				var search = args[0].ToLower();

				foreach (var module in this.loadedModules) {

					var found = false;

					var names = module.GetNames();
					foreach (var name in names) {

						if (name == search) {

							variants.Add(name);
							variantsConcat.Add(name);
							found = true;
							break;

						}

					}

					if (found == false) {

						foreach (var name in names) {

							if (name.StartsWith(search) == true) {

								variants.Add(name);
								variantsConcat.Add(name);
								break;

							}

						}

					}

				}

			} else if (args.Length == 2) {

				// method auto-complete
				var moduleName = args[0].Trim().ToLower();
				if (this.modules.ContainsKey(moduleName) == true) {

					var output = ListPool<string>.Get();
					var outputConcat = ListPool<string>.Get();
					var module = this.modules[moduleName];
					module.HasAutoComplete(cmd, subModuleArgs, output, outputConcat);
					variants.AddRange(output);
					variantsConcat.AddRange(outputConcat);

					ListPool<string>.Release(output);
					ListPool<string>.Release(outputConcat);

				}

			} else if (args.Length > 2) {

				// method parameters auto-complete
				var moduleName = args[0].Trim().ToLower();
				if (this.modules.ContainsKey(moduleName) == true) {
					
					var output = ListPool<string>.Get();
					var outputConcat = ListPool<string>.Get();
					var module = this.modules[moduleName];
					module.GetParamAutoComplete(cmd, subModuleArgs, args.Length, output, outputConcat);
					variants.AddRange(output);
					//variantsConcat.AddRange(outputConcat);

					ListPool<string>.Release(output);
					ListPool<string>.Release(outputConcat);
					
				}

			}

			if (variantsConcat.Count == 1) {
				
				var c = variantsConcat[0];
				// concat (args - 1) and output with `c`
				var str = string.Format("{0} {1}", string.Join(" ", args, 0, args.Length - 1), c).Trim() + " ";
				this.screen.SetInputText(str);
				this.screen.MoveCaretToEnd();
				
				this.autoComplete = false;

			} else if (variantsConcat.Count > 1) {

				this.screen.AddLine("<color=#c2c>Variants:</color>");
				foreach (var v in variants) {

					this.AddLine(string.Format("\t{0}", v), reusable: false);

				}
				
				this.autoComplete = true;

			} else {

				this.autoComplete = true;

			}

			//Debug.Log("Complete: " + this.screen.GetInputText());

		}

		private static string[] GetCmdArgs(ref string cmd, out string first, out string[] subModuleArgs, System.StringSplitOptions options = System.StringSplitOptions.RemoveEmptyEntries) {

			cmd = cmd.Trim();

			var lines = cmd.Split(new string[] { System.Environment.NewLine }, options);
			if (lines.Length > 1) {
				
				cmd = lines[0];
				
			}

			first = cmd;
			var args = cmd.Split(new char[] { ' ' }, options);
			if (args.Length > 0) first = args[0];

			var subModuleArgsList = new List<string>();
			var startsWithQuote = false;
			var startsWithQuoteIndex = -1;
			for (int i = 0; i < args.Length; ++i) {

				if (args[i].StartsWith(@"""") == true) {

					startsWithQuoteIndex = i;
					startsWithQuote = true;

				}
				
				if (startsWithQuote == true) {
					
					if (args[i].EndsWith(@"""") == true) {

						args[startsWithQuoteIndex] = args[startsWithQuoteIndex].Substring(1);
						args[i] = args[i].Substring(0, args[i].Length - 1);

						var str = string.Empty;
						for (int j = startsWithQuoteIndex; j <= i; ++j) {

							str += args[j] + (j == i ? string.Empty : " ");

						}
						subModuleArgsList.Add(str);
						
						startsWithQuote = false;

					} else if (startsWithQuoteIndex != i) {

						subModuleArgsList.Add(args[i]);

					}

				} else {
					
					args[i] = args[i].ToLower();
					args[i] = Regex.Replace(args[i], @"\s+", " ");

					subModuleArgsList.Add(args[i]);

				}

			}

			//subModuleArgs = string.Join(" ", args, 1, args.Length - 1).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
			var argsOutput = subModuleArgsList.ToArray();
			//subModuleArgs = string.Join(" ", argsOutput, 1, argsOutput.Length - 1).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

			if (subModuleArgsList.Count > 1) {

				subModuleArgs = new string[subModuleArgsList.Count - 1];
				for (int i = 1; i < subModuleArgsList.Count; ++i) {

					subModuleArgs[i - 1] = subModuleArgsList[i];

				}

			} else {

				subModuleArgs = new string[0];

			}

			return argsOutput;

		}
		
		public bool OnCommandCheck(string cmd) {
			
			if (cmd.Trim() == "`" ||
			    cmd.Trim() == "+" ||
			    cmd.Trim() == "-") return false;
			
			if (this.autoComplete == true) {

				// set focus and move caret to last symbol
				this.screen.MoveCaretToEnd();

				this.autoComplete = false;
				return false;
				
			}

			return true;
			
		}

		public static void OnCommand(string cmd) {

			ConsoleManager.instance.OnCommand_INTERNAL(cmd);

		}

		public void OnCommand_INTERNAL(string cmd) {

			if (cmd.Trim() == "`") {
				
				this.screen.SetInputText(string.Empty);
				return;
				
			}

			cmd = cmd.Trim().ToLower();

			//var screen = this.root.GetWindow<GameplayScreen>();

			//string @namespaceBattle = "MW2.Managers.Console.SubModules.Battle";
			//string @namespace = "MW2.Managers.Console.SubModules";
			var first = cmd;
			string[] subModuleArgs;
			var args = ConsoleManager.GetCmdArgs(ref cmd, out first, out subModuleArgs);

			if (cmd == "help") {

				var cmdList = new List<string>();
				var commentsList = new List<string>();
				
				var q = this.modulesUnique.Select((module) => {

					return new {
						name = module.Key.ToLower(),
						aliases = this.modules
							.Where((k) => k.Key == module.Key.ToLower())
							.Select((k) => k.Value.GetType().GetCustomAttributes(typeof(AliasAttribute), inherit: false).Cast<AliasAttribute>().Select((a) => a.name))
					};

				});

				foreach (var item in q) {

					var aliases = new List<string>();
					foreach (var aliasKeys in item.aliases) {

						aliases.AddRange(aliasKeys);

					}

					cmdList.Add(item.name);
					commentsList.Add(aliases.Count > 0 ? ("Aliases: " + string.Join(", ", aliases.ToArray())) : string.Empty);

				}

				ConsoleManager.ShowHelp(null, cmdList.ToArray(), commentsList.ToArray());
				
				return;

			}

			if (this.modules.ContainsKey("bind") == true) {

				this.bind = (this.modules["bind"] as Console.SubModules.Bind);

			}

			var result = false;

			if (string.IsNullOrEmpty(first.Trim()) == false) {

				var hasAnyGlobalRewrite = false;
				foreach (var moduleKeyValue in this.modules) {

					var module = moduleKeyValue.Value;
					if (module.HasCommandGlobalRewrite(args) == true) {
						
						module.OnCommandGlobalRewrite(args);
						// skip on success
						hasAnyGlobalRewrite = true;

					}

				}

				if (hasAnyGlobalRewrite == false) {

					// Sub module args
					//args = string.Join(" ", args, 1, args.Length - 1).Split(' ');

					if (this.modules.ContainsKey(first.ToLower()) == true) {

					    try {

					        result = this.Call(cmd, this.modules[first], subModuleArgs);

					    } catch (System.Exception exception) {
					        
							Debug.LogWarning(string.Format("Execute console command exception: {0}", exception.Message));

					    }

					} else {

						this.AddCommandNotFound(cmd);

					}

				}

			}

			if (result == true) {

				if (this.modules[first] != this.bind && this.bind.IsBinding() == true) {

					// add to current bind
					this.bind.AddToBind(cmd);

				}

			}

		}
		
		public void AddLine(string line, params object[] pars) {
			
			this.screen.AddLine(line, pars);
			
		}
		
		public void AddLine(string line, bool reusable = false) {
			
			this.screen.AddLine(line, reusable);
			
		}

		public void AddCommandNotFound(string cmd) {

			this.screen.AddLine(string.Format("<color=red>Command `{0}` not found</color>", cmd));

		}

		public static void ShowHelp(string moduleName, string[] commands, string[] comments) {

			ConsoleManager.instance.ShowHelp_INTERNAL(moduleName, commands, comments);

		}

		public void ShowHelp_INTERNAL(string moduleName, string[] commands, string[] comments) {
			
			this.screen.AddLine("----------------------------------------------");
			
			if (moduleName == null) {
				
				this.screen.AddLine("<color=#c2c>Usage:</color>");
				
			} else {

				this.screen.AddLine(string.Format("<color=#c2c>Module `{0}` commands:</color>", moduleName));

			}
			
			this.ShowHelpList(commands, comments);
			
			this.screen.AddLine("----------------------------------------------");

		}

		public void ShowHelpList(string[] commands, string[] comments) {
			
			for (int i = 0; i < commands.Length; ++i) {
				
				if (string.IsNullOrEmpty(comments[i]) == true) {
					
					this.screen.AddLine(string.Format("\t{0}", commands[i]));
					
				} else {
					
					this.screen.AddLine("\t{0}\t// <color=grey>{1}</color>", commands[i], comments[i]);
					
				}
				
			}

		}

		public static string[] GetHelp(SubModuleBase module, string methodNameStart, bool distinct = false, bool commandsOnly = false) {

			return ConsoleManager.instance.ShowHelp_INTERNAL(module, methodNameStart, distinct, commandsOnly);

		}

		public string[] ShowHelp_INTERNAL(SubModuleBase module, string methodNameStart, bool distinct = false, bool commandsOnly = false) {
			
			//var moduleName = module.GetType().Name;
			var help = ListPool<string>.Get();
			var helpMethods = ListPool<string>.Get();
			var methods = ListPool<MethodInfo>.Get();
			ConsoleManager.GetModuleMethods(module, methods);
			foreach (var method in methods) {

				var methodName = method.Name.ToLower();
				if (string.IsNullOrEmpty(methodNameStart) == true || methodName.StartsWith(methodNameStart) == true) {
					
					if (distinct == true) {
						
						if (helpMethods.Contains(methodName) == true) continue;
						
						helpMethods.Add(methodName);

					}

					var pstr = string.Empty;
					var pars = method.GetParameters();
					foreach (var par in pars) {
						
						if (par.ParameterType == typeof(bool)) {
							
							pstr += string.Format("[b:{0}]", par.Name);
							
						} else if (par.ParameterType == typeof(int)) {
							
							pstr += string.Format("[i:{0}]", par.Name);
							
						} else if (par.ParameterType == typeof(long)) {
							
							pstr += string.Format("[l:{0}]", par.Name);
							
						} else if (par.ParameterType == typeof(float)) {
							
							pstr += string.Format("[f:{0}]", par.Name);
							
						} else if (par.ParameterType.IsEnum == true) {
							
							pstr += string.Format("[e:{0}]", par.Name);
							
						} else if (par.ParameterType == typeof(string)) {
							
							pstr += string.Format("[s:{0}]", par.Name);
							
						} else {
							
							pstr += string.Format("[{0}:{1}]", par.ParameterType, par.Name);
							
						}
						
					}
					
					HelpAttribute comment = null;
					var comments = method.GetCustomAttributes(typeof(HelpAttribute), false);
					if (comments.Length > 0) comment = (HelpAttribute)comments[0];

					if (commandsOnly == true) {

						help.Add(methodName);

					} else {

						help.Add(methodName +
						         (string.IsNullOrEmpty(pstr) == true ? string.Empty : string.Format("\t{0}", pstr)) +
						         (comment == null || string.IsNullOrEmpty(comment.comment) == true ? string.Empty : (string.Format("\t<color=grey>// {0}</color>", comment.comment))));

					}

				}

			}

			var result = help.ToArray();
			ListPool<string>.Release(help);
			ListPool<string>.Release(helpMethods);
			ListPool<MethodInfo>.Release(methods);
			return result;

		}

		public static void ShowHelp(SubModuleBase module) {

			ConsoleManager.instance.ShowHelp_INTERNAL(module);

		}

		public void ShowHelp_INTERNAL(SubModuleBase module) {
			
			this.screen.AddLine("----------------------------------------------");
			
			var moduleName = module.GetType().Name;
			this.screen.AddLine(string.Format("<color=#c2c>Module `{0}` commands:</color>", moduleName));

			var help = ConsoleManager.GetHelp(module, string.Empty);

			foreach (var line in help) {
				
				this.screen.AddLine(string.Format("\t{0}", line));
				
			}

			this.screen.AddLine("----------------------------------------------");

		}

		public static void GetModuleMethods(SubModuleBase module, List<MethodInfo> list) {
			
			var methods = module.GetType().GetMethods();
			foreach (var method in methods) {
				
				var executable = method.GetCustomAttributes(typeof(ExecutableAttribute), false);
				if (executable == null || executable.Length == 0) continue;

				list.Add(method);

			}

		}

		private bool Call(string cmd, SubModuleBase module, string[] args) {
			
			var first = string.Empty;
			if (args.Length > 0) first = args[0];

			if (module.HasCommandRewrite(args) == true) {

				var result = module.OnCommandRewrite(args);
				if (string.IsNullOrEmpty(result) == false) {
					
					this.screen.AddLine(string.Format("<color=red>{0}</color>", result));
					return false;
					
				} else {

					return true;

				}

			}

			var i = 0;
			var methods = ListPool<MethodInfo>.Get();
			ConsoleManager.GetModuleMethods(module, methods);
			foreach (var method in methods) {

				if (method.Name.ToLower() == first.ToLower().Trim() && method.GetParameters().Length == args.Length - 1) {

					var pars = new List<object>();

					i = 1;
					foreach (var par in method.GetParameters()) {

						if (par.ParameterType == typeof(bool)) {
							
							pars.Add((object)bool.Parse(args[i]));

                        } else if (par.ParameterType == typeof(int)) {

                            pars.Add((object)int.Parse(args[i]));

                        } else if (par.ParameterType == typeof(long)) {

                            pars.Add((object)long.Parse(args[i]));

                        } else if (par.ParameterType == typeof(float)) {
							
							pars.Add((object)float.Parse(args[i]));

						} else if (par.ParameterType.IsEnum == true) {

							var names = System.Enum.GetNames(par.ParameterType);
							for (int j = 0; j < names.Length; ++j) names[j] = names[j].ToLower();
							var index = System.Array.IndexOf(names, args[i]);

							pars.Add(System.Enum.GetValues(par.ParameterType).GetValue(index));

						} else {
							
							pars.Add((object)args[i]);
							
						}

						++i;

					}

					var result = (string)method.Invoke(module, pars.ToArray());
					if (string.IsNullOrEmpty(result) == false) {

						this.screen.AddLine(string.Format("<color=red>{0}</color>", result));

					} else {

						return true;

					}

					return false;

				}

			}

			ListPool<MethodInfo>.Release(methods);

			this.AddCommandNotFound(cmd);

			return false;

		}

	}

}