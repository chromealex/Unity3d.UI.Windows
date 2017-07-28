//#if UNITY_TVOS
//#define STORAGE_NOT_SUPPORTED
//#endif
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Utilities;

namespace UnityEngine.UI.Windows.Plugins.GameData {

	public class GameDataSystem : ServiceManager<GameDataSystem> {

		public override string GetServiceName() {

			return GameDataSystem.GetName();

		}

		public static string GetName() {

			return "GameData";

		}

		public static Version DEFAULT_EDITOR_VERSION {

			get {
				
				return GameDataSystem.defaultVersion;

			}

		}
		
		private static Dictionary<VersionCrc, float[]> valuesByVersion = new Dictionary<VersionCrc, float[]>();
		private static string[] keys = new string[0];
		private static Version[] versions = new Version[0];

		private static Version defaultVersion;
		private static Version currentVersion;
		private static int currentVersionNumber;

		private static bool isReady = false;

		public override UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions GetAuthPermission() {

			return UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions.None;

		}

		public static bool IsReady() {

			return GameDataSystem.isReady;

		}

		public static int GetCurrentVersionId() {

			return GameDataSystem.currentVersionNumber;

		}

		public static Version[] GetVersionsList() {

			return GameDataSystem.versions;

		}

		public static string[] GetLanguagesListString() {

			var list = GameDataSystem.GetVersionsList();
			var output = new string[list.Length];
			for (int i = 0; i < list.Length; ++i) {

				output[i] = list[i].ToString();

			}

			return output;

		}

		public static void SetVersion(Version version) {

			var list = GameDataSystem.GetVersionsList();
			for (int i = 0; i < list.Length; ++i) {

				if (list[i] == version) {

					GameDataSystem.SetVersionIndex(i);
					break;

				}

			}

		}

		public static void SetVersionIndex(int index) {

			GameDataSystem.currentVersion = GameDataSystem.GetVersionsList()[index];

			WindowSystem.ForEachWindow((w) => {

				w.OnVersionChanged();

			}, includeInactive: false);

		}

		public static string[] GetKeys() {

			return GameDataSystem.keys;

		}

		public static bool HasKey(string key) {

			return GameDataSystem.HasKey(key, GameDataSystem.GetCurrentVersion());

		}

		public static bool HasKey(string key, Version version) {

			if (GameDataSystem.IsReady() == true) {

				var crc = new VersionCrc(version);

				float[] values;
				if (GameDataSystem.valuesByVersion.TryGetValue(crc, out values) == true) {

					var keys = GameDataSystem.GetKeys();
					if (keys != null && key != null) {

						var index = System.Array.IndexOf(keys, key.ToLower());
						if (index >= 0) {

							return true;

						}

					}

				}

			}

			return false;

		}

		public static float Get(string key) {

			return GameDataSystem.Get(key, GameDataSystem.GetCurrentVersion());

		}

		public static float Get(string key, Version version) {

			if (GameDataSystem.IsReady() == true) {

				var crc = new VersionCrc(version);

				float[] values;
				if (GameDataSystem.valuesByVersion.TryGetValue(crc, out values) == true) {

					var keys = GameDataSystem.GetKeys();
					if (keys != null && key != null) {
						
						var index = System.Array.IndexOf(keys, key.ToLower());
						if (index >= 0) {
							
							return values[index];

						}

					}

				}

				//Debug.LogWarningFormat("[ GameData ] Key was not found: {0}", key);

			} else {

				if (Application.isPlaying == true) {

					Debug.LogWarningFormat("[ GameData ] System not ready. Do not use `GameData.Get()` method while/before system starting. You can check it's state by `GameData.IsReady()` call. Key: `{0}`.", key);

				}

			}

			return 0f;

		}

		public static Version GetCurrentVersion() {

			return GameDataSystem.currentVersion;

		}

		public static int GetDefaultVersionIndex() {

			return System.Array.IndexOf(GameDataSystem.versions, GameDataSystem.defaultVersion);

		}

		public static Version GetDefaultVersion() {

			return GameDataSystem.defaultVersion;

		}

