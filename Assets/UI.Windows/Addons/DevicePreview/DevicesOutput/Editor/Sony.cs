using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview.Output.Sony {
	
	public class Z3Compact : DeviceOutputBase {
		
		public override string GetMainImage() { return "Sony/z3compact"; }
		
	}

	public class Z3 : DeviceOutputBase {
		
		public override string GetMainImage() { return "Sony/z3"; }
		
	}

	public class Z5 : DeviceOutputBase {
		
		public override string GetMainImage() { return "Sony/z5"; }
		
	}

}
