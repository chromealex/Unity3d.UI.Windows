using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.Flow {
	
	class ColorHSV
	{
		private float h;
		private float s;
		private float v;
		private float a;
		
		/**
		    * Construct without alpha (which defaults to 1)
		    */
		
		public ColorHSV (float h ,float s ,float v){
			this.h = h;
			this.s = s;
			this.v = v;
			this.a = 1.0f;
		}
		
		/**
		    * Construct with alpha
		    */
		
		public ColorHSV (float h , float s, float v,float a){
			this.h = h;
			this.s = s;
			this.v = v;
			this.a = a;
		}
		
		/**
		    * Create from an RGBA color object
		    */
		
		public ColorHSV ( Color color  ){
			float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
			float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
			float delta = max - min;
			// value is our max color
			this.v = max;
			// saturation is percent of max
			
			if(!Mathf.Approximately(max, 0)){
				this.s = delta / max;
			}else{
				// all colors are zero, no saturation and hue is undefined
				this.s = 0;
				this.h = -1;
				return;
			}
			
			// grayscale image if min and max are the same
			if(Mathf.Approximately(min, max)){
				this.v = max;
				this.s = 0;
				this.h = -1;
				return;
			}
			// hue depends which color is max (this creates a rainbow effect)
			if( color.r == max ){
				this.h = ( color.g - color.b ) / delta;         // between yellow  magenta
			}else if( color.g == max ){
				this.h = 2 + ( color.b - color.r ) / delta; // between cyan  yellow
			}else{
				this.h = 4 + ( color.r - color.g ) / delta; // between magenta  cyan
			}
			
			// turn hue into 0-360 degrees
			this.h *= 60;
			if(this.h < 0 ){
				this.h += 360;
			}
		}
		
		/**
		    * Return an RGBA color object
		    */
		
		public Color ToColor (){
			// no saturation, we can return the value across the board (grayscale)
			if(this.s == 0 ){
				return new Color(this.v, this.v, this.v, this.a);
			}
			// which chunk of the rainbow are we in?
			float sector = this.h / 60;
			// split across the decimal (ie 3.87f into 3 and 0.87f)
			int i;
			i = (int)Mathf.Floor(sector);
			float f = sector - i;
			float v = this.v;
			float p = v * ( 1 - s );
			float q = v * ( 1 - s * f );
			float t = v * ( 1 - s * ( 1 - f ) );
			// build our rgb color
			Color color = new Color(0, 0, 0, this.a);
			switch(i){
			case 0:
				color.r = v;
				color.g = t;
				color.b = p;
				break;
			case 1:
				color.r = q;
				color.g = v;
				color.b = p;
				break;
			case 2:
				color.r  = p;
				color.g  = v;
				color.b  = t;
				break;
			case 3:
				color.r  = p;
				color.g  = q;
				color.b  = v;
				break;
			case 4:
				color.r  = t;
				color.g  = p;
				color.b  = v;
				break;
			default:
				color.r  = v;
				color.g  = p;
				color.b  = q;
				break;
			}
			return color;
		}
		
		public static Color GetRandomColor(float h ,float s ,float v){
			ColorHSV col = new ColorHSV(h,s,v);
			return col.ToColor();
		}
		
		public static Color GetDistinctColor() {
			
			return ColorHSV.GetRandomColor(Random.Range(0.0f, 360f), 1, 1);
			                               
		}

	}

	[System.Serializable]
	public class FlowWindow {

		public int id;
		public string title;
		public Rect rect;
		public List<int> attaches;
		public bool isContainer = false;
		public Color randomColor;

		public FlowWindow(int id, bool isContainer) {

			this.id = id;
			this.attaches = new List<int>();
			this.rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 200f, 200f);
			this.isContainer = isContainer;
			this.title = (this.isContainer == true ? "Container" : "Window " + this.id.ToString());
			this.randomColor = ColorHSV.GetDistinctColor();

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

	public class FlowSystem {

		public FlowData data;

		private static FlowSystem _instance;
		private static FlowSystem instance {

			get {

				if (FlowSystem._instance == null) FlowSystem._instance = new FlowSystem();
				return FlowSystem._instance;

			}

		}
		
		public static Vector2 grid;

		public static Rect Grid(Rect rect) {
			
			rect.x = Mathf.Round(rect.x / FlowSystem.grid.x) * FlowSystem.grid.x;
			rect.y = Mathf.Round(rect.y / FlowSystem.grid.y) * FlowSystem.grid.y;
			
			return rect;
			
		}

		public static void Save() {

			FlowSystem.instance.data.Save();

		}

		public static FlowData GetData() {
			
			return FlowSystem.instance.data;
			
		}

		public static void SetData(FlowData data) {

			FlowSystem.instance.data = data;

		}

		public static bool HasData() {

			return FlowSystem.instance.data != null;

		}
		
		public static IEnumerable<FlowWindow> GetWindows() {
			
			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetWindows();
			
		}
		
		public static IEnumerable<FlowWindow> GetContainers() {

			if (FlowSystem.HasData() == false) return null;

			return FlowSystem.instance.data.GetContainers();
			
		}

		public static FlowWindow GetWindow(int id) {

			return FlowSystem.instance.data.GetWindow(id);

		}
		
		public static FlowWindow CreateContainer() {
			
			return FlowSystem.instance.data.CreateContainer();
			
		}

		public static FlowWindow CreateWindow() {

			return FlowSystem.instance.data.CreateWindow();

		}

		public static void DestroyWindow(int id) {

			FlowSystem.instance.data.DestroyWindow(id);

		}
		
		public static void Attach(int source, int other, bool oneWay) {

			FlowSystem.instance.data.Attach(source, other, oneWay);

		}
		
		public static void Detach(int source, int other, bool oneWay) {
			
			FlowSystem.instance.data.Detach(source, other, oneWay);

		}
		
		public static bool AlreadyAttached(int source, int other) {
			
			return FlowSystem.instance.data.AlreadyAttached(source, other);

		}
		
		public static void SetScrollPosition(Vector2 pos) {
			
			if (FlowSystem.HasData() == false) return;

			FlowSystem.instance.data.SetScrollPosition(pos);
			
		}
		
		public static Vector2 GetScrollPosition() {

			if (FlowSystem.HasData() == false) return Vector2.zero;

			return FlowSystem.instance.data.GetScrollPosition();
			
		}

		public static void MoveContainerOrWindow(int id, Vector2 delta) {

			var window = FlowSystem.GetWindow(id);
			if (window.isContainer == true) {

				var childs = window.attaches;
				foreach (var child in childs) {

					FlowSystem.MoveContainerOrWindow(child, delta);

				}

			} else {
				
				window.Move(delta);

			}

		}

		public static void SelectWindowsInRect(Rect rect, System.Func<FlowWindow, bool> predicate = null) {

			if (FlowSystem.HasData() == false) return;

			FlowSystem.instance.data.SelectWindowsInRect(rect, predicate);

		}

		public static List<int> GetSelected() {

			return FlowSystem.instance.data.GetSelected();

		}

		public static void ResetSelection() {

			FlowSystem.instance.data.ResetSelection();

		}

	}

}