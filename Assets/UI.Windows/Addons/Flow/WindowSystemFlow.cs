#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
#define UNITY_MOBILE
#endif
#if UNITY_XBOX360 || UNITY_XBOXONE || UNITY_PS3 || UNITY_PS4 || UNITY_WII
#define UNITY_CONSOLE
#endif

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEngine.UI.Windows {
	
	public class WindowSystemFlow : WindowSystem {

		[Header("Flow Projects")]
		public FlowData flow;

		#if UNITY_EDITOR || UNITY_MOBILE
		public FlowData flowMobileOnly;
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		public FlowData flowStandaloneOnly;
		#endif
		#if UNITY_EDITOR || UNITY_CONSOLE
		public FlowData flowConsoleOnly;
		#endif

		public bool showRootOnStart = true;

		protected override void Init() {

			var flow = this.flow;
			if (flow == null) {

				Debug.LogError("Flow data was not set to WindowSystemFlow. Set ");
				return;

			}

			#if UNITY_MOBILE
			if (this.flowMobileOnly != null) flow = this.flowMobileOnly;
			#endif
			#if UNITY_STANDALONE
			if (this.flowStandaloneOnly != null) flow = this.flowStandaloneOnly;
			#endif
			#if UNITY_CONSOLE
			if (this.flowConsoleOnly != null) flow = this.flowConsoleOnly;
			#endif
			
			FlowSystem.SetData(flow);

			this.defaults.AddRange(flow.GetDefaultScreens());
			this.windows.AddRange(flow.GetAllScreens());

			base.Init();

			this.OnStart();

		}
		
		public void OnStart() {
			
			if (this.showRootOnStart == true) {

				var root = this.flow.GetRootScreen();
				if (root != null) WindowSystem.Show(root);

			}
			
		}

	}

}
