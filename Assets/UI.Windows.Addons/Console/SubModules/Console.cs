using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Plugins.Console.SubModules {

	public class Console : SubModuleBase {

		public float alpha {

			set {

				PlayerPrefs.SetFloat("Console.Console.alpha", value);
				var back = this.screen.GetLayoutComponent<DecoratorComponent>();
				back.SetAlpha(value);

			}

			get {

				return PlayerPrefs.GetFloat("Console.Console.alpha", 0.5f);

			}

		}

		public override void OnStart(ConsoleScreen screen) {

			base.OnStart(screen);

			this.alpha = this.alpha;

		}
		
		public override bool HasCommandGlobalRewrite(string[] args) {
			
			if (args[0] == "version" && args.Length == 1) {
				
				return true;
				
			}
			
			return base.HasCommandGlobalRewrite(args);
			
		}
		
		public override void OnCommandGlobalRewrite(string[] args) {
			
			if (args[0] == "version") {

				this.screen.AddLine(string.Format("Console version: \t{0}", ConsoleManager.VERSION));

			}

		}
		
		[Executable, Help("Hides exception border")]
		public string HideException() {

			this.screen.HideExceptionBorder();

			return string.Empty;
			
		}

		[Executable, Help("Prints current alpha value")]
		public string Alpha() {
			
			this.screen.AddLine(this.alpha.ToString());
			
			return string.Empty;
			
		}

		[Executable, Help("Set up background alpha 0..1")]
		public string Alpha(float value) {
			
			this.alpha = value;
			
			return string.Empty;
			
		}

		[Executable, Help("Clear console")]
		public string Clear() {
			
			this.screen.ClearQueue();
			
			return string.Empty;
			
		}
		
		[Executable, Help("Setup auto-clear time (default -1)")]
		public string ClearTime(float time) {
			
			this.screen.clearTime = time;
			
			return string.Empty;
			
		}

	}

}