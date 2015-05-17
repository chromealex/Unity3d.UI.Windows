using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Core {

	public class HeatmapVisualizer : MonoBehaviour {

		public static Texture2D Create(Texture2D map, List<UnityEngine.UI.Windows.Plugins.Heatmap.Core.HeatmapSettings.Point> normalizedPoints, Vector2 size, int radius = 10) {

			if (size == Vector2.zero) return null;

			// Create new texture
			// Texture2D map = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);	
			if (map == null) map = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, false);

			// Set texture to alpha-fied state
			map.SetPixels(HeatmapVisualizer.ColorArray(new Color(1f, 1f, 1f, 0f), map.width * map.height), 0);

			/*** Generate Grayscale Values ***/
			{
				int x2;							// the offset x val in img coordinates
				int y2;							// the offset y val in img coordinates (0,0) - (maxX, maxY)
				float pointAlpha = .9f;			// The alpha that the darkest pixel will be in a poitn.
				Color color = new Color(1f, 1f, 1f, pointAlpha);
				int lineWidth = 1;//(int)(radius * .05f);
				Dictionary<Vector2, Color> pixelAlpha = new Dictionary<Vector2, Color>();
				
				for(int i = 0; i < normalizedPoints.Count; i++)			// generate alpha add for each point and a specified circumference
				{
					pixelAlpha.Clear();
					for(int r = 0; r < radius; r+=lineWidth)	// draw and fill them circles
					{
						for(int angle=0; angle<360; angle++)
						{
							x2 = (int)(r * Mathf.Cos(angle))+(int)(normalizedPoints[i].GetAbsoluteX(size.x));
							y2 = (int)(r * Mathf.Sin(angle))+(int)(normalizedPoints[i].GetAbsoluteY(size.y));
							
							// This could be sped up
							for(int y = y2; y > y2-lineWidth; y--) {
								for(int x = x2; x < x2+lineWidth; x++) {
									Vector2 coord = new Vector2(x, y);
									
									if(pixelAlpha.ContainsKey(coord))
										pixelAlpha[coord] = color;
									else
										pixelAlpha.Add(new Vector2(x, y), color);
								}		
							}
						}
						color = new Color(color.r, color.g, color.b, color.a - (pointAlpha/( (float)radius/lineWidth)) );
					}
					
					// Since the radial fill code overwrites it's own pixels, make sure to only add finalized alpha to
					// old values.
					foreach (KeyValuePair<Vector2, Color> keyval in pixelAlpha)
					{
						Vector2 coord = keyval.Key;
						Color previousColor = map.GetPixel((int)coord.x, (int)coord.y);
						Color newColor = keyval.Value;
						map.SetPixel((int)coord.x, (int)coord.y, new Color(newColor.r, newColor.b, newColor.g, newColor.a + previousColor.a));
					}
					
					// Reset color for next point
					color = new Color(color.r, color.g, color.b, pointAlpha);
				}
			}
			
			map.Apply();
			
			map.SetPixels( Colorize(map.GetPixels(0)), 0);
			
			map.Apply();
			
			return map;

		}
		
		public static Color[] ColorArray(Color col, int arraySize)
		{
			Color[] colArr = new Color[arraySize];
			for(int i  = 0; i < colArr.Length; i++)
			{
				colArr[i] = col;
			}
			return colArr;
		}

		public static int RED_THRESHOLD = 235; 		//!< Red threshold.
		///	Minimum alpha a point must have to be red.
		public static int GREEN_THRESHOLD = 200;	//!< Green threshold.
		///	Minimum alpha a point must have to be green.
		public static int BLUE_THRESHOLD = 150;		//!< Blue threshold.	
		///	Minimum alpha a point must have to be Blue.
		public static int MINIMUM_THRESHOLD = 100;	//!< Minimum threshold.	
		///	Minimum alpha a point must have to be rendered at all.

		public static Color[] Colorize(Color[] pixels) {
			for (int i = 0; i < pixels.Length; i++) {
				
				float r = 0, g = 0, b = 0, tmp = 0;
				pixels[i] *= 255f;
				
				float alpha = pixels[i].a;
				
				if (alpha == 0) {
					continue;
				}
				
				if (alpha <= 255 && alpha >= RED_THRESHOLD) {
					tmp = 255 - alpha;
					r = 255 - tmp;
					g = tmp * 12f;
				} else if (alpha <= (RED_THRESHOLD - 1) && alpha >= GREEN_THRESHOLD) {
					tmp = (RED_THRESHOLD - 1) - alpha;
					r = 255 - (tmp * 8f);
					g = 255;
				} else if (alpha <= (GREEN_THRESHOLD - 1) && alpha >= BLUE_THRESHOLD) {
					tmp = (GREEN_THRESHOLD - 1) - alpha;
					g = 255;
					b = tmp * 5;
				} else if (alpha <= (BLUE_THRESHOLD - 1) && alpha >= MINIMUM_THRESHOLD) {
					tmp = (BLUE_THRESHOLD - 1) - alpha;
					g = 255 - (tmp * 5f);
					b = 255;
				} else
					b = 255;
				pixels[i] = new Color(r, g, b, alpha / 2f);
				pixels[i] = NormalizeColor(pixels[i]);
			}
			
			return pixels;
		}
		
		public static Color NormalizeColor(Color col)
		{
			return new Color( col.r / 255f, col.g / 255f, col.b / 255f, col.a / 255f);
		}

	}

}