using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using ME;

namespace UnityEngine.UI.Windows.Plugins.Console.SubModules {

	public class Log : SubModuleBase {

		public enum LogLevel : byte {

			Low = 0,
			Normal = 1,
			High = 2,

		};

		private List<string> logs = new List<string>();
		private bool enabled {

			get {

				return PlayerPrefs.GetInt("Console.Log.enabled", 1) == 1;

			}
			set {
				
				PlayerPrefs.SetInt("Console.Log.enabled", value == true ? 1 : 0);

			}

		}

		private LogLevel logLevel {
			
			get {

				LogLevel level = LogLevel.High;
				var intValue = PlayerPrefs.GetInt("Console.Log.logLevel", 2);
				switch (intValue) {
					
					case 0:
						level = LogLevel.Low;
						break;
						
					case 1:
						level = LogLevel.Normal;
						break;
						
					case 2:
						level = LogLevel.High;
						break;

				}

				return level;
				
			}
			set {
				
				PlayerPrefs.SetInt("Console.Log.logLevel", (int)value);
				
			}
			
		}

		public override void OnStart(ConsoleScreen screen) {

			base.OnStart(screen);

			Application.logMessageReceived += this.OnLog;

		}

		public void OnLog(string condition, string stackTrace, LogType type) {

			if (this.enabled == false) return;

			if (this.logLevel == LogLevel.Low) {
				
				if (type == LogType.Log) return;
				if (type == LogType.Warning) return;

			} else if (this.logLevel == LogLevel.Normal) {
				
				if (type == LogType.Log) return;

			} else if (this.logLevel == LogLevel.High) {
			}

			this.logs.Add(condition);

			var color = "white";
			switch (type) {

				case LogType.Log:
					break;

				case LogType.Error:
				case LogType.Exception:
					color = "red";
					break;

				case LogType.Assert:
				case LogType.Warning:
					color = "yellow";
					break;

			}

			this.screen.AddLine("<color=" + color + ">" + condition + "</color>");

		}
		
		[Executable, Help("Prints current log state")]
		public string Status() {

			this.screen.AddLine(this.enabled == true ? "Status: " + this.logLevel.ToString() : "Disabled");

			return string.Empty;
			
		}
		
		[Executable, Help("Enable log with log level: Low, Normal, High")]
		public string Enable(LogLevel logLevel) {
			
			this.enabled = true;
			this.logLevel = logLevel;

			return string.Empty;
			
		}

		[Executable, Help("Enable log")]
		public string Enable() {
			
			this.enabled = true;
			
			return string.Empty;
			
		}

		[Executable, Help("Disable log")]
		public string Disable() {
			
			this.enabled = false;
			
			return string.Empty;
			
		}

	}

}