using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Plugins.Console.SubModules {
	
	[Alias("bp")]
	public class BindPlay : SubModuleBase {

		public override bool HasCommandRewrite(string[] args) {

			if (args.Length == 1) {
				
				var play = args[0];
				return ConsoleManager.GetInstance().bind.Exists(play);

			}

			return false;

		}

		public override string OnCommandRewrite(string[] args) {

			if (args.Length == 1) {

				var play = args[0];
				return ConsoleManager.GetInstance().bind.Play(play);

			}

			return string.Empty;

		}

	}

}