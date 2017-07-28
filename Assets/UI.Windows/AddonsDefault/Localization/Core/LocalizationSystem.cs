//#if UNITY_TVOS
//#define STORAGE_NOT_SUPPORTED
//#endif
using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI.Windows.Plugins.Services;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Utilities;
using ME;


#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace UnityEngine.UI.Windows.Plugins.Localization {

	public class LocalizationSystem : ServiceManager<LocalizationSystem> {

		//public const string NO_KEY_STRING = "~NO KEY: {0}~";
		public const string NO_KEY_STRING = "{0}";

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

			}, includeInactive: true);

		}

		public static string[] GetKeys() {

			return LocalizationSystem.keys;

		}

		private static string ArabicCheck(string text, UnityEngine.SystemLanguage language) {
			
			if (language == SystemLanguage.Arabic) {

				return ArabicSupport.ArabicFixer.Fix(text, false, false);

			}

			return text;

		}

		public static string Get(string key) {

			var lang = LocalizationSystem.GetCurrentLanguage();
			return LocalizationSystem.ArabicCheck(LocalizationSystem.Get(key, lang), lang);

		}

		public static string Get(string key, UnityEngine.SystemLanguage language, bool forced = false) {

			if (LocalizationSystem.IsReady() == true || forced == true) {

				string[] values;
				if (LocalizationSystem.valuesByLanguage.TryGetValue(language, out values) == true) {

					var keys = LocalizationSystem.GetKeys();
					if (keys != null && key != null) {
						
						var index = System.Array.IndexOf(keys, key.ToLower());
						if (index >= 0 && index < values.Length) {
							
							return values[index];

						}

					}

				}

			} else {

				if (Application.isPlaying == true) {

					WindowSystemLogger.Warning(LocalizationSystem.GetName(), string.Format("System not ready. Do not use `LocalizationSystem.Get()` method while/before system starting. You can check it's state by `LocalizationSystem.IsReady()` call. Key: `{0}`.", key));

				}

			}

			if (string.IsNullOrEmpty(key) == true) {

				return string.Empty;

			}

			return string.Format(LocalizationSystem.NO_KEY_STRING, key);

		}

		public static Sprite[] GetNewSprites() {

			var languagesCount = System.Enum.GetValues(typeof(UnityEngine.SystemLanguage)).Length;
			return new Sprite[languagesCount];

		}
		
		public static int GetCurrentVersionId() {
			
			return LocalizationSystem.currentVersionNumber;
			
		}

		public static UnityEngine.SystemLanguage GetCurrentLanguage() {

			//if (LocalizationSystem.currentLanguage == null) LocalizationSystem.currentLanguage = LocalizationSystem.defaultLanguage;
			//if (LocalizationSystem.currentLanguage == null) LocalizationSystem.currentLanguage = SystemLanguage.English;

			return LocalizationSystem.currentLanguage;

		}

		public static int GetCurrentLanguageIndex() {

			return System.Array.IndexOf(LocalizationSystem.languages, LocalizationSystem.currentLanguage);

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

			var sprite = UnityEngine.Resources.Load<Sprite>(LocalizationSystem.GetSpritePath(key, parameters));
			return sprite as Sprite;

		}

		public static string Get(LocalizationKey key, UnityEngine.SystemLanguage language) {

			return LocalizationSystem.Get(key.key, language);

		}

		public static string Get(LocalizationKey key, params object[] parameters) {

			if (parameters != null && parameters.Length == key.parameters) {

				var lang = LocalizationSystem.GetCurrentLanguage();
				var value = LocalizationSystem.Get(key.key, lang);
				if (key.formatWithDeclension == true) {

					return LocalizationSystem.ArabicCheck(LocalizationSystem.FormatWithDeclension(value, returnWithNumber: !key.outputDeclensionWithoutNumber, parameters: parameters), lang);

				}

				return LocalizationSystem.ArabicCheck(string.Format(value, parameters), lang);

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

			#if UNITY_TVOS
			return LocalizationSystem.GetCachePath(Application.temporaryCachePath, forceDirectoryCreation: true);
			#else
			return LocalizationSystem.GetCachePath(Application.persistentDataPath, forceDirectoryCreation: true);
			#endif

		}

		public static string GetBuiltinCachePath() {

			#if UNITY_EDITOR
			bool forceDirectoryCreation = true;
			#else
			bool forceDirectoryCreation = false;
			#endif
			return LocalizationSystem.GetCachePath(Application.streamingAssetsPath, forceDirectoryCreation);

		}

		public static string GetCachePath(string storagePath, bool forceDirectoryCreation) {
			
			var dir = string.Format("{0}/UI.Windows/Cache/Services", storagePath);
			var path = string.Format("{0}/{1}.uiws", dir, LocalizationSystem.GetName());
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
			if (LocalizationSystem.cacheLoaded == true) return;
			if (Application.isPlaying == false) {

				LocalizationSystem.cacheLoaded = true;

				var path = LocalizationSystem.GetCachePath();
				#if STORAGE_NOT_SUPPORTED
				if (PlayerPrefs.HasKey(path) == false) return;
				var text = PlayerPrefs.GetString(path, string.Empty);
				#else
				if (System.IO.File.Exists(path) == false) {
					
					path = LocalizationSystem.GetBuiltinCachePath();
					
				}
				var text = System.IO.File.ReadAllText(path);
				#endif
				
				LocalizationSystem.TryToSaveCSV(text, loadCacheOnFail: false);

			} else {
			#endif
				
				#if UNITY_ANDROID
				Coroutines.Run(LocalizationSystem.LoadByWWW(LocalizationSystem.GetCachePath(), LocalizationSystem.GetBuiltinCachePath()));
				#else
				var path = LocalizationSystem.GetCachePath();
				#if STORAGE_NOT_SUPPORTED
				if (PlayerPrefs.HasKey(path) == false) return;
				var text = PlayerPrefs.GetString(path, string.Empty);
				#else
				if (System.IO.File.Exists(path) == false) {
					
					path = LocalizationSystem.GetBuiltinCachePath();
					
				}
				var text = System.IO.File.ReadAllText(path);
				#endif

				LocalizationSystem.TryToSaveCSV(text, loadCacheOnFail: false);
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

			LocalizationSystem.TryToSaveCSV(www.text, loadCacheOnFail: false);

		}

		public static void TryToSaveCSV(string data, bool loadCacheOnFail = true) {

			try {

				#if UNITY_EDITOR
				var monoMemorySize = Profiler.GetMonoUsedSizeLong();
				#endif

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
					try {

						var lng = (UnityEngine.SystemLanguage)System.Enum.Parse(typeof(UnityEngine.SystemLanguage), col);
						languages.Add(lng);

					} catch (Exception) {



					}

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

				foreach (var lang in languages) {

					var val = LocalizationSystem.Get("Last", lang, forced: true);
					if (val != "Last") {

						Debug.Log("Last Key: " + keys.Last());
						Debug.Log("Value: " + val);
						foreach (var key in keys) {

							Debug.Log(key + " :: " + LocalizationSystem.Get(key, lang, forced: true));

						}

						throw new Exception(string.Format("Language `{0}` has errors", lang));

					}

				}

				var path = LocalizationSystem.GetCachePath();
				#if STORAGE_NOT_SUPPORTED
				PlayerPrefs.SetString(path, data);
				#else
				System.IO.File.WriteAllText(path, data);
				#endif

				#if UNITY_EDITOR
				path = LocalizationSystem.GetBuiltinCachePath();
				System.IO.File.WriteAllText(path, data);
				#endif

				LocalizationSystem.currentLanguage = LocalizationSystem.defaultLanguage;

				if (LocalizationSystem.instance == null || LocalizationSystem.instance.logEnabled == true) {
					
					WindowSystemLogger.Log(LocalizationSystem.GetName(), string.Format("Loaded version {3}. Cache saved to: {0}, Keys: {1}, Languages: {2}", path, keysCount, langCount, LocalizationSystem.GetCurrentVersionId()));

				}

				LocalizationSystem.isReady = true;

				#if UNITY_EDITOR
				var monoMemorySizeAfter = Profiler.GetMonoUsedSizeLong();
				var deltaMemory = monoMemorySizeAfter - monoMemorySize;
				WindowSystemLogger.Warning(LocalizationSystem.GetName(), string.Format("Allocated: {0} bytes ({1}MB)", deltaMemory, (deltaMemory / 1024f / 1024f)));
				#endif

			} catch(System.Exception ex) {

				Debug.LogError("LocalizationSystem parse failed");

				if (LocalizationSystem.instance == null || LocalizationSystem.instance.logEnabled == true) {
					
					// Nothing to do: failed to parse
					WindowSystemLogger.Error(LocalizationSystem.GetName(), string.Format("Parser error: {0}\n{1}", ex.Message, ex.StackTrace));

				}

				if (loadCacheOnFail == true) {

					LocalizationSystem.TryToLoadCache();

				}

			}

		}

		public static string GetCultureNameByLanguage(UnityEngine.SystemLanguage lang) {

			/*
			af-ZA	Afrikaans - South Africa	0x0436	AFK
			sq-AL	Albanian - Albania	0x041C	SQI
			ar-DZ	Arabic - Algeria	0x1401	ARG
			ar-BH	Arabic - Bahrain	0x3C01	ARH
			ar-EG	Arabic - Egypt	0x0C01	ARE
			ar-IQ	Arabic - Iraq	0x0801	ARI
			ar-JO	Arabic - Jordan	0x2C01	ARJ
			ar-KW	Arabic - Kuwait	0x3401	ARK
			ar-LB	Arabic - Lebanon	0x3001	ARB
			ar-LY	Arabic - Libya	0x1001	ARL
			ar-MA	Arabic - Morocco	0x1801	ARM
			ar-OM	Arabic - Oman	0x2001	ARO
			ar-QA	Arabic - Qatar	0x4001	ARQ
			ar-SA	Arabic - Saudi Arabia	0x0401	ARA
			ar-SY	Arabic - Syria	0x2801	ARS
			ar-TN	Arabic - Tunisia	0x1C01	ART
			ar-AE	Arabic - United Arab Emirates	0x3801	ARU
			ar-YE	Arabic - Yemen	0x2401	ARY
			hy-AM	Armenian - Armenia	0x042B	 
			Cy-az-AZ	Azeri (Cyrillic) - Azerbaijan	0x082C	 
			Lt-az-AZ	Azeri (Latin) - Azerbaijan	0x042C	 
			eu-ES	Basque - Basque	0x042D	EUQ
			be-BY	Belarusian - Belarus	0x0423	BEL
			bg-BG	Bulgarian - Bulgaria	0x0402	BGR
			ca-ES	Catalan - Catalan	0x0403	CAT
			zh-CN	Chinese - China	0x0804	CHS
			zh-HK	Chinese - Hong Kong SAR	0x0C04	ZHH
			zh-MO	Chinese - Macau SAR	0x1404	 
			zh-SG	Chinese - Singapore	0x1004	ZHI
			zh-TW	Chinese - Taiwan	0x0404	CHT
			zh-CHS	Chinese (Simplified)	0x0004	 
			zh-CHT	Chinese (Traditional)	0x7C04	 
			hr-HR	Croatian - Croatia	0x041A	HRV
			cs-CZ	Czech - Czech Republic	0x0405	CSY
			da-DK	Danish - Denmark	0x0406	DAN
			div-MV	Dhivehi - Maldives	0x0465	 
			nl-BE	Dutch - Belgium	0x0813	NLB
			nl-NL	Dutch - The Netherlands	0x0413	 
			en-AU	English - Australia	0x0C09	ENA
			en-BZ	English - Belize	0x2809	ENL
			en-CA	English - Canada	0x1009	ENC
			en-CB	English - Caribbean	0x2409	 
			en-IE	English - Ireland	0x1809	ENI
			en-JM	English - Jamaica	0x2009	ENJ
			en-NZ	English - New Zealand	0x1409	ENZ
			en-PH	English - Philippines	0x3409	 
			en-ZA	English - South Africa	0x1C09	ENS
			en-TT	English - Trinidad and Tobago	0x2C09	ENT
			en-GB	English - United Kingdom	0x0809	ENG
			en-US	English - United States	0x0409	ENU
			en-ZW	English - Zimbabwe	0x3009	 
			et-EE	Estonian - Estonia	0x0425	ETI
			fo-FO	Faroese - Faroe Islands	0x0438	FOS
			fa-IR	Farsi - Iran	0x0429	FAR
			fi-FI	Finnish - Finland	0x040B	FIN
			fr-BE	French - Belgium	0x080C	FRB
			fr-CA	French - Canada	0x0C0C	FRC
			fr-FR	French - France	0x040C	 
			fr-LU	French - Luxembourg	0x140C	FRL
			fr-MC	French - Monaco	0x180C	 
			fr-CH	French - Switzerland	0x100C	FRS
			gl-ES	Galician - Galician	0x0456	 
			ka-GE	Georgian - Georgia	0x0437	 
			de-AT	German - Austria	0x0C07	DEA
			de-DE	German - Germany	0x0407	 
			de-LI	German - Liechtenstein	0x1407	DEC
			de-LU	German - Luxembourg	0x1007	DEL
			de-CH	German - Switzerland	0x0807	DES
			el-GR	Greek - Greece	0x0408	ELL
			gu-IN	Gujarati - India	0x0447	 
			he-IL	Hebrew - Israel	0x040D	HEB
			hi-IN	Hindi - India	0x0439	HIN
			hu-HU	Hungarian - Hungary	0x040E	HUN
			is-IS	Icelandic - Iceland	0x040F	ISL
			id-ID	Indonesian - Indonesia	0x0421	 
			it-IT	Italian - Italy	0x0410	 
			it-CH	Italian - Switzerland	0x0810	ITS
			ja-JP	Japanese - Japan	0x0411	JPN
			kn-IN	Kannada - India	0x044B	 
			kk-KZ	Kazakh - Kazakhstan	0x043F	 
			kok-IN	Konkani - India	0x0457	 
			ko-KR	Korean - Korea	0x0412	KOR
			ky-KZ	Kyrgyz - Kazakhstan	0x0440	 
			lv-LV	Latvian - Latvia	0x0426	LVI
			lt-LT	Lithuanian - Lithuania	0x0427	LTH
			mk-MK	Macedonian (FYROM)	0x042F	MKD
			ms-BN	Malay - Brunei	0x083E	 
			ms-MY	Malay - Malaysia	0x043E	 
			mr-IN	Marathi - India	0x044E	 
			mn-MN	Mongolian - Mongolia	0x0450	 
			nb-NO	Norwegian (Bokmål) - Norway	0x0414	 
			nn-NO	Norwegian (Nynorsk) - Norway	0x0814	 
			pl-PL	Polish - Poland	0x0415	PLK
			pt-BR	Portuguese - Brazil	0x0416	PTB
			pt-PT	Portuguese - Portugal	0x0816	 
			pa-IN	Punjabi - India	0x0446	 
			ro-RO	Romanian - Romania	0x0418	ROM
			ru-RU	Russian - Russia	0x0419	RUS
			sa-IN	Sanskrit - India	0x044F	 
			Cy-sr-SP	Serbian (Cyrillic) - Serbia	0x0C1A	 
			Lt-sr-SP	Serbian (Latin) - Serbia	0x081A	 
			sk-SK	Slovak - Slovakia	0x041B	SKY
			sl-SI	Slovenian - Slovenia	0x0424	SLV
			es-AR	Spanish - Argentina	0x2C0A	ESS
			es-BO	Spanish - Bolivia	0x400A	ESB
			es-CL	Spanish - Chile	0x340A	ESL
			es-CO	Spanish - Colombia	0x240A	ESO
			es-CR	Spanish - Costa Rica	0x140A	ESC
			es-DO	Spanish - Dominican Republic	0x1C0A	ESD
			es-EC	Spanish - Ecuador	0x300A	ESF
			es-SV	Spanish - El Salvador	0x440A	ESE
			es-GT	Spanish - Guatemala	0x100A	ESG
			es-HN	Spanish - Honduras	0x480A	ESH
			es-MX	Spanish - Mexico	0x080A	ESM
			es-NI	Spanish - Nicaragua	0x4C0A	ESI
			es-PA	Spanish - Panama	0x180A	ESA
			es-PY	Spanish - Paraguay	0x3C0A	ESZ
			es-PE	Spanish - Peru	0x280A	ESR
			es-PR	Spanish - Puerto Rico	0x500A	ES
			es-ES	Spanish - Spain	0x0C0A	 
			es-UY	Spanish - Uruguay	0x380A	ESY
			es-VE	Spanish - Venezuela	0x200A	ESV
			sw-KE	Swahili - Kenya	0x0441	 
			sv-FI	Swedish - Finland	0x081D	SVF
			sv-SE	Swedish - Sweden	0x041D	 
			syr-SY	Syriac - Syria	0x045A	 
			ta-IN	Tamil - India	0x0449	 
			tt-RU	Tatar - Russia	0x0444	 
			te-IN	Telugu - India	0x044A	 
			th-TH	Thai - Thailand	0x041E	THA
			tr-TR	Turkish - Turkey	0x041F	TRK
			uk-UA	Ukrainian - Ukraine	0x0422	UKR
			ur-PK	Urdu - Pakistan	0x0420	URD
			Cy-uz-UZ	Uzbek (Cyrillic) - Uzbekistan	0x0843	 
			Lt-uz-UZ	Uzbek (Latin) - Uzbekistan	0x0443	 
			vi-VN	Vietnamese - Vietnam	0x042A	VIT
			*/

			string output = string.Empty;

			switch (lang) {

				case SystemLanguage.Afrikaans:
					output = "af-ZA";
					break;

				case SystemLanguage.Arabic:
					output = "ar-AE";
					break;

				case SystemLanguage.Basque:
					output = "eu-ES";
					break;

				case SystemLanguage.Belarusian:
					output = "be-BY";
					break;

				case SystemLanguage.Bulgarian:
					output = "bg-BG";
					break;

				case SystemLanguage.Catalan:
					output = "ca-ES";
					break;

				case SystemLanguage.Chinese:
					output = "zh-CN";
					break;

				case SystemLanguage.ChineseSimplified:
					output = "zh-CHS";
					break;

				case SystemLanguage.ChineseTraditional:
					output = "zh-CNT";
					break;

				case SystemLanguage.Czech:
					output = "cs-CZ";
					break;

				case SystemLanguage.Danish:
					output = "da-DK";
					break;

				case SystemLanguage.Dutch:
					output = "nl-BE";
					break;

				case SystemLanguage.Unknown:
				case SystemLanguage.English:
					output = "en-GB";
					break;

				case SystemLanguage.Estonian:
					output = "et-EE";
					break;

				case SystemLanguage.Faroese:
					output = "fo-FO";
					break;

				case SystemLanguage.Finnish:
					output = "fi-FI";
					break;

				case SystemLanguage.French:
					output = "fr-FR";
					break;

				case SystemLanguage.German:
					output = "de-DE";
					break;

				case SystemLanguage.Greek:
					output = "el-GR";
					break;

				case SystemLanguage.Hebrew:
					output = "he-IL";
					break;

				case SystemLanguage.Hungarian:
					output = "hu-HU";
					break;

				case SystemLanguage.Icelandic:
					output = "is-IS";
					break;

				case SystemLanguage.Indonesian:
					output = "id-ID";
					break;

				case SystemLanguage.Italian:
					output = "it-IT";
					break;

				case SystemLanguage.Japanese:
					output = "ja-JP";
					break;

				case SystemLanguage.Korean:
					output = "ko-KR";
					break;

				case SystemLanguage.Latvian:
					output = "lv-LV";
					break;

				case SystemLanguage.Lithuanian:
					output = "lt-LT";
					break;

				case SystemLanguage.Norwegian:
					output = "nn-NO";
					break;

				case SystemLanguage.Polish:
					output = "pl-PL";
					break;

				case SystemLanguage.Portuguese:
					output = "pt-PT";
					break;

				case SystemLanguage.Romanian:
					output = "ro-RO";
					break;

				case SystemLanguage.Russian:
					output = "ru-RU";
					break;

				case SystemLanguage.SerboCroatian:
					output = "Cy-sr-SP";
					break;

				case SystemLanguage.Slovak:
					output = "sk-SK";
					break;

				case SystemLanguage.Slovenian:
					output = "sl-SI";
					break;

				case SystemLanguage.Spanish:
					output = "es-ES";
					break;

				case SystemLanguage.Swedish:
					output = "sv-SE";
					break;

				case SystemLanguage.Thai:
					output = "th-TH";
					break;

				case SystemLanguage.Turkish:
					output = "tr-TR";
					break;

				case SystemLanguage.Ukrainian:
					output = "uk-UA";
					break;

				case SystemLanguage.Vietnamese:
					output = "vi-VN";
					break;

			}

			return output;

		}

	}

}