using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Analytics;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Core {

	public class HeatmapVisualizer {
		
		public static int RED_THRESHOLD = 235; 		//!< Red threshold.
		///	Minimum alpha a point must have to be red.
		public static int GREEN_THRESHOLD = 200;	//!< Green threshold.
		///	Minimum alpha a point must have to be green.
		public static int BLUE_THRESHOLD = 150;		//!< Blue threshold.	
		///	Minimum alpha a point must have to be Blue.
		public static int MINIMUM_THRESHOLD = 100;	//!< Minimum threshold.	
		///	Minimum alpha a point must have to be rendered at all.

		public static int GetRadius() {
			
			int radius = 10;	// Standalone
			
			#if UNITY_MOBILE
			radius = 20;
			#endif

			return radius;

		}

		public static Texture2D Create(Texture2D map, HeatmapResult.Point[] points, Vector2 size) {

			float maxWeight = 0f;
			for (int i = 0; i < points.Length; ++i) {

				if (points[i].weight > maxWeight) {

					maxWeight = (float)points[i].weight;

				}

			}

			var radius = HeatmapVisualizer.GetRadius();

			if (size == Vector2.zero) {

				return null;

			}

			// Create new texture
			// Texture2D map = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);	
			if (map == null) {

				map = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, false);

			}

			// Set texture to alpha-fied state
			map.SetPixels(HeatmapVisualizer.ColorArray(new Color(1f, 1f, 1f, 0f), map.width * map.height), 0);

			/*** Generate Grayscale Values ***/
			{
				int x2;							// the offset x val in img coordinates
				int y2;							// the offset y val in img coordinates (0,0) - (maxX, maxY)
				float pointAlpha = .9f;			// The alpha that the darkest pixel will be in a point.
				Color color = new Color(1f, 1f, 1f, pointAlpha);
				int lineWidth = 1;//(int)(radius * .05f);
				Dictionary<Vector2Int, Color> pixelAlpha = new Dictionary<Vector2Int, Color>();
				
				for (int i = 0; i < points.Length; ++i) {			// generate alpha add for each point and a specified circumference

					pixelAlpha.Clear();
					var weight = points[i].weight / maxWeight;
					pointAlpha = weight;

					for (int r = 0; r < radius; r += lineWidth) {	// draw and fill them circles

						for (int angle = 0; angle < 360; ++angle) {

							x2 = (int)(r * Mathf.Cos(angle)) + (int)(points[i].realPoint.x);
							y2 = (int)(r * Mathf.Sin(angle)) + (int)(points[i].realPoint.y);
							
							// This could be sped up
							for (int y = y2; y > y2 - lineWidth; --y) {

								for (int x = x2; x < x2 + lineWidth; ++x) {

									var coord = new Vector2Int(x, y);
									if (pixelAlpha.ContainsKey(coord) == true) {

										pixelAlpha[coord] = color;

									} else {

										pixelAlpha.Add(new Vector2Int(x, y), color);

									}

								}	

							}

						}

						color = new Color(color.r, color.g, color.b, color.a - (pointAlpha / ((float)radius / lineWidth)));

					}
					
					// Since the radial fill code overwrites it's own pixels, make sure to only add finalized alpha to
					// old values.
					foreach (KeyValuePair<Vector2Int, Color> keyval in pixelAlpha) {

						var coord = keyval.Key;
						Color previousColor = map.GetPixel((int)coord.x, (int)coord.y);
						Color newColor = keyval.Value;
						map.SetPixel(coord.x, coord.y, new Color(newColor.r, newColor.b, newColor.g, newColor.a + previousColor.a));

					}
					
					// Reset color for next point
					color = new Color(color.r, color.g, color.b, pointAlpha);

				}

			}
			
			map.Apply();
			
			map.SetPixels32(Colorize(map.GetPixels32(0)), 0);
			
			map.Apply();
			
			return map;

		}
		
		public static Color[] ColorArray(Color col, int arraySize) {

			var colArr = new Color[arraySize];
			for (int i  = 0; i < colArr.Length; i++) {

				colArr[i] = col;

			}
			return colArr;

		}

		public static Color32[] Colorize(Color32[] pixels) {

			const int max = 255;

			for (int i = 0; i < pixels.Length; i++) {
				
				var r = 0;
				var g = 0;
				var b = 0;
				var tmp = 0;

				var alpha = pixels[i].a;
				
				if (alpha == 0) {

					continue;

				}
				
				if (alpha <= max && alpha >= HeatmapVisualizer.RED_THRESHOLD) {

					tmp = max - alpha;
					r = max - tmp;
					g = tmp * 12;

				} else if (alpha <= (HeatmapVisualizer.RED_THRESHOLD - 1) && alpha >= HeatmapVisualizer.GREEN_THRESHOLD) {

					tmp = (HeatmapVisualizer.RED_THRESHOLD - 1) - alpha;
					r = max - (tmp * 8);
					g = max;

				} else if (alpha <= (HeatmapVisualizer.GREEN_THRESHOLD - 1) && alpha >= HeatmapVisualizer.BLUE_THRESHOLD) {

					tmp = (HeatmapVisualizer.GREEN_THRESHOLD - 1) - alpha;
					g = max;
					b = tmp * 5;

				} else if (alpha <= (HeatmapVisualizer.BLUE_THRESHOLD - 1) && alpha >= HeatmapVisualizer.MINIMUM_THRESHOLD) {

					tmp = (HeatmapVisualizer.BLUE_THRESHOLD - 1) - alpha;
					g = max - (tmp * 5);
					b = max;

				} else {

					b = max;

				}

				pixels[i] = new Color32((byte)r, (byte)g, (byte)b, (byte)(alpha * 0.5f));
				//pixels[i] = HeatmapVisualizer.NormalizeColor(pixels[i]);

			}
			
			return pixels;

		}
		
		public static Color NormalizeColor(Color col) {

			return new Color(col.r / 255f, col.g / 255f, col.b / 255f, col.a / 255f);

		}

	}

}