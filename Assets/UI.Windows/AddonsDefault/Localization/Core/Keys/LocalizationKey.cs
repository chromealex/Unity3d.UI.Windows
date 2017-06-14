using UnityEngine;

namespace UnityEngine.UI.Windows.Plugins.Localization {

	[System.Serializable]
	public struct LocalizationKey {

		public string key;
		public int parameters;
		public bool formatWithDeclension;
		public bool outputDeclensionWithoutNumber;
		
		public LocalizationKey(string key) {
			
			this.key = key;
			this.parameters = LocalizationSystem.GetParametersCount(key, LocalizationSystem.GetCurrentLanguage());
			this.formatWithDeclension = LocalizationSystem.IsNeedToFormatWithDeclension(key, LocalizationSystem.GetCurrentLanguage());
			this.outputDeclensionWithoutNumber = false;
			
		}

		public bool IsNone() {

			return this.key == null || string.IsNullOrEmpty(this.key.Trim());

		}

	}

}