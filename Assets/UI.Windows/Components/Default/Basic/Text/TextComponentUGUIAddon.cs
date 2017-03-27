using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

	public static class TextComponentUGUIAddon {

		public static bool IsValid(MaskableGraphic text) {

			return text is Text;

		}

		public static void SetBestFitState(MaskableGraphic text, bool state) {

			var instance = text as Text;
			if (instance != null) instance.resizeTextForBestFit = state;

		}

		public static void SetBestFitMinSize(MaskableGraphic text, int size) {

			var instance = text as Text;
			if (instance != null) instance.resizeTextMinSize = size;

		}

		public static void SetBestFitMaxSize(MaskableGraphic text, int size) {

			var instance = text as Text;
			if (instance != null) instance.resizeTextMaxSize = size;

		}

		public static int GetFontSize(MaskableGraphic text) {

			var instance = text as Text;
			if (instance != null) return instance.fontSize;

			return 0;

		}

		public static void SetFontSize(MaskableGraphic text, int fontSize) {

			var instance = text as Text;
			if (instance != null) instance.fontSize = fontSize;

		}

		public static void SetLineSpacing(MaskableGraphic text, float value) {

			var instance = text as Text;
			if (instance != null) instance.lineSpacing = value;

		}

		public static void SetRichText(MaskableGraphic text, bool state) {

			var instance = text as Text;
			if (instance != null) instance.supportRichText = state;

		}

		public static float GetContentHeight(MaskableGraphic text, string contentText, Vector2 containerSize) {

			var instance = text as Text;

			if (contentText.Length > TextComponent.MAX_CHARACTERS_COUNT) {

				contentText = contentText.Substring(0, TextComponent.MAX_CHARACTERS_COUNT);

			}

			var settings = instance.GetGenerationSettings(containerSize);
			return instance.cachedTextGenerator.GetPreferredHeight(contentText, settings);

		}

		public static void SetTextAlignByGeometry(MaskableGraphic text, bool state) {

			var instance = text as Text;
			if (instance != null) instance.alignByGeometry = state;

		}

		public static void SetTextVerticalOverflow(MaskableGraphic text, VerticalWrapMode mode) {

			var instance = text as Text;
			if (instance != null) instance.verticalOverflow = mode;

		}

		public static void SetTextHorizontalOverflow(MaskableGraphic text, HorizontalWrapMode mode) {

			var instance = text as Text;
			if (instance != null) instance.horizontalOverflow = mode;

		}

		public static void SetTextAlignment(MaskableGraphic text, TextAnchor anchor) {

			var instance = text as Text;
			if (instance != null) instance.alignment = anchor;

		}

		public static bool IsRichtextSupported(MaskableGraphic text) {

			var instance = text as Text;
			if (instance != null) return instance.supportRichText;

			return false;

		}

		public static string GetText(MaskableGraphic text) {

			var instance = text as Text;
			if (instance != null) return instance.text;

			return string.Empty;

		}

		public static void SetText(MaskableGraphic text, string contentText) {

			var instance = text as Text;
			if (instance != null) {

				if (contentText.Length > TextComponent.MAX_CHARACTERS_COUNT) {

					contentText = contentText.Substring(0, TextComponent.MAX_CHARACTERS_COUNT);

				}

				instance.text = contentText;

			}

		}

	}

}

