#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
#define UNITY_MOBILE
#endif

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEngine.UI.Windows {
	
	public class WindowSystemFlow : WindowSystem {

		public FlowData flow;

		#if UNITY_EDITOR || UNITY_MOBILE
		public FlowData flowMobileOnly;
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		public FlowData flowStandaloneOnly;
		#endif

		public bool showRootOnStart = true;

		protected override void Init() {

			var flow = this.flow;
			FlowSystem.SetData(flow);

			#if UNITY_MOBILE
			if (this.flowMobileOnly != null) flow = this.flowMobileOnly;
			#endif
			#if UNITY_STANDALONE
			if (this.flowStandaloneOnly != null) flow = this.flowStandaloneOnly;
			#endif
			
			this.defaults.AddRange(flow.GetDefaultScreens());
			this.windows.AddRange(flow.GetAllScreens());

			base.Init();

		}
		
		public void Start() {
			
			if (this.showRootOnStart == true) {

				var root = this.flow.GetRootScreen();
				if (root != null) WindowSystem.Show(root);

			}
			
		}

	}

}
