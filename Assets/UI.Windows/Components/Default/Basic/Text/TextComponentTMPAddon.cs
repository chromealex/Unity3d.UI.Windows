using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

	public static class TextComponentTMPAddon {

		public static bool IsValid(MaskableGraphic text) {

			return text is TMPro.TMP_Text;

		}

		public static void SetBestFitState(MaskableGraphic text, bool state) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.enableAutoSizing = state;

		}

		public static void SetBestFitMinSize(MaskableGraphic text, int size) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.fontSizeMin = size;

		}

		public static void SetBestFitMaxSize(MaskableGraphic text, int size) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.fontSizeMax = size;

		}

		public static int GetFontSize(MaskableGraphic text) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) return (int)instance.fontSize;

			return 0;

		}

		public static void SetFontSize(MaskableGraphic text, int fontSize) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.fontSize = fontSize;

		}

		public static void SetLineSpacing(MaskableGraphic text, float value) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.lineSpacing = value;

		}

		public static void SetRichText(MaskableGraphic text, bool state) {

			throw new System.Exception("SetRichText not supported while using TextMeshPro");

		}

		public static float GetContentHeight(MaskableGraphic text, string contentText, Vector2 containerSize) {

			throw new System.Exception("GetContentHeight not supported while using TextMeshPro");

		}

		public static void SetTextAlignByGeometry(MaskableGraphic text, bool state) {

			throw new System.Exception("SetTextAlignByGeometry not supported while using TextMeshPro");

		}

		public static void SetTextVerticalOverflow(MaskableGraphic text, VerticalWrapMode mode) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.OverflowMode = (mode == VerticalWrapMode.Overflow ? TMPro.TextOverflowModes.Overflow : TMPro.TextOverflowModes.Truncate);

		}

		public static void SetTextHorizontalOverflow(MaskableGraphic text, HorizontalWrapMode mode) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.OverflowMode = (mode == HorizontalWrapMode.Overflow ? TMPro.TextOverflowModes.Overflow : TMPro.TextOverflowModes.Truncate);

		}

		public static void SetTextAlignment(MaskableGraphic text, TextAnchor anchor) {

			throw new System.Exception("SetTextAlignment not supported while using TextMeshPro");

		}

		public static bool IsRichtextSupported(MaskableGraphic text) {

			return true;

		}

		public static string GetText(MaskableGraphic text) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) return instance.text;

			return string.Empty;

		}

		public static void SetText(MaskableGraphic text, string contentText) {

			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.text = contentText;

		}

	}

}

