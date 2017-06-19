using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Ads {
	
	[System.Serializable]
	public class AdsServiceItem : ServiceItem {

		public string iosKey;
		public string androidKey;

		public bool testMode;

	};

}