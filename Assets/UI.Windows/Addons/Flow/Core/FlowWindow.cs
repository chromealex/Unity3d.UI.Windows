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
	public class DefaultElement {

		// Where is the element located?
		public LayoutTag tag;
		public string comment;

		// Whats the element?
		public WindowComponentLibraryLinker library;
		public string elementPath;
		public WindowComponent elementComponent;

	}

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

		public enum Flags : int {

			Default = 0x0,

			IsContainer = 0x1,
			IsSmall = 0x2,
			
			CantCompiled = 0x4,
			
			ShowDefault = 0x8,
			IsFunction = 0x10,
			Reserved2 = 0x20,
			Reserved3 = 0x40,
			Reserved4 = 0x80,
			
			Tag1 = 0x100,
			Tag2 = 0x200,
			Tag3 = 0x400,
			Tag4 = 0x800,
			Tag5 = 0x1000,
			Tag6 = 0x2000,
			Tag7 = 0x4000,
			Tag8 = 0x8000,
			Tag9 = 0x10000,
			Tag10 = 0x20000,
			Tag11 = 0x40000,
			Tag12 = 0x80000,

		};

		public enum StoreType : byte {

			NewScreen,
			ReUseScreen,

		};

		public int id;
		[BitMask(typeof(Flags))]
		public Flags flags;
		public string title = string.Empty;
		public string directory = string.Empty;
		public Rect rect;
		public List<int> attaches;
		public List<ComponentLink> attachedComponents;
		public Color randomColor;
		
		public int functionRootId = 0;
		public int functionExitId = 0;
		public int functionId = 0;

		public StoreType storeType = StoreType.NewScreen;

		[System.Obsolete("Bool isContainer does not exists anymore. Use Flags.IsContainer instead.")]
		public bool isContainer = false;
		[System.Obsolete("Bool isDefaultLink does not exists anymore. Use Flags.IsSmall | Flags.NotCompiled | Flags.ShowDefault instead.")]
		public bool isDefaultLink = false;

		public List<int> tags = new List<int>();

		public List<DefaultElement> comments = new List<DefaultElement>();

		public bool compiled = false;
		public string compiledDirectory = string.Empty;
		public string compiledNamespace = string.Empty;
		public string compiledScreenName = string.Empty;
		public string compiledBaseClassName = string.Empty;
		public string compiledDerivedClassName = string.Empty;
		
		public string smallStyleDefault = "flow node 4";
		public string smallStyleSelected = "flow node 4 on";

		public CompletedState[] states = new CompletedState[STATES_COUNT];

		public int screenWindowId;	// used when storeType == ReUseScreen
		[SerializeField]
		private WindowBase screen;

		public FlowWindow(int id, bool isContainer = false, bool isDefaultLink = false, FlowWindow.Flags flags = Flags.Default) {
			
			if (isContainer == true) {
				
				flags |= Flags.IsContainer;
				
			}
			
			if (isDefaultLink == true) {
				
				flags |= Flags.IsSmall;
				flags |= Flags.CantCompiled;
				flags |= Flags.ShowDefault;
				
			}

			this.Init(id, flags);

		}
		
		public FlowWindow(int id, FlowWindow.Flags flags) {

			this.Init(id, flags: flags);

		}

		public FlowWindow GetFunctionContainer() {

			// If current window attached to function
			var attaches = this.attaches;
			foreach (var attach in attaches) {
				
				var win = FlowSystem.GetWindow(attach);
				if (win.IsContainer() == true && win.IsFunction() == true) {
					
					// We are inside a function
					return win;
					
				}
				
			}

			return null;

		}

		public int GetFunctionId() {

			return this.functionId;

		}

		public bool IsFunction() {

			return (this.flags & Flags.IsFunction) != 0;
			
		}

		public bool IsContainer() {

			// For old version compability only
			#pragma warning disable 618
			if (this.isContainer == true) this.flags |= Flags.IsContainer;
			#pragma warning restore 618

			return (this.flags & Flags.IsContainer) != 0;
			
		}

		public bool IsSmall() {
			
			// For old version compability only
			#pragma warning disable 618
			if (this.isDefaultLink == true) {

				this.flags |= Flags.IsSmall;
				this.flags |= Flags.CantCompiled;
				this.flags |= Flags.ShowDefault;
				
			}
			#pragma warning restore 618

			return (this.flags & Flags.IsSmall) != 0;
			
		}

		public bool IsShowDefault() {
			
			// For old version compability only
			#pragma warning disable 618
			if (this.isDefaultLink == true) {
				
				this.flags |= Flags.IsSmall;
				this.flags |= Flags.CantCompiled;
				this.flags |= Flags.ShowDefault;
				
			}
			#pragma warning restore 618

			return (this.flags & Flags.ShowDefault) != 0;

		}
		
		public bool CanCompiled() {
			
			return (this.flags & Flags.CantCompiled) == 0;
			
		}

		public void Init(int id, FlowWindow.Flags flags = Flags.Default) {
			
			this.states = new CompletedState[STATES_COUNT];
			this.tags = new List<int>();
			
			this.id = id;
			this.flags = flags;
			this.attaches = new List<int>();
			this.attachedComponents = new List<ComponentLink>();
			this.rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 200f, 200f);
			this.title = (this.IsContainer() == true ? "Container" : "Window " + this.id.ToString());
			this.directory = (this.IsContainer() == true ? "ContainerDirectory" : "Window" + this.id.ToString() + "Directory");
			this.randomColor = ColorHSV.GetDistinctColor();
			
			this.smallStyleDefault = "flow node 4";
			this.smallStyleSelected = "flow node 4 on";

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

			if (this.storeType == StoreType.ReUseScreen) {

				var win = FlowSystem.GetWindow(this.screenWindowId);
				if (win == null) return null;

				return win.GetScreen();

			}

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
				if (this.IsContainer() == false) {

					pattern = @"^([A-Z]+[a-zA-Z0-9]*)$";

				} else {
					
					pattern = @"^([A-Z]+[a-zA-Z0-9]*)$";

				}
				
				Regex rgx = new Regex(pattern, RegexOptions.Singleline);
				return rgx.IsMatch(directory);

			});

		}

		public GUIStyle GetEditorStyle(bool selected) {

			if (this.IsSmall() == true) {

				// Yellow
				
				if (string.IsNullOrEmpty(this.smallStyleDefault) == true) this.smallStyleDefault = "flow node 4";
				if (string.IsNullOrEmpty(this.smallStyleSelected) == true) this.smallStyleSelected = "flow node 4 on";

				var style = ME.Utilities.CacheStyle("FlowWindow.GetEditorStyle.SmallStyle.NotSelected", this.smallStyleDefault, (styleName) => {
					
					var _style = new GUIStyle(styleName);
					_style.padding = new RectOffset(0, 0, 14, 1);
					_style.contentOffset = new Vector2(0f, -15f);
					_style.fontStyle = FontStyle.Bold;
					_style.alignment = TextAnchor.UpperCenter;
					_style.normal.textColor = Color.black;

					return _style;

				});

				var styleSelected = ME.Utilities.CacheStyle("FlowWindow.GetEditorStyle.SmallStyle.Selected", this.smallStyleSelected, (styleName) => {
					
					var _style = new GUIStyle(styleName);
					_style.padding = new RectOffset(0, 0, 14, 1);
					_style.contentOffset = new Vector2(0f, -15f);
					_style.fontStyle = FontStyle.Bold;
					_style.alignment = TextAnchor.UpperCenter;
					_style.normal.textColor = Color.black;
					
					return _style;
					
				});

				return selected ? styleSelected : style;

			} else if (this.IsContainer() == true) {

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

				var functionWindow = this.GetFunctionContainer();
				var isFunction = functionWindow != null;
				var isRoot = (isFunction == true && functionWindow.functionRootId == this.id);
				var isExit = (isFunction == true && functionWindow.functionExitId == id);
				if (FlowSystem.GetRootWindow() == this.id || (isFunction == true && (isRoot == true || isExit == true))) {

					if (isFunction == true && isExit == true) {

						// Function exit point - Green
						styleNormal = "flow node 3";
						styleSelected = "flow node 3 on";

					} else if (isFunction == true && isRoot == true) {

						// Function root - Yellow
						styleNormal = "flow node 4";
						styleSelected = "flow node 4 on";

					} else {

						// Root - Orange
						styleNormal = "flow node 5";
						styleSelected = "flow node 5 on";

					}

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
			
			return ME.Utilities.CacheByFrame("FlowWindow." + this.id.ToString() + ".GetContainer", () => FlowSystem.GetWindow(this.attaches.FirstOrDefault((id) => FlowSystem.GetWindow(id).IsContainer())));
			
		}
		
		public bool HasContainer() {
			
			return this.attaches.Any((id) => FlowSystem.GetWindow(id).IsContainer());
			
		}
		
		public bool HasContainer(FlowWindow predicate) {
			
			return this.attaches.Any((id) => id == predicate.id && FlowSystem.GetWindow(id).IsContainer());
			
		}

		public List<FlowWindow> GetAttachedWindows() {
			
			List<FlowWindow> output = new List<FlowWindow>();
			foreach (var attachId in this.attaches) {
				
				var window = FlowSystem.GetWindow(attachId);
				if (window.IsContainer() == true) continue;
				
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