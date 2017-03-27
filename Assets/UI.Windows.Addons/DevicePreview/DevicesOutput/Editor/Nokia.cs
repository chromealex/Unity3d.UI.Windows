using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview.Output.Nokia {
	
	public class Lumia920 : DeviceOutputBase {
		
		public override string GetMainImage() { return "Nokia/lumia920"; }
		public override Vector2 GetOffset() { return new Vector2(0f, -30f); }
		
	}

}
