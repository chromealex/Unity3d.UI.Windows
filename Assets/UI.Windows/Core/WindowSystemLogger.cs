using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public class WindowSystemLogger : MonoBehaviour {

		public enum ActiveType : byte {

			None = 0x0,

			InEditor = 0x1,
			InBuild = 0x2,

		};

		[System.Serializable]
		public class Logger {

			public bool enabled;

			public void Log(WindowObject @object, string data) {

				if (this.enabled == false) return;

				Debug.Log(string.Format("[ {0} ] {1}", @object.name, data), @object);

			}

			public void Warning(WindowObject @object, string data) {
				
				if (this.enabled == false) return;
				
				Debug.LogWarning(string.Format("[ {0} ] {1}", @object.name, data), @object);
				
			}

		};
		
		[BitMask(typeof(ActiveType))]
		public ActiveType textLogs;
		
		[BitMask(typeof(ActiveType))]
		public ActiveType componentsLogs;

		private static WindowSystemLogger instance;
		private Logger logger;

		public void Awake() {

			this.logger = new Logger() {
				enabled = 
					(Application.isEditor == true && (this.textLogs & ActiveType.InEditor) != 0) ||
					(Application.isEditor == false && (this.textLogs & ActiveType.InBuild) != 0)
			};

			WindowSystemLogger.instance = this;

		}

		public static bool IsActiveComponents() {

			if (WindowSystemLogger.instance == null) return false;

			var active = WindowSystemLogger.instance.componentsLogs;
			return 
				(Application.isEditor == true && (active & ActiveType.InEditor) != 0) ||
				(Application.isEditor == false && (active & ActiveType.InBuild) != 0);

		}

		public static void Log(WindowObject @object, string data) {
			
			WindowSystemLogger.instance.logger.Log(@object, data);
			
		}
		
		public static void Warning(WindowObject @object, string data) {
			
			WindowSystemLogger.instance.logger.Warning(@object, data);
			
		}

	}

}