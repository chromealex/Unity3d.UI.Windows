using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Diagnostics;

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

			private Stopwatch stopwatch = null;

			private string GetTimestamp() {

				if (this.stopwatch == null) {

					this.stopwatch = new Stopwatch();
					this.stopwatch.Start();

				}

				return string.Format("{0}ms", this.stopwatch.ElapsedMilliseconds);

			}

			#region String
			public void Log(string @object, string data) {
				
				if (this.enabled == false) return;

				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log(string.Format("{2} [ <b>{0}</b> ] {1}", @object, data, this.GetTimestamp()));
				
			}

			public void Warning(string @object, string data) {

				if (this.enabled == false) return;

				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogWarning(string.Format("{2} [ <b>{0}</b> ] {1}", @object, data, this.GetTimestamp()));

			}

			public void Error(string @object, string data) {

				if (this.enabled == false) return;

				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogError(string.Format("{2} [ <b>{0}</b> ] {1}", @object, data, this.GetTimestamp()));

			}
			#endregion

			#region WindowObject
			public void Log(IWindowObject @object, string data) {
				
				if (this.enabled == false) return;
				
				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log(string.Format("{2} [ <b>{0}</b> ] {1}", (@object as MonoBehaviour).name, data, this.GetTimestamp()), @object as MonoBehaviour);
				
			}

			public void Warning(IWindowObject @object, string data) {

				if (this.enabled == false) return;

				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogWarning(string.Format("{2} [ <b>{0}</b> ] {1}", (@object as MonoBehaviour).name, data, this.GetTimestamp()), @object as MonoBehaviour);

			}

			public void Error(IWindowObject @object, string data) {

				if (this.enabled == false) return;

				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogError(string.Format("{2} [ <b>{0}</b> ] {1}", (@object as MonoBehaviour).name, data, this.GetTimestamp()), @object as MonoBehaviour);

			}
			#endregion
			
			#region Service
			public void Log(IServiceBase @object, string data) {
				
				if (this.enabled == false) return;
				
				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log(string.Format("{2} [ <b>{0}</b> ] {1}", @object.GetServiceName(), data, this.GetTimestamp()), @object as MonoBehaviour);
				
			}

			public void Warning(IServiceBase @object, string data) {

				if (this.enabled == false) return;

				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogWarning(string.Format("{2} [ <b>{0}</b> ] {1}", @object.GetServiceName(), data, this.GetTimestamp()), @object as MonoBehaviour);

			}

			public void Error(IServiceBase @object, string data) {

				if (this.enabled == false) return;

				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogWarning(string.Format("{2} [ <b>{0}</b> ] {1}", @object.GetServiceName(), data, this.GetTimestamp()), @object as MonoBehaviour);

			}
			#endregion

		};
		
		[Header("UI.Windows Logs")]
		[BitMask(typeof(ActiveType))]
		public ActiveType textLogs;
		[BitMask(typeof(ActiveType))]
		public ActiveType componentsLogs;

		[Header("Unity Logs")]
		public LogType filterLogType;
		public bool loggerEnabled;

		private static WindowSystemLogger _instance;
		private static WindowSystemLogger instance {

			get {
				
				#if UNITY_EDITOR
				if (Application.isPlaying == false) {

					if (WindowSystemLogger._instance == null) {

						WindowSystemLogger._instance = Object.FindObjectOfType<WindowSystemLogger>();
						if (WindowSystemLogger._instance == null) {

							var logger = GameObject.Find("Logger");
							if (logger != null) WindowSystemLogger._instance = logger.GetComponent<WindowSystemLogger>();

						}

						if (WindowSystemLogger._instance != null) WindowSystemLogger._instance.Awake();

					}

				}
				#endif

				return WindowSystemLogger._instance;

			}

			set {

				WindowSystemLogger._instance = value;

			}

		}

	    protected virtual void OnDestory() {

	        WindowSystemLogger.instance = null;

	    }

		private Logger logger;

		public void Awake() {

			this.logger = new Logger() {
				enabled = 
					(Application.isEditor == true && (this.textLogs & ActiveType.InEditor) != 0) ||
					(Application.isEditor == false && (this.textLogs & ActiveType.InBuild) != 0)
			};

			if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.unityLogger.filterLogType = this.filterLogType;
			if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.unityLogger.logEnabled = this.loggerEnabled;

			WindowSystemLogger.instance = this;

		}

		public static bool IsLogEnabled() {

			return Debug.unityLogger.logEnabled;

		}

		public static void SetState(bool state, LogType filterLogType) {

			if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.unityLogger.filterLogType = filterLogType;
			if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.unityLogger.logEnabled = state;

		}

		public static bool IsActiveComponents() {

			if (WindowSystemLogger.instance == null) return false;

			var active = WindowSystemLogger.instance.componentsLogs;
			return 
				(Application.isEditor == true && (active & ActiveType.InEditor) != 0) ||
				(Application.isEditor == false && (active & ActiveType.InBuild) != 0);

		}
		
		#region String
		public static void Log(string @object, string data) {
			
			WindowSystemLogger.instance.logger.Log(@object, data);
			
		}

		public static void Warning(string @object, string data) {

			WindowSystemLogger.instance.logger.Warning(@object, data);

		}

		public static void Error(string @object, string data) {

			WindowSystemLogger.instance.logger.Error(@object, data);

		}
		#endregion

		#region WindowObject
		public static void Log(IWindowObject @object, string data) {
			
			WindowSystemLogger.instance.logger.Log(@object, data);
			
		}

		public static void Warning(IWindowObject @object, string data) {

			WindowSystemLogger.instance.logger.Warning(@object, data);

		}

		public static void Error(IWindowObject @object, string data) {

			WindowSystemLogger.instance.logger.Error(@object, data);

		}
		#endregion

		#region Service
		public static void Log(IServiceBase @object, string data) {
			
			WindowSystemLogger.instance.logger.Log(@object, data);
			
		}

		public static void Warning(IServiceBase @object, string data) {

			WindowSystemLogger.instance.logger.Warning(@object, data);

		}

		public static void Error(IServiceBase @object, string data) {

			WindowSystemLogger.instance.logger.Error(@object, data);

		}
		#endregion

	}

}