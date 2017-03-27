#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
#define UNITY_MOBILE
#endif
#if UNITY_XBOX360 || UNITY_XBOXONE || UNITY_PS3 || UNITY_PS4 || UNITY_WII
#define UNITY_CONSOLE
#endif

using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Flow.Data;

namespace UnityEngine.UI.Windows {
	
	public class WindowSystemFlow : WindowSystem {

		[System.Serializable]
		public class FlowItem {

			public RuntimePlatform[] oneOfPlatforms;
			public FlowData flow;
			public bool customDefaultsOverride;
			public WindowItem[] customDefaults;

			public bool HasCustomDefaults() {

				return this.customDefaults != null && this.customDefaults.Length > 0;

			}

			public bool IsDefaultsOverride() {

				return this.customDefaultsOverride;

			}

			public virtual bool IsValid() {

				if (this.oneOfPlatforms.Length == 0) return true;

				var checkPlatform = WindowSystem.GetCurrentRuntimePlatform();
				for (int i = 0; i < this.oneOfPlatforms.Length; ++i) {

					if (checkPlatform == this.oneOfPlatforms[i]) return true;

				}

				return false;

			}

			#if UNITY_EDITOR
			public void Validate() {

				if (this.customDefaults != null) {

					for (int i = 0; i < this.customDefaults.Length; ++i) {

						this.customDefaults[i].Validate();

					}

				}

			}
			#endif

		}

		[System.Serializable]
		public class FlowAdditive : FlowItem {

			public LoadType loadType = LoadType.None;

		}

		public enum LoadType : int {

			None = 0x0,

			Window = 0x1,
			Function = 0x2,
			Audio = 0x4,

		};

		[Header("Flow Projects")]
		public FlowItem[] flow;
		public FlowAdditive[] flowAdditive;

		/*#if UNITY_EDITOR || UNITY_MOBILE
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
		#endif*/

		[Header("Start Settings")]
		public bool showRootOnStart = true;

		private FlowData currentFlow;

		protected override void Init() {
			
			#region FLOW DEFAULT
			{
				var customDefaults = false;
				List<WindowItem> defaults = new List<WindowItem>();
				FlowData flow = null;
				for (int i = 0; i < this.flow.Length; ++i) {

					var item = this.flow[i];
					if (item.IsValid() == false) continue;

					flow = item.flow;
					if (item.HasCustomDefaults() == true) {

						if (item.IsDefaultsOverride() == true) {

							defaults.Clear();
							defaults.AddRange(item.customDefaults);

						} else {

							defaults.AddRange(item.customDefaults);

						}

						customDefaults = true;

					}

				}

				if (flow == null) {

					Debug.LogErrorFormat("Flow data was not found for current platform: {0}.", Application.platform);
					return;

				}

				this.currentFlow = flow;

                #if UNITY_EDITOR
                flow.RefreshAllScreens();
                #endif

                FlowSystem.SetData(flow);
				Audio.Manager.InitAndAdd(flow.audio);
                
				if (customDefaults == false) {

					this.defaults.AddRange(flow.GetDefaultScreens(runtime: true));

				} else {

					this.defaults.AddRange(defaults);

				}

				this.windows.AddRange(flow.GetAllScreens(runtime: true));

				this.rootScreen = flow.GetRootScreen(runtime: true);

			}
			#endregion

			#region FLOW ADDITIONAL
			{

				for (int i = 0; i < this.flowAdditive.Length; ++i) {

					var item = this.flowAdditive[i];
					if (item.IsValid() == false) continue;

					var additionalFlow = item.flow;
					var loadType = item.loadType;

                    #if UNITY_EDITOR
                    additionalFlow.RefreshAllScreens();
                    #endif

                    var screens = additionalFlow.GetAllScreens((w) => ((loadType & LoadType.Function) != 0 && (w.IsFunction() == true || w.GetFunctionContainer() != null)) || ((loadType & LoadType.Window) != 0 && w.IsFunction() == false), runtime: true);
					this.windows.RemoveAll(x => screens.Select(w => w.windowId).Contains(x.windowId));
					this.windows.AddRange(screens);
					if ((loadType & LoadType.Audio) != 0) Audio.Manager.InitAndAdd(additionalFlow.audio);

				}

			}
			#endregion

			base.Init();

		}

		public FlowData GetBaseFlow() {

			FlowData flow = null;
			for (int i = 0; i < this.flow.Length; ++i) {

				var item = this.flow[i];
				if (item.IsValid() == false) continue;

				flow = item.flow;

			}

			return flow;

		}

		public void Start() {
			
			this.OnStart();

		}
		