		public static float Get(GDFloat key, Version version) {

			return GameDataSystem.Get(key.key, version);

		}

		public static float Get(GDFloat key) {

			return GameDataSystem.Get(key.key, GameDataSystem.GetCurrentVersion());

		}
		
		public static int Get(GDInt key, Version version) {
			
			return (int)GameDataSystem.Get(key.key, version);
			
		}
		
		public static int Get(GDInt key) {
			
			return (int)GameDataSystem.Get(key.key, GameDataSystem.GetCurrentVersion());
			
		}
		
		public static bool Get(GDBool key, Version version) {
			
			return GameDataSystem.Get(key.key, version) == 1f;
			
		}
		
		public static bool Get(GDBool key) {
			
			return GameDataSystem.Get(key.key, GameDataSystem.GetCurrentVersion()) == 1f;
			
		}

        public static int Get(GDEnum key, Version version) {

            return (int)GameDataSystem.Get(key.key, version);
            
        }

        public static int Get(GDEnum key) {

            return (int)GameDataSystem.Get(key, GameDataSystem.GetCurrentVersion());

        }

        public static bool ContainsKey(string key) {

			return GameDataSystem.GetKeys().Contains(key.ToLower());

		}

		public static string GetCachePath() {

			#if UNITY_TVOS
			return GameDataSystem.GetCachePath(Application.temporaryCachePath, forceDirectoryCreation: true);
			#else
			return GameDataSystem.GetCachePath(Application.persistentDataPath, forceDirectoryCreation: true);
			#endif

		}

		public static string GetBuiltinCachePath() {

			#if UNITY_EDITOR
			bool forceDirectoryCreation = true;
			#else
			bool forceDirectoryCreation = false;
			#endif
			return GameDataSystem.GetCachePath(Application.streamingAssetsPath, forceDirectoryCreation);

		}

		public static string GetCachePath(string storagePath, bool forceDirectoryCreation) {

			var dir = string.Format("{0}/UI.Windows/Cache/Services", storagePath);
			var path = string.Format("{0}/{1}.uiws", dir, GameDataSystem.GetName());
			#if !STORAGE_NOT_SUPPORTED
			if (forceDirectoryCreation == true && System.IO.Directory.Exists(dir) == false) {

				System.IO.Directory.CreateDirectory(dir);

			}
			#endif

			return path;

		}

		#if UNITY_EDITOR
		private static bool cacheLoaded = false;
		#endif
		public static void TryToLoadCache() {

			#if UNITY_EDITOR
			if (GameDataSystem.cacheLoaded == true) return;
			if (Application.isPlaying == false) {

				GameDataSystem.cacheLoaded = true;

				var path = GameDataSystem.GetCachePath();
				#if STORAGE_NOT_SUPPORTED
				if (PlayerPrefs.HasKey(path) == false) return;
				var text = PlayerPrefs.GetString(path, string.Empty);
				#else
				if (System.IO.File.Exists(path) == false) {
					
					path = GameDataSystem.GetBuiltinCachePath();
					
				}
				var text = System.IO.File.ReadAllText(path);
				#endif
				
				GameDataSystem.TryToSaveCSV(text, loadCacheOnFail: false);

			} else {
			#endif
				#if UNITY_ANDROID
				Coroutines.Run(GameDataSystem.LoadByWWW(GameDataSystem.GetCachePath(), GameDataSystem.GetBuiltinCachePath()));
				#else
				var path = GameDataSystem.GetCachePath();
				#if STORAGE_NOT_SUPPORTED
				if (PlayerPrefs.HasKey(path) == false) return;
				var text = PlayerPrefs.GetString(path, string.Empty);
				#else
				if (System.IO.File.Exists(path) == false) {
					
					path = GameDataSystem.GetBuiltinCachePath();
					
				}
				var text = System.IO.File.ReadAllText(path);
				#endif

				GameDataSystem.TryToSaveCSV(text, loadCacheOnFail: false);
				#endif

			#if UNITY_EDITOR
			}
			#endif

		}

