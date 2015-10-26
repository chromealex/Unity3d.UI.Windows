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

		public enum LoadType : int {

			None = 0x0,

			Window = 0x1,
			Function = 0x2,
			Audio = 0x4,

		};

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
		
		[Header("Flow Projects (Additional)")]
		[BitMask(typeof(LoadType))]
		public LoadType additionalLoadType = LoadType.None;

		[ReadOnly("additionalLoadType", (int)(LoadType.Window | LoadType.Function), bitMask: true)]
		public FlowData additionalFlow;
		
		#if UNITY_EDITOR || UNITY_MOBILE
		[ReadOnly("additionalLoadType", (int)(LoadType.Window | LoadType.Function), bitMask: true)]
		public FlowData additionalFlowMobileOnly;
		#endif
		#if UNITY_EDITOR || UNITY_STANDALONE
		[ReadOnly("additionalLoadType", (int)(LoadType.Window | LoadType.Function), bitMask: true)]
		public FlowData additionalFlowStandaloneOnly;
		#endif
		#if UNITY_EDITOR || UNITY_CONSOLE
		[ReadOnly("additionalLoadType", (int)(LoadType.Window | LoadType.Function), bitMask: true)]
		public FlowData additionalFlowConsoleOnly;
		#endif

		[Header("Start Settings")]
		public bool showRootOnStart = true;

		protected override void Init() {

			#region FLOW DEFAULT
			{
				var flow = this.flow;
				if (flow == null) {

					Debug.LogError("Flow data was not set to WindowSystemFlow.");
					return;

				}

				#if UNITY_MOBILE
				if (this.flowMobileOnly != null) flow = this.flowMobileOnly;
				#endif
				#if UNITY_STANDALONE
				if (this.flowStandaloneOnly != null)
				flow = this.flowStandaloneOnly;
				#endif
				#if UNITY_CONSOLE
				if (this.flowConsoleOnly != null) flow = this.flowConsoleOnly;
				#endif

				FlowSystem.SetData(flow);
				Audio.Manager.InitAndAdd(flow.audio);

				this.defaults.AddRange(flow.GetDefaultScreens());
				this.windows.AddRange(flow.GetAllScreens());

			}
			#endregion

			#region FLOW ADDITIONAL
			{

				var additionalFlow = this.additionalFlow;
				#if UNITY_MOBILE
				if (this.additionalFlowMobileOnly != null) flow = this.additionalFlowMobileOnly;
				#endif
				#if UNITY_STANDALONE
				if (this.additionalFlowStandaloneOnly != null) flow = this.additionalFlowStandaloneOnly;
				#endif
				#if UNITY_CONSOLE
				if (this.additionalFlowConsoleOnly != null) flow = this.additionalFlowConsoleOnly;
				#endif

				if (additionalFlow != null) {

					var screens = additionalFlow.GetAllScreens((w) => ((this.additionalLoadType & LoadType.Function) != 0 && (w.IsFunction() == true || w.GetFunctionContainer() != null)) || ((this.additionalLoadType & LoadType.Window) != 0 && w.IsFunction() == false));
					this.windows.AddRange(screens);
					if ((this.additionalLoadType & LoadType.Audio) != 0) Audio.Manager.InitAndAdd(flow.audio);

				}

			}
			#endregion
			
			base.Init();

			this.OnStart();

		}
		
		public void OnStart() {

			if (this.showRootOnStart == true) {

				var root = this.flow.GetRootScreen();
				if (root != null) WindowSystem.Show(root);

			}
			
		}

		public static T DoFlow<T>(IFunctionIteration screen, int from, int to, bool hide, System.Action<T> onParametersPassCall) where T : WindowBase {
			
			var item = UnityEngine.UI.Windows.Plugins.Flow.FlowSystem.GetAttachItem(from, to);
			return WindowSystemFlow.DoFlow<T>(screen, item, hide, onParametersPassCall);

		}

		public static T DoFlow<T>(IFunctionIteration screen, AttachItem item, bool hide, System.Action<T> onParametersPassCall) where T : WindowBase {

			var newWindow = WindowSystem.Show<T>(
				(w) => w.SetFunctionIterationIndex(screen.GetFunctionIterationIndex()),
				item,
				onParametersPassCall
			);
			
			WindowSystemFlow.OnDoTransition((item == null) ? 0 : item.index, screen.GetWindow(), (item == null) ? FlowSystem.GetWindow(newWindow).id : item.targetId, hide);

			if (hide == true) screen.Hide(item);

			return newWindow;

		}

		public static void OnDoTransition(int index, WindowBase fromScreen, int targetId, bool hide = true) {
			
			WindowSystem.OnDoTransition(index, FlowSystem.GetWindow(fromScreen).id, targetId, hide);
			
		}

	}

}
