using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Plugins.Localization;

namespace UnityEngine.UI.Windows.Plugins.Console.SubModules {

	public class UIWindowsProductInfo : SubModuleBase {
		
		[Executable, Help("Prints current version id")]
		public void Version() {
			
			this.screen.AddLine(string.Format("UI.Windows version: \t{0}", UnityEngine.UI.Windows.VersionInfo.BUNDLE_VERSION));

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