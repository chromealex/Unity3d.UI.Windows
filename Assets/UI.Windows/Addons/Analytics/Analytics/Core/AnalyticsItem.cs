using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Plugins.Analytics {
	
	[System.Serializable]
	public class AnalyticsItem {

		[System.NonSerialized]
		public bool isChanged = false;
		
		[System.NonSerialized]
		public bool show;
		
		[System.NonSerialized]
		public bool processing;

		public string platformName;
		public bool enabled;
		public string authKey;
		
		public UserFilter userFilter = new UserFilter();

	};

}