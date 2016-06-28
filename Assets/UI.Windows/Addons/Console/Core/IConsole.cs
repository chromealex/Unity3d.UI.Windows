using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Console {

	public enum AccessGroup : byte {
		Public = 0,
		Private = 1,
	};

	public class AccessLevelAttribute : System.Attribute {

		public AccessGroup group;

		public AccessLevelAttribute(AccessGroup group) {

			this.group = group;

		}

	}

	public class AliasAttribute : System.Attribute {

		public string name;

		public AliasAttribute(string name) {

			this.name = name;

		}

	}

	public class ExecutableAttribute : System.Attribute {
	}

	public class HelpAttribute : System.Attribute {

		public string comment;

		public HelpAttribute(string comment) {

			this.comment = comment;

		}

	}

	public class SubModuleBase {

		protected ConsoleScreen screen;

		public string GetName() {

			return this.GetType().Name.ToLower();

		}

		public string[] GetAliases() {

			var output = new List<string>();

			var aliases = this.GetType().GetCustomAttributes(typeof(AliasAttribute), inherit: false);
			foreach (var alias in aliases) {

				var name = (alias as AliasAttribute).name.ToLower();
				output.Add(name);

			}

			return output.ToArray();

		}

		public string[] GetNames() {

			var output = new List<string>();
			output.Add(this.GetName());
			output.AddRange(this.GetAliases());
			return output.ToArray();

		}

		public virtual void OnStart(ConsoleScreen screen) {

			this.screen = screen;

		}

		public virtual void OnStartLate() {

		}

		public virtual bool HasCommandGlobalRewrite(string[] args) {

			return false;

		}

		public virtual void OnCommandGlobalRewrite(string[] args) {

		}

		public virtual bool HasCommandRewrite(string[] args) {

			return false;

		}

		public virtual string OnCommandRewrite(string[] args) {

			return string.Empty;

		}

		public virtual void GetParamAutoComplete(string text, string[] args, int paramIndex, out List<string> output, out List<string> outputConcat) {

			output = new List<string>();
			outputConcat = new List<string>();

			var first = string.Empty;
			if (args.Length > 0) first = args[0];

			output.AddRange(ConsoleManager.GetHelp(this, first, distinct: false));
			outputConcat.AddRange(ConsoleManager.GetHelp(this, first, distinct: true, commandsOnly: true));

		}

		public virtual void HasAutoComplete(string text, string[] args, out List<string> output, out List<string> outputConcat) {

			output = new List<string>();
			outputConcat = new List<string>();

			var first = string.Empty;
			if (args.Length > 0) first = args[0];

			output.AddRange(ConsoleManager.GetHelp(this, first, distinct: false));
			outputConcat.AddRange(ConsoleManager.GetHelp(this, first, distinct: true, commandsOnly: true));

		}

		[Executable, Help("Shows module usage help")]
		public void Help() {

			ConsoleManager.ShowHelp(this);

		}

	}

}