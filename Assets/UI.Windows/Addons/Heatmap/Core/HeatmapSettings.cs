using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Heatmap.Events;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Core {

	public class HeatmapSettings : ScriptableObject {
		
		[System.Serializable]
		public struct Point {

			[Header("Coords (Normalized)")]
			// Normalized (x, y) coords
			public float x;
			public float y;
			
			[Header("Container Tag")]
			public LayoutTag tag;

			public Point(Vector2 point, LayoutTag tag, WindowComponent component) {
				
				this.x = point.x;
				this.y = point.y;

				this.tag = tag;
				
			}
			
			public float GetAbsoluteX(WindowsData.Window window, float screenWidth) {

				var rect = this.GetRect(window);

				// Container offset + unnormalized current point (x, y)
				return Mathf.Lerp(this.x, rect.x, rect.x + rect.width);
				
			}
			
			public float GetAbsoluteY(WindowsData.Window window, float screenHeight) {
				
				var rect = this.GetRect(window);

				// Container offset + unnormalized current point (x, y)
				return Mathf.Lerp(this.y, rect.y, rect.y + rect.height);
				
			}

			private Rect GetRect(WindowsData.Window window) {
				
				var win = FlowSystem.GetWindow(window.id);
				var screen = win.GetScreen() as LayoutWindowType;
				var container = screen.GetLayoutContainer(this.tag);
				return container.editorRect;

			}

		};

		[System.Serializable]
		public class WindowsData {
			
			[System.Serializable]
			public class Window {

				public enum Status : byte {

					NoData = 0,
					Loading = 1,
					Loaded = 2,

				}

				public int id;
				public Vector2 size;
				public List<Point> points = new List<Point>();

				public Status status = Status.NoData;
				public Texture2D texture = null;
				public bool changed = false;

				public Window(Flow.Data.FlowWindow source) {
					
					this.id = source.id;
					
				}

				public void AddPoint(Vector2 point, LayoutTag tag, WindowComponent component) {

					this.points.Add(new Point(point, tag, component));
					this.changed = true;

					//this.UpdateMap();

				}

				public List<Point> GetPoints() {

					return this.points;

				}

				public void Serialize() {

				}

				public void Deserialize() {

				}

				public void UpdateMap() {

					if (this.texture == null) this.texture = new Texture2D((int)this.size.x, (int)this.size.y, TextureFormat.ARGB32, false);
					
					if (this.changed == false) return;

					this.texture = HeatmapVisualizer.Create(this.texture, this, this.GetPoints(), this.size);
					this.changed = false;

				}
				
				public void SetChanged() {

					this.changed = true;

				}

			}
			
			public List<Window> list = new List<Window>();

			public Window Get(Flow.Data.FlowWindow window) {
				
				Window result = null;
				
				this.list.RemoveAll((info) => {
					
					var w = Flow.FlowSystem.GetWindow(info.id);
					return w == null;
					
				});

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
			
			public void SetChanged() {
				
				foreach (var item in this.list) item.SetChanged();
				
			}

		}

		public bool show;
		public string authKey;

		public WindowsData data;

		public void SetChanged() {

			this.data.SetChanged();

		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/UI Windows/Heatmap/Settings")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<HeatmapSettings>();
			
		}
		#endif

	}

}