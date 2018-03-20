using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Social.Queries;

namespace UnityEngine.UI.Windows.Plugins.Social.Core {
	
	public static class FlowWindowSocialExt {
		
		public static bool IsSocial(this UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow window) {
			
			return (window.flags & UnityEngine.UI.Windows.Plugins.Flow.Data.FlowWindow.Flags.Tag1) != 0;
			
		}
		
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

		public static void WaitFor(System.Collections.IEnumerator routine) {

			SocialSystem.instance.StartCoroutine(routine);

		}

		public ISocialModule GetModule(System.Func<ISocialModule, bool> predicate) {

			return this.modules.FirstOrDefault(predicate);

		}

		public T GetModule<T>() {

			return this.modules.OfType<T>().FirstOrDefault();

		}

		public void Load(SocialSettings settings) {

			foreach (var platform in settings.activePlatforms) {

				if (platform.active == true) {

					this.LoadPlatform(platform);

				}

			}

		}

		public ISocialModule LoadPlatform(Platform platform) {
			
			ISocialModule module = null;

			// Activate social platforms
			var moduleType = string.Format("{0}.{0}Module", platform.GetPlatformClassName());
			var type = System.Type.GetType(string.Format("UnityEngine.UI.Windows.Plugins.Social.Modules.Impl.{0}", moduleType), throwOnError: false, ignoreCase: true);

			if (type != null) {

				module = System.Activator.CreateInstance(type) as ISocialModule;
				module.OnLoad(platform.settings);

				this.modules.Add(module);

			} else {

				if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogWarningFormat("[SOCIAL] Module `{0}` not found. Skipped.", moduleType);

			}

			return module;

		}

	}

}