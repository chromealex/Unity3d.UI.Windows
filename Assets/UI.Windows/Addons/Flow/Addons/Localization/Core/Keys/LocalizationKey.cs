using UnityEngine;

namespace UnityEngine.UI.Windows.Plugins.Localization {

	[System.Serializable]
	public struct LocalizationKey {

		public string key;
		public int parameters;

		public LocalizationKey(string key) {

			this.key = key;
			this.parameters = LocalizationSystem.GetParametersCount(key);

		}

	}

}