using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Plugins.Analytics {
	
	[System.Serializable]
	public class AnalyticsItem {

		[System.NonSerialized]
		public bool isChanged = false;

		public string platformName;
		public bool enabled;
		public bool show;
		public string authKey;
		
		public UserFilter userFilter = new UserFilter();

	};

}