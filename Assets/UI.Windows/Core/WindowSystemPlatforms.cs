#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
#define UNITY_MOBILE
#endif

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEngine.UI.Windows {
	
	public class WindowSystemPlatforms : WindowSystem {

		#if UNITY_EDITOR || UNITY_MOBILE
		public List<WindowItem> defaultsMobileOnly = new List<WindowItem>();
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		public List<WindowItem> defaultsStandaloneOnly = new List<WindowItem>();
		#endif
		
		#if UNITY_EDITOR || UNITY_MOBILE
		public List<WindowItem> mobileOnly = new List<WindowItem>();
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		public List<WindowItem> standaloneOnly = new List<WindowItem>();
		#endif

		protected override void Init() {
			
			#if UNITY_MOBILE
			this.defaults.AddRange(this.defaultsMobileOnly);
			#endif
			#if UNITY_STANDALONE
			this.defaults.AddRange(this.defaultsStandaloneOnly);
			#endif
			
			#if UNITY_MOBILE
			this.windows.AddRange(this.mobileOnly);
			#endif
			#if UNITY_STANDALONE
			this.windows.AddRange(this.standaloneOnly);
			#endif

			base.Init();

		}

	}

}
