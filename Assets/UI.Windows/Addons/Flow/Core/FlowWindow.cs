using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Flow {

	[System.Serializable]
	public class FlowWindow {
		
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
		
		public FlowWindow(int id, bool isContainer) {
			
			this.id = id;
			this.attaches = new List<int>();
			this.rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 200f, 200f);
			this.isContainer = isContainer;
			this.title = (this.isContainer == true ? "Container" : "Window " + this.id.ToString());
			this.directory = (this.isContainer == true ? "Container Directory" : "Window " + this.id.ToString() + " Directory");
			this.randomColor = ColorHSV.GetDistinctColor();

			this.compiled = false;

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