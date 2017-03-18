using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

	public static class TextComponentTMPAddon {

		public static bool IsValid(MaskableGraphic text) {

			#if TEXTMESHPRO_SUPPORTED
			return text is TMPro.TMP_Text;
			#else
			return false;
			#endif

		}

		public static void SetBestFitState(MaskableGraphic text, bool state) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.enableAutoSizing = state;
			#endif

		}

		public static void SetBestFitMinSize(MaskableGraphic text, int size) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.fontSizeMin = size;
			#endif

		}

		public static void SetBestFitMaxSize(MaskableGraphic text, int size) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.fontSizeMax = size;
			#endif

		}

		public static int GetFontSize(MaskableGraphic text) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) return (int)instance.fontSize;
			#endif

			return 0;

		}

		public static void SetFontSize(MaskableGraphic text, int fontSize) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.fontSize = fontSize;
			#endif

		}

		public static void SetLineSpacing(MaskableGraphic text, float value) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.lineSpacing = value;
			#endif

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

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.OverflowMode = (mode == VerticalWrapMode.Overflow ? TMPro.TextOverflowModes.Overflow : TMPro.TextOverflowModes.Truncate);
			#endif

		}

		public static void SetTextHorizontalOverflow(MaskableGraphic text, HorizontalWrapMode mode) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.OverflowMode = (mode == HorizontalWrapMode.Overflow ? TMPro.TextOverflowModes.Overflow : TMPro.TextOverflowModes.Truncate);
			#endif

		}

		public static void SetTextAlignment(MaskableGraphic text, TextAnchor anchor) {

			throw new System.Exception("SetTextAlignment not supported while using TextMeshPro");

		}

		public static bool IsRichtextSupported(MaskableGraphic text) {

			#if TEXTMESHPRO_SUPPORTED
			return true;
			#else
			return false;
			#endif

		}

		public static string GetText(MaskableGraphic text) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) return instance.text;
			#endif

			return string.Empty;

		}

		public static void SetText(MaskableGraphic text, string contentText) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.text = contentText;
			#endif

		}

	}

}