		public void OnStart() {

			if (this.showRootOnStart == true) {

				var root = this.currentFlow.GetRootScreen(runtime: true);
				if (root != null) WindowSystem.Show(root);

			}
			
		}

		public static T DoFlow<T>(IFunctionIteration screen, int from, int to, bool hide, System.Action<T> onParametersPassCall, System.Action<T> onInstance = null, bool async = false) where T : WindowBase {

			var item = UnityEngine.UI.Windows.Plugins.Flow.FlowSystem.GetAttachItem(from, to);
			return WindowSystemFlow.DoFlow<T>(screen, item, hide, false, async, onParametersPassCall, onInstance);

		}

		public static T DoFlow<T>(IFunctionIteration screen, int from, int to, bool hide, bool waitHide, System.Action<T> onParametersPassCall, System.Action<T> onInstance = null, bool async = false) where T : WindowBase {

			var item = UnityEngine.UI.Windows.Plugins.Flow.FlowSystem.GetAttachItem(from, to);
			return WindowSystemFlow.DoFlow<T>(screen, item, hide, waitHide, async, onParametersPassCall, onInstance);

		}

		public static T DoFlow<T>(IFunctionIteration screen, AttachItem item, bool hide, System.Action<T> onParametersPassCall, System.Action<T> onInstance = null, bool async = false) where T : WindowBase {

			return WindowSystemFlow.DoFlow<T>(screen, item, hide, false, async, onParametersPassCall, onInstance);

		}

		/// <summary>
		/// Dos the flow.
		/// </summary>
		/// <returns>The flow.</returns>
		/// <param name="screen">Screen.</param>
		/// <param name="item">Item.</param>
		/// <param name="hide">If set to <c>true</c> hide.</param>
		/// <param name="onParametersPassCall">On parameters pass call.</param>
		/// <param name="onInstance">On instance.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T DoFlow<T>(IFunctionIteration screen, AttachItem item, bool hide, bool waitHide, bool async, System.Action<T> onParametersPassCall, System.Action<T> onInstance = null) where T : WindowBase {

			if (async == true) {

				WindowSystem.ShowAsyncLoader();

			}

			T newWindow = default(T);

			if (hide == true && waitHide == true) {

				screen.Hide(() => {

					newWindow = WindowSystem.Show<T>(
						async: async,
						transitionItem: item,
						afterGetInstance: (w) => {

							if (onInstance != null) onInstance.Invoke(w);
							w.SetFunctionIterationIndex(screen.GetFunctionIterationIndex());

						}, onParametersPassCall: onParametersPassCall);
					
					UnityEngine.Events.UnityAction action = null;
					action = () => {

						newWindow.events.onEveryInstance.Unregister(WindowEventType.OnShowEnd, action);
						if (async == true) {

							WindowSystem.HideAsyncLoader();

						}

					};
					newWindow.events.onEveryInstance.Register(WindowEventType.OnShowEnd, action);

					WindowSystemFlow.OnDoTransition((item == null) ? 0 : item.index, screen.GetWindow(), (item == null) ? newWindow.windowId : item.targetId, hide);

				}, item);
					
			} else {

				newWindow = WindowSystem.Show<T>(
					async: async,
	                transitionItem: item,
	                afterGetInstance: (w) => {
						
						if (onInstance != null) onInstance.Invoke(w);
						w.SetFunctionIterationIndex(screen.GetFunctionIterationIndex());

					}, onParametersPassCall: onParametersPassCall);

				UnityEngine.Events.UnityAction action = null;
				action = () => {

					newWindow.events.onEveryInstance.Unregister(WindowEventType.OnShowEnd, action);
					if (async == true) {

						WindowSystem.HideAsyncLoader();

					}

				};
				newWindow.events.onEveryInstance.Register(WindowEventType.OnShowEnd, action);

				WindowSystemFlow.OnDoTransition((item == null) ? 0 : item.index, screen.GetWindow(), (item == null) ? newWindow.windowId : item.targetId, hide);

				if (hide == true) {

					screen.Hide(item);

				}

			}

			return newWindow;

		}

		public static void OnDoTransition(int index, WindowBase fromScreen, int targetId, bool hide = true) {

			if (fromScreen != null) WindowSystem.OnDoTransition(index, fromScreen.windowId, targetId, hide);
			
		}

		#if UNITY_EDITOR
		public override void OnValidate() {

			if (Application.isPlaying == true) return;
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			
			base.OnValidate();

			if (this.flow != null) {

				for (int i = 0; i < this.flow.Length; ++i) {

					this.flow[i].Validate();

				}

			}

			if (this.flowAdditive != null) {

				for (int i = 0; i < this.flowAdditive.Length; ++i) {

					this.flowAdditive[i].Validate();

				}

			}

		}
		#endif

	}

}
