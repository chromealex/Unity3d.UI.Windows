using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Heatmap.Events;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Core {

	public class HeatmapSettings : ScriptableObject {
		
		[System.Serializable]
		public struct Point {

			[System.Serializable]
			public struct ComponentInfo {
				
				[Header("Tag")]
				public LayoutTag tag;

				[Header("Coords (Absolute)")]
				public float x;
				public float y;
				public float w;
				public float h;

				public ComponentInfo(LayoutTag tag, WindowComponent component) {
					
					this.tag = tag;

					this.x = 0f;
					this.y = 0f;
					this.w = 0f;
					this.h = 0f;

					//var rect = (component.transform as RectTransform).rect;

					var corners = new Vector3[4];
					(component.transform as RectTransform).GetWorldCorners(corners);

					var leftBottom = this.GetScreenPoint(component, corners[0]);
					var topRight = this.GetScreenPoint(component, corners[2]);

					this.x = leftBottom.x;
					this.y = leftBottom.y;
					this.w = topRight.x - leftBottom.x;
					this.h = topRight.y - leftBottom.y;

				}

				private Vector3 GetScreenPoint(WindowComponent component, Vector3 worldPoint) {

					return component.GetWindow().workCamera.WorldToScreenPoint(worldPoint);

				}

			}

			[Header("Coords (Normalized)")]
			// Normalized (x, y) coords
			public float x;
			public float y;
			
			[Header("Container")]
			public ComponentInfo componentInfo;

			public Point(Vector2 point, LayoutTag tag, WindowComponent component) {
				
				this.x = point.x;
				this.y = point.y;

				this.componentInfo = new ComponentInfo(tag, component);
				
			}
			
			public float GetAbsoluteX(float screenWidth) {
				
				// Container offset + unnormalized current point (x, y)
				return this.componentInfo.x + this.x * this.componentInfo.w;
				
			}
			
			public float GetAbsoluteY(float screenHeight) {
				
				// Container offset + unnormalized current point (x, y)
				return this.componentInfo.y + this.y * this.componentInfo.h;
				
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
				public Status status = Status.NoData;
				public Texture2D texture = null;

				public Vector2 size;
				public List<Point> points = new List<Point>();

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

					this.texture = HeatmapVisualizer.Create(this.texture, this.GetPoints(), this.size);
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