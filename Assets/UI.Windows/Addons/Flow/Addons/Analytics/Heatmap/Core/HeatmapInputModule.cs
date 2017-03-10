using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows.Plugins.Heatmap.Components;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Events {
	
	public interface IHeatmapHandler : IEventSystemHandler, IWindowObject {
		
		void OnComponentClick();
		void OnScreenClick();

	}

	public class HeatmapEvents {

		#region Execution Handlers
		
		private static readonly UnityEngine.EventSystems.ExecuteEvents.EventFunction<IHeatmapHandler> _heatmapHandler = Execute;
		
		private static void Execute(IHeatmapHandler handler, BaseEventData eventData) {
			
			handler.OnComponentClick();

		}

		#endregion

		public static ExecuteEvents.EventFunction<IHeatmapHandler> heatmapHandler {

			get {

				return HeatmapEvents._heatmapHandler;

			}

		}
		
	}

	public class HeatmapInputModule : MonoBehaviour {

		private PointerEventData currentData;

		public void Start() {

			WindowSystemInput.onPointerUp.AddListener(this.OnPointerUp);

		}

		public void LateUpdate() {

			if (this.currentData == null) this.currentData = new PointerEventData(EventSystem.current);
			if (this.currentData.used == true) this.currentData.Reset();
			/*
			#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
			if (Input.GetMouseButtonUp(0) == true ||
			    Input.GetMouseButtonUp(1) == true ||
			    Input.GetMouseButtonUp(2) == true) {

				this.currentData.pointerPress = EventSystem.current.currentSelectedGameObject;
				this.currentData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				this.OnPointerClick(this.currentData);

			}
			#endif*/

		}

		private void OnPointerUp(int id) {

			var position = WindowSystemInput.GetPointerPosition();
			this.currentData.pointerPress = EventSystem.current.currentSelectedGameObject;
			this.currentData.position = new Vector2(position.x, position.y);
			this.OnPointerClick(this.currentData);

		}

		public void OnPointerClick(PointerEventData eventData) {

			ExecuteEvents.Execute<IHeatmapHandler>(eventData.pointerPress, eventData, HeatmapEvents.heatmapHandler);
			eventData.Use();

		}

	}

}