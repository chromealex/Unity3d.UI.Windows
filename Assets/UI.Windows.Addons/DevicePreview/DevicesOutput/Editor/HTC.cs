using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview.Output.HTC {
	
	public class OneM9 : DeviceOutputBase {
		
		public override string GetMainImage() { return "HTC/onem9"; }
		public override Vector2 GetOffset() { return new Vector2(0f, 0f); }
		
	}

}
