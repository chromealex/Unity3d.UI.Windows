using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using ME;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows.Plugins.Flow {

	public enum CompletedState {
		NotReady,
		ReadyButWarnings,
		Ready,
	};

	[System.Serializable]
	public class FlowWindow {

		[System.Serializable]
		public struct ComponentLink {
			
			[Header("Base")]
			public int targetWindowId;
			public LayoutTag sourceComponentTag;
			#if UNITY_EDITOR
			[Header("Editor-Only")]
			public string comment;
			#endif

			public ComponentLink(int targetWindowId, LayoutTag sourceComponentTag, string comment) {

				this.targetWindowId = targetWindowId;
				this.sourceComponentTag = sourceComponentTag;
				#if UNITY_EDITOR
				this.comment = string.IsNullOrEmpty(comment) ? string.Empty : ("On " + comment.ToLower().UppercaseWords().Trim().Replace(" ", "") + " Action");
				#endif

			}

		}

		private const int STATES_COUNT = 3;

		public int id;
		public string title = string.Empty;
		public string directory = string.Empty;
		public Rect rect;
		public List<int> attaches;
		public List<ComponentLink> attachedComponents;
		public bool isContainer = false;
		public bool isDefaultLink = false;
		public Color randomColor;
		
		public List<int> tags = new List<int>();

		public bool compiled = false;
		public string compiledDirectory = string.Empty;
		public string compiledNamespace = string.Empty;
		public string compiledScreenName = string.Empty;
		public string compiledBaseClassName = string.Empty;
		public string compiledDerivedClassName = string.Empty;

		public CompletedState[] states = new CompletedState[STATES_COUNT];

		public WindowBase screen;

		public FlowWindow(int id, bool isContainer = false, bool isDefaultLink = false) {

			this.states = new CompletedState[STATES_COUNT];
			this.tags = new List<int>();

			this.id = id;
			this.attaches = new List<int>();
			this.attachedComponents = new List<ComponentLink>();
			this.rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 200f, 200f);
			this.isContainer = isContainer;
			this.isDefaultLink = isDefaultLink;
			this.title = (this.isContainer == true ? "Container" : "Window " + this.id.ToString());
			this.directory = (this.isContainer == true ? "ContainerDirectory" : "Window" + this.id.ToString() + "Directory");
			this.randomColor = ColorHSV.GetDistinctColor();

			this.compiled = false;

		}

		public bool IsEnabled() {

			return !FlowSystem.GetTags().Any((t) => this.tags.Contains(t.id) && t.enabled == false);

		}

		#if UNITY_EDITOR
		private IPreviewEditor editorCache;
		public bool OnPreviewGUI(Rect rect, GUIStyle background, bool drawInfo, bool selectable) {

			WindowLayoutElement.waitForComponentConnectionTemp = false;

			var screen = this.GetScreen() as LayoutWindowType;
			if (screen != null) {

				var layout = screen.layout.layout;
				if (layout != null) {

					if (this.editorCache == null) this.editorCache = UnityEditor.Editor.CreateEditor(layout) as IPreviewEditor;
					if (this.editorCache != null) this.editorCache.OnPreviewGUI(Color.white, rect, background, drawInfo, selectable);

					if (WindowLayoutElement.waitForComponentConnectionTemp == true) {

						return true;

					}

				}

			}

			return false;

		}
		#endif

		public void SetScreen(WindowBase screen) {

			this.screen = screen;

		}

		public WindowBase GetScreen() {
			
			return this.screen;
			
		}

		public WindowLayoutElement GetLayoutComponent(LayoutTag tag) {

			var screen = this.GetScreen() as LayoutWindowType;
			if (screen != null && screen.layout.layout != null) {
				
				return screen.layout.layout.GetRootByTag(tag);

			}

			return null;

		}

		public void AddTag(FlowTag tag) {
			
			if (this.tags.Contains(tag.id) == false) {

				this.tags.Add(tag.id);

			}

		}
		
		public void RemoveTag(FlowTag tag) {

			this.tags.Remove(tag.id);

		}

		public void SetCompletedState(int index, CompletedState state) {

			if (this.states == null || this.states.Length != STATES_COUNT) this.states = new CompletedState[STATES_COUNT];

			var oldState = this.states[index];
			if (oldState != state) {

				this.states[index] = state;
				FlowSystem.SetDirty();

			}

		}

		public bool IsValidToCompile() {

			return ME.Utilities.CacheByTime("FlowWindow." + this.id.ToString() + ".IsValidCompile", () => {

				var pattern = string.Empty;
				var directory = this.directory;
				if (this.isContainer == false) {

					pattern = @"^([A-Z]+[a-zA-Z0-9]*)$";

				} else {
					
					pattern = @"^([A-Z]+[a-zA-Z0-9]*)$";

				}
				
				Regex rgx = new Regex(pattern, RegexOptions.Singleline);
				return rgx.IsMatch(directory);

			});

		}

		public GUIStyle GetEditorStyle(bool selected) {

			if (this.isDefaultLink == true) {

				// Yellow

				var defaultLinkStyle = ME.Utilities.CacheStyle("FlowWindow.GetEditorStyle.DefaultLinkStyle.NotSelected", "flow node 4", (styleName) => {
					
					var _style = new GUIStyle(styleName);
					_style.padding = new RectOffset(0, 0, 14, 1);
					_style.contentOffset = new Vector2(0f, -15f);
					_style.fontStyle = FontStyle.Bold;
					_style.alignment = TextAnchor.MiddleCenter;
					_style.normal.textColor = Color.white;

					return _style;

				});

				var defaultLinkStyleSelected = ME.Utilities.CacheStyle("FlowWindow.GetEditorStyle.DefaultLinkStyle.Selected", "flow node 4 on", (styleName) => {
					
					var _style = new GUIStyle(styleName);
					_style.padding = new RectOffset(0, 0, 14, 1);
					_style.contentOffset = new Vector2(0f, -15f);
					_style.fontStyle = FontStyle.Bold;
					_style.alignment = TextAnchor.MiddleCenter;
					_style.normal.textColor = Color.white;
					
					return _style;
					
				});

				return selected ? defaultLinkStyleSelected : defaultLinkStyle;

			} else if (this.isContainer == true) {

				var styleNormal = string.Empty;
				//var styleSelected = string.Empty;

				// Compiled - Blue
				styleNormal = "flow node 0";
				//styleSelected = "flow node 0 on";

				if (this.IsValidToCompile() == false) {
					
					// Not Valid
					styleNormal = "flow node 6";
					//styleSelected = "flow node 6 on";
					
				}
				
				var containerStyle = ME.Utilities.CacheStyle("FlowWindow.GetEditorStyle.Container", styleNormal, (styleName) => {
					
					var _style = new GUIStyle(styleName);
					_style.padding = new RectOffset(0, 0, 16, 1);
					_style.contentOffset = new Vector2(0f, -15f);
					_style.fontStyle = FontStyle.Bold;
					_style.normal.textColor = Color.white;
					
					return _style;
					
				});

				return containerStyle;

			} else {

				var styleNormal = string.Empty;
				var styleSelected = string.Empty;

				//if (this.compiled == true) {

				if (FlowSystem.GetRootWindow() == this.id) {
					
					// Root - Orange
					styleNormal = "flow node 5";
					styleSelected = "flow node 5 on";

				} else if (FlowSystem.GetDefaultWindows().Contains(this.id) == true) {

					// Default - Cyan
					styleNormal = "flow node 2";
					styleSelected = "flow node 2 on";

				} else {

					// Compiled - Blue
					styleNormal = "flow node 1";
					styleSelected = "flow node 1 on";

				}

				/*} else {

					// Not Compiled - Gray
					styleNormal = "flow node 0";
					styleSelected = "flow node 0 on";

				}*/

				if (this.IsValidToCompile() == false) {

					// Not Valid
					styleNormal = "flow node 6";
					styleSelected = "flow node 6 on";

				}
				
				var windowStyle = ME.Utilities.CacheStyle("FlowWindow.GetEditorStyle.Window.Selected", styleNormal, (styleName) => {
					
					var _style = new GUIStyle(styleName);
					_style.fontStyle = FontStyle.Bold;
					_style.margin = new RectOffset(0, 0, 0, 0);
					_style.padding = new RectOffset(0, 0, 5, 4);
					_style.alignment = TextAnchor.UpperLeft;
					_style.contentOffset = new Vector2(5f, 0f);
					
					return _style;
					
				});
				
				var windowStyleSelected = ME.Utilities.CacheStyle("FlowWindow.GetEditorStyle.Window.NotSelected", styleSelected, (styleName) => {
					
					var _style = new GUIStyle(styleName);
					_style.fontStyle = FontStyle.Bold;
					_style.margin = new RectOffset(0, 0, 0, 0);
					_style.padding = new RectOffset(0, -1, 5, 4);
					_style.alignment = TextAnchor.UpperLeft;
					_style.contentOffset = new Vector2(5f, 0f);
					
					return _style;
					
				});

				return selected ? windowStyleSelected : windowStyle;

			}

		}

		public void Move(Vector2 delta) {
			
			this.rect.x += delta.x;
			this.rect.y += delta.y;
			
			//this.rect = FlowSystem.Grid(this.rect);
			
		}
		
		public FlowWindow GetContainer() {
			
			return ME.Utilities.CacheByFrame("FlowWindow." + this.id.ToString() + ".GetContainer", () => FlowSystem.GetWindow(this.attaches.FirstOrDefault((id) => FlowSystem.GetWindow(id).isContainer)));
			
		}
		
		public bool HasContainer() {
			
			return this.attaches.Any((id) => FlowSystem.GetWindow(id).isContainer);
			
		}
		
		public bool HasContainer(FlowWindow predicate) {
			
			return this.attaches.Any((id) => id == predicate.id && FlowSystem.GetWindow(id).isContainer);
			
		}

		public List<FlowWindow> GetAttachedWindows() {
			
			List<FlowWindow> output = new List<FlowWindow>();
			foreach (var attachId in this.attaches) {
				
				var window = FlowSystem.GetWindow(attachId);
				if (window.isContainer == true) continue;
				
				output.Add(window);
				
			}
			
			return output;
			
		}

		#if UNITY_EDITOR
		public bool AlreadyAttached(int id, WindowLayoutElement component = null) {
			
			if (component != null) {
				
				return this.attachedComponents.Any((c) => c.targetWindowId == id && c.sourceComponentTag == component.tag);
				
			}
			
			return this.attaches.Contains(id);
			
		}

		public bool Attach(int id, bool oneWay = false, WindowLayoutElement component = null) {
			
			if (this.id == id) return false;

			var result = false;

			if (component != null) {

				if (this.attachedComponents.Any((c) => c.targetWindowId == id && c.sourceComponentTag == component.tag) == false) {

					this.attachedComponents.Add(new ComponentLink(id, component.tag, component.comment));

					// If we attaching component - try to attach window if not

					oneWay = true;
					result = true;

				} else {

					return false;

				}

			}

			if (this.attaches.Contains(id) == false) {
				
				this.attaches.Add(id);
				
				if (oneWay == false) {
					
					var window = FlowSystem.GetWindow(id);
					window.Attach(this.id, oneWay: true);
					
				}
				
				return true;
				
			}
			
			return result;
			
		}
		
		public bool Detach(int id, bool oneWay = false, WindowLayoutElement component = null) {
			
			if (this.id == id) return false;
			
			var result = false;

			if (component != null) {

				if (this.attachedComponents.Any((c) => c.targetWindowId == id && c.sourceComponentTag == component.tag) == true) {

					this.attachedComponents.RemoveAll((c) => c.targetWindowId == id && c.sourceComponentTag == component.tag);

					result = true;

				}

			} else {

				if (this.attaches.Contains(id) == true) {
					
					this.attaches.Remove(id);
					this.attachedComponents.RemoveAll((c) => c.targetWindowId == id);
					
					result = true;
					
				}
				
				if (oneWay == false) {
					
					var window = FlowSystem.GetWindow(id);
					if (window != null) result = window.Detach(this.id, oneWay: true);
					
				}

			}

			return result;
			
		}
		#endif
		
	}

}