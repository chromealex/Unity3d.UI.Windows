#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
#define UNITY_MOBILE
#endif

using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {
	
	public class WindowSystemPlatforms : WindowSystem {

		#if UNITY_EDITOR || UNITY_MOBILE
		public List<WindowBase> defaultsMobileOnly = new List<WindowBase>();
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		public List<WindowBase> defaultsStandaloneOnly = new List<WindowBase>();
		#endif
		
		#if UNITY_EDITOR || UNITY_MOBILE
		public List<WindowBase> mobileOnly = new List<WindowBase>();
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		public List<WindowBase> standaloneOnly = new List<WindowBase>();
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
