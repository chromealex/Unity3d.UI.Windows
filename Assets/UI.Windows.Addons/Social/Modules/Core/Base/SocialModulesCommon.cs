using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Plugins.Social.Modules {

	public enum Sex : byte {
		NotSpecified = 0,
		Female = 1,
		Male = 2,
	}

	[System.Serializable]
	public class UserDevice {

		public string hardware;
		public string os;

		public UserDevice(JSONObject json) {

			this.hardware = json.GetField("hardware").str;
			this.os = json.GetField("os").str;

		}

	}

}