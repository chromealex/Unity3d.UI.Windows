using UnityEngine;
using System.Collections;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview.Output.Apple {
	
	public class Mac : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/imac"; }
		public override Vector2 GetOffset() { return new Vector2(-30f, 0f); }

	}

	public class IpadMini : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/ipadmini"; }
		//public override Vector2 GetOffset() { return new Vector2(0f, -20f); }
		
	}

	public class IpadAir : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/ipadair"; }
		//public override Vector2 GetOffset() { return new Vector2(0f, 0f); }
		
	}
	
	public class Ipad1 : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/ipad1"; }
		//public override Vector2 GetOffset() { return new Vector2(-3f, -15f); }
		
	}

	public class Ipad : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/ipad"; }
		//public override Vector2 GetOffset() { return new Vector2(-3f, -15f); }
		
	}

	public class Iphone6plus : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/iphone6plus"; }
		//public override Vector2 GetOffset() { return new Vector2(0f, 0f); }
		
	}

	public class Iphone6 : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/iphone6"; }
		//public override Vector2 GetOffset() { return new Vector2(-8f, -8f); }

	}
	
	public class Iphone5C : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/iphone5c"; }
		//public override Vector2 GetOffset() { return new Vector2(0f, 2f); }
		
	}

	public class Iphone5 : DeviceOutputBase {

		public override string GetMainImage() { return "Apple/iphone5"; }
		//public override Vector2 GetOffset() { return new Vector2(0f, 2f); }

	}
	
	public class Iphone4 : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/iphone4"; }
		//public override Vector2 GetOffset() { return new Vector2(0f, -1f); }

	}
	
	public class Iphone3 : DeviceOutputBase {
		
		public override string GetMainImage() { return "Apple/iphone3"; }

	}

}
