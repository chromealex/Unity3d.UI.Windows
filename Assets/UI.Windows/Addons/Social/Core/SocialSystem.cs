using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Social {

	[Flags]
	public enum Platform : byte {
		
		None = 0x0,
		
		VK = 0x1,
		Facebook = 0x2,
		
		Tweeter = 0x4,
		Instagram = 0x8,

		GooglePlay = 0x10,
		GameCenter = 0x20,
		
	}

	public class SocialSystem : MonoBehaviour {

		public static SocialSystem instance;

		public SocialSettings settings;

		public List<ISocialModule> modules = new List<ISocialModule>();

		public void Awake() {

			SocialSystem.instance = this;
			
			this.Load(this.settings);

		}

		public void Start() {

		}
		
		public static void LoadImage(bool isMain, string url, Action<Texture, bool> callback, Action<Texture> callbackMain) {
			
			if (string.IsNullOrEmpty(url) == true) {
				
				callback(null, false);
				if (isMain == true) callbackMain(null);
				
			} else {
				
				new SocialHttp(url, (texture, result) => {

					callback(texture, result);
					if (isMain == true) callbackMain(texture);

				});

			}
			
		}

		public static void LoadImage(string url, Action<Texture, bool> callback) {

			if (string.IsNullOrEmpty(url) == true) {

				callback(null, false);

			} else {

				new SocialHttp(url, callback);

			}

		}

		public static void WaitFor(IEnumerator routine) {

			SocialSystem.instance.StartCoroutine(routine);

		}

		public ISocialModule GetModule(System.Func<ISocialModule, bool> predicate) {

			return this.modules.FirstOrDefault(predicate);

		}

		public T GetModule<T>() {

			return this.modules.OfType<T>().FirstOrDefault();

		}

		public void Load(SocialSettings settings) {

			var platforms = System.Enum.GetValues(typeof(Platform));
			foreach (var platform in platforms) {

				if ((settings.activePlatforms & (Platform)platform) != 0) {

					this.LoadPlatform((Platform)platform);

				}

			}

		}

		public ISocialModule LoadPlatform(Platform platform) {
			
			ISocialModule module = null;

			// Activate social platforms
			var moduleType = platform.ToString() + "." + platform.ToString() + "Module";
			var type = System.Type.GetType("UnityEngine.UI.Windows.Plugins.Social.Modules." + moduleType + "", throwOnError: false, ignoreCase: true);

			if (type != null) {

				var settings = Resources.Load("UI.Windows/Social/" + platform.ToString() + "Settings") as ModuleSettings;

				module = System.Activator.CreateInstance(type) as ISocialModule;
				module.OnLoad(settings);

				this.modules.Add(module);

			} else {

				Debug.LogWarningFormat("[SOCIAL] Module `{0}` not found.", moduleType);

			}

			return module;

		}

	}

}