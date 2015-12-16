using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Localization {

	public class LocalizationSystem : ServiceManager<LocalizationSystem> {

		public const UnityEngine.SystemLanguage DEFAULT_EDITOR_LANGUAGE = UnityEngine.SystemLanguage.Russian;

		private static Dictionary<UnityEngine.SystemLanguage, string[]> valuesByLanguage = new Dictionary<UnityEngine.SystemLanguage, string[]>();
		private static string[] keys = new string[0];
		private static UnityEngine.SystemLanguage[] languages = new UnityEngine.SystemLanguage[0];

		private static UnityEngine.SystemLanguage defaultLanguage;
		private static UnityEngine.SystemLanguage currentLanguage;

		public override string GetServiceName() {

			return "Localization";

		}

		public override UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions GetAuthPermission() {

			return UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions.None;

		}

		public static UnityEngine.SystemLanguage[] GetLanguagesList() {

			return LocalizationSystem.languages;

		}

		public static string[] GetLanguagesListString() {

			var list = LocalizationSystem.GetLanguagesList();
			var output = new string[list.Length];
			for (int i = 0; i < list.Length; ++i) {

				output[i] = list[i].ToString();

			}

			return output;

		}

		public static void SetLanguageIndex(int index) {

			LocalizationSystem.currentLanguage = LocalizationSystem.GetLanguagesList()[index];

			WindowSystem.ForEachWindow((w) => {

				w.OnLocalizationChanged();

			});

		}

		public static string[] GetKeys() {

			return LocalizationSystem.keys;

		}

		public static string Get(string key) {

			return LocalizationSystem.Get(key, LocalizationSystem.GetCurrentLanguage());

		}

		public static string Get(string key, UnityEngine.SystemLanguage language) {

			string[] values;
			if (LocalizationSystem.valuesByLanguage.TryGetValue(language, out values) == true) {

				var keys = LocalizationSystem.GetKeys();
				if (keys != null && key != null) {
					
					var index = System.Array.IndexOf(keys, key.ToLower());
					if (index >= 0) {
						
						return values[index];

					}

				}

			}

			return string.Empty;

		}

		public static Sprite[] GetNewSprites() {

			var languagesCount = System.Enum.GetValues(typeof(UnityEngine.SystemLanguage)).Length;
			return new Sprite[languagesCount];

		}

		public static UnityEngine.SystemLanguage GetCurrentLanguage() {

			return LocalizationSystem.currentLanguage;

		}

		public static int GetDefaultLanguageIndex() {

			return System.Array.IndexOf(LocalizationSystem.languages, LocalizationSystem.defaultLanguage);

		}

		public static UnityEngine.SystemLanguage GetDefaultLanguage() {

			return LocalizationSystem.defaultLanguage;

		}

		public static Sprite GetSprite(LocalizationKey key, params object[] parameters) {

			var spriteName = LocalizationSystem.Get(key, parameters).Trim();
			var sprite = Resources.Load<Sprite>(spriteName);

			return sprite as Sprite;

		}

		public static string Get(LocalizationKey key, UnityEngine.SystemLanguage language) {

			return LocalizationSystem.Get(key.key, language);

		}

		public static string Get(LocalizationKey key, params object[] parameters) {

			if (parameters.Length == key.parameters) {

				var value = LocalizationSystem.Get(key.key, LocalizationSystem.GetCurrentLanguage());
				return string.Format(value, parameters);

			} else {

				Debug.LogWarningFormat("[ Localization ] Wrong parameters length in key `{0}`", key.key);

			}

			return key.key;

		}

		public static bool ContainsKey(string key) {

			return LocalizationSystem.GetKeys().Contains(key.ToLower());

		}

		public static int GetParametersCount(string key) {

			var value = LocalizationSystem.Get(key, LocalizationSystem.GetCurrentLanguage());
			var matches = Regex.Matches(value, "{.*?}", RegexOptions.ExplicitCapture);
			return matches.Count;

		}

		public static string GetCachePath() {
			
			return Application.persistentDataPath + "/Localization.dat";

		}

		#if UNITY_EDITOR
		private static bool cacheLoaded = false;
		#endif
		public static void TryToLoadCache() {

			#if UNITY_EDITOR
			if (LocalizationSystem.cacheLoaded == true) return;
			if (Application.isPlaying == false) {

				LocalizationSystem.cacheLoaded = true;

				var path = LocalizationSystem.GetCachePath();
				if (System.IO.File.Exists(path) == false) return;

				var text = System.IO.File.ReadAllText(path);
				LocalizationSystem.TryToSaveCSV(text, loadCacheOnFail: true);

			} else {
			#endif

				var path = LocalizationSystem.GetCachePath();
				if (System.IO.File.Exists(path) == false) return;

				var text = System.IO.File.ReadAllText(path);
				LocalizationSystem.TryToSaveCSV(text, loadCacheOnFail: true);

			#if UNITY_EDITOR
			}
			#endif

		}

		public static void TryToSaveCSV(string data, bool loadCacheOnFail = true) {

			try {

				var parsed = LocalizationParser.ReadCSV(data);

				var defaultLanguage = parsed[0][0];
				LocalizationSystem.defaultLanguage = (UnityEngine.SystemLanguage)System.Enum.Parse(typeof(UnityEngine.SystemLanguage), defaultLanguage);

				var keysCount = 0;

				#region KEYS
				var keys = new List<string>();
				for (int i = 0; i < parsed.Count; ++i) {

					if (i == 0) continue;

					var row = parsed[i];
					if (string.IsNullOrEmpty(row[0].Trim()) == true) continue;

					keys.Add(row[0].ToLower());

					++keysCount;

				}

				LocalizationSystem.keys = keys.ToArray();
				#endregion

				var langCount = 0;

				#region LANGUAGES
				var languages = new List<UnityEngine.SystemLanguage>();
				for (int i = 0; i < parsed[0].Length; ++i) {

					if (i == 0) continue;

					var col = parsed[0][i];
					languages.Add((UnityEngine.SystemLanguage)System.Enum.Parse(typeof(UnityEngine.SystemLanguage), col));

					++langCount;

				}

				LocalizationSystem.languages = languages.ToArray();
				#endregion

				#region VALUES
				var values = new Dictionary<UnityEngine.SystemLanguage, string[]>();
				for (int j = 0; j < languages.Count; ++j) {

					var lang = languages[j];

					var output = new List<string>();
					for (int i = 0; i < parsed.Count; ++i) {

						if (i == 0) continue;

						var col = parsed[i][j + 1];
						if (string.IsNullOrEmpty(col.Trim()) == true) continue;

						output.Add(col);

					}

					values.Add(lang, output.ToArray());

				}

				LocalizationSystem.valuesByLanguage = values;
				#endregion

				var path = LocalizationSystem.GetCachePath();
				System.IO.File.WriteAllText(path, data);

				#if !UNITY_EDITOR
				if (LocalizationSystem.instance.logEnabled == true) {
				#endif
					
					Debug.LogFormat("[ Localization ] Loaded. Keys: {0}, Languages: {1}", keysCount, langCount);

				#if !UNITY_EDITOR
				}
				#endif

			} catch(System.Exception ex) {

				// Nothing to do: failed to parse
				Debug.LogError(string.Format("[ Localization ] Parser error: {0}", ex.Message));

				if (loadCacheOnFail == true) {

					LocalizationSystem.TryToLoadCache();

				}

			}

		}

	}

}