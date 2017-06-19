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

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.richText = state;
			#endif

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
			if (instance != null) instance.overflowMode = (mode == VerticalWrapMode.Overflow ? TMPro.TextOverflowModes.Overflow : TMPro.TextOverflowModes.Truncate);
			#endif

		}

		public static void SetTextHorizontalOverflow(MaskableGraphic text, HorizontalWrapMode mode) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance != null) instance.overflowMode = (mode == HorizontalWrapMode.Overflow ? TMPro.TextOverflowModes.Overflow : TMPro.TextOverflowModes.Truncate);
			#endif

		}

		public static void SetTextAlignment(MaskableGraphic text, TextAnchor anchor) {

			#if TEXTMESHPRO_SUPPORTED
			var instance = text as TMPro.TMP_Text;
			if (instance == null) return;

			switch (anchor) {
				
				case TextAnchor.UpperLeft:
					instance.alignment = TMPro.TextAlignmentOptions.TopLeft;
					break;
				case TextAnchor.UpperCenter:
					instance.alignment = TMPro.TextAlignmentOptions.Top;
					break;
				case TextAnchor.UpperRight:
					instance.alignment = TMPro.TextAlignmentOptions.TopRight;
					break;
				case TextAnchor.MiddleLeft:
					instance.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
					break;
				case TextAnchor.MiddleCenter:
					instance.alignment = TMPro.TextAlignmentOptions.Midline;
					break;
				case TextAnchor.MiddleRight:
					instance.alignment = TMPro.TextAlignmentOptions.MidlineRight;
					break;
				case TextAnchor.LowerLeft:
					instance.alignment = TMPro.TextAlignmentOptions.BottomLeft;
					break;
				case TextAnchor.LowerCenter:
					instance.alignment = TMPro.TextAlignmentOptions.Bottom;
					break;
				case TextAnchor.LowerRight:
					instance.alignment = TMPro.TextAlignmentOptions.BottomRight;
					break;
				default:
					throw new System.ArgumentOutOfRangeException("anchor", anchor, null);

			}
			#endif

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

