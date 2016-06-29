using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Utilities;

namespace UnityEngine.UI.Windows.Plugins.Localization {

	public class LocalizationSystem : ServiceManager<LocalizationSystem> {
		
		public override string GetServiceName() {
			
			return LocalizationSystem.GetName();
			
		}

		public static string GetName() {

			return "Localization";

		}

		public const UnityEngine.SystemLanguage DEFAULT_EDITOR_LANGUAGE = UnityEngine.SystemLanguage.Russian;

		private static Dictionary<UnityEngine.SystemLanguage, string[]> valuesByLanguage = new Dictionary<UnityEngine.SystemLanguage, string[]>();
		private static string[] keys = new string[0];
		private static UnityEngine.SystemLanguage[] languages = new UnityEngine.SystemLanguage[0];

		private static UnityEngine.SystemLanguage defaultLanguage;
		private static UnityEngine.SystemLanguage currentLanguage;
		private static int currentVersionNumber;

		private static bool isReady = false;

		/// <summary>
		/// Gets the declension index by number.
		/// String array must be like that:
		/// 	Nominativ
		/// 	Plural
		/// 	Genetiv [Optional]
		/// </summary>
		/// <returns>The declension index by number.</returns>
		/// <param name="number">Number.</param>
		/// <param name="language">Language.</param>
		public static int GetDeclensionIndexByNumber(int number, UnityEngine.SystemLanguage language) {

			if (number <= int.MinValue || number >= int.MaxValue) {

				number = 0;

			}

			var result = 0;

			switch (language) {

				case SystemLanguage.Russian:

					number = Mathf.Abs(number);

					if (number % 10 == 1 && number % 100 != 11) {

						result = 0;

					} else if (number % 10 >= 2 && number % 10 <= 4 && (number % 100 < 10 || number % 100 >= 20)) {

						result = 2;

					} else {

						result = 1;

					}

					break;
					
				default:
					
					if (number == 1) {
						
						// single
						result = 0;
						
					} else {
						
						// multi
						result = 1;
						
					}
					
					break;

			}

			return result;

		}
		
		public static string FormatWithDeclension(string value, params object[] parameters) {

			return LocalizationSystem.FormatWithDeclension(value: value, returnWithNumber: true, parameters: parameters);

		}

		public static string FormatWithDeclension(string value, bool returnWithNumber, params object[] parameters) {

			var result = Regex.Replace(value, @"({\d+, .*?})", new MatchEvaluator((Match match) => {

				var val = match.Value.Trim('{').Trim('}');

				var splitted = val.Split(',');
				var num = int.Parse(splitted[0]);
				var word = string.Empty;
				var parValue = (int)parameters[num];

				var index = 0;
				if (splitted.Length == 3) {
					
					// Standart variant (English)
					index = LocalizationSystem.GetDeclensionIndexByNumber(parValue, SystemLanguage.English);

				} else if (splitted.Length == 4) {

					// Super variant (Russian)
					index = LocalizationSystem.GetDeclensionIndexByNumber(parValue, SystemLanguage.Russian);
					
				}

				word = splitted[index + 1];

				if (returnWithNumber == false) {

					return word;

				} else {

					return string.Format("{0}{1}", "{" + num + "}", word);

				}

			}));

			return string.Format(result, parameters);

		}

		public override UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions GetAuthPermission() {

			return UnityEngine.UI.Windows.Plugins.Flow.AuthKeyPermissions.None;

		}

		public static bool IsReady() {

			return LocalizationSystem.isReady;

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

			if (LocalizationSystem.IsReady() == true) {

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

			} else {

				if (Application.isPlaying == true) {

					WindowSystemLogger.Warning(LocalizationSystem.GetName(), string.Format("System not ready. Do not use `LocalizationSystem.Get()` method while/before system starting. You can check it's state by `LocalizationSystem.IsReady()` call. Key: `{0}`.", key));

				}

			}

			return string.Empty;

		}

