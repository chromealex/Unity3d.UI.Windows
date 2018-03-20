using ME.FS.Core;
using System.IO;

namespace UnityEngine.UI.Windows.Plugins.Resources {

	public static class Utilities {

		public static ResourceAuto GetResource(string groupName, string filename, string resourcesPathMask = "{0}", string webPathMask = null, string webPath = null, bool readable = true) {

			//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("Load: " + groupName + " :: " + filename + " :: " + resourcesPathMask + " :: " + webPathMask);

			ResourceAuto resource = null;

			// Look up built-in storage
			if (resource == null) {

				var path = string.Format("{0}/{1}", groupName, string.Format(resourcesPathMask, filename));
				//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("RES: " + path);
				if (UnityEngine.Resources.Load(path) != null) {

					resource = ResourceAuto.CreateResourceRequest(path);

				}

			}

#if !UNITY_SWITCH
			// Look up cache storage
			if (resource == null) {

				var path = Utilities.GetCachePath(groupName, filename);
				if (File.Exists(path) == true) {

					//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("CCH: " + path);
					resource = ResourceAuto.CreateWebRequest(path, readable: readable);

				}

			}
#endif

			// Create web request
			if (resource == null) {

				var path = (string.IsNullOrEmpty(webPath) == false ? webPath : string.Format(webPathMask, filename));
				//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("WEB: " + path);
				resource = ResourceAuto.CreateWebRequest(path, readable: readable);
				
			}

			return resource;

		}

		public static void SaveResource(string groupName, string filename, byte[] bytes) {

#if !UNITY_SWITCH
			var path = Utilities.GetCachePath(groupName, filename);
			File.WriteAllBytes(path, bytes);
#endif

		}

		private static string GetCachePath(string groupName, string filename) {

			var cachePath = FileSystem.instance.GetCachePath();

			var dir = string.Format("{0}/{1}", cachePath, groupName);
			var path = string.Format("{0}/{1}.cache", dir, filename);

			try {
				
				if (Directory.Exists(dir) == false) {

					Directory.CreateDirectory(dir);

				}

			} catch (System.Exception) {



			}

			return path;

		}

	}
}

