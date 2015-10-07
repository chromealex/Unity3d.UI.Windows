using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview.Output.Samsung {
	
	public class GalaxyS4 : DeviceOutputBase {
		
		public override string GetMainImage() { return "Samsung/galaxys4"; }
		//public override Vector2 GetOffset() { return new Vector2(9f, 1f); }
		
	}
	
	public class GalaxyS4Mini : DeviceOutputBase {
		
		public override string GetMainImage() { return "Samsung/galaxys4mini"; }
		//public override Vector2 GetOffset() { return new Vector2(0f, 0f); }
		
	}

}
