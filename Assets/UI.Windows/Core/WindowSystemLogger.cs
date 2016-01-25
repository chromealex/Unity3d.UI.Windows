using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public class WindowSystemLogger : MonoBehaviour {

		public enum ActiveType : byte {

			None = 0x0,

			InEditor = 0x1,
			InBuild = 0x2,

		};

		private class Logger {

			public bool enabled;

			public void Log(WindowObject @object, string data) {

				if (this.enabled == false) return;

				Debug.Log(string.Format("[ {0} ] {1}",  @object.name, data),  @object);

			}

		};

		[BitMask(typeof(ActiveType))]
		public ActiveType active;

		private static WindowSystemLogger instance;
		private Logger logger;

		public void Awake() {

			this.logger = new Logger() {
				enabled = 
					(Application.isEditor == true && (this.active & ActiveType.InEditor) != 0) ||
					(Application.isEditor == false && (this.active & ActiveType.InBuild) != 0)
			};

			WindowSystemLogger.instance = this;

		}

		public static void Log(WindowObject @object, string data) {

			WindowSystemLogger.instance.logger.Log(@object, data);

		}

	}

}