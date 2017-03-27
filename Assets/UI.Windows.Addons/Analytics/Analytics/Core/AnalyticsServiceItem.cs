using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.UserInfo;

namespace UnityEngine.UI.Windows.Plugins.Analytics {
	
	[System.Serializable]
	public class AnalyticsServiceItem : ServiceItem {

		[System.NonSerialized]
		public bool isChanged = false;
		
		[System.NonSerialized]
		public bool show;

		public UserFilter userFilter = new UserFilter();

	};

}