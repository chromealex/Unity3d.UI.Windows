using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using ME;

namespace UnityEngine.UI.Windows.Plugins.Console.SubModules {
	
	[Alias("t"), AccessLevel(AccessGroup.Private)]
	public class Time : SubModuleBase {

		[Executable, Help("Set current game time")]
		public string Set(float time) {
			
			UnityEngine.Time.timeScale = time;

			return string.Empty;
			
		}
		
		[Executable, Help("Prints current game time")]
		public string Get() {
			
			this.screen.AddLine(string.Format("Current: {0}", UnityEngine.Time.timeScale.ToString()));

			return string.Empty;

		}

	}

}