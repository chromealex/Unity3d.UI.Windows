using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Heatmap.Events;
using UnityEngine.UI.Windows.Types;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.Analytics;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Core {

	public struct Vector2Int {

		public int x;
		public int y;

		public Vector2Int(int x, int y) {

			this.x = x;
			this.y = y;

		}

		public override bool Equals(System.Object obj) {
			// If parameter is null return false.
			if (obj == null) {
				return false;
			}
			
			// If parameter cannot be cast to Point return false.
			var p = (Vector2Int)obj;
			
			// Return true if the fields match:
			return (x == p.x) && (y == p.y);
		}

		public bool Equals(Vector2Int p) {
			// If parameter is null return false:
			if ((object)p == null) {
				return false;
			}
			
			// Return true if the fields match:
			return (x == p.x) && (y == p.y);
		}
		
		public override int GetHashCode() {
			return x ^ y;
		}

		public static bool operator ==(Vector2Int a, Vector2Int b) {
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b)) {
				return true;
			}
			
			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null)) {
				return false;
			}
			
			// Return true if the fields match:
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(Vector2Int a, Vector2Int b) {
			return !(a == b);
		}

	}

	public class HeatmapSettings : ServiceSettings {
		
		[System.Serializable]
		public struct Point {
			
			[Header("Screen Info")]
			public int screenId;
			public int screenWidth;
			public int screenHeight;
			
			[Header("Container Tag")]
			public LayoutTag tag;

			[Header("Coords (Normalized)")]
			// Normalized (x, y) coords
			public float x;
			public float y;

			public Point(Vector2 point, Vector2Int screenSize, int screenId, LayoutTag tag, WindowComponent component) {

				this.screenId = screenId;
				this.screenWidth = screenSize.x;
				this.screenHeight = screenSize.y;

				this.x = point.x;
				this.y = point.y;

				this.tag = tag;
				
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
				public List<Point> points = new List<Point>();

				// Non serialized:
				public Status status = Status.NoData;
				public Texture2D texture = null;
				public bool changed = false;

				public Window(Flow.Data.FlowWindow source) {
					
					this.id = source.id;
					
				}

				public void AddPoint(Vector2 point, Vector2Int screenSize, int screenId, LayoutTag tag, WindowComponent component) {

					var newPoint = new Point(point, screenSize, screenId, tag, component);
					this.points.Add(newPoint);
					this.changed = true;

					Analytics.Analytics.SendScreenPoint(screenId, newPoint.screenWidth, newPoint.screenHeight, (byte)newPoint.tag, newPoint.x, newPoint.y);

				}

				public List<Point> GetPoints() {

					return this.points;

				}

				public void UpdateMap(Vector2 size) {

					this.status = HeatmapSettings.WindowsData.Window.Status.Loaded;
					
					if (this.texture == null) this.texture = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, false);
					
					if (this.changed == false) return;

					//this.texture = HeatmapVisualizer.Create(this.texture, this, this.GetPoints(), size);
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

			public void UpdateMap() {
				
				/*#if UNITY_EDITOR
				string ids = "[";
				for (int i = 0, imax = this.list.Count - 1; i <= imax; ++i) {
					ids += this.list[i].id.ToString();
					if (i < imax) ids += ",";
				}
				ids += "]";

				var settings = ME.EditorUtilities.GetAssetsOfType<HeatmapSettings>(useCache: false).FirstOrDefault();

				HeatmapSystem.sender.Post("http://localhost:8080/hm_load", new Dictionary<string, string>() {
					{"key", settings.authKey},
					//{"uid", SystemInfo.deviceUniqueIdentifier},
					{"windowIds", ids}//
				}, (result) => {

					if (result.IsSuccess() == true) {
						var jo = new JSONObject(result.response.Trim());

						foreach (var item in this.list) {
							item.points.Clear();
						}

						for (int i = 0, imax = jo.list.Count; i < imax; ++i) {
							
							var obj = jo.list[i];
							var x = obj.GetField("x").f;
							var y = obj.GetField("y").f;

							Vector2 point = new Vector2(x, y);
							LayoutTag t = (LayoutTag)(int)obj.GetField("tag").d;

							int windowId = (int)obj.GetField("windowId").d;

							foreach (var item in this.list) {
								if (item.id == windowId) {
									item.AddPoint(point, t, null);
									item.status = HeatmapSettings.WindowsData.Window.Status.Loaded;
									break;
								}
							}
						}

						UnityEditor.EditorUtility.SetDirty(settings);

						foreach (var item in this.list) {
							if (item.texture == null) item.texture = new Texture2D((int)item.size.x, (int)item.size.y, TextureFormat.ARGB32, false);									
							if (item.changed == false) return;					
							item.texture = HeatmapVisualizer.Create(item.texture, item, item.GetPoints(), item.size);
							item.changed = false;
						}

					}

				});
				#endif*/

			}
			
			public void SetChanged() {
				
				foreach (var item in this.list) item.SetChanged();
				
			}

		}

		public WindowsData data;
		
		public List<AnalyticsServiceItem> items = new List<AnalyticsServiceItem>();

		public override void AddService(ServiceItem item) {

			this.items.Add(item as AnalyticsServiceItem);

		}

		public override List<ServiceItem> GetItems() {

			return this.items.Cast<ServiceItem>().ToList();

		}

		public override void SetChanged() {

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