		public static Sprite[] GetNewSprites() {

			var languagesCount = System.Enum.GetValues(typeof(UnityEngine.SystemLanguage)).Length;
			return new Sprite[languagesCount];

		}
		
		public static int GetCurrentVersionId() {
			
			return LocalizationSystem.currentVersionNumber;
			
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
		
		public static string GetSpritePath(LocalizationKey key, params object[] parameters) {
			
			return LocalizationSystem.Get(key, parameters).Trim();

		}

		public static Sprite GetSprite(LocalizationKey key, params object[] parameters) {

			var sprite = Resources.Load<Sprite>(LocalizationSystem.GetSpritePath(key, parameters));
			return sprite as Sprite;

		}

		public static string Get(LocalizationKey key, UnityEngine.SystemLanguage language) {

			return LocalizationSystem.Get(key.key, language);

		}

		public static string Get(LocalizationKey key, params object[] parameters) {

			if (parameters.Length == key.parameters) {

				var value = LocalizationSystem.Get(key.key, LocalizationSystem.GetCurrentLanguage());
				if (key.formatWithDeclension == true) {

					return LocalizationSystem.FormatWithDeclension(value, returnWithNumber: !key.outputDeclensionWithoutNumber, parameters: parameters);

				}

				return string.Format(value, parameters);

			} else {
				
				if (LocalizationSystem.instance.logEnabled == true) {
					
					WindowSystemLogger.Warning(LocalizationSystem.GetName(), string.Format("Wrong parameters length in key `{0}`", key.key));

				}

			}

			return key.key;

		}

		public static bool ContainsKey(string key) {

			return LocalizationSystem.GetKeys().Contains(key.ToLower());

		}

		public static bool IsNeedToFormatWithDeclension(string key, UnityEngine.SystemLanguage language) {
			
			var value = LocalizationSystem.Get(key, language);
			var matches = Regex.Matches(value, @"{\d+, (.*?)}", RegexOptions.None);
			return (matches != null && matches.Count > 0);

		}

		public static int GetParametersCount(string key, UnityEngine.SystemLanguage language) {

			var value = LocalizationSystem.Get(key, language);
			var matches = Regex.Matches(value, @"{.*?}", RegexOptions.None);
			return matches.Count;

		}

		public static string GetCachePath() {
			
			return string.Format("{0}/Localization.dat", Application.persistentDataPath);

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
				LocalizationSystem.TryToSaveCSV(text, loadCacheOnFail: false);

			} else {
			#endif

				var path = LocalizationSystem.GetCachePath();
				if (System.IO.File.Exists(path) == false) return;

				var text = System.IO.File.ReadAllText(path);
				LocalizationSystem.TryToSaveCSV(text, loadCacheOnFail: false);

			#if UNITY_EDITOR
			}
			#endif

		}

		public static void TryToSaveCSV(string data, bool loadCacheOnFail = true) {

			try {

				var parsed = CSVParser.ReadCSV(data);

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

					if (col == LocalizationSystem.defaultLanguage.ToString()) {
						
						LocalizationSystem.currentVersionNumber = int.Parse(parsed[1][i]);
						
					}

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

				LocalizationSystem.currentLanguage = LocalizationSystem.defaultLanguage;

				if (LocalizationSystem.instance == null || LocalizationSystem.instance.logEnabled == true) {
					
					WindowSystemLogger.Log(LocalizationSystem.GetName(), string.Format("Loaded version {3}. Cache saved to: {0}, Keys: {1}, Languages: {2}", path, keysCount, langCount, LocalizationSystem.GetCurrentVersionId()));

				}

				LocalizationSystem.isReady = true;

			} catch(System.Exception ex) {

				if (LocalizationSystem.instance == null || LocalizationSystem.instance.logEnabled == true) {
					
					// Nothing to do: failed to parse
					WindowSystemLogger.Error(LocalizationSystem.GetName(), string.Format("Parser error: {0}\n{1}", ex.Message, ex.StackTrace));

				}

				if (loadCacheOnFail == true) {

					LocalizationSystem.TryToLoadCache();

				}

			}

		}

	}

}