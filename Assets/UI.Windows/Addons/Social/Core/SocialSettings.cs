using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEngine.UI.Windows.Plugins.Social.Core {
	
	[System.Serializable]
	public class Platform {
		
		public bool active;
		public ModuleSettings settings;
		
		public string GetPlatformName() {
			
			return this.settings.GetPlatformName();
			
		}
		
		public string GetPlatformClassName() {
			
			return this.settings.GetPlatformClassName();
			
		}
		
	}

	public class SocialSettings : ScriptableObject {

		[System.Serializable]
		public class WindowsData {

			[System.Serializable]
			public class Window {

				public int id;
				public ModuleSettings settings;

				public Window(FlowWindow source) {

					this.id = source.id;

				}

			}

			public List<Window> list = new List<Window>();

			public Window Get(FlowWindow window) {

				Window result = null;
				
				this.list.RemoveAll((info) => {
					
					var w = Flow.FlowSystem.GetWindow(info.id);
					return w == null || w.IsSocial() == false;
					
				});

				if (window.IsSocial() == false) return result;

				foreach (var item in this.list) {

					if (item.id == window.id) {

						result = item;
						break;

					}

				}

				if (result == null) {

					result = new Window(window);
					this.list.Add(result);

				}

				return result;

			}

		}

		public FlowWindow.Flags uniqueTag = FlowWindow.Flags.Tag1;

		public Platform[] activePlatforms;

		public WindowsData data;

		public bool IsPlatformActive(ModuleSettings settings) {

			foreach (var item in this.activePlatforms) {

				if (item.settings == settings) return item.active;

			}

			return false;

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Social/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<SocialSettings>();
			
		}
		#endif

	}

}