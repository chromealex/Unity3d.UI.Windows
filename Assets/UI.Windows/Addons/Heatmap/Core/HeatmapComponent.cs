using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Heatmap.Events;

namespace UnityEngine.UI.Windows.Plugins.Heatmap.Components {

	public class HeatmapWindowComponent : WindowComponentBase, IHeatmapHandler {
		
		public void OnComponentClick() {

			Core.HeatmapSystem.Put(this);

		}
		
		public void OnScreenClick() {

			Core.HeatmapSystem.Put();

		}

	}

}