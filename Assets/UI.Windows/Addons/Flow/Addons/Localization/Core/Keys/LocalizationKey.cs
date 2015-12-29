using UnityEngine;

namespace UnityEngine.UI.Windows.Plugins.Localization {

	[System.Serializable]
	public struct LocalizationKey {

		public string key;
		public int parameters;
		public bool formatWithDeclension;

		public LocalizationKey(string key) {

			this.key = key;
			this.parameters = LocalizationSystem.GetParametersCount(key, LocalizationSystem.GetCurrentLanguage());
			this.formatWithDeclension = LocalizationSystem.IsNeedToFormatWithDeclension(key, LocalizationSystem.GetCurrentLanguage());

		}

		public bool IsNone() {

			return this.key == null || string.IsNullOrEmpty(this.key.Trim());

		}

	}

}