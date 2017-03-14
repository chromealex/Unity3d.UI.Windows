using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Plugins.Heatmap.Components;
using UnityEngine.UI.Windows.Plugins.Heatmap.Events;
using System.Linq;
using UnityEngine.UI.Windows.Types;

using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using System;
using UnityEngine.UI.Windows.Plugins.Analytics;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Core {
	
	public enum ClickType : byte {

		Component,
		Screen,

	}

	public static class HeatmapSystem {

		public static void Put() {
			
			HeatmapSystem.Put(null, WindowSystemInput.GetPointerPosition(), ClickType.Screen);
			
		}

		public static void Put(IHeatmapHandler component) {

			var wComp = component as WindowComponent;

			var corners = new Vector3[4];
			(wComp.transform as RectTransform).GetWorldCorners(corners);
			
			var leftBottom = HeatmapSystem.GetScreenPoint(wComp, corners[0]);
			//var topRight = HeatmapSystem.GetScreenPoint(wComp, corners[2]);

			var inputPosition = WindowSystemInput.GetPointerPosition();
			//var w = topRight.x - leftBottom.x;
			//var h = topRight.y - leftBottom.y;

			var pos = new Vector2(inputPosition.x - leftBottom.x, inputPosition.y - leftBottom.y);

			HeatmapSystem.Put(component, pos, ClickType.Component);

		}

		public static void Put(IHeatmapHandler component, Vector2 localPoint, ClickType clickType) {

			// Normalize coords - make it ready to save
			var fullScreen = new Vector2(Screen.width, Screen.height);
			var current = localPoint;
			var localNormalizedPoint = Vector2.zero;

			var tag = LayoutTag.None;
			WindowBase screen = null;
			if (component != null) {

				// Find component position
				var rectTransform = (component as WindowComponent).transform as RectTransform;
				if (rectTransform != null) {

					var offset = Vector2.zero;
					/*var scrolls = (component as WindowComponent).GetComponentsInParent<ScrollRect>();
					if (scrolls != null && scrolls.Length > 0) {

						var scroll = scrolls[0];
						var scrollRect = (scroll.transform as RectTransform).rect;

						offset = new Vector2(scrollRect.width * scroll.normalizedPosition.x, scrollRect.height * (1f - scroll.normalizedPosition.y));

					}*/

					var elementRect = rectTransform.rect;
					elementRect.x += offset.x;
					elementRect.y += offset.y;

					// Clamp localPoint to element rect

					localNormalizedPoint = new Vector2(localPoint.x / elementRect.width, localPoint.y / elementRect.height);

					//Debug.Log(elementRect, component as ButtonComponent);

				}

				screen = component.GetWindow();

				var comp = component as WindowComponent;
				if (comp != null) {

					var layout = comp.GetLayoutRoot() as WindowLayoutElement;
					if (layout != null) {

						tag = layout.tag;

					}

				}

			} else {

				screen = WindowSystem.GetCurrentWindow();
				localNormalizedPoint = new Vector2(current.x / fullScreen.x, current.y / fullScreen.y);

			}
			
			//Debug.Log(fullScreen + " :: " + localNormalizedPoint + " :: " + localPoint);

			// Send point to server
			HeatmapSystem.Send(tag, screen, component as WindowComponent, localNormalizedPoint);

		}
		
		private static Vector3 GetScreenPoint(WindowComponent component, Vector3 worldPoint) {

		    var window = component.GetWindow();
			if (window == null) return Vector3.zero;

		    var cam = window.workCamera;
            return cam.WorldToScreenPoint(worldPoint);
			
		}

		public static void Send(LayoutTag tag, WindowBase window, WindowComponent component, Vector2 localNormalizedPoint) {

			var flowWindow = Flow.FlowSystem.GetWindow(window, runtime: true);
			if (flowWindow == null) {
				
				Debug.LogWarningFormat("[ Heatmap ] FlowWindow not found. Source {0} used ({1}).", window, tag);
				return;

			}

			// Offline
			#if UNITY_EDITOR
			var modulesPath = FlowSystem.GetData().GetModulesPath();
			var settings = ME.EditorUtilities.GetAssetsOfType<HeatmapSettings>(modulesPath, useCache: false).FirstOrDefault();
			#else
			HeatmapSettings settings = null;
			#endif
			if (settings == null) return;

			var data = settings.data.Get(flowWindow);
			data.AddPoint(localNormalizedPoint, new Vector2Int(Screen.width, Screen.height), flowWindow.id, tag, component);

		}

		public static void GenerateTextureFromData(int screenWidth, int screenHeight, HeatmapResult data, System.Action<Texture2D> onResult) {

			var texture = new Texture2D(screenWidth, screenHeight, TextureFormat.ARGB32, false);
			texture = HeatmapVisualizer.Create(texture, data.points, new Vector2(screenWidth, screenHeight));

			onResult(texture);

		}

		public static float GetFactor(Vector2 inner, Vector2 boundingBox) {     
			
			var widthScale = 0f;
			var heightScale = 0f;
			if (inner.x != 0f) {
				
				widthScale = boundingBox.x / inner.x;
				
			}
			
			if (inner.y != 0f) {
				
				heightScale = boundingBox.y / inner.y;                
				
			}
			
			return Mathf.Min(widthScale, heightScale);
			
		}

		#if UNITY_EDITOR
		public static WindowLayout GetLayout(int windowId, out LayoutWindowType screen) {

			screen = null;

			var window = FlowSystem.GetWindow(windowId);
			if (window == null) return null;

			var screenInfo = window.GetScreen();
			if (screenInfo == null) return null;

			screen = screenInfo.Load<LayoutWindowType>();
			if (screen == null || screen.GetCurrentLayout().layout == null) return null;
			
			return screen.GetCurrentLayout().layout;
			
		}
		#endif

	}

}
