using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace UnityEngine.UI.Windows.Plugins.Flow {

	public enum CompletedState {
		NotReady,
		ReadyButWarnings,
		Ready,
	};

	[System.Serializable]
	public class FlowWindow {

		private const int STATES_COUNT = 5;
		
		public bool isDefault;
		public int defaultIndex;

		public int id;
		public string title = string.Empty;
		public string directory = string.Empty;
		public Rect rect;
		public List<int> attaches;
		public bool isContainer = false;
		public Color randomColor;

		public bool compiled = false;
		public string compiledDirectory = string.Empty;
		public string compiledNamespace = string.Empty;
		public string compiledScreenName = string.Empty;
		public string compiledClassName = string.Empty;

		public CompletedState[] states = new CompletedState[STATES_COUNT];

		public FlowWindow(int id, bool isContainer) {

			this.states = new CompletedState[STATES_COUNT];

			this.id = id;
			this.attaches = new List<int>();
			this.rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 200f, 200f);
			this.isContainer = isContainer;
			this.title = (this.isContainer == true ? "Container" : "Window " + this.id.ToString());
			this.directory = (this.isContainer == true ? "ContainerDirectory" : "Window" + this.id.ToString() + "Directory");
			this.randomColor = ColorHSV.GetDistinctColor();

			this.compiled = false;

		}

		public void SetCompletedState(int index, CompletedState state) {

			if (this.states == null || this.states.Length != STATES_COUNT) this.states = new CompletedState[STATES_COUNT];

			var oldState = this.states[index];
			if (oldState != state) {

				this.states[index] = state;
				FlowSystem.GetData().isDirty = true;

			}

		}

		public bool IsValidToCompile() {
			
			var pattern = string.Empty;
			var directory = this.directory;
			if (this.isContainer == false) {

				pattern = @"^([A-Z]+[a-zA-Z0-9]*)$";

			} else {
				
				pattern = @"^([A-Z]+[a-zA-Z0-9]*)$";

			}
			
			Regex rgx = new Regex(pattern, RegexOptions.Singleline);
			return rgx.IsMatch(directory);

		}

		public GUIStyle GetEditorStyle(bool selected) {

			if (this.isContainer == true) {

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
				
				var containerStyle = new GUIStyle(GUI.skin.FindStyle(styleNormal));
				containerStyle.padding = new RectOffset(0, 0, 16, 1);
				containerStyle.contentOffset = new Vector2(0f, -15f);
				containerStyle.fontStyle = FontStyle.Bold;
				containerStyle.normal.textColor = Color.white;

				return containerStyle;

			} else {

				var styleNormal = string.Empty;
				var styleSelected = string.Empty;

				if (this.compiled == true) {

					// Compiled - Blue
					styleNormal = "flow node 1";
					styleSelected = "flow node 1 on";

				} else {

					// Not Compiled - Gray
					styleNormal = "flow node 0";
					styleSelected = "flow node 0 on";

				}

				if (this.IsValidToCompile() == false) {

					// Not Valid
					styleNormal = "flow node 6";
					styleSelected = "flow node 6 on";

				}

				var windowStyle = new GUIStyle(styleNormal);
				windowStyle.fontStyle = FontStyle.Bold;
				windowStyle.margin = new RectOffset(0, 0, 0, 0);
				windowStyle.padding = new RectOffset(0, 0, 5, 4);
				windowStyle.alignment = TextAnchor.UpperLeft;
				windowStyle.contentOffset = new Vector2(5f, 0f);

				var windowStyleSelected = new GUIStyle(styleSelected);
				windowStyleSelected.fontStyle = FontStyle.Bold;
				windowStyleSelected.margin = new RectOffset(0, 0, 0, 0);
				windowStyleSelected.padding = new RectOffset(1, -1, 5, 4);
				windowStyleSelected.alignment = TextAnchor.UpperLeft;
				windowStyleSelected.contentOffset = new Vector2(5f, 0f);

				return selected ? windowStyleSelected : windowStyle;

			}

		}

		public void Move(Vector2 delta) {
			
			this.rect.x += delta.x;
			this.rect.y += delta.y;
			
			//this.rect = FlowSystem.Grid(this.rect);
			
		}
		
		public FlowWindow GetContainer() {
			
			return FlowSystem.GetWindow(this.attaches.FirstOrDefault((id) => FlowSystem.GetWindow(id).isContainer));
			
		}
		
		public bool HasContainer() {
			
			return this.attaches.Any((id) => FlowSystem.GetWindow(id).isContainer);
			
		}
		
		public bool HasContainer(FlowWindow predicate) {
			
			return this.attaches.Any((id) => id == predicate.id && FlowSystem.GetWindow(id).isContainer);
			
		}
		
		public bool AlreadyAttached(int id) {
			
			return this.attaches.Contains(id);
			
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
		
		public bool Attach(int id, bool oneWay = false) {
			
			if (this.id == id) return false;
			
			if (this.attaches.Contains(id) == false) {
				
				this.attaches.Add(id);
				
				if (oneWay == false) {
					
					var window = FlowSystem.GetWindow(id);
					window.Attach(this.id, oneWay: true);
					
				}
				
				return true;
				
			}
			
			return false;
			
		}
		
		public bool Detach(int id, bool oneWay = false) {
			
			if (this.id == id) return false;
			
			var result = false;
			
			if (this.attaches.Contains(id) == true) {
				
				this.attaches.Remove(id);
				
				result = true;
				
			}
			
			if (oneWay == false) {
				
				var window = FlowSystem.GetWindow(id);
				if (window != null) result = window.Detach(this.id, oneWay: true);
				
			}
			
			return result;
			
		}
		
	}

}