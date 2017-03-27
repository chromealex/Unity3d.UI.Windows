using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Plugins.GameData;

namespace UnityEngine.UI.Windows.Plugins.Console.SubModules {
	
	[AccessLevel(AccessGroup.Private)]
	public class UIWindowsGameData : SubModuleBase {
		
		[Executable, Help("Prints current version id")]
		public void Version() {

			this.screen.AddLine(string.Format("UI.Windows GameData version: \t{0}", GameDataSystem.GetCurrentVersionId()));

		}
		
		[Executable, Help("Pull GameData")]
		public void Pull() {
			
			this.screen.AddLine("Pulling...");
			GameDataSystem.InitializeAsync(() => {

				this.screen.AddLine("Pull completed.");
				this.Version();

			});

		}

		public override bool HasCommandGlobalRewrite(string[] args) {
			
			if (args[0] == "version" && args.Length == 1) {
				
				return true;
				
			}
			
			return base.HasCommandGlobalRewrite(args);
			
		}
		
		public override void OnCommandGlobalRewrite(string[] args) {
			
			if (args[0] == "version") {

				this.Version();
				
			}
			
		}

	}

}