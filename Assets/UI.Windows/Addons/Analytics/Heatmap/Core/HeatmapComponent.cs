using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Heatmap.Events;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Components {

	public class HeatmapWindowComponent : WindowComponentBase, IHeatmapHandler {

		public bool heatmapActive = true;

		public void OnComponentClick() {

			if (this.heatmapActive == true) {

				Core.HeatmapSystem.Put(this);

			}

		}
		
		public void OnScreenClick() {
			
			if (this.heatmapActive == true) {

				Core.HeatmapSystem.Put();

			}

		}

	}

}