		private static IEnumerator<byte> LoadByWWW(string cachePath, string builtinCachePath) {

			var www = new WWW(cachePath);
			while (www.isDone == false) yield return 0;

			if (string.IsNullOrEmpty(www.error) == false) {

				www.Dispose();
				www = null;

				www = new WWW(builtinCachePath);
				while (www.isDone == false) yield return 0;

			}

			GameDataSystem.TryToSaveCSV(www.text, loadCacheOnFail: false);

		}

		public static void TryToSaveCSV(string data, bool loadCacheOnFail = true) {

			try {

				var parsed = CSVParser.ReadCSV(data);

				if (GameDataSystem.currentVersion == default(Version)) {

					var defaultVersion = parsed[0][0];
					GameDataSystem.currentVersion = new Version(defaultVersion);

					if (GameDataSystem.instance == null || GameDataSystem.instance.logEnabled == true) {
						
						WindowSystemLogger.Warning(GameDataSystem.GetName(), string.Format("Default version is used: {0}", GameDataSystem.currentVersion));

					}

				}
				GameDataSystem.defaultVersion = GameDataSystem.currentVersion;

				var keysCount = 0;

				#region KEYS
				var keys = new List<string>();
				for (int i = 0; i < parsed.Count; ++i) {

					if (i == 0) continue;

					var row = parsed[i];
					var str = row[0].Trim();
					if (string.IsNullOrEmpty(str) == true) {

						//str = string.Empty;
						continue;

					}

					keys.Add(str.ToLower());

					++keysCount;

				}

				GameDataSystem.keys = keys.ToArray();
				#endregion

				var verCount = 0;

				#region VERSIONS
				var versions = new List<Version>();
				for (int i = 0; i < parsed[0].Length; ++i) {

					if (i == 0) continue;

					var col = parsed[0][i];
					var version = new Version(col);
					versions.Add(version);
					
					if (version == GameDataSystem.defaultVersion) {
						
						GameDataSystem.currentVersionNumber = int.Parse(parsed[1][i]);
						
					}

					++verCount;

				}

				GameDataSystem.versions = versions.ToArray();
				#endregion

				#region VALUES
				var values = new Dictionary<VersionCrc, float[]>();
				for (int j = 0; j < versions.Count; ++j) {

					var version = versions[j];
					var crc = new VersionCrc(version);

					var output = new List<float>();
					for (int i = 0; i < parsed.Count; ++i) {

						if (i == 0) continue;

						var value = parsed[i][j + 1];
						if (string.IsNullOrEmpty(value.Trim()) == true) {

							//value = "0";
							continue;

						}

						value = value.Replace(",", ".").Replace(" ", string.Empty);

						var col = float.Parse(value);
						output.Add(col);

					}

					if (values.ContainsKey(crc) == false) values.Add(crc, output.ToArray());

				}

				GameDataSystem.valuesByVersion = values;
				#endregion

				var path = GameDataSystem.GetCachePath();
				#if STORAGE_NOT_SUPPORTED
				PlayerPrefs.SetString(path, data);
				#else
				System.IO.File.WriteAllText(path, data);
				#endif

				#if UNITY_EDITOR
				path = GameDataSystem.GetBuiltinCachePath();
				System.IO.File.WriteAllText(path, data);
				#endif

				GameDataSystem.currentVersion = GameDataSystem.defaultVersion;

				if (GameDataSystem.instance == null || GameDataSystem.instance.logEnabled == true) {
					
					WindowSystemLogger.Log(GameDataSystem.GetName(), string.Format("Loaded version {3}. Cache saved to: {0}, Keys: {1}, Versions: {2}, Version: {4}", path, keysCount, verCount, GameDataSystem.GetCurrentVersionId(), GameDataSystem.currentVersion));

				}

				GameDataSystem.isReady = true;

			} catch(System.Exception ex) {
				
				if (GameDataSystem.instance == null || GameDataSystem.instance.logEnabled == true) {
					
					// Nothing to do: failed to parse
					WindowSystemLogger.Error(GameDataSystem.GetName(), string.Format("Parser error: {0}\n{1}", ex.Message, ex.StackTrace));

				}

				if (loadCacheOnFail == true) {

					GameDataSystem.TryToLoadCache();

				}

			}

		}

	}

}